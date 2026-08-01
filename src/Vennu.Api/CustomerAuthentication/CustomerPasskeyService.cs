using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.DataProtection;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed record PasskeyChallenge<TOptions>(Guid ChallengeId, TOptions Options);

public interface ICustomerPasskeyService
{
    Task<PasskeyChallenge<CredentialCreateOptions>> BeginRegistrationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CompleteRegistrationAsync(Guid userId, Guid challengeId, string displayName, AuthenticatorAttestationRawResponse response, CancellationToken cancellationToken = default);
    Task<PasskeyChallenge<AssertionOptions>?> BeginAssertionAsync(string email, CancellationToken cancellationToken = default);
    Task<CustomerSessionIssue?> CompleteAssertionAsync(Guid challengeId, AuthenticatorAssertionRawResponse response, CancellationToken cancellationToken = default);
}

public sealed class CustomerPasskeyService(
    IFido2 fido2,
    ICustomerAuthenticationRepository authenticationRepository,
    ICustomerIdentityRepository identityRepository,
    ICustomerSessionService sessionService,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider) : ICustomerPasskeyService
{
    private readonly IDataProtector challengeProtector = dataProtectionProvider.CreateProtector("Vennu.CustomerAuthentication.PasskeyChallenges.v1");

    public async Task<PasskeyChallenge<CredentialCreateOptions>> BeginRegistrationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await RequireUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var existing = await authenticationRepository.GetPasskeysAsync(userId, cancellationToken).ConfigureAwait(false);
        var options = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = new Fido2User { Id = userId.ToByteArray(), Name = user.Email, DisplayName = user.DisplayName },
            ExcludeCredentials = existing.Select(item => new PublicKeyCredentialDescriptor(item.CredentialId)).ToList(),
            AuthenticatorSelection = AuthenticatorSelection.Default,
            AttestationPreference = AttestationConveyancePreference.None
        });
        var challenge = await StoreChallengeAsync(userId, CustomerAuthenticationChallengeType.PasskeyRegistration, options.ToJson(), cancellationToken).ConfigureAwait(false);
        return new PasskeyChallenge<CredentialCreateOptions>(challenge.Id, options);
    }

    public async Task CompleteRegistrationAsync(Guid userId, Guid challengeId, string displayName, AuthenticatorAttestationRawResponse response, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 100) throw new ArgumentException("A bounded passkey name is required.", nameof(displayName));
        var challenge = await ConsumeChallengeAsync(challengeId, CustomerAuthenticationChallengeType.PasskeyRegistration, cancellationToken).ConfigureAwait(false);
        if (challenge.UserId != userId) throw new UnauthorizedAccessException("The challenge does not belong to this session.");
        var options = CredentialCreateOptions.FromJson(challengeProtector.Unprotect(challenge.ProtectedOptions));
        var result = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = response,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (args, callbackCancellationToken) =>
                await authenticationRepository.GetPasskeyByCredentialIdAsync(args.CredentialId, callbackCancellationToken).ConfigureAwait(false) is null
        }, cancellationToken).ConfigureAwait(false);
        await authenticationRepository.CreatePasskeyAsync(new CustomerPasskeyCredential
        {
            Id = Guid.NewGuid(), UserId = userId, CredentialId = result.Id, PublicKey = result.PublicKey,
            UserHandle = result.User.Id, DisplayName = displayName.Trim(), CreatedUtc = timeProvider.GetUtcNow().UtcDateTime
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PasskeyChallenge<AssertionOptions>?> BeginAssertionAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var user = await identityRepository.GetUserByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken).ConfigureAwait(false);
        if (user is null || user.Status != CustomerUserStatus.Active) return null;
        var credentials = await authenticationRepository.GetPasskeysAsync(user.Id, cancellationToken).ConfigureAwait(false);
        if (credentials.Count == 0) return null;
        var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = credentials.Select(item => new PublicKeyCredentialDescriptor(item.CredentialId)).ToList(),
            UserVerification = UserVerificationRequirement.Required
        });
        var challenge = await StoreChallengeAsync(user.Id, CustomerAuthenticationChallengeType.PasskeyAssertion, options.ToJson(), cancellationToken).ConfigureAwait(false);
        return new PasskeyChallenge<AssertionOptions>(challenge.Id, options);
    }

    public async Task<CustomerSessionIssue?> CompleteAssertionAsync(Guid challengeId, AuthenticatorAssertionRawResponse response, CancellationToken cancellationToken = default)
    {
        var challenge = await ConsumeChallengeAsync(challengeId, CustomerAuthenticationChallengeType.PasskeyAssertion, cancellationToken).ConfigureAwait(false);
        var credential = await authenticationRepository.GetPasskeyByCredentialIdAsync(response.RawId, cancellationToken).ConfigureAwait(false);
        if (credential is null || credential.UserId != challenge.UserId) return null;
        var options = AssertionOptions.FromJson(challengeProtector.Unprotect(challenge.ProtectedOptions));
        var result = await fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = response,
            OriginalOptions = options,
            StoredPublicKey = credential.PublicKey,
            StoredSignatureCounter = credential.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = (args, _) => Task.FromResult(
                args.UserHandle.SequenceEqual(credential.UserHandle) && args.CredentialId.SequenceEqual(credential.CredentialId))
        }, cancellationToken).ConfigureAwait(false);
        if (!await authenticationRepository.UpdatePasskeyCounterAsync(credential.Id, result.Counter, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false)) return null;
        return await sessionService.IssueStrongAsync(credential.UserId, CustomerAuthenticationMethod.Passkey, cancellationToken).ConfigureAwait(false);
    }

    private async Task<CustomerUser> RequireUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await identityRepository.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return user is { Status: CustomerUserStatus.Active } ? user : throw new UnauthorizedAccessException("An active customer account is required.");
    }

    private Task<CustomerAuthenticationChallenge> StoreChallengeAsync(Guid userId, CustomerAuthenticationChallengeType type, string json, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        return authenticationRepository.CreateChallengeAsync(new CustomerAuthenticationChallenge
        {
            Id = Guid.NewGuid(), UserId = userId, Type = type, ProtectedOptions = challengeProtector.Protect(json),
            CreatedUtc = utcNow, ExpiresUtc = utcNow.AddMinutes(5)
        }, cancellationToken);
    }

    private async Task<CustomerAuthenticationChallenge> ConsumeChallengeAsync(Guid id, CustomerAuthenticationChallengeType type, CancellationToken cancellationToken) =>
        await authenticationRepository.ConsumeChallengeAsync(id, type, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false)
        ?? throw new UnauthorizedAccessException("The challenge is invalid, expired, or already used.");
}

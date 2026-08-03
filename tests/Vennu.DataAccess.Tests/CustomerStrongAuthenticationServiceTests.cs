using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerStrongAuthenticationServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 23, 15, 0, TimeSpan.Zero);

    [Fact]
    public async Task Enrollment_ProtectsSecretAndReturnsRecoveryCodesOnlyAfterVerification()
    {
        var repository = new RepositoryFake();
        var protector = new ProtectorFake();
        var service = new CustomerStrongAuthenticationService(repository, protector, new FixedTimeProvider(UtcNow));

        var enrollment = await service.BeginTotpEnrollmentAsync(repository.UserId, "USER@example.com");
        var code = Totp(protector.LastSecret!, UtcNow.ToUnixTimeSeconds() / 30);
        var recoveryCodes = await service.CompleteTotpEnrollmentAsync(repository.UserId, code);

        Assert.StartsWith("otpauth://totp/Vennusign%3Auser%40example.com", enrollment.OtpAuthUri, StringComparison.Ordinal);
        Assert.NotEqual(enrollment.Secret, repository.Authenticator!.ProtectedSecret);
        Assert.Equal(10, recoveryCodes!.Count);
        Assert.Equal(10, repository.Codes.Count);
        Assert.All(repository.Codes, item => Assert.Equal(64, item.CodeHash.Length));
    }

    [Fact]
    public async Task TotpAndRecoveryCodes_AreSingleUse()
    {
        var repository = new RepositoryFake();
        var protector = new ProtectorFake();
        var service = new CustomerStrongAuthenticationService(repository, protector, new FixedTimeProvider(UtcNow));
        await service.BeginTotpEnrollmentAsync(repository.UserId, "user@example.com");
        var code = Totp(protector.LastSecret!, UtcNow.ToUnixTimeSeconds() / 30);
        var recovery = await service.CompleteTotpEnrollmentAsync(repository.UserId, code);

        Assert.True(await service.VerifyTotpAsync(repository.UserId, code));
        Assert.False(await service.VerifyTotpAsync(repository.UserId, code));
        Assert.True(await service.RedeemRecoveryCodeAsync(repository.UserId, recovery![0]));
        Assert.False(await service.RedeemRecoveryCodeAsync(repository.UserId, recovery[0]));
    }

    private static string Totp(byte[] key, long counter)
    {
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(bytes, counter);
        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(bytes.ToArray());
        var offset = hash[^1] & 0x0f;
        var binary = ((hash[offset] & 0x7f) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    }

    private sealed class ProtectorFake : ICustomerSecretProtector
    {
        public byte[]? LastSecret { get; private set; }
        public string Protect(byte[] secret) { LastSecret = secret.ToArray(); return Convert.ToBase64String(secret.Reverse().ToArray()); }
        public byte[] Unprotect(string protectedSecret) => Convert.FromBase64String(protectedSecret).Reverse().ToArray();
    }

    private sealed class RepositoryFake : ICustomerAuthenticationRepository
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public CustomerTotpAuthenticator? Authenticator { get; private set; }
        public IReadOnlyList<CustomerRecoveryCode> Codes { get; private set; } = [];
        private long? acceptedCounter;
        private readonly HashSet<string> usedCodes = [];
        public Task<CustomerTotpAuthenticator> SaveTotpAsync(CustomerTotpAuthenticator value, CancellationToken cancellationToken = default) { Authenticator = value; return Task.FromResult(value); }
        public Task<CustomerTotpAuthenticator?> GetTotpAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Authenticator);
        public Task<bool> VerifyTotpAsync(Guid id, DateTime verifiedUtc, CancellationToken cancellationToken = default) { Authenticator!.VerifiedUtc = verifiedUtc; return Task.FromResult(true); }
        public Task<bool> AcceptTotpCounterAsync(Guid id, long counter, CancellationToken cancellationToken = default)
        {
            if (acceptedCounter is not null && acceptedCounter >= counter) return Task.FromResult(false);
            acceptedCounter = counter; return Task.FromResult(true);
        }
        public Task ReplaceRecoveryCodesAsync(Guid userId, IReadOnlyList<CustomerRecoveryCode> codes, CancellationToken cancellationToken = default) { Codes = codes; return Task.CompletedTask; }
        public Task<bool> ConsumeRecoveryCodeAsync(Guid userId, string codeHash, DateTime usedUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(Codes.Any(item => item.CodeHash == codeHash) && usedCodes.Add(codeHash));
        public Task<CustomerAuthSession> CreateSessionAsync(CustomerAuthSession session, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<CustomerAuthSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> TouchSessionAsync(Guid sessionId, DateTime lastSeenUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> RevokeSessionAsync(string tokenHash, DateTime revokedUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<EmailLoginToken> CreateEmailLoginTokenAsync(EmailLoginToken token, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<EmailLoginToken?> ConsumeEmailLoginTokenAsync(string tokenHash, DateTime consumedUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider { public override DateTimeOffset GetUtcNow() => value; }
}

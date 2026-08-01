using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class CustomerStrongAuthenticationService(
    ICustomerAuthenticationRepository repository,
    ICustomerSecretProtector protector,
    TimeProvider timeProvider) : ICustomerStrongAuthenticationService
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public async Task<TotpEnrollment> BeginTotpEnrollmentAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A user identifier is required.", nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var secret = RandomNumberGenerator.GetBytes(20);
        var encoded = Base32(secret);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await repository.SaveTotpAsync(new CustomerTotpAuthenticator
        {
            Id = Guid.NewGuid(), UserId = userId, ProtectedSecret = protector.Protect(secret), CreatedUtc = utcNow
        }, cancellationToken).ConfigureAwait(false);
        var label = Uri.EscapeDataString($"Vennu:{email.Trim().ToLowerInvariant()}");
        return new TotpEnrollment(encoded, $"otpauth://totp/{label}?secret={encoded}&issuer=Vennu&algorithm=SHA1&digits=6&period=30");
    }

    public async Task<IReadOnlyList<string>?> CompleteTotpEnrollmentAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var authenticator = await repository.GetTotpAsync(userId, cancellationToken).ConfigureAwait(false);
        if (authenticator is null || authenticator.VerifiedUtc is not null || !TryValidate(authenticator, code, out _)) return null;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        if (!await repository.VerifyTotpAsync(authenticator.Id, utcNow, cancellationToken).ConfigureAwait(false)) return null;
        var plain = Enumerable.Range(0, 10).Select(_ => RecoveryCode()).ToArray();
        var entities = plain.Select(value => new CustomerRecoveryCode
        {
            Id = Guid.NewGuid(), UserId = userId, CodeHash = Hash(value), CreatedUtc = utcNow
        }).ToArray();
        await repository.ReplaceRecoveryCodesAsync(userId, entities, cancellationToken).ConfigureAwait(false);
        return plain;
    }

    public async Task<bool> VerifyTotpAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var authenticator = await repository.GetTotpAsync(userId, cancellationToken).ConfigureAwait(false);
        if (authenticator?.VerifiedUtc is null || !TryValidate(authenticator, code, out var counter)) return false;
        return await repository.AcceptTotpCounterAsync(authenticator.Id, counter, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> RedeemRecoveryCodeAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(code)) return Task.FromResult(false);
        return repository.ConsumeRecoveryCodeAsync(userId, Hash(NormalizeCode(code)), timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }

    private bool TryValidate(CustomerTotpAuthenticator authenticator, string code, out long acceptedCounter)
    {
        acceptedCounter = 0;
        var normalized = new string((code ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalized.Length != 6) return false;
        var secret = protector.Unprotect(authenticator.ProtectedSecret);
        var current = timeProvider.GetUtcNow().ToUnixTimeSeconds() / 30;
        for (var offset = -1; offset <= 1; offset++)
        {
            var counter = current + offset;
            if (FixedEquals(Totp(secret, counter), normalized)) { acceptedCounter = counter; return true; }
        }
        return false;
    }

    private static string Totp(byte[] key, long counter)
    {
        Span<byte> bytes = stackalloc byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(bytes, counter);
        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(bytes.ToArray());
        var offset = hash[^1] & 0x0f;
        var binary = ((hash[offset] & 0x7f) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    }

    private static bool FixedEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(left), Encoding.ASCII.GetBytes(right));
    private static string RecoveryCode() => $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(4))}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(4))}";
    private static string NormalizeCode(string value) => value.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(NormalizeCode(value))));
    private static string Base32(byte[] data)
    {
        var output = new StringBuilder((data.Length * 8 + 4) / 5);
        var buffer = 0; var bits = 0;
        foreach (var value in data)
        {
            buffer = (buffer << 8) | value; bits += 8;
            while (bits >= 5) { bits -= 5; output.Append(Alphabet[(buffer >> bits) & 31]); }
        }
        if (bits > 0) output.Append(Alphabet[(buffer << (5 - bits)) & 31]);
        return output.ToString();
    }
}

using System.Security.Cryptography;
using System.Text;

namespace Vennu.Data.Configuration;

public sealed class EnvironmentKeyConfigurationSecretProtector : IConfigurationSecretProtector
{
    private const byte PayloadVersion = 1;
    private readonly byte[] key;

    public EnvironmentKeyConfigurationSecretProtector(string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key);
        key = Convert.FromBase64String(base64Key);
        if (key.Length != 32) throw new ArgumentException("The configuration encryption key must be a Base64-encoded 256-bit key.", nameof(base64Key));
    }

    public string Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        var payload = new byte[1 + nonce.Length + tag.Length + ciphertext.Length];
        payload[0] = PayloadVersion;
        nonce.CopyTo(payload, 1);
        tag.CopyTo(payload, 13);
        ciphertext.CopyTo(payload, 29);
        return Convert.ToBase64String(payload);
    }

    public string Unprotect(string protectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue);
        var payload = Convert.FromBase64String(protectedValue);
        if (payload.Length < 29 || payload[0] != PayloadVersion) throw new CryptographicException("The configuration secret payload is invalid.");
        var plaintext = new byte[payload.Length - 29];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(payload.AsSpan(1, 12), payload.AsSpan(29), payload.AsSpan(13, 16), plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}

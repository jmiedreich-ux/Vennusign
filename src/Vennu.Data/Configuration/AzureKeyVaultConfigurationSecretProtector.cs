using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace Vennu.Data.Configuration;

public sealed class AzureKeyVaultConfigurationSecretProtector : IConfigurationSecretProtector
{
    private const byte PayloadVersion = 1;
    private readonly CryptographyClient client;

    public AzureKeyVaultConfigurationSecretProtector(Uri keyIdentifier, TokenCredential credential)
    {
        ArgumentNullException.ThrowIfNull(keyIdentifier);
        ArgumentNullException.ThrowIfNull(credential);
        client = new CryptographyClient(keyIdentifier, credential);
    }

    public string Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        var dataKey = RandomNumberGenerator.GetBytes(32);
        try
        {
            var wrappedKey = client.WrapKey(KeyWrapAlgorithm.RsaOaep256, dataKey).EncryptedKey;
            var nonce = RandomNumberGenerator.GetBytes(12);
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[16];
            using var aes = new AesGcm(dataKey, tag.Length);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            var payload = new byte[5 + wrappedKey.Length + nonce.Length + tag.Length + ciphertext.Length];
            payload[0] = PayloadVersion;
            BinaryPrimitives.WriteInt32BigEndian(payload.AsSpan(1, 4), wrappedKey.Length);
            wrappedKey.CopyTo(payload, 5);
            nonce.CopyTo(payload, 5 + wrappedKey.Length);
            tag.CopyTo(payload, 17 + wrappedKey.Length);
            ciphertext.CopyTo(payload, 33 + wrappedKey.Length);
            return Convert.ToBase64String(payload);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }

    public string Unprotect(string protectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue);
        var payload = Convert.FromBase64String(protectedValue);
        if (payload.Length < 33 || payload[0] != PayloadVersion) throw new CryptographicException("The Key Vault configuration payload is invalid.");
        var wrappedLength = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(1, 4));
        if (wrappedLength <= 0 || payload.Length < 33 + wrappedLength) throw new CryptographicException("The wrapped configuration key is invalid.");
        var dataKey = client.UnwrapKey(KeyWrapAlgorithm.RsaOaep256, payload.AsSpan(5, wrappedLength).ToArray()).Key;
        try
        {
            var plaintext = new byte[payload.Length - 33 - wrappedLength];
            using var aes = new AesGcm(dataKey, 16);
            aes.Decrypt(payload.AsSpan(5 + wrappedLength, 12), payload.AsSpan(33 + wrappedLength), payload.AsSpan(17 + wrappedLength, 16), plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }
}

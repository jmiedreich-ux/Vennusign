using System.Security.Cryptography;
using Vennu.Data.Configuration;

namespace Vennu.DataAccess.Tests.Configuration;

public sealed class EnvironmentKeyConfigurationSecretProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsWithoutEmbeddingPlaintext()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var sut = new EnvironmentKeyConfigurationSecretProtector(key);

        var protectedValue = sut.Protect("customer-secret");

        Assert.DoesNotContain("customer-secret", protectedValue, StringComparison.Ordinal);
        Assert.Equal("customer-secret", sut.Unprotect(protectedValue));
    }

    [Fact]
    public void Unprotect_RejectsTamperedPayload()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var sut = new EnvironmentKeyConfigurationSecretProtector(key);
        var payload = Convert.FromBase64String(sut.Protect("customer-secret"));
        payload[^1] ^= 1;

        Assert.ThrowsAny<CryptographicException>(() => sut.Unprotect(Convert.ToBase64String(payload)));
    }

    [Fact]
    public void Constructor_RequiresA256BitKey()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

        Assert.Throws<ArgumentException>(() => new EnvironmentKeyConfigurationSecretProtector(key));
    }
}

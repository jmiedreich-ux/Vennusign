using Microsoft.AspNetCore.DataProtection;
using Vennu.Api.Infrastructure;

namespace Vennu.Api.Tests.Infrastructure;

[Trait("Category", "Unit")]
public sealed class DataProtectionPosCredentialProtectorTests
{
    [Fact]
    public void Protect_RoundTripsWithoutReturningPlaintext()
    {
        var sut = new DataProtectionPosCredentialProtector(new EphemeralDataProtectionProvider());

        var protectedValue = sut.Protect("provider-secret");

        Assert.NotEqual("provider-secret", protectedValue);
        Assert.Equal("provider-secret", sut.Unprotect(protectedValue));
    }
}

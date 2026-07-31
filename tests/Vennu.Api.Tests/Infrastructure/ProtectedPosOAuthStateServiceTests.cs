using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Infrastructure;

[Trait("Category", "Unit")]
public sealed class ProtectedPosOAuthStateServiceTests
{
    [Fact]
    public void State_IsVenueCorrelatedAndSingleUse()
    {
        var venueId = Guid.NewGuid();
        var service = CreateService();

        var state = service.Create(venueId);

        Assert.Equal(venueId, service.Consume(state));
        Assert.Throws<InvalidOperationException>(() => service.Consume(state));
    }

    [Fact]
    public void State_RejectsTampering()
    {
        var service = CreateService();
        var state = service.Create(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => service.Consume(state + "tampered"));
    }

    private static ProtectedPosOAuthStateService CreateService() =>
        new(new EphemeralDataProtectionProvider(), new MemoryCache(new MemoryCacheOptions()), TimeProvider.System);
}

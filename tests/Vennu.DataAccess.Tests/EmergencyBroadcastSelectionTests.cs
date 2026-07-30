using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class EmergencyBroadcastSelectionTests
{
    [Fact]
    public void Select_PrefersScreenTarget_AndExpiresAutomatically()
    {
        var screenId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 7, 30, 8, 0, 0, TimeSpan.Zero);
        var venueWide = Broadcast(null, now.AddMinutes(-1), now.AddMinutes(20), "Venue");
        var targeted = Broadcast(screenId, now.AddMinutes(-2), now.AddMinutes(5), "Screen");

        Assert.Equal("Screen", EmergencyBroadcastSelection.Select([venueWide, targeted], screenId, now)?.Title);
        Assert.Null(EmergencyBroadcastSelection.Select([targeted], screenId, now.AddMinutes(6)));
    }

    [Fact]
    public void Select_IgnoresInactiveAndOtherScreenTargets()
    {
        var now = DateTimeOffset.UtcNow;
        var inactive = Broadcast(null, now.AddMinutes(-1), now.AddMinutes(5), "Inactive");
        inactive.IsActive = false;
        var other = Broadcast(Guid.NewGuid(), now.AddMinutes(-1), now.AddMinutes(5), "Other");

        Assert.Null(EmergencyBroadcastSelection.Select([inactive, other], Guid.NewGuid(), now));
    }

    private static EmergencyBroadcast Broadcast(Guid? screenId, DateTimeOffset start, DateTimeOffset end, string title) =>
        new() { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), ScreenId = screenId, Title = title,
            StartsUtc = start.UtcDateTime, ExpiresUtc = end.UtcDateTime, IsActive = true, CreatedUtc = start.UtcDateTime };
}

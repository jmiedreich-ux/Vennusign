using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.BackOffice;
using Vennu.Api.Controllers;
using Vennu.Api.TestAutomation;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Xunit;

namespace Vennu.Api.Tests.TestAutomation;

public sealed class TestAutomationControllerTests
{
    private static readonly Guid VenueId = Guid.Parse("73000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task Reset_requires_the_session_venue_to_be_allowlisted_and_delegates_once()
    {
        var repository = new FakeContentRepository();
        var controller = Create(repository, resetVenues: [VenueId]);

        var result = await controller.ResetVenue(new("token"), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal([VenueId], repository.ResetAutomationVenues);

        var refused = Create(repository, resetVenues: [Guid.NewGuid()]);
        Assert.IsType<NotFoundResult>(await refused.ResetVenue(new("token"), CancellationToken.None));
        Assert.Single(repository.ResetAutomationVenues);
    }

    [Fact]
    public async Task Backdate_reads_and_writes_only_the_session_venue_item()
    {
        var repository = new FakeContentRepository();
        var itemId = Guid.NewGuid();
        var changedUtc = new DateTime(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);
        repository.Availability.Add(new ItemAvailability
        {
            VenueId = VenueId,
            ItemId = itemId,
            IsAvailable = false,
            ChangedUtc = changedUtc,
            ChangedBy = "owner"
        });
        var controller = Create(repository, availabilityVenues: [VenueId]);

        Assert.IsType<NoContentResult>(await controller.BackdateAvailability(new("token", itemId, 90), CancellationToken.None));
        var updated = Assert.Single(repository.Availability);
        Assert.Equal(changedUtc.AddMinutes(-90), updated.ChangedUtc);

        Assert.IsType<NotFoundResult>(await controller.BackdateAvailability(new("token", Guid.NewGuid(), 90), CancellationToken.None));

        Assert.IsType<ObjectResult>(await controller.BackdateAvailability(new("token", itemId, 0), CancellationToken.None));
        Assert.Equal(changedUtc.AddMinutes(-90), Assert.Single(repository.Availability).ChangedUtc);
    }

    [Fact]
    public async Task History_write_uses_the_session_venue_and_exact_utc_instant()
    {
        var repository = new FakeContentRepository();
        repository.WorkingSnapshotJson = "{}";
        var menuId = Guid.NewGuid();
        repository.Menus.Add(new Menu { Id = menuId, VenueId = VenueId, Name = "History menu" });
        var occurredUtc = new DateTime(2026, 8, 10, 12, 30, 0, DateTimeKind.Utc);
        var controller = Create(repository, historyVenues: [VenueId]);

        Assert.IsType<NoContentResult>(await controller.WriteHistoryAt(
            new("token", menuId, MenuHistoryKinds.Assigned, "Seeded history", occurredUtc), CancellationToken.None));
        var entry = Assert.Single(repository.History);
        Assert.Equal(VenueId, entry.VenueId);
        Assert.Equal(menuId, entry.MenuId);
        Assert.Equal(occurredUtc, entry.OccurredUtc);

        Assert.IsType<ObjectResult>(await controller.WriteHistoryAt(
            new("token", menuId, "invented", null, occurredUtc), CancellationToken.None));
        Assert.Single(repository.History);
        Assert.IsType<NotFoundResult>(await Create(repository, historyVenues: [Guid.NewGuid()]).WriteHistoryAt(
            new("token", menuId, MenuHistoryKinds.Assigned, null, occurredUtc), CancellationToken.None));
    }

    private static TestAutomationController Create(
        FakeContentRepository repository,
        HashSet<Guid>? resetVenues = null,
        HashSet<Guid>? availabilityVenues = null,
        HashSet<Guid>? historyVenues = null)
    {
        var automation = new TestAutomationOptions
        {
            ApiKey = "automation-key",
            Scopes = ["venue.reset", "availability.backdate", "history.write_at"],
            ResetVenueIds = resetVenues ?? [],
            AvailabilityVenueIds = availabilityVenues ?? [],
            HistoryVenueIds = historyVenues ?? []
        };
        var sessions = new BackOfficeAuthenticationOptions
        {
            Sessions = [new BackOfficeSessionOptions { AccessToken = "token", VenueId = VenueId }]
        };
        var controller = new TestAutomationController(
            new TestAutomationAuthorization(Options.Create(automation)),
            new FixedOptionsMonitor<BackOfficeAuthenticationOptions>(sessions),
            repository);
        var context = new DefaultHttpContext();
        context.Request.Headers[TestAutomationOptions.HeaderName] = "automation-key";
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    private sealed class FixedOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Contracts.Venues;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.E2E;

[Trait("Category", "E2E")]
public sealed class Phase02VerticalSliceTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;
    private readonly HttpClient client;

    public Phase02VerticalSliceTests(VennuApiFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task PairedDisplay_TransitionsOnlineThenOfflineWhenHeartbeatBecomesStale()
    {
        var venueResponse = await client.PostAsJsonAsync("/api/venues", new CreateVenueRequest
        {
            Name = "Phase 02 Venue",
            Timezone = "UTC",
            Type = "Bar",
            PrimaryLanguage = "en"
        });
        Assert.Equal(HttpStatusCode.Created, venueResponse.StatusCode);
        var venue = Assert.IsType<CreateVenueResponse>(await venueResponse.Content.ReadFromJsonAsync<CreateVenueResponse>());

        var screenResponse = await client.PostAsJsonAsync("/api/screens", new RegisterScreenRequest
        {
            Name = "Phase 02 Display",
            Location = "Main Wall",
            Platform = "web",
            AppVersion = "1.0.0"
        });
        Assert.Equal(HttpStatusCode.Created, screenResponse.StatusCode);
        var screen = Assert.IsType<RegisterScreenResponse>(await screenResponse.Content.ReadFromJsonAsync<RegisterScreenResponse>());

        var pairingResponse = await client.PostAsJsonAsync("/api/screens/pairing-code", new CreateScreenPairingCodeRequest
        {
            ScreenId = screen.ScreenId
        });
        Assert.Equal(HttpStatusCode.Created, pairingResponse.StatusCode);
        var pairing = Assert.IsType<CreateScreenPairingCodeResponse>(await pairingResponse.Content.ReadFromJsonAsync<CreateScreenPairingCodeResponse>());

        using var claimRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/screens/pairing/{pairing.Code}/claim");
        claimRequest.Headers.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");
        claimRequest.Content = JsonContent.Create(new ClaimScreenPairingCodeRequest { VenueId = venue.VenueId });
        var claimResponse = await client.SendAsync(claimRequest);
        claimResponse.EnsureSuccessStatusCode();

        var heartbeatResponse = await client.PostAsJsonAsync($"/api/display/{screen.ScreenId}/heartbeat", new ScreenHeartbeatRequest
        {
            Status = "Online"
        });
        heartbeatResponse.EnsureSuccessStatusCode();

        var onlineContentResponse = await client.GetAsync($"/api/display/{screen.ScreenId}/content");
        onlineContentResponse.EnsureSuccessStatusCode();
        var onlineContent = Assert.IsType<DisplayContentResponse>(await onlineContentResponse.Content.ReadFromJsonAsync<DisplayContentResponse>());
        Assert.Equal("Online", onlineContent.Status);
        Assert.NotNull(onlineContent.LastSeenUtc);

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IScreenRepository>();
        var storedScreen = Assert.IsType<Vennu.Core.Models.Screen>(await repository.GetByIdAsync(screen.ScreenId));
        storedScreen.LastSeen = DateTime.UtcNow.AddSeconds(-91);

        var markedOffline = await repository.MarkStaleOnlineScreensOfflineAsync(DateTime.UtcNow.AddSeconds(-90));
        Assert.Equal(1, markedOffline);

        var offlineContentResponse = await client.GetAsync($"/api/display/{screen.ScreenId}/content");
        offlineContentResponse.EnsureSuccessStatusCode();
        var offlineContent = Assert.IsType<DisplayContentResponse>(await offlineContentResponse.Content.ReadFromJsonAsync<DisplayContentResponse>());
        Assert.Equal("Offline", offlineContent.Status);
    }
}

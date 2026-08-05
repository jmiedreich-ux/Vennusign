using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Contracts.Venues;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Xunit.Abstractions;

namespace Vennu.Api.Tests.E2E;

[Trait("Category", "E2E")]
public class ApiE2ETests : IClassFixture<VennuApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly HttpClient client;
    private readonly ITestOutputHelper output;

    public ApiE2ETests(VennuApiFactory factory, ITestOutputHelper output)
    {
        this.output = output;
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task PairingFlow_CanBeDrivenThroughHttpApi()
    {
        var rootResponse = await GetAsync("/");
        rootResponse.EnsureSuccessStatusCode();

        var venueResponse = await PostAsJsonAsync("/api/venues", new CreateVenueRequest
        {
            Name = "E2E Venue",
            Timezone = "UTC",
            Type = "Bar",
            PrimaryLanguage = "en"
        });

        Assert.Equal(HttpStatusCode.Created, venueResponse.StatusCode);
        var venue = await ReadJsonAsync<CreateVenueResponse>(venueResponse);
        Assert.NotEqual(Guid.Empty, venue.VenueId);

        var screenResponse = await PostAsJsonAsync("/api/screens", new RegisterScreenRequest
        {
            Name = "E2E Screen",
            Location = "North Wall",
            Platform = "web",
            AppVersion = "1.0.0"
        });

        Assert.Equal(HttpStatusCode.Created, screenResponse.StatusCode);
        var screen = await ReadJsonAsync<RegisterScreenResponse>(screenResponse);
        Assert.NotEqual(Guid.Empty, screen.ScreenId);
        Assert.StartsWith("sc-", screen.ScreenKey, StringComparison.Ordinal);

        var pairingResponse = await PostAsJsonAsync("/api/screens/pairing-code", new CreateScreenPairingCodeRequest
        {
            ScreenId = screen.ScreenId
        });

        Assert.Equal(HttpStatusCode.Created, pairingResponse.StatusCode);
        var pairing = await ReadJsonAsync<CreateScreenPairingCodeResponse>(pairingResponse);
        Assert.Equal(screen.ScreenId, pairing.ScreenId);
        Assert.Equal(6, pairing.Code.Length);

        var beforeClaimResponse = await GetAsync($"/api/screens/pairing/{pairing.Code}/status");
        beforeClaimResponse.EnsureSuccessStatusCode();
        var beforeClaim = await ReadJsonAsync<ScreenPairingStatusResponse>(beforeClaimResponse);
        Assert.False(beforeClaim.Linked);
        Assert.Null(beforeClaim.ScreenId);

        using var claimRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/screens/pairing/{pairing.Code}/claim");
        claimRequest.Headers.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");
        claimRequest.Content = JsonContent.Create(new ClaimScreenPairingCodeRequest { VenueId = venue.VenueId });
        var claimResponse = await client.SendAsync(claimRequest);

        claimResponse.EnsureSuccessStatusCode();
        var claim = await ReadJsonAsync<ClaimScreenPairingCodeResponse>(claimResponse);
        Assert.True(claim.Linked);
        Assert.Equal(screen.ScreenId, claim.ScreenId);
        Assert.Equal(venue.VenueId, claim.VenueId);

        var afterClaimResponse = await GetAsync($"/api/screens/pairing/{pairing.Code}/status");
        afterClaimResponse.EnsureSuccessStatusCode();
        var afterClaim = await ReadJsonAsync<ScreenPairingStatusResponse>(afterClaimResponse);
        Assert.True(afterClaim.Linked);
        Assert.Equal(screen.ScreenId, afterClaim.ScreenId);

        var heartbeatResponse = await PostAsJsonAsync($"/api/display/{screen.ScreenId}/heartbeat", new ScreenHeartbeatRequest
        {
            Status = " Online "
        });

        heartbeatResponse.EnsureSuccessStatusCode();
        var heartbeat = await ReadJsonAsync<ScreenHeartbeatResponse>(heartbeatResponse);
        Assert.Equal(screen.ScreenId, heartbeat.ScreenId);
        Assert.Equal("Online", heartbeat.Status);

        var contentResponse = await GetAsync($"/api/display/{screen.ScreenId}/content");
        contentResponse.EnsureSuccessStatusCode();
        var content = await ReadJsonAsync<DisplayContentResponse>(contentResponse);
        Assert.Equal(screen.ScreenId, content.ScreenId);
        Assert.Equal(venue.VenueId, content.VenueId);
        Assert.Equal(screen.ScreenKey, content.ScreenKey);
        Assert.Equal("E2E Screen", content.ScreenName);
        Assert.Equal("Online", content.Status);
        Assert.Equal("default", content.Layout);
        Assert.NotNull(content.LastSeenUtc);
    }

    private async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        output.WriteLine($"--> GET {requestUri}");

        var response = await client.GetAsync(requestUri);
        await LogResponseAsync(response);

        return response;
    }

    private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        output.WriteLine($"--> POST {requestUri}");
        output.WriteLine(JsonSerializer.Serialize(value, JsonOptions));

        var response = await client.PostAsJsonAsync(requestUri, value);
        await LogResponseAsync(response);

        return response;
    }

    private async Task LogResponseAsync(HttpResponseMessage response)
    {
        output.WriteLine($"<-- {(int)response.StatusCode} {response.ReasonPhrase}");

        var body = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(body))
        {
            output.WriteLine(body);
        }
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>();
        return Assert.IsType<T>(value);
    }
}

public sealed class VennuApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("PlatformOperations:ApiKey", "test-admin-key");
        builder.UseSetting("BackOffice:Sessions:0:AccessToken", "test-venue-token");
        builder.UseSetting("BackOffice:Sessions:0:VenueId", "11111111-1111-1111-1111-111111111111");
        builder.UseSetting("BackOffice:Sessions:0:OrganizationId", "22222222-2222-2222-2222-222222222222");
        builder.UseSetting("BackOffice:Sessions:0:UserId", "33333333-3333-3333-3333-333333333333");
        builder.UseSetting("BackOffice:Sessions:0:DisplayName", "Harbor Owner");
        builder.UseSetting("BackOffice:Sessions:0:SystemRole", "organization_owner");
        builder.UseSetting("Stripe:Webhook:SigningSecret", "whsec_test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IVenueRepository>();
            services.RemoveAll<IScreenRepository>();
            services.RemoveAll<IScreenPairingCodeRepository>();
            services.RemoveAll<ICapabilityAccessPolicyRepository>();
            services.RemoveAll<IScopedAuthorityRepository>();

            services.AddSingleton<InMemoryApiStore>();
            services.AddScoped<IVenueRepository, InMemoryVenueRepository>();
            services.AddScoped<IScreenRepository, InMemoryScreenRepository>();
            services.AddScoped<IScreenPairingCodeRepository, InMemoryScreenPairingCodeRepository>();
            services.AddScoped<ICapabilityAccessPolicyRepository, InMemoryCapabilityAccessPolicyRepository>();
            services.AddScoped<IScopedAuthorityRepository, InMemoryScopedAuthorityRepository>();
        });
    }
}

internal sealed class InMemoryScopedAuthorityRepository : IScopedAuthorityRepository
{
    public Task<IReadOnlyCollection<ScopedRoleAssignment>> GetActiveAssignmentsAsync(Guid actorUserId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<ScopedRoleAssignment>>([]);
    public Task SaveAssignmentAsync(ScopedRoleAssignment assignment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<SupportAccessGrant?> GetActiveSupportGrantAsync(Guid supportUserId, Guid organizationId, Guid? venueId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<SupportAccessGrant?>(null);
    public Task SaveSupportGrantAsync(SupportAccessGrant grant, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task AppendSupportAuditAsync(SupportAccessAuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class InMemoryCapabilityAccessPolicyRepository : ICapabilityAccessPolicyRepository
{
    public Task<CapabilityAccessPolicy> GetAsync(
        Guid organizationId,
        Guid venueId,
        CapabilityId capability,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(CapabilityAccessPolicy.DefaultFor(Version1CapabilityRegistry.Get(capability)));
}

internal sealed class InMemoryApiStore
{
    public ConcurrentDictionary<Guid, Venue> Venues { get; } = new();

    public ConcurrentDictionary<Guid, Screen> Screens { get; } = new();

    public ConcurrentDictionary<string, ScreenPairingCode> PairingCodes { get; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class InMemoryVenueRepository : IVenueRepository
{
    private readonly InMemoryApiStore store;

    public InMemoryVenueRepository(InMemoryApiStore store) => this.store = store;

    public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        venue.Id = venue.Id == Guid.Empty ? Guid.NewGuid() : venue.Id;
        store.Venues[venue.Id] = venue;
        return Task.FromResult(venue.Id);
    }

    public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<Venue>>(store.Venues.Values.ToArray());
    }

    public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        store.Venues.TryGetValue(venueId, out var venue);
        return Task.FromResult(venue);
    }
}

internal sealed class InMemoryScreenRepository : IScreenRepository
{
    private readonly InMemoryApiStore store;

    public InMemoryScreenRepository(InMemoryApiStore store) => this.store = store;

    public Task<Guid> CreateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        screen.Id = screen.Id == Guid.Empty ? Guid.NewGuid() : screen.Id;
        store.Screens[screen.Id] = screen;
        return Task.FromResult(screen.Id);
    }

    public Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default)
    {
        if (!store.Screens.TryGetValue(screenId, out var screen))
        {
            return Task.FromResult(false);
        }

        screen.VenueId = venueId;
        return Task.FromResult(true);
    }

    public Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        store.Screens.TryGetValue(screenId, out var screen);
        return Task.FromResult(screen);
    }

    public Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default)
    {
        var screen = store.Screens.Values.FirstOrDefault(screen => string.Equals(screen.ScreenKey, screenKey, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(screen);
    }

    public Task<Screen?> GetByPreRegistrationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var screen = store.Screens.Values.FirstOrDefault(screen =>
            string.Equals(screen.PreRegistrationTokenHash, tokenHash, StringComparison.Ordinal));
        return Task.FromResult(screen);
    }

    public Task<IReadOnlyCollection<Screen>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Screen>>(store.Screens.Values.ToArray());

    public Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Screen> screens = store.Screens.Values.Where(screen => screen.VenueId == venueId).ToArray();
        return Task.FromResult(screens);
    }

    public Task<bool> UpdateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        if (!store.Screens.ContainsKey(screen.Id))
        {
            return Task.FromResult(false);
        }

        store.Screens[screen.Id] = screen;
        return Task.FromResult(true);
    }

    public Task<bool> ClaimPreRegisteredAsync(Guid screenId, string platform, string appVersion, DateTime claimedUtc, CancellationToken cancellationToken = default)
    {
        if (!store.Screens.TryGetValue(screenId, out var screen))
        {
            return Task.FromResult(false);
        }
        screen.Platform = platform;
        screen.AppVersion = appVersion;
        screen.PreRegistrationTokenHash = null;
        screen.PreRegistrationExpiresUtc = null;
        screen.PreRegisteredUtc = claimedUtc;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default) =>
        UpdateHeartbeatAsync(screenId, lastSeenUtc, status, null, null, cancellationToken);

    public Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, string? platform, string? appVersion, CancellationToken cancellationToken = default)
    {
        if (!store.Screens.TryGetValue(screenId, out var screen))
        {
            return Task.FromResult(false);
        }

        screen.LastSeen = lastSeenUtc;
        screen.Status = status;
        screen.Platform = platform ?? screen.Platform;
        screen.AppVersion = appVersion ?? screen.AppVersion;
        return Task.FromResult(true);
    }

    public Task<int> MarkStaleOnlineScreensOfflineAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        var updatedUtc = DateTime.UtcNow;
        var staleScreens = store.Screens.Values
            .Where(screen => string.Equals(screen.Status, "Online", StringComparison.Ordinal)
                && screen.LastSeen.HasValue
                && screen.LastSeen.Value < cutoffUtc)
            .ToArray();

        foreach (var screen in staleScreens)
        {
            screen.Status = "Offline";
            screen.UpdatedUtc = updatedUtc;
        }

        return Task.FromResult(staleScreens.Length);
    }
}

internal sealed class InMemoryScreenPairingCodeRepository : IScreenPairingCodeRepository
{
    private readonly InMemoryApiStore store;

    public InMemoryScreenPairingCodeRepository(InMemoryApiStore store) => this.store = store;

    public Task<string> CreateAsync(ScreenPairingCode pairingCode, CancellationToken cancellationToken = default)
    {
        store.PairingCodes[pairingCode.Code] = pairingCode;
        return Task.FromResult(pairingCode.Code);
    }

    public Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        store.PairingCodes.TryGetValue(code, out var pairingCode);
        return Task.FromResult(pairingCode);
    }

    public Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default)
    {
        if (!store.PairingCodes.TryGetValue(code, out var pairingCode) || pairingCode.IsClaimed || pairingCode.ExpiresAt <= DateTime.UtcNow)
        {
            return Task.FromResult(false);
        }

        pairingCode.VenueId = venueId;
        pairingCode.IsClaimed = true;
        pairingCode.ClaimedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }
}

using System.Net;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Core.Models;
using Vennu.Data;

namespace Vennu.Api.Tests.E2E;

[Trait("Category", "Unit")]
public sealed class Phase12CriticalJourneyTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public Phase12CriticalJourneyTests(VennuApiFactory factory) => this.factory = factory;

    public static TheoryData<string, string> ProtectedVenueOperations => new()
    {
        { "GET", "/api/venue-admin/pos/square/status" },
        { "POST", "/api/venue-admin/pos/square/connect" },
        { "POST", "/api/venue-admin/pos/square/catalog/import" },
        { "DELETE", "/api/venue-admin/pos/square/connection" },
        { "GET", "/api/venue-admin/pos/toast/status" },
        { "PUT", "/api/venue-admin/pos/toast/connection" },
        { "POST", "/api/venue-admin/pos/toast/catalog/import" },
        { "GET", "/api/venue-admin/pos/clover/status" },
        { "POST", "/api/venue-admin/pos/clover/connect" },
        { "POST", "/api/venue-admin/pos/clover/catalog/import" },
        { "DELETE", "/api/venue-admin/pos/clover/connection" }
    };

    [Theory]
    [MemberData(nameof(ProtectedVenueOperations))]
    public async Task VenuePosOperations_RequireVenueAdminAuthentication(string method, string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void PublicStatusContracts_AreCredentialFree()
    {
        var contractTypes = new[]
        {
            typeof(VenueAdminPosConnectResponse),
            typeof(VenueAdminPosConnectionResponse),
            typeof(VenueAdminToastStatusResponse),
            typeof(VenueAdminToastPollingHealthResponse),
            typeof(VenueAdminCloverStatusResponse)
        };

        string[] forbiddenCredentialMembers =
        [
            "AccessToken", "RefreshToken", "ProtectedAccessToken", "ProtectedRefreshToken",
            "Secret", "Credentials"
        ];
        var exposed = contractTypes
            .SelectMany(type => type.GetProperties().Select(property => $"{type.Name}.{property.Name}"))
            .Where(name => forbiddenCredentialMembers.Any(forbidden =>
                name.EndsWith($".{forbidden}", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.Empty(exposed);
    }

    [Fact]
    public void Phase12Migrations_AreEmbeddedInContiguousOrder()
    {
        var phaseScripts = DatabaseMigrator.GetEmbeddedScriptNames()
            .Where(name => Enumerable.Range(35, 5).Any(number =>
                name.Contains($".Scripts.{number:000}_", StringComparison.Ordinal)))
            .ToArray();

        Assert.Equal(5, phaseScripts.Length);
        Assert.Collection(phaseScripts,
            name => Assert.EndsWith(".Scripts.035_create_pos_connections.sql", name, StringComparison.Ordinal),
            name => Assert.EndsWith(".Scripts.036_create_pos_catalog_mappings.sql", name, StringComparison.Ordinal),
            name => Assert.EndsWith(".Scripts.037_create_pos_webhook_events.sql", name, StringComparison.Ordinal),
            name => Assert.EndsWith(".Scripts.038_add_pos_sync_health.sql", name, StringComparison.Ordinal),
            name => Assert.EndsWith(".Scripts.039_add_pos_refresh_token_expiration.sql", name, StringComparison.Ordinal));
    }

    [Fact]
    public void ProviderContract_CoversApprovedPhase12ProvidersOnly()
    {
        Assert.Equal(
            [PosProvider.Square, PosProvider.Toast, PosProvider.Clover],
            Enum.GetValues<PosProvider>());
    }
}

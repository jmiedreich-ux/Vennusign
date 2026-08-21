using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Contracts.Venues;
using Vennu.Api.Controllers;
using Vennu.Data;
using Vennu.Data.Repositories;
using Vennu.DataAccess;

namespace Vennu.Api.Tests.Integration;

/// <summary>
/// The pair / claim / heartbeat vertical slice against a real SQL Server.
///
/// <para>
/// This test used to read <c>VENU_TEST_AZURE_SQL_CONNECTION_STRING</c> and, if it
/// was set, run <c>DELETE FROM dbo.ScreenPairingCodes; DELETE FROM dbo.Screens;
/// DELETE FROM dbo.Venues;</c> against whatever it pointed at - with no guard on
/// which database that was. On a developer machine that variable pointed at the
/// dev product database. The only thing standing between this test and every venue
/// and screen on dev was a password that had gone stale.
/// </para>
///
/// <para>
/// It now creates its own database, uses it, and drops it. A run that owns its
/// database needs no cleanup step, so the destructive one is gone rather than
/// guarded - there is nothing here that can reach a database somebody cares about.
/// Unsetting the variable would have fixed today; owning the database fixes it.
/// </para>
/// </summary>
[Trait("Category", "Integration")]
public class Phase02VerticalSliceIntegrationTests
{
    private const string LocalDb =
        @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

    [Fact]
    public async Task Phase02_VerticalSlice_PairClaimHeartbeat_WorksAgainstRealSqlServer()
    {
        var database = $"vennusign_phase02_{Guid.NewGuid():N}"[..40];
        var master = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = "master" }.ConnectionString;
        var connectionString = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = database }.ConnectionString;

        await ExecuteAsync(master, $"CREATE DATABASE [{database}];");

        try
        {
            DatabaseMigrator.Run(connectionString);

            var dataAccess = CreateSqlDataAccess(connectionString);
            var venueRepository = new VenueRepository(dataAccess);
            var menuRepository = new MenuRepository(dataAccess);
            var screenRepository = new ScreenRepository(dataAccess);
            var pairingRepository = new ScreenPairingCodeRepository(dataAccess);
            var venuesController = new VenuesController(venueRepository);
            var screensController = new ScreensController(screenRepository, pairingRepository, venueRepository);
            var displayController = new DisplayController(screenRepository, venueRepository, menuRepository);

            var createVenueResult = await venuesController.CreateVenue(new CreateVenueRequest
            {
                Name = "Integration Venue",
                Timezone = "UTC",
                Type = "Bar",
                PrimaryLanguage = "en"
            }, CancellationToken.None);

            var venueCreated = Assert.IsType<CreatedResult>(createVenueResult.Result);
            var venueResponse = Assert.IsType<CreateVenueResponse>(venueCreated.Value);
            Assert.NotEqual(Guid.Empty, venueResponse.VenueId);

            var registerScreenResult = await screensController.RegisterScreen(new RegisterScreenRequest
            {
                Name = "Integration Screen",
                Platform = "web",
                AppVersion = "1.0.0"
            }, CancellationToken.None);

            var screenCreated = Assert.IsType<CreatedResult>(registerScreenResult.Result);
            var screenResponse = Assert.IsType<RegisterScreenResponse>(screenCreated.Value);
            Assert.NotEqual(Guid.Empty, screenResponse.ScreenId);
            Assert.StartsWith("sc-", screenResponse.ScreenKey, StringComparison.Ordinal);

            var createCodeResult = await screensController.CreatePairingCode(new CreateScreenPairingCodeRequest
            {
                ScreenId = screenResponse.ScreenId
            }, CancellationToken.None);

            var codeCreated = Assert.IsType<CreatedResult>(createCodeResult.Result);
            var codeResponse = Assert.IsType<CreateScreenPairingCodeResponse>(codeCreated.Value);
            Assert.Equal(6, codeResponse.Code.Length);

            var statusBeforeClaimResult = await screensController.GetPairingStatus(codeResponse.Code, CancellationToken.None);
            var statusBeforeClaim = Assert.IsType<OkObjectResult>(statusBeforeClaimResult.Result);
            var statusBeforeClaimResponse = Assert.IsType<ScreenPairingStatusResponse>(statusBeforeClaim.Value);
            Assert.False(statusBeforeClaimResponse.Linked);

            var claimResult = await screensController.ClaimPairingCode(codeResponse.Code, new ClaimScreenPairingCodeRequest
            {
                VenueId = venueResponse.VenueId
            }, CancellationToken.None);

            var claimOk = Assert.IsType<OkObjectResult>(claimResult.Result);
            var claimResponse = Assert.IsType<ClaimScreenPairingCodeResponse>(claimOk.Value);
            Assert.True(claimResponse.Linked);
            Assert.Equal(venueResponse.VenueId, claimResponse.VenueId);
            Assert.Equal(screenResponse.ScreenId, claimResponse.ScreenId);

            var statusAfterClaimResult = await screensController.GetPairingStatus(codeResponse.Code, CancellationToken.None);
            var statusAfterClaim = Assert.IsType<OkObjectResult>(statusAfterClaimResult.Result);
            var statusAfterClaimResponse = Assert.IsType<ScreenPairingStatusResponse>(statusAfterClaim.Value);
            Assert.True(statusAfterClaimResponse.Linked);
            Assert.Equal(screenResponse.ScreenId, statusAfterClaimResponse.ScreenId);

            var heartbeatResult = await displayController.Heartbeat(screenResponse.ScreenId, new ScreenHeartbeatRequest { Status = "Online" }, CancellationToken.None);
            var heartbeatOk = Assert.IsType<OkObjectResult>(heartbeatResult.Result);
            var heartbeatResponse = Assert.IsType<ScreenHeartbeatResponse>(heartbeatOk.Value);
            Assert.Equal("Online", heartbeatResponse.Status);
        }
        finally
        {
            await ExecuteAsync(master,
                $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END;");
        }
    }

    private static SqlDataAccess CreateSqlDataAccess(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionString"] = connectionString }).Build();
        return new SqlDataAccess(configuration);
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        _ = await command.ExecuteNonQueryAsync();
    }
}

using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class TestAutomationResetIntegrationTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Reset_rolls_back_on_an_unknown_screen_reference_then_clears_every_known_path()
    {
        var data = fixture.CreateDataAccess();
        var repository = new ContentRepository(data);
        var venueId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var probeTable = $"ResetProbe_{Guid.NewGuid():N}";
        var pairingCode = fixture.UniqueCode();

        await data.ExecuteSqlQueryAsync<Row, object>("""
            INSERT dbo.Venues (Id, Name, Timezone, Type, PrimaryLanguage)
            VALUES (@VenueId, N'Reset integration', N'UTC', N'Test', N'en');
            INSERT dbo.Screens (Id, VenueId, ScreenKey, Name, Status)
            VALUES (@SourceId, @VenueId, @SourceKey, N'Source', N'Offline'),
                   (@TargetId, @VenueId, @TargetKey, N'Target', N'Offline');
            INSERT dbo.ScreenPairingCodes (Code, VenueId, ScreenId, ExpiresAt)
            VALUES (@PairingCode, @VenueId, @TargetId, DATEADD(minute, 5, SYSUTCDATETIME()));
            INSERT dbo.ScreenContentDeliveries
                (Id, ScreenId, VenueId, Revision, State, RequestedUtc, UpdatedUtc)
            VALUES (NEWID(), @TargetId, @VenueId, 1, N'Requested', SYSUTCDATETIME(), SYSUTCDATETIME());
            INSERT dbo.ScreenReplacementAudits
                (Id, VenueId, TargetScreenId, SourceScreenId, PairingCode, Actor, OccurredUtc)
            VALUES (NEWID(), @VenueId, @TargetId, @SourceId, @PairingCode, N'integration', SYSUTCDATETIME());
            SELECT 1 AS Value;
            """, new
            {
                VenueId = venueId,
                SourceId = sourceId,
                TargetId = targetId,
                SourceKey = fixture.UniqueScreenKey(),
                TargetKey = fixture.UniqueScreenKey(),
                PairingCode = pairingCode
            });

        try
        {
            await data.ExecuteSqlQueryAsync<Row, object>($"""
                CREATE TABLE dbo.[{probeTable}]
                (
                    ScreenId UNIQUEIDENTIFIER NOT NULL
                        CONSTRAINT [FK_{probeTable}_Screens] REFERENCES dbo.Screens(Id)
                );
                INSERT dbo.[{probeTable}] (ScreenId) VALUES (@ScreenId);
                SELECT 1 AS Value;
                """, new { ScreenId = targetId });

            await Assert.ThrowsAnyAsync<Exception>(() => repository.ResetAutomationVenueAsync(venueId));

            Assert.Equal(2, await CountAsync(data, "dbo.Screens", venueId));
            Assert.Equal(1, await CountAsync(data, "dbo.ScreenContentDeliveries", venueId));
            Assert.Equal(1, await CountAsync(data, "dbo.ScreenPairingCodes", venueId));

            await data.ExecuteSqlQueryAsync<Row, object>($"DROP TABLE dbo.[{probeTable}]; SELECT 1 AS Value;", new { });
            await repository.ResetAutomationVenueAsync(venueId);

            Assert.Equal(0, await CountAsync(data, "dbo.Screens", venueId));
            Assert.Equal(0, await CountAsync(data, "dbo.ScreenContentDeliveries", venueId));
            Assert.Equal(0, await CountAsync(data, "dbo.ScreenPairingCodes", venueId));
            Assert.Equal(0, await CountAsync(data, "dbo.ScreenReplacementAudits", venueId));
        }
        finally
        {
            await data.ExecuteSqlQueryAsync<Row, object>($"""
                IF OBJECT_ID(N'dbo.{probeTable}', N'U') IS NOT NULL DROP TABLE dbo.[{probeTable}];
                DELETE FROM dbo.ScreenReplacementAudits WHERE VenueId = @VenueId;
                DELETE FROM dbo.ScreenContentDeliveries WHERE VenueId = @VenueId;
                DELETE FROM dbo.ScreenPairingCodes WHERE VenueId = @VenueId;
                DELETE FROM dbo.Screens WHERE VenueId = @VenueId;
                DELETE FROM dbo.Venues WHERE Id = @VenueId;
                SELECT 1 AS Value;
                """, new { VenueId = venueId });
        }
    }

    private static async Task<int> CountAsync(Vennu.DataAccess.ISqlDataAccess data, string table, Guid venueId)
    {
        var rows = await data.ExecuteSqlQueryAsync<Row, object>($"SELECT COUNT(*) AS Value FROM {table} WHERE VenueId = @VenueId;", new { VenueId = venueId });
        return rows.Single().Value;
    }

    private sealed class Row { public int Value { get; set; } }
}

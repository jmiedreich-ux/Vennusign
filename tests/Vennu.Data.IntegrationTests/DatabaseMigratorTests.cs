namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Unit")]
public class DatabaseMigratorTests
{
    [Fact]
    public void GetEmbeddedScriptNames_ReturnsAllMigrationScriptsFromVennuDataAssembly()
    {
        var scriptNames = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Equal(
            [
                "Vennu.Data.Scripts.001_create_venues.sql",
                "Vennu.Data.Scripts.002_create_screens.sql",
                "Vennu.Data.Scripts.003_create_screen_pairing_codes.sql",
                "Vennu.Data.Scripts.004_create_feature_tier_core.sql",
                "Vennu.Data.Scripts.005_create_venue_feature_overrides.sql",
                "Vennu.Data.Scripts.006_create_feature_usages.sql",
                "Vennu.Data.Scripts.007_add_stripe_billing_catalog.sql",
                "Vennu.Data.Scripts.008_create_processed_stripe_events.sql",
                "Vennu.Data.Scripts.009_create_feature_matrix_audit.sql",
                "Vennu.Data.Scripts.010_create_operational_events.sql",
                "Vennu.Data.Scripts.011_create_revenue_daily_snapshots.sql"
            ],
            scriptNames);
    }

    [Fact]
    public void GetEmbeddedScriptNames_ReturnsEmptyForAssemblyWithoutMigrationScripts()
    {
        var scriptNames = DatabaseMigrator.GetEmbeddedScriptNames(typeof(DatabaseMigratorTests).Assembly);

        Assert.Empty(scriptNames);
    }
}

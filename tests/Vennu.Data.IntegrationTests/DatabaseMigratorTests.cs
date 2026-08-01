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
                "Vennu.Data.Scripts.011_create_revenue_daily_snapshots.sql",
                "Vennu.Data.Scripts.012_create_menu_domain.sql",
                "Vennu.Data.Scripts.013_add_quick_update.sql",
                "Vennu.Data.Scripts.014_add_video_wall_feature.sql",
                "Vennu.Data.Scripts.015_add_photo_grid_density.sql",
                "Vennu.Data.Scripts.016_add_screen_display_layout.sql",
                "Vennu.Data.Scripts.017_create_venue_themes.sql",
                "Vennu.Data.Scripts.018_add_advanced_venue_themes.sql",
                "Vennu.Data.Scripts.019_add_split_layout.sql",
                "Vennu.Data.Scripts.020_add_daily_special_hero.sql",
                "Vennu.Data.Scripts.021_add_hero_dwell_seconds.sql",
                "Vennu.Data.Scripts.022_create_meal_periods.sql",
                "Vennu.Data.Scripts.023_add_meal_period_targets.sql",
                "Vennu.Data.Scripts.024_create_happy_hour_schedules.sql",
                "Vennu.Data.Scripts.025_create_playlist_slides.sql",
                "Vennu.Data.Scripts.026_create_emergency_broadcasts.sql",
                "Vennu.Data.Scripts.027_create_date_range_promotions.sql",
                "Vennu.Data.Scripts.028_create_tap_domain.sql",
                "Vennu.Data.Scripts.029_add_classic_chalkboard_layout.sql",
                "Vennu.Data.Scripts.030_add_tap_strips_layout.sql",
                "Vennu.Data.Scripts.031_add_digital_tap_board_layout.sql",
                "Vennu.Data.Scripts.032_add_screen_pre_registration.sql",
                "Vennu.Data.Scripts.033_add_subscription_period_end_state.sql",
                "Vennu.Data.Scripts.034_create_haas_contracts.sql",
                "Vennu.Data.Scripts.035_create_pos_connections.sql",
                "Vennu.Data.Scripts.036_create_pos_catalog_mappings.sql",
                "Vennu.Data.Scripts.037_create_pos_webhook_events.sql",
                "Vennu.Data.Scripts.038_add_pos_sync_health.sql",
                "Vennu.Data.Scripts.039_add_pos_refresh_token_expiration.sql"
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

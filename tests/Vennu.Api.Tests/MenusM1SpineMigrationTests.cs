using Vennu.Data;

namespace Vennu.Api.Tests;

/// <summary>
/// The migration is the authority on what the item library keeps and what it
/// discards, so these assert the script itself rather than a running database.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenusM1SpineMigrationTests
{
    private static string ReadSpineMigration()
    {
        var scriptName = Assert.Single(
            DatabaseMigrator.GetEmbeddedScriptNames()
                .Where(name => name.EndsWith(".Scripts.058_create_menu_item_library_spine.sql", StringComparison.Ordinal)));

        using var stream = Assert.IsAssignableFrom<Stream>(
            typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public void SpineMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.058_create_menu_item_library_spine.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void SpineMigration_CreatesTheLibraryAndSaveModelTables()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("CREATE TABLE dbo.Items", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.Placements", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.ItemAvailability", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuScreenAssignments", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuDraftChanges", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuPublishEvents", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuPublishTargets", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuHistoryEntries", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_NamesEveryFieldItDiscardsFromTheLibrary()
    {
        var sql = ReadSpineMigration();

        // AGENTS.md: a migration that discards data names what it discards.
        foreach (var discarded in new[]
        {
            "MenuItems.HappyHourPrice",
            "MenuItems.QuantityAvailable",
            "MenuItems.Tags",
            "MenuItems.IsPopular",
            "MenuItems.AvailabilityResetUtc",
            "dbo.MenuItemTranslations"
        })
        {
            Assert.Contains(discarded, sql, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SpineMigration_DropsTheOwnerKilledConceptsOnly()
    {
        var sql = ReadSpineMigration();

        // Killed outright: the auto-reset column and per-item translations.
        Assert.Contains("DROP TABLE dbo.MenuItemTranslations", sql, StringComparison.Ordinal);
        Assert.Contains("ALTER TABLE dbo.MenuItems DROP COLUMN AvailabilityResetUtc", sql, StringComparison.Ordinal);

        // Regression: migration 013 indexed that column, and SQL Server refuses to
        // drop a column an index depends on. The index has to go first.
        Assert.Contains("DROP INDEX IX_MenuItems_AvailabilityResetUtc ON dbo.MenuItems", sql, StringComparison.Ordinal);
        Assert.True(
            sql.IndexOf("DROP INDEX IX_MenuItems_AvailabilityResetUtc", StringComparison.Ordinal)
                < sql.IndexOf("DROP COLUMN AvailabilityResetUtc", StringComparison.Ordinal),
            "The dependent index must be dropped before the column.");

        // Deferred while live code still reads them, so master stays releasable.
        Assert.DoesNotContain("DROP COLUMN HappyHourPrice", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("DROP COLUMN QuantityAvailable", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("DROP COLUMN Tags", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("DROP COLUMN IsPopular", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_KeepsFieldLimitsAndForbidsBlankNames()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("Name NVARCHAR(200) NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("Description NVARCHAR(1000) NULL", sql, StringComparison.Ordinal);
        Assert.Contains("CK_Items_Name_NotBlank", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_HoldsOneMenuPerScreen()
    {
        var sql = ReadSpineMigration();

        // A separate assignment record, uniquely keyed by screen, so Schedules
        // can multiplex later without a migration.
        Assert.Contains("CONSTRAINT UQ_MenuScreenAssignments_Screen UNIQUE (ScreenId)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_MakesTheDraftQueueTheCurrentDiff()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("CREATE UNIQUE INDEX UQ_MenuDraftChanges_CurrentDiff", sql, StringComparison.Ordinal);
        Assert.Contains("ON dbo.MenuDraftChanges (MenuId, TargetKind, TargetId, Field)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_SeedsCeilingsAsAllowancesNotConstants()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("INSERT dbo.CapabilityAllowances", sql, StringComparison.Ordinal);
        Assert.Contains("('content.menu.count', 50)", sql, StringComparison.Ordinal);
        Assert.Contains("('content.menu.items', 500)", sql, StringComparison.Ordinal);
        Assert.Contains("('content.menu.import.lines', 2000)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_LandsConfigurableDwellAndLoopWarning()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("DwellSeconds INT NOT NULL CONSTRAINT DF_Menus_DwellSeconds DEFAULT 8", sql, StringComparison.Ordinal);
        Assert.Contains("LoopWarningSeconds INT NOT NULL CONSTRAINT DF_Menus_LoopWarningSeconds DEFAULT 60", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_MarksSeededMenusPublishedSoFixturesWork()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("INSERT dbo.MenuPublishEvents", sql, StringComparison.Ordinal);
        Assert.Contains("WHERE m.IsActive = 1", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void SpineMigration_AddsTheMenusCapabilityIdsAutoGranted()
    {
        var sql = ReadSpineMigration();

        Assert.Contains("'content.menu.manage'", sql, StringComparison.Ordinal);
        Assert.Contains("'content.menu.import'", sql, StringComparison.Ordinal);
        Assert.Contains("'publishing.history.view'", sql, StringComparison.Ordinal);
        Assert.Contains("INSERT dbo.AuthorityRolePermissions", sql, StringComparison.Ordinal);
    }
}

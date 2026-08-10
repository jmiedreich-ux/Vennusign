using Vennu.Data;

namespace Vennu.Api.Tests;

/// <summary>
/// The migration is the authority on what the item library keeps and what it
/// discards, so these assert the script itself rather than a running database.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenusM1MigrationTests
{
    /// <summary>
    /// The content model's own statements, not the whole baseline.
    ///
    /// The fifty-nine migrations were collapsed into one file, but each keeps its
    /// <c>-- ===== name =====</c> header, so a test about one migration can still be
    /// about that migration. It matters: the legacy menu domain in the same file
    /// declares a DECIMAL price, and asserting the content model does not would otherwise read
    /// the wrong statements and fail for the wrong reason.
    /// </summary>
    private static string ReadM1MigrationSection() =>
        ReadBaselineSection("058_create_menu_item_library_spine.sql");

    internal static string ReadBaselineSection(string originalScriptName)
    {
        var sql = ReadBaseline();
        var start = sql.IndexOf($"-- ===== {originalScriptName} =====", StringComparison.Ordinal);
        Assert.True(start >= 0, $"The baseline no longer carries the statements from '{originalScriptName}'.");

        var next = sql.IndexOf("-- ===== ", start + 1, StringComparison.Ordinal);
        return next < 0 ? sql[start..] : sql[start..next];
    }

    internal static string ReadBaseline()
    {
        var scriptName = Assert.Single(
            DatabaseMigrator.GetEmbeddedScriptNames()
                .Where(name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal)));

        using var stream = Assert.IsAssignableFrom<Stream>(
            typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public void M1Migration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void M1Migration_CreatesTheLibraryAndSaveModelTables()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("CREATE TABLE dbo.Items", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.Placements", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.ItemAvailability", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuScreenAssignments", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuPublishEvents", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuPublishTargets", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuHistoryEntries", sql, StringComparison.Ordinal);
    }

    // The draft is derived (owner decision, 2026-08-09): there is deliberately no
    // draft table, because a stored queue could disagree with what Publish ships.
    [Fact]
    public void M1Migration_CreatesNoDraftTable()
    {
        var sql = ReadM1MigrationSection();

        Assert.DoesNotContain("CREATE TABLE dbo.MenuDraftChanges", sql, StringComparison.Ordinal);
        Assert.Contains("NO draft table", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void M1Migration_NamesEveryFieldItDiscardsFromTheLibrary()
    {
        var sql = ReadM1MigrationSection();

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
    public void M1Migration_NeverBuildsTheOwnerKilledConcepts()
    {
        var sql = ReadM1MigrationSection();

        // These used to be created by scripts 012 and 013 and dropped again by 058, so
        // a new database built them and then demolished them. The baseline does not
        // create them at all, which is the same end state and less work - so the claim
        // to prove is absence, not a drop statement. A database that ran the old chain
        // had them and lost them, and arrives in the same place.
        foreach (var neverBuilt in new[]
        {
            "CREATE TABLE dbo.MenuItemTranslations",
            "ADD AvailabilityResetUtc",
            "CREATE INDEX IX_MenuItems_AvailabilityResetUtc"
        })
        {
            Assert.DoesNotContain(neverBuilt, sql, StringComparison.Ordinal);
        }

        // Deferred while live code still reads them, so master stays releasable.
        Assert.DoesNotContain("DROP COLUMN HappyHourPrice", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("DROP COLUMN QuantityAvailable", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("DROP COLUMN Tags", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("DROP COLUMN IsPopular", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void M1Migration_KeepsFieldLimitsAndForbidsBlankNames()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("Name NVARCHAR(200) NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("Description NVARCHAR(1000) NULL", sql, StringComparison.Ordinal);
        Assert.Contains("CK_Items_Name_NotBlank", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void M1Migration_HoldsOneMenuPerScreen()
    {
        var sql = ReadM1MigrationSection();

        // A separate assignment record, uniquely keyed by screen, so Schedules
        // can multiplex later without a migration.
        Assert.Contains("CONSTRAINT UQ_MenuScreenAssignments_Screen UNIQUE (ScreenId)", sql, StringComparison.Ordinal);
    }

    // Review finding #5: the tenant invariant has to cover every relationship,
    // not only direct parents. A placement proves its section is on its own menu;
    // a publish target proves its event and screen share its venue; a history
    // entry proves the event it names belongs to its own menu and venue.
    [Fact]
    public void M1Migration_ClosesTheIndirectTenantRelationships()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("CONSTRAINT FK_Placements_SectionOnMenu FOREIGN KEY (MenuSectionId, MenuId) REFERENCES dbo.MenuSections (Id, MenuId)", sql, StringComparison.Ordinal);
        Assert.Contains("CONSTRAINT FK_MenuPublishTargets_Event FOREIGN KEY (PublishEventId, VenueId) REFERENCES dbo.MenuPublishEvents (Id, VenueId)", sql, StringComparison.Ordinal);
        Assert.Contains("CONSTRAINT FK_MenuPublishTargets_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id)", sql, StringComparison.Ordinal);
        Assert.Contains("CONSTRAINT FK_MenuHistoryEntries_PublishEvent FOREIGN KEY (PublishEventId, MenuId, VenueId) REFERENCES dbo.MenuPublishEvents (Id, MenuId, VenueId)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void M1Migration_SeedsCeilingsAsAllowancesNotConstants()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("INSERT dbo.CapabilityAllowances", sql, StringComparison.Ordinal);
        Assert.Contains("('content.menu.count', 50)", sql, StringComparison.Ordinal);
        Assert.Contains("('content.menu.items', 500)", sql, StringComparison.Ordinal);
        Assert.Contains("('content.menu.import.lines', 2000)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void M1Migration_LandsConfigurableDwellAndLoopWarning()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("DwellSeconds INT NOT NULL CONSTRAINT DF_Menus_DwellSeconds DEFAULT 8", sql, StringComparison.Ordinal);
        Assert.Contains("LoopWarningSeconds INT NOT NULL CONSTRAINT DF_Menus_LoopWarningSeconds DEFAULT 60", sql, StringComparison.Ordinal);
    }

    // Q45: fresh start. The new tables begin empty and the migration must not
    // carry legacy menu content across; the old tables stay untouched but unused.
    [Fact]
    public void M1Migration_DoesNotCarryLegacyContentIntoTheLibrary()
    {
        var sql = ReadM1MigrationSection();

        Assert.DoesNotContain("INSERT dbo.Items", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("INSERT dbo.Placements", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("INSERT dbo.ItemAvailability", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("FROM dbo.MenuItems mi", sql, StringComparison.Ordinal);
        Assert.Contains("FRESH START", sql, StringComparison.Ordinal);
    }

    // Tenancy is a database invariant, not a predicate each query must remember:
    // every child reaches its parent through the parent's (Id, VenueId) key.
    [Fact]
    public void M1Migration_MakesTenantOwnershipADatabaseInvariant()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("REFERENCES dbo.Menus (Id, VenueId)", sql, StringComparison.Ordinal);
        Assert.Contains("REFERENCES dbo.MenuSections (Id, VenueId)", sql, StringComparison.Ordinal);
        Assert.Contains("REFERENCES dbo.Items (Id, VenueId)", sql, StringComparison.Ordinal);
        Assert.Contains("REFERENCES dbo.Screens (VenueId, Id)", sql, StringComparison.Ordinal);
        Assert.Contains("CONSTRAINT UQ_Items_Id_VenueId UNIQUE (Id, VenueId)", sql, StringComparison.Ordinal);
    }

    // Q115/Q190: a price is stored exactly as typed, so "MP" is a valid price and
    // "9.5" never becomes "9.50". A numeric column could do neither.
    [Fact]
    public void M1Migration_StoresPricesAsTypedText()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("Price NVARCHAR(40) NULL", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("Price DECIMAL", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void M1Migration_AddsTheMenusCapabilityIdsAutoGranted()
    {
        var sql = ReadM1MigrationSection();

        Assert.Contains("'content.menu.manage'", sql, StringComparison.Ordinal);
        Assert.Contains("'content.menu.import'", sql, StringComparison.Ordinal);
        Assert.Contains("'publishing.history.view'", sql, StringComparison.Ordinal);
        Assert.Contains("INSERT dbo.AuthorityRolePermissions", sql, StringComparison.Ordinal);
    }
}

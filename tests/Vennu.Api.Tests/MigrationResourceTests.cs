using Vennu.Data;

namespace Vennu.Api.Tests;

[Trait("Category", "Unit")]
public sealed class MigrationResourceTests
{
    [Fact]
    public void MenuImportReplaceMigration_IsEmbeddedOrderedAndKeepsHistoricalSnapshotsIndependentOfSessions()
    {
        var scripts=DatabaseMigrator.GetEmbeddedScriptNames();var scriptName=Assert.Single(scripts,name=>name.EndsWith(".Scripts.070_menu_import_replace.sql",StringComparison.Ordinal));using var stream=Assert.IsAssignableFrom<Stream>(typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(scriptName));using var reader=new StreamReader(stream);var sql=reader.ReadToEnd();
        Assert.Contains("CREATE TABLE dbo.MenuImportReplacementSnapshots",sql,StringComparison.Ordinal);Assert.Contains("snapshot_retention_days",sql,StringComparison.Ordinal);Assert.Contains("restore_enabled",sql,StringComparison.Ordinal);Assert.Contains("CompletedSnapshotId",sql,StringComparison.Ordinal);Assert.DoesNotContain("FK_MenuImportReplacementSnapshots_Session",sql,StringComparison.Ordinal);Assert.Equal(scripts.OrderBy(name=>name,StringComparer.OrdinalIgnoreCase),scripts);
    }
    [Fact]
    public void MenuImportCreateMigration_IsEmbeddedOrderedAndAddsAtomicCompletionShape()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        var scriptName = Assert.Single(scripts, name => name.EndsWith(".Scripts.069_menu_import_create.sql", StringComparison.Ordinal));
        using var stream = Assert.IsAssignableFrom<Stream>(typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();
        Assert.Contains("WHAT THIS DISCARDS: nothing", sql, StringComparison.Ordinal);
        Assert.Contains("CompletedMenuId", sql, StringComparison.Ordinal);
        Assert.Contains("ImportedPriceOverride", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuImportCreatedLines", sql, StringComparison.Ordinal);
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void MenuImportSessionMigration_IsEmbeddedOrderedAndDoesNotMutateMenus()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        var scriptName = Assert.Single(scripts.Where(name => name.EndsWith(".Scripts.068_menu_import_sessions.sql", StringComparison.Ordinal)));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);

        using var stream = Assert.IsAssignableFrom<Stream>(typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        Assert.Contains("WHAT THIS DISCARDS: nothing", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuImportSessions", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuImportSourceLines", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuImportReviewQuestions", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuImportCandidates", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.MenuImportAnswers", sql, StringComparison.Ordinal);
        Assert.Contains("content.menu.import.session_retention_minutes", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("ALTER TABLE dbo.Menus", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ALTER TABLE dbo.Items", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ItemPriceMigration_IsEmbeddedNarrowsWithoutSilentDataLossAndKeepsSnapshotReadsWide()
    {
        var assembly = typeof(DatabaseMigrator).Assembly;
        var scriptName = Assert.Single(DatabaseMigrator.GetEmbeddedScriptNames()
            .Where(name => name.EndsWith(".Scripts.067_item_price_and_canonical_name.sql", StringComparison.Ordinal)));
        using var stream = Assert.IsAssignableFrom<Stream>(assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();
        Assert.Contains("LEN(Price) > 12", sql, StringComparison.Ordinal);
        Assert.Contains("THROW 51067", sql, StringComparison.Ordinal);
        Assert.Contains("ALTER COLUMN Price NVARCHAR(12) NULL", sql, StringComparison.Ordinal);
        Assert.Contains("WHAT THIS DISCARDS: nothing", sql, StringComparison.Ordinal);
    }
    [Fact]
    public void ScopedAuthorityMigration_IsEmbeddedInOrderAndSeedsProtectedContracts()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        var scriptName = Assert.Single(
            scripts.Where(name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal)));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);

        var assembly = typeof(DatabaseMigrator).Assembly;
        using var stream = Assert.IsAssignableFrom<Stream>(assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        var script = reader.ReadToEnd();

        Assert.Contains("CREATE TABLE dbo.AuthorityPermissions", script, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.ScopedRoleAssignments", script, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.SupportAccessGrants", script, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE dbo.SupportAccessAuditEntries", script, StringComparison.Ordinal);
        Assert.Contains("DATEDIFF(MINUTE, StartsUtc, ExpiresUtc) <= 480", script, StringComparison.Ordinal);
        Assert.Contains("'support_operator', 'roles.support_operator.name', 1, 1", script, StringComparison.Ordinal);
    }

    [Fact]
    public void AdministrativeIdentityMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void AdministrativeIdentityMigration_RejectsCompositeCanonicalCollision()
    {
        var assembly = typeof(DatabaseMigrator).Assembly;
        var scriptName = Assert.Single(
            DatabaseMigrator.GetEmbeddedScriptNames()
                .Where(name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal)));

        using var stream = Assert.IsAssignableFrom<Stream>(assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        var script = reader.ReadToEnd();

        Assert.Contains("canonical.Id <> legacy.Id", script, StringComparison.Ordinal);
        Assert.Contains("ELSE legacy.ApplicationScope", script, StringComparison.Ordinal);
        Assert.Contains("ELSE legacy.[Key]", script, StringComparison.Ordinal);
        Assert.Contains("Administrative identity migration found a canonical duplicate.", script, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomerOnboardingMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void OrganizationSubscriptionsMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void TierTrialEntitlementsMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void CustomerStrongAuthenticationMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void CustomerAuthenticationMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void CustomerIdentityTenancyMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void FeatureMatrixMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void MenuDomainMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void QuickUpdateMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void PhotoGridDensityMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void ScreenDisplayLayoutMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void VenueThemeMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void AdvancedVenueThemeMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void SplitLayoutMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void DailySpecialHeroMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void HeroDwellMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void MealPeriodMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void MealPeriodTargetsMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void HappyHourScheduleMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void DateRangePromotionMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void TapDomainMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void ClassicChalkboardMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void TapStripsMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void DigitalTapBoardMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void ScreenPreRegistrationMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void SubscriptionPeriodEndStateMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void HaasContractMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void PosConnectionMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void PosCatalogMappingMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void PosWebhookEventMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void PosSyncHealthMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void PosRefreshTokenExpirationMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void ScreenReplacementAuditMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void ScreenContentDeliveryMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();
        Assert.Contains(scripts, name => name.EndsWith(".Scripts.001_baseline.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }
}

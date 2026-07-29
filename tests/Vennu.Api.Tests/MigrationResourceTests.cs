using Vennu.Data;

namespace Vennu.Api.Tests;

[Trait("Category", "Unit")]
public sealed class MigrationResourceTests
{
    [Fact]
    public void FeatureMatrixMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.009_create_feature_matrix_audit.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }

    [Fact]
    public void MenuDomainMigration_IsEmbeddedInOrder()
    {
        var scripts = DatabaseMigrator.GetEmbeddedScriptNames();

        Assert.Contains(scripts, name => name.EndsWith(".Scripts.012_create_menu_domain.sql", StringComparison.Ordinal));
        Assert.Equal(scripts.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), scripts);
    }
}

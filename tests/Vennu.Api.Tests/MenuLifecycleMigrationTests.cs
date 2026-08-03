using Vennu.Data;

namespace Vennu.Api.Tests;

[Trait("Category", "Unit")]
public sealed class MenuLifecycleMigrationTests
{
    [Fact]
    public void Migration053_AddsRecoverableMenuItemLifecycle()
    {
        var scriptName = Assert.Single(
            DatabaseMigrator.GetEmbeddedScriptNames()
                .Where(name => name.EndsWith(".Scripts.053_add_menu_item_lifecycle.sql", StringComparison.Ordinal)));
        using var stream = Assert.IsAssignableFrom<Stream>(typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(scriptName));
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        Assert.Contains("COL_LENGTH('dbo.MenuItems', 'IsActive')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IsActive BIT NOT NULL", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DEFAULT 1 WITH VALUES", sql, StringComparison.OrdinalIgnoreCase);
    }
}

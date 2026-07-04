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
                "Vennu.Data.Scripts.003_create_screen_pairing_codes.sql"
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

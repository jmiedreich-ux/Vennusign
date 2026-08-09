namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Unit")]
public class DatabaseMigratorTests
{
    // Every .sql file under Vennu.Data/Scripts must actually be embedded, in
    // order. Comparing against the files on disk rather than a hand-listed array
    // is what makes this catch the real mistake - a migration added to the folder
    // but never embedded, which a hard-coded list silently tolerates until it is
    // updated by hand.
    [Fact]
    public void GetEmbeddedScriptNames_MatchesEveryScriptOnDiskInOrder()
    {
        var scriptNames = DatabaseMigrator.GetEmbeddedScriptNames();

        var expected = Directory
            .EnumerateFiles(FindScriptsDirectory(), "*.sql")
            .Select(path => $"Vennu.Data.Scripts.{Path.GetFileName(path)}")
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.NotEmpty(expected);
        Assert.Equal(expected, scriptNames);
    }

    private static string FindScriptsDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var scripts = Path.Combine(directory.FullName, "src", "Vennu.Data", "Scripts");
            if (Directory.Exists(scripts))
            {
                return scripts;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/Vennu.Data/Scripts from the test output directory.");
    }

    [Fact]
    public void GetEmbeddedScriptNames_ReturnsEmptyForAssemblyWithoutMigrationScripts()
    {
        var scriptNames = DatabaseMigrator.GetEmbeddedScriptNames(typeof(DatabaseMigratorTests).Assembly);

        Assert.Empty(scriptNames);
    }

    [Fact]
    public void RotationMetadataMigration_DefersNewColumnReferenceUntilExecution()
    {
        const string resourceName = "Vennu.Data.Scripts.050_add_configuration_rotation_metadata.sql";
        using var stream = typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(resourceName);
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream);
        var script = reader.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal);

        Assert.Contains("RotationReminderDays INT NULL;\nEXEC sys.sp_executesql N'ALTER TABLE", script, StringComparison.Ordinal);
        Assert.DoesNotContain("\nALTER TABLE dbo.SystemConfigurationDefinitions ADD CONSTRAINT", script, StringComparison.Ordinal);
        Assert.Contains("EXEC sys.sp_executesql N'UPDATE dbo.SystemConfigurationDefinitions", script, StringComparison.Ordinal);
        Assert.DoesNotContain("\nUPDATE dbo.SystemConfigurationDefinitions SET RotationReminderDays", script, StringComparison.Ordinal);
    }
}

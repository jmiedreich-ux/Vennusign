using Microsoft.Data.SqlClient;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// Migrating a database concurrently is the normal case, not an edge one: the API
/// calls the migrator from two places at startup, and every test host that boots
/// the app calls it again — in parallel, because the test runner is parallel.
/// </summary>
[Trait("Category", "Integration")]
public class DatabaseMigratorConcurrencyTests
{
    /// <summary>
    /// Found while landing migration 059: the product database ended up with seven
    /// journal rows for one script. DbUp reads the journal, decides what to run, and
    /// writes the journal afterwards; nothing between those steps stops a second
    /// caller reading the same "not applied yet" answer. Every one of them then runs
    /// the script. 059 survived it because its statements happen to be repeatable —
    /// a CREATE TABLE would have thrown, and startup would have failed.
    ///
    /// This is the same read-then-write shape the baseline recorder was fixed for,
    /// and the fix is the same: one named application lock around the whole decision.
    /// </summary>
    [Fact]
    public async Task EightConcurrentMigrations_ApplyEveryScriptExactlyOnce()
    {
        var database = $"vennusign_dev_migrate_{Guid.NewGuid():N}"[..40];
        var master = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = "master" }.ConnectionString;
        var target = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = database }.ConnectionString;

        try
        {
            await ExecuteAsync(master, $"CREATE DATABASE [{database}];");

            // All eight start together, the way the real callers do.
            using var gate = new SemaphoreSlim(0, 8);
            var runs = Enumerable.Range(0, 8)
                .Select(_ => Task.Run(async () =>
                {
                    await gate.WaitAsync();
                    DatabaseMigrator.Run(target);
                }))
                .ToArray();

            gate.Release(8);
            await Task.WhenAll(runs);

            var duplicated = await ScalarAsync(
                target,
                """
                SELECT COUNT(*) FROM (
                    SELECT ScriptName FROM dbo.SchemaVersions GROUP BY ScriptName HAVING COUNT(*) > 1
                ) AS d;
                """);

            Assert.True(
                duplicated == 0,
                $"{duplicated} script(s) were journaled more than once: the migration decision is not serialised.");
        }
        finally
        {
            await ExecuteAsync(
                master,
                $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END;");
        }
    }

    private const string LocalDb =
        @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }
}

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
        const string resourceName = "Vennu.Data.Scripts.001_baseline.sql";
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

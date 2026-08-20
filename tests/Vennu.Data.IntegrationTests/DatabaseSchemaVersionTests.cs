using Microsoft.Data.SqlClient;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// What /health/version reports about the schema, against a real database.
///
/// Written as an integration test rather than a unit test on purpose. The value
/// being replaced was a deploy-supplied environment variable, and a unit test
/// seeded with the answer would prove exactly as much as that did - which is the
/// failure this whole change exists to stop repeating (#739, #740).
/// </summary>
[Trait("Category", "Integration")]
public class DatabaseSchemaVersionTests
{
    private const string LocalDb =
        @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

    [Fact]
    public async Task ReportsTheNewestScriptTheDatabaseHasActuallyApplied()
    {
        var database = $"vennusign_schemaver_{Guid.NewGuid():N}"[..40];
        var master = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = "master" }.ConnectionString;
        var target = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = database }.ConnectionString;

        try
        {
            await ExecuteAsync(master, $"CREATE DATABASE [{database}];");
            DatabaseMigrator.Run(target);

            // The answer is not hard-coded: it is whatever the migrator just applied,
            // so adding script 074 tomorrow does not make this test wrong.
            var newest = await ScalarStringAsync(target,
                "SELECT TOP 1 ScriptName FROM dbo.SchemaVersions ORDER BY ScriptName DESC;");
            var expected = DatabaseSchemaVersion.Describe(newest);

            Assert.Equal(expected, new DatabaseSchemaVersion(target).Current());
            Assert.NotEqual(DatabaseSchemaVersion.Unavailable, expected);

            // It reports the schema level, not the most recently written journal row.
            // A re-applied earlier script must not drag the reported level backwards.
            await ExecuteAsync(target,
                "INSERT INTO dbo.SchemaVersions (ScriptName, Applied) VALUES ('Vennu.Data.Scripts.001_baseline.sql', SYSUTCDATETIME());");
            Assert.Equal(expected, new DatabaseSchemaVersion(target).Current());
        }
        finally
        {
            await ExecuteAsync(master,
                $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END;");
        }
    }

    [Fact]
    public async Task AnUnmigratedDatabaseIsReportedAsUnavailableRatherThanFailing()
    {
        // The journal table does not exist yet. /health/version still has to answer:
        // it is the endpoint someone reaches for when the API is misbehaving.
        var database = $"vennusign_nojournal_{Guid.NewGuid():N}"[..40];
        var master = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = "master" }.ConnectionString;
        var target = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = database }.ConnectionString;

        try
        {
            await ExecuteAsync(master, $"CREATE DATABASE [{database}];");
            Assert.Equal(DatabaseSchemaVersion.Unavailable, new DatabaseSchemaVersion(target).Current());
        }
        finally
        {
            await ExecuteAsync(master,
                $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END;");
        }
    }

    [Fact]
    public void AnUnreachableDatabaseIsReportedRatherThanThrown()
    {
        var unreachable = new SqlConnectionStringBuilder(LocalDb)
        {
            InitialCatalog = "vennusign_no_such_database_anywhere",
            ConnectTimeout = 5
        }.ConnectionString;

        Assert.Equal(DatabaseSchemaVersion.Unavailable, new DatabaseSchemaVersion(unreachable).Current());
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<string?> ScalarStringAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return await command.ExecuteScalarAsync() as string;
    }
}

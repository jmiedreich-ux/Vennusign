namespace Vennu.Data.IntegrationTests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string ConnectionStringVariable = "VENU_TEST_AZURE_SQL_CONNECTION_STRING";

    public string? ConnectionString { get; private set; }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(ConnectionString);

    public Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (IsAvailable)
        {
            DatabaseMigrator.Run(ConnectionString!);
        }

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public SqlDataAccess CreateDataAccess()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException($"Azure SQL connection string not configured. Set the {ConnectionStringVariable} environment variable.");
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionString"] = ConnectionString })
            .Build();

        return new SqlDataAccess(configuration);
    }

    public async Task ResetTablesAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        const string cleanupSql = """
            DELETE FROM dbo.ScreenPairingCodes;
            DELETE FROM dbo.Screens;
            DELETE FROM dbo.Venues;
            """;

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(cleanupSql, connection);
        _ = await command.ExecuteNonQueryAsync();
    }
}

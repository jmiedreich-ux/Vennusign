namespace Vennu.Data.IntegrationTests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string SettingsFileName = "app.settings.json";
    private const string ConnectionStringVariable = "VENU_TEST_AZURE_SQL_CONNECTION_STRING";
    private const string ResetTablesVariable = "VENU_TEST_RESET_TABLES";

    private readonly Dictionary<string, string?> settings = [];

    public string? ConnectionString { get; private set; }

    public string TestRunId { get; } = Guid.NewGuid().ToString("N")[..8];

    public bool IsAvailable => !string.IsNullOrWhiteSpace(ConnectionString);

    public bool ResetTablesBeforeEachTest => IsEnabled(GetSetting(ResetTablesVariable));

    public Task InitializeAsync()
    {
        LoadSettings();
        ConnectionString = GetSetting(ConnectionStringVariable);

        if (IsAvailable)
        {
            EnsureDevDatabase(ConnectionString!);
            DatabaseMigrator.Run(ConnectionString!);
            return EnsureTestRecordTraceTableAsync();
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

    public string UniqueValue(string prefix) => $"{prefix}-{TestRunId}-{Guid.NewGuid():N}";

    public string UniqueScreenKey(string prefix = "SC")
    {
        var normalizedPrefix = new string(prefix.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedPrefix))
        {
            normalizedPrefix = "SC";
        }

        if (normalizedPrefix.Length > 2)
        {
            normalizedPrefix = normalizedPrefix[..2];
        }

        return $"{normalizedPrefix}{Guid.NewGuid():N}"[..9].ToUpperInvariant();
    }

    public string UniqueCode()
    {
        return Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
    }

    public async Task ResetTablesAsync()
    {
        if (!IsAvailable || !ResetTablesBeforeEachTest)
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

    public async Task TraceAsync(string testName, string intent, string tableName, string recordKey, string action, object? payload = null)
    {
        if (!IsAvailable)
        {
            return;
        }

        const string insertSql = """
            INSERT INTO dbo.TestRecordTrace (TestRunId, TestName, Intent, TableName, RecordKey, Action, Payload)
            VALUES (@TestRunId, @TestName, @Intent, @TableName, @RecordKey, @Action, @Payload);
            """;

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(insertSql, connection);
        _ = command.Parameters.AddWithValue("@TestRunId", TestRunId);
        _ = command.Parameters.AddWithValue("@TestName", testName);
        _ = command.Parameters.AddWithValue("@Intent", intent);
        _ = command.Parameters.AddWithValue("@TableName", tableName);
        _ = command.Parameters.AddWithValue("@RecordKey", recordKey);
        _ = command.Parameters.AddWithValue("@Action", action);
        _ = command.Parameters.AddWithValue("@Payload", payload is null ? DBNull.Value : System.Text.Json.JsonSerializer.Serialize(payload));
        _ = await command.ExecuteNonQueryAsync();
    }

    private async Task EnsureTestRecordTraceTableAsync()
    {
        const string createTraceTableSql = """
            IF OBJECT_ID(N'dbo.TestRecordTrace', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TestRecordTrace
                (
                    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestRecordTrace PRIMARY KEY,
                    TestRunId NVARCHAR(32) NOT NULL,
                    TestName NVARCHAR(256) NOT NULL,
                    Intent NVARCHAR(1000) NOT NULL,
                    TableName NVARCHAR(128) NOT NULL,
                    RecordKey NVARCHAR(256) NOT NULL,
                    Action NVARCHAR(50) NOT NULL,
                    Payload NVARCHAR(MAX) NULL,
                    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TestRecordTrace_CreatedUtc DEFAULT SYSUTCDATETIME()
                );

                CREATE INDEX IX_TestRecordTrace_TestRunId ON dbo.TestRecordTrace (TestRunId, CreatedUtc);
                CREATE INDEX IX_TestRecordTrace_TableName_RecordKey ON dbo.TestRecordTrace (TableName, RecordKey);
            END;
            """;

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(createTraceTableSql, connection);
        _ = await command.ExecuteNonQueryAsync();
    }

    private string? GetSetting(string key)
    {
        var environmentValue = Environment.GetEnvironmentVariable(key);

        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        return settings.GetValueOrDefault(key);
    }

    private void LoadSettings()
    {
        var settingsPath = FindSettingsFile();

        if (settingsPath is null)
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsPath, optional: true)
            .Build();

        foreach (var setting in configuration.AsEnumerable())
        {
            settings[setting.Key] = setting.Value;
        }
    }

    private static string? FindSettingsFile()
    {
        foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startPath);

            while (directory is not null)
            {
                var settingsPath = Path.Combine(directory.FullName, SettingsFileName);

                if (File.Exists(settingsPath))
                {
                    return settingsPath;
                }

                var testProjectSettingsPath = Path.Combine(directory.FullName, "tests", "Vennu.Data.IntegrationTests", SettingsFileName);

                if (File.Exists(testProjectSettingsPath))
                {
                    return testProjectSettingsPath;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static void EnsureDevDatabase(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName) || !databaseName.Contains("dev", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Integration tests can only run against a dev database. The connection string database name must contain 'dev'.");
        }
    }

    private static bool IsEnabled(string? value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }
}

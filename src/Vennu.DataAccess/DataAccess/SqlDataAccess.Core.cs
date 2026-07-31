using System.Collections;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RepoDb;
using Serilog;
using Serilog.Events;
using Vennu.Core.Models;

namespace Vennu.DataAccess;

public partial class SqlDataAccess : ISqlDataAccess
{
    private const int DefaultCommandTimeoutSeconds = 180;
    private const int DynamicQueryTimeoutSeconds = 60;
    private const string LegacyConnectionStringName = "ConnectionString";
    private const string PreferredConnectionStringName = "VennuDatabase";
    private static readonly object TableMappingLock = new();
    private static bool tableMappingsConfigured;

    private readonly IConfiguration configuration;
    private readonly ILogger logger;

    public SqlDataAccess(IConfiguration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        logger = Log.ForContext<SqlDataAccess>();

        GlobalConfiguration.Setup().UseSqlServer();
        
        // Configure RepoDb table mappings for plural table names
        ConfigureTableMappings();
    }

    private static void ConfigureTableMappings()
    {
        lock (TableMappingLock)
        {
            if (tableMappingsConfigured)
            {
                return;
            }

            // Map singular entity names to plural table names in the database
            FluentMapper
                .Entity<Venue>()
                .Table("dbo.Venues", true);

            FluentMapper
                .Entity<Screen>()
                .Table("dbo.Screens", true);

            FluentMapper
                .Entity<ScreenPairingCode>()
                .Table("dbo.ScreenPairingCodes", true);

            FluentMapper
                .Entity<Feature>()
                .Table("dbo.Features", true);

            FluentMapper
                .Entity<SubscriptionTier>()
                .Table("dbo.SubscriptionTiers", true);

            FluentMapper
                .Entity<TierFeature>()
                .Table("dbo.TierFeatures", true);

            FluentMapper
                .Entity<VenueSubscription>()
                .Table("dbo.VenueSubscriptions", true);

            FluentMapper
                .Entity<VenueFeatureOverride>()
                .Table("dbo.VenueFeatureOverrides", true);

            FluentMapper
                .Entity<FeatureUsage>()
                .Table("dbo.FeatureUsages", true);

            FluentMapper
                .Entity<ProcessedStripeEvent>()
                .Table("dbo.ProcessedStripeEvents", true);

            FluentMapper
                .Entity<FeatureMatrixAuditEntry>()
                .Table("dbo.FeatureMatrixAudit", true);

            tableMappingsConfigured = true;
        }
    }

    protected TResult Execute<TResult>(string operationName, Func<SqlConnection, TResult> operation, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        LogParameters(operationName, parameters);

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var result = operation(connection);
            LogSuccess(operationName, result);
            return result;
        }
        catch (Exception ex)
        {
            LogFailure(operationName, ex);
            throw;
        }
    }

    protected async Task<TResult> ExecuteAsync<TResult>(
        string operationName,
        Func<SqlConnection, CancellationToken, Task<TResult>> operation,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        LogParameters(operationName, parameters);

        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var result = await operation(connection, cancellationToken).ConfigureAwait(false);
            LogSuccess(operationName, result);
            return result;
        }
        catch (Exception ex)
        {
            LogFailure(operationName, ex);
            throw;
        }
    }

    protected SqlConnection CreateConnection() => new(GetConnectionString());

    private string GetConnectionString()
    {
        var configuredConnectionString = configuration.GetConnectionString(PreferredConnectionStringName)
            ?? configuration[PreferredConnectionStringName]
            ?? configuration.GetConnectionString(LegacyConnectionStringName)
            ?? configuration[LegacyConnectionStringName];

        if (string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            throw new InvalidOperationException($"A connection string named '{PreferredConnectionStringName}' or '{LegacyConnectionStringName}' was not found.");
        }

        return configuredConnectionString;
    }

    private void LogParameters(string operationName, object? parameters)
    {
        if (parameters is null || !logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }

        logger.ForContext("Method", operationName)
            .Debug("Database operation parameters: {@Parameters}", parameters);
    }

    private void LogSuccess(string operationName, object? result)
    {
        var operationLogger = logger.ForContext("Method", operationName);

        if (TryGetResultCount(result, out var count))
        {
            operationLogger.Verbose("Database operation completed. Count: {Count}", count);
            return;
        }

        operationLogger.Verbose("Database operation completed successfully.");
    }

    private void LogFailure(string operationName, Exception exception)
    {
        logger.ForContext("Method", operationName)
            .Error(exception, "Database operation failed.");
    }

    private static bool TryGetResultCount(object? result, out int count)
    {
        switch (result)
        {
            case null:
                count = 0;
                return false;
            case int affectedRows:
                count = affectedRows;
                return true;
            case ICollection collection:
                count = collection.Count;
                return true;
            default:
                count = 0;
                return false;
        }
    }

    public List<Field>? GetFieldList(string? delimitedList)
    {
        if (string.IsNullOrWhiteSpace(delimitedList))
        {
            return null;
        }

        return delimitedList
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static fieldName => new Field(fieldName))
            .ToList();
    }
}

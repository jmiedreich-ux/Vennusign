using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Configuration;

public sealed class VennuDatabaseConfigurationProvider : ConfigurationProvider, IDisposable
{
    private static readonly HashSet<string> Environments = ["Development", "Test", "Staging", "Production"];
    private static readonly HashSet<string> Scopes = ["API", "Admin", "VenueAdmin", "Display", "Background"];
    private readonly VennuDatabaseConfigurationOptions options;
    private Timer? reloadTimer;
    private int loading;

    public VennuDatabaseConfigurationProvider(VennuDatabaseConfigurationOptions options)
    {
        this.options = options;
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString);
        if (!Environments.Contains(options.EnvironmentName)) throw new ArgumentException("Unsupported configuration environment.", nameof(options));
        if (!Scopes.Contains(options.ApplicationScope)) throw new ArgumentException("Unsupported configuration application scope.", nameof(options));
        if (options.ReloadInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options));
    }

    public override void Load()
    {
        Data = LoadValues();
        reloadTimer ??= new Timer(_ => Reload(), null, options.ReloadInterval, options.ReloadInterval);
    }

    public void Dispose() => reloadTimer?.Dispose();

    private void Reload()
    {
        if (Interlocked.Exchange(ref loading, 1) != 0) return;
        try
        {
            var values = LoadValues();
            if (!Data.OrderBy(pair => pair.Key).SequenceEqual(values.OrderBy(pair => pair.Key)))
            {
                Data = values;
                OnReload();
            }
        }
        catch
        {
            // Retain the last successfully loaded in-memory snapshot during transient reload failures.
        }
        finally
        {
            Volatile.Write(ref loading, 0);
        }
    }

    private Dictionary<string, string?> LoadValues()
    {
        const string sql = """
            WITH candidates AS
            (
                SELECT d.[Key], d.IsSecret, d.DefaultValue, v.ValuePayload, v.IsEncrypted,
                    ROW_NUMBER() OVER (PARTITION BY d.[Key] ORDER BY CASE WHEN d.ApplicationScope = @ApplicationScope THEN 0 ELSE 1 END) AS precedence
                FROM dbo.SystemConfigurationDefinitions d
                LEFT JOIN dbo.SystemConfigurationValues v ON v.DefinitionId = d.Id AND v.EnvironmentName = @EnvironmentName
                WHERE d.ApplicationScope IN (N'Shared', @ApplicationScope)
            )
            SELECT [Key], IsSecret, DefaultValue, ValuePayload, IsEncrypted
            FROM candidates WHERE precedence = 1;
            """;

        using var connection = new SqlConnection(options.ConnectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ApplicationScope", options.ApplicationScope);
        command.Parameters.AddWithValue("@EnvironmentName", options.EnvironmentName);
        using var reader = command.ExecuteReader();
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
        {
            var key = reader.GetString(0);
            var isSecret = reader.GetBoolean(1);
            var value = reader.IsDBNull(3) ? (reader.IsDBNull(2) ? null : reader.GetString(2)) : reader.GetString(3);
            if (value is null) continue;
            var isEncrypted = !reader.IsDBNull(4) && reader.GetBoolean(4);
            if (isSecret)
            {
                if (!isEncrypted) throw new InvalidOperationException($"Secret configuration '{key}' is not encrypted.");
                value = (options.SecretProtector ?? throw new InvalidOperationException("A configuration secret protector is required to load encrypted settings.")).Unprotect(value);
            }
            values[key] = value;
        }
        return values;
    }
}

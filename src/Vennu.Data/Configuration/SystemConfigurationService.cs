using System.Globalization;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Configuration;

public sealed class SystemConfigurationService(
    IConfiguration configuration,
    IConfigurationSecretProtector? secretProtector = null) : ISystemConfigurationService
{
    private static readonly HashSet<string> Environments = ["Development", "Test", "Staging", "Production"];
    private static readonly HashSet<string> Scopes = ["Shared", "API", "Admin", "VenueAdmin", "Display", "Background"];

    public async Task<IReadOnlyList<SystemConfigurationSetting>> GetAsync(
        string environmentName,
        string? applicationScope,
        CancellationToken cancellationToken = default)
    {
        ValidateEnvironment(environmentName);
        if (applicationScope is not null && !Scopes.Contains(applicationScope)) throw new ArgumentException("Unsupported application scope.", nameof(applicationScope));
        const string sql = """
            SELECT d.Id, d.[Key], d.ApplicationScope, d.[Description], d.ValueType, d.IsRequired, d.IsSecret,
                   d.DefaultValue, d.RequiresRestart, d.ExportPolicy, v.ValuePayload, v.IsDeleted, v.Version, v.UpdatedUtc, d.RotationReminderDays
            FROM dbo.SystemConfigurationDefinitions d
            LEFT JOIN dbo.SystemConfigurationValues v ON v.DefinitionId = d.Id AND v.EnvironmentName = @EnvironmentName
            WHERE @ApplicationScope IS NULL OR d.ApplicationScope = @ApplicationScope
            ORDER BY d.ApplicationScope, d.[Key];
            """;
        await using var connection = new SqlConnection(ConnectionString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@EnvironmentName", environmentName);
        command.Parameters.AddWithValue("@ApplicationScope", (object?)applicationScope ?? DBNull.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var settings = new List<SystemConfigurationSetting>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) settings.Add(ReadSetting(reader));
        return settings;
    }

    public Task<SystemConfigurationSetting> SetAsync(SystemConfigurationWrite write, CancellationToken cancellationToken = default) =>
        WriteAsync(write, false, cancellationToken);

    public Task<SystemConfigurationSetting> ClearAsync(SystemConfigurationWrite write, CancellationToken cancellationToken = default) =>
        WriteAsync(write with { Value = null }, true, cancellationToken);

    private async Task<SystemConfigurationSetting> WriteAsync(SystemConfigurationWrite write, bool clear, CancellationToken cancellationToken)
    {
        ValidateEnvironment(write.EnvironmentName);
        if (string.IsNullOrWhiteSpace(write.Actor)) throw new ArgumentException("An actor is required.", nameof(write));
        await using var connection = new SqlConnection(ConnectionString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var definition = await LoadDefinitionAsync(connection, transaction, write.DefinitionId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The registered configuration definition was not found.");
        if (clear && definition.IsRequired) throw new ArgumentException("Required settings cannot be cleared.", nameof(write));
        if (!clear) ValidateValue(definition, write.Value);
        var existing = await LoadExistingAsync(connection, transaction, write.DefinitionId, write.EnvironmentName, cancellationToken).ConfigureAwait(false);
        ValidateVersion(existing?.Version, write.ExpectedVersion);

        var storedValue = clear ? null : definition.IsSecret
            ? (secretProtector ?? throw new InvalidOperationException("Secret protection is not configured.")).Protect(write.Value!)
            : write.Value;
        var encrypted = definition.IsSecret && !clear;
        var valueId = existing?.Id ?? Guid.NewGuid();
        var previousFingerprint = existing?.ValuePayload is null ? null : Fingerprint(existing.ValuePayload);
        var newFingerprint = storedValue is null ? null : Fingerprint(storedValue);

        await UpsertAsync(connection, transaction, valueId, write, storedValue, encrypted, clear, existing is not null, cancellationToken).ConfigureAwait(false);
        await InsertRevisionAsync(connection, transaction, valueId, storedValue, newFingerprint, encrypted, write, cancellationToken).ConfigureAwait(false);
        await InsertAuditAsync(connection, transaction, definition, write, clear ? "Clear" : existing is null ? "Create" : "Update", previousFingerprint, newFingerprint, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        var settings = await GetAsync(write.EnvironmentName, definition.ApplicationScope, cancellationToken).ConfigureAwait(false);
        return settings.Single(setting => setting.DefinitionId == write.DefinitionId);
    }

    private string ConnectionString() => configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

    private static void ValidateEnvironment(string environmentName)
    {
        if (!Environments.Contains(environmentName)) throw new ArgumentException("Unsupported configuration environment.", nameof(environmentName));
    }

    private static void ValidateValue(Definition definition, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (definition.IsRequired) throw new ArgumentException("A value is required.", nameof(value));
            return;
        }
        var valid = definition.ValueType switch
        {
            "Boolean" => bool.TryParse(value, out _),
            "Integer" => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "Decimal" => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
            "Uri" => Uri.TryCreate(value, UriKind.Absolute, out _),
            "Json" => IsJson(value),
            _ => true
        };
        if (!valid) throw new ArgumentException($"The value is not a valid {definition.ValueType}.", nameof(value));
        if (definition.ValidationPattern is not null && !Regex.IsMatch(value, definition.ValidationPattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
            throw new ArgumentException("The value does not match the registered validation rule.", nameof(value));
    }

    private static bool IsJson(string value)
    {
        try { using var _ = JsonDocument.Parse(value); return true; }
        catch (JsonException) { return false; }
    }

    private static void ValidateVersion(byte[]? actual, string? expected)
    {
        if (actual is null && expected is null) return;
        byte[]? expectedBytes;
        try { expectedBytes = expected is null ? null : Convert.FromBase64String(expected); }
        catch (FormatException) { throw new ArgumentException("The configuration version is invalid.", nameof(expected)); }
        if (actual is null || expectedBytes is null || !CryptographicOperations.FixedTimeEquals(actual, expectedBytes))
            throw new DBConcurrencyException("The configuration setting changed after it was loaded.");
    }

    private static SystemConfigurationSetting ReadSetting(SqlDataReader reader)
    {
        var isSecret = reader.GetBoolean(6);
        var hasValue = !reader.IsDBNull(10) && !reader.GetBoolean(11);
        var value = isSecret ? null : hasValue ? reader.GetString(10) : reader.IsDBNull(7) ? null : reader.GetString(7);
        return new(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetBoolean(5), isSecret,
            value, hasValue, reader.GetBoolean(8), reader.GetString(9), reader.IsDBNull(12) ? null : Convert.ToBase64String((byte[])reader[12]),
            reader.IsDBNull(13) ? null : reader.GetDateTime(13), reader.IsDBNull(14) ? null : reader.GetInt32(14));
    }

    private static async Task<Definition?> LoadDefinitionAsync(SqlConnection connection, SqlTransaction transaction, Guid id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT ApplicationScope, [Key], ValueType, IsRequired, IsSecret, ValidationPattern FROM dbo.SystemConfigurationDefinitions WHERE Id = @Id;";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), reader.GetBoolean(4), reader.IsDBNull(5) ? null : reader.GetString(5))
            : null;
    }

    private static async Task<Existing?> LoadExistingAsync(SqlConnection connection, SqlTransaction transaction, Guid definitionId, string environmentName, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id, ValuePayload, Version FROM dbo.SystemConfigurationValues WITH (UPDLOCK, HOLDLOCK) WHERE DefinitionId = @DefinitionId AND EnvironmentName = @EnvironmentName;";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@DefinitionId", definitionId);
        command.Parameters.AddWithValue("@EnvironmentName", environmentName);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? new(reader.GetGuid(0), reader.IsDBNull(1) ? null : reader.GetString(1), (byte[])reader[2])
            : null;
    }

    private static async Task UpsertAsync(SqlConnection connection, SqlTransaction transaction, Guid valueId, SystemConfigurationWrite write, string? value, bool encrypted, bool deleted, bool exists, CancellationToken cancellationToken)
    {
        var sql = exists
            ? "UPDATE dbo.SystemConfigurationValues SET ValuePayload=@Value, IsEncrypted=@Encrypted, IsDeleted=@Deleted, UpdatedBy=@Actor, UpdatedUtc=SYSUTCDATETIME() WHERE Id=@Id;"
            : "INSERT dbo.SystemConfigurationValues (Id,DefinitionId,EnvironmentName,ValuePayload,IsEncrypted,IsDeleted,UpdatedBy) VALUES (@Id,@DefinitionId,@EnvironmentName,@Value,@Encrypted,@Deleted,@Actor);";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Id", valueId); command.Parameters.AddWithValue("@DefinitionId", write.DefinitionId);
        command.Parameters.AddWithValue("@EnvironmentName", write.EnvironmentName); command.Parameters.AddWithValue("@Value", (object?)value ?? DBNull.Value);
        command.Parameters.AddWithValue("@Encrypted", encrypted); command.Parameters.AddWithValue("@Deleted", deleted); command.Parameters.AddWithValue("@Actor", write.Actor);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertRevisionAsync(SqlConnection connection, SqlTransaction transaction, Guid valueId, string? value, string? fingerprint, bool encrypted, SystemConfigurationWrite write, CancellationToken cancellationToken)
    {
        const string sql = "INSERT dbo.SystemConfigurationRevisions(ConfigurationValueId,RevisionNumber,ValuePayload,ValueFingerprint,IsEncrypted,ChangedBy,ChangeSource) SELECT @Id,ISNULL(MAX(RevisionNumber),0)+1,@Value,@Fingerprint,@Encrypted,@Actor,@Source FROM dbo.SystemConfigurationRevisions WHERE ConfigurationValueId=@Id;";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Id", valueId); command.Parameters.AddWithValue("@Value", (object?)value ?? DBNull.Value);
        command.Parameters.AddWithValue("@Fingerprint", (object?)fingerprint ?? new string('0', 64)); command.Parameters.AddWithValue("@Encrypted", encrypted);
        command.Parameters.AddWithValue("@Actor", write.Actor); command.Parameters.AddWithValue("@Source", write.Source);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertAuditAsync(SqlConnection connection, SqlTransaction transaction, Definition definition, SystemConfigurationWrite write, string action, string? previousFingerprint, string? newFingerprint, CancellationToken cancellationToken)
    {
        const string sql = "INSERT dbo.SystemConfigurationAudit(EnvironmentName,ApplicationScope,SettingKey,ActionName,Actor,ChangeSource,PreviousFingerprint,NewFingerprint) VALUES(@Environment,@Scope,@Key,@Action,@Actor,@Source,@Previous,@New);";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Environment", write.EnvironmentName); command.Parameters.AddWithValue("@Scope", definition.ApplicationScope);
        command.Parameters.AddWithValue("@Key", definition.Key); command.Parameters.AddWithValue("@Action", action); command.Parameters.AddWithValue("@Actor", write.Actor);
        command.Parameters.AddWithValue("@Source", write.Source); command.Parameters.AddWithValue("@Previous", (object?)previousFingerprint ?? DBNull.Value);
        command.Parameters.AddWithValue("@New", (object?)newFingerprint ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string Fingerprint(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private sealed record Definition(string ApplicationScope, string Key, string ValueType, bool IsRequired, bool IsSecret, string? ValidationPattern);
    private sealed record Existing(Guid Id, string? ValuePayload, byte[] Version);
}

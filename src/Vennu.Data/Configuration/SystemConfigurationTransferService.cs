using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Configuration;

public sealed class SystemConfigurationTransferService(
    IConfiguration configuration,
    ISystemConfigurationService configurationService) : ISystemConfigurationTransferService
{
    public async Task<SystemConfigurationManifest> ExportAsync(string environmentName, CancellationToken cancellationToken = default)
    {
        var settings = await configurationService.GetAsync(environmentName, null, cancellationToken).ConfigureAwait(false);
        return new(1, environmentName, DateTime.UtcNow, settings
            .Where(setting => !setting.IsSecret)
            .Select(setting => new SystemConfigurationManifestItem(
                setting.Key, setting.ApplicationScope, setting.ValueType, setting.RequiresRestart,
                setting.HasConfiguredValue ? setting.Value : null))
            .ToArray());
    }

    public async Task<SystemConfigurationImportPreview> PreviewAsync(
        string targetEnvironment,
        SystemConfigurationManifest manifest,
        CancellationToken cancellationToken = default)
    {
        if (manifest.SchemaVersion != 1) throw new ArgumentException("Unsupported configuration manifest version.", nameof(manifest));
        var current = await configurationService.GetAsync(targetEnvironment, null, cancellationToken).ConfigureAwait(false);
        var lookup = current.ToDictionary(setting => (setting.ApplicationScope, setting.Key));
        var items = new List<SystemConfigurationImportItem>();
        foreach (var incoming in manifest.Settings)
        {
            if (!lookup.TryGetValue((incoming.ApplicationScope, incoming.Key), out var setting))
            {
                items.Add(new(incoming.Key, incoming.ApplicationScope, "Invalid", incoming.Value, null, "The setting is not registered on this instance."));
                continue;
            }
            if (setting.IsSecret)
            {
                items.Add(new(incoming.Key, incoming.ApplicationScope, "Invalid", null, setting.Version, "Secret settings cannot be imported in a standard manifest."));
                continue;
            }
            if (incoming.Value is null)
            {
                items.Add(new(incoming.Key, incoming.ApplicationScope, "NoValue", null, setting.Version, "The manifest contains the definition but no configured value."));
                continue;
            }
            items.Add(new(incoming.Key, incoming.ApplicationScope,
                setting.HasConfiguredValue && string.Equals(setting.Value, incoming.Value, StringComparison.Ordinal) ? "Unchanged" : setting.HasConfiguredValue ? "Conflict" : "New",
                incoming.Value, setting.Version, null));
        }
        return new(Guid.NewGuid(), targetEnvironment, items);
    }

    public async Task ApplyAsync(SystemConfigurationImportApply import, CancellationToken cancellationToken = default)
    {
        if (import.OperationId == Guid.Empty) throw new ArgumentException("An import operation ID is required.", nameof(import));
        if (string.IsNullOrWhiteSpace(import.Actor)) throw new ArgumentException("An import actor is required.", nameof(import));
        var selected = import.Settings.Where(item => item.Status is "New" or "Conflict").ToArray();
        if (selected.Length == 0) return;
        await using var connection = new SqlConnection(configuration.GetConnectionString("VennuDatabase")
            ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required."));
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        foreach (var item in selected)
            await ApplyItemAsync(connection, transaction, import, item, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task ApplyItemAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        SystemConfigurationImportApply import,
        SystemConfigurationImportItem item,
        CancellationToken cancellationToken)
    {
        const string selectSql = """
            SELECT d.Id,d.ValueType,d.IsSecret,d.ValidationPattern,v.Id,v.Version,v.ValuePayload
            FROM dbo.SystemConfigurationDefinitions d
            LEFT JOIN dbo.SystemConfigurationValues v WITH (UPDLOCK,HOLDLOCK) ON v.DefinitionId=d.Id AND v.EnvironmentName=@Environment
            WHERE d.ApplicationScope=@Scope AND d.[Key]=@Key;
            """;
        await using var select = new SqlCommand(selectSql, connection, transaction);
        select.Parameters.AddWithValue("@Environment", import.TargetEnvironment); select.Parameters.AddWithValue("@Scope", item.ApplicationScope); select.Parameters.AddWithValue("@Key", item.Key);
        await using var reader = await select.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) throw new ArgumentException($"Setting '{item.Key}' is not registered.");
        var definitionId = reader.GetGuid(0); var valueType = reader.GetString(1); var isSecret = reader.GetBoolean(2);
        var pattern = reader.IsDBNull(3) ? null : reader.GetString(3); var valueId = reader.IsDBNull(4) ? Guid.NewGuid() : reader.GetGuid(4);
        var version = reader.IsDBNull(5) ? null : (byte[])reader[5]; var previousValue = reader.IsDBNull(6) ? null : reader.GetString(6);
        await reader.DisposeAsync().ConfigureAwait(false);
        if (isSecret) throw new ArgumentException("Secret settings cannot be imported in a standard manifest.");
        ValidateValue(item.Value, valueType, pattern);
        ValidateVersion(version, item.ExpectedVersion);
        var exists = version is not null;
        var upsertSql = exists
            ? "UPDATE dbo.SystemConfigurationValues SET ValuePayload=@Value,IsEncrypted=0,IsDeleted=0,UpdatedBy=@Actor,UpdatedUtc=SYSUTCDATETIME() WHERE Id=@Id;"
            : "INSERT dbo.SystemConfigurationValues(Id,DefinitionId,EnvironmentName,ValuePayload,IsEncrypted,IsDeleted,UpdatedBy) VALUES(@Id,@DefinitionId,@Environment,@Value,0,0,@Actor);";
        await using (var upsert = new SqlCommand(upsertSql, connection, transaction))
        {
            upsert.Parameters.AddWithValue("@Id", valueId); upsert.Parameters.AddWithValue("@DefinitionId", definitionId); upsert.Parameters.AddWithValue("@Environment", import.TargetEnvironment);
            upsert.Parameters.AddWithValue("@Value", item.Value!); upsert.Parameters.AddWithValue("@Actor", import.Actor);
            await upsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        var fingerprint = Fingerprint(item.Value!);
        const string revisionSql = "INSERT dbo.SystemConfigurationRevisions(ConfigurationValueId,RevisionNumber,ValuePayload,ValueFingerprint,IsEncrypted,ChangedBy,ChangeSource) SELECT @Id,ISNULL(MAX(RevisionNumber),0)+1,@Value,@Fingerprint,0,@Actor,N'Import' FROM dbo.SystemConfigurationRevisions WHERE ConfigurationValueId=@Id;";
        await using (var revision = new SqlCommand(revisionSql, connection, transaction))
        {
            revision.Parameters.AddWithValue("@Id", valueId); revision.Parameters.AddWithValue("@Value", item.Value!); revision.Parameters.AddWithValue("@Fingerprint", fingerprint); revision.Parameters.AddWithValue("@Actor", import.Actor);
            await revision.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        const string auditSql = "INSERT dbo.SystemConfigurationAudit(EnvironmentName,ApplicationScope,SettingKey,ActionName,Actor,ChangeSource,PreviousFingerprint,NewFingerprint,ImportOperationId) VALUES(@Environment,@Scope,@Key,N'Import',@Actor,N'Import',@Previous,@New,@OperationId);";
        await using var audit = new SqlCommand(auditSql, connection, transaction);
        audit.Parameters.AddWithValue("@Environment", import.TargetEnvironment); audit.Parameters.AddWithValue("@Scope", item.ApplicationScope); audit.Parameters.AddWithValue("@Key", item.Key); audit.Parameters.AddWithValue("@Actor", import.Actor);
        audit.Parameters.AddWithValue("@Previous", previousValue is null ? DBNull.Value : Fingerprint(previousValue)); audit.Parameters.AddWithValue("@New", fingerprint); audit.Parameters.AddWithValue("@OperationId", import.OperationId);
        await audit.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateVersion(byte[]? actual, string? expected)
    {
        var expectedBytes = expected is null ? null : Convert.FromBase64String(expected);
        if (actual is null && expectedBytes is null) return;
        if (actual is null || expectedBytes is null || !CryptographicOperations.FixedTimeEquals(actual, expectedBytes)) throw new DBConcurrencyException("A configuration setting changed after import preview.");
    }

    private static void ValidateValue(string? value, string type, string? pattern)
    {
        if (value is null) throw new ArgumentException("Imported settings require a value.");
        var valid = type switch
        {
            "Boolean" => bool.TryParse(value, out _), "Integer" => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "Decimal" => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _), "Uri" => Uri.TryCreate(value, UriKind.Absolute, out _),
            "Json" => IsJson(value), _ => true
        };
        if (!valid || pattern is not null && !System.Text.RegularExpressions.Regex.IsMatch(value, pattern)) throw new ArgumentException("An imported value failed its registered validation.");
    }

    private static bool IsJson(string value) { try { using var _ = JsonDocument.Parse(value); return true; } catch (JsonException) { return false; } }
    private static string Fingerprint(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

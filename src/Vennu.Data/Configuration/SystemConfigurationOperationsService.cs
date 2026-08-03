using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Configuration;

public sealed class SystemConfigurationOperationsService(IConfiguration configuration) : ISystemConfigurationOperationsService
{
    public async Task<IReadOnlyList<SystemConfigurationRevision>> GetRevisionsAsync(
        Guid definitionId,
        string environmentName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT r.RevisionNumber,r.ValueFingerprint,d.IsSecret,r.ValuePayload,r.ChangedBy,r.ChangeSource,r.CreatedUtc
            FROM dbo.SystemConfigurationValues v
            JOIN dbo.SystemConfigurationDefinitions d ON d.Id=v.DefinitionId
            JOIN dbo.SystemConfigurationRevisions r ON r.ConfigurationValueId=v.Id
            WHERE v.DefinitionId=@DefinitionId AND v.EnvironmentName=@Environment
            ORDER BY r.RevisionNumber DESC;
            """;
        await using var connection = new SqlConnection(ConnectionString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DefinitionId", definitionId); command.Parameters.AddWithValue("@Environment", environmentName);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var revisions = new List<SystemConfigurationRevision>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) revisions.Add(new(
            reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2), reader.IsDBNull(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6)));
        return revisions;
    }

    public async Task RollbackAsync(SystemConfigurationRollback rollback, CancellationToken cancellationToken = default)
    {
        if (rollback.RevisionNumber <= 0 || string.IsNullOrWhiteSpace(rollback.Actor)) throw new ArgumentException("A revision and actor are required.", nameof(rollback));
        await using var connection = new SqlConnection(ConnectionString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        const string selectSql = """
            SELECT v.Id,v.Version,v.ValuePayload,d.ApplicationScope,d.[Key],r.ValuePayload,r.ValueFingerprint,r.IsEncrypted
            FROM dbo.SystemConfigurationValues v WITH (UPDLOCK,HOLDLOCK)
            JOIN dbo.SystemConfigurationDefinitions d ON d.Id=v.DefinitionId
            JOIN dbo.SystemConfigurationRevisions r ON r.ConfigurationValueId=v.Id AND r.RevisionNumber=@Revision
            WHERE v.DefinitionId=@DefinitionId AND v.EnvironmentName=@Environment;
            """;
        await using var select = new SqlCommand(selectSql, connection, transaction);
        select.Parameters.AddWithValue("@Revision", rollback.RevisionNumber); select.Parameters.AddWithValue("@DefinitionId", rollback.DefinitionId); select.Parameters.AddWithValue("@Environment", rollback.EnvironmentName);
        await using var reader = await select.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) throw new KeyNotFoundException("The configuration revision was not found.");
        var valueId = reader.GetGuid(0); var version = (byte[])reader[1]; var previousPayload = reader.IsDBNull(2) ? null : reader.GetString(2);
        var scope = reader.GetString(3); var key = reader.GetString(4); var targetPayload = reader.IsDBNull(5) ? null : reader.GetString(5);
        var targetFingerprint = reader.GetString(6); var encrypted = reader.GetBoolean(7);
        await reader.DisposeAsync().ConfigureAwait(false);
        byte[] expected;
        try { expected = Convert.FromBase64String(rollback.ExpectedVersion); }
        catch (FormatException) { throw new ArgumentException("The rollback version is invalid.", nameof(rollback)); }
        if (!CryptographicOperations.FixedTimeEquals(version, expected)) throw new DBConcurrencyException("The configuration setting changed before rollback.");

        const string updateSql = "UPDATE dbo.SystemConfigurationValues SET ValuePayload=@Value,IsEncrypted=@Encrypted,IsDeleted=@Deleted,UpdatedBy=@Actor,UpdatedUtc=SYSUTCDATETIME() WHERE Id=@Id;";
        await using (var update = new SqlCommand(updateSql, connection, transaction))
        {
            update.Parameters.AddWithValue("@Value", (object?)targetPayload ?? DBNull.Value); update.Parameters.AddWithValue("@Encrypted", encrypted);
            update.Parameters.AddWithValue("@Deleted", targetPayload is null); update.Parameters.AddWithValue("@Actor", rollback.Actor); update.Parameters.AddWithValue("@Id", valueId);
            await update.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        const string revisionSql = "INSERT dbo.SystemConfigurationRevisions(ConfigurationValueId,RevisionNumber,ValuePayload,ValueFingerprint,IsEncrypted,ChangedBy,ChangeSource) SELECT @Id,MAX(RevisionNumber)+1,@Value,@Fingerprint,@Encrypted,@Actor,N'Rollback' FROM dbo.SystemConfigurationRevisions WHERE ConfigurationValueId=@Id;";
        await using (var revision = new SqlCommand(revisionSql, connection, transaction))
        {
            revision.Parameters.AddWithValue("@Id", valueId); revision.Parameters.AddWithValue("@Value", (object?)targetPayload ?? DBNull.Value);
            revision.Parameters.AddWithValue("@Fingerprint", targetFingerprint); revision.Parameters.AddWithValue("@Encrypted", encrypted); revision.Parameters.AddWithValue("@Actor", rollback.Actor);
            await revision.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        const string auditSql = "INSERT dbo.SystemConfigurationAudit(EnvironmentName,ApplicationScope,SettingKey,ActionName,Actor,ChangeSource,PreviousFingerprint,NewFingerprint) VALUES(@Environment,@Scope,@Key,N'Rollback',@Actor,N'PlatformOperations',@Previous,@New);";
        await using var audit = new SqlCommand(auditSql, connection, transaction);
        audit.Parameters.AddWithValue("@Environment", rollback.EnvironmentName); audit.Parameters.AddWithValue("@Scope", scope); audit.Parameters.AddWithValue("@Key", key); audit.Parameters.AddWithValue("@Actor", rollback.Actor);
        audit.Parameters.AddWithValue("@Previous", previousPayload is null ? DBNull.Value : Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(previousPayload)))); audit.Parameters.AddWithValue("@New", targetFingerprint);
        await audit.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private string ConnectionString() => configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");
}

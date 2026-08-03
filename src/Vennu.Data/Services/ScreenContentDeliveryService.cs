using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Services;

public sealed record ScreenContentDelivery(
    Guid ScreenId,
    Guid VenueId,
    long AuthoritativeRevision,
    long? AppliedRevision,
    string State,
    DateTime RequestedUtc,
    DateTime? ReceivedUtc = null,
    DateTime? AppliedUtc = null,
    string? PlayerVersion = null,
    string? ShellVersion = null,
    string? Platform = null,
    string? FailureCode = null,
    string? FailureDetail = null);

public sealed record ScreenContentReceipt(
    long Revision,
    string State,
    string ScreenKey,
    string? PlayerVersion,
    string? ShellVersion,
    string? Platform,
    string? FailureCode,
    string? FailureDetail,
    bool Recovered);

public interface IScreenContentDeliveryService
{
    Task<ScreenContentDelivery?> IssueAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
    Task<ScreenContentDelivery?> GetLatestAsync(Guid screenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, ScreenContentDelivery>> GetLatestByVenueAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<ScreenContentDelivery?> AcknowledgeAsync(Guid screenId, ScreenContentReceipt receipt, CancellationToken cancellationToken = default);
}

public sealed class ScreenContentDeliveryService(IConfiguration configuration, TimeProvider timeProvider) : IScreenContentDeliveryService
{
    public async Task<ScreenContentDelivery?> IssueAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.Screens WITH (UPDLOCK,HOLDLOCK) WHERE Id=@ScreenId AND VenueId=@VenueId AND Status<>N'Archived')
                RETURN;
            DECLARE @Revision BIGINT=ISNULL((SELECT MAX(Revision) FROM dbo.ScreenContentDeliveries WITH (UPDLOCK,HOLDLOCK) WHERE ScreenId=@ScreenId),0)+1;
            UPDATE dbo.ScreenContentDeliveries SET State=N'Superseded',UpdatedUtc=@Now
              WHERE ScreenId=@ScreenId AND State IN (N'Requested',N'Received');
            INSERT dbo.ScreenContentDeliveries (Id,ScreenId,VenueId,Revision,State,RequestedUtc,UpdatedUtc)
              VALUES (NEWID(),@ScreenId,@VenueId,@Revision,N'Requested',@Now,@Now);
            DELETE dbo.ScreenContentDeliveries WHERE ScreenId=@ScreenId AND Revision<@Revision
              AND RequestedUtc<DATEADD(DAY,-90,@Now);
            SELECT @ScreenId ScreenId,@VenueId VenueId,@Revision AuthoritativeRevision,
              (SELECT MAX(Revision) FROM dbo.ScreenContentDeliveries WHERE ScreenId=@ScreenId AND State IN (N'Applied',N'Recovered')) AppliedRevision,
              N'Requested' State,@Now RequestedUtc,NULL ReceivedUtc,NULL AppliedUtc,NULL PlayerVersion,NULL ShellVersion,NULL Platform,NULL FailureCode,NULL FailureDetail;
            """, connection, transaction);
        command.Parameters.AddWithValue("@ScreenId", screenId);
        command.Parameters.AddWithValue("@VenueId", venueId);
        command.Parameters.AddWithValue("@Now", timeProvider.GetUtcNow().UtcDateTime);
        var result = await ReadOneAsync(command, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<ScreenContentDelivery?> GetLatestAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand(LatestSql + " WHERE latest.ScreenId=@ScreenId;", connection);
        command.Parameters.AddWithValue("@ScreenId", screenId);
        return await ReadOneAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyDictionary<Guid, ScreenContentDelivery>> GetLatestByVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand(LatestSql + " WHERE latest.VenueId=@VenueId;", connection);
        command.Parameters.AddWithValue("@VenueId", venueId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var result = new Dictionary<Guid, ScreenContentDelivery>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var delivery = Read(reader);
            result[delivery.ScreenId] = delivery;
        }
        return result;
    }

    public async Task<ScreenContentDelivery?> AcknowledgeAsync(Guid screenId, ScreenContentReceipt receipt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        var state = NormalizeState(receipt.State);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.Screens WITH (UPDLOCK,HOLDLOCK) WHERE Id=@ScreenId AND ScreenKey=@ScreenKey AND Status<>N'Archived') RETURN;
            DECLARE @Latest BIGINT=(SELECT MAX(Revision) FROM dbo.ScreenContentDeliveries WITH (UPDLOCK,HOLDLOCK) WHERE ScreenId=@ScreenId);
            DECLARE @Applied BIGINT=(SELECT MAX(Revision) FROM dbo.ScreenContentDeliveries WHERE ScreenId=@ScreenId AND State IN (N'Applied',N'Recovered'));
            IF @Latest IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.ScreenContentDeliveries WHERE ScreenId=@ScreenId AND Revision=@Revision) RETURN;
            IF @Revision<@Latest OR (@Applied IS NOT NULL AND @Revision<@Applied)
              UPDATE dbo.ScreenContentDeliveries SET State=N'Superseded',UpdatedUtc=@Now WHERE ScreenId=@ScreenId AND Revision=@Revision AND State IN (N'Requested',N'Received');
            ELSE IF @State=N'Received'
              UPDATE dbo.ScreenContentDeliveries SET State=CASE WHEN State=N'Requested' THEN N'Received' ELSE State END,
                ReceivedUtc=COALESCE(ReceivedUtc,@Now),UpdatedUtc=@Now,PlayerVersion=@PlayerVersion,ShellVersion=@ShellVersion,Platform=@Platform
                WHERE ScreenId=@ScreenId AND Revision=@Revision AND State NOT IN (N'Applied',N'Recovered',N'Failed');
            ELSE IF @State=N'Applied'
            BEGIN
              UPDATE dbo.ScreenContentDeliveries SET State=CASE WHEN @Recovered=1 THEN N'Recovered' ELSE N'Applied' END,
                ReceivedUtc=COALESCE(ReceivedUtc,@Now),AppliedUtc=COALESCE(AppliedUtc,@Now),UpdatedUtc=@Now,
                PlayerVersion=@PlayerVersion,ShellVersion=@ShellVersion,Platform=@Platform,FailureCode=NULL,FailureDetail=NULL
                WHERE ScreenId=@ScreenId AND Revision=@Revision AND State NOT IN (N'Applied',N'Recovered');
              UPDATE dbo.ScreenContentDeliveries SET State=N'Superseded',UpdatedUtc=@Now
                WHERE ScreenId=@ScreenId AND Revision<@Revision AND State IN (N'Requested',N'Received');
            END
            ELSE IF @State=N'Failed'
              UPDATE dbo.ScreenContentDeliveries SET State=N'Failed',UpdatedUtc=@Now,PlayerVersion=@PlayerVersion,ShellVersion=@ShellVersion,
                Platform=@Platform,FailureCode=@FailureCode,FailureDetail=@FailureDetail
                WHERE ScreenId=@ScreenId AND Revision=@Revision AND State IN (N'Requested',N'Received',N'Failed');
            """ + LatestSql + " WHERE latest.ScreenId=@ScreenId;", connection, transaction);
        Add(command, "@ScreenId", screenId); Add(command, "@ScreenKey", receipt.ScreenKey.Trim()); Add(command, "@Revision", receipt.Revision);
        Add(command, "@State", state); Add(command, "@Now", now); Add(command, "@Recovered", receipt.Recovered);
        Add(command, "@PlayerVersion", Trim(receipt.PlayerVersion, 50)); Add(command, "@ShellVersion", Trim(receipt.ShellVersion, 50));
        Add(command, "@Platform", Trim(receipt.Platform, 50)); Add(command, "@FailureCode", Trim(receipt.FailureCode, 50));
        Add(command, "@FailureDetail", Trim(receipt.FailureDetail, 240));
        var result = await ReadOneAsync(command, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    private const string LatestSql = """
        ;WITH ranked AS (
          SELECT d.*,ROW_NUMBER() OVER(PARTITION BY d.ScreenId ORDER BY d.Revision DESC) row_number
          FROM dbo.ScreenContentDeliveries d
        ), latest AS (
          SELECT r.ScreenId,r.VenueId,r.Revision AuthoritativeRevision,
            (SELECT MAX(a.Revision) FROM dbo.ScreenContentDeliveries a WHERE a.ScreenId=r.ScreenId AND a.State IN (N'Applied',N'Recovered')) AppliedRevision,
            r.State,r.RequestedUtc,r.ReceivedUtc,r.AppliedUtc,r.PlayerVersion,r.ShellVersion,r.Platform,r.FailureCode,r.FailureDetail
          FROM ranked r WHERE r.row_number=1
        ) SELECT * FROM latest
        """;

    private async Task<SqlConnection> OpenAsync(CancellationToken token)
    {
        var connection = new SqlConnection(configuration.GetConnectionString("VennuDatabase") ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required."));
        await connection.OpenAsync(token).ConfigureAwait(false);
        return connection;
    }
    private static async Task<ScreenContentDelivery?> ReadOneAsync(SqlCommand command, CancellationToken token)
    { await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false); return await reader.ReadAsync(token).ConfigureAwait(false) ? Read(reader) : null; }
    private static ScreenContentDelivery Read(SqlDataReader r) => new(r.GetGuid(0),r.GetGuid(1),r.GetInt64(2),r.IsDBNull(3)?null:r.GetInt64(3),r.GetString(4),r.GetDateTime(5),r.IsDBNull(6)?null:r.GetDateTime(6),r.IsDBNull(7)?null:r.GetDateTime(7),r.IsDBNull(8)?null:r.GetString(8),r.IsDBNull(9)?null:r.GetString(9),r.IsDBNull(10)?null:r.GetString(10),r.IsDBNull(11)?null:r.GetString(11),r.IsDBNull(12)?null:r.GetString(12));
    private static void Add(SqlCommand command,string name,object? value)=>command.Parameters.AddWithValue(name,value??DBNull.Value);
    private static string NormalizeState(string value) => value?.Trim().ToLowerInvariant() switch { "received"=>"Received", "applied"=>"Applied", "failed"=>"Failed", _=>throw new ArgumentException("Receipt state must be Received, Applied, or Failed.",nameof(value)) };
    private static string? Trim(string? value,int max) { var result=string.IsNullOrWhiteSpace(value)?null:value.Trim(); return result is null?null:result.Length<=max?result:result[..max]; }
}

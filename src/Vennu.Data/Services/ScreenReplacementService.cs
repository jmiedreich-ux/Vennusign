using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Services;

public enum ScreenReplacementStatus
{
    Ready,
    Completed,
    PairingCodeNotFound,
    PairingCodeExpired,
    PairingCodeClaimed,
    TargetNotFound,
    TargetArchived,
    SourceNotFound,
    SourceAlreadyAssigned,
    TargetChanged,
    Conflict
}

public sealed record ScreenReplacementResult(
    ScreenReplacementStatus Status,
    Guid? TargetScreenId = null,
    Guid? SourceScreenId = null,
    string? TargetName = null,
    string? ReplacementPlatform = null,
    string? ReplacementAppVersion = null,
    string? WallGroup = null,
    int? WallPosition = null,
    bool PreservesConfiguration = false,
    bool PreservesHistory = false,
    bool PreservesVideoWall = false,
    DateTime? TargetUpdatedUtc = null,
    DateTime? CompletedUtc = null);

public interface IScreenReplacementService
{
    Task<ScreenReplacementResult> PreviewAsync(Guid venueId, Guid targetScreenId, string pairingCode, CancellationToken cancellationToken = default);
    Task<ScreenReplacementResult> ReplaceAsync(Guid venueId, Guid targetScreenId, string pairingCode, DateTime expectedTargetUpdatedUtc, string actor, CancellationToken cancellationToken = default);
}

public sealed class ScreenReplacementService(IConfiguration configuration, TimeProvider timeProvider) : IScreenReplacementService
{
    public Task<ScreenReplacementResult> PreviewAsync(Guid venueId, Guid targetScreenId, string pairingCode, CancellationToken cancellationToken = default) =>
        ExecuteAsync(venueId, targetScreenId, NormalizeCode(pairingCode), null, false, cancellationToken);

    public Task<ScreenReplacementResult> ReplaceAsync(Guid venueId, Guid targetScreenId, string pairingCode, DateTime expectedTargetUpdatedUtc, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor)) throw new ArgumentException("An actor is required.", nameof(actor));
        return ExecuteAsync(venueId, targetScreenId, NormalizeCode(pairingCode), actor.Trim(), true, cancellationToken, expectedTargetUpdatedUtc);
    }

    private async Task<ScreenReplacementResult> ExecuteAsync(
        Guid venueId,
        Guid targetScreenId,
        string code,
        string? actor,
        bool mutate,
        CancellationToken cancellationToken,
        DateTime? expectedTargetUpdatedUtc = null)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue is required.", nameof(venueId));
        if (targetScreenId == Guid.Empty) throw new ArgumentException("Target screen is required.", nameof(targetScreenId));

        await using var connection = new SqlConnection(ConnectionString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var previous = await ReadCompletedAsync(connection, transaction, venueId, targetScreenId, code, cancellationToken).ConfigureAwait(false);
        if (previous is not null)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return previous;
        }

        var target = await ReadScreenAsync(connection, transaction, targetScreenId, cancellationToken).ConfigureAwait(false);
        if (target is null || target.VenueId != venueId) return await FinishAsync(transaction, new(ScreenReplacementStatus.TargetNotFound), cancellationToken);
        if (string.Equals(target.Status, "Archived", StringComparison.OrdinalIgnoreCase)) return await FinishAsync(transaction, new(ScreenReplacementStatus.TargetArchived), cancellationToken);
        if (mutate && (!expectedTargetUpdatedUtc.HasValue || target.UpdatedUtc != expectedTargetUpdatedUtc.Value)) return await FinishAsync(transaction, new(ScreenReplacementStatus.TargetChanged), cancellationToken);

        var pairing = await ReadPairingAsync(connection, transaction, code, cancellationToken).ConfigureAwait(false);
        if (pairing is null) return await FinishAsync(transaction, new(ScreenReplacementStatus.PairingCodeNotFound), cancellationToken);
        if (pairing.IsClaimed) return await FinishAsync(transaction, new(ScreenReplacementStatus.PairingCodeClaimed), cancellationToken);
        if (pairing.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime) return await FinishAsync(transaction, new(ScreenReplacementStatus.PairingCodeExpired), cancellationToken);
        if (pairing.ScreenId == targetScreenId) return await FinishAsync(transaction, new(ScreenReplacementStatus.Conflict), cancellationToken);

        var source = await ReadScreenAsync(connection, transaction, pairing.ScreenId, cancellationToken).ConfigureAwait(false);
        if (source is null) return await FinishAsync(transaction, new(ScreenReplacementStatus.SourceNotFound), cancellationToken);
        if (source.VenueId.HasValue) return await FinishAsync(transaction, new(ScreenReplacementStatus.SourceAlreadyAssigned), cancellationToken);

        var ready = new ScreenReplacementResult(
            mutate ? ScreenReplacementStatus.Completed : ScreenReplacementStatus.Ready,
            target.Id,
            source.Id,
            target.Name,
            source.Platform,
            source.AppVersion,
            target.WallGroup,
            target.WallPosition,
            true,
            true,
            target.WallGroup is not null,
            target.UpdatedUtc,
            mutate ? timeProvider.GetUtcNow().UtcDateTime : null);
        if (!mutate) return await FinishAsync(transaction, ready, cancellationToken);

        var occurredUtc = ready.CompletedUtc!.Value;
        var retiredKey = Guid.NewGuid().ToString("N")[..9];
        await ExecuteMutationAsync(connection, transaction, """
            UPDATE dbo.Screens
            SET ScreenKey=@RetiredKey, VenueId=NULL, Status=N'Replaced', LastSeen=NULL,
                WallGroup=NULL, WallPosition=NULL, UpdatedUtc=@OccurredUtc
            WHERE Id=@SourceScreenId;

            UPDATE dbo.Screens
            SET ScreenKey=@ReplacementKey, Platform=@Platform, AppVersion=@AppVersion,
                Status=N'Offline', LastSeen=NULL, UpdatedUtc=@OccurredUtc
            WHERE Id=@TargetScreenId AND VenueId=@VenueId;

            UPDATE dbo.ScreenPairingCodes
            SET ScreenId=@TargetScreenId, VenueId=@VenueId, IsClaimed=1, ClaimedAt=@OccurredUtc
            WHERE Code=@PairingCode AND IsClaimed=0;

            INSERT dbo.ScreenReplacementAudits
                (Id,VenueId,TargetScreenId,SourceScreenId,PairingCode,Actor,PreviousPlatform,PreviousAppVersion,ReplacementPlatform,ReplacementAppVersion,OccurredUtc)
            VALUES
                (@AuditId,@VenueId,@TargetScreenId,@SourceScreenId,@PairingCode,@Actor,@PreviousPlatform,@PreviousAppVersion,@Platform,@AppVersion,@OccurredUtc);
            """, new Dictionary<string, object?>
            {
                ["@RetiredKey"] = retiredKey,
                ["@ReplacementKey"] = source.ScreenKey,
                ["@Platform"] = source.Platform,
                ["@AppVersion"] = source.AppVersion,
                ["@OccurredUtc"] = occurredUtc,
                ["@SourceScreenId"] = source.Id,
                ["@TargetScreenId"] = target.Id,
                ["@VenueId"] = venueId,
                ["@PairingCode"] = code,
                ["@AuditId"] = Guid.NewGuid(),
                ["@Actor"] = actor,
                ["@PreviousPlatform"] = target.Platform,
                ["@PreviousAppVersion"] = target.AppVersion
            }, cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return ready;
    }

    private static async Task<ScreenReplacementResult> FinishAsync(SqlTransaction transaction, ScreenReplacementResult result, CancellationToken token)
    {
        await transaction.RollbackAsync(token).ConfigureAwait(false);
        return result;
    }

    private static string NormalizeCode(string code)
    {
        var normalized = code?.Trim() ?? string.Empty;
        return normalized.Length == 6 && normalized.All(char.IsAsciiDigit)
            ? normalized
            : throw new ArgumentException("A six-digit pairing code is required.", nameof(code));
    }

    private string ConnectionString() => configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

    private static async Task<ScreenRow?> ReadScreenAsync(SqlConnection connection, SqlTransaction transaction, Guid id, CancellationToken token)
    {
        await using var command = new SqlCommand("SELECT Id,VenueId,ScreenKey,Name,WallGroup,WallPosition,Status,Platform,AppVersion,UpdatedUtc FROM dbo.Screens WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Id;", connection, transaction);
        command.Parameters.AddWithValue("@Id", id);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return null;
        return new(reader.GetGuid(0), reader.IsDBNull(1) ? null : reader.GetGuid(1), reader.GetString(2), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetInt32(5), reader.GetString(6), reader.IsDBNull(7) ? null : reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetString(8), reader.GetDateTime(9));
    }

    private static async Task<PairingRow?> ReadPairingAsync(SqlConnection connection, SqlTransaction transaction, string code, CancellationToken token)
    {
        await using var command = new SqlCommand("SELECT ScreenId,ExpiresAt,IsClaimed FROM dbo.ScreenPairingCodes WITH (UPDLOCK,HOLDLOCK) WHERE Code=@Code;", connection, transaction);
        command.Parameters.AddWithValue("@Code", code);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        return await reader.ReadAsync(token).ConfigureAwait(false) ? new(reader.GetGuid(0), reader.GetDateTime(1), reader.GetBoolean(2)) : null;
    }

    private static async Task<ScreenReplacementResult?> ReadCompletedAsync(SqlConnection connection, SqlTransaction transaction, Guid venueId, Guid targetId, string code, CancellationToken token)
    {
        await using var command = new SqlCommand("SELECT SourceScreenId,ReplacementPlatform,ReplacementAppVersion,OccurredUtc FROM dbo.ScreenReplacementAudits WITH (UPDLOCK,HOLDLOCK) WHERE PairingCode=@Code AND VenueId=@VenueId AND TargetScreenId=@TargetId;", connection, transaction);
        command.Parameters.AddWithValue("@Code", code); command.Parameters.AddWithValue("@VenueId", venueId); command.Parameters.AddWithValue("@TargetId", targetId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        return await reader.ReadAsync(token).ConfigureAwait(false)
            ? new(ScreenReplacementStatus.Completed, targetId, reader.GetGuid(0), ReplacementPlatform: reader.IsDBNull(1) ? null : reader.GetString(1), ReplacementAppVersion: reader.IsDBNull(2) ? null : reader.GetString(2), PreservesConfiguration: true, PreservesHistory: true, CompletedUtc: reader.GetDateTime(3))
            : null;
    }

    private static async Task ExecuteMutationAsync(SqlConnection connection, SqlTransaction transaction, string sql, IReadOnlyDictionary<string, object?> values, CancellationToken token)
    {
        await using var command = new SqlCommand(sql, connection, transaction);
        foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        var affected = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        if (affected != 4) throw new InvalidOperationException("The screen replacement transaction did not update every required record.");
    }

    private sealed record ScreenRow(Guid Id, Guid? VenueId, string ScreenKey, string Name, string? WallGroup, int? WallPosition, string Status, string? Platform, string? AppVersion, DateTime UpdatedUtc);
    private sealed record PairingRow(Guid ScreenId, DateTime ExpiresAt, bool IsClaimed);
}

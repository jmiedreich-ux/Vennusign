using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IMenuImportRepository
{
    Task<MenuImportAggregate> CreateAsync(MenuImportAggregate aggregate, CancellationToken cancellationToken = default);

    Task<MenuImportAggregate?> GetAsync(Guid venueId, Guid sessionId, DateTime nowUtc, CancellationToken cancellationToken = default);

    Task<MenuImportMutationOutcome> PutAnswerAsync(
        Guid venueId,
        Guid sessionId,
        byte[] expectedRevision,
        string questionKey,
        string fingerprint,
        string choice,
        Guid? selectedItemId,
        DateTime answeredUtc,
        string? answeredBy,
        CancellationToken cancellationToken = default);

    Task<MenuImportMutationOutcome> AcceptSafeMatchesAsync(
        Guid venueId,
        Guid sessionId,
        byte[] expectedRevision,
        DateTime answeredUtc,
        string? answeredBy,
        CancellationToken cancellationToken = default);

    Task<MenuImportMutationOutcome> ReplaceParseAsync(
        MenuImportAggregate aggregate,
        byte[] expectedRevision,
        CancellationToken cancellationToken = default);

    Task<MenuImportMutationOutcome> SetCreateDestinationAsync(
        Guid venueId, Guid sessionId, byte[] expectedRevision, string menuName,
        DateTime nowUtc, string? actor, CancellationToken cancellationToken = default);

    Task<MenuImportCreateOutcome> ConfirmCreateAsync(
        Guid venueId, Guid sessionId, byte[] expectedRevision, Guid actorUserId,
        IReadOnlyCollection<string> systemRoleKeys, DateTime nowUtc, string? actor,
        CancellationToken cancellationToken = default);

    Task<int> DeleteExpiredAsync(DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default);
}

public sealed record MenuImportCreateOutcome(string Result, MenuImportAggregate? Aggregate, Guid? MenuId)
{
    public const string Created = "created";
    public const string AlreadyCompleted = "already_completed";
    public const string NameConflict = "name_conflict";
    public const string MenuLimit = "menu_limit";
    public const string ItemLimit = "item_limit";
    public const string InvalidContent = "invalid_content";
    public const string PermissionDenied = "permission_denied";
}

public sealed record MenuImportMutationOutcome(string Result, MenuImportAggregate? Aggregate)
{
    public const string Updated = "updated";
    public const string NotFound = "not_found";
    public const string Expired = "expired";
    public const string Conflict = "conflict";
    public const string Invalid = "invalid";
}

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

    Task<int> DeleteExpiredAsync(DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default);
}

public sealed record MenuImportMutationOutcome(string Result, MenuImportAggregate? Aggregate)
{
    public const string Updated = "updated";
    public const string NotFound = "not_found";
    public const string Expired = "expired";
    public const string Conflict = "conflict";
    public const string Invalid = "invalid";
}

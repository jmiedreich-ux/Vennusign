namespace Vennu.Api.Contracts.BackOffice;

// Requests -------------------------------------------------------------------

public sealed record AvailabilityRequest(bool IsAvailable);

public sealed record DraftChangeRequest(
    string TargetKind,
    Guid? TargetId,
    string Field,
    string? BeforeValue,
    string? AfterValue);

public sealed record AssignmentRequest(Guid MenuId);

// Responses ------------------------------------------------------------------

/// <summary>
/// Timezone is the venue's, because every Menus surface renders its times in
/// venue-local time. Ceilings are read from the allowance model, never constants.
/// </summary>
public sealed record MenuContextResponse(
    string Timezone,
    IReadOnlyDictionary<string, int> Ceilings,
    int MenuCount);

public sealed record AvailabilityResponse(
    Guid ItemId,
    string Name,
    bool IsAvailable,
    DateTime ChangedUtc,
    string? ChangedBy,
    IReadOnlyCollection<Guid> ScreenIds);

public sealed record AvailabilityStateResponse(
    Guid ItemId,
    bool IsAvailable,
    DateTime ChangedUtc,
    string? ChangedBy);

/// <summary>
/// Count is the menu's current difference from its screens — exactly what a
/// publish will ship, not a tally of keystrokes.
/// </summary>
public sealed record DraftResponse(int Count, IReadOnlyCollection<DraftChangeResponse> Changes);

public sealed record DraftChangeResponse(
    string TargetKind,
    Guid? TargetId,
    string Field,
    string? BeforeValue,
    string? AfterValue,
    string? Author,
    DateTime UpdatedUtc);

public sealed record DiscardResponse(int Discarded);

public sealed record PublishResponse(
    long Version,
    int ChangeCount,
    DateTime PublishedUtc,
    string? Author,
    IReadOnlyCollection<PublishTargetResponse> Targets);

public sealed record PublishTargetResponse(Guid ScreenId, string State);

public sealed record HistoryEntryResponse(
    string Kind,
    DateTime OccurredUtc,
    string? Author,
    string? Detail,
    long? ReplacedByVersion);

public sealed record AssignmentResponse(
    Guid ScreenId,
    Guid MenuId,
    DateTime AssignedUtc,
    string? AssignedBy);

/// <summary>
/// A restore rebuilt the draft from a published version. ReplacedChangeCount says
/// how many queued changes it displaced, so the caller can warn honestly (Q67).
/// </summary>
public sealed record RestoreResponse(
    int Count,
    IReadOnlyCollection<DraftChangeResponse> Changes,
    int ReplacedChangeCount);

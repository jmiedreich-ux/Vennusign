namespace Vennu.Api.Contracts.BackOffice;

// Requests -------------------------------------------------------------------

public sealed record AvailabilityRequest(bool IsAvailable);

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

/// <summary>
/// One difference between the menu and what its screens are showing. It is
/// computed, so BeforeValue always comes from the published snapshot rather than
/// from whoever is asking.
/// </summary>
public sealed record DraftChangeResponse(
    string TargetKind,
    Guid? TargetId,
    string Field,
    string? BeforeValue,
    string? AfterValue);

public sealed record DiscardResponse(int Discarded);

public sealed record PutAwayRequest(bool IsPutAway);

/// <summary>
/// ConflictedScreenIds are screens this publish left alone because another menu
/// has since been given them. A safe no-op is still reported, never swallowed.
/// </summary>
public sealed record PublishResponse(
    long Version,
    int ChangeCount,
    DateTime PublishedUtc,
    string? Author,
    IReadOnlyCollection<PublishTargetResponse> Targets,
    IReadOnlyCollection<Guid> ConflictedScreenIds);

public sealed record PutAwayResponse(bool Changed, bool IsPutAway, int ActiveMenuCount);

public sealed record PublishTargetResponse(Guid ScreenId, string State);

/// <summary>
/// Version is the publish this entry names, and null for the kinds that are not a
/// publish. It is what makes "Go back to…" reachable: that act is addressed by
/// version, and without this the only place a client ever learns one is the response
/// to its own publish.
/// </summary>
public sealed record HistoryEntryResponse(
    string Kind,
    DateTime OccurredUtc,
    string? Author,
    string? Detail,
    long? ReplacedByVersion,
    long? Version);

/// <summary>
/// One card on the Menus home shelf.
///
/// Board is what the screens are showing, and is null when the menu has never been
/// published — the shelf renders that state rather than treating it as an error.
/// DraftCount is the difference between that board and the menu as it stands, so a
/// card's count and the board it draws always describe the same pair.
///
/// ScreenIds is published truth, never the assignments: a menu can be assigned to a
/// screen and not yet be on it, which is the whole point of a deliberate publish.
/// </summary>
public sealed record ShelfMenuResponse(
    Guid MenuId,
    string Name,
    string? Theme,
    bool IsPutAway,
    long? PublishedVersion,
    DateTime? LastPublishedUtc,
    string? LastPublishedBy,
    int DraftCount,
    IReadOnlyCollection<Guid> ScreenIds,
    BoardResponse? Board);

/// <summary>
/// A board as the render engine consumes it. Prices are strings because they are
/// stored exactly as typed — "9.5" never becomes "9.50", and "MP" is a price
/// (Q115/Q190). Theme is the menu theme attached to it, or null: no theme attached
/// is a valid state the engine renders plainly (Q86).
/// </summary>
public sealed record BoardResponse(
    Guid MenuId,
    string? Name,
    string? Theme,
    int DwellSeconds,
    int LoopWarningSeconds,
    IReadOnlyCollection<BoardSectionResponse> Sections);

public sealed record BoardSectionResponse(
    Guid SectionId,
    string? Name,
    int SortOrder,
    IReadOnlyCollection<BoardItemResponse> Items);

public sealed record BoardItemResponse(
    Guid ItemId,
    string? Name,
    string? Description,
    string? Price,
    int SortOrder);

/// <summary>
/// The board a menu's screens are showing, with the publish that put it there — read
/// as one row, so the version can never label a different board than the one returned.
/// </summary>
public sealed record PublishedBoardResponse(
    Guid MenuId,
    long Version,
    DateTime PublishedUtc,
    string? PublishedBy,
    BoardResponse? Board);

/// <summary>
/// The copy's id and the name it actually got. The name is returned rather than
/// assumed: a caller asking to duplicate "Summer Menu" may be given
/// "Summer Menu copy 3", and saying so is the difference between the UI showing the
/// truth and showing a guess.
/// </summary>
public sealed record DuplicateResponse(Guid MenuId, string Name, int ActiveMenuCount);

public sealed record AssignmentResponse(
    Guid ScreenId,
    Guid MenuId,
    DateTime AssignedUtc,
    string? AssignedBy);

/// <summary>
/// What a screen is showing right now: the menu and published version that reached
/// it. Everything but the screen is null when it is showing nothing.
///
/// This is the published truth, not the assignment. A menu can be assigned to a screen
/// and not yet be on it - that is the whole point of a deliberate publish - so the two
/// disagreeing is normal, and reading content from the assignment is a defect.
/// </summary>
public sealed record ScreenShowingResponse(
    Guid ScreenId,
    string ScreenName,
    Guid? MenuId,
    string? MenuName,
    long? Version,
    DateTime? PublishedUtc,
    string? PublishedBy);

/// <summary>
/// A restore rebuilt the draft from a published version. ReplacedChangeCount says
/// how many queued changes it displaced, so the caller can warn honestly (Q67).
/// </summary>
public sealed record RestoreResponse(
    int Count,
    IReadOnlyCollection<DraftChangeResponse> Changes,
    int ReplacedChangeCount);

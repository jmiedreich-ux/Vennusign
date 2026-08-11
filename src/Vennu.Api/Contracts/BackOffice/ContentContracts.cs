namespace Vennu.Api.Contracts.BackOffice;

// Requests -------------------------------------------------------------------

public sealed record AvailabilityRequest(bool IsAvailable);

public sealed record AssignmentRequest(Guid MenuId, Guid PageId, string Mode = "replace");

public sealed record PageNameRequest(string Name);
public sealed record PageOrderRequest(IReadOnlyCollection<Guid> PageIds);
public sealed record PageDeleteRequest(Guid? MoveSectionsToPageId, bool DeleteSections = false);
public sealed record SectionNameRequest(string Name, Guid? PageId = null);

public sealed record SectionOrderRequest(IReadOnlyCollection<Guid> SectionIds);

public sealed record ItemOrderRequest(IReadOnlyCollection<Guid> ItemIds);

/// <summary>
/// Placing something on a board. Exactly one of these is set: an ItemId places an
/// item the library already holds; a Name creates one born with that text and an
/// empty price and description (Q113).
/// </summary>
public sealed record PlaceRequest(Guid? ItemId, string? Name);

/// <summary>
/// An item's values. These are the item's, not this board's: one item is one
/// shared price everywhere it sits (Q5).
/// </summary>
/// <summary>
/// New values for an item, and optionally the values the caller believes are still
/// in place. The expectation is what makes Undo safe: with it, the edit applies only
/// while the row still holds what the caller last saw, so an inverse cannot erase
/// somebody else's later change without saying so.
/// </summary>
public sealed record ItemValuesRequest(
    string Name,
    string? Description,
    string? Price,
    string? ExpectedName = null,
    string? ExpectedDescription = null,
    string? ExpectedPrice = null);

// Responses ------------------------------------------------------------------

/// <summary>
/// Timezone is the venue's, because every Menus surface renders its times in
/// venue-local time. Ceilings are read from the allowance model, never constants.
/// </summary>
public sealed record MenuContextResponse(
    string Timezone,
    IReadOnlyDictionary<string, int> Ceilings,
    int MenuCount);

public sealed record MenuBuilderConfigurationResponse(
    long ImportFileSizeLimitBytes,
    double PublishRetrySilenceThresholdSeconds,
    int HistoryRetentionDepth);

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
    IReadOnlyCollection<PageResponse> Pages,
    IReadOnlyCollection<BoardSectionResponse> Sections);

public sealed record BoardSectionResponse(
    Guid SectionId,
    Guid PageId,
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

/// <summary>
/// A menu open in the builder. Board is the WORKING state: the canvas draws what
/// the menu says now, not what the screens are showing, because the canvas IS the
/// preview. Draft is the difference between the two, and the publish fields
/// describe the board it differs from — all read together, so the bar's one
/// sentence cannot describe two different menus.
/// </summary>
public sealed record BuilderBoardResponse(
    BoardResponse Board,
    int DraftCount,
    IReadOnlyCollection<DraftChangeResponse> Changes,
    long? PublishedVersion,
    DateTime? LastPublishedUtc,
    string? LastPublishedBy,
    IReadOnlyCollection<Guid> ScreenIds);

public sealed record SectionResponse(Guid SectionId, string Name, int SortOrder);

public sealed record PageResponse(Guid PageId, string Name, int SortOrder);
public sealed record PageDeleteResponse(int MovedSectionCount, int RemovedAssignmentCount);

/// <summary>
/// What a delete released back to the library, so the UI can say it rather than
/// guess (Q96).
/// </summary>
public sealed record SectionDeleteResponse(int ReleasedItemCount);

/// <summary>
/// The outcome of placing something. <c>already_on_board</c> is not a failure: it
/// carries the section the item already sits in so the UI jumps there instead of
/// placing a second copy (Q112).
/// </summary>
public sealed record PlaceResponse(
    string Outcome,
    Guid? ItemId,
    Guid? SectionId,
    int SortOrder,
    int ItemCountOnMenu);

/// <summary>
/// One add-row search result. Boards names where it already lives, capped in the
/// UI by Q123's vocabulary rather than here — the API states the fact, the surface
/// decides how much of it to say.
/// </summary>
public sealed record LibraryItemResponse(
    Guid ItemId,
    string Name,
    string? Description,
    string? Price,
    bool IsAvailable,
    IReadOnlyCollection<LibraryItemBoardResponse> Boards);

public sealed record LibraryItemBoardResponse(Guid MenuId, string MenuName);

/// <summary>
/// A menu theme that could be attached. Always empty for now, and deliberately a
/// real read rather than a hard-coded list: menu themes are created in the theme
/// editor, which does not exist yet, and no named looks ship (Q86).
/// </summary>
public sealed record MenuThemeResponse(string Key, string Name);

public sealed record AssignmentResponse(
    Guid ScreenId,
    Guid MenuId,
    Guid PageId,
    string? MenuName,
    string? PageName,
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
    int WidthPixels,
    int HeightPixels,
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

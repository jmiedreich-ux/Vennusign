namespace Vennu.Core.Models;

public sealed record MenuImportSession(
    Guid Id,
    Guid VenueId,
    string RawPaste,
    long ParseRevision,
    string Status,
    int LineCount,
    int ItemCount,
    DateTime ExpiresUtc,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    string? UpdatedBy,
    byte[] Revision,
    string? Destination = null,
    string? ProposedMenuName = null,
    Guid? CompletedMenuId = null,
    DateTime? CompletedUtc = null,
    Guid? TargetMenuId = null,
    DateTime? TargetUpdatedUtc = null,
    Guid? CompletedSnapshotId = null,
    string? TargetMenuName = null,
    bool? TargetHadPublishedVersion = null,
    int? TargetWorkingItemCount = null,
    int? TargetPublishedItemCount = null,
    int? TargetAddedCount = null,
    int? TargetRemovedCount = null,
    int? TargetChangedCount = null,
    DateTime? CompletedSnapshotRestoredUtc = null,
    /// <summary>Filled from an accepted suggestion, or typed. Back-office only for now (owner, 2026-08-26).</summary>
    string? ProposedMenuDescription = null,
    /// <summary>
    /// What the residue pass thinks this menu is called and what it is. A suggestion, never an
    /// answer: A18 permits nothing to be pre-answered unless a rule can name why, and a model
    /// names a reason rather than a rule. Applied only when the operator says so.
    /// </summary>
    string? SuggestedMenuName = null,
    string? SuggestedMenuDescription = null);

public sealed record MenuImportSourceLine(
    Guid SessionId,
    Guid VenueId,
    int LineNumber,
    /// <summary>Orders the items found within one pasted line (Q216). Zero for a line holding one thing.</summary>
    int LineSubIndex,
    string RawText,
    string Disposition,
    string? ParsedName,
    string? ParsedDescription,
    string? ParsedPrice,
    string? ParserReason,
    long ParseRevision,
    /// <summary>
    /// What the residue pass thinks this line is, and why, or null when it was never asked about.
    ///
    /// A suggestion sits beside the question, never in place of it: A18 permits nothing to be
    /// pre-answered unless a rule can name why, and a model names a reason rather than a rule. The
    /// operator applies it, or ignores it and answers as before.
    /// </summary>
    string? SuggestedVerdict = null,
    string? SuggestedReason = null);

public sealed record MenuImportReviewQuestion(
    Guid SessionId,
    Guid VenueId,
    string QuestionKey,
    string Fingerprint,
    string Kind,
    int DisplayOrder,
    bool Required,
    long ParseRevision,
    IReadOnlyCollection<int> LineNumbers,
    IReadOnlyCollection<MenuImportCandidate> Candidates,
    MenuImportAnswer? Answer);

public sealed record MenuImportCandidate(
    Guid ItemId,
    string DisplayName,
    string? DisplayPrice,
    string MatchRule,
    bool IsSafe);

public sealed record MenuImportAnswer(
    string Fingerprint,
    string Choice,
    Guid? SelectedItemId,
    long ParseRevision,
    DateTime AnsweredUtc,
    string? AnsweredBy);

public sealed record MenuImportAggregate(
    MenuImportSession Session,
    IReadOnlyCollection<MenuImportSourceLine> Lines,
    IReadOnlyCollection<MenuImportReviewQuestion> Questions);

public static class MenuImportStatuses
{
    public const string Reviewing = "reviewing";
    public const string Resolved = "resolved";
}

public static class MenuImportDestinations
{
    public const string Create = "create";
    public const string Replace = "replace";
}

public sealed record MenuImportReplacementFacts(Guid MenuId, string MenuName, DateTime TargetUpdatedUtc,
    bool HasPublishedVersion, int WorkingItemCount, int PublishedItemCount, int Added, int Removed, int Changed);

public sealed record MenuImportReplacementSnapshot(Guid Id, Guid VenueId, Guid MenuId, Guid SessionId,
    DateTime CreatedUtc, string? CreatedBy, DateTime ExpiresUtc, DateTime? RestoredUtc, string? RestoredBy);

public static class MenuImportChoices
{
    public const string SameItem = "same_item";
    public const string NewItem = "new_item";
    public const string Section = "section";
    public const string Fallback = "fallback";

    /// <summary>
    /// The third answer the design always specified for an unreadable line, and the only one that
    /// was never built: "An item / A section / Leave it out" (M1a, S6A-Q07). Without it the only
    /// way past a line the parser could not read was to import it and delete it afterwards.
    ///
    /// Nothing is destroyed. Menu creation pulls unresolved lines only where the answer is
    /// <see cref="Fallback"/>, so a line answered here is never placed while its text stays on the
    /// session - which is what Q81's "never silently drop a line" asks for.
    /// </summary>
    public const string LeaveOut = "leave_out";
}

/// <summary>What the residue pass may say about a line it was shown.</summary>
public static class MenuImportSuggestions
{
    public const string MenuName = "menu_name";
    public const string MenuDescription = "menu_description";
    public const string SectionHeading = "section_heading";
    public const string Dish = "dish";
    public const string LeaveOut = "leave_out";
}

/// <summary>
/// One unfinished import, as the Menus home needs to describe it.
///
/// The session already survives a closed tab - what it did not have was a way back. A screen that
/// says "saved until Friday" and offers no route to it states a fact and withholds the action,
/// which is what decision 5 exists to forbid.
///
/// <paramref name="AnswersRemaining"/> counts required questions at the session's current parse
/// revision that nobody has answered yet. Zero means the review is done and the import is waiting
/// at its destination step, not that there is nothing to come back to.
/// </summary>
public sealed record MenuImportSummary(
    Guid Id,
    int ItemCount,
    int LineCount,
    int AnswersRemaining,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    DateTime ExpiresUtc);

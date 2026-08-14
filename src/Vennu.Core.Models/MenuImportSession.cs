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
    DateTime? CompletedSnapshotRestoredUtc = null);

public sealed record MenuImportSourceLine(
    Guid SessionId,
    Guid VenueId,
    int LineNumber,
    string RawText,
    string Disposition,
    string? ParsedName,
    string? ParsedDescription,
    string? ParsedPrice,
    string? ParserReason,
    long ParseRevision);

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
}

namespace Vennu.Core.Models;

/// <summary>
/// The attributable record of what happened to a menu. Publishes land here, and
/// so do the destructive-but-instant acts, so that nothing irreversible is
/// anonymous. This is the provisional audit record for the milestone; a dedicated
/// audit capability is backlogged.
/// </summary>
public sealed class MenuHistoryEntry
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid MenuId { get; set; }

    /// <summary>
    /// Immutable page attribution for page-scoped events. It is intentionally an
    /// audit identity rather than a live navigation relationship: deleted pages
    /// do not delete or anonymise their history.
    /// </summary>
    public Guid? PageId { get; set; }

    public string? PageName { get; set; }

    public string Kind { get; set; } = MenuHistoryKinds.Published;

    public Guid? PublishEventId { get; set; }

    /// <summary>
    /// Supersession is never an action anyone takes; it survives only here, as
    /// the version that replaced this one.
    /// </summary>
    public long? ReplacedByVersion { get; set; }

    public string? Detail { get; set; }

    public string? Author { get; set; }

    public DateTime OccurredUtc { get; set; }

    /// <summary>
    /// The version of the publish event this entry names, when it names one. Read
    /// only — it is the publish event's value, not the entry's own — and it is what
    /// makes "Go back to…" reachable from a list of what happened, since that act is
    /// addressed by version.
    /// </summary>
    public long? Version { get; set; }
}

public static class MenuHistoryKinds
{
    public const string Published = "published";

    public const string DraftDiscarded = "draft_discarded";

    public const string PutAway = "put_away";

    /// <summary>Put back on the shelf — the one way out of put-away, which is otherwise terminal.</summary>
    public const string PutBack = "put_back";

    public const string TakenOffScreens = "taken_off_screens";

    public const string Restored = "restored";

    public const string Assigned = "assigned";

    /// <summary>
    /// Recorded on the copy, not the original: it is the only place that says where
    /// a never-published menu came from. Plain creation is deliberately not recorded
    /// — CreatedUtc already says all there is to say about it.
    /// </summary>
    public const string Duplicated = "duplicated";

    public const string SectionAdded = "section_added";

    public const string SectionRenamed = "section_renamed";

    public const string SectionsReordered = "sections_reordered";

    public const string SectionDeleted = "section_deleted";

    public const string ItemAdded = "item_added";

    public const string ItemsReordered = "items_reordered";

    public const string ItemMoved = "item_moved";

    public const string ItemRemoved = "item_removed";

    public static bool IsSupported(string? value) =>
        value is Published or DraftDiscarded or PutAway or PutBack or TakenOffScreens or Restored or Assigned or Duplicated
            or SectionAdded or SectionRenamed or SectionsReordered or SectionDeleted
            or ItemAdded or ItemsReordered or ItemMoved or ItemRemoved;
}

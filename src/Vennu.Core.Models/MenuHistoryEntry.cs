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
}

public static class MenuHistoryKinds
{
    public const string Published = "published";

    public const string DraftDiscarded = "draft_discarded";

    public const string PutAway = "put_away";

    public const string TakenOffScreens = "taken_off_screens";

    public const string Restored = "restored";

    public const string Assigned = "assigned";

    public static bool IsSupported(string? value) =>
        value is Published or DraftDiscarded or PutAway or TakenOffScreens or Restored or Assigned;
}

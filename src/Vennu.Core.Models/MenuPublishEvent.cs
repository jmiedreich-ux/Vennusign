namespace Vennu.Core.Models;

/// <summary>
/// One deliberate act of putting a menu on its screens. A publish is atomic:
/// the whole queued set ships, or nothing does and the draft is untouched.
/// </summary>
public sealed class MenuPublishEvent
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid MenuId { get; set; }

    public long Version { get; set; }

    public int ChangeCount { get; set; }

    public string? Author { get; set; }

    public DateTime PublishedUtc { get; set; }

    /// <summary>The published content itself, so this version can be rendered and restored later.</summary>
    public string? Snapshot { get; set; }

    /// <summary>The exact change set this publish shipped, captured as it was removed from the queue.</summary>
    public string? ShippedChanges { get; set; }
}

/// <summary>
/// How one screen is doing with one publish. Offline screens are not failures:
/// they catch up when they reconnect.
/// </summary>
public sealed class MenuPublishTarget
{
    public Guid Id { get; set; }

    public Guid PublishEventId { get; set; }

    public Guid ScreenId { get; set; }

    public string State { get; set; } = MenuPublishTargetStates.Pending;

    public DateTime UpdatedUtc { get; set; }
}

public static class MenuPublishTargetStates
{
    public const string Pending = "Pending";

    public const string Delivered = "Delivered";

    public const string Offline = "Offline";

    public const string Failed = "Failed";

    public static bool IsSupported(string? value) =>
        value is Pending or Delivered or Offline or Failed;
}

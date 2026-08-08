namespace Vennu.Core.Models;

/// <summary>
/// One queued change against a menu. The queue is the menu's current difference
/// from what its screens are showing: editing the same field twice replaces the
/// row rather than adding a second change, so the count always equals what
/// Publish will ship.
/// </summary>
public sealed class MenuDraftChange
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid MenuId { get; set; }

    public string TargetKind { get; set; } = DraftTargetKinds.Menu;

    public Guid? TargetId { get; set; }

    public string Field { get; set; } = string.Empty;

    public string? BeforeValue { get; set; }

    public string? AfterValue { get; set; }

    public string? Author { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

public static class DraftTargetKinds
{
    public const string Menu = "menu";

    public const string Section = "section";

    public const string Placement = "placement";

    public const string Item = "item";

    public const string Layout = "layout";

    public const string Theme = "theme";

    public static bool IsSupported(string? value) =>
        value is Menu or Section or Placement or Item or Layout or Theme;
}

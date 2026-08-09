namespace Vennu.Core.Models;

/// <summary>
/// What kind of thing a derived draft change is about. The draft itself is
/// computed — the difference between the working state and the last published
/// snapshot — so these kinds name parts of a menu, not rows in any queue.
/// </summary>
public static class DraftTargetKinds
{
    public const string Menu = "menu";

    public const string Section = "section";

    public const string Placement = "placement";

    public const string Item = "item";

    public const string Layout = "layout";

    public const string Theme = "theme";

    /// <summary>Which screens the menu is on. Take-off is permanent, so it waits in the draft rather than committing instantly (Q68).</summary>
    public const string Screens = "screens";
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Vennu.Core.Models;

namespace Vennu.Api.Services;

/// <summary>
/// The shape of a menu at a moment in time: what a board renders, plus which
/// screens it is on. A publish records one of these; the draft is the difference
/// between the menu now and the one the screens are showing.
/// </summary>
/// <remarks>
/// Item identity is permanent (Q43). A snapshot records an item's values against
/// its existing id and never mints a new one, so an 86 keeps its anchor across
/// every publish and restore.
/// </remarks>
public sealed class MenuSnapshot
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [JsonPropertyName("menuId")]
    public Guid MenuId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("theme")]
    public string? Theme { get; set; }

    [JsonPropertyName("dwellSeconds")]
    public int DwellSeconds { get; set; }

    [JsonPropertyName("loopWarningSeconds")]
    public int LoopWarningSeconds { get; set; }

    [JsonPropertyName("screens")]
    public List<SnapshotScreen>? Screens { get; set; }

    [JsonPropertyName("pages")]
    public List<SnapshotPage>? Pages { get; set; }

    [JsonPropertyName("sections")]
    public List<SnapshotSection>? Sections { get; set; }

    public static MenuSnapshot? Parse(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<MenuSnapshot>(json, Options);

    public static string Serialize(MenuSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Options);

    /// <summary>
    /// What is different between the menu the screens are showing and the menu as
    /// it stands. This is the draft: it is computed, never authored, so the count
    /// cannot disagree with what a publish will ship, and no caller can misreport
    /// a previous value (Q182).
    /// </summary>
    /// <param name="publishedJson">The last published snapshot, or null if never published.</param>
    /// <param name="workingJson">The menu as it stands now.</param>
    public static IReadOnlyList<SnapshotChange> Diff(string? publishedJson, string? workingJson)
    {
        var published = Parse(publishedJson);
        var working = Parse(workingJson);
        if (working is null)
        {
            return [];
        }

        var changes = new List<SnapshotChange>();

        Compare(changes, DraftTargetKinds.Menu, null, "name", published?.Name, working.Name);
        Compare(changes, DraftTargetKinds.Theme, null, "theme", published?.Theme, working.Theme);
        Compare(changes, DraftTargetKinds.Menu, null, "dwellSeconds", Number(published?.DwellSeconds), Number(working.DwellSeconds));
        Compare(changes, DraftTargetKinds.Menu, null, "loopWarningSeconds", Number(published?.LoopWarningSeconds), Number(working.LoopWarningSeconds));

        CompareScreens(changes, published, working);
        ComparePages(changes, published, working);
        CompareSections(changes, published, working);
        CompareItems(changes, published, working);
        ComparePlacements(changes, published, working);

        return changes;
    }

    private static void CompareScreens(List<SnapshotChange> changes, MenuSnapshot? published, MenuSnapshot working)
    {
        // Take-off is permanent, so it waits here as a difference in which screens
        // the menu is on, and ships on the next publish (Q68).
        var before = Join(published?.Screens?.Select(screen => $"{screen.ScreenId}:{screen.PageId}"));
        var after = Join(working.Screens?.Select(screen => $"{screen.ScreenId}:{screen.PageId}"));
        Compare(changes, DraftTargetKinds.Screens, null, "assignedScreens", before, after);
    }

    private static void ComparePages(List<SnapshotChange> changes, MenuSnapshot? published, MenuSnapshot working)
    {
        var before = (published?.Pages ?? []).ToDictionary(page => page.PageId);
        var after = (working.Pages ?? []).ToDictionary(page => page.PageId);
        foreach (var (pageId, page) in after)
        {
            before.TryGetValue(pageId, out var previous);
            Compare(changes, DraftTargetKinds.Page, pageId, "name", previous?.Name, page.Name);
            Compare(changes, DraftTargetKinds.Page, pageId, "sortOrder", Number(previous?.SortOrder), Number(page.SortOrder));
        }
        foreach (var pageId in before.Keys.Where(id => !after.ContainsKey(id)))
            changes.Add(new SnapshotChange(DraftTargetKinds.Page, pageId, "present", "true", "false"));
    }

    private static void CompareSections(List<SnapshotChange> changes, MenuSnapshot? published, MenuSnapshot working)
    {
        var before = (published?.Sections ?? []).ToDictionary(section => section.SectionId);
        var after = (working.Sections ?? []).ToDictionary(section => section.SectionId);

        foreach (var (sectionId, section) in after)
        {
            before.TryGetValue(sectionId, out var previous);
            Compare(changes, DraftTargetKinds.Section, sectionId, "name", previous?.Name, section.Name);
            Compare(changes, DraftTargetKinds.Section, sectionId, "pageId", previous?.PageId.ToString(), section.PageId.ToString());
            Compare(changes, DraftTargetKinds.Section, sectionId, "sortOrder", Number(previous?.SortOrder), Number(section.SortOrder));
        }

        foreach (var sectionId in before.Keys.Where(id => !after.ContainsKey(id)))
        {
            changes.Add(new SnapshotChange(DraftTargetKinds.Section, sectionId, "present", "true", "false"));
        }
    }

    /// <summary>
    /// An item's own values are the library's, not a placement's. Editing the price
    /// of an item that sits on three sections is one change, so it is compared once
    /// per item id — Q182 counts the latest state per field per item, and a person
    /// who changed one price should never be told they changed three things.
    /// </summary>
    private static void CompareItems(List<SnapshotChange> changes, MenuSnapshot? published, MenuSnapshot working)
    {
        var before = Items(published);
        var after = Items(working);

        foreach (var (itemId, item) in after)
        {
            if (!before.TryGetValue(itemId, out var previous))
            {
                // An item new to this menu arrives through its placement, which is
                // the change; its values travel inside the publish snapshot.
                continue;
            }

            Compare(changes, DraftTargetKinds.Item, itemId, "name", previous.Name, item.Name);
            Compare(changes, DraftTargetKinds.Item, itemId, "description", previous.Description, item.Description);
            // Prices are compared as typed, so "9.5" and "9.50" are genuinely
            // different values rather than the same number (Q115/Q190).
            Compare(changes, DraftTargetKinds.Item, itemId, "price", previous.Price, item.Price);
        }
    }

    private static void ComparePlacements(List<SnapshotChange> changes, MenuSnapshot? published, MenuSnapshot working)
    {
        // Keyed by section and item together: the same library item can sit on more
        // than one board, and flattening by item alone would lose that.
        var before = Placements(published);
        var after = Placements(working);

        foreach (var (key, placement) in after)
        {
            if (!before.TryGetValue(key, out var previous))
            {
                // A new placement is one change, like a removal is one change: the
                // person did one thing, and the count says so (Q182).
                changes.Add(new SnapshotChange(DraftTargetKinds.Placement, placement.ItemId, "placed", "false", "true"));
                continue;
            }

            Compare(changes, DraftTargetKinds.Placement, placement.ItemId, "sortOrder", Number(previous.SortOrder), Number(placement.SortOrder));
        }

        foreach (var (_, placement) in before.Where(entry => !after.ContainsKey(entry.Key)))
        {
            changes.Add(new SnapshotChange(DraftTargetKinds.Placement, placement.ItemId, "placed", "true", "false"));
        }
    }

    /// <summary>
    /// Every item the menu renders, once each, whatever it is placed on. Where the
    /// same item appears twice its values are identical by construction: they come
    /// from one library row.
    /// </summary>
    private static Dictionary<Guid, SnapshotItem> Items(MenuSnapshot? snapshot)
    {
        var map = new Dictionary<Guid, SnapshotItem>();
        foreach (var section in snapshot?.Sections ?? [])
        {
            foreach (var item in section.Items ?? [])
            {
                map[item.ItemId] = item;
            }
        }

        return map;
    }

    // Keyed by section and item together: the same library item can sit on more
    // than one board, and flattening by item alone would lose that.
    private static Dictionary<(Guid Section, Guid Item), SnapshotItem> Placements(MenuSnapshot? snapshot)
    {
        var map = new Dictionary<(Guid, Guid), SnapshotItem>();
        foreach (var section in snapshot?.Sections ?? [])
        {
            foreach (var item in section.Items ?? [])
            {
                map[(section.SectionId, item.ItemId)] = item;
            }
        }

        return map;
    }

    private static string Join(IEnumerable<string>? values) =>
        string.Join(",", (values ?? []).OrderBy(value => value, StringComparer.Ordinal));

    private static string Number(int? value) =>
        (value ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void Compare(
        List<SnapshotChange> changes,
        string targetKind,
        Guid? targetId,
        string field,
        string? before,
        string? after)
    {
        if (!string.Equals(before, after, StringComparison.Ordinal))
        {
            changes.Add(new SnapshotChange(targetKind, targetId, field, before, after));
        }
    }
}

public sealed class SnapshotScreen
{
    [JsonPropertyName("screenId")]
    public Guid ScreenId { get; set; }

    [JsonPropertyName("pageId")]
    public Guid PageId { get; set; }
}

public sealed class SnapshotPage
{
    [JsonPropertyName("pageId")]
    public Guid PageId { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

public sealed class SnapshotSection
{
    [JsonPropertyName("sectionId")]
    public Guid SectionId { get; set; }

    [JsonPropertyName("pageId")]
    public Guid PageId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    [JsonPropertyName("items")]
    public List<SnapshotItem>? Items { get; set; }
}

public sealed class SnapshotItem
{
    [JsonPropertyName("itemId")]
    public Guid ItemId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Exactly as typed: "12", "9.5" or "MP".</summary>
    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

public sealed record SnapshotChange(string TargetKind, Guid? TargetId, string Field, string? BeforeValue, string? AfterValue);

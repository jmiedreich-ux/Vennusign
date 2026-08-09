using System.Text.Json;
using System.Text.Json.Serialization;
using Vennu.Core.Models;

namespace Vennu.Api.Services;

/// <summary>
/// The published shape of a menu: what a board renders, captured at publish time
/// so a version can be shown and restored later without the draft queue that
/// produced it.
/// </summary>
/// <remarks>
/// Item identity is permanent (Q43). A snapshot records an item's values against
/// its existing id and never mints a new one, so an 86 keeps its anchor across
/// every publish and restore.
/// </remarks>
public sealed class MenuSnapshot
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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

    [JsonPropertyName("sections")]
    public List<SnapshotSection> Sections { get; set; } = [];

    public static MenuSnapshot? Parse(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<MenuSnapshot>(json, Options);

    public static string Serialize(MenuSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Options);

    /// <summary>
    /// The changes that would turn <paramref name="currentJson"/> into
    /// <paramref name="targetJson"/>. Only genuine differences are returned, so a
    /// restore to the state a menu is already in queues nothing.
    /// </summary>
    public static IReadOnlyList<SnapshotChange> Diff(string? currentJson, string? targetJson)
    {
        var current = Parse(currentJson);
        var target = Parse(targetJson);
        if (target is null)
        {
            return [];
        }

        var changes = new List<SnapshotChange>();

        AddIfDifferent(changes, DraftTargetKinds.Menu, null, "name", current?.Name, target.Name);
        AddIfDifferent(changes, DraftTargetKinds.Theme, null, "theme", current?.Theme, target.Theme);
        AddIfDifferent(
            changes,
            DraftTargetKinds.Menu,
            null,
            "dwellSeconds",
            current?.DwellSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture),
            target.DwellSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));

        var currentItems = Flatten(current);
        var targetItems = Flatten(target);

        foreach (var (itemId, targetItem) in targetItems)
        {
            currentItems.TryGetValue(itemId, out var currentItem);
            AddIfDifferent(changes, DraftTargetKinds.Item, itemId, "name", currentItem?.Name, targetItem.Name);
            AddIfDifferent(changes, DraftTargetKinds.Item, itemId, "description", currentItem?.Description, targetItem.Description);
            // Prices are compared as the text they were typed as, so "9.5" and
            // "9.50" are genuinely different values rather than the same number.
            AddIfDifferent(changes, DraftTargetKinds.Item, itemId, "price", currentItem?.Price, targetItem.Price);
        }

        // An item on the board now but absent from the target version comes off.
        foreach (var (itemId, currentItem) in currentItems)
        {
            if (!targetItems.ContainsKey(itemId))
            {
                changes.Add(new SnapshotChange(DraftTargetKinds.Placement, itemId, "placed", "true", "false"));
            }
        }

        return changes;
    }

    private static Dictionary<Guid, SnapshotItem> Flatten(MenuSnapshot? snapshot)
    {
        var map = new Dictionary<Guid, SnapshotItem>();
        foreach (var item in (snapshot?.Sections ?? []).SelectMany(section => section.Items ?? []))
        {
            map[item.ItemId] = item;
        }

        return map;
    }

    private static void AddIfDifferent(
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

public sealed class SnapshotSection
{
    [JsonPropertyName("sectionId")]
    public Guid SectionId { get; set; }

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

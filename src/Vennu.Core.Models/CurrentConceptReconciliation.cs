using System.Collections.ObjectModel;

namespace Vennu.Core.Models;

public enum CurrentConceptDispositionKind
{
    Capability = 1,
    SplitCapabilities = 2,
    Permission = 3,
    ProductState = 4,
    Allowance = 5,
    AddOnService = 6,
    LayoutTemplate = 7,
    RolloutControl = 8,
    Navigation = 9,
    Removal = 10
}

public sealed record CurrentConceptDisposition(
    string CurrentKey,
    CurrentConceptDispositionKind Kind,
    IReadOnlyList<CapabilityId> Capabilities,
    string TypedTarget);

public static class CurrentConceptReconciliation
{
    private static readonly ReadOnlyDictionary<string, CurrentConceptDisposition> FeatureKeysValue =
        Create(
        [
            Layout("photo_grid", "LayoutTemplate:photo_grid"),
            Layout("classic_diner", "LayoutTemplate:classic_diner"),
            Capability("basic_scheduling", "schedule.entry.manage"),
            Capability("allergen_badges", "content.item.dietary_information_manage"),
            Split("analytics", "Analytics capability family", "analytics.delivery_health.view", "analytics.operations.view", "analytics.portfolio.view"),
            Split("meal_periods", "Manual schedule plus advanced rotation", "schedule.entry.manage", "schedule.rotation.manage"),
            Capability("bilingual_display", "localization.variant.manage"),
            AddOn("ai_translation", "AddOnService:automated_translation", "localization.translation.automate"),
            Split("quick_update", "Core manual availability plus advanced bulk action", "content.item.availability_update", "content.collection.bulk_update"),
            Layout("all_layouts", "LayoutTemplateCatalog:advanced", "branding.layout.manage"),
            Split("happy_hour", "Promotion automation plus promotion product state", "schedule.promotion.automate"),
            AddOn("pos_integration", "AddOnService:point_of_sale", "content.source.synchronize"),
            Remove("staff_app", "Dormant presentation-only key; client choice never grants an action"),
            AddOn("ai_custom_builder", "AddOnService:automated_content_assistance"),
            Split("multi_location", "Authorized venue context plus organization governance", "organization.venue.manage", "organization.content.bulk_publish"),
            Capability("white_label", "branding.standard.manage"),
            Capability("html_editor", "branding.custom_content.manage"),
            Split("video_wall", "Wall configuration state plus coordination action", "screen.wall.coordinate")
        ]);

    private static readonly ReadOnlyDictionary<string, CurrentConceptDisposition> RouteKeysValue =
        Create(
        [
            Navigation("menus"),
            Navigation("scheduling"),
            Navigation("tap_list"),
            Navigation("screens"),
            Navigation("themes"),
            Navigation("pos_integration"),
            Navigation("billing"),
            Navigation("account")
        ]);

    public static IReadOnlyDictionary<string, CurrentConceptDisposition> FeatureKeys => FeatureKeysValue;

    public static IReadOnlyDictionary<string, CurrentConceptDisposition> RouteKeys => RouteKeysValue;

    private static ReadOnlyDictionary<string, CurrentConceptDisposition> Create(IEnumerable<CurrentConceptDisposition> dispositions) =>
        new(dispositions.ToDictionary(item => item.CurrentKey, StringComparer.Ordinal));

    private static CurrentConceptDisposition Capability(string key, string capability) =>
        new(key, CurrentConceptDispositionKind.Capability, Ids(capability), $"Capability:{capability}");

    private static CurrentConceptDisposition Split(string key, string target, params string[] capabilities) =>
        new(key, CurrentConceptDispositionKind.SplitCapabilities, Ids(capabilities), target);

    private static CurrentConceptDisposition AddOn(string key, string target, params string[] supportingCapabilities) =>
        new(key, CurrentConceptDispositionKind.AddOnService, Ids(supportingCapabilities), target);

    private static CurrentConceptDisposition Layout(string key, string target, params string[] supportingCapabilities) =>
        new(key, CurrentConceptDispositionKind.LayoutTemplate, Ids(supportingCapabilities), target);

    private static CurrentConceptDisposition Navigation(string key) =>
        new(key, CurrentConceptDispositionKind.Navigation, [], $"Navigation:{key}");

    private static CurrentConceptDisposition Remove(string key, string target) =>
        new(key, CurrentConceptDispositionKind.Removal, [], target);

    private static IReadOnlyList<CapabilityId> Ids(params string[] values) =>
        Array.AsReadOnly(values.Select(CapabilityId.Parse).ToArray());
}

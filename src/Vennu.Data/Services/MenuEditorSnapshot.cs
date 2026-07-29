using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed record MenuEditorSnapshot(
    IReadOnlyCollection<MenuEditorMenu> Menus,
    IReadOnlyCollection<MenuEditorItemGroup> ItemGroups,
    MenuEditorCapabilities Capabilities);

public sealed record MenuEditorMenu(Menu Menu, IReadOnlyCollection<MenuSection> Sections);

public sealed record MenuEditorItemGroup(Guid SectionId, IReadOnlyCollection<MenuItem> Items);

public sealed record MenuEditorCapabilities(bool HappyHour, bool AllergenBadges, bool QuickUpdate);

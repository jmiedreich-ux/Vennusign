using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed record MenuEditorSnapshot(IReadOnlyCollection<MenuEditorMenu> Menus);

public sealed record MenuEditorMenu(Menu Menu, IReadOnlyCollection<MenuSection> Sections);

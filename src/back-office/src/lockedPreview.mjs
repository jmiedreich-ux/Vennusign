const personalizedPreviewFeatures = new Set(['all_layouts', 'white_label', 'html_editor']);

export function supportsPersonalizedLockedPreview(featureKey) {
  return personalizedPreviewFeatures.has(featureKey);
}

export function buildPersonalizedLockedPreview(snapshot) {
  const menuEntry = snapshot.menus.find(entry => entry.menu.isActive) ?? snapshot.menus[0];
  if (!menuEntry) return undefined;

  const itemGroups = new Map(snapshot.itemGroups.map(group => [group.sectionId, group.items]));
  const sections = [...menuEntry.sections]
    .filter(section => section.isActive)
    .sort((left, right) => left.sortOrder - right.sortOrder)
    .map(section => ({
      id: section.id,
      name: section.name,
      items: [...(itemGroups.get(section.id) ?? [])]
        .filter(item => item.isActive)
        .sort((left, right) => left.sortOrder - right.sortOrder)
        .slice(0, 3)
        .map(item => ({ name: item.name, price: item.price, available: item.isAvailable }))
    }))
    .filter(section => section.items.length > 0)
    .slice(0, 2);

  return {
    menuName: menuEntry.menu.name,
    dailySpecial: menuEntry.menu.dailySpecial,
    sections
  };
}

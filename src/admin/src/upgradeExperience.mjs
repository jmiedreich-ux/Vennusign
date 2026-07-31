export const upgradeDismissalStorageKey = 'vennu.upgrade.dismissed';

export const tierPresentation = Object.freeze({
  starter: Object.freeze({ label: 'Starter', badgeLabel: 'STARTER', tone: 'slate' }),
  restaurant_starter: Object.freeze({
    label: 'Restaurant Starter',
    badgeLabel: 'RESTAURANT STARTER',
    tone: 'green'
  }),
  pro: Object.freeze({ label: 'Pro', badgeLabel: 'PRO', tone: 'amber' }),
  business: Object.freeze({ label: 'Business', badgeLabel: 'BUSINESS', tone: 'purple' })
});

export const upgradeCatalog = Object.freeze([
  Object.freeze({ featureKey: 'meal_periods', title: 'Meal periods', benefit: 'Breakfast, lunch, and dinner menus switch on time without staff intervention.', requiredTier: 'restaurant_starter' }),
  Object.freeze({ featureKey: 'bilingual_display', title: 'Bilingual displays', benefit: 'Guests can read the same board in two languages without duplicating screens.', requiredTier: 'restaurant_starter' }),
  Object.freeze({ featureKey: 'ai_translation', title: 'Menu translation', benefit: 'Translate menu copy quickly while keeping the original wording available for review.', requiredTier: 'restaurant_starter' }),
  Object.freeze({ featureKey: 'quick_update', title: 'Quick Update', benefit: 'Staff can mark an item sold out in seconds from one mobile-friendly list.', requiredTier: 'restaurant_starter' }),
  Object.freeze({ featureKey: 'all_layouts', title: 'All display layouts', benefit: 'Match every service style with premium restaurant and bar layouts.', requiredTier: 'pro' }),
  Object.freeze({ featureKey: 'happy_hour', title: 'Happy hour', benefit: 'Prices switch automatically at the scheduled time—no staff update needed.', requiredTier: 'pro' }),
  Object.freeze({ featureKey: 'pos_integration', title: 'POS integration', benefit: 'Items that sell out at the register can update on the board in seconds.', requiredTier: 'pro' }),
  Object.freeze({ featureKey: 'staff_app', title: 'Staff mobile app', benefit: 'Your team can update the board from anywhere without opening the full editor.', requiredTier: 'pro' }),
  Object.freeze({ featureKey: 'video_wall', title: 'Video walls', benefit: 'Coordinate one menu across multiple screens with deterministic positioning.', requiredTier: 'business' }),
  Object.freeze({ featureKey: 'multi_location', title: 'Multi-location control', benefit: 'Manage menus and screens across every venue from one operating view.', requiredTier: 'business' }),
  Object.freeze({ featureKey: 'white_label', title: 'White label', benefit: 'Keep every customer-facing screen aligned to your own brand.', requiredTier: 'business' }),
  Object.freeze({ featureKey: 'html_editor', title: 'Custom HTML', benefit: 'Build a fully custom presentation when standard layouts are not enough.', requiredTier: 'business' })
]);

export function readDismissedUpgradeFeatures(storage = globalThis.sessionStorage) {
  try {
    const value = JSON.parse(storage.getItem(upgradeDismissalStorageKey) ?? '[]');
    return new Set(Array.isArray(value) ? value.filter(item => typeof item === 'string') : []);
  } catch {
    return new Set();
  }
}

export function dismissUpgradeFeature(featureKey, storage = globalThis.sessionStorage) {
  if (!upgradeCatalog.some(item => item.featureKey === featureKey)) return;
  const dismissed = readDismissedUpgradeFeatures(storage);
  dismissed.add(featureKey);
  storage.setItem(upgradeDismissalStorageKey, JSON.stringify([...dismissed].sort()));
}

export function selectUpgradeOpportunity(effectiveFeatures, dismissed = new Set()) {
  return upgradeCatalog.find(item =>
    effectiveFeatures[item.featureKey]?.enabled === false && !dismissed.has(item.featureKey)
  );
}

const panelByFeature = Object.freeze({
  all_layouts: 'design',
  video_wall: 'design',
  white_label: 'design',
  html_editor: 'design',
  quick_update: 'menu',
  bilingual_display: 'menu',
  ai_translation: 'menu',
  pos_integration: 'menu',
  staff_app: 'menu',
  meal_periods: 'scheduling',
  happy_hour: 'scheduling',
  multi_location: 'operations'
});

export function upgradePanelForFeature(featureKey) {
  return panelByFeature[featureKey] ?? 'operations';
}

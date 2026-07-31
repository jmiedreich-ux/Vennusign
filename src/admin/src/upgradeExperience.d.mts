export type UpgradeTier = 'starter' | 'restaurant_starter' | 'pro' | 'business';
export type TierPresentation = { label: string; badgeLabel: string; tone: 'slate' | 'green' | 'amber' | 'purple' };
export type UpgradeOpportunity = { featureKey: string; title: string; benefit: string; requiredTier: UpgradeTier };
export type EffectiveFeatureMap = Record<string, { enabled: boolean } | undefined>;

export const upgradeDismissalStorageKey: string;
export const tierPresentation: Readonly<Record<UpgradeTier, Readonly<TierPresentation>>>;
export const upgradeCatalog: readonly Readonly<UpgradeOpportunity>[];
export function readDismissedUpgradeFeatures(storage?: Storage): Set<string>;
export function dismissUpgradeFeature(featureKey: string, storage?: Storage): void;
export function selectUpgradeOpportunity(effectiveFeatures: EffectiveFeatureMap, dismissed?: ReadonlySet<string>): Readonly<UpgradeOpportunity> | undefined;
export type UpgradePanel = 'design' | 'menu' | 'scheduling' | 'operations';
export function upgradePanelForFeature(featureKey: string): UpgradePanel;

export function buildTierSwitchImpact(detail, snapshot, targetTierId) {
  const targetTier = snapshot.tiers.find(tier => tier.id === targetTierId);
  if (!targetTier) return undefined;

  const targetFeatureIds = new Set(snapshot.enabledFeatures.filter(item => item.tierId === targetTierId).map(item => item.featureId));
  const targetFeatureKeys = new Set(snapshot.features.filter(feature => targetFeatureIds.has(feature.id)).map(feature => feature.key));
  const currentFeatureKeys = new Set(Object.values(detail.features).filter(feature => feature.enabled).map(feature => feature.key));
  const enabled = [...targetFeatureKeys].filter(key => !currentFeatureKeys.has(key)).sort();
  const disabled = [...currentFeatureKeys].filter(key => !targetFeatureKeys.has(key)).sort();
  const screenLimitExceeded = targetTier.maxScreens !== -1 && detail.screens.length > targetTier.maxScreens;

  return {
    currentTierName: detail.tier?.name ?? "No tier",
    targetTierName: targetTier.name,
    screenCount: detail.screens.length,
    targetScreenLimit: targetTier.maxScreens,
    screenLimitExceeded,
    enabled,
    disabled
  };
}

export function summarizeFeatureMatrixImpact(changes, snapshot) {
  const tierIds = new Set(changes.map(change => change.tierId));
  return {
    changedCount: changes.length,
    tierCount: tierIds.size,
    enabledCount: changes.filter(change => change.enabled).length,
    disabledCount: changes.filter(change => !change.enabled).length,
    tierNames: snapshot.tiers.filter(tier => tierIds.has(tier.id)).map(tier => tier.name)
  };
}

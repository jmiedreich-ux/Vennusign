import { useEffect, useMemo, useState } from "react";
import EntitlementLockChip from "./EntitlementLockChip";
import {
  dismissUpgradeFeature,
  listUpgradeOpportunities,
  readDismissedUpgradeFeatures,
  type EffectiveFeatureMap,
  type UpgradeOpportunity
} from "./upgradeExperience.mjs";

type Props = {
  effectiveFeatures: EffectiveFeatureMap;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
};

const rotationMilliseconds = 7_000;

export default function SidebarUpgradeNudge({ effectiveFeatures, onUpgrade }: Props) {
  const [dismissalVersion, setDismissalVersion] = useState(0);
  const [currentIndex, setCurrentIndex] = useState(0);
  const opportunities = useMemo(
    () => listUpgradeOpportunities(effectiveFeatures, readDismissedUpgradeFeatures()),
    [effectiveFeatures, dismissalVersion]
  );

  useEffect(() => {
    setCurrentIndex(index => opportunities.length ? index % opportunities.length : 0);
  }, [opportunities.length]);

  useEffect(() => {
    if (opportunities.length < 2 || window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    const timer = window.setInterval(
      () => setCurrentIndex(index => (index + 1) % opportunities.length),
      rotationMilliseconds
    );
    return () => window.clearInterval(timer);
  }, [opportunities.length]);

  const opportunity = opportunities[currentIndex];
  if (!opportunity) return null;

  const dismiss = () => {
    dismissUpgradeFeature(opportunity.featureKey);
    setDismissalVersion(version => version + 1);
  };

  return (
    <section className="sidebar-upgrade-nudge" aria-labelledby={`sidebar-upgrade-${opportunity.featureKey}`}>
      <button
        className="sidebar-upgrade-nudge__dismiss"
        type="button"
        aria-label={`Dismiss ${opportunity.title} suggestion`}
        onClick={dismiss}
      >×</button>
      <EntitlementLockChip opportunity={opportunity} onOpen={onUpgrade} compact />
      <strong className="sr-only" id={`sidebar-upgrade-${opportunity.featureKey}`}>{opportunity.title}</strong>
      <p>{opportunity.benefit}</p>
      {opportunities.length > 1 ? (
        <div className="sidebar-upgrade-nudge__dots" aria-label="Upgrade suggestions">
          {opportunities.map((item, index) => (
            <button
              type="button"
              key={item.featureKey}
              className={index === currentIndex ? "active" : ""}
              aria-label={`Show ${item.title}`}
              aria-current={index === currentIndex ? "true" : undefined}
              onClick={() => setCurrentIndex(index)}
            />
          ))}
        </div>
      ) : null}
    </section>
  );
}

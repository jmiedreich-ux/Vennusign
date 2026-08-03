import TierBadge from './TierBadge';
import type { UpgradeOpportunity } from './upgradeExperience.mjs';

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
};

export default function LockedNavigationItem({ opportunity, onUpgrade }: Props) {
  return (
    <button className="locked-navigation-item" type="button" onClick={() => onUpgrade(opportunity)}>
      <span><strong><span className="locked-navigation-item__lock" aria-hidden="true" />{opportunity.title}</strong><small>Preview this feature</small></span>
      <TierBadge tier={opportunity.requiredTier} />
    </button>
  );
}

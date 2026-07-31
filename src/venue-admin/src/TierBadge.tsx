import { tierPresentation, type UpgradeTier } from './upgradeExperience.mjs';

type TierBadgeProps = { tier: UpgradeTier };

export default function TierBadge({ tier }: TierBadgeProps) {
  const presentation = tierPresentation[tier];
  return <span aria-label={`${presentation.label} tier`} className={`upgrade-tier-badge upgrade-tier-badge--${presentation.tone}`}>{presentation.badgeLabel}</span>;
}

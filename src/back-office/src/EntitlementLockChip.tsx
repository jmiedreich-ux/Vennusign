import TierBadge from "./TierBadge";
import type { UpgradeOpportunity } from "./upgradeExperience.mjs";

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onOpen?: (opportunity: Readonly<UpgradeOpportunity>) => void;
  compact?: boolean;
};

export default function EntitlementLockChip({ opportunity, onOpen, compact = false }: Props) {
  const content = <>
    <span className="entitlement-lock-chip__icon" aria-hidden="true" />
    <span className="entitlement-lock-chip__label">{opportunity.title}</span>
    <TierBadge tier={opportunity.requiredTier} />
  </>;

  return onOpen
    ? <button
        className={`entitlement-lock-chip${compact ? " entitlement-lock-chip--compact" : ""}`}
        type="button"
        aria-label={`${opportunity.title} is locked; review ${opportunity.requiredTier.replace("_", " ")} upgrade options`}
        onClick={() => onOpen(opportunity)}
      >{content}</button>
    : <span className={`entitlement-lock-chip${compact ? " entitlement-lock-chip--compact" : ""}`}>{content}</span>;
}

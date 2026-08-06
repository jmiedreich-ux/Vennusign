import TierBadge from "./TierBadge";
import type { UpgradeOpportunity } from "./upgradeExperience.mjs";

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onOpen?: (opportunity: Readonly<UpgradeOpportunity>) => void;
  compact?: boolean;
  /** Test hooks so a locked entry is discoverable the same way an unlocked one is. */
  testId?: string;
  route?: string;
};

export default function EntitlementLockChip({ opportunity, onOpen, compact = false, testId, route }: Props) {
  const hooks = { "data-testid": testId, "data-route": route, "data-unlocked": route ? "false" : undefined };
  const content = <>
    <span className="entitlement-lock-chip__icon" aria-hidden="true" />
    <span className="entitlement-lock-chip__label">{opportunity.title}</span>
    <TierBadge tier={opportunity.requiredTier} />
  </>;

  return onOpen
    ? <button
        className={`entitlement-lock-chip${compact ? " entitlement-lock-chip--compact" : ""}`}
        type="button"
        {...hooks}
        aria-label={`${opportunity.title} is locked; review ${opportunity.requiredTier.replace("_", " ")} upgrade options`}
        onClick={() => onOpen(opportunity)}
      >{content}</button>
    : <span className={`entitlement-lock-chip${compact ? " entitlement-lock-chip--compact" : ""}`} {...hooks}>{content}</span>;
}

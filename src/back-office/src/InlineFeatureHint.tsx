import EntitlementLockChip from './EntitlementLockChip';
import type { UpgradeOpportunity } from './upgradeExperience.mjs';

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onDismiss: (featureKey: string) => void;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
};

export default function InlineFeatureHint({ opportunity, onDismiss, onUpgrade }: Props) {
  return (
    <aside className="inline-feature-hint" aria-labelledby={`hint-${opportunity.featureKey}`}>
      <div>
        <EntitlementLockChip opportunity={opportunity} onOpen={onUpgrade} compact />
        <strong id={`hint-${opportunity.featureKey}`}>{opportunity.title}</strong>
        <p>{opportunity.benefit}</p>
      </div>
      <button className="inline-feature-hint__dismiss" type="button" aria-label={`Dismiss ${opportunity.title} suggestion`} onClick={() => onDismiss(opportunity.featureKey)}>×</button>
    </aside>
  );
}

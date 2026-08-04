import EntitlementLockChip from './EntitlementLockChip';
import type { UpgradeOpportunity } from './upgradeExperience.mjs';

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onDismiss: (featureKey: string) => void;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
};

export default function LockedSectionPreview({ opportunity, onDismiss, onUpgrade }: Props) {
  return (
    <section className="locked-section-preview" aria-labelledby={`locked-${opportunity.featureKey}`}>
      <div className="locked-section-glimpse" aria-hidden="true">
        <span /><span /><span />
      </div>
      <div className="locked-section-copy">
        <EntitlementLockChip opportunity={opportunity} onOpen={onUpgrade} />
        <h3 id={`locked-${opportunity.featureKey}`}>{opportunity.title}</h3>
        <p>{opportunity.benefit}</p>
        <div>
          <button className="quiet" type="button" onClick={() => onDismiss(opportunity.featureKey)}>Not now</button>
        </div>
      </div>
    </section>
  );
}

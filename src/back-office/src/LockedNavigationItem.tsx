import EntitlementLockChip from './EntitlementLockChip';
import type { UpgradeOpportunity } from './upgradeExperience.mjs';

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
};

export default function LockedNavigationItem({ opportunity, onUpgrade }: Props) {
  return <EntitlementLockChip opportunity={opportunity} onOpen={onUpgrade} />;
}

import EntitlementLockChip from './EntitlementLockChip';
import type { UpgradeOpportunity } from './upgradeExperience.mjs';

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
  /** Route this entry stands in for, so locked entries are addressable like unlocked ones. */
  route?: string;
};

export default function LockedNavigationItem({ opportunity, onUpgrade, route }: Props) {
  return <EntitlementLockChip opportunity={opportunity} onOpen={onUpgrade} testId="nav-item" route={route} />;
}

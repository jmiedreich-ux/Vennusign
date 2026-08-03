import type { FeatureMatrixChange, FeatureMatrixSnapshot, VenueSupportDetail } from "./api";

export type TierSwitchImpact = {
  currentTierName: string;
  targetTierName: string;
  screenCount: number;
  targetScreenLimit: number;
  screenLimitExceeded: boolean;
  enabled: string[];
  disabled: string[];
};

export function buildTierSwitchImpact(detail: VenueSupportDetail, snapshot: FeatureMatrixSnapshot, targetTierId: string): TierSwitchImpact | undefined;
export function summarizeFeatureMatrixImpact(changes: FeatureMatrixChange[], snapshot: FeatureMatrixSnapshot): {
  changedCount: number;
  tierCount: number;
  enabledCount: number;
  disabledCount: number;
  tierNames: string[];
};

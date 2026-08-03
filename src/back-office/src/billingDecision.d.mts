import type { BackOfficeTierSummary } from './api';
export type PendingTierDecision = { targetTierId: string; targetTierName: string; requestedUtc: string };
export const pendingTierStaleAfterMs: number;
export function writePendingTierDecision(targetTier: Pick<BackOfficeTierSummary, 'id' | 'name'>, storage?: Storage, now?: number): PendingTierDecision;
export function readPendingTierDecision(storage?: Storage): PendingTierDecision | undefined;
export function clearPendingTierDecision(storage?: Storage): void;
export function resolvePendingTierDecision(pending: PendingTierDecision | undefined, currentTierId: string | undefined, now?: number): 'applied' | 'stale' | 'pending' | undefined;

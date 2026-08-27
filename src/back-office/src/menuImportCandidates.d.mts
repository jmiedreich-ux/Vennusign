export type ImportCandidate = {
  itemId: string;
  displayName: string;
  displayPrice: string | null;
  matchRule: string;
  isSafe: boolean;
  onMenus?: string[] | null;
  itemCreatedUtc?: string | null;
};

export function candidateProvenance(
  candidate: ImportCandidate | null | undefined,
  candidateCount: number
): string | null;

export function listPhrase(names: readonly string[], limit?: number): string;
export function madePhrase(createdUtc: string | null | undefined): string | null;

export type PriceMove = { name: string; from: string | null; to: string | null };

export type ReplacePreview = {
  arrivingCount: number;
  leavingCount: number;
  repricedCount: number;
  arriving: string[];
  leaving: string[];
  repriced: PriceMove[];
};

export function replaceSummary(preview: ReplacePreview | null | undefined): string | null;
export function andMore(shown: readonly unknown[] | null | undefined, total: number): string | null;
export function priceMovePhrase(move: PriceMove | null | undefined): string | null;

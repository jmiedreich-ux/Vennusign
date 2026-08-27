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

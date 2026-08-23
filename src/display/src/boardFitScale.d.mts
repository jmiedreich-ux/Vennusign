export const boardFitMinScale: number;
export function clampScale(scale: number, minScale?: number): number;
export function computeFitScale(contentHeight: number, viewportHeight: number, minScale?: number): number;
export type BoardFitSample = { width: number; height: number };
export function solveFitWidth(
  sampleA: BoardFitSample,
  sampleB: BoardFitSample,
  viewportWidth: number,
  viewportHeight: number
): number | null;
export type BoardFit = { scale: number; width: number | null };
export function computeBoardFit(
  natural: BoardFitSample,
  probe: BoardFitSample,
  viewportWidth: number,
  viewportHeight: number,
  minScale?: number
): BoardFit;

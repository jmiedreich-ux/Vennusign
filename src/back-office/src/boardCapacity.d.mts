import type { BoardResponse } from "./api";
export type BoardCapacity = { limit: number; count: number; dropped: string[]; state: "fits" | "nearly-full" | "overflow" };
export function calculateBoardCapacity(board: Pick<BoardResponse, "sections"> | null | undefined, geometry: { width: number; height: number }, theme?: string | null): BoardCapacity;

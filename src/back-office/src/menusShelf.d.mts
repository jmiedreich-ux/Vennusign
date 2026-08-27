import type { BoardResponse, ShelfMenu } from "./api";

export const shelfScaleThreshold: number;

export const unnamedMenuName: string;

export function isShelfAtScale(menus: readonly ShelfMenu[] | null | undefined): boolean;

export function menusInUse(menus: readonly ShelfMenu[] | null | undefined): ShelfMenu[];

export function menusNotInUse(menus: readonly ShelfMenu[] | null | undefined): ShelfMenu[];

export type ShelfFilter = {
  key: string;
  label: string;
  matches: (menu: ShelfMenu) => boolean;
};

export const shelfFilters: readonly ShelfFilter[];

export function availableShelfFilters(menus: readonly ShelfMenu[] | null | undefined): ShelfFilter[];

export function filterShelf(
  menus: readonly ShelfMenu[] | null | undefined,
  options?: { search?: string; filter?: string | null }
): ShelfMenu[];

export function hasChangesWaiting(menu: ShelfMenu): boolean;

export function screensOf(menu: ShelfMenu): readonly string[];

export function shelfHeadline(menus: readonly ShelfMenu[] | null | undefined): string;

export function shelfSubLine(menus: readonly ShelfMenu[] | null | undefined): string;

export function changePhrase(count: number): string;

export type CardStatus = { tone: "live" | "pending" | "idle"; text: string };

export function cardStatus(menu: ShelfMenu): CardStatus;

export function boardCounts(
  board: BoardResponse | null | undefined,
  unavailableItemIds?: Iterable<string> | null
): { sections: number; items: number };

export type OpenImportSummary = {
  id: string;
  itemCount: number;
  lineCount: number;
  answersRemaining: number;
  createdUtc: string;
  updatedUtc: string;
  expiresUtc: string;
};

export function importInProgressPhrase(
  open: OpenImportSummary | null | undefined,
  now?: Date
): string | null;

export function expiryPhrase(expiresUtc: string, now?: Date): string | null;

export type MenuAllowance = { used: number; limit: number | null };

export function menuAllowanceNotice(
  allowance: MenuAllowance | null | undefined
): { tone: "full" | "nearly"; text: string } | null;

export function isAtMenuLimit(allowance: MenuAllowance | null | undefined): boolean;

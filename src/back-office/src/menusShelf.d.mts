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

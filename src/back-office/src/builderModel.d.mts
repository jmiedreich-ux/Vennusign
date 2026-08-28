import type { BoardResponse, LibraryItem, MenuAvailability } from "./api";

export type BuilderView = "one-section" | "whole-board";

export type BuilderPlace = {
  view: BuilderView;
  sectionId: string | null;
  selectedItemId: string | null;
};

export type BoardSection = BoardResponse["sections"][number];
export type BoardItem = BoardSection["items"][number];

export type PublishScreen = { screenId: string; screenName: string; state: string; detail?: string | null };

export type PublishTargets = {
  mode: "chips" | "count";
  total: number;
  chips: PublishScreen[];
  exceptions: PublishScreen[];
  countPhrase: string;
};

export type BoardHit = {
  itemId: string;
  name: string | null;
  sectionId: string;
  sectionName: string | null;
};

export function sectionsOf(board: BoardResponse | null | undefined): BoardSection[];
export function itemsOf(board: BoardResponse | null | undefined, sectionId: string | null): BoardItem[];
export function sectionOf(board: BoardResponse | null | undefined, sectionId: string | null): BoardSection | null;
export function findItem(
  board: BoardResponse | null | undefined,
  itemId: string | null
): { item: BoardItem; sectionId: string } | null;

export function firstOpenState(board: BoardResponse | null | undefined): BuilderPlace;
export function resumeState(
  board: BoardResponse | null | undefined,
  remembered: Partial<BuilderPlace> | null | undefined
): BuilderPlace;
export function canvasBoard(board: BoardResponse, place: Pick<BuilderPlace, "view" | "sectionId">): BoardResponse;

export function draftPhrase(count: number, options?: { neverPublished?: boolean }): string;
export function canDiscardDraft(state: { draftCount: number; publishedVersion: number | null }): boolean;
export function publishLabel(count: number): string;
export function venueTime(utc: string | null | undefined, timezone: string | null | undefined): string | null;
export function publishedLine(
  menu: { lastPublishedUtc: string | null; lastPublishedBy: string | null },
  timezone: string | null | undefined
): string;

export function boardsPhrase(boards: LibraryItem["boards"] | null | undefined, currentMenuId: string): string | null;
export function sharedItemLine(boards: LibraryItem["boards"] | null | undefined, currentMenuId: string): string | null;
export function unavailableNote(
  availability: MenuAvailability | null | undefined,
  timezone: string | null | undefined,
  now?: Date
): string | null;
export function isMissingPrice(item: { price?: string | null } | null | undefined): boolean;
export type MissingPriceItem = { itemId: string; name: string };
export function changedItemsMissingPrice(
  board: BoardResponse | null | undefined,
  changes: readonly { targetKind: string; targetId: string | null; field: string; afterValue?: string | null }[] | null | undefined
): MissingPriceItem[];
export function availabilityLine(
  availability: MenuAvailability | null | undefined,
  timezone: string | null | undefined,
  now?: Date
): string | null;
export function availabilityTime(utc: string | null | undefined, timezone: string | null | undefined, now?: Date): string | null;

export function availabilityImpactNotice(
  itemName: string,
  isAvailable: boolean,
  screenIds: readonly string[] | null | undefined,
  screens: readonly { screenId: string; screenName: string; status: string; lastSeenUtc?: string | null }[],
  now?: number
): string;

export function publishBlockedReason(state: {
  draftCount: number;
  saveState: "clean" | "saving" | "failed";
  isPutAway?: boolean;
}): string | null;

export const screenChipCutover: number;
export function publishTargets(screens: PublishScreen[] | null | undefined): PublishTargets;

export function reorder(ids: string[] | null | undefined, from: number, to: number): string[];
export function findOnBoard(board: BoardResponse | null | undefined, query: string): BoardHit[];
export function changeSentence(
  change: { targetKind: string; targetId: string | null; field: string; beforeValue: string | null },
  board: BoardResponse | null | undefined
): string;
export function changeValues(change: {
  targetKind: string;
  beforeValue: string | null;
  afterValue: string | null;
}): string;
export function releasedPhrase(count: number): string;
export const bannedWords: string[];

export type PriceScopeQuestion = {
  total: number;
  title: string;
  hereLabel: string;
  hereDetail: string;
  everywhereLabel: string;
  everywhereDetail: string;
};

export function priceScopeQuestion(
  itemName: string | null | undefined,
  boards: readonly { menuId: string; menuName?: string }[] | null | undefined,
  currentMenuId: string
): PriceScopeQuestion | null;

/**
 * What a screen's state actually is. Archived and never-reported are named rather than
 * flattered into "Online" — see the note in builderModel.mjs.
 */
export function screenState(
  screen: { status?: string | null; lastSeenUtc?: string | null },
  now?: number
): { key: "online" | "offline" | "stale" | "unpaired" | "archived"; text: string };

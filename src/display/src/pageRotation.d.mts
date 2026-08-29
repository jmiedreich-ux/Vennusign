import type { DisplayContent, DisplayMenuPage } from './displayContent.mjs';

export function shouldRotate(pages: readonly DisplayMenuPage[] | undefined): boolean;
export function advance(index: number, count: number): number;
export function pageAt(
  pages: readonly DisplayMenuPage[] | undefined,
  index: number
): DisplayMenuPage | null | undefined;
export function contentForPage(
  content: DisplayContent | undefined,
  index: number
): DisplayContent | undefined;
export function dwellSecondsFor(content: DisplayContent | undefined): number;

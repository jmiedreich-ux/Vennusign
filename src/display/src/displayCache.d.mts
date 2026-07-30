import type { DisplayContent } from './displayContent.mjs';

export const displayContentCacheVersion: number;
export const displayContentCacheMaxAgeMs: number;

export type DisplayContentStorage = Pick<Storage, 'length' | 'key' | 'getItem' | 'setItem' | 'removeItem'>;

export type CachedDisplayContent = {
  version: number;
  screenId: string;
  cachedAt: number;
  content: DisplayContent;
};

export function buildDisplayContentCacheKey(screenId: string): string;

export function cacheDisplayContent(
  screenId: string,
  content: DisplayContent,
  storage?: DisplayContentStorage,
  now?: number
): void;

export function readCachedDisplayContent(
  screenId: string,
  storage?: DisplayContentStorage,
  now?: number
): CachedDisplayContent | null;

export function loadDisplayContentResilient(
  apiBaseUrl: string,
  screenId: string,
  options?: {
    fetchImpl?: typeof fetch;
    storage?: DisplayContentStorage;
    now?: number;
  }
): Promise<{
  content: DisplayContent;
  source: 'network' | 'cache';
  cachedAt: number;
}>;

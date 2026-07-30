export const fallbackLayoutKey: 'default';

export type DisplayLayoutRegistration<TRenderer> = Readonly<{
  key: string;
  label: string;
  renderer: TRenderer;
}>;

export type ResolvedDisplayLayout<TRenderer> = Readonly<{
  requestedKey: string;
  key: string;
  isFallback: boolean;
  registration: DisplayLayoutRegistration<TRenderer>;
}>;

export type DisplayLayoutRegistry<TRenderer> = Readonly<{
  keys: readonly string[];
  resolve(requestedKey: string | null | undefined): ResolvedDisplayLayout<TRenderer>;
}>;

export function normalizeLayoutKey(value: unknown): string;

export function createLayoutRegistry<TRenderer>(
  registrations: readonly DisplayLayoutRegistration<TRenderer>[],
  fallbackKey?: string
): DisplayLayoutRegistry<TRenderer>;

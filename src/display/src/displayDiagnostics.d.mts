export type DisplayDiagnosticsStorage = Pick<Storage, 'getItem' | 'setItem' | 'removeItem'>;

export type DisplayDiagnosticEventKind = 'content-fetch' | 'heartbeat' | 'receipt' | 'connection';

export type DisplayDiagnosticEvent = {
  kind: DisplayDiagnosticEventKind;
  at: number;
  detail: Record<string, unknown>;
};

export type DisplayDiagnosticsRecord = {
  version: number;
  screenId: string;
  events: DisplayDiagnosticEvent[];
  latest: Partial<Record<DisplayDiagnosticEventKind, DisplayDiagnosticEvent>>;
};

export const displayDiagnosticsVersion: number;
export const displayDiagnosticsMaxEvents: number;

export function buildDisplayDiagnosticsKey(screenId: string): string;

export function readDisplayDiagnostics(
  screenId: string,
  storage?: DisplayDiagnosticsStorage
): DisplayDiagnosticsRecord;

export function recordDisplayDiagnosticEvent(
  screenId: string,
  kind: DisplayDiagnosticEventKind,
  detail: Record<string, unknown>,
  storage?: DisplayDiagnosticsStorage,
  now?: number
): void;

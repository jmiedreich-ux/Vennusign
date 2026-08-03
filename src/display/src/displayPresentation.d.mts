import type { DisplayConnectionState } from './signalRClient';

export type PlayerStatePresentation = {
  eyebrow: string;
  title: string;
  message: string;
  busy: boolean;
  tone: 'loading' | 'error';
  actionLabel?: string;
};

export function getDisplayStatePresentation(kind: string): PlayerStatePresentation;
export function describeCachedContent(cachedAt: number, now?: number): string;
export function getConnectionPresentation(state: DisplayConnectionState): {
  label: string;
  tone: 'working' | 'offline' | 'online';
  visible: boolean;
};

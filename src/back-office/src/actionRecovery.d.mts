import type { ManagedScreen } from './api';
export type ScreenIdentityDraft = { name: string; location: string };
export type ScreenPresentationDraft = Pick<ManagedScreen, 'displayLayout' | 'photoGridDensity' | 'splitRatio' | 'heroDwellSeconds'>;
export function updateIdentityDraft(current: ScreenIdentityDraft | undefined, screen: ManagedScreen, patch: Partial<ScreenIdentityDraft>): ScreenIdentityDraft;
export function identityHasChanges(screen: ManagedScreen, draft: ScreenIdentityDraft | undefined): boolean;
export function updateScreenPresentationDraft(current: ScreenPresentationDraft | undefined, screen: ManagedScreen, patch: Partial<ScreenPresentationDraft>): ScreenPresentationDraft;
export function screenPresentationHasChanges(screen: ManagedScreen, draft: ScreenPresentationDraft | undefined): boolean;
export function passkeyInventoryView(state: { loading: boolean; failed: boolean; count: number }): 'loading' | 'failed' | 'empty' | 'loaded';

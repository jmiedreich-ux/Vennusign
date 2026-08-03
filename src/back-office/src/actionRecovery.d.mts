import type { ManagedScreen } from './api';
export type ScreenIdentityDraft = { name: string; location: string };
export function updateIdentityDraft(current: ScreenIdentityDraft | undefined, screen: ManagedScreen, patch: Partial<ScreenIdentityDraft>): ScreenIdentityDraft;
export function identityHasChanges(screen: ManagedScreen, draft: ScreenIdentityDraft | undefined): boolean;
export function passkeyInventoryView(state: { loading: boolean; failed: boolean; count: number }): 'loading' | 'failed' | 'empty' | 'loaded';

import type { MenuEditorSnapshot } from './api';

export type PersonalizedLockedPreview = {
  menuName: string;
  dailySpecial?: string;
  sections: Array<{
    id: string;
    name: string;
    items: Array<{ name: string; price: number; available: boolean }>;
  }>;
};

export function supportsPersonalizedLockedPreview(featureKey: string): boolean;
export function buildPersonalizedLockedPreview(snapshot: MenuEditorSnapshot): PersonalizedLockedPreview | undefined;

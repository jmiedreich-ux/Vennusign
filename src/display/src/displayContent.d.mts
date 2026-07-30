export type DisplayContent = {
  screenId: string;
  venueId: string | null;
  screenKey: string;
  screenName: string;
  status: string;
  lastSeenUtc: string | null;
  layout: string;
  venueName?: string | null;
  menuName?: string | null;
  dailySpecial?: string | null;
  isHappyHour?: boolean;
  happyHourEndsAtUtc?: string | null;
  happyHourMode?: 'automatic' | 'force_on' | 'force_off';
  photoGridDensity?: '2x2' | '3x2' | '4x2' | '3x3';
  photoGridOverflowItems?: number;
  splitRatio?: '40_60' | '50_50';
  heroDwellSeconds?: number;
  theme?: {
    backgroundColor: string;
    accentColor: string;
    fontFamily: 'Inter' | 'Georgia' | 'Arial';
    presetKey: string;
    titleColor: string;
    glowColor: string;
    boardBackgroundColor: string;
    sectionColors: string[];
    glowIntensity: number;
    titleFont: string;
    itemFont: string;
  };
  sections?: DisplayMenuSection[];
};

export type DisplayMenuSection = {
  id: string;
  name: string;
  items: DisplayMenuItem[];
};

export type DisplayMenuItem = {
  id: string;
  name: string;
  description: string | null;
  price: number;
  happyHourPrice: number | null;
  isAvailable: boolean;
  quantityAvailable: number | null;
  isPopular: boolean;
  tags: string[];
  imageUrl: string | null;
};

export type DisplayContentErrorKind = 'not-found' | 'api-error';

export class DisplayContentError extends Error {
  readonly kind: DisplayContentErrorKind;
  constructor(kind: DisplayContentErrorKind, message: string);
}

export function buildDisplayContentUrl(apiBaseUrl: string, screenId: string): string;

export function loadDisplayContent(
  apiBaseUrl: string,
  screenId: string,
  fetchImpl?: typeof fetch
): Promise<DisplayContent>;

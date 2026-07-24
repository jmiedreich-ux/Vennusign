export type DisplayContent = {
  screenId: string;
  venueId: string | null;
  screenKey: string;
  screenName: string;
  status: string;
  lastSeenUtc: string | null;
  layout: string;
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

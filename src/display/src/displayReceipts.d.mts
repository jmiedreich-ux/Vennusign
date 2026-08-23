import type { DisplayContent } from './displayContent.mjs';
export function buildDisplayReceiptUrl(apiBaseUrl: string, screenId: string): string;
export function describeReceiptSkipReason(content: DisplayContent | null | undefined): 'no-content-revision' | 'no-screen-key' | null;
export function reportContentReceipt(
  apiBaseUrl: string, screenId: string, content: DisplayContent, state: 'Received' | 'Applied' | 'Failed',
  metadata?: { playerVersion?: string; shellVersion?: string; platform?: string; recovered?: boolean; failureCode?: string; failureDetail?: string },
  fetchImpl?: typeof fetch
): Promise<unknown | null>;

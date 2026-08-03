export type CheckoutReturnState = 'success' | 'cancel';

export const checkoutRefreshDelays: readonly number[];
export function readCheckoutReturnState(search?: string): CheckoutReturnState | undefined;
export function stripCheckoutReturnParameter(search?: string): string;
export function requireHostedCheckoutUrl(value: string): string;

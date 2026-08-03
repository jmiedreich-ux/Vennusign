export const checkoutRefreshDelays = Object.freeze([750, 2000, 5000]);

export function readCheckoutReturnState(search = '') {
  const value = new URLSearchParams(search).get('checkout');
  return value === 'success' || value === 'cancel' ? value : undefined;
}

export function stripCheckoutReturnParameter(search = '') {
  const parameters = new URLSearchParams(search);
  parameters.delete('checkout');
  const remaining = parameters.toString();
  return remaining ? `?${remaining}` : '';
}

export function requireHostedCheckoutUrl(value) {
  const url = new URL(value);
  if (url.protocol !== 'https:' || url.hostname.toLowerCase() !== 'checkout.stripe.com') {
    throw new Error('Checkout did not return an approved hosted URL.');
  }
  return url.href;
}

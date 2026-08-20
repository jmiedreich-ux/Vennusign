export const canonicalOnboardingPath = '/onboarding';

export function safeLocalReturnPath(value, fallback = canonicalOnboardingPath) {
  return typeof value === 'string' && value.startsWith('/') && !value.startsWith('//') && !value.includes('\\')
    ? value
    : fallback;
}

export function authenticatedCustomerDestination(entryPath, requestedReturnPath, onboarding) {
  // Returning a destination tells the caller to window.location.replace() it. Naming
  // the page the visitor is already on therefore reloads it, resolves the same answer,
  // and replaces again - an endless loop that shows the onboarding page flickering
  // against its own loading state. Nobody is sent to where they already are.
  if (entryPath === canonicalOnboardingPath && !onboarding?.progress?.goLive) return undefined;

  if (!onboarding?.progress?.goLive) return canonicalOnboardingPath;
  if (entryPath === canonicalOnboardingPath && requestedReturnPath === canonicalOnboardingPath) return undefined;
  const destination = safeLocalReturnPath(requestedReturnPath, '/');
  return ['/signup', '/signin', canonicalOnboardingPath].includes(destination.split('?')[0]) ? '/' : destination;
}

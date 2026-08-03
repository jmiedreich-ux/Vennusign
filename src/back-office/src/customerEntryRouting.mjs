export const canonicalOnboardingPath = '/onboarding';

export function safeLocalReturnPath(value, fallback = canonicalOnboardingPath) {
  return typeof value === 'string' && value.startsWith('/') && !value.startsWith('//') && !value.includes('\\')
    ? value
    : fallback;
}

export function authenticatedCustomerDestination(entryPath, requestedReturnPath, onboarding) {
  if (!onboarding?.progress?.goLive) return canonicalOnboardingPath;
  if (entryPath === canonicalOnboardingPath && requestedReturnPath === canonicalOnboardingPath) return undefined;
  const destination = safeLocalReturnPath(requestedReturnPath, '/');
  return ['/signup', '/signin', canonicalOnboardingPath].includes(destination.split('?')[0]) ? '/' : destination;
}

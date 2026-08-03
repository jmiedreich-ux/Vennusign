import type { CustomerOnboardingSnapshot } from './customerOnboardingApi';
export const canonicalOnboardingPath: '/onboarding';
export function safeLocalReturnPath(value: string | null | undefined, fallback?: string): string;
export function authenticatedCustomerDestination(entryPath: string, requestedReturnPath: string, onboarding: CustomerOnboardingSnapshot): string | undefined;

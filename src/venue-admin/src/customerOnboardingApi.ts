import type { VenueAdminConfiguration } from "./config";
import { requireHostedCheckoutUrl } from "./checkoutFlow.mjs";

export type CustomerSession = {
  userId: string;
  email: string;
  displayName: string;
  authenticationMethod: string;
};

export type PublicOnboardingPlan = {
  id: string;
  name: string;
  slug: string;
  monthlyPrice: number;
  trialDays: number;
  maxVenues: number;
  maxScreens: number;
  monthlyCheckoutAvailable: boolean;
  annualCheckoutAvailable: boolean;
};

export type CustomerOnboardingSnapshot = {
  userId: string;
  organizationId?: string;
  selectedTierId?: string;
  venueId?: string;
  firstScreenId?: string;
  currentStep: "account" | "plan" | "venue" | "first-screen" | "go-live";
  entitlementStatus: "none" | "trialing" | "active" | "past_due" | "canceled";
  trialEndsAt?: string;
  checkoutPending: boolean;
  progress: { account: boolean; plan: boolean; venue: boolean; firstScreen: boolean; goLive: boolean };
  updatedUtc: string;
};

export class CustomerOnboardingApiError extends Error {
  constructor(public readonly status: number, message: string) { super(message); }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, { ...init, credentials: "include" });
  if (!response.ok) {
    const text = await response.text();
    throw new CustomerOnboardingApiError(response.status, text || "Vennu could not complete that request.");
  }
  return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

export function loadPublicPlans(configuration: VenueAdminConfiguration, signal?: AbortSignal) {
  return request<PublicOnboardingPlan[]>(`${configuration.apiBaseUrl}/api/customer-onboarding/plans`, { signal });
}

export function loadCustomerSession(configuration: VenueAdminConfiguration, signal?: AbortSignal) {
  return request<CustomerSession>(`${configuration.apiBaseUrl}/api/customer-auth/session`, { signal });
}

export function loadCustomerOnboarding(configuration: VenueAdminConfiguration, signal?: AbortSignal) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding`, { signal });
}

export function requestEmailLink(configuration: VenueAdminConfiguration, email: string) {
  return request<void>(`${configuration.apiBaseUrl}/api/customer-auth/email-links`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, returnPath: "/onboarding" })
  });
}

export function createOnboardingOrganization(configuration: VenueAdminConfiguration, name: string) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding/organization`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name })
  });
}

export function startOnboardingTrial(configuration: VenueAdminConfiguration, tierId: string) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding/trial`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ tierId })
  });
}

export async function createOnboardingCheckout(
  configuration: VenueAdminConfiguration,
  tierId: string,
  billingInterval: "monthly" | "annual"
) {
  const result = await request<{ checkoutUrl: string }>(`${configuration.apiBaseUrl}/api/customer-onboarding/checkout`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ tierId, billingInterval })
  });
  return requireHostedCheckoutUrl(result.checkoutUrl);
}

export function revokeCustomerSession(configuration: VenueAdminConfiguration) {
  return request<void>(`${configuration.apiBaseUrl}/api/customer-auth/session`, { method: "DELETE" });
}

export function externalSignInUrl(configuration: VenueAdminConfiguration, provider: "google" | "apple") {
  return `${configuration.apiBaseUrl}/api/customer-auth/external/${provider}?returnPath=${encodeURIComponent("/onboarding")}`;
}

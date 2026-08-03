import type { BackOfficeConfiguration } from "./config";
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
  organization?: { name: string; legalName?: string; primaryContactName?: string; contactEmail?: string; contactPhone?: string; mailingAddress?: string };
  selectedTierId?: string;
  venueId?: string;
  firstScreenId?: string;
  currentStep: "account" | "plan" | "venue" | "first-screen" | "go-live";
  entitlementStatus: "none" | "trialing" | "active" | "past_due" | "canceled";
  trialEndsAt?: string;
  checkoutPending: boolean;
  firstScreenStatus: "not-paired" | "paired-offline" | "online";
  firstScreenLastSeenUtc?: string;
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
    throw new CustomerOnboardingApiError(response.status, text || "Vennusign could not complete that request.");
  }
  return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

export function loadPublicPlans(configuration: BackOfficeConfiguration, signal?: AbortSignal) {
  return request<PublicOnboardingPlan[]>(`${configuration.apiBaseUrl}/api/customer-onboarding/plans`, { signal });
}

export function loadCustomerSession(configuration: BackOfficeConfiguration, signal?: AbortSignal) {
  return request<CustomerSession>(`${configuration.apiBaseUrl}/api/customer-auth/session`, { signal });
}

export function loadCustomerOnboarding(configuration: BackOfficeConfiguration, signal?: AbortSignal) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding`, { signal });
}

export function requestEmailLink(configuration: BackOfficeConfiguration, email: string, returnPath = "/onboarding") {
  return request<void>(`${configuration.apiBaseUrl}/api/customer-auth/email-links`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, returnPath })
  });
}

export function createOnboardingOrganization(configuration: BackOfficeConfiguration, organization: {
  name: string; legalName?: string; primaryContactName: string; contactEmail: string; contactPhone?: string; mailingAddress: string;
}) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding/organization`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(organization)
  });
}

export function startOnboardingTrial(configuration: BackOfficeConfiguration, tierId: string) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding/trial`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ tierId })
  });
}

export function createOnboardingVenue(
  configuration: BackOfficeConfiguration,
  venue: { name: string; timezone: string; type: string; primaryLanguage: string; secondaryLanguage?: string }
) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding/venue`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(venue)
  });
}

export function claimOnboardingFirstScreen(configuration: BackOfficeConfiguration, code: string) {
  return request<CustomerOnboardingSnapshot>(`${configuration.apiBaseUrl}/api/customer-onboarding/first-screen`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ code })
  });
}

export async function createOnboardingCheckout(
  configuration: BackOfficeConfiguration,
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

export function revokeCustomerSession(configuration: BackOfficeConfiguration) {
  return request<void>(`${configuration.apiBaseUrl}/api/customer-auth/session`, { method: "DELETE" });
}

export function externalSignInUrl(configuration: BackOfficeConfiguration, provider: "google" | "apple", returnPath = "/onboarding") {
  return `${configuration.apiBaseUrl}/api/customer-auth/external/${provider}?returnPath=${encodeURIComponent(returnPath)}`;
}

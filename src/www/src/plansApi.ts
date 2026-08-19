import { apiBaseUrl } from "./config";

export type PublicOnboardingPlan = {
  id: string;
  name: string;
  slug: string;
  monthlyPrice: number;
  trialDays: number;
  maxVenues: number;
  maxScreens: number;
};

export async function loadPublicPlans(signal?: AbortSignal): Promise<PublicOnboardingPlan[]> {
  const response = await fetch(`${apiBaseUrl}/api/customer-onboarding/plans`, { signal });
  if (!response.ok) return [];
  return response.json() as Promise<PublicOnboardingPlan[]>;
}

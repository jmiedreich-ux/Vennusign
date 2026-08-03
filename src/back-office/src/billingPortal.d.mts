import type { BackOfficeSubscriptionSummary } from './api';

export type SubscriptionStatusCopy = {
  tone: 'neutral' | 'scheduled' | 'trial' | 'attention' | 'ended' | 'active';
  title: string;
  detail: string;
};

export function requireHostedBillingPortalUrl(value: string): string;
export function subscriptionStatusCopy(subscription?: BackOfficeSubscriptionSummary): SubscriptionStatusCopy;

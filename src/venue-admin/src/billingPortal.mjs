export function requireHostedBillingPortalUrl(value) {
  const url = new URL(value);
  if (url.protocol !== 'https:' || url.hostname.toLowerCase() !== 'billing.stripe.com') {
    throw new Error('Billing management did not return an approved hosted URL.');
  }
  return url.href;
}

function formatDate(value) {
  if (!value) return undefined;
  const date = new Date(value);
  if (Number.isNaN(date.valueOf())) return undefined;
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    timeZone: 'UTC'
  }).format(date);
}

export function subscriptionStatusCopy(subscription) {
  if (!subscription) {
    return {
      tone: 'neutral',
      title: 'Subscription setup',
      detail: 'Your venue does not have an active self-service billing subscription yet.'
    };
  }

  const periodEnd = formatDate(subscription.currentPeriodEnd);
  if (subscription.cancelAtPeriodEnd) {
    return {
      tone: 'scheduled',
      title: 'Plan change scheduled',
      detail: periodEnd
        ? `Your current plan remains available through ${periodEnd}. Manage the scheduled change in Stripe.`
        : 'A plan change is scheduled for the end of the current billing period.'
    };
  }

  if (subscription.status === 'trialing') {
    const trialEnd = formatDate(subscription.trialEndsAt);
    return {
      tone: 'trial',
      title: 'Trial active',
      detail: trialEnd ? `Your trial runs through ${trialEnd}.` : 'Your trial is active.'
    };
  }

  if (subscription.status === 'past_due') {
    return {
      tone: 'attention',
      title: 'Payment needs attention',
      detail: 'Open secure billing management to update your payment method and protect service continuity.'
    };
  }

  if (subscription.status === 'canceled') {
    return {
      tone: 'ended',
      title: 'Subscription ended',
      detail: 'Contact support or choose an available plan to restore subscription access.'
    };
  }

  return {
    tone: 'active',
    title: 'Plan active',
    detail: periodEnd ? `Your current billing period runs through ${periodEnd}.` : 'Your subscription is active.'
  };
}

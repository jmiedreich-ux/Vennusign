import type { VenueAdminSubscriptionSummary, VenueAdminTierSummary } from "./api";
import { subscriptionStatusCopy } from "./billingPortal.mjs";

type Props = {
  currentTier?: VenueAdminTierSummary;
  subscription?: VenueAdminSubscriptionSummary;
  isOpening: boolean;
  error?: string;
  onManage: () => void;
};

export default function BillingStatusCard({ currentTier, subscription, isOpening, error, onManage }: Props) {
  const status = subscriptionStatusCopy(subscription);
  return <section className={`billing-status billing-status--${status.tone}`} aria-labelledby="billing-status-title">
    <div className="billing-status__heading">
      <div><p>Current plan</p><h2>{currentTier?.name ?? "Not assigned"}</h2></div>
      <span>{subscription?.status.replace("_", " ") ?? "setup"}</span>
    </div>
    <div>
      <strong id="billing-status-title">{status.title}</strong>
      <p>{status.detail}</p>
    </div>
    {error ? <p className="billing-status__error" role="alert">{error}</p> : null}
    {subscription?.canManageBilling
      ? <button type="button" onClick={onManage} disabled={isOpening}>
          {isOpening ? "Opening secure billing…" : "Manage billing in Stripe"}
        </button>
      : <p className="billing-status__support">Billing management is unavailable for this subscription state.</p>}
    <small>Payments and payment methods are managed securely by Stripe. Vennu does not collect card details.</small>
  </section>;
}

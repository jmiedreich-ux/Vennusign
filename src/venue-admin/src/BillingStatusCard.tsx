import type { VenueAdminBillingPresentation, VenueAdminHaasBundleSummary, VenueAdminSubscriptionSummary, VenueAdminTierSummary } from "./api";
import { subscriptionStatusCopy } from "./billingPortal.mjs";

type Props = {
  currentTier?: VenueAdminTierSummary;
  subscription?: VenueAdminSubscriptionSummary;
  isOpening: boolean;
  error?: string;
  onManage: () => void;
  haasBundles: VenueAdminBillingPresentation["haasBundles"];
  haasContract?: VenueAdminBillingPresentation["haasContract"];
  haasOpening?: string;
  haasError?: string;
  onStartHaas: (bundle: VenueAdminHaasBundleSummary) => void;
};

const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 });

export default function BillingStatusCard({ currentTier, subscription, isOpening, error, onManage, haasBundles, haasContract, haasOpening, haasError, onStartHaas }: Props) {
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
    <small>Payments and payment methods are managed securely by Stripe. Vennusign does not collect card details.</small>
    <div className="haas-billing" aria-labelledby="haas-billing-title">
      <div><p>Hardware as a Service</p><h3 id="haas-billing-title">Managed screen bundles</h3></div>
      {haasContract
        ? <div className="haas-contract">
            <strong>{haasContract.bundleName} · {haasContract.termMonths} months</strong>
            <p>{haasContract.remainingMonths} payments remain. Estimated remaining-term buyout: {currency.format(haasContract.estimatedBuyoutAmount)}.</p>
            <small>Disclosure only. Any early-cancel amount is acted on only after Stripe confirms the subscription event.</small>
          </div>
        : <div className="haas-bundles">
            {haasBundles.map(bundle => <article key={bundle.key}>
              <strong>{bundle.name}</strong>
              <p>{currency.format(bundle.monthlyAmount)}/month · {bundle.termMonths}-month term</p>
              <small>Transitions to {bundle.postContractTierSlug.replace("_", " ")} software service after the term.</small>
              <button type="button" onClick={() => onStartHaas(bundle)} disabled={Boolean(haasOpening)}>
                {haasOpening === bundle.key ? "Opening secure checkout…" : "Choose bundle"}
              </button>
            </article>)}
          </div>}
      {haasError ? <p className="billing-status__error" role="alert">{haasError}</p> : null}
    </div>
  </section>;
}

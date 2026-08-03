import type { BackOfficeBillingPresentation, BackOfficeHaasBundleSummary, BackOfficeSubscriptionSummary, BackOfficeTierSummary } from "./api";
import { subscriptionStatusCopy } from "./billingPortal.mjs";

type Props = {
  currentTier?: BackOfficeTierSummary;
  subscription?: BackOfficeSubscriptionSummary;
  isOpening: boolean;
  error?: string;
  onManage: () => void;
  usage: BackOfficeBillingPresentation["usage"];
  availableTiers: BackOfficeTierSummary[];
  onSelectTier: (tier: BackOfficeTierSummary) => void;
  haasBundles: BackOfficeBillingPresentation["haasBundles"];
  haasContract?: BackOfficeBillingPresentation["haasContract"];
  haasOpening?: string;
  haasError?: string;
  onStartHaas: (bundle: BackOfficeHaasBundleSummary) => void;
};

const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 });

const limit = (value: number) => value < 0 ? "Unlimited" : String(value);

export default function BillingStatusCard({ currentTier, subscription, isOpening, error, onManage, usage, availableTiers, onSelectTier, haasBundles, haasContract, haasOpening, haasError, onStartHaas }: Props) {
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
    <section className="billing-usage" aria-labelledby="billing-usage-title">
      <div><p>Plan usage</p><h3 id="billing-usage-title">Know the impact before changing tiers</h3></div>
      <dl><div><dt>Active screens</dt><dd>{usage.activeScreens} / {limit(usage.currentScreenLimit)}</dd></div><div><dt>Organization venues</dt><dd>{usage.organizationVenues} / {limit(usage.currentVenueLimit)}</dd></div></dl>
    </section>
    <section className="tier-comparison" aria-labelledby="tier-comparison-title">
      <div><p>Compare plans</p><h3 id="tier-comparison-title">Select a tier to review</h3></div>
      <div className="tier-comparison__grid">{availableTiers.map(tier => <article key={tier.id} className={tier.direction === "current" ? "current" : undefined}>
        <div><strong>{tier.name}</strong><span>{currency.format(tier.monthlyPrice)}/month</span></div>
        <ul><li>{limit(tier.maxScreens)} screens per venue</li><li>{limit(tier.maxVenues)} organization venues</li></ul>
        {tier.lostFeatures.length ? <p>{tier.lostFeatures.length} current feature{tier.lostFeatures.length === 1 ? "" : "s"} would be removed.</p> : <p>No current feature losses identified.</p>}
        {tier.blockingReasons.length ? <ul className="tier-comparison__blocks">{tier.blockingReasons.map(reason => <li key={reason}>{reason}</li>)}</ul> : null}
        <button type="button" disabled={!tier.canSelect} onClick={() => onSelectTier(tier)}>{tier.direction === "current" ? "Current plan" : tier.canSelect ? `Review ${tier.direction}` : "Resolve usage first"}</button>
      </article>)}</div>
    </section>
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

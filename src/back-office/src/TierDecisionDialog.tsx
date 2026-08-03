import { useEffect, useRef, useState } from "react";
import type { BackOfficeBillingUsage, BackOfficeTierSummary, CheckoutBillingInterval } from "./api";

type Props = {
  currentTier?: BackOfficeTierSummary;
  targetTier: BackOfficeTierSummary;
  usage: BackOfficeBillingUsage;
  usesPortal: boolean;
  isSubmitting: boolean;
  error?: string;
  onClose: () => void;
  onConfirm: (interval: CheckoutBillingInterval) => void;
};

const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 });
const limit = (value: number) => value < 0 ? "Unlimited" : String(value);

export default function TierDecisionDialog({ currentTier, targetTier, usage, usesPortal, isSubmitting, error, onClose, onConfirm }: Props) {
  const dialog = useRef<HTMLDialogElement>(null);
  const cancel = useRef<HTMLButtonElement>(null);
  const [interval, setInterval] = useState<CheckoutBillingInterval>("monthly");
  useEffect(() => {
    dialog.current?.showModal();
    cancel.current?.focus();
    return () => { if (dialog.current?.open) dialog.current.close(); };
  }, []);
  const direction = targetTier.direction === "downgrade" ? "downgrade" : targetTier.direction === "start" ? "start" : "upgrade";
  const amount = interval === "monthly" ? targetTier.monthlyPrice : targetTier.monthlyPrice * 10;

  return <dialog ref={dialog} className="tier-decision-dialog" aria-labelledby="tier-decision-title" onCancel={event => { event.preventDefault(); if (!isSubmitting) onClose(); }}>
    <p className="tier-decision-dialog__eyebrow">Review {direction}</p>
    <h2 id="tier-decision-title">{currentTier?.name ?? "No plan"} → {targetTier.name}</h2>
    <p>No plan or feature access changes when this dialog closes or when Stripe merely returns. Vennusign waits for authoritative webhook state.</p>
    <dl className="tier-decision-impact">
      <div><dt>Active screens</dt><dd>{usage.activeScreens} used · {limit(targetTier.maxScreens)} allowed</dd></div>
      <div><dt>Organization venues</dt><dd>{usage.organizationVenues} used · {limit(targetTier.maxVenues)} allowed</dd></div>
    </dl>
    {targetTier.lostFeatures.length ? <section className="tier-feature-loss" aria-labelledby="tier-feature-loss-title"><h3 id="tier-feature-loss-title">Features removed after confirmation</h3><ul>{targetTier.lostFeatures.map(feature => <li key={feature}>{feature}</li>)}</ul></section> : <p className="tier-decision-safe">No current feature losses were identified.</p>}
    {!usesPortal ? <fieldset className="upgrade-modal__billing"><legend>Billing interval</legend><label><input type="radio" name="tierBillingInterval" checked={interval === "monthly"} onChange={() => setInterval("monthly")} /> Monthly</label><label><input type="radio" name="tierBillingInterval" checked={interval === "annual"} onChange={() => setInterval("annual")} /> Annual · two months included</label></fieldset> : null}
    {!usesPortal ? <div className="upgrade-modal__price"><strong>{currency.format(amount)}</strong><span>/{interval === "monthly" ? "month" : "year"}</span></div> : <p>Stripe will show the final effective date, proration, and price before you confirm.</p>}
    {error ? <p className="upgrade-modal__error" role="alert">{error}</p> : null}
    <div className="tier-decision-actions">
      <button ref={cancel} type="button" className="upgrade-modal__later" onClick={onClose} disabled={isSubmitting}>Keep current plan</button>
      <button type="button" className="upgrade-modal__primary" onClick={() => onConfirm(interval)} disabled={isSubmitting || !targetTier.canSelect}>{isSubmitting ? "Opening secure Stripe…" : usesPortal ? "Continue to Stripe review" : "Continue to secure checkout"}</button>
    </div>
  </dialog>;
}

import { useEffect, useMemo, useRef, useState } from "react";
import type { BackOfficeTierSummary } from "./api";
import EntitlementLockChip from "./EntitlementLockChip";
import { upgradeFeaturePills, type UpgradeOpportunity } from "./upgradeExperience.mjs";

export type BillingInterval = "monthly" | "annual";

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  currentTier?: Pick<BackOfficeTierSummary, "name" | "slug">;
  targetTier: BackOfficeTierSummary;
  onClose: () => void;
  onUpgrade: (interval: BillingInterval) => void;
  isSubmitting?: boolean;
  error?: string;
};

const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 });

export default function UpgradeSheet({ opportunity, currentTier, targetTier, onClose, onUpgrade, isSubmitting = false, error }: Props) {
  const [interval, setInterval] = useState<BillingInterval>("monthly");
  const closeButton = useRef<HTMLButtonElement>(null);
  const closeAction = useRef(onClose);
  const submitting = useRef(isSubmitting);
  closeAction.current = onClose;
  submitting.current = isSubmitting;
  const features = useMemo(() => upgradeFeaturePills(opportunity.requiredTier), [opportunity.requiredTier]);
  const amount = interval === "monthly" ? targetTier.monthlyPrice : targetTier.monthlyPrice * 10;

  useEffect(() => {
    const previous = document.activeElement instanceof HTMLElement ? document.activeElement : undefined;
    closeButton.current?.focus();
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === "Escape" && !submitting.current) closeAction.current(); };
    window.addEventListener("keydown", closeOnEscape);
    return () => { window.removeEventListener("keydown", closeOnEscape); previous?.focus(); };
  }, []);

  return (
    <div className="upgrade-sheet-backdrop" role="presentation" onMouseDown={event => { if (event.target === event.currentTarget && !isSubmitting) onClose(); }}>
      <section className="upgrade-sheet" role="dialog" aria-modal="true" aria-labelledby="upgrade-sheet-title" aria-describedby="upgrade-sheet-benefit">
        <button ref={closeButton} className="upgrade-sheet__close" type="button" onClick={onClose} aria-label="Close upgrade options" disabled={isSubmitting}>×</button>
        <EntitlementLockChip opportunity={opportunity} compact />
        <p className="upgrade-sheet__eyebrow">Upgrade sheet</p>
        <h2 id="upgrade-sheet-title">Move from {currentTier?.name ?? "your current plan"} to {targetTier.name}</h2>
        <p id="upgrade-sheet-benefit">{opportunity.benefit}</p>
        <div className="upgrade-sheet__features" aria-label={`${targetTier.name} features`}>
          {features.map(feature => <span key={feature}>{feature}</span>)}
        </div>
        <fieldset className="upgrade-sheet__billing">
          <legend>Billing interval</legend>
          <label><input type="radio" name="billingInterval" checked={interval === "monthly"} onChange={() => setInterval("monthly")} /> Monthly</label>
          <label><input type="radio" name="billingInterval" checked={interval === "annual"} onChange={() => setInterval("annual")} /> Annual · two months included</label>
        </fieldset>
        <div className="upgrade-sheet__price"><strong>{currency.format(amount)}</strong><span>/{interval === "monthly" ? "month" : "year"}</span></div>
        {error ? <p className="upgrade-sheet__error" role="alert">{error}</p> : null}
        <button className="upgrade-sheet__primary" type="button" onClick={() => onUpgrade(interval)} disabled={isSubmitting}>
          {isSubmitting ? "Opening secure checkout…" : `Upgrade to ${targetTier.name}`}
        </button>
        <button className="upgrade-sheet__later" type="button" onClick={onClose} disabled={isSubmitting}>Maybe later</button>
      </section>
    </div>
  );
}

import { useEffect, useMemo, useState } from "react";
import type { SubscriptionTier } from "./api";
import TierBadge from "./TierBadge";
import { upgradeFeaturePills, type UpgradeOpportunity } from "./upgradeExperience.mjs";

export type BillingInterval = "monthly" | "annual";

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  currentTier?: Pick<SubscriptionTier, "name" | "slug">;
  targetTier: SubscriptionTier;
  onClose: () => void;
  onUpgrade: (interval: BillingInterval) => void;
};

const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 });

export default function UpgradeModal({ opportunity, currentTier, targetTier, onClose, onUpgrade }: Props) {
  const [interval, setInterval] = useState<BillingInterval>("monthly");
  const features = useMemo(() => upgradeFeaturePills(opportunity.requiredTier), [opportunity.requiredTier]);
  const amount = interval === "monthly" ? targetTier.price : targetTier.price * 10;

  useEffect(() => {
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === "Escape") onClose(); };
    window.addEventListener("keydown", closeOnEscape);
    return () => window.removeEventListener("keydown", closeOnEscape);
  }, [onClose]);

  return (
    <div className="upgrade-modal-backdrop" role="presentation" onMouseDown={event => { if (event.target === event.currentTarget) onClose(); }}>
      <section className="upgrade-modal" role="dialog" aria-modal="true" aria-labelledby="upgrade-modal-title">
        <button className="upgrade-modal__close" type="button" onClick={onClose} aria-label="Close upgrade options">×</button>
        <TierBadge tier={opportunity.requiredTier} />
        <p className="upgrade-modal__eyebrow">Unlock {opportunity.title}</p>
        <h2 id="upgrade-modal-title">Move from {currentTier?.name ?? "your current plan"} to {targetTier.name}</h2>
        <p>{opportunity.benefit}</p>
        <div className="upgrade-modal__features" aria-label={`${targetTier.name} features`}>
          {features.map(feature => <span key={feature}>{feature}</span>)}
        </div>
        <fieldset className="upgrade-modal__billing">
          <legend>Billing interval</legend>
          <label><input type="radio" name="billingInterval" checked={interval === "monthly"} onChange={() => setInterval("monthly")} /> Monthly</label>
          <label><input type="radio" name="billingInterval" checked={interval === "annual"} onChange={() => setInterval("annual")} /> Annual · two months included</label>
        </fieldset>
        <div className="upgrade-modal__price"><strong>{currency.format(amount)}</strong><span>/{interval === "monthly" ? "month" : "year"}</span></div>
        <button className="upgrade-modal__primary" type="button" onClick={() => onUpgrade(interval)}>Upgrade to {targetTier.name}</button>
        <button className="upgrade-modal__later" type="button" onClick={onClose}>Maybe later</button>
      </section>
    </div>
  );
}

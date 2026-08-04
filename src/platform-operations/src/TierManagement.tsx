import { useEffect, useState, type FormEvent } from "react";
import { archiveTier, cloneTier, loadTiers, saveTier, type SubscriptionTier, type TierManagementRequest } from "./api";
import type { PlatformOperationsConfiguration } from "./config";
import TransientFeedback from "./TransientFeedback";

const empty: TierManagementRequest = { name: "", slug: "", price: 0, maxScreens: 1, isPublic: false, isActive: false };
type Props = { configuration: PlatformOperationsConfiguration; apiKey: string };
type Pending = { kind: "save"; request: TierManagementRequest; tierId?: string; previous?: SubscriptionTier } | { kind: "archive"; tier: SubscriptionTier };

export default function TierManagement({ configuration, apiKey }: Props) {
  const [tiers, setTiers] = useState<SubscriptionTier[]>([]);
  const [draft, setDraft] = useState<TierManagementRequest>(empty);
  const [editingId, setEditingId] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [busy, setBusy] = useState(true);
  const [pending, setPending] = useState<Pending>();
  const refresh = () => loadTiers(configuration, apiKey).then(setTiers);
  useEffect(() => { refresh().catch(() => setError("Tiers could not be loaded.")).finally(() => setBusy(false)); }, [apiKey, configuration]);
  const edit = (tier: SubscriptionTier) => { const { id, ...request } = tier; setEditingId(id); setDraft(request); setError(undefined); setNotice(undefined); };
  const submit = (event: FormEvent) => { event.preventDefault(); setError(undefined); setNotice(undefined); setPending({ kind: "save", request: { ...draft }, tierId: editingId, previous: tiers.find(tier => tier.id === editingId) }); };
  const run = async (action: () => Promise<unknown>, success: string) => { setBusy(true); setError(undefined); setNotice(undefined); try { await action(); await refresh(); setPending(undefined); setEditingId(undefined); setDraft(empty); setNotice(success); } catch { setError("The tier action could not be completed. Review the current catalog and Stripe mapping before retrying."); } finally { setBusy(false); } };
  const confirm = () => pending?.kind === "archive" ? run(() => archiveTier(configuration, apiKey, pending.tier.id), `${pending.tier.name} archived.`) : pending ? run(() => saveTier(configuration, apiKey, pending.request, pending.tierId), pending.tierId ? `${pending.request.name} updated.` : `${pending.request.name} created.`) : Promise.resolve();

  return <section className="tier-management">
    <form className="tier-form" onSubmit={submit}><div><p>Catalog editor</p><h2>{editingId ? "Edit tier" : "Create tier"}</h2></div><label>Name<input required value={draft.name} onChange={e => setDraft({ ...draft, name: e.target.value })} /></label><label>Slug<input required value={draft.slug} onChange={e => setDraft({ ...draft, slug: e.target.value })} /></label><label>Price<input required min="0" step="0.01" type="number" value={draft.price} onChange={e => setDraft({ ...draft, price: Number(e.target.value) })} /></label><label>Max screens<input required type="number" value={draft.maxScreens} onChange={e => setDraft({ ...draft, maxScreens: Number(e.target.value) })} /></label><label>Stripe product<input value={draft.stripeProductId ?? ""} onChange={e => setDraft({ ...draft, stripeProductId: e.target.value || undefined })} /></label><label>Monthly price<input value={draft.stripeMonthlyPriceId ?? ""} onChange={e => setDraft({ ...draft, stripeMonthlyPriceId: e.target.value || undefined })} /></label><label>Annual price<input value={draft.stripeAnnualPriceId ?? ""} onChange={e => setDraft({ ...draft, stripeAnnualPriceId: e.target.value || undefined })} /></label><div className="checks"><label><input type="checkbox" checked={draft.isPublic} onChange={e => setDraft({ ...draft, isPublic: e.target.checked })} /> Public</label><label><input type="checkbox" checked={draft.isActive} onChange={e => setDraft({ ...draft, isActive: e.target.checked })} /> Active</label></div><div className="form-actions"><button disabled={busy} type="submit">{editingId ? "Review changes" : "Review new tier"}</button>{editingId ? <button type="button" onClick={() => { setEditingId(undefined); setDraft(empty); setPending(undefined); }}>Cancel</button> : null}</div></form>
    <div className="tier-catalog">
      {error ? <p className="state error" role="alert">{error}</p> : null}{notice ? <TransientFeedback message={notice} onDismiss={() => setNotice(undefined)} /> : null}
      {pending ? <section className="impact-preview" aria-labelledby="tier-impact-title"><p>Tier catalog review</p><h3 id="tier-impact-title">{pending.kind === "archive" ? `Archive ${pending.tier.name}?` : `${pending.tierId ? "Update" : "Create"} ${pending.request.name}?`}</h3>{pending.kind === "archive" ? <p>The tier will no longer be available for new assignments. Existing subscriptions are not silently moved; operational reconciliation remains visible.</p> : <ul><li>Price: {pending.previous ? `$${pending.previous.price.toFixed(2)} → ` : ""}${pending.request.price.toFixed(2)}</li><li>Screen limit: {pending.previous ? `${pending.previous.maxScreens} → ` : ""}{pending.request.maxScreens === -1 ? "Unlimited" : pending.request.maxScreens}</li><li>{pending.request.isPublic ? "Public" : "Internal"} · {pending.request.isActive ? "Active" : "Inactive"}</li></ul>}<div><button type="button" onClick={() => setPending(undefined)} disabled={busy}>Cancel</button><button className="danger" type="button" onClick={confirm} disabled={busy}>{busy ? "Applying…" : "Confirm tier action"}</button></div></section> : null}
      {busy && tiers.length === 0 ? <p className="state">Loading tiers…</p> : <div className="tier-cards">{tiers.map(tier => <article key={tier.id}><div><span className={`health ${tier.isActive ? "online" : "offline"}`}>{tier.isActive ? "Active" : "Archived"}</span><h3>{tier.name}</h3><p>{tier.slug} · ${tier.price.toFixed(2)} · {tier.maxScreens === -1 ? "Unlimited screens" : `${tier.maxScreens} screens`}</p></div><div className="tier-actions"><button onClick={() => edit(tier)}>Edit</button><button onClick={() => run(() => cloneTier(configuration, apiKey, tier.id), `${tier.name} cloned for review.`)}>Clone</button>{tier.isActive ? <button onClick={() => setPending({ kind: "archive", tier })}>Review archive</button> : null}</div></article>)}</div>}
    </div>
  </section>;
}

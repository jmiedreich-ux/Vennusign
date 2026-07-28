import { useEffect, useState, type FormEvent } from "react";
import { archiveTier, cloneTier, loadTiers, saveTier, type SubscriptionTier, type TierManagementRequest } from "./api";
import type { AdminConfiguration } from "./config";

const empty: TierManagementRequest = { name: "", slug: "", price: 0, maxScreens: 1, isPublic: false, isActive: false };
type Props = { configuration: AdminConfiguration; apiKey: string };

export default function TierManagement({ configuration, apiKey }: Props) {
  const [tiers, setTiers] = useState<SubscriptionTier[]>([]);
  const [draft, setDraft] = useState<TierManagementRequest>(empty);
  const [editingId, setEditingId] = useState<string>();
  const [error, setError] = useState<string>();
  const [busy, setBusy] = useState(true);
  const refresh = () => loadTiers(configuration, apiKey).then(setTiers);

  useEffect(() => { refresh().catch(() => setError("Tiers could not be loaded.")).finally(() => setBusy(false)); }, [apiKey, configuration]);
  const edit = (tier: SubscriptionTier) => {
    const { id, ...request } = tier; setEditingId(id); setDraft(request); setError(undefined);
  };
  const submit = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined);
    try { await saveTier(configuration, apiKey, draft, editingId); await refresh(); setEditingId(undefined); setDraft(empty); }
    catch { setError("The tier could not be saved. Check the values and slug."); }
    finally { setBusy(false); }
  };
  const mutate = async (action: () => Promise<unknown>) => {
    setBusy(true); setError(undefined);
    try { await action(); await refresh(); } catch { setError("The tier action could not be completed."); } finally { setBusy(false); }
  };

  return <section className="tier-management">
    <form className="tier-form" onSubmit={submit}>
      <div><p>Catalog editor</p><h2>{editingId ? "Edit tier" : "Create tier"}</h2></div>
      <label>Name<input required value={draft.name} onChange={e => setDraft({ ...draft, name: e.target.value })} /></label>
      <label>Slug<input required value={draft.slug} onChange={e => setDraft({ ...draft, slug: e.target.value })} /></label>
      <label>Price<input required min="0" step="0.01" type="number" value={draft.price} onChange={e => setDraft({ ...draft, price: Number(e.target.value) })} /></label>
      <label>Max screens<input required type="number" value={draft.maxScreens} onChange={e => setDraft({ ...draft, maxScreens: Number(e.target.value) })} /></label>
      <label>Stripe product<input value={draft.stripeProductId ?? ""} onChange={e => setDraft({ ...draft, stripeProductId: e.target.value || undefined })} /></label>
      <label>Monthly price<input value={draft.stripeMonthlyPriceId ?? ""} onChange={e => setDraft({ ...draft, stripeMonthlyPriceId: e.target.value || undefined })} /></label>
      <label>Annual price<input value={draft.stripeAnnualPriceId ?? ""} onChange={e => setDraft({ ...draft, stripeAnnualPriceId: e.target.value || undefined })} /></label>
      <div className="checks"><label><input type="checkbox" checked={draft.isPublic} onChange={e => setDraft({ ...draft, isPublic: e.target.checked })} /> Public</label><label><input type="checkbox" checked={draft.isActive} onChange={e => setDraft({ ...draft, isActive: e.target.checked })} /> Active</label></div>
      <div className="form-actions"><button disabled={busy} type="submit">{editingId ? "Save changes" : "Create tier"}</button>{editingId && <button type="button" onClick={() => { setEditingId(undefined); setDraft(empty); }}>Cancel</button>}</div>
    </form>
    {error && <p className="state error">{error}</p>}
    {busy && tiers.length === 0 ? <p className="state">Loading tiers…</p> : <div className="tier-cards">{tiers.map(tier => <article key={tier.id}>
      <div><span className={`health ${tier.isActive ? "online" : "offline"}`}>{tier.isActive ? "Active" : "Archived"}</span><h3>{tier.name}</h3><p>{tier.slug} · ${tier.price.toFixed(2)} · {tier.maxScreens === -1 ? "Unlimited screens" : `${tier.maxScreens} screens`}</p></div>
      <div className="tier-actions"><button onClick={() => edit(tier)}>Edit</button><button onClick={() => mutate(() => cloneTier(configuration, apiKey, tier.id))}>Clone</button>{tier.isActive && <button onClick={() => mutate(() => archiveTier(configuration, apiKey, tier.id))}>Archive</button>}</div>
    </article>)}</div>}
  </section>;
}

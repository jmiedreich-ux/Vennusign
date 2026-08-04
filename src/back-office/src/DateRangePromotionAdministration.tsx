import { useEffect, useState, type FormEvent } from "react";
import { archiveDateRangePromotion, loadDateRangePromotions, saveDateRangePromotion, type DateRangePromotion } from "./api";
import type { BackOfficeConfiguration } from "./config";
import { useDestructiveReview } from "./DestructiveReviewDialog";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; enabled: boolean; showUpgradePrompt?: boolean };
type Draft = Omit<DateRangePromotion, "id" | "venueId">;
const today = new Date().toISOString().slice(0, 10);
const initial: Draft = { name: "", startLocalDate: today, endLocalDate: today, priority: 0, isEnabled: true };

export default function DateRangePromotionAdministration({ configuration, apiKey, venueId, enabled, showUpgradePrompt = true }: Props) {
  const [rows, setRows] = useState<DateRangePromotion[]>([]);
  const [draft, setDraft] = useState<Draft>(initial);
  const [editingId, setEditingId] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [busy, setBusy] = useState(false);
  const { review, reviewDialog } = useDestructiveReview();
  const refresh = () => loadDateRangePromotions(configuration, apiKey, venueId).then(setRows);
  useEffect(() => { refresh().catch(() => setError("Promotions could not be loaded.")); }, [apiKey, configuration, venueId]);
  const save = async (event: FormEvent) => {
    event.preventDefault(); if (!enabled) return; setBusy(true); setError(undefined); setNotice(undefined);
    try { await saveDateRangePromotion(configuration, apiKey, venueId, draft, editingId); setDraft(initial); setEditingId(undefined); await refresh(); setNotice("Promotion saved. The server will apply the highest-priority eligible promotion in venue-local time."); }
    catch { setError("The promotion could not be saved."); } finally { setBusy(false); }
  };
  const archive = async (id: string) => {
    const row = rows.find(item => item.id === id);
    if (!row || !await review({ title: `Archive ${row.name}?`, consequence: "This promotion will stop being eligible for display. Its existing record remains available for audit history.", confirmLabel: "Archive promotion", tone: "caution" })) return;
    setBusy(true); setError(undefined); setNotice(undefined); try { await archiveDateRangePromotion(configuration, apiKey, venueId, id); await refresh(); setNotice(`${row.name} archived.`); }
    catch { setError("The promotion could not be archived."); } finally { setBusy(false); }
  };
  const edit = (row: DateRangePromotion) => {
    setEditingId(row.id);
    setDraft({
      name: row.name,
      startLocalDate: row.startLocalDate.slice(0, 10),
      endLocalDate: row.endLocalDate.slice(0, 10),
      targetLayout: row.targetLayout,
      title: row.title,
      body: row.body,
      priority: row.priority,
      isEnabled: row.isEnabled
    });
  };
  const cancelEdit = () => { setEditingId(undefined); setDraft(initial); };
  return <article className="promotion-admin">
    {reviewDialog}
    <div className="promotion-heading"><div><p>Scheduling</p><h3>Date-range promotions</h3></div><span>{rows.filter(row => row.isEnabled).length} enabled</span></div>
    {showUpgradePrompt && !enabled ? <p className="tier-notice">Promotion scheduling is visible as a preview. Enable Basic Scheduling to edit it.</p> : null}
    <p>Dates use the venue timezone. When eligible promotions overlap, the highest numeric priority wins; ties are resolved deterministically by the server.</p>
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {notice ? <p className="state success" role="status">{notice}</p> : null}
    <form onSubmit={save}>
      <input aria-label="Promotion name" disabled={!enabled} maxLength={160} required placeholder="Holiday menu" value={draft.name} onChange={event => setDraft(value => ({ ...value, name: event.target.value }))} />
      <label>Start<input disabled={!enabled} required type="date" value={draft.startLocalDate.slice(0, 10)} onChange={event => setDraft(value => ({ ...value, startLocalDate: event.target.value }))} /></label>
      <label>End<input disabled={!enabled} min={draft.startLocalDate.slice(0, 10)} required type="date" value={draft.endLocalDate.slice(0, 10)} onChange={event => setDraft(value => ({ ...value, endLocalDate: event.target.value }))} /></label>
      <select aria-label="Promotion layout" disabled={!enabled} value={draft.targetLayout ?? ""} onChange={event => setDraft(value => ({ ...value, targetLayout: event.target.value || undefined }))}>
        <option value="">Keep screen layout</option><option value="photo_grid">Photo Grid</option><option value="classic_diner">Classic Diner</option><option value="neon_chalkboard">Neon Chalkboard</option><option value="split_layout">Split</option><option value="daily_special_hero">Daily Special Hero</option>
      </select>
      <input aria-label="Promotion title" disabled={!enabled} maxLength={200} placeholder="Seasonal special" value={draft.title ?? ""} onChange={event => setDraft(value => ({ ...value, title: event.target.value }))} />
      <textarea aria-label="Promotion body" disabled={!enabled} maxLength={1000} placeholder="Limited-time message" value={draft.body ?? ""} onChange={event => setDraft(value => ({ ...value, body: event.target.value }))} />
      <input aria-label="Promotion priority" disabled={!enabled} max={1000} min={-1000} type="number" value={draft.priority} onChange={event => setDraft(value => ({ ...value, priority: Number(event.target.value) }))} />
      <button disabled={!enabled || busy}>{editingId ? "Save promotion" : "Add promotion"}</button>
      {editingId ? <button type="button" disabled={busy} onClick={cancelEdit}>Cancel</button> : null}
    </form>
    <ul>{rows.map(row => <li key={row.id}><div><strong>{row.name}</strong><span>{row.startLocalDate.slice(0, 10)} through {row.endLocalDate.slice(0, 10)} · priority {row.priority}{row.targetLayout ? ` · ${row.targetLayout}` : ""}</span></div>{row.isEnabled ? <div className="promotion-actions"><button disabled={!enabled || busy} onClick={() => edit(row)}>Edit</button><button disabled={!enabled || busy} onClick={() => archive(row.id)}>Archive</button></div> : <span>Archived</span>}</li>)}</ul>
  </article>;
}

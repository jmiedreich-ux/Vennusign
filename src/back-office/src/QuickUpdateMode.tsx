import { useMemo, useState } from "react";
import { updateQuickAvailability, type MenuEditorSnapshot } from "./api";
import type { BackOfficeConfiguration } from "./config";
import TransientFeedback from "./TransientFeedback";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; snapshot: MenuEditorSnapshot; menuId: string; onChanged: () => Promise<void> };
type QuickItem = { sectionId: string; sectionName: string; id: string; name: string; isAvailable: boolean; isActive: boolean };
type UndoEntry = { sectionId: string; itemId: string; isAvailable: boolean };
const bulkLimit = 25;

export default function QuickUpdateMode({ configuration, apiKey, venueId, snapshot, menuId, onChanged }: Props) {
  const menu = snapshot.menus.find(entry => entry.menu.id === menuId)?.menu;
  const sections = snapshot.menus.find(entry => entry.menu.id === menuId)?.sections ?? [];
  const [search, setSearch] = useState("");
  const [sectionFilter, setSectionFilter] = useState("");
  const [availabilityFilter, setAvailabilityFilter] = useState<"all" | "live" | "off">("all");
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [undo, setUndo] = useState<UndoEntry[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const items = useMemo(() => sections.flatMap(section => (snapshot.itemGroups.find(group => group.sectionId === section.id)?.items ?? []).map(item => ({ sectionId: section.id, sectionName: section.name, id: item.id, name: item.name, isAvailable: item.isAvailable, isActive: item.isActive }))).filter(item => item.isActive), [sections, snapshot.itemGroups]);
  const visible = items.filter(item => (!search.trim() || item.name.toLowerCase().includes(search.trim().toLowerCase())) && (!sectionFilter || item.sectionId === sectionFilter) && (availabilityFilter === "all" || (availabilityFilter === "live" ? item.isAvailable : !item.isAvailable)));
  if (!menu || !snapshot.capabilities.quickUpdate) return null;

  const toggle = async (item: QuickItem) => { setBusy(true); setError(undefined); setNotice(undefined); try { await updateQuickAvailability(configuration, apiKey, venueId, menu.id, item.sectionId, item.id, !item.isAvailable); setUndo([{ sectionId: item.sectionId, itemId: item.id, isAvailable: item.isAvailable }]); await onChanged(); setNotice(`${item.name} marked ${item.isAvailable ? "off" : "live"}.`); } catch { setError("Availability could not be updated. Retry the unchanged item."); } finally { setBusy(false); } };
  const bulk = async (isAvailable: boolean) => {
    const targets = items.filter(item => selected.has(item.id)).slice(0, bulkLimit);
    if (!targets.length) return;
    setBusy(true); setError(undefined); setNotice(undefined);
    try { for (const item of targets) await updateQuickAvailability(configuration, apiKey, venueId, menu.id, item.sectionId, item.id, isAvailable); setUndo(targets.map(item => ({ sectionId: item.sectionId, itemId: item.id, isAvailable: item.isAvailable }))); setSelected(new Set()); await onChanged(); setNotice(`${targets.length} items marked ${isAvailable ? "live" : "off"}. Undo is available until the next change.`); }
    catch { setError("The bulk availability change did not complete. Refresh to verify current item state before retrying."); await onChanged(); }
    finally { setBusy(false); }
  };
  const undoLast = async () => { if (!undo.length) return; setBusy(true); setError(undefined); try { for (const item of undo) await updateQuickAvailability(configuration, apiKey, venueId, menu.id, item.sectionId, item.itemId, item.isAvailable); setUndo([]); await onChanged(); setNotice("The last availability change was restored."); } catch { setError("Undo did not complete. Refresh and verify each selected item."); await onChanged(); } finally { setBusy(false); } };
  const selectVisible = () => setSelected(new Set(visible.slice(0, bulkLimit).map(item => item.id)));

  return <section className="quick-update">
    <div><p>Mobile service controls</p><h3>Quick Update</h3><span>An item stays off until someone turns it back on. Bulk changes are limited to {bulkLimit} items.</span></div>
    {error ? <p className="state error" role="alert">{error}</p> : null}{notice ? <TransientFeedback message={notice} onDismiss={() => setNotice(undefined)} /> : null}
    <div className="quick-filter-bar"><input type="search" aria-label="Search quick-update items" placeholder="Search items" value={search} onChange={event => setSearch(event.target.value)} /><select aria-label="Filter quick-update section" value={sectionFilter} onChange={event => setSectionFilter(event.target.value)}><option value="">All sections</option>{sections.map(section => <option key={section.id} value={section.id}>{section.name}</option>)}</select><select aria-label="Filter quick-update availability" value={availabilityFilter} onChange={event => setAvailabilityFilter(event.target.value as typeof availabilityFilter)}><option value="all">All availability</option><option value="live">Live only</option><option value="off">Off only</option></select></div>
    <div className="bulk-toolbar"><span role="status">{visible.length} results · {selected.size} selected</span><button type="button" disabled={busy || visible.length === 0} onClick={selectVisible}>Select visible (max {bulkLimit})</button><button type="button" disabled={busy || selected.size === 0} onClick={() => void bulk(true)}>Mark selected live</button><button type="button" disabled={busy || selected.size === 0} onClick={() => void bulk(false)}>Mark selected off</button>{undo.length ? <button type="button" disabled={busy} onClick={() => void undoLast()}>Undo last change</button> : null}</div>
    {visible.length ? <div className="quick-items">{visible.map(item => <div className={item.isAvailable ? "quick-item" : "quick-item off"} key={item.id}><label><input type="checkbox" checked={selected.has(item.id)} disabled={busy} onChange={event => setSelected(current => { const next = new Set(current); if (event.target.checked && next.size < bulkLimit) next.add(item.id); else next.delete(item.id); return next; })} /><span className="sr-only">Select {item.name}</span></label><button type="button" disabled={busy} onClick={() => void toggle(item)}><span><small>{item.sectionName}</small><strong>{item.name}</strong></span><span>{item.isAvailable ? "Live" : "Off"}</span></button></div>)}</div> : <p className="state">No active items match these filters.</p>}
  </section>;
}

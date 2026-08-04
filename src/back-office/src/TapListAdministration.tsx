import { useEffect, useMemo, useState, type FormEvent } from "react";
import {
  deleteTapCategory, deleteTapItem, loadTapList, reorderTapRows, saveTapCategory, saveTapItem,
  type TapCategory, type TapItem, type TapListSnapshot
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import { useDestructiveReview } from "./DestructiveReviewDialog";
import TransientFeedback from "./TransientFeedback";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; enabled: boolean; showUpgradePrompt?: boolean };
const newItem = (): Omit<TapItem, "id" | "venueId" | "sortOrder"> => ({
  name: "", description: "", price: 0, isAvailable: true, isComingSoon: false, glassColor: "#F5C842", nameColor: "#FFD700"
});
const tapStripsCapacity = 12;
const bulkLimit = 25;

export default function TapListAdministration({ configuration, apiKey, venueId, enabled, showUpgradePrompt = true }: Props) {
  const [data, setData] = useState<TapListSnapshot>({ categories: [], items: [] });
  const [category, setCategory] = useState({ name: "", categoryPrice: undefined as number | undefined, isActive: true });
  const [item, setItem] = useState(newItem());
  const [query, setQuery] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [selected, setSelected] = useState<string[]>([]);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [retry, setRetry] = useState<(() => Promise<unknown>)>();
  const { review, reviewDialog } = useDestructiveReview();
  const refresh = () => loadTapList(configuration, apiKey, venueId).then(setData);
  useEffect(() => { refresh().catch(() => setError("Tap list could not be loaded. Retry after checking the venue connection.")); }, [apiKey, configuration, venueId]);

  const run = async (operation: () => Promise<unknown>, success: string) => {
    setBusy(true); setError(undefined); setNotice(undefined); setRetry(undefined);
    try { await operation(); await refresh(); setNotice(success); }
    catch { setError("The tap-list change could not be saved or pushed. Current server data remains authoritative."); setRetry(() => operation); }
    finally { setBusy(false); }
  };
  const retryLast = () => retry && void run(retry, "The tap-list change was saved on retry and queued for venue screens.");
  const addCategory = (event: FormEvent) => {
    event.preventDefault(); if (!enabled) return;
    void run(async () => { await saveTapCategory(configuration, apiKey, venueId, category); setCategory({ name: "", categoryPrice: undefined, isActive: true }); }, "Category saved and queued for venue screens.");
  };
  const addItem = (event: FormEvent) => {
    event.preventDefault(); if (!enabled) return;
    void run(async () => { await saveTapItem(configuration, apiKey, venueId, item); setItem(newItem()); }, "Tap saved and queued for venue screens.");
  };
  const move = (kind: "categories" | "items", id: string, offset: number) => {
    const rows = [...data[kind]];
    const from = rows.findIndex(row => row.id === id); const to = from + offset;
    if (from < 0 || to < 0 || to >= rows.length) return;
    [rows[from], rows[to]] = [rows[to], rows[from]];
    void run(() => reorderTapRows(configuration, apiKey, venueId, kind, rows.map(row => row.id)), `${kind === "items" ? "Tap" : "Category"} order saved and queued for venue screens.`);
  };
  const patchItem = (id: string, value: Partial<TapItem>) =>
    setData(current => ({ ...current, items: current.items.map(row => row.id === id ? { ...row, ...value } : row) }));
  const patchCategory = (id: string, value: Partial<TapCategory>) =>
    setData(current => ({ ...current, categories: current.categories.map(row => row.id === id ? { ...row, ...value } : row) }));
  const removeCategory = async (row: TapCategory) => {
    const dependencyCount = data.items.filter(value => value.tapCategoryId === row.id).length;
    if (dependencyCount) { setError(`${row.name} contains ${dependencyCount} tap${dependencyCount === 1 ? "" : "s"}. Move or delete them before deleting the category.`); return; }
    if (!await review({ title: `Delete category “${row.name}”?`, consequence: "This empty category will be permanently deleted. It cannot be restored.", confirmLabel: "Delete category" })) return;
    void run(() => deleteTapCategory(configuration, apiKey, venueId, row.id), `${row.name} deleted and screens queued to refresh.`);
  };
  const removeItem = async (row: TapItem) => {
    const position = data.items.findIndex(value => value.id === row.id) + 1;
    if (!await review({ title: `Delete “${row.name}”?`, consequence: `This tap will be permanently removed from position ${position}, and venue screens will be queued to refresh.`, confirmLabel: "Delete tap" })) return;
    void run(() => deleteTapItem(configuration, apiKey, venueId, row.id), `${row.name} deleted and screens queued to refresh.`);
  };
  const toggleSelected = (id: string) => setSelected(current => current.includes(id) ? current.filter(value => value !== id) : current.length < bulkLimit ? [...current, id] : current);
  const bulkAvailability = (isAvailable: boolean) => {
    const targets = data.items.filter(row => selected.includes(row.id));
    if (!targets.length || targets.length > bulkLimit) return;
    void run(async () => {
      for (const row of targets) await saveTapItem(configuration, apiKey, venueId, { ...row, isAvailable }, row.id);
      setSelected([]);
    }, `${targets.length} tap${targets.length === 1 ? "" : "s"} marked ${isAvailable ? "available" : "unavailable"} and queued for venue screens.`);
  };
  const visibleItems = useMemo(() => data.items.filter(row => {
    const text = `${row.name} ${row.style ?? ""} ${row.description ?? ""}`.toLowerCase();
    return (!query.trim() || text.includes(query.trim().toLowerCase())) && (!categoryFilter || row.tapCategoryId === categoryFilter);
  }), [categoryFilter, data.items, query]);
  const categoryName = (id?: string) => data.categories.find(row => row.id === id)?.name ?? "Uncategorized";

  return <article className="tap-list-admin">
    {reviewDialog}
    <div className="promotion-heading"><div><p>Breweries & bars</p><h3>Tap list</h3></div><span>{data.items.length} taps</span></div>
    {showUpgradePrompt && !enabled ? <p className="tier-notice">Tap List controls remain visible. Enable All Layouts to edit them.</p> : null}
    <p className={data.items.length > tapStripsCapacity ? "state error" : "state"} role="status">
      Tap Strips placement: positions 1–{Math.min(data.items.length, tapStripsCapacity)} visible
      {data.items.length > tapStripsCapacity ? ` · positions ${tapStripsCapacity + 1}–${data.items.length} overflow` : " · no overflow"}
    </p>
    {error ? <div className="state error" role="alert"><span>{error}</span>{retry ? <button disabled={busy} onClick={retryLast}>Retry last change</button> : null}</div> : null}
    {notice ? <TransientFeedback message={notice} onDismiss={() => setNotice(undefined)} /> : null}
    <section><div className="tap-section-heading"><div><h4>Categories</h4><span>Groups and optional shared price</span></div></div>
      <form onSubmit={addCategory}><input required disabled={!enabled} maxLength={120} placeholder="Import Beer" value={category.name} onChange={event => setCategory(value => ({ ...value, name: event.target.value }))} /><input disabled={!enabled} min={0} step=".01" type="number" placeholder="Category price" value={category.categoryPrice ?? ""} onChange={event => setCategory(value => ({ ...value, categoryPrice: event.target.value ? Number(event.target.value) : undefined }))} /><button disabled={!enabled || busy}>Add category</button></form>
      {!data.categories.length ? <p className="state">No categories yet. Taps can remain uncategorized.</p> : null}
      <ul>{data.categories.map((row, index) => { const dependencies = data.items.filter(itemRow => itemRow.tapCategoryId === row.id).length; return <li key={row.id}>
        <input aria-label={`${row.name} category name`} disabled={!enabled} maxLength={120} value={row.name} onChange={event => patchCategory(row.id, { name: event.target.value })} />
        <input aria-label={`${row.name} category price`} disabled={!enabled} min={0} step=".01" type="number" placeholder="Per-item pricing" value={row.categoryPrice ?? ""} onChange={event => patchCategory(row.id, { categoryPrice: event.target.value ? Number(event.target.value) : undefined })} />
        <label><input disabled={!enabled} type="checkbox" checked={row.isActive} onChange={event => patchCategory(row.id, { isActive: event.target.checked })} />Active</label>
        <span>{dependencies} tap{dependencies === 1 ? "" : "s"}</span>
        <button disabled={!enabled || busy} onClick={() => run(() => saveTapCategory(configuration, apiKey, venueId, row, row.id), `${row.name} saved and queued for venue screens.`)}>Save</button>
        <button aria-label={`Move ${row.name} earlier`} disabled={!enabled || busy || index === 0} onClick={() => move("categories", row.id, -1)}>↑</button><button aria-label={`Move ${row.name} later`} disabled={!enabled || busy || index === data.categories.length - 1} onClick={() => move("categories", row.id, 1)}>↓</button><button disabled={!enabled || busy || dependencies > 0} title={dependencies ? "Move or delete category taps first" : undefined} onClick={() => removeCategory(row)}>Delete</button>
      </li>; })}</ul>
    </section>
    <section><div className="tap-section-heading"><div><h4>Tap items</h4><span>Search, group, preview placement, and update up to {bulkLimit} taps</span></div></div>
      <form className="tap-item-form" onSubmit={addItem}>
        <input required disabled={!enabled} maxLength={200} placeholder="Beer name" value={item.name} onChange={event => setItem(value => ({ ...value, name: event.target.value }))} />
        <input disabled={!enabled} maxLength={160} placeholder="Style" value={item.style ?? ""} onChange={event => setItem(value => ({ ...value, style: event.target.value }))} />
        <textarea disabled={!enabled} maxLength={1000} aria-label="Tap description" placeholder="Description" value={item.description ?? ""} onChange={event => setItem(value => ({ ...value, description: event.target.value }))} />
        <input required disabled={!enabled} min={0} step=".01" type="number" aria-label="Tap price" value={item.price} onChange={event => setItem(value => ({ ...value, price: Number(event.target.value) }))} />
        <select disabled={!enabled} aria-label="Tap category" value={item.tapCategoryId ?? ""} onChange={event => setItem(value => ({ ...value, tapCategoryId: event.target.value || undefined }))}><option value="">No category</option>{data.categories.map(row => <option key={row.id} value={row.id}>{row.name}</option>)}</select>
        <button disabled={!enabled || busy}>Add tap</button>
      </form>
      <div className="tap-list-tools"><label>Search taps<input type="search" placeholder="Name, style, or description" value={query} onChange={event => setQuery(event.target.value)} /></label><label>Group filter<select value={categoryFilter} onChange={event => setCategoryFilter(event.target.value)}><option value="">All categories</option>{data.categories.map(row => <option key={row.id} value={row.id}>{row.name}</option>)}</select></label><div><span>{selected.length}/{bulkLimit} selected</span><button disabled={!enabled || busy || !selected.length} onClick={() => bulkAvailability(true)}>Mark available</button><button disabled={!enabled || busy || !selected.length} onClick={() => bulkAvailability(false)}>Mark unavailable</button><button disabled={!selected.length} onClick={() => setSelected([])}>Clear</button></div></div>
      {!visibleItems.length ? <p className="state">No taps match the current search and group filter.</p> : null}
      <ul>{visibleItems.map(row => { const index = data.items.findIndex(value => value.id === row.id); return <li key={row.id} className="tap-item-row">
        <label className="tap-select"><input disabled={!enabled || (!selected.includes(row.id) && selected.length >= bulkLimit)} type="checkbox" checked={selected.includes(row.id)} onChange={() => toggleSelected(row.id)} />Select <span>#{index + 1} · {index < tapStripsCapacity ? "visible" : "overflow"}</span></label>
        <input aria-label={`${row.name} name`} disabled={!enabled} maxLength={200} value={row.name} onChange={event => patchItem(row.id, { name: event.target.value })} />
        <input aria-label={`${row.name} style`} disabled={!enabled} maxLength={160} value={row.style ?? ""} onChange={event => patchItem(row.id, { style: event.target.value })} />
        <textarea aria-label={`${row.name} description`} disabled={!enabled} maxLength={1000} value={row.description ?? ""} onChange={event => patchItem(row.id, { description: event.target.value })} />
        <select aria-label={`${row.name} category`} disabled={!enabled} value={row.tapCategoryId ?? ""} onChange={event => patchItem(row.id, { tapCategoryId: event.target.value || undefined })}><option value="">Uncategorized</option>{data.categories.map(value => <option key={value.id} value={value.id}>{value.name}</option>)}</select>
        <label>ABV<input disabled={!enabled} min={0} max={100} step=".01" type="number" value={row.abv ?? ""} onChange={event => patchItem(row.id, { abv: event.target.value ? Number(event.target.value) : undefined })} /></label>
        <label>IBU<input disabled={!enabled} min={0} max={1000} type="number" value={row.ibu ?? ""} onChange={event => patchItem(row.id, { ibu: event.target.value ? Number(event.target.value) : undefined })} /></label>
        <input disabled={!enabled} type="color" aria-label={`${row.name} glass color`} value={row.glassColor ?? "#F5C842"} onChange={event => patchItem(row.id, { glassColor: event.target.value })} />
        <input disabled={!enabled} type="color" aria-label={`${row.name} name color`} value={row.nameColor ?? "#FFD700"} onChange={event => patchItem(row.id, { nameColor: event.target.value })} />
        <label><input disabled={!enabled} type="checkbox" checked={row.isAvailable} onChange={event => patchItem(row.id, { isAvailable: event.target.checked })} />Available</label>
        <label><input disabled={!enabled} type="checkbox" checked={row.isComingSoon} onChange={event => patchItem(row.id, { isComingSoon: event.target.checked })} />Now brewing</label>
        <span className="tap-group">{categoryName(row.tapCategoryId)}</span>
        <button disabled={!enabled || busy} onClick={() => run(() => saveTapItem(configuration, apiKey, venueId, row, row.id), `${row.name} saved and queued for venue screens.`)}>Save</button>
        <button aria-label={`Move ${row.name} earlier`} disabled={!enabled || busy || index === 0} onClick={() => move("items", row.id, -1)}>↑</button>
        <button aria-label={`Move ${row.name} later`} disabled={!enabled || busy || index === data.items.length - 1} onClick={() => move("items", row.id, 1)}>↓</button>
        <button disabled={!enabled || busy} onClick={() => removeItem(row)}>Delete</button>
      </li>; })}</ul>
    </section>
  </article>;
}

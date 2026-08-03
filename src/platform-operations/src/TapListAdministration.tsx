import { useEffect, useState, type FormEvent } from "react";
import {
  deleteTapCategory, deleteTapItem, loadTapList, reorderTapRows, saveTapCategory, saveTapItem,
  type TapCategory, type TapItem, type TapListSnapshot
} from "./api";
import type { PlatformOperationsConfiguration } from "./config";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string; venueId: string; enabled: boolean };
const newItem = (): Omit<TapItem, "id" | "venueId" | "sortOrder"> => ({
  name: "", price: 0, isAvailable: true, isComingSoon: false, glassColor: "#F5C842", nameColor: "#FFD700"
});
const tapStripsCapacity = 12;

export default function TapListAdministration({ configuration, apiKey, venueId, enabled }: Props) {
  const [data, setData] = useState<TapListSnapshot>({ categories: [], items: [] });
  const [category, setCategory] = useState({ name: "", categoryPrice: undefined as number | undefined, isActive: true });
  const [item, setItem] = useState(newItem());
  const [error, setError] = useState<string>();
  const [busy, setBusy] = useState(false);
  const refresh = () => loadTapList(configuration, apiKey, venueId).then(setData);
  useEffect(() => { refresh().catch(() => setError("Tap list could not be loaded.")); }, [apiKey, configuration, venueId]);
  const run = async (operation: () => Promise<unknown>) => {
    setBusy(true); setError(undefined);
    try { await operation(); await refresh(); } catch { setError("The tap-list change could not be saved."); }
    finally { setBusy(false); }
  };
  const addCategory = (event: FormEvent) => {
    event.preventDefault(); if (!enabled) return;
    void run(async () => { await saveTapCategory(configuration, apiKey, venueId, category); setCategory({ name: "", categoryPrice: undefined, isActive: true }); });
  };
  const addItem = (event: FormEvent) => {
    event.preventDefault(); if (!enabled) return;
    void run(async () => { await saveTapItem(configuration, apiKey, venueId, item); setItem(newItem()); });
  };
  const move = (kind: "categories" | "items", id: string, offset: number) => {
    const rows = [...data[kind]];
    const from = rows.findIndex(row => row.id === id); const to = from + offset;
    if (from < 0 || to < 0 || to >= rows.length) return;
    [rows[from], rows[to]] = [rows[to], rows[from]];
    void run(() => reorderTapRows(configuration, apiKey, venueId, kind, rows.map(row => row.id)));
  };
  const patchItem = (id: string, value: Partial<TapItem>) =>
    setData(current => ({ ...current, items: current.items.map(row => row.id === id ? { ...row, ...value } : row) }));
  const patchCategory = (id: string, value: Partial<TapCategory>) =>
    setData(current => ({ ...current, categories: current.categories.map(row => row.id === id ? { ...row, ...value } : row) }));

  return <article className="tap-list-admin">
    <div className="promotion-heading"><div><p>Breweries & bars</p><h3>Tap list</h3></div><span>{data.items.length} taps</span></div>
    {!enabled ? <p className="tier-notice">Tap List controls remain visible. Enable All Layouts to edit them.</p> : null}
    <p className={data.items.length > tapStripsCapacity ? "state error" : "state"}>
      Tap Strips TV capacity: {Math.min(data.items.length, tapStripsCapacity)} visible
      {data.items.length > tapStripsCapacity ? ` · ${data.items.length - tapStripsCapacity} overflow` : " · no overflow"}
    </p>
    {error ? <p className="state error">{error}</p> : null}
    <section><h4>Categories</h4>
      <form onSubmit={addCategory}><input required disabled={!enabled} maxLength={120} placeholder="Import Beer" value={category.name} onChange={event => setCategory(value => ({ ...value, name: event.target.value }))} /><input disabled={!enabled} min={0} step=".01" type="number" placeholder="Category price" value={category.categoryPrice ?? ""} onChange={event => setCategory(value => ({ ...value, categoryPrice: event.target.value ? Number(event.target.value) : undefined }))} /><button disabled={!enabled || busy}>Add category</button></form>
      <ul>{data.categories.map((row, index) => <li key={row.id}>
        <input aria-label={`${row.name} category name`} disabled={!enabled} maxLength={120} value={row.name} onChange={event => patchCategory(row.id, { name: event.target.value })} />
        <input aria-label={`${row.name} category price`} disabled={!enabled} min={0} step=".01" type="number" placeholder="Per-item pricing" value={row.categoryPrice ?? ""} onChange={event => patchCategory(row.id, { categoryPrice: event.target.value ? Number(event.target.value) : undefined })} />
        <label><input disabled={!enabled} type="checkbox" checked={row.isActive} onChange={event => patchCategory(row.id, { isActive: event.target.checked })} />Active</label>
        <button disabled={!enabled || busy} onClick={() => run(() => saveTapCategory(configuration, apiKey, venueId, row, row.id))}>Save</button>
        <button disabled={!enabled || busy || index === 0} onClick={() => move("categories", row.id, -1)}>↑</button><button disabled={!enabled || busy || index === data.categories.length - 1} onClick={() => move("categories", row.id, 1)}>↓</button><button disabled={!enabled || busy} onClick={() => run(() => deleteTapCategory(configuration, apiKey, venueId, row.id))}>Delete</button>
      </li>)}</ul>
    </section>
    <section><h4>Tap items</h4>
      <form className="tap-item-form" onSubmit={addItem}>
        <input required disabled={!enabled} maxLength={200} placeholder="Beer name" value={item.name} onChange={event => setItem(value => ({ ...value, name: event.target.value }))} />
        <input disabled={!enabled} maxLength={160} placeholder="Style" value={item.style ?? ""} onChange={event => setItem(value => ({ ...value, style: event.target.value }))} />
        <input required disabled={!enabled} min={0} step=".01" type="number" aria-label="Tap price" value={item.price} onChange={event => setItem(value => ({ ...value, price: Number(event.target.value) }))} />
        <select disabled={!enabled} aria-label="Tap category" value={item.tapCategoryId ?? ""} onChange={event => setItem(value => ({ ...value, tapCategoryId: event.target.value || undefined }))}><option value="">No category</option>{data.categories.map(row => <option key={row.id} value={row.id}>{row.name}</option>)}</select>
        <button disabled={!enabled || busy}>Add tap</button>
      </form>
      <ul>{data.items.map((row, index) => <li key={row.id} className="tap-item-row">
        <input disabled={!enabled} maxLength={200} value={row.name} onChange={event => patchItem(row.id, { name: event.target.value })} />
        <input disabled={!enabled} maxLength={160} value={row.style ?? ""} onChange={event => patchItem(row.id, { style: event.target.value })} />
        <label>ABV<input disabled={!enabled} min={0} max={100} step=".01" type="number" value={row.abv ?? ""} onChange={event => patchItem(row.id, { abv: event.target.value ? Number(event.target.value) : undefined })} /></label>
        <label>IBU<input disabled={!enabled} min={0} max={1000} type="number" value={row.ibu ?? ""} onChange={event => patchItem(row.id, { ibu: event.target.value ? Number(event.target.value) : undefined })} /></label>
        <input disabled={!enabled} type="color" aria-label="Glass color" value={row.glassColor ?? "#F5C842"} onChange={event => patchItem(row.id, { glassColor: event.target.value })} />
        <input disabled={!enabled} type="color" aria-label="Name color" value={row.nameColor ?? "#FFD700"} onChange={event => patchItem(row.id, { nameColor: event.target.value })} />
        <label><input disabled={!enabled} type="checkbox" checked={row.isAvailable} onChange={event => patchItem(row.id, { isAvailable: event.target.checked })} />Available</label>
        <label><input disabled={!enabled} type="checkbox" checked={row.isComingSoon} onChange={event => patchItem(row.id, { isComingSoon: event.target.checked })} />Now brewing</label>
        <button disabled={!enabled || busy} onClick={() => run(() => saveTapItem(configuration, apiKey, venueId, row, row.id))}>Save</button>
        <button disabled={!enabled || busy || index === 0} onClick={() => move("items", row.id, -1)}>↑</button>
        <button disabled={!enabled || busy || index === data.items.length - 1} onClick={() => move("items", row.id, 1)}>↓</button>
        <button disabled={!enabled || busy} onClick={() => run(() => deleteTapItem(configuration, apiKey, venueId, row.id))}>Delete</button>
      </li>)}</ul>
    </section>
  </article>;
}

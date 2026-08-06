import { useEffect, useRef, useState, type FormEvent } from "react";
import { createMenuItem, reorderMenuItems, updateMenuItem, updateMenuItemLifecycle, updateMenuItemPresentation, type MenuItem, type MenuItemWrite } from "./api";
import type { BackOfficeConfiguration } from "./config";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; menuId: string; sectionId: string; items: MenuItem[]; capabilities: { happyHour: boolean; allergenBadges: boolean }; disabled: boolean; onChanged: () => Promise<void>; onError: (message: string) => void; onTierPrompt: (title: string, message: string) => void };
type SaveState = "draft" | "saving" | "saved" | "failed";
const emptyItem: MenuItemWrite = { name: "", description: "", price: 0 };

export default function MenuItemsEditor({ configuration, apiKey, venueId, menuId, sectionId, items, capabilities, disabled, onChanged, onError, onTierPrompt }: Props) {
  const [drafts, setDrafts] = useState<MenuItem[]>(items);
  const [newItem, setNewItem] = useState<MenuItemWrite>(emptyItem);
  const [states, setStates] = useState<Record<string, SaveState>>({});
  const [pendingArchive, setPendingArchive] = useState<MenuItem>();
  /**
   * Per-item edit revision, bumped on every keystroke-level change.
   *
   * A plain "is pending" flag is not enough. Blurring an input to click Save fires
   * both onBlur->save and onClick->save, and an edit made while a request is still in
   * flight would previously have its pending marker cleared by the *older* request
   * completing; the refresh that followed then replaced the newer draft with the
   * server's stale value. Saves now capture the revision they sent and clear the
   * marker only if nothing has changed since, so a newer edit always survives.
   */
  const draftRevisions = useRef(new Map<string, number>());
  const savingItems = useRef(new Set<string>());
  const isPending = (itemId: string) => draftRevisions.current.has(itemId);
  const settleSave = (itemId: string, savedRevision: number) => {
    if (draftRevisions.current.get(itemId) === savedRevision) {
      draftRevisions.current.delete(itemId);
      return true;
    }
    return false;
  };
  useEffect(() => {
    setDrafts(current => items.map(item => {
      const draft = isPending(item.id) ? current.find(entry => entry.id === item.id) : undefined;
      return draft ?? item;
    }));
  }, [items]);
  const patch = (itemId: string, values: Partial<MenuItem>) => {
    draftRevisions.current.set(itemId, (draftRevisions.current.get(itemId) ?? 0) + 1);
    setDrafts(current => current.map(item => item.id === itemId ? { ...item, ...values } : item));
    setStates(current => ({ ...current, [itemId]: "draft" }));
  };
  const save = async (item: MenuItem) => {
    if (savingItems.current.has(item.id)) return;
    savingItems.current.add(item.id);
    const sentRevision = draftRevisions.current.get(item.id) ?? 0;
    setStates(current => ({ ...current, [item.id]: "saving" }));
    try {
      await updateMenuItem(configuration, apiKey, venueId, menuId, sectionId, item.id, { name: item.name, description: item.description, price: item.price, happyHourPrice: item.happyHourPrice });
      const settled = settleSave(item.id, sentRevision);
      // Only report "saved" when nothing changed while the request was in flight;
      // otherwise the newer edit is still unsaved and must keep saying so.
      setStates(current => ({ ...current, [item.id]: settled ? "saved" : "draft" }));
      await onChanged();
    }
    catch { setStates(current => ({ ...current, [item.id]: "failed" })); onError(`${item.name || "Menu item"} could not be saved. Correct the values or retry.`); }
    finally { savingItems.current.delete(item.id); }
  };
  const create = async (event: FormEvent) => { event.preventDefault(); try { await createMenuItem(configuration, apiKey, venueId, menuId, sectionId, newItem); setNewItem(emptyItem); await onChanged(); } catch { onError("The new menu item could not be created."); } };
  const savePresentation = async (item: MenuItem) => {
    if (savingItems.current.has(item.id)) return;
    savingItems.current.add(item.id);
    const sentRevision = draftRevisions.current.get(item.id) ?? 0;
    setStates(current => ({ ...current, [item.id]: "saving" }));
    try {
      await updateMenuItemPresentation(configuration, apiKey, venueId, menuId, sectionId, item);
      const settled = settleSave(item.id, sentRevision);
      setStates(current => ({ ...current, [item.id]: settled ? "saved" : "draft" }));
      await onChanged();
    }
    catch { setStates(current => ({ ...current, [item.id]: "failed" })); onError(`${item.name} presentation could not be saved. Retry the item.`); }
    finally { savingItems.current.delete(item.id); }
  };
  const setActive = async (item: MenuItem, isActive: boolean) => { try { await updateMenuItemLifecycle(configuration, apiKey, venueId, menuId, sectionId, item.id, isActive); setPendingArchive(undefined); await onChanged(); } catch { onError(`${item.name} could not be ${isActive ? "restored" : "archived"}.`); } };
  const move = async (index: number, delta: number) => { const next = [...drafts]; const target = index + delta; if (target < 0 || target >= next.length) return; [next[index], next[target]] = [next[target], next[index]]; setDrafts(next); try { await reorderMenuItems(configuration, apiKey, venueId, menuId, sectionId, next.map(item => item.id)); await onChanged(); } catch { setDrafts(items); onError("Item order could not be saved. The previous order was restored."); } };

  return <div className="menu-items">
    {pendingArchive ? <section className="destructive-review compact" aria-labelledby={`archive-${pendingArchive.id}`}><h4 id={`archive-${pendingArchive.id}`}>Archive {pendingArchive.name}?</h4><p>The item will be hidden but can be restored from this section.</p><div><button type="button" onClick={() => setPendingArchive(undefined)}>Cancel</button><button className="danger" type="button" onClick={() => void setActive(pendingArchive, false)}>Confirm archive</button></div></section> : null}
    {drafts.map((item, index) => <div data-testid="menu-item" data-item-id={item.id} data-save-state={states[item.id] ?? "clean"} data-available={item.isAvailable} data-active={item.isActive} className={`${item.isAvailable ? "menu-item" : "menu-item unavailable"}${item.isActive ? "" : " archived"}`} key={item.id}>
      <div className="menu-badges">{item.isPopular ? <span className="popular">Bestseller</span> : null}{!item.isActive ? <span>Archived</span> : null}{item.quantityAvailable != null ? <span>{item.quantityAvailable} left</span> : null}{item.tags?.split(",").filter(Boolean).map(tag => <span key={tag}>{tag}</span>)}</div>
      <div className="item-save-status" role="status" aria-live="polite" data-testid="item-save-status">{states[item.id] === "saving" ? "Saving…" : states[item.id] === "saved" ? "Saved" : states[item.id] === "failed" ? "Save failed" : states[item.id] === "draft" ? "Unsaved draft" : ""}{states[item.id] === "draft" || states[item.id] === "failed" ? <button type="button" className="item-save" data-testid="item-save" aria-label={`${states[item.id] === "failed" ? "Retry saving" : "Save"} ${item.name || "menu item"}`} disabled={disabled || !item.isActive || states[item.id] === "saving"} onClick={() => void save(item)}>{states[item.id] === "failed" ? "Retry" : "Save"}</button> : null}</div>
      <div className="menu-item-row"><input data-testid="item-name" aria-label="Item name" disabled={disabled || !item.isActive} maxLength={160} required value={item.name} onChange={event => patch(item.id, { name: event.target.value })} onBlur={() => void save(item)} /><input data-testid="item-description" aria-label="Item description" disabled={disabled || !item.isActive} maxLength={1000} value={item.description ?? ""} onChange={event => patch(item.id, { description: event.target.value })} onBlur={() => void save(item)} /><label>Price<input data-testid="item-price" aria-label="Item price" disabled={disabled || !item.isActive} min="0" max="999999.99" step="0.01" type="number" value={item.price} onChange={event => patch(item.id, { price: Number(event.target.value) })} onBlur={() => void save(item)} /></label><label>Happy hour <span className="feature-badge">Tier feature</span><input aria-label="Happy hour price" disabled={disabled || !item.isActive || !capabilities.happyHour} min="0" max="999999.99" step="0.01" type="number" value={item.happyHourPrice ?? ""} onChange={event => patch(item.id, { happyHourPrice: event.target.value === "" ? undefined : Number(event.target.value) })} onBlur={() => void save(item)} />{!capabilities.happyHour ? <button type="button" className="feature-preview" onClick={() => onTierPrompt("Happy-hour pricing", "Preview promotional pricing beside each standard menu price. Enable Happy Hour for this venue to edit it.")}>Preview</button> : null}</label></div>
      <div className="menu-item-presentation"><button type="button" data-testid="item-availability" className={item.isAvailable ? "available" : ""} disabled={disabled || !item.isActive} onClick={() => { const changed = { ...item, isAvailable: !item.isAvailable }; patch(item.id, changed); void savePresentation(changed); }}>{item.isAvailable ? "Available" : "Unavailable"}</button><label>Quantity<input aria-label="Quantity available" disabled={disabled || !item.isActive} min="0" step="1" type="number" value={item.quantityAvailable ?? ""} onChange={event => patch(item.id, { quantityAvailable: event.target.value === "" ? undefined : Number(event.target.value) })} onBlur={() => void savePresentation(item)} /></label><label className="tag-field">Dietary / allergen tags <span className="feature-badge">Tier feature</span><input aria-label="Menu item tags" disabled={disabled || !item.isActive || !capabilities.allergenBadges} maxLength={500} placeholder="vegan, gluten-free, contains nuts" value={item.tags ?? ""} onChange={event => patch(item.id, { tags: event.target.value })} onBlur={() => void savePresentation(item)} />{!capabilities.allergenBadges ? <button type="button" className="feature-preview" onClick={() => onTierPrompt("Dietary and allergen badges", "Preview clear dietary and allergen labels on menu items. Enable Allergen Badges for this venue to edit them.")}>Preview</button> : null}</label><label className="popular-check"><input checked={item.isPopular} disabled={disabled || !item.isActive} type="checkbox" onChange={event => { const changed = { ...item, isPopular: event.target.checked }; patch(item.id, changed); void savePresentation(changed); }} /> Bestseller</label></div>
      <div className="item-lifecycle-actions"><button type="button" aria-label={`Move ${item.name} up`} disabled={disabled || index === 0} onClick={() => void move(index, -1)}>Move up</button><button type="button" aria-label={`Move ${item.name} down`} disabled={disabled || index === drafts.length - 1} onClick={() => void move(index, 1)}>Move down</button>{item.isActive ? <button type="button" className="danger-link" disabled={disabled} onClick={() => setPendingArchive(item)}>Archive item</button> : <button type="button" disabled={disabled} onClick={() => void setActive(item, true)}>Restore item</button>}</div>
    </div>)}
    <form className="menu-item-create" onSubmit={create}><input aria-label="New item name" disabled={disabled} maxLength={160} required placeholder="Add an item" value={newItem.name} onChange={event => setNewItem(value => ({ ...value, name: event.target.value }))} /><input aria-label="New item price" disabled={disabled} min="0" max="999999.99" step="0.01" required type="number" value={newItem.price} onChange={event => setNewItem(value => ({ ...value, price: Number(event.target.value) }))} /><button disabled={disabled}>Add item</button></form>
  </div>;
}

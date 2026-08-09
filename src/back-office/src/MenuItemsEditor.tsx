import { useEffect, useRef, useState, type FormEvent } from "react";
import { createMenuItem, reorderMenuItems, updateMenuItem, updateQuickAvailability, type MenuItem, type MenuItemWrite } from "./api";
import type { BackOfficeConfiguration } from "./config";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; menuId: string; sectionId: string; items: MenuItem[]; disabled: boolean; onChanged: () => Promise<void>; onError: (message: string) => void };
type SaveState = "draft" | "saving" | "saved" | "failed";
const emptyItem: MenuItemWrite = { name: "", description: "", price: 0 };

export default function MenuItemsEditor({ configuration, apiKey, venueId, menuId, sectionId, items, disabled, onChanged, onError }: Props) {
  const [drafts, setDrafts] = useState<MenuItem[]>(items);
  const [newItem, setNewItem] = useState<MenuItemWrite>(emptyItem);
  const [states, setStates] = useState<Record<string, SaveState>>({});
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
      await updateMenuItem(configuration, apiKey, venueId, menuId, sectionId, item.id, { name: item.name, description: item.description, price: item.price });
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
  const toggleAvailability = async (item: MenuItem) => {
    if (savingItems.current.has(item.id)) return;
    savingItems.current.add(item.id);
    setStates(current => ({ ...current, [item.id]: "saving" }));
    try {
      await updateQuickAvailability(configuration, apiKey, venueId, menuId, sectionId, item.id, !item.isAvailable);
      setStates(current => ({ ...current, [item.id]: "saved" }));
      await onChanged();
    }
    catch { setStates(current => ({ ...current, [item.id]: "failed" })); onError(`${item.name} availability could not be updated.`); }
    finally { savingItems.current.delete(item.id); }
  };
  const move = async (index: number, delta: number) => { const next = [...drafts]; const target = index + delta; if (target < 0 || target >= next.length) return; [next[index], next[target]] = [next[target], next[index]]; setDrafts(next); try { await reorderMenuItems(configuration, apiKey, venueId, menuId, sectionId, next.map(item => item.id)); await onChanged(); } catch { setDrafts(items); onError("Item order could not be saved. The previous order was restored."); } };

  return <div className="menu-items">
    {drafts.map((item, index) => <div data-testid="menu-item" data-item-id={item.id} data-save-state={states[item.id] ?? "clean"} data-available={item.isAvailable} className={item.isAvailable ? "menu-item" : "menu-item unavailable"} key={item.id}>
      <div className="item-save-status" role="status" aria-live="polite" data-testid="item-save-status">{states[item.id] === "saving" ? "Saving…" : states[item.id] === "saved" ? "Saved" : states[item.id] === "failed" ? "Save failed" : states[item.id] === "draft" ? "Unsaved draft" : ""}{states[item.id] === "draft" || states[item.id] === "failed" ? <button type="button" className="item-save" data-testid="item-save" aria-label={`${states[item.id] === "failed" ? "Retry saving" : "Save"} ${item.name || "menu item"}`} disabled={disabled || states[item.id] === "saving"} onClick={() => void save(item)}>{states[item.id] === "failed" ? "Retry" : "Save"}</button> : null}</div>
      <div className="menu-item-row"><input data-testid="item-name" aria-label="Item name" disabled={disabled} maxLength={200} required value={item.name} onChange={event => patch(item.id, { name: event.target.value })} onBlur={() => void save(item)} /><input data-testid="item-description" aria-label="Item description" disabled={disabled} maxLength={1000} value={item.description ?? ""} onChange={event => patch(item.id, { description: event.target.value })} onBlur={() => void save(item)} /><label>Price<input data-testid="item-price" aria-label="Item price" disabled={disabled} min="0" max="999999.99" step="0.01" type="number" value={item.price} onChange={event => patch(item.id, { price: Number(event.target.value) })} onBlur={() => void save(item)} /></label></div>
      <div className="menu-item-presentation"><button type="button" data-testid="item-availability" className={item.isAvailable ? "available" : ""} disabled={disabled} onClick={() => void toggleAvailability(item)}>{item.isAvailable ? "Available" : "Unavailable"}</button></div>
      <div className="item-lifecycle-actions"><button type="button" aria-label={`Move ${item.name} up`} disabled={disabled || index === 0} onClick={() => void move(index, -1)}>Move up</button><button type="button" aria-label={`Move ${item.name} down`} disabled={disabled || index === drafts.length - 1} onClick={() => void move(index, 1)}>Move down</button></div>
    </div>)}
    <form className="menu-item-create" onSubmit={create}><input aria-label="New item name" disabled={disabled} maxLength={200} required placeholder="Add an item" value={newItem.name} onChange={event => setNewItem(value => ({ ...value, name: event.target.value }))} /><input aria-label="New item price" disabled={disabled} min="0" max="999999.99" step="0.01" required type="number" value={newItem.price} onChange={event => setNewItem(value => ({ ...value, price: Number(event.target.value) }))} /><button disabled={disabled}>Add item</button></form>
  </div>;
}

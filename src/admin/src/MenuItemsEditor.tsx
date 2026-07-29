import { useEffect, useState, type FormEvent } from "react";
import { createMenuItem, updateMenuItem, type MenuItem, type MenuItemWrite } from "./api";
import type { AdminConfiguration } from "./config";

type Props = {
  configuration: AdminConfiguration;
  apiKey: string;
  venueId: string;
  menuId: string;
  sectionId: string;
  items: MenuItem[];
  disabled: boolean;
  onChanged: () => Promise<void>;
  onError: () => void;
};

const emptyItem: MenuItemWrite = { name: "", description: "", price: 0 };

export default function MenuItemsEditor({
  configuration, apiKey, venueId, menuId, sectionId, items, disabled, onChanged, onError
}: Props) {
  const [drafts, setDrafts] = useState<MenuItem[]>(items);
  const [newItem, setNewItem] = useState<MenuItemWrite>(emptyItem);
  const [saving, setSaving] = useState(false);
  useEffect(() => setDrafts(items), [items]);

  const patch = (itemId: string, values: Partial<MenuItem>) =>
    setDrafts(current => current.map(item => item.id === itemId ? { ...item, ...values } : item));
  const save = async (item: MenuItem) => {
    setSaving(true);
    try {
      await updateMenuItem(configuration, apiKey, venueId, menuId, sectionId, item.id, {
        name: item.name,
        description: item.description,
        price: item.price,
        happyHourPrice: item.happyHourPrice
      });
      await onChanged();
    } catch { onError(); }
    finally { setSaving(false); }
  };
  const create = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    try {
      await createMenuItem(configuration, apiKey, venueId, menuId, sectionId, newItem);
      setNewItem(emptyItem);
      await onChanged();
    } catch { onError(); }
    finally { setSaving(false); }
  };

  return <div className="menu-items">
    {drafts.map(item => <div className="menu-item-row" key={item.id}>
      <input aria-label="Item name" disabled={saving || disabled} maxLength={160} required value={item.name}
        onChange={event => patch(item.id, { name: event.target.value })} onBlur={() => save(item)} />
      <input aria-label="Item description" disabled={saving || disabled} maxLength={1000} value={item.description ?? ""}
        onChange={event => patch(item.id, { description: event.target.value })} onBlur={() => save(item)} />
      <label>Price<input aria-label="Item price" disabled={saving || disabled} min="0" max="999999.99" step="0.01" type="number" value={item.price}
        onChange={event => patch(item.id, { price: Number(event.target.value) })} onBlur={() => save(item)} /></label>
      <label>Happy hour<input aria-label="Happy hour price" disabled={saving || disabled} min="0" max="999999.99" step="0.01" type="number"
        value={item.happyHourPrice ?? ""} onChange={event => patch(item.id, { happyHourPrice: event.target.value === "" ? undefined : Number(event.target.value) })}
        onBlur={() => save(item)} /></label>
    </div>)}
    <form className="menu-item-create" onSubmit={create}>
      <input aria-label="New item name" disabled={saving || disabled} maxLength={160} required placeholder="Add an item" value={newItem.name}
        onChange={event => setNewItem(value => ({ ...value, name: event.target.value }))} />
      <input aria-label="New item price" disabled={saving || disabled} min="0" max="999999.99" step="0.01" required type="number" value={newItem.price}
        onChange={event => setNewItem(value => ({ ...value, price: Number(event.target.value) }))} />
      <button disabled={saving || disabled}>Add item</button>
    </form>
  </div>;
}

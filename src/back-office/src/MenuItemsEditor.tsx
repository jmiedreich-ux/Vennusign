import { useEffect, useState, type FormEvent } from "react";
import { createMenuItem, updateMenuItem, updateMenuItemPresentation, type MenuItem, type MenuItemWrite } from "./api";
import type { BackOfficeConfiguration } from "./config";

type Props = {
  configuration: BackOfficeConfiguration;
  apiKey: string;
  venueId: string;
  menuId: string;
  sectionId: string;
  items: MenuItem[];
  capabilities: { happyHour: boolean; allergenBadges: boolean };
  disabled: boolean;
  onChanged: () => Promise<void>;
  onError: () => void;
  onTierPrompt: (title: string, message: string) => void;
};

const emptyItem: MenuItemWrite = { name: "", description: "", price: 0 };

export default function MenuItemsEditor({
  configuration, apiKey, venueId, menuId, sectionId, items, capabilities, disabled, onChanged, onError, onTierPrompt
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
  const savePresentation = async (item: MenuItem) => {
    setSaving(true);
    try {
      await updateMenuItemPresentation(configuration, apiKey, venueId, menuId, sectionId, item);
      await onChanged();
    } catch { onError(); }
    finally { setSaving(false); }
  };

  return <div className="menu-items">
    {drafts.map(item => <div className={item.isAvailable ? "menu-item" : "menu-item unavailable"} key={item.id}>
      <div className="menu-badges">
        {item.isPopular ? <span className="popular">Bestseller</span> : null}
        {item.quantityAvailable != null ? <span>{item.quantityAvailable} left</span> : null}
        {item.tags?.split(",").filter(Boolean).map(tag => <span key={tag}>{tag}</span>)}
      </div>
      <div className="menu-item-row">
      <input aria-label="Item name" disabled={saving || disabled} maxLength={160} required value={item.name}
        onChange={event => patch(item.id, { name: event.target.value })} onBlur={() => save(item)} />
      <input aria-label="Item description" disabled={saving || disabled} maxLength={1000} value={item.description ?? ""}
        onChange={event => patch(item.id, { description: event.target.value })} onBlur={() => save(item)} />
      <label>Price<input aria-label="Item price" disabled={saving || disabled} min="0" max="999999.99" step="0.01" type="number" value={item.price}
        onChange={event => patch(item.id, { price: Number(event.target.value) })} onBlur={() => save(item)} /></label>
      <label>Happy hour <span className="feature-badge">Tier feature</span><input aria-label="Happy hour price" disabled={saving || disabled || !capabilities.happyHour} min="0" max="999999.99" step="0.01" type="number"
        value={item.happyHourPrice ?? ""} onChange={event => patch(item.id, { happyHourPrice: event.target.value === "" ? undefined : Number(event.target.value) })}
        onBlur={() => save(item)} />{!capabilities.happyHour ? <button className="feature-preview" onClick={() => onTierPrompt("Happy-hour pricing", "Preview promotional pricing beside each standard menu price. Enable Happy Hour for this venue to edit it.")}>Preview</button> : null}</label>
      </div>
      <div className="menu-item-presentation">
        <button className={item.isAvailable ? "available" : ""} disabled={saving || disabled}
          onClick={() => { const changed = { ...item, isAvailable: !item.isAvailable }; patch(item.id, changed); void savePresentation(changed); }}>
          {item.isAvailable ? "Available" : "Unavailable"}
        </button>
        <label>Quantity<input aria-label="Quantity available" disabled={saving || disabled} min="0" step="1" type="number"
          value={item.quantityAvailable ?? ""} onChange={event => patch(item.id, { quantityAvailable: event.target.value === "" ? undefined : Number(event.target.value) })}
          onBlur={() => savePresentation(item)} /></label>
        <label className="tag-field">Dietary / allergen tags <span className="feature-badge">Tier feature</span><input aria-label="Menu item tags" disabled={saving || disabled || !capabilities.allergenBadges} maxLength={500}
          placeholder="vegan, gluten-free, contains nuts" value={item.tags ?? ""} onChange={event => patch(item.id, { tags: event.target.value })}
          onBlur={() => savePresentation(item)} />{!capabilities.allergenBadges ? <button className="feature-preview" onClick={() => onTierPrompt("Dietary and allergen badges", "Preview clear dietary and allergen labels on menu items. Enable Allergen Badges for this venue to edit them.")}>Preview</button> : null}</label>
        <label className="popular-check"><input checked={item.isPopular} disabled={saving || disabled} type="checkbox"
          onChange={event => { const changed = { ...item, isPopular: event.target.checked }; patch(item.id, changed); void savePresentation(changed); }} /> Bestseller</label>
      </div>
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

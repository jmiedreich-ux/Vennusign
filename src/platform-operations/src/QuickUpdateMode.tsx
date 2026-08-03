import { useState, type FormEvent } from "react";
import { updateQuickAvailability, updateQuickDailySpecial, type MenuEditorSnapshot } from "./api";
import type { PlatformOperationsConfiguration } from "./config";

type Props = {
  configuration: PlatformOperationsConfiguration;
  apiKey: string;
  venueId: string;
  snapshot: MenuEditorSnapshot;
  onChanged: () => Promise<void>;
};

export default function QuickUpdateMode({ configuration, apiKey, venueId, snapshot, onChanged }: Props) {
  const menu = snapshot.menus[0]?.menu;
  const [dailySpecial, setDailySpecial] = useState(menu?.dailySpecial ?? "");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  if (!menu || !snapshot.capabilities.quickUpdate) return null;

  const saveSpecial = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined);
    try { await updateQuickDailySpecial(configuration, apiKey, venueId, menu.id, dailySpecial); await onChanged(); }
    catch { setError("The daily special could not be pushed."); }
    finally { setBusy(false); }
  };
  const toggle = async (sectionId: string, itemId: string, isAvailable: boolean) => {
    setBusy(true); setError(undefined);
    try { await updateQuickAvailability(configuration, apiKey, venueId, menu.id, sectionId, itemId, isAvailable); await onChanged(); }
    catch { setError("Availability could not be updated."); }
    finally { setBusy(false); }
  };

  return <section className="quick-update">
    <div><p>Mobile service controls</p><h4>Quick Update</h4><span>Unavailable items restore at venue-local midnight.</span></div>
    {error ? <p className="state error">{error}</p> : null}
    <form onSubmit={saveSpecial}>
      <label>Daily special<input aria-label="Daily special" maxLength={240} placeholder="Tonight: smoked brisket tacos" value={dailySpecial} onChange={event => setDailySpecial(event.target.value)} /></label>
      <button disabled={busy}>Push special</button>
    </form>
    <div className="quick-items">{snapshot.menus[0].sections.flatMap(section => {
      const items = snapshot.itemGroups.find(group => group.sectionId === section.id)?.items ?? [];
      return items.map(item => <button className={item.isAvailable ? "" : "off"} disabled={busy} key={item.id}
        onClick={() => toggle(section.id, item.id, !item.isAvailable)}>
        <span><small>{section.name}</small><strong>{item.name}</strong></span>
        <span>{item.isAvailable ? "Live" : "Off"}</span>
      </button>);
    })}</div>
  </section>;
}

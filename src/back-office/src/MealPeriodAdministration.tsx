import { useEffect, useState, type FormEvent } from "react";
import { createMealPeriod, deleteMealPeriod, loadMealPeriods, updateMealPeriod, type MealPeriod, type MealPeriodSnapshot } from "./api";
import type { BackOfficeConfiguration } from "./config";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string };
const days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const time = (value: string) => value.slice(0, 5);
const wireTime = (value: string) => `${value}:00`;

export default function MealPeriodAdministration({ configuration, apiKey, venueId }: Props) {
  const [snapshot, setSnapshot] = useState<MealPeriodSnapshot>();
  const [draft, setDraft] = useState({ name: "", startLocalTime: "07:00", endLocalTime: "11:00", activeDaysMask: 127, isEnabled: true, targetLayout: "", menuFilter: "", themePresetKey: "" });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  const refresh = () => loadMealPeriods(configuration, apiKey, venueId).then(setSnapshot);
  useEffect(() => { refresh().catch(() => setError("Meal periods could not be loaded.")); }, [apiKey, configuration, venueId]);

  const create = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined);
    try {
      await createMealPeriod(configuration, apiKey, venueId, {
        ...draft, startLocalTime: wireTime(draft.startLocalTime), endLocalTime: wireTime(draft.endLocalTime)
      });
      setDraft(value => ({ ...value, name: "" })); await refresh();
    } catch { setError("The meal period could not be created."); }
    finally { setBusy(false); }
  };
  const patch = (id: string, value: Partial<MealPeriod>) => setSnapshot(current => current ? {
    ...current, mealPeriods: current.mealPeriods.map(period => period.id === id ? { ...period, ...value } : period)
  } : current);
  const save = async (period: MealPeriod) => {
    setBusy(true); setError(undefined);
    try {
      await updateMealPeriod(configuration, apiKey, venueId, {
        ...period, startLocalTime: wireTime(time(period.startLocalTime)), endLocalTime: wireTime(time(period.endLocalTime))
      });
      await refresh();
    } catch { setError("The meal period could not be saved."); }
    finally { setBusy(false); }
  };
  const remove = async (id: string) => {
    setBusy(true); setError(undefined);
    try { await deleteMealPeriod(configuration, apiKey, venueId, id); await refresh(); }
    catch { setError("The meal period could not be deleted."); }
    finally { setBusy(false); }
  };
  const toggleDay = (mask: number, day: number) => mask ^ (1 << day);

  if (!snapshot) return <p className="state">Loading meal periods…</p>;
  return <article className="menu-editor meal-periods">
    <div className="menu-editor-heading"><div><p>Venue-local schedule</p><h3>Meal periods</h3></div><span>{snapshot.mealPeriods.length} periods</span></div>
    <p>Times use the venue timezone. Overlaps are allowed and resolved by listed priority.</p>
    {error ? <p className="state error">{error}</p> : null}
    {snapshot.conflicts.length ? <aside className="tier-prompt" role="status"><div><strong>Overlapping periods</strong>{snapshot.conflicts.map(item => <p key={`${item.firstId}-${item.secondId}`}>{item.firstName} overlaps {item.secondName}; the first listed period wins.</p>)}</div></aside> : null}
    <form className="section-create" onSubmit={create}>
      <input required maxLength={100} aria-label="Meal period name" placeholder="Breakfast" value={draft.name} onChange={event => setDraft(value => ({ ...value, name: event.target.value }))} />
      <input required type="time" aria-label="Start time" value={draft.startLocalTime} onChange={event => setDraft(value => ({ ...value, startLocalTime: event.target.value }))} />
      <input required type="time" aria-label="End time" value={draft.endLocalTime} onChange={event => setDraft(value => ({ ...value, endLocalTime: event.target.value }))} />
      <select aria-label="Target layout" value={draft.targetLayout} onChange={event => setDraft(value => ({ ...value, targetLayout: event.target.value }))}>
        <option value="">Keep current layout</option><option value="photo_grid">Photo Grid</option><option value="classic_diner">Classic Diner</option><option value="neon_chalkboard">Neon Chalkboard</option><option value="split_layout">Split Layout</option><option value="daily_special_hero">Daily Special Hero</option>
      </select>
      <input maxLength={100} aria-label="Menu filter" placeholder="Optional menu filter" value={draft.menuFilter} onChange={event => setDraft(value => ({ ...value, menuFilter: event.target.value }))} />
      <input maxLength={50} aria-label="Theme preset" placeholder="Optional theme preset" value={draft.themePresetKey} onChange={event => setDraft(value => ({ ...value, themePresetKey: event.target.value }))} />
      <div>{days.map((label, day) => <label key={label}><input type="checkbox" checked={(draft.activeDaysMask & (1 << day)) !== 0} onChange={() => setDraft(value => ({ ...value, activeDaysMask: toggleDay(value.activeDaysMask, day) }))} />{label}</label>)}</div>
      <button disabled={busy || draft.activeDaysMask === 0}>Add period</button>
    </form>
    <div className="menu-sections">{snapshot.mealPeriods.map(period => <section className={period.isEnabled ? "" : "inactive"} key={period.id}>
      <div className="section-row">
        <input aria-label="Period name" maxLength={100} value={period.name} onChange={event => patch(period.id, { name: event.target.value })} />
        <input aria-label="Period start" type="time" value={time(period.startLocalTime)} onChange={event => patch(period.id, { startLocalTime: wireTime(event.target.value) })} />
        <input aria-label="Period end" type="time" value={time(period.endLocalTime)} onChange={event => patch(period.id, { endLocalTime: wireTime(event.target.value) })} />
        <select aria-label="Period target layout" value={period.targetLayout ?? ""} onChange={event => patch(period.id, { targetLayout: event.target.value })}>
          <option value="">Keep current layout</option><option value="photo_grid">Photo Grid</option><option value="classic_diner">Classic Diner</option><option value="neon_chalkboard">Neon Chalkboard</option><option value="split_layout">Split Layout</option><option value="daily_special_hero">Daily Special Hero</option>
        </select>
        <input aria-label="Period menu filter" maxLength={100} value={period.menuFilter ?? ""} onChange={event => patch(period.id, { menuFilter: event.target.value })} />
        <input aria-label="Period theme preset" maxLength={50} value={period.themePresetKey ?? ""} onChange={event => patch(period.id, { themePresetKey: event.target.value })} />
        <button className="activation" disabled={busy} onClick={() => patch(period.id, { isEnabled: !period.isEnabled })}>{period.isEnabled ? "Enabled" : "Disabled"}</button>
        <button disabled={busy || period.activeDaysMask === 0} onClick={() => save(period)}>Save</button>
        <button disabled={busy} onClick={() => remove(period.id)}>Delete</button>
      </div>
      <div>{days.map((label, day) => <label key={label}><input type="checkbox" checked={(period.activeDaysMask & (1 << day)) !== 0} onChange={() => patch(period.id, { activeDaysMask: toggleDay(period.activeDaysMask, day) })} />{label}</label>)}</div>
    </section>)}</div>
  </article>;
}

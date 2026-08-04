import { useEffect, useMemo, useState } from "react";
import {
  loadManagedScreens,
  loadMealPeriods,
  loadMenuEditor,
  updateQuickAvailability,
  updateQuickDailySpecial,
  type ManagedScreen,
  type MealPeriodSnapshot,
  type MenuEditorSnapshot,
  type MenuItem
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import EmptyState from "./EmptyState";
import SkyIcon from "./SkyIcon";
import LoadingSkeleton from "./LoadingSkeleton";

type Props = {
  configuration: BackOfficeConfiguration;
  accessToken: string;
  venueId: string;
  venueName: string;
  capabilities: string[];
};

type HomeState = { mealPeriods: MealPeriodSnapshot; screens: ManagedScreen[]; menu: MenuEditorSnapshot };
const unavailableMealPeriods: MealPeriodSnapshot = { mealPeriods: [], conflicts: [] };
const unavailableMenu: MenuEditorSnapshot = { menus: [], itemGroups: [], capabilities: { happyHour: false, allergenBadges: false, quickUpdate: false } };

const localTime = (value?: string) => value
  ? new Intl.DateTimeFormat(undefined, { hour: "numeric", minute: "2-digit" }).format(new Date(value))
  : "Venue time unavailable";

export default function DaypartHome({ configuration, accessToken, venueId, venueName, capabilities }: Props) {
  const [state, setState] = useState<HomeState>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [busyItem, setBusyItem] = useState<string>();
  const [special, setSpecial] = useState("");

  const load = async () => {
    setError(undefined);
    try {
      const [mealPeriods, screens, menu] = await Promise.all([
        capabilities.includes("scheduling") ? loadMealPeriods(configuration, accessToken, venueId) : Promise.resolve(unavailableMealPeriods),
        capabilities.includes("screens") ? loadManagedScreens(configuration, accessToken, venueId) : Promise.resolve([]),
        capabilities.includes("menus") ? loadMenuEditor(configuration, accessToken, venueId) : Promise.resolve(unavailableMenu)
      ]);
      setState({ mealPeriods, screens, menu });
      setSpecial(menu.menus.find(entry => entry.menu.isActive)?.menu.dailySpecial ?? "");
    } catch {
      setError("Today’s venue overview could not be loaded. No live settings were changed.");
    }
  };

  useEffect(() => { void load(); }, [accessToken, capabilities, configuration, venueId]);

  const activePeriod = state?.mealPeriods.mealPeriods.find(period => period.id === state.mealPeriods.activeMealPeriodId);
  const nextPeriod = state?.mealPeriods.mealPeriods.find(period => period.id === state.mealPeriods.nextMealPeriodId);
  const quickItems = useMemo(() => state?.menu.itemGroups.flatMap(group => group.items.map(item => ({ ...item, sectionId: group.sectionId }))).slice(0, 8) ?? [], [state]);
  const activeMenu = state?.menu.menus.find(entry => entry.menu.isActive)?.menu;

  const toggleAvailability = async (item: MenuItem & { sectionId: string }) => {
    if (!activeMenu || busyItem) return;
    setBusyItem(item.id); setNotice(undefined); setError(undefined);
    try {
      await updateQuickAvailability(configuration, accessToken, venueId, activeMenu.id, item.sectionId, item.id, !item.isAvailable);
      setState(current => current ? { ...current, menu: { ...current.menu, itemGroups: current.menu.itemGroups.map(group => ({ ...group, items: group.items.map(row => row.id === item.id ? { ...row, isAvailable: !row.isAvailable } : row) })) } } : current);
      setNotice(`${item.name} is now ${item.isAvailable ? "86’d" : "available"}; screens were queued to refresh.`);
    } catch { setError(`${item.name} could not be updated. Its current availability remains unchanged.`); }
    finally { setBusyItem(undefined); }
  };

  const saveSpecial = async () => {
    if (!activeMenu) return;
    setNotice(undefined); setError(undefined);
    try {
      await updateQuickDailySpecial(configuration, accessToken, venueId, activeMenu.id, special.trim() || undefined);
      setNotice(special.trim() ? "Today’s special was saved and screens were queued to refresh." : "Today’s special was cleared and screens were queued to refresh.");
    } catch { setError("Today’s special could not be saved. The current special remains live."); }
  };

  if (!state && !error) return <LoadingSkeleton label="Loading today’s venue home" rows={5} />;
  if (!state) return <div className="state error" role="alert"><p>{error}</p><button type="button" onClick={() => void load()}>Retry venue overview</button></div>;

  return <div className="daypart-home">
    <section className="daypart-hero" aria-labelledby="daypart-home-heading">
      <div><p>Right now · {localTime(state.mealPeriods.venueLocalNow)}</p><h2 id="daypart-home-heading">{activePeriod ? activePeriod.name : "Normal service"} at {venueName}</h2><span>{nextPeriod && state.mealPeriods.nextStartsLocal ? `${nextPeriod.name} begins ${localTime(state.mealPeriods.nextStartsLocal)}` : "No later daypart is scheduled."}</span></div>
      {capabilities.includes("scheduling")
        ? <a href="?schedule=emergency#/schedules" className="emergency-home-action"><SkyIcon name="refresh" size={18} />Open emergency controls</a>
        : <span className="emergency-home-action" aria-disabled="true"><SkyIcon name="refresh" size={18} />Emergency controls locked</span>}
    </section>

    {notice ? <p className="screen-notice" role="status" aria-live="polite">{notice}</p> : null}
    {error ? <p className="state error" role="alert">{error}</p> : null}

    <section className="daypart-timeline" aria-labelledby="daypart-timeline-heading"><div><p>Venue timeline</p><h3 id="daypart-timeline-heading">Today’s dayparts</h3></div>
      {!capabilities.includes("scheduling") ? <p className="state">Daypart scheduling is unavailable for this venue.</p>
      : state.mealPeriods.mealPeriods.length ? <ol>{state.mealPeriods.mealPeriods.filter(period => period.isEnabled).map(period => <li key={period.id} data-active={period.id === activePeriod?.id}><strong>{period.name}</strong><span>{period.startLocalTime}–{period.endLocalTime}</span>{period.id === activePeriod?.id ? <em>Now</em> : null}</li>)}</ol>
      : <EmptyState icon="refresh" title="No dayparts yet" message="Normal venue content remains active." action={<a href="?schedule=meal-periods#/schedules">Set up dayparts</a>} />}</section>

    <div className="daypart-grid">
      <section aria-labelledby="live-screens-heading"><header><div><p>Live screens</p><h3 id="live-screens-heading">Fleet at a glance</h3></div><a href="#/screens">Manage</a></header>
        {!capabilities.includes("screens") ? <p className="state">Screen status is unavailable for this venue.</p>
        : state.screens.length ? <ul className="screen-miniatures">{state.screens.slice(0, 6).map(screen => <li key={screen.id}><span aria-hidden="true"><SkyIcon name="screen" size={22} /></span><div><strong>{screen.name}</strong><small>{screen.location || screen.displayLayout.replaceAll("_", " ")}</small></div><em data-online={screen.status.toLowerCase() === "online"}>{screen.status}</em></li>)}</ul>
        : <EmptyState icon="screen" title="No screens paired" message="Pair a screen to see live venue status." action={<a href="#/screens">Pair a screen</a>} />}</section>

      <section aria-labelledby="availability-heading"><header><div><p>86 board</p><h3 id="availability-heading">Quick availability</h3></div><a href="#/menu">Open menu</a></header>
        {!capabilities.includes("menus") ? <p className="state">Menu availability is unavailable for this venue.</p>
        : quickItems.length ? <ul className="availability-list">{quickItems.map(item => <li key={item.id}><span><strong>{item.name}</strong><small>{item.quantityAvailable == null ? "Manual availability" : `${item.quantityAvailable} remaining`}</small></span><button type="button" disabled={busyItem === item.id || !state.menu.capabilities.quickUpdate} aria-pressed={!item.isAvailable} onClick={() => void toggleAvailability(item)}>{item.isAvailable ? "86 item" : "Restore"}</button></li>)}</ul>
        : <EmptyState icon="search" title="No menu items" message="Add menu items before using the 86 board." action={<a href="#/menu">Open menu</a>} />}</section>
    </div>

    <section className="specials-card" aria-labelledby="specials-heading"><div><p>Today’s special</p><h3 id="specials-heading">Featured on venue screens</h3><span>Saving queues the authoritative menu update for active screens.</span></div>
      {activeMenu ? <form onSubmit={event => { event.preventDefault(); void saveSpecial(); }}><label htmlFor="today-special">Special<input id="today-special" value={special} maxLength={200} onChange={event => setSpecial(event.target.value)} placeholder="Add today’s special" /></label><button type="submit" disabled={!state.menu.capabilities.quickUpdate}>Save special</button></form>
      : <p className="state">Create and activate a menu before adding a special.</p>}
    </section>
  </div>;
}

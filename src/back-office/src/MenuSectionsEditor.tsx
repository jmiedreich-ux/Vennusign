import { useEffect, useState, type FormEvent } from "react";
import { createMenu, createMenuSection, loadMenuEditor, reorderMenuSections, updateMenuSection, type MenuEditorSnapshot, type MenuSection } from "./api";
import type { BackOfficeConfiguration } from "./config";
import MenuItemsEditor from "./MenuItemsEditor";
import QuickUpdateMode from "./QuickUpdateMode";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; starterMenu?: "restaurant" | "cafe" | "bar" };
type FailedSectionAction = { label: string; run: () => Promise<void> };

const starterNames = { restaurant: "Lunch & Dinner", cafe: "Cafe Menu", bar: "Drinks & Tap List" } as const;

export default function MenuSectionsEditor({ configuration, apiKey, venueId, starterMenu }: Props) {
  const storageKey = `vennusign.menu.sections.${venueId}`;
  const [snapshot, setSnapshot] = useState<MenuEditorSnapshot>();
  const [selectedMenuId, setSelectedMenuId] = useState("");
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>(() => {
    try { return JSON.parse(localStorage.getItem(storageKey) ?? "{}") as Record<string, boolean>; }
    catch { return {}; }
  });
  const [newMenuName, setNewMenuName] = useState(starterMenu ? starterNames[starterMenu] : "");
  const [newSectionName, setNewSectionName] = useState("");
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [tierPrompt, setTierPrompt] = useState<{ title: string; message: string }>();
  const [busy, setBusy] = useState(false);
  const [pendingArchive, setPendingArchive] = useState<MenuSection>();
  const [failedAction, setFailedAction] = useState<FailedSectionAction>();

  const refresh = async () => {
    const next = await loadMenuEditor(configuration, apiKey, venueId);
    setSnapshot(next);
    setSelectedMenuId(current => next.menus.some(entry => entry.menu.id === current) ? current : next.menus.find(entry => entry.menu.isActive)?.menu.id ?? next.menus[0]?.menu.id ?? "");
  };
  useEffect(() => { refresh().catch(() => setError("Menus could not be loaded. Retry to recover the venue editor.")); }, [apiKey, configuration, venueId]);
  useEffect(() => { localStorage.setItem(storageKey, JSON.stringify(collapsed)); }, [collapsed, storageKey]);

  const selectedMenu = snapshot?.menus.find(entry => entry.menu.id === selectedMenuId);
  const run = async (label: string, action: () => Promise<void>) => {
    setBusy(true); setError(undefined); setNotice(undefined); setFailedAction(undefined);
    try { await action(); await refresh(); setNotice(`${label} saved.`); }
    catch { setError(`${label} could not be saved.`); setFailedAction({ label, run: action }); }
    finally { setBusy(false); }
  };
  const createNewMenu = async (event: FormEvent) => {
    event.preventDefault();
    const name = newMenuName.trim();
    if (!name) return;
    await run("Menu", async () => { const menu = await createMenu(configuration, apiKey, name); setNewMenuName(""); setSelectedMenuId(menu.id); });
  };
  const createSection = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedMenu || !newSectionName.trim()) return;
    await run("Section", async () => { await createMenuSection(configuration, apiKey, venueId, selectedMenu.menu.id, newSectionName); setNewSectionName(""); });
  };
  const save = (section: MenuSection, patch: Partial<MenuSection>, label = "Section") => run(label, async () => { await updateMenuSection(configuration, apiKey, venueId, { ...section, ...patch }); });
  const move = async (index: number, delta: number) => {
    if (!selectedMenu) return;
    const sections = [...selectedMenu.sections];
    const target = index + delta;
    if (target < 0 || target >= sections.length) return;
    [sections[index], sections[target]] = [sections[target], sections[index]];
    await run("Section order", async () => { await reorderMenuSections(configuration, apiKey, venueId, selectedMenu.menu.id, sections.map(section => section.id)); });
  };
  const rename = (sectionId: string, name: string) => setSnapshot(current => current ? {
    ...current,
    menus: current.menus.map(menu => ({ ...menu, sections: menu.sections.map(section => section.id === sectionId ? { ...section, name } : section) }))
  } : current);

  if (!snapshot) return <div className="state"><p>Loading menu editor…</p>{error ? <button type="button" onClick={() => void refresh()}>Retry menus</button> : null}</div>;

  return <article className="menu-editor">
    <div className="menu-editor-heading"><div><p>Venue menus</p><h2>Menu lifecycle</h2></div><span role="status">{busy ? "Saving…" : notice ?? `${snapshot.menus.length} menus`}</span></div>
    {starterMenu ? <p className="state" role="status"><strong>{starterNames[starterMenu]} starter selected.</strong> Review the draft name, then choose Create menu. No content has been created yet.</p> : null}
    {error ? <div className="state error" role="alert"><p>{error}</p>{failedAction ? <button type="button" onClick={() => void run(failedAction.label, failedAction.run)}>Retry last change</button> : <button type="button" onClick={() => void refresh()}>Retry menus</button>}</div> : null}
    {tierPrompt ? <aside className="tier-prompt" role="status"><div><strong>{tierPrompt.title}</strong><p>{tierPrompt.message}</p></div><button aria-label="Dismiss tier prompt" onClick={() => setTierPrompt(undefined)}>×</button></aside> : null}
    <div className="menu-lifecycle-toolbar">
      <label>Select menu<select data-testid="menu-picker" value={selectedMenuId} onChange={event => setSelectedMenuId(event.target.value)}>{snapshot.menus.map(entry => <option key={entry.menu.id} value={entry.menu.id}>{entry.menu.name}{entry.menu.isActive ? "" : " (archived)"}</option>)}</select></label>
      <form onSubmit={createNewMenu}><label>New menu<input maxLength={200} required value={newMenuName} onChange={event => setNewMenuName(event.target.value)} placeholder="Lunch menu" /></label><button disabled={busy}>Create menu</button></form>
    </div>
    {!selectedMenu ? <section className="state"><p>No menu exists for this venue. Create one to begin.</p></section> : <>
      <QuickUpdateMode configuration={configuration} apiKey={apiKey} venueId={venueId} snapshot={snapshot} menuId={selectedMenu.menu.id} onChanged={refresh} />
      <form className="section-create" onSubmit={createSection}><input aria-label="New section name" maxLength={120} required value={newSectionName} onChange={event => setNewSectionName(event.target.value)} placeholder="Add a section" /><button disabled={busy}>Add section</button></form>
      {pendingArchive ? <section className="destructive-review" aria-labelledby="section-archive-title"><h3 id="section-archive-title">Archive {pendingArchive.name}?</h3><p>The section and its items will be hidden from active menus. It remains available to restore.</p><div><button type="button" onClick={() => setPendingArchive(undefined)}>Cancel</button><button className="danger" type="button" onClick={() => { const section = pendingArchive; setPendingArchive(undefined); void save(section, { isActive: false }, "Section archive"); }}>Confirm archive</button></div></section> : null}
      <div className="menu-sections">{selectedMenu.sections.map((section, index) => <section className={section.isActive ? "" : "inactive"} key={section.id}>
        <div className="section-row">
          <button type="button" aria-expanded={!collapsed[section.id]} aria-label={`${collapsed[section.id] ? "Expand" : "Collapse"} ${section.name}`} className="collapse" onClick={() => setCollapsed(value => ({ ...value, [section.id]: !value[section.id] }))}>{collapsed[section.id] ? "▸" : "▾"}</button>
          <input aria-label="Section name" disabled={!section.isActive} maxLength={120} value={section.name} onChange={event => rename(section.id, event.target.value)} onBlur={() => void save(section, { name: selectedMenu.sections.find(item => item.id === section.id)?.name ?? section.name }, "Section name")} />
          <button type="button" aria-label={`Move ${section.name} up`} disabled={busy || index === 0} onClick={() => void move(index, -1)}>↑</button><button type="button" aria-label={`Move ${section.name} down`} disabled={busy || index === selectedMenu.sections.length - 1} onClick={() => void move(index, 1)}>↓</button>
          {section.isActive ? <button type="button" className="danger-link" disabled={busy} onClick={() => setPendingArchive(section)}>Archive</button> : <button type="button" disabled={busy} onClick={() => void save(section, { isActive: true }, "Section restore")}>Restore</button>}
        </div>
        {!collapsed[section.id] ? <MenuItemsEditor configuration={configuration} apiKey={apiKey} venueId={venueId} menuId={selectedMenu.menu.id} sectionId={section.id} items={snapshot.itemGroups.find(group => group.sectionId === section.id)?.items ?? []} capabilities={snapshot.capabilities} disabled={busy || !section.isActive} onChanged={refresh} onError={message => { setError(message); setNotice(undefined); }} onTierPrompt={(title, message) => setTierPrompt({ title, message })} /> : null}
      </section>)}</div>
    </>}
  </article>;
}

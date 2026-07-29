import { useEffect, useState, type FormEvent } from "react";
import { createMenuSection, loadMenuEditor, reorderMenuSections, updateMenuSection, type MenuEditorSnapshot, type MenuSection } from "./api";
import type { AdminConfiguration } from "./config";
import MenuItemsEditor from "./MenuItemsEditor";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string };

export default function MenuSectionsEditor({ configuration, apiKey, venueId }: Props) {
  const storageKey = `vennu.menu.sections.${venueId}`;
  const [snapshot, setSnapshot] = useState<MenuEditorSnapshot>();
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>(() => {
    try { return JSON.parse(localStorage.getItem(storageKey) ?? "{}") as Record<string, boolean>; }
    catch { return {}; }
  });
  const [newName, setNewName] = useState("");
  const [error, setError] = useState<string>();
  const [tierPrompt, setTierPrompt] = useState<{ title: string; message: string }>();
  const [busy, setBusy] = useState(false);

  const refresh = () => loadMenuEditor(configuration, apiKey, venueId).then(setSnapshot);
  useEffect(() => { refresh().catch(() => setError("Menu sections could not be loaded.")); }, [apiKey, configuration, venueId]);
  useEffect(() => { localStorage.setItem(storageKey, JSON.stringify(collapsed)); }, [collapsed, storageKey]);

  const firstMenu = snapshot?.menus[0];
  const create = async (event: FormEvent) => {
    event.preventDefault();
    if (!firstMenu) return;
    setBusy(true); setError(undefined);
    try { await createMenuSection(configuration, apiKey, venueId, firstMenu.menu.id, newName); setNewName(""); await refresh(); }
    catch { setError("The section could not be created."); }
    finally { setBusy(false); }
  };
  const save = async (section: MenuSection, patch: Partial<MenuSection>) => {
    setBusy(true); setError(undefined);
    try { await updateMenuSection(configuration, apiKey, venueId, { ...section, ...patch }); await refresh(); }
    catch { setError("The section could not be updated."); }
    finally { setBusy(false); }
  };
  const move = async (index: number, delta: number) => {
    if (!firstMenu) return;
    const sections = [...firstMenu.sections];
    const target = index + delta;
    if (target < 0 || target >= sections.length) return;
    [sections[index], sections[target]] = [sections[target], sections[index]];
    setBusy(true); setError(undefined);
    try { await reorderMenuSections(configuration, apiKey, venueId, firstMenu.menu.id, sections.map(section => section.id)); await refresh(); }
    catch { setError("The section order could not be saved."); }
    finally { setBusy(false); }
  };
  const rename = (sectionId: string, name: string) => setSnapshot(current => current ? {
    ...current,
    menus: current.menus.map(menu => ({ ...menu, sections: menu.sections.map(section => section.id === sectionId ? { ...section, name } : section) }))
  } : current);

  if (!snapshot) return <p className="state">Loading menu editor…</p>;
  if (!firstMenu) return <article className="menu-editor"><h3>Menu editor</h3><p>No menu exists for this venue.</p></article>;

  return <article className="menu-editor">
    <div className="menu-editor-heading"><div><p>Venue menu</p><h3>{firstMenu.menu.name}</h3></div><span>{firstMenu.sections.length} sections</span></div>
    {error ? <p className="state error">{error}</p> : null}
    {tierPrompt ? <aside className="tier-prompt" role="status"><div><strong>{tierPrompt.title}</strong><p>{tierPrompt.message}</p></div><button aria-label="Dismiss tier prompt" onClick={() => setTierPrompt(undefined)}>×</button></aside> : null}
    <form className="section-create" onSubmit={create}><input aria-label="New section name" maxLength={120} required value={newName} onChange={event => setNewName(event.target.value)} placeholder="Add a section" /><button disabled={busy}>Add section</button></form>
    <div className="menu-sections">{firstMenu.sections.map((section, index) => <section className={section.isActive ? "" : "inactive"} key={section.id}>
      <div className="section-row">
        <button aria-label={`${collapsed[section.id] ? "Expand" : "Collapse"} ${section.name}`} className="collapse" onClick={() => setCollapsed(value => ({ ...value, [section.id]: !value[section.id] }))}>{collapsed[section.id] ? "▸" : "▾"}</button>
        <input aria-label="Section name" maxLength={120} value={section.name} onChange={event => rename(section.id, event.target.value)} onBlur={() => save(section, { name: firstMenu.sections.find(item => item.id === section.id)?.name ?? section.name })} />
        <button disabled={busy || index === 0} onClick={() => move(index, -1)}>↑</button><button disabled={busy || index === firstMenu.sections.length - 1} onClick={() => move(index, 1)}>↓</button>
        <button className="activation" disabled={busy} onClick={() => save(section, { isActive: !section.isActive })}>{section.isActive ? "Active" : "Hidden"}</button>
      </div>
      {!collapsed[section.id] ? <MenuItemsEditor
        configuration={configuration}
        apiKey={apiKey}
        venueId={venueId}
        menuId={firstMenu.menu.id}
        sectionId={section.id}
        items={snapshot.itemGroups.find(group => group.sectionId === section.id)?.items ?? []}
        capabilities={snapshot.capabilities}
        disabled={busy}
        onChanged={refresh}
        onError={() => setError("The menu item could not be saved.")}
        onTierPrompt={(title, message) => setTierPrompt({ title, message })}
      /> : null}
    </section>)}</div>
  </article>;
}

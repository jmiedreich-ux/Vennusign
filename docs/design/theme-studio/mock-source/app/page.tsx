"use client";

import { useMemo, useState } from "react";

type Screen = "library" | "studio" | "create" | "repair" | "save" | "tests" | "menu";
type Mode = "Edit" | "Test";
type Dataset = "Typical" | "Busy" | "Maximum" | "Long text" | "Missing images" | "Sold out";
type RailPanel = "Fields" | "Layouts" | "Components" | "Elements" | "Assets" | "Pages" | "Variants";
type Resolution = "1920 × 1080" | "3840 × 2160";
type Orientation = "Landscape" | "Portrait";
type StartingPoint = "Northside menu" | "Generate a layout" | "Begin blank";
type StudioEntry = "theme" | "new" | "menu";
type TemplateDraft = {
  model: "Menu";
  resolution: Resolution;
  orientation: Orientation;
  safeArea: "5%" | "Custom";
  customSafeArea: number;
  startingPoint: StartingPoint;
  guidedBuild: boolean;
};
type FieldPayload = { label: string; field: string; type: string };
type DragPoint = { x: number; y: number };
type GuideAction = "layout" | "component" | "field" | "style" | "behavior" | "test" | "save";
type CanvasColumn = "left" | "right";

const themeLibrary = [
  { name: "Northside Menu", detail: "Two-column dinner menu · Used by 2 menus", updated: "Last saved Aug 10", visual: "northside" },
  { name: "Midnight Tapboard", detail: "Dense tap list · Used by 1 menu", updated: "Last saved Aug 8", visual: "tapboard" },
  { name: "Bakery Window", detail: "Portrait product showcase · Not used by a menu", updated: "Last saved yesterday", visual: "bakery" },
];

const defaultDraft: TemplateDraft = {
  model: "Menu",
  resolution: "1920 × 1080",
  orientation: "Landscape",
  safeArea: "5%",
  customSafeArea: 64,
  startingPoint: "Northside menu",
  guidedBuild: false,
};

function orientedResolution(draft: TemplateDraft) {
  const [width, height] = draft.resolution.split(" × ");
  return draft.orientation === "Landscape" ? `${width} × ${height}` : `${height} × ${width}`;
}

type IconName =
  | "data" | "layout" | "components" | "elements" | "assets" | "pages" | "variants"
  | "undo" | "redo" | "test" | "chevron" | "spark" | "close"
  | "warning" | "check" | "text" | "repeater" | "image" | "shape" | "plus"
  | "menu" | "display" | "template" | "blank" | "arrow" | "lock" | "search";

function Icon({ name, size = 18 }: { name: IconName; size?: number }) {
  const paths: Record<IconName, React.ReactNode> = {
    data: <><ellipse cx="12" cy="5" rx="7" ry="3"/><path d="M5 5v6c0 1.7 3.1 3 7 3s7-1.3 7-3V5M5 11v6c0 1.7 3.1 3 7 3s7-1.3 7-3v-6"/></>,
    layout: <><rect x="4" y="4" width="16" height="16" rx="2"/><path d="M10 4v16M10 10h10"/></>,
    components: <><rect x="4" y="4" width="6" height="6" rx="1"/><rect x="14" y="4" width="6" height="6" rx="1"/><rect x="4" y="14" width="6" height="6" rx="1"/><rect x="14" y="14" width="6" height="6" rx="1"/></>,
    elements: <><circle cx="8" cy="8" r="4"/><path d="m15 5 5 8h-10zM5 16h7v5H5z"/></>,
    assets: <><rect x="3" y="5" width="18" height="14" rx="2"/><circle cx="9" cy="10" r="2"/><path d="m5 17 5-4 3 2 3-3 3 5"/></>,
    pages: <><path d="M7 3h8l4 4v14H7z"/><path d="M15 3v5h5M4 7v14h11"/></>,
    variants: <><path d="M5 7h14M5 17h14"/><circle cx="9" cy="7" r="2"/><circle cx="15" cy="17" r="2"/></>,
    undo: <path d="M9 7 5 11l4 4M5 11h8a5 5 0 0 1 5 5"/>,
    redo: <path d="m15 7 4 4-4 4M19 11h-8a5 5 0 0 0-5 5"/>,
    test: <><path d="M9 3h6M10 3v5l-5 9a3 3 0 0 0 2.6 4h8.8a3 3 0 0 0 2.6-4l-5-9V3"/><path d="M8 15h8"/></>,
    chevron: <path d="m9 18 6-6-6-6"/>,
    spark: <><path d="m12 3 1.4 4.6L18 9l-4.6 1.4L12 15l-1.4-4.6L6 9l4.6-1.4z"/><path d="m18.5 15 .7 2.3 2.3.7-2.3.7-.7 2.3-.7-2.3-2.3-.7 2.3-.7z"/></>,
    close: <path d="m6 6 12 12M18 6 6 18"/>,
    warning: <><path d="m12 3 10 18H2z"/><path d="M12 9v5M12 18h.01"/></>,
    check: <path d="m5 12 4 4L19 6"/>,
    text: <><path d="M5 5h14M12 5v14M8 19h8"/></>,
    repeater: <><path d="M7 7h13M7 12h13M7 17h13"/><circle cx="3.5" cy="7" r=".5"/><circle cx="3.5" cy="12" r=".5"/><circle cx="3.5" cy="17" r=".5"/></>,
    image: <><rect x="3" y="4" width="18" height="16" rx="2"/><circle cx="9" cy="10" r="2"/><path d="m5 18 5-5 3 3 2-2 4 4"/></>,
    shape: <><rect x="4" y="4" width="8" height="8" rx="1"/><circle cx="16" cy="16" r="4"/></>,
    plus: <path d="M12 5v14M5 12h14"/>,
    menu: <><path d="M5 6h14M5 12h14M5 18h14"/></>,
    display: <><rect x="3" y="4" width="18" height="14" rx="2"/><path d="M8 21h8M12 18v3"/></>,
    template: <><rect x="4" y="3" width="16" height="18" rx="2"/><path d="M8 8h8M8 12h8M8 16h5"/></>,
    blank: <><rect x="4" y="3" width="16" height="18" rx="2"/><path d="M12 8v8M8 12h8"/></>,
    arrow: <path d="M5 12h14M14 7l5 5-5 5"/>,
    lock: <><rect x="5" y="10" width="14" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/></>,
    search: <><circle cx="11" cy="11" r="7"/><path d="m16 16 4 4"/></>,
  };
  return <svg aria-hidden="true" viewBox="0 0 24 24" width={size} height={size} fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round">{paths[name]}</svg>;
}

const datasets: Dataset[] = ["Typical", "Busy", "Maximum", "Long text", "Missing images", "Sold out"];

const menuItems = {
  Typical: [["Charred broccoli", "$11"], ["Hot honey chicken", "$16"], ["Crispy potatoes", "$9"], ["House chopped salad", "$13"]],
  Busy: [["Charred broccoli", "$11"], ["Hot honey chicken", "$16"], ["Crispy potatoes", "$9"], ["House chopped salad", "$13"], ["Wood-fired carrots", "$10"], ["Steak frites", "$24"], ["Miso salmon", "$21"], ["Fried oyster mushrooms", "$14"]],
  Maximum: [["Charred broccoli", "$11"], ["Hot honey chicken", "$16"], ["Crispy potatoes", "$9"], ["House chopped salad", "$13"], ["Wood-fired carrots", "$10"], ["Steak frites", "$24"], ["Miso salmon", "$21"], ["Fried oyster mushrooms", "$14"], ["Brown butter gnocchi", "$19"], ["Roasted half chicken", "$25"]],
  "Long text": [["Charred broccoli with whipped tahini, preserved lemon and toasted sesame", "$11"], ["Nashville hot honey fried chicken sandwich", "$16"], ["Crispy sea-salt potatoes", "$9"], ["House chopped salad", "$13"]],
  "Missing images": [["Charred broccoli", "$11"], ["Hot honey chicken", "$16"], ["Crispy potatoes", "$9"], ["House chopped salad", "$13"]],
  "Sold out": [["Charred broccoli", "$11"], ["Hot honey chicken", "$16"], ["Crispy potatoes", "$9"], ["House chopped salad", "$13"]],
} as Record<Dataset, string[][]>;

function Logo() {
  return <div className="brand-mark" aria-label="Vennue"><span>V</span></div>;
}

function ThemeLibrary({ onOpen, onCreate, openMenu }: { onOpen: () => void; onCreate: () => void; openMenu: () => void }) {
  const [query, setQuery] = useState("");
  const filteredThemes = themeLibrary.filter(theme => theme.name.toLowerCase().includes(query.toLowerCase()));
  return <div className="library-shell">
    <header className="library-header"><div className="flow-brand"><Logo/><span>Theme Studio</span></div><button className="button quiet" onClick={openMenu}><Icon name="menu"/>Open Menu Builder handoff</button></header>
    <main className="library-main">
      <section className="library-intro"><div><h1>Themes</h1><p>Create a theme or open an existing design. Each theme has one latest saved state.</p></div><button className="button primary" onClick={onCreate}><Icon name="plus"/>Create new</button></section>
      <div className="library-assurance"><Icon name="lock"/><div><strong>Saving a theme never updates a live screen.</strong><span>Menu Builder uses the latest saved theme. Screens change only when the menu is published.</span></div></div>
      <section className="library-section"><div className="library-tools"><strong>Your themes</strong><label className="search library-search"><Icon name="search"/><input aria-label="Search themes" placeholder="Search themes" value={query} onChange={event => setQuery(event.target.value)}/></label></div>
        {filteredThemes.length ? <div className="theme-grid">{filteredThemes.map(theme => <article className="theme-card" key={theme.name}>
          <div className={`theme-thumb ${theme.visual}`}><span>{theme.visual === "northside" ? "NORTHSIDE" : theme.visual === "tapboard" ? "ON TAP" : "TODAY"}</span><i/><i/><i/></div>
          <div className="theme-card-copy"><div><h2>{theme.name}</h2></div><p>{theme.detail}</p><small>{theme.updated}</small></div>
          <div className="theme-card-actions"><button className="button primary" onClick={onOpen}>Open theme</button></div>
        </article>)}</div> : <div className="library-empty"><Icon name="search"/><strong>No matching themes</strong><p>Try another search.</p><button className="button quiet" onClick={() => setQuery("")}>Clear search</button></div>}
      </section>
    </main>
  </div>;
}

function MenuThemeHandoff({ go, editTheme }: { go: (screen: Screen) => void; editTheme: () => void }) {
  const [menuPublished, setMenuPublished] = useState(false);
  return <div className="menu-handoff-shell">
    <header className="library-header"><div className="flow-brand"><Logo/><span>Menu Builder</span></div><button className="button quiet" onClick={() => go("library")}><Icon name="close"/>Close</button></header>
    <main className="menu-handoff-main"><div className="menu-breadcrumb">Menus / Northside Social / Saturday Dinner</div><section className="menu-handoff-heading"><div><h1>Theme</h1><p>Choose the design used by this menu. Menu Builder always shows its latest saved state.</p></div><span className={menuPublished ? "menu-live" : "menu-draft"}>{menuPublished ? "Menu is live" : "Menu draft"}</span></section>
      <div className="menu-handoff-grid"><section className="applied-theme"><div className="theme-thumb northside"><span>NORTHSIDE</span><i/><i/><i/></div><div><span>CURRENT THEME</span><h2>Northside Menu</h2><p>Latest saved theme · applied to this menu</p><div className="applied-actions"><button className="button quiet" onClick={() => go("library")}>Change theme</button><button className="button primary" onClick={editTheme}>Edit in Theme Studio</button></div></div></section><aside className="menu-publish-card"><Icon name={menuPublished ? "check" : "display"}/><h2>{menuPublished ? "Saturday Dinner is published" : "Ready when the menu is"}</h2><p>{menuPublished ? "The menu and its theme are now live on the assigned screens." : "Saving a theme does not affect screens. Publish this menu when its content and design are ready."}</p><button className="button primary" disabled={menuPublished} onClick={() => setMenuPublished(true)}>{menuPublished ? "Published" : "Publish menu to screens"}</button></aside></div>
    </main>
  </div>;
}

function StudioCanvas({ dataset, selected, onSelect, draft, empty, layoutChosen, selectedLayout, onDropField, onDropComponent, repeaterColumn, boundFields, confirmation, guideAction, guidedWorkflow, pendingComponent, pendingField, onClearPendingDrag }: { dataset: Dataset; selected: string; onSelect: (v: string) => void; draft: TemplateDraft; empty: boolean; layoutChosen: boolean; selectedLayout: string; onDropField: (field: FieldPayload) => void; onDropComponent: (component: string, column: CanvasColumn) => void; repeaterColumn: CanvasColumn | null; boundFields: Set<string>; confirmation: string | null; guideAction: GuideAction | null; guidedWorkflow: boolean; pendingComponent: string | null; pendingField: FieldPayload | null; onClearPendingDrag: () => void }) {
  const items = menuItems[dataset];
  const isGenerated = draft.startingPoint === "Generate a layout";
  const isBlankBuild = draft.startingPoint === "Begin blank";
  const [zoom, setZoom] = useState(42);
  const dropComponent = (event: React.DragEvent, column: CanvasColumn) => {
    event.preventDefault();
    const component = event.dataTransfer.getData("application/x-vennue-component");
    if (component) onDropComponent(component, column);
  };
  const dropField = (event: React.DragEvent) => {
    event.preventDefault();
    event.stopPropagation();
    const value = event.dataTransfer.getData("application/x-vennue-field");
    if (value) onDropField(JSON.parse(value) as FieldPayload);
  };
  return (
    <div className="canvas-stage">
      <div className="ruler ruler-x"><span>0</span><span>25%</span><span>50%</span><span>75%</span><span>100%</span></div>
      <div className="ruler ruler-y"><span>0</span><span>33%</span><span>66%</span><span>100%</span></div>
      <div style={{ transform: `scale(${zoom / 42})` }} className={`sign-canvas ${draft.orientation.toLowerCase()} ${isBlankBuild ? "blank-canvas" : ""} ${!guidedWorkflow && guideAction === "field" ? "guided-drop-target" : ""}`} aria-label={`${draft.orientation} menu preview using ${dataset} data`} onDragOver={!guidedWorkflow ? event => { event.preventDefault(); event.dataTransfer.dropEffect = "copy"; } : undefined} onDrop={!guidedWorkflow ? event => { event.preventDefault(); const value = event.dataTransfer.getData("application/x-vennue-field"); if (value) onDropField(JSON.parse(value) as FieldPayload); } : undefined}>
        <div className={`safe-area ${draft.safeArea === "Custom" ? "custom" : ""}`} style={draft.safeArea === "Custom" ? { inset: `${Math.min(15, Math.max(2, draft.customSafeArea / 20))}%` } : undefined}/>
        {guidedWorkflow ? (!layoutChosen ? <><div className="layout-wireframe" aria-hidden="true"><i/><i/><i/></div><div className="canvas-empty"><span><Icon name="layout" size={22}/></span><strong>Choose a layout to start</strong><p>Layouts establish the page regions before content is added.</p></div></> : <div className={`blank-layout ${selectedLayout.toLowerCase().replaceAll(" ", "-")}`}>
          {(["left", "right"] as CanvasColumn[]).map(column => <section key={column} className={`canvas-column-region ${guideAction === "component" ? "guided-column-target" : ""} ${repeaterColumn === column ? "has-component" : ""}`} onDragOver={event => { event.preventDefault(); event.dataTransfer.dropEffect = "copy"; }} onDrop={event => dropComponent(event, column)} onPointerUp={() => { if (pendingComponent) onDropComponent(pendingComponent, column); onClearPendingDrag(); }} aria-label={`${column === "left" ? "Left" : "Right"} column drop area`}>
            <span className="column-name">{column === "left" ? "Left column" : "Right column"}</span>
            {repeaterColumn === column ? <button className={`blank-repeater ${selected === "repeater" ? "selected" : ""} ${guideAction === "field" ? "guided-repeater-target" : ""}`} onClick={() => onSelect("repeater")} onDragOver={event => { event.preventDefault(); event.stopPropagation(); event.dataTransfer.dropEffect = "copy"; }} onDrop={dropField} onPointerUp={event => { event.stopPropagation(); if (pendingField) onDropField(pendingField); onClearPendingDrag(); }} aria-label={`Menu item repeater in ${column} column`}>
              <span className="blank-repeater-label"><Icon name="repeater" size={13}/>Menu item repeater</span>
              {boundFields.has("item.name") ? <div className="blank-repeater-rows">{items.slice(0, 4).map(([name, price], index) => <span className="blank-repeater-row" key={`${name}-${index}`}><strong>{name}</strong>{boundFields.has("item.price") && <em>{price}</em>}</span>)}</div> : <span className="blank-repeater-empty"><Icon name="text" size={17}/><strong>Empty item list</strong><small>Drop Item name here</small></span>}
              {guideAction === "field" && <span className="drop-instruction">Drop Item name inside this repeater</span>}
            </button> : <div className="empty-column"><Icon name="plus" size={18}/><strong>Drop component here</strong><span>{column === "left" ? "Build the left side" : "Build the right side"}</span></div>}
          </section>)}
        </div>) : empty ? <><div className={`layout-wireframe ${layoutChosen ? `chosen ${selectedLayout.toLowerCase().replaceAll(" ", "-")}` : ""}`} aria-hidden="true"><i/><i/><i/></div><div className="canvas-empty"><span><Icon name={layoutChosen ? "components" : "layout"} size={22}/></span><strong>{layoutChosen ? `${selectedLayout} ready` : "Choose a layout to start"}</strong><p>{layoutChosen ? "Next, add a component from the left panel." : "Layouts establish the page regions before content is added."}</p></div></> : <>
        <div className="menu-brand">{isGenerated ? <>YOUR <span>VENUE</span></> : <>NORTHSIDE <span>SOCIAL</span></>}</div>
        <div className="menu-meta">{isGenerated ? "DINNER • MENU" : "SATURDAY • DINNER"}</div>
        <button className={`canvas-selection title-selection ${selected === "title" ? "selected" : ""}`} onClick={() => onSelect("title")} aria-label="Select Small plates page title">
          <span className="selection-label">Page title · menu.page.title</span>
          <h2>{isGenerated ? "DINNER MENU" : "SMALL PLATES"}</h2>
        </button>
        <button className={`canvas-selection repeater-selection ${selected === "repeater" ? "selected" : ""}`} onClick={() => onSelect("repeater")} aria-label="Select Menu item repeater">
          <span className="selection-label">Menu item repeater · section.items</span>
          <div className={`menu-list ${dataset === "Maximum" ? "is-overflow" : ""}`}>
            {items.map(([name, price], index) => (
              <div className={`menu-row ${dataset === "Sold out" && index === 1 ? "sold-out" : ""}`} key={`${name}-${index}`}>
                <div><strong>{name}</strong><p>{dataset === "Long text" && index === 0 ? "smoked almond, tahini, lemon oil" : "seasonal ingredients · made to order"}</p></div>
                <span>{dataset === "Sold out" && index === 1 ? "SOLD OUT" : price}</span>
              </div>
            ))}
          </div>
          {dataset === "Maximum" && <div className="overflow-mark">+148 px</div>}
        </button>
        <div className="menu-footer"><span>SHARE A FEW.</span><span>KEEP ONE.</span></div>
        </>}
        {confirmation && <div className="binding-toast" role="status"><Icon name="check" size={13}/>{confirmation}</div>}
      </div>
      <div className="zoom-control"><button aria-label="Zoom out" disabled={zoom === 34} onClick={() => setZoom(value => Math.max(34, value - 4))}>−</button><span>{zoom}%</span><button aria-label="Zoom in" disabled={zoom === 50} onClick={() => setZoom(value => Math.min(50, value + 4))}>+</button></div>
    </div>
  );
}

function PanelHeader({ section, title }: { section: string; title: string }) {
  return <div className="panel-title"><div><span>{section}</span><h2>{title}</h2></div></div>;
}

function LeftPanel({ panel, active, onSelect, draft, selectedLayout, onLayout, onBehaviorConfigured, guideAction, guidedWorkflow, onBeginComponentDrag, onBeginFieldDrag, onCancelPendingDrag }: { panel: RailPanel; active: string; onSelect: (value: string) => void; draft: TemplateDraft; selectedLayout: string; onLayout: (layout: string) => void; onBehaviorConfigured: () => void; guideAction: GuideAction | null; guidedWorkflow: boolean; onBeginComponentDrag: (component: string, point: DragPoint) => void; onBeginFieldDrag: (field: FieldPayload, point: DragPoint) => void; onCancelPendingDrag: () => void }) {
  const [sourceConnected, setSourceConnected] = useState(draft.startingPoint === "Northside menu");
  const [selectedVariant, setSelectedVariant] = useState("Default");
  const [pageCount, setPageCount] = useState(2);
  const [selectedPage, setSelectedPage] = useState(1);
  const [variantCreated, setVariantCreated] = useState(false);
  const fieldGroups = [
    { label: "Menu", fields: [["Menu name", "menu.name", "text"], ["Page title", "page.title", "text"]] },
    { label: "Section", fields: [["Section title", "section.title", "text"]] },
    { label: "Item", fields: [["Item name", "item.name", "text"], ["Description", "item.description", "text"], ["Price", "item.price", "currency"], ["Image", "item.image", "image"], ["Availability", "item.availability", "state"]] },
  ] as const;
  const components = [
    ["Page title", "menu.page.title", "text"],
    ["Section repeater", "menu.page.sections", "repeater"],
    ["Menu item repeater", "section.items", "repeater"],
    ["Menu item image", "item.image", "image"],
  ] as const;
  if (panel === "Fields") return <aside className="left-panel">
    <PanelHeader section="MENU" title="Fields"/>
    <div className={`source-summary ${sourceConnected ? "connected" : "empty"}`}>
      <div><span className="source-icon"><Icon name="data"/></span><span><strong>{sourceConnected ? "Northside Social" : "No source connected"}</strong><small>{sourceConnected ? "Connected · 2 pages · 16 items" : "Sample data is shown on the canvas"}</small></span></div>
      <button onClick={() => setSourceConnected(value => !value)}>{sourceConnected ? "Disconnect" : "Connect sample"}</button>
    </div>
    <div className="panel-subsection field-browser">
      <div className="subsection-title"><strong>Available fields</strong><span>8</span></div>
      {fieldGroups.map(group => <section className="field-group" key={group.label}>
        <h3>{group.label}</h3>
        {group.fields.map(([label, field, type]) => {
          const pointerGuided = guidedWorkflow && guideAction === "field" && field === "item.name";
          const payload = { label, field, type };
          return <button className={`field-row ${pointerGuided ? "guided-target" : ""}`} key={field} draggable={!pointerGuided} onPointerDown={event => { if (pointerGuided) onBeginFieldDrag(payload, { x: event.clientX, y: event.clientY }); }} onClick={onCancelPendingDrag} onDragStart={event => { event.dataTransfer.effectAllowed = "copy"; event.dataTransfer.setData("application/x-vennue-field", JSON.stringify(payload)); }}><span className="binding-pin"/><span><strong>{label}</strong><small>{field} · {type}</small></span><span className="drag-dots" aria-label={`Drag ${label} onto canvas`}>••<br/>••</span></button>;
        })}
      </section>)}
    </div>
    <div className="panel-note"><Icon name="data"/><div><strong>Drag fields onto the canvas</strong><p>Drop a field inside a compatible component to bind and style it.</p></div></div>
  </aside>;
  if (panel === "Layouts") return <aside className="left-panel"><PanelHeader section="STRUCTURE" title="Layouts"/><div className="layout-context"><span>Current page</span><strong>Dinner page</strong></div><div className="panel-subsection first"><div className="subsection-title"><strong>Choose a page layout</strong></div>{[["Two-column menu","Balanced menu columns","layout"],["Split feature","Feature image + menu","template"],["Single column","Focused vertical list","pages"]].map(([name,description,icon])=><button onClick={() => onLayout(name)} aria-pressed={selectedLayout === name} className={`preset-row ${selectedLayout === name ? "active" : ""} ${guideAction === "layout" && name === "Two-column menu" ? "guided-target" : ""}`} key={name}><span className="component-icon"><Icon name={icon as IconName}/></span><span><strong>{name}</strong><small>{description}</small></span>{selectedLayout === name ? <Icon name="check" size={14}/> : <Icon name="chevron" size={14}/>}</button>)}</div><div className="panel-note"><Icon name="layout"/><div><strong>Layouts arrange the page</strong><p>Choosing one replaces the current page structure. Styling stays in Settings.</p></div></div></aside>;
  if (panel === "Elements") return <aside className="left-panel"><PanelHeader section="BASIC" title="Elements"/><label className="search"><svg viewBox="0 0 24 24"><circle cx="11" cy="11" r="7"/><path d="m16 16 4 4"/></svg><input aria-label="Search elements" placeholder="Search elements" /></label><div className="element-grid">{[["Text","text"],["Image","image"],["Logo","assets"],["Shape","shape"],["Line","variants"],["Safe area","layout"]].map(([name,icon])=><button onClick={() => onSelect(name.toLowerCase())} key={name}><span><Icon name={icon as IconName}/></span><strong>{name}</strong></button>)}</div><div className="panel-note"><Icon name="spark"/><div><strong>Use structure first</strong><p>Basic elements are best for decoration. Use components for data-bound content.</p></div></div></aside>;
  if (panel === "Assets") return <aside className="left-panel"><PanelHeader section="LIBRARY" title="Assets"/><div className="asset-actions"><button className="button primary" onClick={() => onSelect("asset:upload")}><Icon name="plus"/>Add asset</button><button className="button quiet" onClick={() => onSelect("asset:font")}>Fonts</button></div><div className="asset-grid"><button onClick={() => onSelect("asset:logo")}><span className="asset-thumb logo-thumb">N</span><strong>Northside logo</strong><small>SVG · 24 KB</small></button><button onClick={() => onSelect("asset:image")}><span className="asset-thumb food-thumb"/><strong>Charred broccoli</strong><small>JPG · 1.2 MB</small></button><button onClick={() => onSelect("asset:font")}><span className="asset-thumb font-thumb">Ag</span><strong>Aptos Display</strong><small>Font family · 6 weights</small></button></div><div className="panel-note"><Icon name="check"/><div><strong>Ready to save</strong><p>All used assets will be available wherever this theme is used.</p></div></div></aside>;
  if (panel === "Pages") return <aside className="left-panel"><PanelHeader section="STRUCTURE" title="Pages"/><div className="page-list"><button onClick={() => setSelectedPage(1)} className={`page-row ${selectedPage === 1 ? "active" : ""}`}><span className="page-preview"><i/><i/></span><span><strong>Dinner page</strong><small>4 sections · capacity 14</small></span><em>1</em></button><button onClick={() => setSelectedPage(2)} className={`page-row ${selectedPage === 2 ? "active" : ""}`}><span className="page-preview continuation"><i/><i/></span><span><strong>Continuation</strong><small>Generated only when capacity is exceeded</small></span><em>2</em></button>{pageCount > 2 && <button onClick={() => setSelectedPage(pageCount)} className={`page-row ${selectedPage === pageCount ? "active" : ""}`}><span className="page-preview"><i/><i/></span><span><strong>Page {pageCount}</strong><small>Empty page</small></span><em>{pageCount}</em></button>}<button className="add-row" onClick={() => { setPageCount(value => value + 1); setSelectedPage(pageCount + 1); }}><Icon name="plus"/>Add page</button></div><div className="panel-subsection"><div className="subsection-title"><strong>Page rules</strong></div><label className="switch-row panel-switch"><span>Allow continuation pages</span><input type="checkbox" defaultChecked/></label><label className="switch-row panel-switch"><span>Keep section together</span><input type="checkbox" defaultChecked/></label></div></aside>;
  if (panel === "Variants") return <aside className="left-panel"><PanelHeader section="RESPONSIVE RULES" title="Variants"/><div className="variant-list">{[["Default","Typical content","Active"],["Dense","More than 12 items","Automatic"],["Sold out","Unavailable items exist","Automatic"],["Promotional","Promotion is active","Optional"],...(variantCreated ? [["Weekend","Friday and Saturday","Optional"]] : [])].map(([name,rule,status],index)=><button onClick={() => { setSelectedVariant(name); onBehaviorConfigured(); }} className={`variant-row ${selectedVariant === name ? "active" : ""} ${guideAction === "behavior" && name === "Sold out" ? "guided-target" : ""}`} key={name}><span className={`variant-preview v${Math.min(index,3)}`}><i/><i/><i/></span><span><strong>{name}</strong><small>{rule}</small></span><em>{status}</em></button>)}</div><button className="add-row" disabled={variantCreated} onClick={() => { setVariantCreated(true); setSelectedVariant("Weekend"); onBehaviorConfigured(); }}><Icon name="plus"/>{variantCreated ? "Variant created" : "Create variant"}</button><div className="panel-note"><Icon name="variants"/><div><strong>Ordered fallbacks</strong><p>The renderer chooses the first eligible variant, then applies permitted overflow rules.</p></div></div></aside>;
  return (
    <aside className="left-panel">
      <PanelHeader section="STRUCTURED" title="Components"/>
      <label className="search"><svg viewBox="0 0 24 24"><circle cx="11" cy="11" r="7"/><path d="m16 16 4 4"/></svg><input aria-label="Search components" placeholder="Search components" /></label>
      <div className="component-list">
        {components.map(([name, binding, icon]) => {
          const pointerGuided = guidedWorkflow && guideAction === "component" && name === "Menu item repeater";
          return <button draggable={false} onPointerDown={event => { if (pointerGuided) onBeginComponentDrag("repeater", { x: event.clientX, y: event.clientY }); }} onClick={() => { onSelect(name === "Menu item repeater" ? "repeater" : name === "Page title" ? "title" : active); onCancelPendingDrag(); }} className={`component-row ${active === name ? "active" : ""} ${pointerGuided ? "guided-target" : ""}`} key={name}><span className="component-icon"><Icon name={icon}/></span><span><strong>{name}</strong><small className={binding === "Not bound" ? "muted" : "bound"}>{binding}</small></span><span className="drag-dots" aria-label={pointerGuided ? `Drag ${name} onto canvas` : undefined}>••<br/>••</span></button>;
        })}
      </div>
      <div className="panel-note"><Icon name="spark"/><div><strong>Data-aware by default</strong><p>Repeaters inherit fields, ordering and empty states from menu.v1.</p></div></div>
    </aside>
  );
}

function Inspector({ selected, onStyleChange, requestedTab, guideAction }: { selected: string; onStyleChange: () => void; requestedTab: "Properties" | "Style" | "Rules"; guideAction: GuideAction | null }) {
  const repeater = selected === "repeater";
  const selectedField = selected.startsWith("field:") ? selected.slice(6) : null;
  const selectionName = selectedField ?? (repeater ? "Menu item repeater" : "Page title");
  const [tab, setTab] = useState<"Properties" | "Style" | "Rules">(requestedTab);
  return (
    <aside className="inspector">
      <div className="inspector-head"><div><span className="object-dot"/><span className="settings-title"><small>SETTINGS</small><strong>{selectionName} settings</strong></span></div></div>
      <div className="inspector-tabs">{(["Properties","Style","Rules"] as const).map(name=><button key={name} className={`${tab===name ? "active" : ""} ${guideAction === "style" && name === "Style" ? "guided-target" : ""}`} onClick={()=>setTab(name)}>{name}</button>)}</div>
      {tab === "Style" ? <>
        <InspectorSection title="Theme tokens" open><div className="token-binding"><span className="token-preview primary"/><span><strong>{repeater ? "Content ink" : "Display ink"}</strong><small>{repeater ? "color.ink.primary" : "color.ink.display"}</small></span><button onClick={onStyleChange}>Detach</button></div><div className="token-binding"><span className="token-preview accent"/><span><strong>Accent</strong><small>color.brand.accent</small></span><button onClick={onStyleChange}>Detach</button></div></InspectorSection>
        <InspectorSection title="Typography" open><label>Font family<select defaultValue="Aptos Display" onChange={onStyleChange}><option>Aptos Display</option><option>Inter</option></select></label><div className="field-grid triple"><label>Weight<select defaultValue={repeater ? "600" : "700"} onChange={onStyleChange}><option>600</option><option>700</option></select></label><label>Size<input defaultValue={repeater ? "28" : "112"} onChange={onStyleChange}/></label><label>Line<input defaultValue={repeater ? "1.2" : "1.0"} onChange={onStyleChange}/></label></div></InspectorSection>
        <InspectorSection title="Fill & effects"><div/></InspectorSection>
      </> : tab === "Rules" ? <>
        <div className="protected-summary"><Icon name="lock"/><div><strong>Protected constraints</strong><p>The repair agent and automatic variants cannot weaken these rules.</p></div></div>
        <InspectorSection title="Required content" open><label className="switch-row"><span>Item name is required</span><input type="checkbox" defaultChecked disabled/></label><label className="switch-row"><span>Price is required</span><input type="checkbox" defaultChecked disabled/></label></InspectorSection>
        <InspectorSection title="Safe limits" open><div className="field-grid"><label>Minimum font<input defaultValue={repeater ? "22 px" : "72 px"}/></label><label>Maximum density<input defaultValue="Compact"/></label></div><label>Failure behavior<select defaultValue="Block save"><option>Block save</option></select></label></InspectorSection>
        <InspectorSection title="Renderer support" open><div className="rule-line"><span>Static image</span><em><Icon name="check" size={13}/>Supported</em></div><div className="rule-line"><span>Live render</span><em><Icon name="check" size={13}/>Supported</em></div><div className="rule-line"><span>Hybrid</span><em><Icon name="check" size={13}/>Supported</em></div></InspectorSection>
      </> : selectedField ? <>
        <InspectorSection title="Binding" open><label>Source field<input value={selectedField.toLowerCase().replaceAll(" ", ".")} readOnly/></label><div className="binding-line"><span className="binding-pin"/>Bound to selected component</div></InspectorSection>
        <InspectorSection title="Display" open><label>Format<select defaultValue={selectedField === "Price" ? "Currency" : selectedField === "Availability" ? "State treatment" : "Text"}><option>{selectedField === "Price" ? "Currency" : selectedField === "Availability" ? "State treatment" : "Text"}</option></select></label><label>Fallback value<input defaultValue={selectedField}/></label></InspectorSection>
        <InspectorSection title="Position & size"><div/></InspectorSection>
      </> : repeater ? <>
        <InspectorSection title="Data source" open><label>Collection<select defaultValue="section.items"><option>section.items</option></select></label><div className="binding-line"><span className="binding-pin"/>Bound to menu.v1</div></InspectorSection>
        <InspectorSection title="Layout" open><div className="field-grid"><label>Columns<input defaultValue="1"/></label><label>Gap<input defaultValue="24 px"/></label><label>Item limit<input defaultValue="8"/></label><label>Order<select defaultValue="Manual"><option>Manual</option><option>Name</option><option>Price</option></select></label></div></InspectorSection>
        <InspectorSection title="Overflow" open><label>Primary strategy<select defaultValue="Dense variant"><option>Dense variant</option><option>Continue page</option><option>Block theme save</option></select></label><label className="switch-row"><span>Reduce spacing</span><input type="checkbox" defaultChecked/></label><label className="switch-row"><span>Hide optional descriptions</span><input type="checkbox" defaultChecked/></label></InspectorSection>
        <InspectorSection title="Position & size"><div/></InspectorSection>
      </> : <>
        <InspectorSection title="Typography" open><label>Font family<select defaultValue="Aptos Display"><option>Aptos Display</option></select></label><div className="field-grid triple"><label>Weight<select defaultValue="700"><option>700</option></select></label><label>Size<input defaultValue="112"/></label><label>Line<input defaultValue="1.0"/></label></div><label className="switch-row"><span>Auto-fit text</span><input type="checkbox" defaultChecked/></label><div className="field-grid"><label>Minimum size<input defaultValue="72 px"/></label><label>Line limit<input defaultValue="2"/></label></div></InspectorSection>
        <InspectorSection title="Binding" open><label>Source field<select defaultValue="menu.page.title"><option>menu.page.title</option></select></label><label>Fallback value<input defaultValue="Menu title"/></label></InspectorSection>
        <InspectorSection title="Position & size"><div/></InspectorSection>
      </>}
    </aside>
  );
}

function InspectorSection({ title, children, open = false }: { title: string; children: React.ReactNode; open?: boolean }) {
  const [expanded, setExpanded] = useState(open);
  return <section className={`inspector-section ${expanded ? "open" : ""}`}><button className="section-toggle" aria-expanded={expanded} onClick={() => setExpanded(value => !value)}><span>{title}</span><span>{expanded ? "−" : "+"}</span></button>{expanded && <div className="section-content">{children}</div>}</section>;
}

function Diagnostics({ dataset, onRepair, draft, empty, layoutChosen, contentReady }: { dataset: Dataset; onRepair: () => void; draft: TemplateDraft; empty: boolean; layoutChosen: boolean; contentReady: boolean }) {
  const danger = dataset === "Maximum";
  const long = dataset === "Long text";
  if (!contentReady) return <section className="diagnostics"><div className="diagnostics-head"><div><strong>Diagnostics</strong><span className="count danger">Setup required</span></div><div className="diagnostic-summary"><span><i className="dot red"/>1 blocking</span><span><i className="dot amber"/>0 warnings</span><span><i className="dot green"/>1 passed</span></div></div><div className="diagnostic-body"><div className="diagnostic-item warning"><Icon name="warning"/><div><strong>{!layoutChosen ? "Choose a layout before adding content." : empty ? "Add at least one component to the selected layout." : "Drag at least one field onto the component."}</strong><span>{orientedResolution(draft)} · {draft.orientation} · Menu fields are ready</span></div></div></div></section>;
  return (
    <section className="diagnostics">
      <div className="diagnostics-head"><div><strong>Diagnostics</strong><span className={danger || long ? "count danger" : "count"}>{danger || long ? "2 issues" : "Ready"}</span></div><div className="diagnostic-summary"><span><i className="dot red"/>0 blocking</span><span><i className="dot amber"/>{danger || long ? 2 : 0} warnings</span><span><i className="dot green"/>6 passed</span></div></div>
      <div className="diagnostic-body">
        {danger ? <><div className="diagnostic-item warning"><Icon name="warning"/><div><strong>Beer repeater exceeds its region by 148 pixels with 16 items.</strong><span>Busy content · {orientedResolution(draft)} · Static and live</span></div><button onClick={onRepair}>Auto-fix layout <Icon name="spark" size={15}/></button></div><div className="diagnostic-item"><Icon name="warning"/><div><strong>The dense variant supports 14 items; this dataset contains 16.</strong><span>Change capacity, continue onto another page, or block the theme from being saved.</span></div><button onClick={onRepair}>Review rule</button></div></> : long ? <><div className="diagnostic-item warning"><Icon name="warning"/><div><strong>The longest item name needs three lines; this template permits two.</strong><span>Long text · Menu item repeater · Row 1</span></div><button onClick={onRepair}>Auto-fix layout <Icon name="spark" size={15}/></button></div><div className="diagnostic-item"><Icon name="warning"/><div><strong>Auto-fit reaches the minimum permitted size of 22 px.</strong><span>Typography limit remains protected.</span></div><button onClick={onRepair}>Inspect text</button></div></> : <><div className="diagnostic-item success"><Icon name="check"/><div><strong>Required content fits within all safe regions.</strong><span>{dataset} data · {orientedResolution(draft)} · Renderer 2.3</span></div></div><div className="diagnostic-item success"><Icon name="check"/><div><strong>Static and live output are visually equivalent.</strong><span>0 mismatched pixels above tolerance</span></div></div></>}
      </div>
    </section>
  );
}

function BuildGuide({ completed, current, collapsed, onToggle, onStep, coach, onExit }: { completed: Set<string>; current: string; collapsed: boolean; onToggle: () => void; onStep: (step: string) => void; coach: { step: string; progress: string; title: string; body: string }; onExit: () => void }) {
  const steps = [
    ["Structure", "Choose layout and pages"],
    ["Content", "Add components and fields"],
    ["Appearance", "Style in Settings"],
    ["Behavior", "Configure variants"],
    ["Test", "Check every variation"],
    ["Save", "Validate and save"],
  ];
  const completedCount = steps.filter(([name]) => completed.has(name)).length;
  return <aside className={`build-guide ${collapsed ? "collapsed" : ""}`}>
    <button className="guide-toggle" onClick={onToggle} aria-expanded={!collapsed}><span><Icon name="spark" size={15}/><span><strong>Build guide</strong><small>{completedCount} of 6 complete</small></span></span><Icon name="chevron" size={14}/></button>
    {!collapsed && <><div className="guide-steps">{steps.map(([name, description], index) => <button key={name} className={`${current === name ? "active" : ""} ${completed.has(name) ? "done" : ""}`} onClick={() => onStep(name)}><i>{completed.has(name) ? <Icon name="check" size={12}/> : index + 1}</i><span><strong>{name}</strong><small>{description}</small></span></button>)}</div><div className="guide-coach" aria-live="polite"><span className="coach-marker"><Icon name="spark" size={16}/></span><div className="coach-copy"><span>{coach.step} · {coach.progress}</span><strong>{coach.title}</strong><p>{coach.body}</p></div><span className="coach-direction">Use the highlighted control <Icon name="arrow" size={14}/></span><button className="guide-exit" onClick={onExit}>Exit guide</button></div></>}
  </aside>;
}

function Studio({ go, initialDataset, draft, entry }: { go: (screen: Screen) => void; initialDataset: Dataset; draft: TemplateDraft; entry: StudioEntry }) {
  const isExistingTheme = entry !== "new";
  const fromMenu = entry === "menu";
  const [mode, setMode] = useState<Mode>("Edit");
  const [dataset, setDataset] = useState<Dataset>(initialDataset);
  const [selected, setSelected] = useState(draft.startingPoint === "Begin blank" && draft.guidedBuild ? "canvas" : "repeater");
  const [settingsTabRequest, setSettingsTabRequest] = useState<"Properties" | "Style" | "Rules">("Properties");
  const [leftActive, setLeftActive] = useState<RailPanel>(draft.startingPoint === "Begin blank" ? "Layouts" : "Components");
  const [layoutChosen, setLayoutChosen] = useState(draft.startingPoint !== "Begin blank");
  const [selectedLayout, setSelectedLayout] = useState(draft.startingPoint === "Northside menu" ? "Two-column menu" : "");
  const [styleVisited, setStyleVisited] = useState(isExistingTheme);
  const [freeformBlankStarted, setFreeformBlankStarted] = useState(false);
  const [repeaterColumn, setRepeaterColumn] = useState<CanvasColumn | null>(null);
  const [boundFields, setBoundFields] = useState<Set<string>>(new Set(draft.startingPoint === "Northside menu" ? ["item.name"] : []));
  const [behaviorVisited, setBehaviorVisited] = useState(isExistingTheme);
  const [testVisited, setTestVisited] = useState(isExistingTheme);
  const [guideEnabled, setGuideEnabled] = useState(entry === "new" && draft.guidedBuild);
  const [guideWasExited, setGuideWasExited] = useState(false);
  const [guideCollapsed, setGuideCollapsed] = useState(false);
  const [confirmation, setConfirmation] = useState<string | null>(null);
  const [pendingComponent, setPendingComponent] = useState<string | null>(null);
  const [pendingField, setPendingField] = useState<FieldPayload | null>(null);
  const [dragOrigin, setDragOrigin] = useState<DragPoint | null>(null);
  const [dragPoint, setDragPoint] = useState<DragPoint | null>(null);
  const [dragMoved, setDragMoved] = useState(false);
  const guidedWorkflow = entry === "new" && draft.startingPoint === "Begin blank" && (draft.guidedBuild || guideEnabled || guideWasExited);
  const blankStarted = guidedWorkflow ? repeaterColumn !== null : freeformBlankStarted;
  const empty = draft.startingPoint === "Begin blank" && !blankStarted;
  const contentReady = draft.startingPoint !== "Begin blank" || (guidedWorkflow ? repeaterColumn !== null && boundFields.has("item.name") : freeformBlankStarted && boundFields.size > 0);
  const rail = [["Layouts","layout"],["Components","components"],["Fields","data"],["Elements","elements"],["Assets","assets"],["Pages","pages"],["Variants","variants"]] as const;
  const completed = new Set<string>([
    ...(layoutChosen ? ["Structure"] : []),
    ...(contentReady ? ["Content"] : []),
    ...(styleVisited ? ["Appearance"] : []),
    ...(behaviorVisited ? ["Behavior"] : []),
    ...(testVisited ? ["Test"] : []),
  ]);
  const saveReady = contentReady && styleVisited && behaviorVisited && testVisited;
  const canUseGuide = entry === "new" && draft.startingPoint === "Begin blank";
  const showBuildGuide = canUseGuide && guideEnabled;
  const currentGuideStep = !layoutChosen ? "Structure" : !contentReady ? "Content" : !styleVisited ? "Appearance" : !behaviorVisited ? "Behavior" : !testVisited ? "Test" : "Save";
  const guideAction: GuideAction | null = !showBuildGuide ? null : !layoutChosen ? "layout" : !blankStarted ? "component" : !boundFields.has("item.name") ? "field" : !styleVisited ? "style" : !behaviorVisited ? "behavior" : !testVisited ? "test" : "save";
  const coachByAction: Record<GuideAction, { step: string; progress: string; title: string; body: string }> = {
    layout: { step: "Structure", progress: "Action 1 of 1", title: "Choose the page structure", body: "Select Two-column menu. It creates the regions that will hold your menu content." },
    component: { step: "Content", progress: "Action 1 of 2", title: "Place the menu item container", body: "Drag Menu item repeater into either the left or right column. The column you choose becomes its actual location." },
    field: { step: "Content", progress: "Action 2 of 2", title: "Put item names inside the repeater", body: "Drag Item name from Fields into the highlighted repeater. Sample names will appear only after you drop it." },
    style: { step: "Appearance", progress: "Action 1 of 1", title: "Use the panel that just opened", body: "Style settings just opened on the right. Change a font, weight, color, or spacing option to define the theme’s look." },
    behavior: { step: "Behavior", progress: "Action 1 of 1", title: "Choose a sold-out treatment", body: "Select Sold out to confirm how unavailable menu items should appear." },
    test: { step: "Test", progress: "Action 1 of 1", title: "Test a difficult content case", body: "Choose Long text from the Dataset list to check that the design still fits." },
    save: { step: "Save", progress: "Ready", title: "Your first theme is ready to save", body: "Select Save theme. This saves the reusable design and does not change any live screen." },
  };
  const showConfirmation = (message: string) => { setConfirmation(message); window.setTimeout(() => setConfirmation(null), 2400); };
  const clearPendingDrag = () => { setPendingComponent(null); setPendingField(null); setDragOrigin(null); setDragPoint(null); setDragMoved(false); };
  const beginGuidedDrag = (point: DragPoint) => { setDragOrigin(point); setDragPoint(point); setDragMoved(false); };
  const moveGuidedDrag = (event: React.PointerEvent) => {
    if (!dragOrigin) return;
    const point = { x: event.clientX, y: event.clientY };
    setDragPoint(point);
    setDragMoved(current => current || Math.hypot(point.x - dragOrigin.x, point.y - dragOrigin.y) > 5);
  };
  const chooseLayout = (layout: string) => { setSelectedLayout(layout); setLayoutChosen(true); if (showBuildGuide) { showConfirmation(`${layout} selected; Components opened`); setLeftActive("Components"); setMode("Edit"); } };
  const addContent = (value: string) => { if (!layoutChosen) { setLeftActive("Layouts"); return; } setSelected(value); setConfirmation(null); if (!guidedWorkflow) setFreeformBlankStarted(true); };
  const dropComponent = (component: string, column: CanvasColumn) => { if (!guidedWorkflow || !layoutChosen || component !== "repeater") return; setRepeaterColumn(column); setSelected("repeater"); showConfirmation(`Menu item repeater added to the ${column} column`); if (showBuildGuide) { setLeftActive("Fields"); setMode("Edit"); } };
  const dropField = (field: FieldPayload) => { if (guidedWorkflow) { if (!repeaterColumn || (!boundFields.has("item.name") && field.field !== "item.name")) return; } else if (!freeformBlankStarted) return; setBoundFields(current => { const next = new Set(current); next.add(field.field); return next; }); setSelected(`field:${field.label}`); showConfirmation(`${field.label} added to the menu item repeater`); setStyleVisited(false); if (showBuildGuide) { setSettingsTabRequest("Style"); setMode("Edit"); } };
  const markAppearanceComplete = () => { setStyleVisited(true); if (showBuildGuide) { showConfirmation("Appearance updated; Variants opened"); setLeftActive("Variants"); setMode("Edit"); } };
  const markBehaviorComplete = () => { setBehaviorVisited(true); if (showBuildGuide) { showConfirmation("Behavior confirmed; Test mode opened"); setMode("Test"); } };
  const openGuideStep = (step: string) => { if (step === "Structure") { setLeftActive("Layouts"); setMode("Edit"); } else if (step === "Content") { setLeftActive(blankStarted ? "Fields" : "Components"); setMode("Edit"); } else if (step === "Appearance") { setSettingsTabRequest("Style"); setMode("Edit"); } else if (step === "Behavior") { setLeftActive("Variants"); setMode("Edit"); } else if (step === "Test") { setMode("Test"); } else if (step === "Save" && saveReady) go("save"); };
  return <div className={`studio-shell ${showBuildGuide ? "" : "without-guide"}`}>
    <header className="topbar">
      <div className="topbar-brand"><button className="studio-exit" onClick={() => go(fromMenu ? "menu" : "library")} aria-label={fromMenu ? "Back to Saturday Dinner menu" : "Back to themes"}><Icon name="arrow" size={15}/></button><Logo/><div className="name-block"><span>{fromMenu ? "Saturday Dinner menu" : "Theme Studio"}</span><strong>{draft.startingPoint === "Northside menu" ? "Northside Menu" : draft.startingPoint === "Generate a layout" ? "Generated theme" : "Untitled theme"}</strong></div></div>
      <div className="document-meta"><span>Menu · menu.v1</span><span className="meta-rule"/><span>{orientedResolution(draft).replaceAll(" ", "")} · {draft.orientation}</span></div>
      <div className="top-actions"><button className="button quiet new-template" onClick={() => go("create")}><Icon name="plus" size={15}/>New</button><button className="icon-button" disabled aria-label="Undo unavailable"><Icon name="undo"/></button><button className="icon-button" disabled aria-label="Redo unavailable"><Icon name="redo"/></button><span className="meta-rule"/><span className="save-state"><i/>Changes are not live</span><button className={`button primary ${guideAction === "save" ? "guided-target" : ""}`} disabled={!saveReady} title={!saveReady ? "Complete and test the theme before saving" : undefined} onClick={() => go("save")}><Icon name="check"/>{fromMenu ? "Save & return" : "Save theme"}</button></div>
    </header>
    <div className="modebar"><div className="mode-tabs">{(["Edit","Test"] as Mode[]).map(item => <button key={item} className={mode === item ? "active" : ""} aria-pressed={mode === item} onClick={() => setMode(item)}>{item}</button>)}</div>{mode === "Test" ? <div className="test-controls"><label className={`dataset-select ${guideAction === "test" ? "guided-target" : ""}`}><span>Dataset</span><select aria-label="Dataset" value={dataset} onChange={e => { const next = e.target.value as Dataset; setDataset(next); if (!guidedWorkflow || next === "Long text") { setTestVisited(true); if (showBuildGuide) showConfirmation(`${next} dataset selected`); } }}>{datasets.map(d => <option key={d}>{d}</option>)}</select></label><button className="matrix-link" onClick={() => { setTestVisited(true); go("tests"); }}>Review all tests <Icon name="chevron" size={13}/></button></div> : <div className="modebar-status">{canUseGuide && !showBuildGuide && <button className="resume-guide" onClick={() => { setGuideEnabled(true); setGuideCollapsed(false); openGuideStep(currentGuideStep); }}><Icon name="spark" size={14}/>{guideWasExited ? "Resume guide" : "Start guide"}</button>}<div className="viewport-note"><span className="live-dot"/>Shared renderer · exact preview</div></div>}</div>
    {showBuildGuide && guideAction && (
      <BuildGuide completed={completed} current={currentGuideStep} collapsed={guideCollapsed} onToggle={() => setGuideCollapsed(value => !value)} onStep={openGuideStep} coach={coachByAction[guideAction]} onExit={() => { setGuideWasExited(true); setGuideEnabled(false); }}/>
    )}
    <div className={`workspace ${mode === "Test" ? "test-workspace" : ""} ${showBuildGuide && guideAction ? `guided-workspace guide-${guideAction}` : ""}`} onPointerMove={moveGuidedDrag} onPointerUp={clearPendingDrag} onPointerCancel={clearPendingDrag}>
      <nav className="tool-rail" aria-label="Editor tools">{rail.map(([label, icon]) => <button key={label} className={mode === "Edit" && leftActive === label ? "active" : ""} aria-pressed={mode === "Edit" && leftActive === label} onClick={() => {setLeftActive(label); setMode("Edit");}}><Icon name={icon}/><span>{label}</span></button>)}</nav>
      {mode === "Edit" && (
        <LeftPanel panel={leftActive} active={selected === "repeater" ? "Menu item repeater" : selected === "title" ? "Page title" : ""} onSelect={addContent} draft={draft} selectedLayout={selectedLayout} onLayout={chooseLayout} onBehaviorConfigured={markBehaviorComplete} guideAction={guideAction} guidedWorkflow={guidedWorkflow} onBeginComponentDrag={(component, point) => { setPendingField(null); setPendingComponent(component); beginGuidedDrag(point); }} onBeginFieldDrag={(field, point) => { setPendingComponent(null); setPendingField(field); beginGuidedDrag(point); }} onCancelPendingDrag={clearPendingDrag}/>
      )}
      <main className="canvas-column"><StudioCanvas dataset={dataset} selected={selected} onSelect={setSelected} draft={draft} empty={empty} layoutChosen={layoutChosen} selectedLayout={selectedLayout} onDropField={dropField} onDropComponent={dropComponent} repeaterColumn={repeaterColumn} boundFields={boundFields} confirmation={confirmation} guideAction={guideAction} guidedWorkflow={guidedWorkflow} pendingComponent={pendingComponent} pendingField={pendingField} onClearPendingDrag={clearPendingDrag}/><Diagnostics dataset={dataset} onRepair={() => go("repair")} draft={draft} empty={empty} layoutChosen={layoutChosen} contentReady={contentReady}/></main>
      {mode === "Edit" && (empty ? <aside className="inspector blank-inspector"><div className="inspector-head"><div><span className="object-dot"/><span className="settings-title"><small>SETTINGS</small><strong>Canvas settings</strong></span></div></div><div><Icon name={layoutChosen ? "components" : "layout"} size={22}/><strong>{layoutChosen ? "Add a component next" : "Choose a layout first"}</strong><p>{layoutChosen ? "The selected layout is ready for structured content." : "Settings will appear here when you select something on the canvas."}</p></div></aside> : <Inspector key={settingsTabRequest} selected={selected} requestedTab={settingsTabRequest} onStyleChange={markAppearanceComplete} guideAction={guideAction}/>)}
    </div>
    {dragMoved && dragPoint && (pendingComponent || pendingField) && <div className={`guided-drag-preview ${pendingField ? "field-preview" : "component-preview"}`} style={{ left: dragPoint.x + 14, top: dragPoint.y + 14 }} aria-hidden="true">
      <span className="drag-preview-icon"><Icon name={pendingField ? "text" : "repeater"} size={17}/></span>
      <span className="drag-preview-copy"><strong>{pendingField?.label ?? "Menu item repeater"}</strong><small>{pendingField?.field ?? "section.items"}</small></span>
      <span className="drag-preview-status">Dragging</span>
    </div>}
  </div>;
}

function CreateFlow({ go, onCreate }: { go: (s: Screen) => void; onCreate: (draft: TemplateDraft) => void }) {
  const [step, setStep] = useState(1);
  const [resolution, setResolution] = useState<Resolution>(defaultDraft.resolution);
  const [orientation, setOrientation] = useState<Orientation>(defaultDraft.orientation);
  const [safeArea, setSafeArea] = useState<"5%" | "Custom">(defaultDraft.safeArea);
  const [customSafeArea, setCustomSafeArea] = useState(defaultDraft.customSafeArea);
  const [startingPoint, setStartingPoint] = useState<StartingPoint>(defaultDraft.startingPoint);
  const [guidedBuild, setGuidedBuild] = useState(defaultDraft.guidedBuild);
  const draft: TemplateDraft = { model: "Menu", resolution, orientation, safeArea, customSafeArea, startingPoint, guidedBuild };
  const safeAreaValid = safeArea === "5%" || (Number.isFinite(customSafeArea) && customSafeArea >= 0 && customSafeArea <= 400);
  return <div className="flow-shell">
    <header className="flow-header"><div className="flow-brand"><Logo/><span>Theme Studio</span></div><button className="button quiet" onClick={() => go("library")}><Icon name="close"/>Close</button></header>
    <main className="flow-main">
      <div className="flow-progress"><span>New theme</span><div>{[1,2,3].map(n => <button key={n} className={step === n ? "active" : step > n ? "done" : ""} onClick={() => setStep(n)}><i>{step > n ? <Icon name="check" size={13}/> : n}</i>{n === 1 ? "Data model" : n === 2 ? "Display" : "Starting point"}</button>)}</div></div>
      {step === 1 && <section className="flow-stage"><div className="flow-copy"><h1>What type of content is this theme for?</h1></div><div className="choice-grid model-grid">{["Menu","Cinema","Tap board","Bakery"].map((name, i) => <button key={name} className={`choice ${i === 0 ? "selected" : "future"}`} disabled={i > 0} aria-disabled={i > 0}><span className="choice-icon"><Icon name={i === 1 ? "display" : i === 0 ? "menu" : "data"} size={24}/></span><strong>{name}</strong><p>{i === 0 ? "Sections, items, prices, availability and promotional states." : i === 1 ? "Movies, showtimes, formats and auditorium states." : i === 2 ? "Taps, styles, ABV, pours and availability." : "Products, batches, prices and daily availability."}</p>{i === 0 ? <em>menu.v1 · Available</em> : <em>Planned</em>}</button>)}</div></section>}
      {step === 2 && <section className="flow-stage"><div className="flow-copy"><h1>Choose the design surface.</h1></div><div className="display-builder"><div className={`display-preview ${orientation.toLowerCase()}`}><i className={`preview-safe ${safeArea === "Custom" ? "custom" : ""}`}><span>Keep content inside this line</span></i><span>{orientation === "Landscape" ? "16:9" : "9:16"}</span><small>{orientedResolution(draft)}</small></div><div className="display-controls"><label>Resolution<select value={resolution} onChange={e => setResolution(e.target.value as Resolution)}><option value="1920 × 1080">1920 × 1080 (Full HD)</option><option value="3840 × 2160">3840 × 2160 (4K)</option></select></label><label>Orientation<div className="segmented">{(["Landscape","Portrait"] as Orientation[]).map(item => <button key={item} className={orientation === item ? "active" : ""} aria-pressed={orientation === item} onClick={() => setOrientation(item)}>{item}</button>)}</div></label><label className="safe-area-control"><span>Safe area</span><small>Keep important words and pictures inside this line so they don’t get cut off at the edges of the screen.</small><div className="segmented">{(["5%","Custom"] as const).map(item => <button key={item} className={safeArea === item ? "active" : ""} aria-pressed={safeArea === item} onClick={() => setSafeArea(item)}>{item}</button>)}</div></label>{safeArea === "Custom" && <label className="custom-safe">Inset in pixels<input type="number" min="0" max="400" value={customSafeArea} aria-invalid={!safeAreaValid} onChange={e => setCustomSafeArea(Number(e.target.value))}/><small>0–400 px on every edge</small></label>}<div className="compat-note"><Icon name="check"/><span>Supports static, live and hybrid output</span></div></div></div></section>}
      {step === 3 && <section className="flow-stage"><div className="flow-copy"><h1>Start with the right amount of structure.</h1><p>Choose a tested design, generate a layout, or begin with an empty canvas.</p></div><div className="choice-grid start-grid">{(["Northside menu","Generate a layout","Begin blank"] as StartingPoint[]).map((item, index) => <button key={item} className={`choice ${startingPoint === item ? "selected" : ""}`} aria-pressed={startingPoint === item} onClick={() => { setStartingPoint(item); setGuidedBuild(item === "Begin blank"); }}><span className={`choice-visual ${index === 0 ? "template-visual" : index === 1 ? "generated-visual" : "blank-visual"}`}>{index === 0 ? <><i/><i/><i/></> : <Icon name={index === 1 ? "spark" : "blank"} size={28}/>}</span><strong>{item}</strong><p>{index === 0 ? "A tested two-column menu with dense and sold-out variants." : index === 1 ? "Create an editable layout from the menu.v1 structure." : "Start with safe areas and empty page structure."}</p><em>{index === 0 ? "Recommended · capacity 14" : index === 1 ? "Uses menu.v1" : "Full control"}</em></button>)}</div><label className="guided-build-option"><input type="checkbox" checked={guidedBuild} onChange={event => setGuidedBuild(event.target.checked)}/><span><strong>Guide me through building this theme</strong><small>Show the next recommended step while I design. You can turn the guide off at any time.</small></span></label><div className="theme-summary"><span>Theme setup</span><strong>menu.v1 · {orientedResolution(draft)} {orientation.toLowerCase()} · {safeArea === "5%" ? "5% safe area" : `${customSafeArea}px safe area`} · {startingPoint}{guidedBuild ? " · Build guide on" : ""}</strong></div></section>}
      <footer className="flow-footer"><button className="button quiet" onClick={() => step > 1 ? setStep(step - 1) : go("library")}>{step > 1 ? "Back" : "Cancel"}</button><div><span>Step {step} of 3</span><button className="button primary" disabled={step === 2 && !safeAreaValid} onClick={() => step < 3 ? setStep(step + 1) : onCreate(draft)}>{step < 3 ? "Continue" : "Start designing"}<Icon name="arrow"/></button></div></footer>
    </main>
  </div>;
}

function RepairReview({ go }: { go: (s: Screen) => void }) {
  const [applied, setApplied] = useState(false);
  const [patchOpen, setPatchOpen] = useState(false);
  return <div className="review-shell">
    <header className="review-header"><div><button className="icon-button" onClick={() => go("studio")} aria-label="Back to studio"><svg viewBox="0 0 24 24"><path d="m15 18-6-6 6-6"/></svg></button><div><span>THEME REPAIR AGENT</span><strong>Review proposed repair</strong></div></div><div className="context-chip"><Icon name="lock" size={14}/>Last saved theme remains unchanged</div></header>
    <main className="review-main">
      <section className="comparison"><div className="comparison-head"><div><h1>{applied ? "Repair applied to theme" : "A safe repair is ready to review."}</h1><p>{applied ? "The working theme now passes the selected boundary dataset." : "The agent found a repair without changing brand tokens, minimum font sizes, required fields or supported output modes."}</p></div><span className={applied ? "repair-status applied" : "repair-status"}>{applied ? <Icon name="check"/> : <Icon name="spark"/>}{applied ? "Theme updated" : "3 changes proposed"}</span></div>
        <div className="preview-pair"><PreviewCard label="Before" bad/><div className="repair-arrow"><Icon name="arrow" size={22}/></div><PreviewCard label="After" bad={false}/></div>
      </section>
      <aside className="repair-details"><div className="repair-details-head"><div><span>EXPLANATION</span><h2>Proposed changes</h2></div><span>Attempt 2 of 3</span></div><div className="change-list"><Change n="1" title="Use the dense menu variant" body="Switches the repeater to the existing dense variant when item count exceeds 14." protectedLabel="Brand tokens preserved"/><Change n="2" title="Reduce vertical gap" body="Changes row gap from 24 px to 18 px. The template permits a minimum of 16 px." protectedLabel="Inside allowed limit"/><Change n="3" title="Continue remaining items" body="Moves 2 overflow items to a generated continuation page instead of clipping them." protectedLabel="Required fields remain visible"/></div><button className="patch-disclosure" aria-expanded={patchOpen} onClick={() => setPatchOpen(open => !open)}>{patchOpen ? "Hide JSON Patch" : "View JSON Patch"} <Icon name="chevron" size={15}/></button>{patchOpen && <pre className="json-patch">{`[
  { "op": "replace", "path": "/variant", "value": "dense" },
  { "op": "replace", "path": "/layout/rowGap", "value": 18 },
  { "op": "add", "path": "/overflow/strategy", "value": "continue-page" }
]`}</pre>}<div className="validation-box"><div><Icon name="check"/><span><strong>Authoritative validation passed</strong><small>6 datasets · 3 output modes · renderer 2.3</small></span></div><span>0 blocking · 0 warnings</span></div>{applied ? <div className="review-actions applied-actions"><button className="button primary" onClick={() => go("studio")}>Return to Theme Studio <Icon name="arrow"/></button></div> : <div className="review-actions"><button className="button quiet" onClick={() => go("studio")}>Keep original</button><button className="button primary" onClick={() => setApplied(true)}>Apply 3 changes</button></div>}</aside>
    </main>
  </div>;
}

function PreviewCard({ label, bad }: { label: string; bad: boolean }) {
  return <div className="preview-card"><div className="preview-label"><span>{label}</span><em>{bad ? "2 warnings" : "Safe render"}</em></div><div className={`mini-canvas ${bad ? "bad" : ""}`}><div className="mini-brand">NORTHSIDE SOCIAL</div><h3>SMALL PLATES</h3>{Array.from({length: bad ? 10 : 8}).map((_,i) => <div className="mini-row" key={i}><span>{["Charred broccoli","Hot honey chicken","Crispy potatoes","House chopped salad"][i%4]}</span><b>${9+i}</b></div>)}{bad && <div className="mini-overflow">148 px overflow</div>}</div></div>;
}

function Change({ n, title, body, protectedLabel }: { n: string; title: string; body: string; protectedLabel: string }) {
  return <div className="change"><span className="change-number">{n}</span><div><strong>{title}</strong><p>{body}</p><em><Icon name="lock" size={12}/>{protectedLabel}</em></div></div>;
}

function SaveTheme({ go, fromMenu }: { go: (s: Screen) => void; fromMenu: boolean }) {
  const [saved, setSaved] = useState(false);
  const checks = ["Boundary datasets", "Required fonts", "Repeater capacity", "Static / live equivalence", "Renderer compatibility"];
  return <div className="publish-shell"><header className="review-header"><div><button className="icon-button" onClick={() => go("studio")} aria-label="Back to studio"><svg viewBox="0 0 24 24"><path d="m15 18-6-6 6-6"/></svg></button><div><span>SAVE THEME</span><strong>Northside Menu</strong></div></div></header><main className="publish-main"><section className="publish-summary"><div className="publish-hero"><span className="publish-check"><Icon name="check" size={32}/></span><h1>{saved ? "Theme saved." : "Ready to save this theme."}</h1><p>{saved ? "The latest design is now available in Menu Builder. No live screen changed." : "Saving replaces the latest saved design that Menu Builder uses. Live screens stay unchanged until the menu is published."}</p>{saved && <div className="saved-actions"><button className="button primary" onClick={() => go(fromMenu ? "menu" : "library")}>{fromMenu ? "Return to Saturday Dinner" : "Return to themes"} <Icon name="arrow" size={15}/></button></div>}</div><div className="publish-preview"><PreviewCard label={saved ? "Saved theme preview" : "Theme preview"} bad={false}/></div></section><aside className="publish-settings"><div><span>THEME DETAILS</span><h2>What will be saved?</h2><p className="save-explanation">The reusable design—not a screen update.</p></div><div className="theme-record"><h3>Theme details</h3><dl><div><dt>Canvas schema</dt><dd>1.0</dd></div><div><dt>Data model</dt><dd>menu.v1</dd></div><div><dt>Renderer</dt><dd>2.3 or later</dd></div><div><dt>Canvas</dt><dd>1920×1080</dd></div></dl></div><div className="preflight"><h3>Pre-save validation</h3>{checks.map(c => <div key={c}><Icon name="check" size={15}/><span>{c}</span><em>{saved ? "Passed" : "Ready"}</em></div>)}</div>{!saved && <><button className="button primary publish-button" onClick={() => setSaved(true)}>Validate & save theme <Icon name="check"/></button><p className="publish-note"><strong>No live effect.</strong> Menu Builder shows the latest saved theme. Publish the menu to update screens.</p></>}</aside></main></div>;
}

const testRows: Array<{dataset: Dataset; source: string; result: "Passed" | "Warning"; detail: string}> = [
  {dataset:"Typical", source:"Fixture", result:"Passed", detail:"8 items · 1 page"},
  {dataset:"Busy", source:"Fixture", result:"Passed", detail:"14 items · dense variant"},
  {dataset:"Maximum", source:"Fixture", result:"Warning", detail:"16 items · 148 px overflow"},
  {dataset:"Long text", source:"Fixture", result:"Warning", detail:"3-line title exceeds limit"},
  {dataset:"Missing images", source:"Fixture", result:"Passed", detail:"Fallback treatment used"},
  {dataset:"Sold out", source:"Fixture", result:"Passed", detail:"Availability treatment visible"},
];

function TestMatrix({ go, openDataset }: { go: (s: Screen) => void; openDataset: (dataset: Dataset) => void }) {
  return <div className="test-shell"><header className="review-header"><div><button className="icon-button" onClick={() => go("studio")} aria-label="Back to studio"><svg viewBox="0 0 24 24"><path d="m15 18-6-6 6-6"/></svg></button><div><span>TEST MODE</span><strong>Variation test matrix</strong></div></div><div className="context-chip"><span className="live-dot"/>menu.v1 · 1920×1080 · renderer 2.3</div></header><main className="test-main"><section className="test-overview"><div className="test-heading"><div><h1>Test the variation, not only the ideal.</h1><p>Every supported dataset and display must remain safe before this theme can be saved.</p></div></div><div className="test-summary-line"><span><strong>6</strong> datasets</span><span><strong>3</strong> output modes</span><span><strong>2</strong> warnings to resolve</span><span><strong>0</strong> blocking failures</span></div><div className="test-table" role="table" aria-label="Variation datasets"><div className="test-row test-table-head" role="row"><span>Dataset</span><span>Source</span><span>Observed result</span><span>Status</span><span/></div>{testRows.map(row=><div className="test-row" role="row" key={row.dataset}><span><strong>{row.dataset}</strong>{row.dataset==="Typical" && <small>Default preview</small>}</span><span>{row.source}</span><span>{row.detail}</span><span className={`test-result ${row.result.toLowerCase()}`}>{row.result === "Passed" ? <Icon name="check" size={14}/> : <Icon name="warning" size={14}/>} {row.result}</span><button onClick={()=>openDataset(row.dataset)}>Open on canvas <Icon name="chevron" size={13}/></button></div>)}<div className="test-row customer-row"><span><strong>Northside Social</strong><small>Actual customer data</small></span><span>Connected menu</span><span>12 items · current Saturday Dinner</span><span className="test-result passed"><Icon name="check" size={14}/>Passed</span><button onClick={()=>openDataset("Busy")}>Open on canvas <Icon name="chevron" size={13}/></button></div></div></section><aside className="test-sidebar"><div><span>SAVE COVERAGE</span><h2>Checked before saving</h2><p>These immediate results help resolve variation problems. Final validation runs when the theme is saved.</p></div><div className="scope-list">{["1920×1080 landscape","3840×2160 landscape","Static output","Live output","Hybrid output","Required fonts"].map(item=><div key={item}><Icon name="check" size={14}/><span>{item}</span><em>Included</em></div>)}</div><div className="test-action-box"><Icon name="test"/><div><strong>Checks update while editing</strong><p>Select any row to inspect it on the exact canvas.</p></div></div><button className="button primary run-test" onClick={() => go("repair")}><Icon name="spark"/>Review repair for 2 warnings</button></aside></main></div>;
}

export default function Home() {
  const [screen, setScreen] = useState<Screen>("library");
  const [dataset, setDataset] = useState<Dataset>("Typical");
  const [draft, setDraft] = useState<TemplateDraft>(defaultDraft);
  const [entry, setEntry] = useState<StudioEntry>("theme");
  const openTheme = (nextEntry: StudioEntry) => { setEntry(nextEntry); setDraft(defaultDraft); setDataset("Typical"); setScreen("studio"); };
  const openDataset = (next: Dataset) => { setDataset(next); setScreen("studio"); };
  const createDraft = (next: TemplateDraft) => { setDraft(next); setEntry("new"); setDataset("Typical"); setScreen("studio"); };
  const view = useMemo(() => screen === "library" ? <ThemeLibrary onOpen={() => openTheme("theme")} onCreate={() => setScreen("create")} openMenu={() => setScreen("menu")}/> : screen === "menu" ? <MenuThemeHandoff go={setScreen} editTheme={() => openTheme("menu")}/> : screen === "create" ? <CreateFlow go={setScreen} onCreate={createDraft}/> : screen === "repair" ? <RepairReview go={setScreen}/> : screen === "save" ? <SaveTheme go={setScreen} fromMenu={entry === "menu"}/> : screen === "tests" ? <TestMatrix go={setScreen} openDataset={openDataset}/> : <Studio key={entry} go={setScreen} initialDataset={dataset} draft={draft} entry={entry}/>, [screen, dataset, draft, entry]);
  return view;
}

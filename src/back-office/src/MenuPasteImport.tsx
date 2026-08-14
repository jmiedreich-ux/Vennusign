import { useCallback, useEffect, useMemo, useState } from "react";
import { ArrowLeft, Check, ChevronDown, ChevronRight, ClipboardPaste, RotateCcw, Sparkles } from "lucide-react";
import {
  MenuImportApiError,
  acceptSafeMenuImportMatches,
  answerMenuImport,
  confirmMenuImportCreate,
  confirmMenuImportReplace,
  loadShelf,
  loadMenuImport,
  setMenuImportLineSection,
  setMenuImportCreateDestination,
  setMenuImportReplaceDestination,
  restoreMenuImportReplacement,
  startMenuImport,
  type MenuImportQuestion,
  type MenuImportSession,
  type ShelfMenu
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import "./menu-paste-import.css";

type Props = { configuration: BackOfficeConfiguration; accessToken: string; sessionId: string | null; onBack: () => void; onStarted: (id: string) => void; onOpenMenu: (id: string) => void };

export default function MenuPasteImport({ configuration, accessToken, sessionId, onBack, onStarted, onOpenMenu }: Props) {
  const [paste, setPaste] = useState("");
  const [session, setSession] = useState<MenuImportSession | null>(null);
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(Boolean(sessionId));
  const [error, setError] = useState<string | null>(null);
  const [inventoryOpen, setInventoryOpen] = useState(false);
  const [menuName, setMenuName] = useState("New menu");
  const [menus,setMenus]=useState<ShelfMenu[]>([]);
  const [restoreConfirm,setRestoreConfirm]=useState(false);
  const [restored,setRestored]=useState(false);

  useEffect(() => {
    if (!sessionId) return;
    let current = true;
    setLoading(true);
    loadMenuImport(configuration, accessToken, sessionId)
      .then(value => { if (current) { setSession(value); setMenuName(value.session.proposedMenuName ?? "New menu"); setError(null); } })
      .catch(failure => { if (current) setError(failure instanceof Error ? failure.message : "This import could not be resumed."); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [configuration, accessToken, sessionId]);

  useEffect(()=>{if(session?.session.status!=="resolved"||session.session.completedMenuId)return;let active=true;loadShelf(configuration,accessToken).then(value=>{if(active)setMenus(value.filter(menu=>!menu.isPutAway));}).catch(()=>{if(active)setError("Menus could not be loaded. Your review is still saved.");});return()=>{active=false;};},[configuration,accessToken,session?.session.status,session?.session.completedMenuId]);

  const unresolved = useMemo(() => session?.questions.filter(question => !question.answer) ?? [], [session]);
  const safeCount = unresolved.filter(question => question.candidates.length === 1 && question.candidates[0].isSafe).length;
  const nearMisses = unresolved.filter(question => question.kind === "identity" && question.candidates.length > 0 && question.candidates.every(candidate => !candidate.isSafe));
  const standaloneQuestions = unresolved.filter(question => !nearMisses.includes(question));

  const mutate = useCallback(async (run: (current: MenuImportSession) => Promise<MenuImportSession>) => {
    if (!session) return;
    setBusy(true); setError(null);
    try { setSession(await run(session)); }
    catch (failure) {
      if (failure instanceof MenuImportApiError && failure.current) setSession(failure.current);
      setError(failure instanceof Error ? failure.message : "That answer could not be saved.");
    } finally { setBusy(false); }
  }, [session]);

  if (window.innerWidth < 900) return <main className="paste-import narrow-import" data-testid="menu-import-narrow" aria-labelledby="import-narrow-title">
    <div className="import-mark"><ClipboardPaste aria-hidden="true" /></div>
    <h1 id="import-narrow-title">Importing a menu needs a wider window</h1>
    <p>{sessionId ? "Your work is saved. Open this same link in a window at least 900px wide to continue." : "Open this page in a window at least 900px wide to start an import."}</p>
    {session?.session.expiresUtc && <p className="expiry">Saved until {formatExpiry(session.session.expiresUtc)}</p>}
    <button className="import-secondary" onClick={onBack}>Back to menus</button>
  </main>;

  if (!sessionId) return <main className="paste-import paste-start" data-testid="menu-import-start" aria-labelledby="paste-title">
    <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
    <section className="paste-start-card">
      <div className="import-mark"><ClipboardPaste aria-hidden="true" /></div>
      <p className="import-kicker">Import a menu</p>
      <h1 id="paste-title">Paste what you have</h1>
      <p>Keep headings, item names, descriptions, and prices in the text. Nothing is added to a menu until a later confirmation step.</p>
      <label htmlFor="menu-paste">Menu text</label>
      <textarea id="menu-paste" value={paste} onChange={event => setPaste(event.target.value)} placeholder={"DINNER\nBurger  14\nHouse salad  11"} rows={13} disabled={busy} />
      {error && <p className="import-error" role="alert">{error}</p>}
      <div className="paste-actions"><button className="import-secondary" onClick={onBack}>Cancel</button><button className="import-primary" disabled={busy || !paste.trim()} onClick={async () => {
        setBusy(true); setError(null);
        try { const created = await startMenuImport(configuration, accessToken, paste); setSession(created); onStarted(created.session.id); }
        catch (failure) { setError(failure instanceof Error ? failure.message : "VennuSign could not read that paste."); }
        finally { setBusy(false); }
      }}>{busy ? "Reading…" : "Read menu"}</button></div>
    </section>
  </main>;

  if (loading) return <main className="paste-import import-loading" aria-live="polite"><div className="import-spinner" /> Resuming your import…</main>;
  if (!session) return <main className="paste-import import-unavailable"><button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button><h1>We couldn’t resume this import</h1>{error && <p role="alert">{error}</p>}</main>;

  if (session.session.completedMenuId) return <main className="paste-import import-destination" data-testid="menu-import-complete" aria-labelledby="import-complete-title">
    <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
    <section className="destination-card completion-card"><div className="completion-check"><Check aria-hidden="true" /></div>
      <h1 id="import-complete-title">{session.session.proposedMenuName??session.session.targetMenuName} is ready to review</h1><p className="not-live">Not live yet</p>
      <p>{session.session.destination==="replace"?"The imported version is saved as working content. The menu identity and what is live on screens stayed unchanged.":"The menu and its imported items are saved as working content. Nothing changed on your screens."}</p>
      <dl><div><dt>{session.session.destination==="replace"?"Items now in draft":"Items added"}</dt><dd>{session.session.itemCount}</dd></div><div><dt>Published screens changed</dt><dd>0</dd></div></dl>
      {(restored||session.session.completedSnapshotRestoredUtc)&&<p className="restore-result" role="status">The working draft from before this import has been restored. Screens still have not changed.</p>}
      {session.session.completedSnapshotId&&!restored&&!session.session.completedSnapshotRestoredUtc&&<div className="restore-option">{restoreConfirm?<><strong>Restore the draft from before this import?</strong><p>The imported draft will be replaced. Published screens stay unchanged.</p><div><button className="import-secondary" disabled={busy} onClick={()=>setRestoreConfirm(false)}>Keep imported draft</button><button className="import-primary" disabled={busy} onClick={()=>void(async()=>{setBusy(true);setError(null);try{await restoreMenuImportReplacement(configuration,accessToken,session.session.completedSnapshotId!);setRestored(true);setRestoreConfirm(false);}catch(failure){setError(failure instanceof Error?failure.message:"The previous draft could not be restored.");}finally{setBusy(false);}})()}>Restore previous draft</button></div></>:<button className="restore-link" onClick={()=>setRestoreConfirm(true)}>Restore the draft from before this import</button>}</div>}
      {error&&<p className="import-error" role="alert">{error}</p>}
      <div className="destination-actions"><button className="import-secondary" onClick={onBack}>Done for now</button><button className="import-primary" onClick={() => onOpenMenu(session.session.completedMenuId!)}>Review draft in builder</button></div>
    </section>
  </main>;

  if (session.session.status === "resolved") return <main className="paste-import import-destination" data-testid="menu-import-create" aria-labelledby="destination-title">
    <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
    <section className="destination-card"><h1 id="destination-title">{session.session.destination==="create"?"Create this menu?":session.session.destination==="replace"?`Replace ${session.session.targetMenuName}?`:"Where should these items go?"}</h1>
      {!session.session.destination ? <><p>Your review is saved. Creating is the first step that changes menu working content.</p>{error && <p className="import-error" role="alert">{error}</p>}
        <button className="destination-choice" disabled={busy} onClick={() => void mutate(current => setMenuImportCreateDestination(configuration, accessToken, current, menuName))}><strong>Create a new menu</strong><span>Build a new unpublished menu from all {session.session.itemCount} imported items.</span></button>
        <div className="replace-options"><h2>Replace an existing menu</h2><p>The pasted menu becomes its new unpublished draft. What guests see stays live until you publish.</p>{menus.length?<div className="target-list">{menus.map(menu=><button type="button" key={menu.menuId} disabled={busy} onClick={()=>void mutate(current=>setMenuImportReplaceDestination(configuration,accessToken,current,menu.menuId))}><span><strong>{menu.name}</strong><small>{menu.publishedVersion===null?"Never published":`${menu.draftCount} unpublished ${menu.draftCount===1?"change":"changes"}`}</small></span><ChevronRight aria-hidden="true"/></button>)}</div>:<p className="future-destination">No active menus are available to replace.</p>}</div></> : session.session.destination==="create"?
      <form onSubmit={event => { event.preventDefault(); void (async () => { setBusy(true); setError(null); try { let current = session; if (menuName.trim() !== session.session.proposedMenuName) current = await setMenuImportCreateDestination(configuration, accessToken, session, menuName); const created = await confirmMenuImportCreate(configuration, accessToken, current); setSession(created.import); } catch (failure) { if (failure instanceof MenuImportApiError && failure.current) setSession(failure.current); setError(failure instanceof Error ? failure.message : "This menu could not be created. Nothing changed."); } finally { setBusy(false); } })(); }}>
        <p>Confirm the name, item count, and publishing state. Everything is created together or nothing changes.</p>
        <label htmlFor="import-menu-name">Menu name</label><input id="import-menu-name" required maxLength={200} value={menuName} onChange={event => setMenuName(event.target.value)} onBlur={() => { if (menuName.trim() && menuName.trim() !== session.session.proposedMenuName) void mutate(current => setMenuImportCreateDestination(configuration, accessToken, current, menuName)); }} />
        <div className="confirm-facts"><div><strong>{session.session.itemCount}</strong><span>items will be added</span></div><div><strong>0</strong><span>screens change now</span></div></div>
        <p className="not-live">Not live yet — publishing remains a separate action.</p>{error && <p className="import-error" role="alert">{error}</p>}
        <div className="destination-actions"><button type="button" className="import-secondary" disabled={busy} onClick={onBack}>Back</button><button type="submit" className="import-primary" disabled={busy || !menuName.trim()} onMouseDown={event => event.preventDefault()}>{busy ? "Creating…" : "Create menu"}</button></div>
      </form>:<form onSubmit={event=>{event.preventDefault();void(async()=>{setBusy(true);setError(null);try{const replaced=await confirmMenuImportReplace(configuration,accessToken,session);setSession(replaced.import);}catch(failure){if(failure instanceof MenuImportApiError&&failure.current)setSession(failure.current);setError(failure instanceof Error?failure.message:"This menu could not be replaced. Nothing changed.");}finally{setBusy(false);}})();}}>
        <p>Confirm the target and consequences. Replacement happens together or nothing changes.</p>
        <div className="replacement-target"><strong>{session.session.targetMenuName}</strong><span>{session.session.targetHadPublishedVersion?"The published version stays on screens.":"This menu has never been published."}</span></div>
        <div className="confirm-facts replacement-facts"><div><strong>{session.session.itemCount}</strong><span>items in the new draft</span></div><div><strong>{(session.session.targetAddedCount??0)+(session.session.targetRemovedCount??0)+(session.session.targetChangedCount??0)}</strong><span>unpublished changes already present</span></div><div><strong>0</strong><span>screens change now</span></div></div>
        <details><summary>What will be preserved</summary><p>Menu identity, theme, screen assignments, published version, and current 86 status. A restorable copy of today’s working draft is saved first.</p></details>
        <p className="not-live">Not live yet — publishing remains a separate action.</p>{error&&<p className="import-error" role="alert">{error}</p>}
        <div className="destination-actions"><button type="button" className="import-secondary" disabled={busy} onClick={()=>setSession({...session,session:{...session.session,destination:null,targetMenuId:null,targetUpdatedUtc:null,targetMenuName:null}})}>Choose another menu</button><button type="submit" className="import-primary" disabled={busy}>{busy?"Replacing…":"Replace menu"}</button></div>
      </form>}
    </section>
  </main>;

  return <main className="paste-import import-review" data-testid="menu-import-review" aria-labelledby="review-title">
    <div className="import-review-header">
      <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
      <div><p className="import-kicker">Import a menu · Review</p><h1 id="review-title">{unresolved.length ? `${unresolved.length} ${unresolved.length === 1 ? "item needs" : "items need"} you` : "Your review is complete"}</h1>
      <p>{unresolved.length ? "Answer only the uncertain lines. Everything else stays available below for inspection." : "Every pasted line has a destination. No menu has been changed."}</p></div>
      <div className="import-summary" aria-label="Import summary"><strong>{session.session.itemCount}</strong><span>items read</span><strong>{unresolved.length}</strong><span>answers left</span><small>Saved until {formatExpiry(session.session.expiresUtc)}</small></div>
    </div>
    {error && <p className="import-error review-error" role="alert">{error}</p>}
    {safeCount > 0 && <section className="safe-match-banner" data-testid="safe-match-banner"><Sparkles aria-hidden="true" /><div><strong>{safeCount} {safeCount === 1 ? "safe match" : "safe matches"}</strong><p>Same name after differences in capitals, punctuation, or spacing.</p></div><button disabled={busy} onClick={() => void mutate(current => acceptSafeMenuImportMatches(configuration, accessToken, current))}>Accept {safeCount} safe {safeCount === 1 ? "match" : "matches"}</button></section>}
    <section className="question-stack" aria-label="Items needing review">
      {nearMisses.length > 0 && <article className="grouped-question" data-testid="near-match-group"><p className="import-kicker">One grouped question</p><h2>Check these possible matches</h2><p>{nearMisses.length} pasted {nearMisses.length === 1 ? "item has" : "items have"} a similar library name. Nothing is selected for you.</p><div className="grouped-question-rows">{nearMisses.map(question => <QuestionCard key={question.questionKey} session={session} question={question} busy={busy}
        onAnswer={(choice, itemId) => void mutate(current => answerMenuImport(configuration, accessToken, current, question, choice, itemId))}
        onPromote={() => void 0} />)}</div></article>}
      {standaloneQuestions.map(question => <QuestionCard key={question.questionKey} session={session} question={question} busy={busy}
        onAnswer={(choice, itemId) => void mutate(current => answerMenuImport(configuration, accessToken, current, question, choice, itemId))}
        onPromote={() => void mutate(current => setMenuImportLineSection(configuration, accessToken, current, question.lineNumbers[0], true))} />)}
      {!unresolved.length && <div className="review-complete" data-testid="import-review-complete"><span><Check aria-hidden="true" /></span><div><h2>Ready for the next step</h2><p>This saved review can now be used to create or replace a menu when those destination steps open.</p></div></div>}
    </section>
    <section className="inventory-panel"><button aria-expanded={inventoryOpen} onClick={() => setInventoryOpen(value => !value)}>{inventoryOpen ? <ChevronDown aria-hidden="true" /> : <ChevronRight aria-hidden="true" />} Review all {session.session.lineCount} pasted lines</button>
      {inventoryOpen && <ol>{session.lines.map(line => <li key={line.lineNumber}><span>{line.lineNumber}</span><code>{line.rawText || "(blank line)"}</code><em>{line.disposition}</em>{line.disposition === "section" && !isNaturalHeading(line.rawText) && <button disabled={busy} onClick={() => void mutate(current => setMenuImportLineSection(configuration, accessToken, current, line.lineNumber, false))}><RotateCcw aria-hidden="true" /> Undo section</button>}</li>)}</ol>}
    </section>
    <p className="import-status" aria-live="polite">{busy ? "Saving your answer…" : `${unresolved.length} answers remaining.`}</p>
  </main>;
}

function QuestionCard({ session, question, busy, onAnswer, onPromote }: { session: MenuImportSession; question: MenuImportQuestion; busy: boolean; onAnswer: (choice: string, itemId?: string) => void; onPromote: () => void }) {
  const line = session.lines.find(candidate => candidate.lineNumber === question.lineNumbers[0]);
  if (question.kind === "unreadable") return <div className="question-card"><div className="question-number">Line {line?.lineNumber}</div><h2>What should this line become?</h2><blockquote>{line?.rawText}</blockquote><p>We couldn’t confidently read this as an item or heading.</p><div className="question-actions"><button disabled={busy} onClick={onPromote}>Make it a section</button><button disabled={busy} onClick={() => onAnswer("fallback")}>Keep in Imported items</button></div></div>;
  return <div className="question-card"><div className="question-number">Line {line?.lineNumber}</div><h2>Is “{line?.parsedName}” already in your library?</h2><blockquote>{line?.rawText}</blockquote><div className="candidate-list">{question.candidates.map(candidate => <button disabled={busy} key={candidate.itemId} onClick={() => onAnswer("same_item", candidate.itemId)}><span><strong>{candidate.displayName}</strong><small>{candidate.isSafe ? "Safe name match" : "Possible match"}</small></span><em>{candidate.displayPrice ?? "No price"}</em></button>)}</div><button className="new-item-choice" disabled={busy} onClick={() => onAnswer("new_item")}>No, add as a new item</button></div>;
}

function formatExpiry(value: string) { return new Intl.DateTimeFormat(undefined, { weekday: "short", hour: "numeric", minute: "2-digit" }).format(new Date(value)); }
function isNaturalHeading(value: string) { const letters = [...value.trim()].filter(character => /\p{L}/u.test(character)); return letters.length > 0 && letters.every(character => character === character.toUpperCase()); }

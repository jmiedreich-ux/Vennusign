import { useCallback, useEffect, useMemo, useRef, useState } from "react";
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
import VennusignLoader from "./VennusignLoader";

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
      .then(value => { if (current) { setSession(value); setMenuName(nameFor(value)); setError(null); } })
      .catch(failure => { if (current) setError(failure instanceof Error ? failure.message : "This import could not be resumed."); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [configuration, accessToken, sessionId]);

  useEffect(()=>{if(session?.session.status!=="resolved"||session.session.completedMenuId)return;let active=true;loadShelf(configuration,accessToken).then(value=>{if(active)setMenus(value.filter(menu=>!menu.isPutAway));}).catch(()=>{if(active)setError("Menus could not be loaded. Your review is still saved.");});return()=>{active=false;};},[configuration,accessToken,session?.session.status,session?.session.completedMenuId]);

  const unresolved = useMemo(() => session?.questions.filter(question => !question.answer) ?? [], [session]);
  /*
   * The newest session, for handlers that were created earlier.
   *
   * Every write carries the session revision as If-Match, and a stale one comes back 409 "This
   * import changed in another window" - which is true of a second tab and a lie about the only tab
   * open. The name field's own blur save bumps the revision, and the submit handler beside it was
   * closed over the session as it stood before that blur, so confirming a name you had just typed
   * conflicted with yourself. Reported by the owner on a brand-new import with nothing else open.
   */
  const latest = useRef<MenuImportSession | null>(null);
  const [suggestionDismissed, setSuggestionDismissed] = useState(false);
  /*
   * Two answers go over the wire one after the other, so the click is a wait, not an instant. It
   * disabled the buttons and said nothing, which is indistinguishable from a button that does not
   * work - and was reported as exactly that.
   */
  const [applying, setApplying] = useState(false);
  const safeCount = unresolved.filter(question => question.candidates.length === 1 && question.candidates[0].isSafe).length;
  const nearMisses = unresolved.filter(question => question.kind === "identity" && question.candidates.length > 0 && question.candidates.every(candidate => !candidate.isSafe));
  const standaloneQuestions = unresolved.filter(question => !nearMisses.includes(question));

  /*
   * A suggestion replaces its questions; it does not sit above them (owner, 2026-08-27).
   *
   * The banner asked "is this menu called Mana-Thai Cuisine?" and then the same two lines asked
   * again underneath, one row each. Two askings of one question, and the rows offered answers -
   * section, dish, leave out - that the banner had already made unnecessary. The owner's steer:
   * the rows belong behind "No, I'll answer them", not beside it.
   *
   * Declining is a real answer too, not just a way to see the rows. It means we do not know what
   * this menu is called - so the name is dropped rather than quietly kept, and naming happens in
   * the builder where an unnamed menu already knows what to do with itself (M6.5).
   */
  const suggestedLines = new Set(session?.lines.filter(line => line.suggestedVerdict).map(line => line.lineNumber) ?? []);
  const coveredBySuggestion = (question: MenuImportQuestion) =>
    !suggestionDismissed && suggestedName !== null && question.lineNumbers.every(line => suggestedLines.has(line));

  /*
   * What the screen is actually asking, which is not the same as how many questions exist.
   *
   * The banner stands in for every question it covers, so it counts as one thing to check rather
   * than as nothing. Counting the underlying questions instead said "2 items need you" above a
   * page showing none of them.
   */
  const asking = unresolved.filter(question => !coveredBySuggestion(question)).length
    + (unresolved.some(coveredBySuggestion) ? 1 : 0);

  useEffect(() => { latest.current = session ?? null; }, [session]);

  const mutate = useCallback(async (run: (current: MenuImportSession) => Promise<MenuImportSession>) => {
    const current = latest.current ?? session;
    if (!current) return;
    setBusy(true); setError(null);
    try { setSession(await run(current)); }
    catch (failure) {
      /*
       * A conflict that carries the current state is one we caused: the server hands back exactly
       * what changed, and the only writer was this page. Retried once against it, silently, because
       * telling somebody their import changed in another window when no other window exists is
       * worse than the extra round trip. A second failure is a real conflict and is shown.
       */
      if (failure instanceof MenuImportApiError && failure.current) {
        try { setSession(await run(failure.current)); return; }
        catch (retry) {
          if (retry instanceof MenuImportApiError && retry.current) setSession(retry.current);
          setError(retry instanceof Error ? retry.message : "That answer could not be saved.");
          return;
        }
      }
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

  /*
   * Reading is a wait worth drawing, drawn OVER the page rather than instead of it (M6.12).
   *
   * The first cut returned the loader in place of the paste card, which cleared the screen: the
   * text you had just pasted vanished, and what you got back was a blank page with an animation on
   * it. The owner's note was exactly that - the background does not need clearing. The loader's
   * modal variant is already `position: fixed; inset: 0`, so it only ever needed to be a sibling of
   * the page, not a replacement for it.
   */
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
        try { const created = await startMenuImport(configuration, accessToken, paste); setSession(created); setMenuName(nameFor(created)); onStarted(created.session.id); }
        catch (failure) { setError(failure instanceof Error ? failure.message : "VennuSign could not read that paste."); }
        finally { setBusy(false); }
      }}>{busy ? "Reading…" : "Read menu"}</button></div>
    </section>
    {busy && <VennusignLoader variant="modal" message="Reading your menu — finding its sections, items and prices." />}
  </main>;

  if (loading) return <main className="paste-import import-loading">
    <VennusignLoader variant="modal" message="Picking your import back up where you left it." />
  </main>;
  /*
   * A dead end is still a screen somebody is standing on.
   *
   * This said "We couldn't resume this import / This import is unavailable" and stopped: no reason,
   * no way forward, and a Menus link at the top that reads as decoration. The owner's words were
   * "does not tell me why". The message now comes from the server where the server had one and
   * names the fault where it did not, and there are two things to do rather than none.
   */
  if (!session) return <main className="paste-import import-unavailable" data-testid="menu-import-unavailable">
    <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
    <h1>We couldn’t resume this import</h1>
    {error && <p role="alert" data-testid="import-unavailable-reason">{error}</p>}
    <div className="destination-actions">
      <button type="button" className="import-secondary" onClick={onBack}>Back to menus</button>
      <button type="button" className="import-primary" data-testid="import-retry" onClick={() => window.location.reload()}>Try again</button>
    </div>
  </main>;

  /*
   * The suggestion, applied in one act (M6.11).
   *
   * A18 permits nothing to be pre-answered unless a rule can name why, and the residue pass names
   * a reason rather than a rule - so it arrives as a proposal and this is the operator saying yes.
   * The lines it identified are answered `leave_out`, because the restaurant's name is not a dish
   * and not a heading; the name and description travel to the destination step, where the operator
   * confirms them again before anything is created.
   */
  const suggestedName = !suggestionDismissed && session?.session.suggestedMenuName && unresolved.length > 0
    ? session.session.suggestedMenuName : null;

  const applySuggestion = async () => {
    if (!session) return;
    setApplying(true);
    setBusy(true);
    setError(null);
    /*
     * Held open for a beat.
     *
     * The animation was already here and already correct; two answers over a fast connection
     * finished before it could be seen, so the click still read as doing nothing - which is what
     * the owner reported the second time. This waits for the work AND for long enough to have
     * shown that there was work. It never shortens the wait and never invents one: if the answers
     * take two seconds, this adds nothing at all.
     */
    const seen = new Promise(resolve => setTimeout(resolve, 700));
    try {
      let current = session;
      let answered = 0;
      for (const question of session.questions.filter(candidate => !candidate.answer)) {
        const line = session.lines.find(candidate => candidate.lineNumber === question.lineNumbers[0]);
        if (!line?.suggestedVerdict) continue;
        current = await answerMenuImport(configuration, accessToken, current, question, "leave_out");
        answered++;
      }
      /*
       * Answering nothing is a fault, not a no-op.
       *
       * The first version returned quietly when no line carried a verdict, which is exactly what
       * happened once a re-parse wiped them: the button flashed and the questions stayed. Silence
       * made a server-side bug look like a dead control. If there is nothing to apply, say so and
       * leave the questions where the operator can answer them.
       */
      if (answered === 0) {
        setSuggestionDismissed(true);
        throw new Error("That suggestion is no longer current — this import has been re-read since. Answer the lines below instead.");
      }
      await seen;
      /*
       * The point of the feature, and it was missing.
       *
       * `suggestedMenuName` and `proposedMenuName` are unrelated fields: the first is written when
       * the paste is read, the second only when a destination is chosen. Accepting the suggestion
       * answered the lines and set neither, so the name the banner had just offered went nowhere
       * and the destination step still said "New menu". Reported by the owner as the name never
       * surfacing anywhere, which is exactly what happened.
       */
      if (session.session.suggestedMenuName) setMenuName(session.session.suggestedMenuName);
      setSession(current);
    } catch (failure) {
      await seen;
      setError(failure instanceof Error ? failure.message : "That suggestion could not be applied. Nothing changed.");
    } finally {
      setBusy(false);
      setApplying(false);
    }
  };

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

  /*
   * Choosing is confirming (owner, 2026-08-27).
   *
   * "Create a new menu" used to lead to a second screen asking the same thing again with a name
   * field on it - you had already said what you wanted and were asked to say it once more. The
   * name, the counts and the button that does it now sit on the choice itself.
   *
   * Replace keeps its own screen: that one states what it is about to overwrite and what stays
   * live, which is a confirmation. This one was a formality.
   *
   * Rendered in both arms of the ternary below, because a resumed session arrives with its
   * destination already chosen and has to land on something it can finish from.
   */
  const createMenuForm = <form className="destination-choice destination-choice--create" data-testid="create-menu-form" onSubmit={event => { event.preventDefault(); void (async () => {
    setBusy(true); setError(null);
    try {
      const chosen = await setMenuImportCreateDestination(configuration, accessToken, latest.current ?? session!, menuName);
      const created = await confirmMenuImportCreate(configuration, accessToken, chosen);
      setSession(created.import);
    } catch (failure) {
      if (failure instanceof MenuImportApiError && failure.current) setSession(failure.current);
      setError(failure instanceof Error ? failure.message : "This menu could not be created. Nothing changed.");
    } finally { setBusy(false); }
  })(); }}>
    <strong>Create a new menu</strong>
    <span>Build a new unpublished menu from all {session?.session.itemCount} imported items.</span>
    <label htmlFor="import-menu-name">Menu name</label>
    <input id="import-menu-name" required maxLength={200} value={menuName} data-testid="create-menu-name" onChange={event => setMenuName(event.target.value)} />
    <div className="confirm-facts"><div><strong>{session?.session.itemCount}</strong><span>items will be added</span></div><div><strong>0</strong><span>screens change now</span></div></div>
    <p className="not-live">Not live yet — publishing remains a separate action.</p>
    <button type="submit" className="import-primary" data-testid="create-menu" disabled={busy || !menuName.trim()}>{busy ? "Creating…" : "Create menu"}</button>
  </form>;

  if (session.session.status === "resolved") return <main className="paste-import import-destination" data-testid="menu-import-create" aria-labelledby="destination-title">
    <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
    <section className="destination-card"><h1 id="destination-title">{session.session.destination==="create"?"Create this menu?":session.session.destination==="replace"?`Replace ${session.session.targetMenuName}?`:"Where should these items go?"}</h1>
      {!session.session.destination ? <><p>Your review is saved. Creating is the first step that changes menu working content.</p>{error && <p className="import-error" role="alert">{error}</p>}
        {createMenuForm}
        <div className="replace-options"><h2>Replace an existing menu</h2><p>The pasted menu becomes its new unpublished draft. What guests see stays live until you publish.</p>{menus.length?<div className="target-list">{menus.map(menu=><button type="button" key={menu.menuId} disabled={busy} onClick={()=>void mutate(current=>setMenuImportReplaceDestination(configuration,accessToken,current,menu.menuId))}><span><strong>{menu.name}</strong><small>{menu.publishedVersion===null?"Never published":`${menu.draftCount} unpublished ${menu.draftCount===1?"change":"changes"}`}</small></span><ChevronRight aria-hidden="true"/></button>)}</div>:<p className="future-destination">No active menus are available to replace.</p>}</div></> : session.session.destination==="create"?
      createMenuForm:<form onSubmit={event=>{event.preventDefault();void(async()=>{setBusy(true);setError(null);try{const replaced=await confirmMenuImportReplace(configuration,accessToken,latest.current??session);setSession(replaced.import);}catch(failure){if(failure instanceof MenuImportApiError&&failure.current)setSession(failure.current);setError(failure instanceof Error?failure.message:"This menu could not be replaced. Nothing changed.");}finally{setBusy(false);}})();}}>
        <p>Confirm the target and consequences. Replacement happens together or nothing changes.</p>
        <div className="replacement-target"><strong>{session.session.targetMenuName}</strong><span>{session.session.targetHadPublishedVersion?"The published version stays on screens.":"This menu has never been published."}</span></div>
        <div className="confirm-facts replacement-facts"><div><strong>{session.session.itemCount}</strong><span>items in the new draft</span></div><div><strong>{(session.session.targetAddedCount??0)+(session.session.targetRemovedCount??0)+(session.session.targetChangedCount??0)}</strong><span>{`${(session.session.targetAddedCount??0)+(session.session.targetRemovedCount??0)+(session.session.targetChangedCount??0)===1?"unpublished change":"unpublished changes"} already present`}</span><small>{`${session.session.targetAddedCount??0} ${(session.session.targetAddedCount??0)===1?"item added":"items added"} · ${session.session.targetRemovedCount??0} removed · ${session.session.targetChangedCount??0} changed`}</small></div><div><strong>0</strong><span>screens change now</span></div></div>
        <details><summary>What will be preserved</summary><p>Menu identity, theme, screen assignments, published version, and current 86 status. A restorable copy of today’s working draft is saved first.</p></details>
        <p className="not-live">Not live yet — publishing remains a separate action.</p>{error&&<p className="import-error" role="alert">{error}</p>}
        <div className="destination-actions"><button type="button" className="import-secondary" disabled={busy} onClick={()=>setSession({...session,session:{...session.session,destination:null,targetMenuId:null,targetUpdatedUtc:null,targetMenuName:null}})}>Choose another menu</button><button type="submit" className="import-primary" disabled={busy}>{busy?"Replacing…":"Replace menu"}</button></div>
      </form>}
    </section>
  </main>;

  return <main className="paste-import import-review" data-testid="menu-import-review" aria-labelledby="review-title">
    <div className="import-review-header">
      <button className="import-back" onClick={onBack}><ArrowLeft aria-hidden="true" /> Menus</button>
      <div><p className="import-kicker">Import a menu · Review</p><h1 id="review-title">{asking ? `${asking} ${asking === 1 ? "item needs" : "items need"} you` : "Your review is complete"}</h1>
      <p>{unresolved.length ? "Answer only the uncertain lines. Everything else stays available below for inspection." : "Every pasted line has a destination. No menu has been changed."}</p></div>
      <div className="import-summary" aria-label="Import summary"><strong>{session.session.itemCount}</strong><span>items read</span><strong>{asking}</strong><span>answers left</span><small>Saved until {formatExpiry(session.session.expiresUtc)}</small></div>
    </div>
    {error && <p className="import-error review-error" role="alert">{error}</p>}
    {safeCount > 0 && <section className="safe-match-banner" data-testid="safe-match-banner"><Sparkles aria-hidden="true" /><div><strong>{safeCount} {safeCount === 1 ? "safe match" : "safe matches"}</strong><p>Same name after differences in capitals, punctuation, or spacing.</p></div><button disabled={busy} onClick={() => void mutate(current => acceptSafeMenuImportMatches(configuration, accessToken, current))}>Accept {safeCount} safe {safeCount === 1 ? "match" : "matches"}</button></section>}
    {suggestedName && <section className="suggestion-banner" data-testid="import-suggestion">
      <div>
        <p className="import-kicker">Two lines we could not place</p>
        <h2>Is this menu called “{suggestedName}”?</h2>
        {session.session.suggestedMenuDescription && <p>We read “{session.session.suggestedMenuDescription}” as how you describe it.</p>}
        <p className="suggestion-banner__note">Nothing is filled in until you say so. Both lines are left off the menu itself.</p>
      </div>
      {applying
        ? <div className="suggestion-banner__actions" data-testid="suggestion-applying">
            <VennusignLoader variant="inline" message="Taking these off the menu and keeping the name." />
          </div>
        : <div className="suggestion-banner__actions">
            <button type="button" className="import-secondary" disabled={busy} data-testid="suggestion-dismiss" onClick={() => { setSuggestionDismissed(true); if (session.session.suggestedMenuName && menuName === session.session.suggestedMenuName) setMenuName("New menu"); }}>No, I'll answer them</button>
            <button type="button" className="import-primary" disabled={busy} data-testid="suggestion-accept" onClick={() => void applySuggestion()}>Yes, use these</button>
          </div>}
    </section>}

    <section className="question-stack" aria-label="Items needing review">
      {nearMisses.length > 0 && <article className="grouped-question" data-testid="near-match-group"><p className="import-kicker">One grouped question</p><h2>Check these possible matches</h2><p>{nearMisses.length} pasted {nearMisses.length === 1 ? "item has" : "items have"} a similar library name. Nothing is selected for you.</p><div className="grouped-question-rows">{nearMisses.map(question => <QuestionCard key={question.questionKey} session={session} question={question} busy={busy}
        onAnswer={(choice, itemId) => void mutate(current => answerMenuImport(configuration, accessToken, current, question, choice, itemId))}
        onPromote={() => void 0} />)}</div></article>}
      {standaloneQuestions.filter(question => !coveredBySuggestion(question)).map(question => <QuestionCard key={question.questionKey} session={session} question={question} busy={busy}
        onAnswer={(choice, itemId) => void mutate(current => answerMenuImport(configuration, accessToken, current, question, choice, itemId))}
        onPromote={() => void mutate(current => setMenuImportLineSection(configuration, accessToken, current, question.lineNumbers[0], true))} />)}
      {!unresolved.length && <div className="review-complete" data-testid="import-review-complete"><span><Check aria-hidden="true" /></span><div><h2>Nothing left to answer</h2><p>Choose where these items go next. Nothing reaches a screen until you publish.</p></div></div>}
    </section>
    <section className="inventory-panel"><button aria-expanded={inventoryOpen} onClick={() => setInventoryOpen(value => !value)}>{inventoryOpen ? <ChevronDown aria-hidden="true" /> : <ChevronRight aria-hidden="true" />} Review all {session.session.lineCount} pasted lines</button>
      {inventoryOpen && <ol>{session.lines.map(line => <li key={line.lineNumber}><span>{line.lineNumber}</span><code>{line.rawText || "(blank line)"}</code><em>{line.disposition}</em>{line.disposition === "section" && !isNaturalHeading(line.rawText) && <button disabled={busy} onClick={() => void mutate(current => setMenuImportLineSection(configuration, accessToken, current, line.lineNumber, false))}><RotateCcw aria-hidden="true" /> Undo section</button>}</li>)}</ol>}
    </section>
    <p className="import-status" aria-live="polite">{busy ? "Saving your answer…" : `${asking} answers remaining.`}</p>
  </main>;
}

/**
 * One decision, one row.
 *
 * This was five stacked blocks per question - a line number, a heading, the raw line in a
 * blockquote, a sentence of explanation, then the buttons. At two questions it read as roomy; at
 * fifteen it was a page of scrolling to make fifteen small decisions. The row puts the pasted text
 * where the eye already is and the choices beside it, so ten fit in the space one used to take.
 *
 * The choices name **outcomes**, which is decision 10's rule - "never a bare action: it states what
 * replaces it, in the same click" - applied to a screen that had been ignoring it. "Keep in
 * Imported items" named a mechanism a first-time operator has never heard of, and did not say that
 * it creates an item, or where that item goes. It is gone. "Imported items" survives as the place a
 * dish lands when no heading sits above it, which is plumbing nobody needs told.
 */
function QuestionCard({ session, question, busy, onAnswer, onPromote }: { session: MenuImportSession; question: MenuImportQuestion; busy: boolean; onAnswer: (choice: string, itemId?: string) => void; onPromote: () => void }) {
  const line = session.lines.find(candidate => candidate.lineNumber === question.lineNumbers[0]);

  const suggestion = line?.suggestedVerdict ? describeSuggestion(line.suggestedVerdict) : null;

  if (question.kind === "unreadable") return (
    <div className="question-row" data-testid="question-row" data-suggested={line?.suggestedVerdict ?? undefined}>
      <div className="question-row__line">
        <span className="question-row__number">Line {line?.lineNumber}</span>
        <q>{line?.rawText.trim()}</q>
        {suggestion ? <small className="question-row__suggestion" data-testid="row-suggestion" title={line?.suggestedReason ?? undefined}>{suggestion}</small> : null}
      </div>
      <div className="question-row__choices" role="group" aria-label={`What should line ${line?.lineNumber} become?`}>
        <button type="button" disabled={busy} onClick={onPromote} data-testid="answer-section">
          <strong>A section heading</strong><small>Everything under it goes in this group</small>
        </button>
        <button type="button" disabled={busy} onClick={() => onAnswer("fallback")} data-testid="answer-dish">
          <strong>A dish</strong><small>Goes in an Imported items group to sort later</small>
        </button>
        <button type="button" className="question-row__leave" disabled={busy} onClick={() => onAnswer("leave_out")} data-testid="answer-leave-out">
          <strong>Leave it out</strong><small>Not imported. Your pasted text still has it</small>
        </button>
      </div>
    </div>
  );

  return (
    <div className="question-row" data-testid="question-row">
      <div className="question-row__line">
        <span className="question-row__number">Line {line?.lineNumber}</span>
        <q>{line?.rawText.trim()}</q>
        <small>Already in your library?</small>
      </div>
      <div className="question-row__choices" role="group" aria-label={`Is ${line?.parsedName} already in your library?`}>
        {question.candidates.map(candidate => (
          <button type="button" disabled={busy} key={candidate.itemId} onClick={() => onAnswer("same_item", candidate.itemId)}>
            <strong>{candidate.displayName}</strong>
            <small>{candidate.isSafe ? "Safe name match" : "Possible match"} · {candidate.displayPrice ?? "No price"}</small>
          </button>
        ))}
        <button type="button" disabled={busy} onClick={() => onAnswer("new_item")} data-testid="answer-new-item">
          <strong>Add as a new item</strong><small>It isn't one you already have</small>
        </button>
        <button type="button" className="question-row__leave" disabled={busy} onClick={() => onAnswer("leave_out")} data-testid="answer-leave-out">
          <strong>Leave it out</strong><small>Not imported. Your pasted text still has it</small>
        </button>
      </div>
    </div>
  );
}

/**
 * What the menu-name field should say for a session.
 *
 * A name the operator has already settled wins; then the suggestion, which is what the banner
 * offered; then the placeholder. Written once because three separate paths were each deciding it
 * and two of them decided nothing at all.
 */
function nameFor(session: MenuImportSession) {
  return session.session.proposedMenuName ?? session.session.suggestedMenuName ?? "New menu";
}

function formatExpiry(value: string) { return new Intl.DateTimeFormat(undefined, { weekday: "short", hour: "numeric", minute: "2-digit" }).format(new Date(value)); }
function isNaturalHeading(value: string) { const letters = [...value.trim()].filter(character => /\p{L}/u.test(character)); return letters.length > 0 && letters.every(character => character === character.toUpperCase()); }

/**
 * What a verdict is called on the row it is about.
 *
 * Phrased as a reading rather than a ruling - "we read this as…" - because that is what it is: a
 * proposal the operator is checking, not a decision already taken (A18). The model's own reason
 * rides along as the row's title attribute for anyone who wants to know why.
 */
function describeSuggestion(verdict: string) {
  if (verdict === "menu_name") return "We read this as the menu's name";
  if (verdict === "menu_description") return "We read this as how you describe the menu";
  if (verdict === "section_heading") return "We read this as a section heading";
  if (verdict === "dish") return "We read this as a dish";
  return "We'd leave this one out";
}

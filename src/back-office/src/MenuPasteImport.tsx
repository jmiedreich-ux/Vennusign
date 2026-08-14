import { useCallback, useEffect, useMemo, useState } from "react";
import { ArrowLeft, Check, ChevronDown, ChevronRight, ClipboardPaste, RotateCcw, Sparkles } from "lucide-react";
import {
  MenuImportApiError,
  acceptSafeMenuImportMatches,
  answerMenuImport,
  loadMenuImport,
  setMenuImportLineSection,
  startMenuImport,
  type MenuImportQuestion,
  type MenuImportSession
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import "./menu-paste-import.css";

type Props = { configuration: BackOfficeConfiguration; accessToken: string; sessionId: string | null; onBack: () => void; onStarted: (id: string) => void };

export default function MenuPasteImport({ configuration, accessToken, sessionId, onBack, onStarted }: Props) {
  const [paste, setPaste] = useState("");
  const [session, setSession] = useState<MenuImportSession | null>(null);
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(Boolean(sessionId));
  const [error, setError] = useState<string | null>(null);
  const [inventoryOpen, setInventoryOpen] = useState(false);

  useEffect(() => {
    if (!sessionId) return;
    let current = true;
    setLoading(true);
    loadMenuImport(configuration, accessToken, sessionId)
      .then(value => { if (current) { setSession(value); setError(null); } })
      .catch(failure => { if (current) setError(failure instanceof Error ? failure.message : "This import could not be resumed."); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [configuration, accessToken, sessionId]);

  const unresolved = useMemo(() => session?.questions.filter(question => !question.answer) ?? [], [session]);
  const safeCount = unresolved.filter(question => question.candidates.length === 1 && question.candidates[0].isSafe).length;

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
    <p className="import-kicker">Menu import</p>
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
      {unresolved.map(question => <QuestionCard key={question.questionKey} session={session} question={question} busy={busy}
        onAnswer={(choice, itemId) => void mutate(current => answerMenuImport(configuration, accessToken, current, question, choice, itemId))}
        onPromote={() => void mutate(current => setMenuImportLineSection(configuration, accessToken, current, question.lineNumbers[0], true))} />)}
      {!unresolved.length && <div className="review-complete" data-testid="import-review-complete"><span><Check aria-hidden="true" /></span><div><h2>Ready for the next step</h2><p>This saved review can now be used to create or replace a menu when those destination steps open.</p></div></div>}
    </section>
    <section className="inventory-panel"><button aria-expanded={inventoryOpen} onClick={() => setInventoryOpen(value => !value)}>{inventoryOpen ? <ChevronDown aria-hidden="true" /> : <ChevronRight aria-hidden="true" />} Review all {session.session.lineCount} pasted lines</button>
      {inventoryOpen && <ol>{session.lines.map(line => <li key={line.lineNumber}><span>{line.lineNumber}</span><code>{line.rawText || "(blank line)"}</code><em>{line.disposition}</em>{line.disposition === "section" && !isNaturalHeading(line.rawText) && <button disabled={busy} onClick={() => void mutate(current => setMenuImportLineSection(configuration, accessToken, current, line.lineNumber, false))}><RotateCcw aria-hidden="true" /> Undo section</button>}</li>)}</ol>}
    </section>
    <p className="import-status" aria-live="polite">{busy ? "Saving your answer…" : session.session.status === "resolved" ? "All required answers are saved." : `${unresolved.length} answers remaining.`}</p>
  </main>;
}

function QuestionCard({ session, question, busy, onAnswer, onPromote }: { session: MenuImportSession; question: MenuImportQuestion; busy: boolean; onAnswer: (choice: string, itemId?: string) => void; onPromote: () => void }) {
  const line = session.lines.find(candidate => candidate.lineNumber === question.lineNumbers[0]);
  if (question.kind === "unreadable") return <article className="question-card"><div className="question-number">Line {line?.lineNumber}</div><h2>What should this line become?</h2><blockquote>{line?.rawText}</blockquote><p>We couldn’t confidently read this as an item or heading.</p><div className="question-actions"><button disabled={busy} onClick={onPromote}>Make it a section</button><button disabled={busy} onClick={() => onAnswer("fallback")}>Keep in Imported items</button></div></article>;
  return <article className="question-card"><div className="question-number">Line {line?.lineNumber}</div><h2>Is “{line?.parsedName}” already in your library?</h2><blockquote>{line?.rawText}</blockquote><div className="candidate-list">{question.candidates.map(candidate => <button disabled={busy} key={candidate.itemId} onClick={() => onAnswer("same_item", candidate.itemId)}><span><strong>{candidate.displayName}</strong><small>{candidate.isSafe ? "Safe name match" : "Possible match"}</small></span><em>{candidate.displayPrice ?? "No price"}</em></button>)}</div><button className="new-item-choice" disabled={busy} onClick={() => onAnswer("new_item")}>No, add as a new item</button></article>;
}

function formatExpiry(value: string) { return new Intl.DateTimeFormat(undefined, { weekday: "short", hour: "numeric", minute: "2-digit" }).format(new Date(value)); }
function isNaturalHeading(value: string) { const letters = [...value.trim()].filter(character => /\p{L}/u.test(character)); return letters.length > 0 && letters.every(character => character === character.toUpperCase()); }

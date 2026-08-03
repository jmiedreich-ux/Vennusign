import { useEffect, useState, type FormEvent } from "react";
import { cancelEmergencyBroadcast, createEmergencyBroadcast, loadEmergencyBroadcasts, type EmergencyBroadcast } from "./api";
import type { BackOfficeConfiguration } from "./config";
import { useDestructiveReview } from "./DestructiveReviewDialog";

type Props = {
  configuration: BackOfficeConfiguration; apiKey: string; venueId: string; enabled: boolean;
  screens: Array<{ id: string; name: string }>;
  showUpgradePrompt?: boolean;
};

export default function EmergencyBroadcastAdministration({ configuration, apiKey, venueId, enabled, screens, showUpgradePrompt = true }: Props) {
  const [rows, setRows] = useState<EmergencyBroadcast[]>([]);
  const [screenId, setScreenId] = useState("");
  const [title, setTitle] = useState("");
  const [message, setMessage] = useState("");
  const [mediaUrl, setMediaUrl] = useState("");
  const [duration, setDuration] = useState(15);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const { review, reviewDialog } = useDestructiveReview();
  const reload = () => { void loadEmergencyBroadcasts(configuration, apiKey, venueId).then(setRows).catch(() => setError("Broadcasts could not be loaded.")); };
  useEffect(reload, [apiKey, configuration, venueId]);
  const create = async (event: FormEvent) => {
    event.preventDefault(); setError(undefined); setNotice(undefined);
    const target = screenId ? screens.find(screen => screen.id === screenId)?.name ?? "the selected screen" : `all ${screens.length} venue screen${screens.length === 1 ? "" : "s"}`;
    if (!await review({ title: `Activate “${title}”?`, consequence: `This will override normal content on ${target} immediately for ${duration} minute${duration === 1 ? "" : "s"}. Delivery acknowledgement is not available.`, confirmLabel: "Activate broadcast", tone: "caution" })) return;
    setBusy(true);
    try {
      const created = await createEmergencyBroadcast(configuration, apiKey, venueId, {
        screenId: screenId || undefined, title, message, mediaUrl: mediaUrl || undefined, durationMinutes: duration
      });
      setTitle(""); setMessage(""); setMediaUrl(""); reload();
      setNotice(`Broadcast ${created.id.slice(0, 8)} queued to ${target}. Screen acknowledgements are not available; verify player status if delivery is critical.`);
    } catch { setError("Broadcast could not be activated."); }
    finally { setBusy(false); }
  };
  const cancel = async (row: EmergencyBroadcast) => {
    if (!await review({ title: `Cancel “${row.title}”?`, consequence: "A cancellation will be queued immediately and the target will return to normal content after the player receives it. Delivery acknowledgement is not available.", confirmLabel: "Cancel broadcast", tone: "caution" })) return;
    setBusy(true); setError(undefined); setNotice(undefined);
    try { await cancelEmergencyBroadcast(configuration, apiKey, venueId, row.id); reload(); setNotice(`“${row.title}” cancellation queued. Verify player status if delivery is critical.`); }
    catch { setError("Broadcast could not be cancelled."); }
    finally { setBusy(false); }
  };
  const active = rows.filter(row => row.isActive && Date.parse(row.expiresUtc) > Date.now());
  const history = rows.filter(row => !active.some(item => item.id === row.id)).slice(0, 10);
  const targetDescription = screenId
    ? screens.find(screen => screen.id === screenId)?.name ?? "Selected screen unavailable"
    : `All venue screens (${screens.length})`;

  return <article className="menu-editor emergency-broadcast-admin">
    {reviewDialog}
    <div className="menu-editor-heading"><div><p>Priority override</p><h3>Emergency broadcast</h3></div><span>{active.length} active</span></div>
    {showUpgradePrompt && !enabled ? <aside className="tier-prompt"><div><strong>Emergency Broadcast requires Pro</strong><p>Controls remain visible while activation is soft locked.</p></div></aside> : null}
    <p>Emergency broadcasts are the highest-priority override. Activation and cancellation are sent in real time, but delivery acknowledgement is not currently available.</p>
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {notice ? <p className="state success" role="status">{notice}</p> : null}
    {!active.length ? <p className="state">No emergency broadcast is active. Scheduled content remains in control.</p> : null}
    <ul>{active.map(row => <li key={row.id}><strong>{row.title}</strong><span>{row.screenId ? screens.find(screen => screen.id === row.screenId)?.name ?? "Targeted screen" : `All venue screens (${screens.length})`} · expires {new Date(row.expiresUtc).toLocaleTimeString()}</span><button disabled={!enabled || busy} onClick={() => cancel(row)}>Cancel override</button></li>)}</ul>
    <form onSubmit={create}><fieldset disabled={!enabled || busy || screens.length === 0}>
      <legend>Activate a live override</legend>
      <label>Target<select value={screenId} onChange={event => setScreenId(event.target.value)}><option value="">All venue screens</option>{screens.map(screen => <option key={screen.id} value={screen.id}>{screen.name}</option>)}</select></label>
      <p className="target-impact"><strong>Target impact:</strong> {targetDescription}</p>
      <label>Title<input required maxLength={200} value={title} onChange={event => setTitle(event.target.value)} /></label>
      <label>Message<textarea required maxLength={2000} value={message} onChange={event => setMessage(event.target.value)} /></label>
      <label>Media URL<input type="url" value={mediaUrl} onChange={event => setMediaUrl(event.target.value)} /></label>
      <label>Duration minutes<input type="number" min={1} max={1440} value={duration} onChange={event => setDuration(Number(event.target.value))} /></label>
      <button>Activate broadcast</button>
    </fieldset></form>
    {screens.length === 0 ? <p className="state">Activation is disabled because there are no screen targets.</p> : null}
    {history.length ? <details className="broadcast-history"><summary>Recent broadcast history ({history.length})</summary><ul>{history.map(row => <li key={row.id}><strong>{row.title}</strong><span>{row.isActive ? "Expired" : "Cancelled"} · started {new Date(row.startsUtc).toLocaleString()}</span></li>)}</ul></details> : null}
  </article>;
}

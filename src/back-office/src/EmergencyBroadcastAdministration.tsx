import { useEffect, useState, type FormEvent } from "react";
import { cancelEmergencyBroadcast, createEmergencyBroadcast, loadEmergencyBroadcasts, type EmergencyBroadcast } from "./api";
import type { BackOfficeConfiguration } from "./config";

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
  const [error, setError] = useState<string>();
  const reload = () => { void loadEmergencyBroadcasts(configuration, apiKey, venueId).then(setRows).catch(() => setError("Broadcasts could not be loaded.")); };
  useEffect(reload, [apiKey, configuration, venueId]);
  const create = async (event: FormEvent) => {
    event.preventDefault(); setError(undefined);
    try {
      await createEmergencyBroadcast(configuration, apiKey, venueId, {
        screenId: screenId || undefined, title, message, mediaUrl: mediaUrl || undefined, durationMinutes: duration
      });
      setTitle(""); setMessage(""); setMediaUrl(""); reload();
    } catch { setError("Broadcast could not be activated."); }
  };
  const cancel = async (id: string) => {
    try { await cancelEmergencyBroadcast(configuration, apiKey, venueId, id); reload(); }
    catch { setError("Broadcast could not be cancelled."); }
  };
  const active = rows.filter(row => row.isActive && Date.parse(row.expiresUtc) > Date.now());

  return <article className="menu-editor emergency-broadcast-admin">
    <div className="menu-editor-heading"><div><p>Priority override</p><h3>Emergency broadcast</h3></div><span>{active.length} active</span></div>
    {showUpgradePrompt && !enabled ? <aside className="tier-prompt"><div><strong>Emergency Broadcast requires Pro</strong><p>Controls remain visible while activation is soft locked.</p></div></aside> : null}
    {error ? <p className="state error">{error}</p> : null}
    <ul>{active.map(row => <li key={row.id}><strong>{row.title}</strong><span>{row.screenId ? "Targeted screen" : "All venue screens"} · expires {new Date(row.expiresUtc).toLocaleTimeString()}</span><button disabled={!enabled} onClick={() => cancel(row.id)}>Cancel</button></li>)}</ul>
    <form onSubmit={create}><fieldset disabled={!enabled}>
      <label>Target<select value={screenId} onChange={event => setScreenId(event.target.value)}><option value="">All venue screens</option>{screens.map(screen => <option key={screen.id} value={screen.id}>{screen.name}</option>)}</select></label>
      <label>Title<input required maxLength={200} value={title} onChange={event => setTitle(event.target.value)} /></label>
      <label>Message<textarea required maxLength={2000} value={message} onChange={event => setMessage(event.target.value)} /></label>
      <label>Media URL<input type="url" value={mediaUrl} onChange={event => setMediaUrl(event.target.value)} /></label>
      <label>Duration minutes<input type="number" min={1} max={1440} value={duration} onChange={event => setDuration(Number(event.target.value))} /></label>
      <button>Activate broadcast</button>
    </fieldset></form>
  </article>;
}

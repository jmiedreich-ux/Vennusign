import { useEffect, useState, type FormEvent } from "react";
import { loadHappyHour, saveHappyHour, type HappyHourWrite } from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string; enabled: boolean };
const days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const time = (value: string) => value.slice(0, 5);
const wireTime = (value: string) => `${value}:00`;

export default function HappyHourAdministration({ configuration, apiKey, venueId, enabled }: Props) {
  const [draft, setDraft] = useState<HappyHourWrite>({
    startLocalTime: "16:00:00", endLocalTime: "19:00:00",
    activeDaysMask: 127, isEnabled: true, overrideMode: "automatic"
  });
  const [active, setActive] = useState(false);
  const [endsAtUtc, setEndsAtUtc] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();

  useEffect(() => {
    loadHappyHour(configuration, apiKey, venueId)
      .then(snapshot => {
        setActive(snapshot.isActive); setEndsAtUtc(snapshot.endsAtUtc);
        if (snapshot.schedule) setDraft({
          startLocalTime: snapshot.schedule.startLocalTime,
          endLocalTime: snapshot.schedule.endLocalTime,
          activeDaysMask: snapshot.schedule.activeDaysMask,
          isEnabled: snapshot.schedule.isEnabled,
          overrideMode: snapshot.schedule.overrideMode
        });
      })
      .catch(() => setError("Happy hour could not be loaded."));
  }, [apiKey, configuration, venueId]);

  const save = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined);
    try {
      const snapshot = await saveHappyHour(configuration, apiKey, venueId, {
        ...draft, startLocalTime: wireTime(time(draft.startLocalTime)), endLocalTime: wireTime(time(draft.endLocalTime))
      });
      setActive(snapshot.isActive); setEndsAtUtc(snapshot.endsAtUtc);
    } catch { setError("Happy hour could not be saved."); }
    finally { setBusy(false); }
  };
  const toggleDay = (day: number) => setDraft(value => ({ ...value, activeDaysMask: value.activeDaysMask ^ (1 << day) }));

  return <article className="menu-editor happy-hour-admin">
    <div className="menu-editor-heading"><div><p>Pro scheduling</p><h3>Happy hour</h3></div><span>{active ? "Active" : "Inactive"}</span></div>
    {!enabled ? <aside className="tier-prompt"><div><strong>Happy Hour requires Pro</strong><p>Your schedule stays visible. Upgrade or apply an override to edit it.</p></div></aside> : null}
    {error ? <p className="state error">{error}</p> : null}
    <form onSubmit={save}>
      <fieldset disabled={!enabled || busy}>
        <label>Start<input type="time" value={time(draft.startLocalTime)} onChange={event => setDraft(value => ({ ...value, startLocalTime: wireTime(event.target.value) }))} /></label>
        <label>End<input type="time" value={time(draft.endLocalTime)} onChange={event => setDraft(value => ({ ...value, endLocalTime: wireTime(event.target.value) }))} /></label>
        <label>Mode<select value={draft.overrideMode} onChange={event => setDraft(value => ({ ...value, overrideMode: event.target.value as HappyHourWrite["overrideMode"] }))}>
          <option value="automatic">Automatic</option><option value="force_on">Force on</option><option value="force_off">Force off</option>
        </select></label>
        <label><input type="checkbox" checked={draft.isEnabled} onChange={event => setDraft(value => ({ ...value, isEnabled: event.target.checked }))} />Enabled</label>
        <div>{days.map((label, day) => <label key={label}><input type="checkbox" checked={(draft.activeDaysMask & (1 << day)) !== 0} onChange={() => toggleDay(day)} />{label}</label>)}</div>
        <button disabled={draft.activeDaysMask === 0}>Save happy hour</button>
      </fieldset>
    </form>
    {active && endsAtUtc ? <p>Current automatic window ends {new Date(endsAtUtc).toLocaleTimeString()}.</p> : null}
  </article>;
}

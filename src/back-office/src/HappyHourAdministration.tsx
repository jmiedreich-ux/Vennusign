import { useEffect, useState, type FormEvent } from "react";
import { BackOfficeApiError, loadHappyHour, saveHappyHour, type BackOfficeCapabilityDenial, type HappyHourWrite } from "./api";
import type { BackOfficeConfiguration } from "./config";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; enabled: boolean; showUpgradePrompt?: boolean };
const days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

/**
 * The API's `resolution` field is a machine code for the client to act on, not copy.
 * Only codes with a genuine user-facing instruction get sentence text; everything
 * else renders nothing rather than leaking an identifier into the UI.
 */
const resolutionCopy = (resolution?: string) => {
  switch (resolution) {
    case "review_product_access": return "Review your plan to enable it.";
    case "ask_scope_administrator": return "Ask an administrator to grant access.";
    case "sign_in_again": return "Sign in again to refresh your access.";
    case "remove_or_increase_allowance": return "Remove an existing item or increase the allowance.";
    // retry_later adds nothing beyond the retry-timing sentence that follows.
    default: return undefined;
  }
};
const time = (value: string) => value.slice(0, 5);
const wireTime = (value: string) => `${value}:00`;

export default function HappyHourAdministration({ configuration, apiKey, venueId, enabled, showUpgradePrompt = true }: Props) {
  const [draft, setDraft] = useState<HappyHourWrite>({
    startLocalTime: "16:00:00", endLocalTime: "19:00:00",
    activeDaysMask: 127, isEnabled: true, overrideMode: "automatic"
  });
  const [active, setActive] = useState(false);
  const [endsAtUtc, setEndsAtUtc] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  const [denial, setDenial] = useState<BackOfficeCapabilityDenial>();

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
      .catch((reason: unknown) => {
        // A capability refusal is not a load failure. Reporting a temporary rollout
        // block as "could not be loaded" tells the user something is broken when the
        // feature is simply not switched on for them yet.
        const refusal = reason instanceof BackOfficeApiError ? reason.denial : undefined;
        if (refusal) { setDenial(refusal); setError(undefined); }
        else { setDenial(undefined); setError("Happy hour could not be loaded."); }
      });
  }, [apiKey, configuration, venueId]);

  const retryLabel = (seconds?: number) => {
    if (!seconds || seconds <= 0) return undefined;
    if (seconds < 60) return `Try again in about ${seconds} seconds.`;
    const minutes = Math.ceil(seconds / 60);
    if (minutes < 60) return `Try again in about ${minutes} minute${minutes === 1 ? "" : "s"}.`;
    const hours = Math.ceil(minutes / 60);
    return `Try again in about ${hours} hour${hours === 1 ? "" : "s"}.`;
  };

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
    {showUpgradePrompt && !enabled ? <aside className="tier-prompt"><div><strong>Happy Hour requires Pro</strong><p>Your schedule stays visible. Upgrade or apply an override to edit it.</p></div></aside> : null}
    {denial ? <p className={`state ${denial.decision === "temporarily-blocked" ? "blocked" : "denied"}`} role="status" data-testid="capability-denial" data-decision={denial.decision} data-capability={denial.capabilityId} data-retry-after={denial.retryAfterSeconds}>
      <strong>{denial.decision === "temporarily-blocked" ? "Temporarily unavailable" : "Not available"}</strong>
      {" "}{denial.message ?? "Happy hour is not available for this venue right now."}
      {/* denial.resolution is a machine code (retry_later, review_product_access);
          never render it. Map the ones that carry user-facing meaning instead. */}
      {resolutionCopy(denial.resolution) ? ` ${resolutionCopy(denial.resolution)}` : ""}
      {denial.decision === "temporarily-blocked" ? ` ${retryLabel(denial.retryAfterSeconds) ?? "This is temporary - your schedule is unchanged."}` : ""}
    </p> : null}
    {error ? <p className="state error" role="alert">{error}</p> : null}
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

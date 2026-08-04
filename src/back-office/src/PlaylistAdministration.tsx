import { useEffect, useState, type FormEvent } from "react";
import { createPlaylistSlide, deletePlaylistSlide, loadPlaylist, reorderPlaylist, updatePlaylistSlide, type PlaylistSlide, type PlaylistSlideWrite } from "./api";
import type { BackOfficeConfiguration } from "./config";
import { useDestructiveReview } from "./DestructiveReviewDialog";

type Props = {
  configuration: BackOfficeConfiguration; apiKey: string; venueId: string; enabled: boolean;
  screens: Array<{ id: string; name: string }>;
  showUpgradePrompt?: boolean;
};
const days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const blank = (): PlaylistSlideWrite => ({ slideType: "menu", dwellSeconds: 10, isEnabled: true });
const time = (value?: string) => value?.slice(0, 5) ?? "";

export default function PlaylistAdministration({ configuration, apiKey, venueId, enabled, screens, showUpgradePrompt = true }: Props) {
  const [screenId, setScreenId] = useState("");
  const [slides, setSlides] = useState<PlaylistSlide[]>([]);
  const [draft, setDraft] = useState<PlaylistSlideWrite>(blank);
  const [editingId, setEditingId] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const { review, reviewDialog } = useDestructiveReview();

  useEffect(() => {
    if (!screens.some(screen => screen.id === screenId)) setScreenId(screens[0]?.id ?? "");
  }, [screenId, screens]);
  const reload = async () => {
    if (!screenId) { setSlides([]); return; }
    setSlides(await loadPlaylist(configuration, apiKey, venueId, screenId));
  };
  useEffect(() => { reload().catch(() => setError("Playlist could not be loaded.")); }, [apiKey, configuration, screenId, venueId]);

  const save = async (event: FormEvent) => {
    event.preventDefault(); if (!screenId) return;
    setBusy(true); setError(undefined); setNotice(undefined);
    try {
      const value = {
        ...draft,
        title: draft.title?.trim() || undefined,
        body: draft.body?.trim() || undefined,
        mediaUrl: draft.mediaUrl?.trim() || undefined,
        startLocalTime: draft.startLocalTime ? `${time(draft.startLocalTime)}:00` : undefined,
        endLocalTime: draft.endLocalTime ? `${time(draft.endLocalTime)}:00` : undefined,
        activeDaysMask: draft.startLocalTime ? draft.activeDaysMask ?? 127 : undefined
      };
      if (editingId) await updatePlaylistSlide(configuration, apiKey, venueId, screenId, editingId, value);
      else await createPlaylistSlide(configuration, apiKey, venueId, screenId, value);
      setDraft(blank()); setEditingId(undefined); await reload();
      setNotice(editingId ? "Slide changes saved and queued for the selected screen." : "Slide added to the selected screen.");
    } catch { setError("Slide could not be saved. Check its window and content, then retry."); }
    finally { setBusy(false); }
  };
  const edit = (slide: PlaylistSlide) => {
    setEditingId(slide.id);
    setDraft({ slideType: slide.slideType, title: slide.title, body: slide.body, mediaUrl: slide.mediaUrl,
      dwellSeconds: slide.dwellSeconds, startLocalTime: time(slide.startLocalTime), endLocalTime: time(slide.endLocalTime),
      activeDaysMask: slide.activeDaysMask, isEnabled: slide.isEnabled });
    setNotice(undefined); setError(undefined);
  };
  const move = async (index: number, offset: number) => {
    const next = [...slides]; const [slide] = next.splice(index, 1); next.splice(index + offset, 0, slide);
    setBusy(true); setError(undefined);
    try { setSlides(await reorderPlaylist(configuration, apiKey, venueId, screenId, next.map(item => item.id))); setNotice("Playlist order saved."); }
    catch { setError("Playlist order could not be saved."); }
    finally { setBusy(false); }
  };
  const remove = async (slide: PlaylistSlide) => {
    if (!await review({ title: `Remove ${slide.title || slide.slideType}?`, consequence: `The slide will be removed from ${screenName ?? "the selected screen"}'s playlist. Other slides and normal screen content remain unchanged.`, confirmLabel: "Remove slide", tone: "caution" })) return;
    setBusy(true); setError(undefined);
    try { await deletePlaylistSlide(configuration, apiKey, venueId, screenId, slide.id); await reload(); setNotice("Slide removed."); }
    catch { setError("Slide could not be removed."); }
    finally { setBusy(false); }
  };
  const toggleDay = (day: number) => setDraft(value => ({ ...value, activeDaysMask: (value.activeDaysMask ?? 127) ^ (1 << day) }));
  const screenName = screens.find(screen => screen.id === screenId)?.name;

  return <article className="menu-editor playlist-admin">
    {reviewDialog}
    <div className="menu-editor-heading"><div><p>Pro scheduling</p><h3>Screen playlist</h3></div><span>{slides.length} slides</span></div>
    <p>Choose one screen before editing. Eligible enabled slides rotate in this saved order using venue-local windows.</p>
    {showUpgradePrompt && !enabled ? <aside className="tier-prompt"><div><strong>Playlist Rotation requires Pro</strong><p>Controls remain visible while editing is soft locked.</p></div></aside> : null}
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {notice ? <p className="state success" role="status">{notice}</p> : null}
    {!screens.length ? <p className="state">No screen can be selected. Add or restore a screen before configuring a playlist.</p> : <label>Screen<select value={screenId} onChange={event => { setScreenId(event.target.value); setEditingId(undefined); setDraft(blank()); }}><option value="" disabled>Select a screen</option>{screens.map(screen => <option key={screen.id} value={screen.id}>{screen.name}</option>)}</select></label>}
    {screenId && !slides.length ? <p className="state">{screenName} has no playlist slides. Its normal content remains visible.</p> : null}
    <ol>{slides.map((slide, index) => <li className={slide.isEnabled ? "" : "inactive"} key={slide.id}><strong>{slide.title || slide.slideType}</strong><span>{slide.isEnabled ? "Enabled" : "Disabled"} · {slide.dwellSeconds}s{slide.startLocalTime ? ` · ${time(slide.startLocalTime)}–${time(slide.endLocalTime)}` : " · always eligible"}</span>
      <button disabled={!enabled || busy} onClick={() => edit(slide)}>Edit</button>
      <button aria-label={`Move ${slide.title || slide.slideType} earlier`} disabled={!enabled || busy || index === 0} onClick={() => move(index, -1)}>↑</button>
      <button aria-label={`Move ${slide.title || slide.slideType} later`} disabled={!enabled || busy || index === slides.length - 1} onClick={() => move(index, 1)}>↓</button>
      <button disabled={!enabled || busy} onClick={() => remove(slide)}>Remove</button></li>)}</ol>
    <form onSubmit={save}><fieldset disabled={!enabled || !screenId || busy}>
      <legend>{editingId ? `Edit slide for ${screenName}` : `Add slide to ${screenName ?? "selected screen"}`}</legend>
      <label>Type<select value={draft.slideType} onChange={event => setDraft(value => ({ ...value, slideType: event.target.value as PlaylistSlide["slideType"] }))}><option value="menu">Menu</option><option value="image">Image</option><option value="message">Message</option></select></label>
      <label>Title<input maxLength={200} value={draft.title ?? ""} onChange={event => setDraft(value => ({ ...value, title: event.target.value }))} /></label>
      {draft.slideType === "message" ? <label>Message<textarea required maxLength={2000} value={draft.body ?? ""} onChange={event => setDraft(value => ({ ...value, body: event.target.value }))} /></label> : null}
      <label>Media URL<input type="url" value={draft.mediaUrl ?? ""} onChange={event => setDraft(value => ({ ...value, mediaUrl: event.target.value }))} /></label>
      <label>Dwell seconds<input type="number" min={5} max={120} value={draft.dwellSeconds} onChange={event => setDraft(value => ({ ...value, dwellSeconds: Number(event.target.value) }))} /></label>
      <label><input type="checkbox" checked={draft.isEnabled} onChange={event => setDraft(value => ({ ...value, isEnabled: event.target.checked }))} />Enabled</label>
      <label><input type="checkbox" checked={Boolean(draft.startLocalTime)} onChange={event => setDraft(value => ({ ...value, startLocalTime: event.target.checked ? "09:00" : undefined, endLocalTime: event.target.checked ? "17:00" : undefined, activeDaysMask: event.target.checked ? 127 : undefined }))} />Use venue-local window</label>
      {draft.startLocalTime ? <div className="schedule-window"><label>Start<input required type="time" value={time(draft.startLocalTime)} onChange={event => setDraft(value => ({ ...value, startLocalTime: event.target.value }))} /></label><label>End<input required type="time" value={time(draft.endLocalTime)} onChange={event => setDraft(value => ({ ...value, endLocalTime: event.target.value }))} /></label><fieldset><legend>Active days</legend><div>{days.map((label, day) => <label key={label}><input type="checkbox" checked={Boolean((draft.activeDaysMask ?? 0) & (1 << day))} onChange={() => toggleDay(day)} />{label}</label>)}</div></fieldset></div> : null}
      <button disabled={Boolean(draft.startLocalTime) && !draft.activeDaysMask}>{editingId ? "Save slide" : "Add slide"}</button>
      {editingId ? <button type="button" onClick={() => { setEditingId(undefined); setDraft(blank()); }}>Cancel edit</button> : null}
    </fieldset></form>
  </article>;
}

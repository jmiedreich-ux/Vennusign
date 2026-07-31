import { useEffect, useState, type FormEvent } from "react";
import { createPlaylistSlide, deletePlaylistSlide, loadPlaylist, reorderPlaylist, type PlaylistSlide } from "./api";
import type { VenueAdminConfiguration as AdminConfiguration } from "./config";

type Props = {
  configuration: AdminConfiguration; apiKey: string; venueId: string; enabled: boolean;
  screens: Array<{ id: string; name: string }>;
};

export default function PlaylistAdministration({ configuration, apiKey, venueId, enabled, screens }: Props) {
  const [screenId, setScreenId] = useState(screens[0]?.id ?? "");
  const [slides, setSlides] = useState<PlaylistSlide[]>([]);
  const [type, setType] = useState<PlaylistSlide["slideType"]>("menu");
  const [title, setTitle] = useState("");
  const [mediaUrl, setMediaUrl] = useState("");
  const [dwell, setDwell] = useState(10);
  const [useWindow, setUseWindow] = useState(false);
  const [start, setStart] = useState("09:00");
  const [end, setEnd] = useState("17:00");
  const [daysMask, setDaysMask] = useState(127);
  const [error, setError] = useState<string>();
  const reload = () => screenId && loadPlaylist(configuration, apiKey, venueId, screenId).then(setSlides).catch(() => setError("Playlist could not be loaded."));
  useEffect(() => { void reload(); }, [apiKey, configuration, screenId, venueId]);

  const add = async (event: FormEvent) => {
    event.preventDefault(); setError(undefined);
    try {
      await createPlaylistSlide(configuration, apiKey, venueId, screenId, {
        slideType: type, title: title || undefined, body: type === "message" ? title : undefined,
        mediaUrl: mediaUrl || undefined, dwellSeconds: dwell, isEnabled: true,
        startLocalTime: useWindow ? `${start}:00` : undefined,
        endLocalTime: useWindow ? `${end}:00` : undefined,
        activeDaysMask: useWindow ? daysMask : undefined
      });
      setTitle(""); setMediaUrl(""); reload();
    } catch { setError("Slide could not be saved."); }
  };
  const move = async (index: number, offset: number) => {
    const next = [...slides]; const [slide] = next.splice(index, 1); next.splice(index + offset, 0, slide);
    try { setSlides(await reorderPlaylist(configuration, apiKey, venueId, screenId, next.map(item => item.id))); }
    catch { setError("Playlist order could not be saved."); }
  };
  const remove = async (id: string) => {
    try { await deletePlaylistSlide(configuration, apiKey, venueId, screenId, id); reload(); }
    catch { setError("Slide could not be removed."); }
  };

  return <article className="menu-editor playlist-admin">
    <div className="menu-editor-heading"><div><p>Pro scheduling</p><h3>Screen playlist</h3></div><span>{slides.length} slides</span></div>
    {!enabled ? <aside className="tier-prompt"><div><strong>Playlist Rotation requires Pro</strong><p>Controls remain visible while editing is soft locked.</p></div></aside> : null}
    {error ? <p className="state error">{error}</p> : null}
    <label>Screen<select value={screenId} onChange={event => setScreenId(event.target.value)}>{screens.map(screen => <option key={screen.id} value={screen.id}>{screen.name}</option>)}</select></label>
    <ol>{slides.map((slide, index) => <li key={slide.id}><strong>{slide.title || slide.slideType}</strong><span>{slide.dwellSeconds}s</span>
      <button disabled={!enabled || index === 0} onClick={() => move(index, -1)}>↑</button>
      <button disabled={!enabled || index === slides.length - 1} onClick={() => move(index, 1)}>↓</button>
      <button disabled={!enabled} onClick={() => remove(slide.id)}>Remove</button></li>)}</ol>
    <form onSubmit={add}><fieldset disabled={!enabled || !screenId}>
      <label>Type<select value={type} onChange={event => setType(event.target.value as PlaylistSlide["slideType"])}><option value="menu">Menu</option><option value="image">Image</option><option value="message">Message</option></select></label>
      <label>Title<input maxLength={200} value={title} onChange={event => setTitle(event.target.value)} /></label>
      <label>Media URL<input type="url" value={mediaUrl} onChange={event => setMediaUrl(event.target.value)} /></label>
      <label>Dwell seconds<input type="number" min={5} max={120} value={dwell} onChange={event => setDwell(Number(event.target.value))} /></label>
      <label><input type="checkbox" checked={useWindow} onChange={event => setUseWindow(event.target.checked)} />Use venue-local window</label>
      {useWindow ? <div><label>Start<input type="time" value={start} onChange={event => setStart(event.target.value)} /></label><label>End<input type="time" value={end} onChange={event => setEnd(event.target.value)} /></label><label>Active days mask<input type="number" min={1} max={127} value={daysMask} onChange={event => setDaysMask(Number(event.target.value))} /></label></div> : null}
      <button>Add slide</button>
    </fieldset></form>
  </article>;
}

import { useEffect, useMemo, useState } from "react";
import {
  loadManagedScreens,
  loadVenueTheme,
  saveVenueTheme,
  type VenueTheme
} from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string };
type DraftTheme = Pick<VenueTheme, "backgroundColor" | "accentColor" | "fontFamily">;

const swatches: Array<{ name: string; backgroundColor: string; accentColor: string }> = [
  { name: "Ember", backgroundColor: "#111315", accentColor: "#FFB74D" },
  { name: "Ocean", backgroundColor: "#071E2B", accentColor: "#38BDF8" },
  { name: "Forest", backgroundColor: "#10271F", accentColor: "#A3E635" },
  { name: "Cafe", backgroundColor: "#F6EDDA", accentColor: "#7B241C" },
  { name: "Mono", backgroundColor: "#101010", accentColor: "#F5F5F5" },
  { name: "Plum", backgroundColor: "#271329", accentColor: "#F0ABFC" }
];

export default function ThemeBuilder({ configuration, apiKey, venueId }: Props) {
  const [theme, setTheme] = useState<DraftTheme>();
  const [screenId, setScreenId] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string>();

  useEffect(() => {
    Promise.all([
      loadVenueTheme(configuration, apiKey, venueId),
      loadManagedScreens(configuration, apiKey, venueId)
    ])
      .then(([value, screens]) => {
        setTheme(value);
        setScreenId(screens[0]?.id);
      })
      .catch(() => setMessage("Theme controls could not be loaded."));
  }, [apiKey, configuration, venueId]);

  const previewUrl = useMemo(() => {
    if (!theme || !screenId) return undefined;
    const query = new URLSearchParams({
      preview: "theme",
      background: theme.backgroundColor,
      accent: theme.accentColor,
      font: theme.fontFamily
    });
    return `${configuration.displayBaseUrl}/display/${screenId}?${query}`;
  }, [configuration.displayBaseUrl, screenId, theme]);

  const patch = (value: Partial<DraftTheme>) =>
    setTheme(current => current ? { ...current, ...value } : current);

  const save = async () => {
    if (!theme) return;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await saveVenueTheme(configuration, apiKey, venueId, theme);
      setTheme(saved);
      setMessage("Theme saved and pushed to all venue screens.");
    } catch {
      setMessage("The theme could not be saved.");
    } finally {
      setBusy(false);
    }
  };

  return <article className="theme-builder">
    <div className="theme-builder__heading">
      <div><p>All-tier styling</p><h3>Basic theme builder</h3></div>
      <button disabled={busy || !theme} onClick={save}>Save and push to all</button>
    </div>
    {message ? <p className="screen-notice" role="status">{message}</p> : null}
    {theme ? <div className="theme-builder__workspace">
      <div className="theme-builder__controls">
        <fieldset>
          <legend>Quick swatches</legend>
          <div className="theme-swatches">{swatches.map(swatch =>
            <button
              aria-label={`Use ${swatch.name} swatch`}
              key={swatch.name}
              onClick={() => patch(swatch)}
              style={{ background: swatch.backgroundColor, color: swatch.accentColor }}
              type="button"
            >{swatch.name}</button>)}</div>
        </fieldset>
        <label>Background color<input type="color" value={theme.backgroundColor} onChange={event => patch({ backgroundColor: event.target.value.toUpperCase() })} /></label>
        <label>Accent color<input type="color" value={theme.accentColor} onChange={event => patch({ accentColor: event.target.value.toUpperCase() })} /></label>
        <label>Font
          <select value={theme.fontFamily} onChange={event => patch({ fontFamily: event.target.value as DraftTheme["fontFamily"] })}>
            <option value="Inter">Inter</option>
            <option value="Georgia">Georgia</option>
            <option value="Arial">Arial</option>
          </select>
        </label>
      </div>
      <div className="theme-preview">
        <div><strong>Exact TV preview</strong><span>Draft changes update before saving.</span></div>
        {previewUrl
          ? <iframe key={previewUrl} src={previewUrl} title="Exact TV theme preview" />
          : <p>Add a venue screen to enable the player-backed preview.</p>}
      </div>
    </div> : <p>Loading theme…</p>}
  </article>;
}

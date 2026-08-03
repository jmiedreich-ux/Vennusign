import { useEffect, useMemo, useState } from "react";
import {
  applyVenueThemePreset,
  loadManagedScreens,
  loadVenueTheme,
  loadVenueThemePresets,
  saveAdvancedVenueTheme,
  saveVenueTheme,
  type VenueTheme,
  type VenueThemePreset
} from "./api";
import type { PlatformOperationsConfiguration } from "./config";

type Props = {
  configuration: PlatformOperationsConfiguration;
  apiKey: string;
  venueId: string;
  advancedEnabled: boolean;
};
type DraftTheme = VenueTheme;

const swatches: Array<{ name: string; backgroundColor: string; accentColor: string }> = [
  { name: "Ember", backgroundColor: "#111315", accentColor: "#FFB74D" },
  { name: "Ocean", backgroundColor: "#071E2B", accentColor: "#38BDF8" },
  { name: "Forest", backgroundColor: "#10271F", accentColor: "#A3E635" },
  { name: "Cafe", backgroundColor: "#F6EDDA", accentColor: "#7B241C" },
  { name: "Mono", backgroundColor: "#101010", accentColor: "#F5F5F5" },
  { name: "Plum", backgroundColor: "#271329", accentColor: "#F0ABFC" }
];
const titleFonts: DraftTheme["titleFont"][] = ["Pacifico", "Lobster", "Righteous", "Fredoka One", "Bungee", "Permanent Marker"];
const itemFonts: DraftTheme["itemFont"][] = ["Caveat", "Kalam", "Patrick Hand", "Permanent Marker"];

export default function ThemeBuilder({ configuration, apiKey, venueId, advancedEnabled }: Props) {
  const [theme, setTheme] = useState<DraftTheme>();
  const [presets, setPresets] = useState<VenueThemePreset[]>([]);
  const [screenId, setScreenId] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string>();

  useEffect(() => {
    Promise.all([
      loadVenueTheme(configuration, apiKey, venueId),
      loadManagedScreens(configuration, apiKey, venueId),
      loadVenueThemePresets(configuration, apiKey, venueId)
    ])
      .then(([value, screens, availablePresets]) => {
        setTheme(value);
        setScreenId(screens[0]?.id);
        setPresets(availablePresets);
      })
      .catch(() => setMessage("Theme controls could not be loaded."));
  }, [apiKey, configuration, venueId]);

  const previewUrl = useMemo(() => {
    if (!theme || !screenId) return undefined;
    const query = new URLSearchParams({
      preview: "theme",
      background: theme.backgroundColor,
      accent: theme.accentColor,
      font: theme.fontFamily,
      preset: theme.presetKey,
      title: theme.titleColor,
      glow: theme.glowColor,
      board: theme.boardBackgroundColor,
      sections: theme.sectionColors.join(","),
      intensity: String(theme.glowIntensity),
      titleFont: theme.titleFont,
      itemFont: theme.itemFont
    });
    return `${configuration.displayBaseUrl}/display/${screenId}?${query}`;
  }, [configuration.displayBaseUrl, screenId, theme]);

  const patchBasic = (value: Partial<DraftTheme>) =>
    setTheme(current => current ? { ...current, ...value } : current);
  const patchAdvanced = (value: Partial<DraftTheme>) =>
    setTheme(current => current ? { ...current, presetKey: "custom", ...value } : current);

  const saveBasic = async () => {
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

  const saveAdvanced = async () => {
    if (!theme || !advancedEnabled) return;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await saveAdvancedVenueTheme(configuration, apiKey, venueId, theme);
      setTheme(saved);
      setMessage("Advanced theme saved and pushed to all venue screens.");
    } catch {
      setMessage("The advanced theme could not be saved.");
    } finally {
      setBusy(false);
    }
  };

  const applyPreset = async (preset: VenueThemePreset) => {
    if (!advancedEnabled) return;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await applyVenueThemePreset(configuration, apiKey, venueId, preset.key);
      setTheme(saved);
      setMessage(`${preset.label} applied and pushed to all venue screens.`);
    } catch {
      setMessage("The preset could not be applied.");
    } finally {
      setBusy(false);
    }
  };

  const patchSectionColor = (index: number, color: string) =>
    patchAdvanced({ sectionColors: theme?.sectionColors.map((value, position) => position === index ? color : value) });
  const addSectionColor = () =>
    patchAdvanced({ sectionColors: [...(theme?.sectionColors ?? []), theme?.glowColor ?? "#00E5FF"] });
  const removeSectionColor = (index: number) =>
    patchAdvanced({ sectionColors: theme?.sectionColors.filter((_, position) => position !== index) });

  return <article className="theme-builder">
    <div className="theme-builder__heading">
      <div><p>All-tier styling</p><h3>Theme builder</h3></div>
      <button disabled={busy || !theme} onClick={saveBasic}>Save basic theme</button>
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
              onClick={() => patchBasic(swatch)}
              style={{ background: swatch.backgroundColor, color: swatch.accentColor }}
              type="button"
            >{swatch.name}</button>)}</div>
        </fieldset>
        <label>Background color<input type="color" value={theme.backgroundColor} onChange={event => patchBasic({ backgroundColor: event.target.value.toUpperCase() })} /></label>
        <label>Accent color<input type="color" value={theme.accentColor} onChange={event => patchBasic({ accentColor: event.target.value.toUpperCase() })} /></label>
        <label>Font
          <select value={theme.fontFamily} onChange={event => patchBasic({ fontFamily: event.target.value as DraftTheme["fontFamily"] })}>
            <option value="Inter">Inter</option>
            <option value="Georgia">Georgia</option>
            <option value="Arial">Arial</option>
          </select>
        </label>
        <section className="advanced-theme">
          <div><p>Pro styling</p><h4>Full theme controls</h4></div>
          {!advancedEnabled ? <aside className="tier-prompt" role="status"><div><strong>Full themes require All Layouts</strong><p>The controls remain visible for evaluation. Upgrade to Pro or add a venue override to save presets and advanced values.</p></div></aside> : null}
          <fieldset disabled={!advancedEnabled || busy}>
            <legend>Presets</legend>
            <div className="theme-presets">{presets.map(preset =>
              <button
                aria-pressed={theme.presetKey === preset.key}
                key={preset.key}
                onClick={() => void applyPreset(preset)}
                style={{ background: preset.boardBackgroundColor, color: preset.glowColor }}
                type="button"
              >{preset.label}</button>)}</div>
          </fieldset>
          <fieldset disabled={!advancedEnabled || busy}>
            <legend>Neon palette</legend>
            <label>Title color<input type="color" value={theme.titleColor} onChange={event => patchAdvanced({ titleColor: event.target.value.toUpperCase() })} /></label>
            <label>Glow color<input type="color" value={theme.glowColor} onChange={event => patchAdvanced({ glowColor: event.target.value.toUpperCase() })} /></label>
            <label>Board background<input type="color" value={theme.boardBackgroundColor} onChange={event => patchAdvanced({ boardBackgroundColor: event.target.value.toUpperCase() })} /></label>
          </fieldset>
          <fieldset disabled={!advancedEnabled || busy}>
            <legend>Section colors</legend>
            <div className="section-colors">{theme.sectionColors.map((color, index) =>
              <label key={`${index}-${color}`}>Section {index + 1}
                <span><input type="color" value={color} onChange={event => patchSectionColor(index, event.target.value.toUpperCase())} />
                <button disabled={theme.sectionColors.length === 1} onClick={() => removeSectionColor(index)} type="button">Remove</button></span>
              </label>)}</div>
            <button disabled={theme.sectionColors.length === 4} onClick={addSectionColor} type="button">Add section color</button>
          </fieldset>
          <fieldset disabled={!advancedEnabled || busy}>
            <legend>Type and glow</legend>
            <label>Glow intensity <output>{theme.glowIntensity.toFixed(2)}</output>
              <input min="0.2" max="2" step="0.05" type="range" value={theme.glowIntensity} onChange={event => patchAdvanced({ glowIntensity: Number(event.target.value) })} />
            </label>
            <label>Title font<select value={theme.titleFont} onChange={event => patchAdvanced({ titleFont: event.target.value as DraftTheme["titleFont"] })}>{titleFonts.map(font => <option key={font}>{font}</option>)}</select></label>
            <label>Item font<select value={theme.itemFont} onChange={event => patchAdvanced({ itemFont: event.target.value as DraftTheme["itemFont"] })}>{itemFonts.map(font => <option key={font}>{font}</option>)}</select></label>
          </fieldset>
          <button disabled={!advancedEnabled || busy} onClick={saveAdvanced}>Save full theme</button>
        </section>
      </div>
      <div className="theme-preview">
        <div><strong>Exact TV preview</strong><span>Basic and advanced draft changes update before saving.</span></div>
        {previewUrl
          ? <iframe key={previewUrl} src={previewUrl} title="Exact TV theme preview" />
          : <p>Add a venue screen to enable the player-backed preview.</p>}
      </div>
    </div> : <p>Loading theme…</p>}
  </article>;
}

import { useEffect, useMemo, useState } from "react";
import {
  applyVenueThemePreset,
  loadManagedScreens,
  loadVenueTheme,
  loadVenueThemePresets,
  resetVenueTheme,
  saveAdvancedVenueTheme,
  saveVenueTheme,
  type VenueTheme,
  type VenueThemePreset,
  type ManagedScreen
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import { useDestructiveReview } from "./DestructiveReviewDialog";
import VennusignLoader from "./VennusignLoader";

type Props = {
  configuration: BackOfficeConfiguration;
  apiKey: string;
  venueId: string;
  advancedEnabled: boolean;
  showUpgradePrompt?: boolean;
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

function contrastRatio(first: string, second: string) {
  const luminance = (color: string) => {
    const channels = color.slice(1).match(/.{2}/g)?.map(value => parseInt(value, 16) / 255) ?? [0, 0, 0];
    const [red, green, blue] = channels.map(value => value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4);
    return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
  };
  const [lighter, darker] = [luminance(first), luminance(second)].sort((left, right) => right - left);
  return (lighter + 0.05) / (darker + 0.05);
}

export default function ThemeBuilder({ configuration, apiKey, venueId, advancedEnabled, showUpgradePrompt = true }: Props) {
  const [theme, setTheme] = useState<DraftTheme>();
  const [presets, setPresets] = useState<VenueThemePreset[]>([]);
  const [screenId, setScreenId] = useState<string>();
  const [screens, setScreens] = useState<ManagedScreen[]>([]);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string>();
  const [loading, setLoading] = useState(true);
  const [loadFailed, setLoadFailed] = useState(false);
  const [undoTheme, setUndoTheme] = useState<DraftTheme>();
  const { review, reviewDialog } = useDestructiveReview();

  const load = async () => {
    setLoading(true); setLoadFailed(false); setMessage(undefined);
    try {
      const [value, loadedScreens, availablePresets] = await Promise.all([
      loadVenueTheme(configuration, apiKey, venueId),
      loadManagedScreens(configuration, apiKey, venueId),
      loadVenueThemePresets(configuration, apiKey, venueId)
      ]);
      setTheme(value);
      const activeScreens = loadedScreens.filter(screen => screen.status.toLowerCase() !== "archived");
      setScreens(activeScreens); setScreenId(activeScreens[0]?.id); setPresets(availablePresets);
    } catch {
      setTheme(undefined); setLoadFailed(true); setMessage("Theme controls could not be loaded.");
    } finally { setLoading(false); }
  };
  useEffect(() => {
    void load();
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
    const previous = theme;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await saveVenueTheme(configuration, apiKey, venueId, theme);
      setTheme(saved);
      setUndoTheme(previous);
      setMessage("Theme saved and pushed to all venue screens.");
    } catch {
      setMessage("The theme could not be saved.");
    } finally {
      setBusy(false);
    }
  };

  const saveAdvanced = async () => {
    if (!theme || !advancedEnabled) return;
    const previous = theme;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await saveAdvancedVenueTheme(configuration, apiKey, venueId, theme);
      setTheme(saved);
      setUndoTheme(previous);
      setMessage("Advanced theme saved and pushed to all venue screens.");
    } catch {
      setMessage("The advanced theme could not be saved.");
    } finally {
      setBusy(false);
    }
  };

  const applyPreset = async (preset: VenueThemePreset) => {
    if (!advancedEnabled) return;
    const previous = theme;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await applyVenueThemePreset(configuration, apiKey, venueId, preset.key);
      setTheme(saved);
      setUndoTheme(previous);
      setMessage(`${preset.label} applied and pushed to all venue screens.`);
    } catch {
      setMessage("The preset could not be applied.");
    } finally {
      setBusy(false);
    }
  };

  const resetTheme = async () => {
    if (!theme) return;
    const previous = theme;
    if (!await review({ title: "Reset the venue-wide theme?", consequence: "All custom theme values will be replaced with Vennusign defaults and the reset will be queued for every active screen.", confirmLabel: "Reset venue theme" })) return;
    setBusy(true); setMessage(undefined);
    try {
      const saved = await resetVenueTheme(configuration, apiKey, venueId);
      setTheme(saved);
      setUndoTheme(previous);
      setMessage("Venue-wide theme reset and queued for all active screens.");
    } catch {
      setMessage("The theme could not be reset.");
    } finally {
      setBusy(false);
    }
  };

  const undoAppliedTheme = async () => {
    if (!undoTheme) return;
    setBusy(true); setMessage(undefined);
    try {
      const restored = advancedEnabled
        ? await saveAdvancedVenueTheme(configuration, apiKey, venueId, undoTheme)
        : await saveVenueTheme(configuration, apiKey, venueId, undoTheme);
      setTheme(restored); setUndoTheme(undefined); setMessage("Previous venue theme restored and queued for active screens.");
    } catch { setMessage("The previous theme could not be restored. The applied theme remains active."); }
    finally { setBusy(false); }
  };

  const patchSectionColor = (index: number, color: string) =>
    patchAdvanced({ sectionColors: theme?.sectionColors.map((value, position) => position === index ? color : value) });
  const addSectionColor = () =>
    patchAdvanced({ sectionColors: [...(theme?.sectionColors ?? []), theme?.glowColor ?? "#00E5FF"] });
  const removeSectionColor = (index: number) =>
    patchAdvanced({ sectionColors: theme?.sectionColors.filter((_, position) => position !== index) });
  const basicContrast = theme ? contrastRatio(theme.accentColor, theme.backgroundColor) : 0;
  const titleContrast = theme ? contrastRatio(theme.titleColor, theme.boardBackgroundColor) : 0;

  return <article className="theme-builder">
    {reviewDialog}
    <div className="theme-builder__heading">
      <div><p>All-tier styling</p><h3>Theme builder</h3></div>
      <div className="sticky-action-bar" aria-label="Theme actions"><button className="action-primary" disabled={busy || !theme} onClick={saveBasic}>Save basic theme</button><details className="action-overflow"><summary>More actions</summary><div><button className="action-danger" disabled={busy || !theme} onClick={resetTheme}>Reset theme</button></div></details></div>
    </div>
    {message ? <p className="screen-notice" role="status">{message}</p> : null}
    {undoTheme ? <div className="applied-state-undo" role="status"><span>Theme change applied to venue screens.</span><button type="button" disabled={busy} onClick={() => void undoAppliedTheme()}>Undo applied theme</button></div> : null}
    {theme ? <div className="theme-builder__workspace">
      <div className="theme-builder__controls">
        <p className="screen-notice" role="status"><strong>Venue-wide scope:</strong> saved changes apply to every active screen. The screen selector changes only the preview target.</p>
        <label>Preview screen<select value={screenId ?? ""} onChange={event => setScreenId(event.target.value)} disabled={screens.length === 0}><option value="" disabled>Select a screen</option>{screens.map(screen => <option key={screen.id} value={screen.id}>{screen.name}{screen.location ? ` · ${screen.location}` : ""}</option>)}</select></label>
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
        <p className={basicContrast >= 4.5 ? "screen-notice" : "state error"} role="status">Basic text contrast: {basicContrast.toFixed(2)}:1 {basicContrast >= 4.5 ? "· readable" : "· increase contrast to at least 4.5:1"}</p>
        <section className="advanced-theme">
          <div><p>Pro styling</p><h4>Full theme controls</h4></div>
          {showUpgradePrompt && !advancedEnabled ? <aside className="tier-prompt" role="status"><div><strong>Full themes require All Layouts</strong><p>The controls remain visible for evaluation. Upgrade to Pro or add a venue override to save presets and advanced values.</p></div></aside> : null}
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
          <button className="action-secondary" disabled={!advancedEnabled || busy} onClick={saveAdvanced}>Save full theme</button>
          <p className={titleContrast >= 4.5 ? "screen-notice" : "state error"} role="status">Title-to-board contrast: {titleContrast.toFixed(2)}:1 {titleContrast >= 4.5 ? "· readable" : "· increase contrast to at least 4.5:1"}</p>
        </section>
      </div>
      <div className="theme-preview">
        <div><strong>Exact TV preview</strong><span>Basic and advanced draft changes update before saving.</span></div>
        {previewUrl
          ? <iframe key={previewUrl} src={previewUrl} title="Exact TV theme preview" />
          : <p>Add a venue screen to enable the player-backed preview.</p>}
      </div>
    </div> : loadFailed ? <div className="state error" role="alert"><p>Theme state is unavailable.</p><button type="button" disabled={loading} onClick={() => void load()}>Retry theme controls</button></div> : <VennusignLoader message="Loading theme…" />}
  </article>;
}

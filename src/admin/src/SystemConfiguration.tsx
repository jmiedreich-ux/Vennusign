import { useEffect, useState, type FormEvent } from "react";
import {
  AdminApiError,
  applySystemConfigurationImport,
  clearSystemConfiguration,
  exportSystemConfiguration,
  loadSystemConfiguration,
  previewSystemConfigurationImport,
  saveSystemConfiguration,
  type SystemConfigurationImportPreview,
  type SystemConfigurationManifest,
  type SystemConfigurationSetting
} from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string };
const environments = ["Development", "Test", "Staging", "Production"];
const scopes = ["", "Shared", "API", "Admin", "VenueAdmin", "Display", "Background"];

export default function SystemConfiguration({ configuration, apiKey }: Props) {
  const [environmentName, setEnvironmentName] = useState("Development");
  const [scope, setScope] = useState("");
  const [settings, setSettings] = useState<SystemConfigurationSetting[]>([]);
  const [drafts, setDrafts] = useState<Record<string, string>>({});
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [preview, setPreview] = useState<SystemConfigurationImportPreview>();
  const [selectedImport, setSelectedImport] = useState<string[]>([]);

  const refresh = async () => {
    setBusy(true); setError(undefined);
    try { setSettings(await loadSystemConfiguration(configuration, apiKey, environmentName, scope || undefined)); }
    catch { setError("Configuration could not be loaded."); }
    finally { setBusy(false); }
  };

  const exportSettings = async () => {
    const manifest = await exportSystemConfiguration(configuration, apiKey, environmentName);
    const url = URL.createObjectURL(new Blob([JSON.stringify(manifest, null, 2)], { type: "application/json" }));
    const link = document.createElement("a"); link.href = url; link.download = `vennu-${environmentName.toLowerCase()}-configuration.json`; link.click(); URL.revokeObjectURL(url);
  };
  const selectImport = async (file?: File) => {
    if (!file) return;
    setError(undefined); setNotice(undefined);
    try {
      const manifest = JSON.parse(await file.text()) as SystemConfigurationManifest;
      const result = await previewSystemConfigurationImport(configuration, apiKey, environmentName, manifest);
      setPreview(result);
      setSelectedImport(result.settings.filter(item => item.status === "New").map(item => `${item.applicationScope}:${item.key}`));
    } catch { setError("The configuration file could not be previewed."); }
  };
  const applyImport = async () => {
    if (!preview || selectedImport.length === 0 || !window.confirm(`Apply ${selectedImport.length} configuration change(s) to ${environmentName}?`)) return;
    setBusy(true); setError(undefined);
    try { await applySystemConfigurationImport(configuration, apiKey, preview, selectedImport); setPreview(undefined); setSelectedImport([]); setNotice("Configuration import applied transactionally."); await refresh(); }
    catch (reason) { setError(reason instanceof AdminApiError ? reason.message : "The configuration import failed without applying changes."); }
    finally { setBusy(false); }
  };
  useEffect(() => { void refresh(); }, [apiKey, configuration, environmentName, scope]);

  const save = (setting: SystemConfigurationSetting) => async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault(); setBusy(true); setError(undefined); setNotice(undefined);
    try {
      const value = drafts[setting.definitionId] ?? setting.value ?? "";
      await saveSystemConfiguration(configuration, apiKey, setting, environmentName, value);
      setDrafts(current => ({ ...current, [setting.definitionId]: "" }));
      setNotice(`${setting.key} saved.${setting.requiresRestart ? " Restart the affected application to apply it." : ""}`);
      await refresh();
    } catch (reason) {
      setError(reason instanceof AdminApiError ? reason.message : "The setting could not be saved.");
    } finally { setBusy(false); }
  };

  const clear = async (setting: SystemConfigurationSetting) => {
    if (!window.confirm(`Clear ${setting.key} for ${environmentName}?`)) return;
    setBusy(true); setError(undefined); setNotice(undefined);
    try {
      await clearSystemConfiguration(configuration, apiKey, setting, environmentName);
      setNotice(`${setting.key} cleared.${setting.requiresRestart ? " Restart the affected application to apply it." : ""}`);
      await refresh();
    } catch (reason) {
      setError(reason instanceof AdminApiError ? reason.message : "The setting could not be cleared.");
    } finally { setBusy(false); }
  };

  return <section className="system-configuration" aria-labelledby="configuration-heading">
    <div className="configuration-heading">
      <div><p>Environment-owned settings</p><h2 id="configuration-heading">Application configuration</h2><p>Secrets are write-only and never displayed after storage.</p></div>
      <div className="configuration-filters">
        <label>Environment<select value={environmentName} onChange={event => setEnvironmentName(event.target.value)}>{environments.map(value => <option key={value}>{value}</option>)}</select></label>
        <label>Application<select value={scope} onChange={event => setScope(event.target.value)}>{scopes.map(value => <option key={value} value={value}>{value || "All"}</option>)}</select></label>
        <button type="button" onClick={() => void exportSettings()}>Export</button>
        <label className="configuration-import">Import JSON<input type="file" accept="application/json" onChange={event => void selectImport(event.target.files?.[0])} /></label>
      </div>
    </div>
    {notice ? <p className="state" role="status">{notice}</p> : null}
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {busy && settings.length === 0 ? <p className="state" role="status">Loading configuration…</p> : null}
    {!busy && settings.length === 0 ? <p className="state">No registered settings match these filters.</p> : null}
    {preview ? <section className="configuration-preview" aria-labelledby="import-preview-heading"><h3 id="import-preview-heading">Import preview</h3><p>Secrets are excluded. Select reviewed changes to apply atomically.</p>
      {preview.settings.map(item => { const id = `${item.applicationScope}:${item.key}`; const selectable = item.status === "New" || item.status === "Conflict"; return <label key={id}><input type="checkbox" disabled={!selectable} checked={selectedImport.includes(id)} onChange={event => setSelectedImport(current => event.target.checked ? [...current, id] : current.filter(value => value !== id))} /><span><strong>{item.status}</strong> {id}{item.message ? ` — ${item.message}` : ""}</span></label>; })}
      <div className="configuration-actions"><button type="button" disabled={selectedImport.length === 0 || busy} onClick={() => void applyImport()}>Apply selected changes</button><button type="button" onClick={() => setPreview(undefined)}>Cancel</button></div>
    </section> : null}
    <div className="configuration-list">{settings.map(setting => <form key={setting.definitionId} className="configuration-card" onSubmit={save(setting)}>
      <div className="configuration-card__description">
        <span>{setting.applicationScope} · {setting.valueType}{setting.requiresRestart ? " · Restart required" : ""}</span>
        <h3>{setting.key}</h3><p>{setting.description}</p>
        {setting.isSecret ? <strong>{setting.hasConfiguredValue ? "Secret configured" : "Secret not configured"}</strong> : null}
      </div>
      <label>{setting.isSecret ? "Replacement secret" : "Value"}
        <input type={setting.isSecret ? "password" : setting.valueType === "Integer" || setting.valueType === "Decimal" ? "number" : "text"}
          required={setting.isRequired || setting.isSecret}
          value={drafts[setting.definitionId] ?? (setting.isSecret ? "" : setting.value ?? "")}
          onChange={event => setDrafts(current => ({ ...current, [setting.definitionId]: event.target.value }))}
          autoComplete={setting.isSecret ? "new-password" : "off"} />
      </label>
      <div className="configuration-actions"><button disabled={busy} type="submit">{setting.isSecret ? "Replace secret" : "Save"}</button>
        {setting.hasConfiguredValue && !setting.isRequired ? <button disabled={busy} type="button" onClick={() => void clear(setting)}>Clear</button> : null}</div>
    </form>)}</div>
  </section>;
}

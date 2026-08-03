import { useEffect, useState } from "react";
import {
  beginPosConnection,
  disconnectPosProvider,
  importPosCatalog,
  loadPosProviderStatus,
  type PosProvider,
  type PosProviderStatus
} from "./api";
import type { BackOfficeConfiguration } from "./config";

type Props = { configuration: BackOfficeConfiguration; accessToken: string };
const providers: PosProvider[] = ["square", "toast", "clover"];

export default function PosIntegrationAdministration({ configuration, accessToken }: Props) {
  const [rows, setRows] = useState<PosProviderStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();

  const refresh = async () => {
    setLoading(true); setError(undefined);
    try { setRows(await Promise.all(providers.map(provider => loadPosProviderStatus(configuration, accessToken, provider)))); }
    catch { setError("POS connection status could not be loaded. Retry without changing provider configuration."); }
    finally { setLoading(false); }
  };
  useEffect(() => { void refresh(); }, [accessToken, configuration]);

  const connect = async (provider: "square" | "clover") => {
    setBusy(provider); setError(undefined); setNotice("Opening the provider authorization page…");
    try { window.location.assign(await beginPosConnection(configuration, accessToken, provider)); }
    catch { setError(`${provider} authorization could not be opened.`); setBusy(undefined); }
  };
  const importCatalog = async (provider: PosProvider) => {
    setBusy(provider); setError(undefined); setNotice(undefined);
    try { const result = await importPosCatalog(configuration, accessToken, provider); setNotice(`${provider} catalog imported: ${result.sectionsCreated} sections created, ${result.itemsCreated} items created, and ${result.itemsUpdated} items updated.`); await refresh(); }
    catch { setError(`${provider} catalog import failed. The existing menu was not reported as successfully replaced.`); }
    finally { setBusy(undefined); }
  };
  const disconnect = async (provider: "square" | "clover") => {
    setBusy(provider); setError(undefined); setNotice(undefined);
    try { await disconnectPosProvider(configuration, accessToken, provider); setNotice(`${provider} disconnected.`); await refresh(); }
    catch { setError(`${provider} could not be disconnected.`); }
    finally { setBusy(undefined); }
  };

  return <section className="pos-administration">
    <div className="page-toolbar"><div><p>Authorized integration workspace</p><h2>POS connections</h2><span>Connection state and catalog actions stay scoped to this venue.</span></div><button type="button" disabled={loading} onClick={refresh}>{loading ? "Refreshing…" : "Refresh status"}</button></div>
    {error ? <p className="state error" role="alert">{error}</p> : null}{notice ? <p className="state" role="status">{notice}</p> : null}
    {loading && rows.length === 0 ? <p className="state">Loading POS providers…</p> : <div className="pos-provider-grid">{rows.map(row => <article key={row.provider}>
      <div><p>{row.provider}</p><h3>{row.connection ? row.connection.status.replaceAll("_", " ") : "Not connected"}</h3><span>{row.connection?.externalMerchantId ? `Merchant ${row.connection.externalMerchantId}` : "No venue connection is stored."}</span></div>
      {row.guidance ? <p className="provider-guidance">{row.guidance}</p> : null}
      <div className="provider-actions">
        {!row.connection && row.provider !== "toast" ? <button disabled={!!busy} onClick={() => connect(row.provider as "square" | "clover")}>Connect {row.provider}</button> : null}
        {row.connection ? <button disabled={!!busy} onClick={() => importCatalog(row.provider)}>Import catalog</button> : null}
        {row.connection && row.provider !== "toast" ? <button className="danger" disabled={!!busy} onClick={() => disconnect(row.provider as "square" | "clover")}>Disconnect</button> : null}
      </div>
    </article>)}</div>}
  </section>;
}

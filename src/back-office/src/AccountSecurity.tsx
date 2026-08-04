import { useEffect, useState, type FormEvent } from "react";
import type { BackOfficeConfiguration } from "./config";
import { listPasskeys, registerPasskey, removePasskey, renamePasskey, type PasskeySummary } from "./passkeyManagement";
import { passkeyInventoryView } from "./actionRecovery.mjs";
import { useDestructiveReview } from "./DestructiveReviewDialog";

export default function AccountSecurity({ configuration, customerSession }: { configuration: BackOfficeConfiguration; customerSession: boolean }) {
  const [passkeys, setPasskeys] = useState<PasskeySummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [error, setError] = useState<string>();
  const { review, reviewDialog } = useDestructiveReview();

  const [inventoryFailed, setInventoryFailed] = useState(false);
  const refresh = async () => {
    setLoading(true); setInventoryFailed(false); setError(undefined);
    try { setPasskeys(await listPasskeys(configuration)); }
    catch (reason) { setInventoryFailed(true); setError(reason instanceof Error ? reason.message : "Passkeys could not be loaded."); throw reason; }
    finally { setLoading(false); }
  };
  useEffect(() => {
    if (!customerSession) { setLoading(false); return; }
    void refresh().catch(() => undefined);
  }, [configuration, customerSession]);

  const run = async (key: string, action: () => Promise<void>, success: string) => {
    setBusy(key); setError(undefined); setNotice(undefined);
    try { await action(); await refresh(); setNotice(success); }
    catch (reason) { setError(reason instanceof Error ? reason.message : "Vennusign could not update passkey security."); }
    finally { setBusy(undefined); }
  };

  const add = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault(); const form = event.currentTarget;
    const name = String(new FormData(form).get("passkeyName") ?? "").trim();
    void run("add", () => registerPasskey(configuration, name), "Passkey added. You can use it the next time you sign in.").then(() => form.reset());
  };
  const remove = async (passkey: PasskeySummary) => {
    if (!await review({ title: `Remove ${passkey.displayName}?`, consequence: "This passkey will stop working immediately and cannot be restored. Keep another verified sign-in method available.", confirmLabel: "Remove passkey" })) return;
    await run(`remove-${passkey.id}`, () => removePasskey(configuration, passkey.id), "Passkey removed.");
  };

  if (!customerSession) return <section className="account-security" aria-labelledby="account-security-heading"><p>Account</p><h2 id="account-security-heading">Security requires a customer session</h2><p>Sign out of the temporary legacy venue link, then sign in with your customer account to manage passkeys.</p></section>;
  const inventoryView = passkeyInventoryView({ loading, failed: inventoryFailed, count: passkeys.length });
  return <section className="account-security" aria-labelledby="account-security-heading">
    {reviewDialog}
    <p>Account</p><h2 id="account-security-heading">Passkeys and recovery</h2>
    <p>Passkeys use your device screen lock. Vennusign stores only the public verification credential and safe device metadata—not your private key.</p>
    {notice ? <p role="status" aria-live="polite" className="account-security__notice">{notice}</p> : null}
    {error ? <p role="alert" className="account-security__error">{error}</p> : null}
    <form className="account-security__add" onSubmit={add}>
      <label htmlFor="passkey-name">Passkey name</label>
      <input id="passkey-name" name="passkeyName" maxLength={100} placeholder="Work laptop" required />
      <button type="submit" disabled={Boolean(busy) || inventoryView !== "loaded" && inventoryView !== "empty"}>{busy === "add" ? "Waiting for your device…" : "Add a passkey"}</button>
      <small>Recent sign-in is required. If prompted to recover, sign in again by email, Google, or Apple and return here.</small>
    </form>
    <h3>Your passkeys</h3>
    {inventoryView === "loading" ? <p role="status">Loading passkeys…</p>
      : inventoryView === "failed" ? <div className="account-security__error" role="alert"><p>Passkey inventory is unknown. No empty-state assumption has been made.</p><button type="button" onClick={() => void refresh().catch(() => undefined)}>Retry passkey inventory</button></div>
      : inventoryView === "empty" ? <p>No passkeys yet. Keep another verified sign-in method available before relying on a new passkey.</p> : <ul className="account-security__list">{passkeys.map(passkey => <li key={passkey.id}>
      <form onSubmit={event => { event.preventDefault(); const name = String(new FormData(event.currentTarget).get("displayName") ?? "").trim(); void run(`rename-${passkey.id}`, () => renamePasskey(configuration, passkey.id, name), "Passkey name updated."); }}>
        <label htmlFor={`passkey-${passkey.id}`}>Passkey name</label><input id={`passkey-${passkey.id}`} name="displayName" defaultValue={passkey.displayName} maxLength={100} required />
        <span>Added {new Date(passkey.createdUtc).toLocaleDateString()}{passkey.lastUsedUtc ? ` · Last used ${new Date(passkey.lastUsedUtc).toLocaleDateString()}` : " · Not used yet"}</span>
        <div><button type="submit" disabled={Boolean(busy)}>Save name</button><button className="danger" type="button" disabled={Boolean(busy)} onClick={() => void remove(passkey)}>Remove passkey</button></div>
      </form>
    </li>)}</ul>}
    <aside><strong>Recovery</strong><p>Removing your last passkey is blocked unless your verified email recovery remains available. TOTP and recovery codes keep their existing separate enrollment and step-up boundaries.</p></aside>
  </section>;
}

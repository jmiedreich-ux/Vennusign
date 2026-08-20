/**
 * Disposable Zoho mailboxes for end-to-end signup testing.
 *
 * Entra will not send verification codes to disposable-email services (mailinator
 * and a private testinator domain both received nothing), so exercising a real
 * signup needs a real mailbox on a real domain. This provisions one on
 * vennusign.com, reads the code out of it, and removes it again.
 *
 * The organization has very few mailbox allocations and a leaked mailbox stays
 * until someone deletes it by hand, so every entry point here either cleans up in
 * a `finally` or is explicitly a sweep. Prefer `withDisposableMailbox`.
 *
 * Credentials live outside the repo in ~/.config/vennusign-zoho.json (or
 * $VENNUSIGN_ZOHO_CONFIG). The refresh token does not expire; access tokens are
 * minted per run and never persisted.
 */
import { readFileSync } from "node:fs";
import { homedir } from "node:os";
import { join } from "node:path";

const API = "https://mail.zoho.com/api";
const CONFIG_PATH = process.env.VENNUSIGN_ZOHO_CONFIG ?? join(homedir(), ".config", "vennusign-zoho.json");

/** Mailboxes we create are always prefixed so a sweep can recognise its own litter. */
export const QA_PREFIX = "qa-murphy-";

function config() {
  try {
    return JSON.parse(readFileSync(CONFIG_PATH, "utf8"));
  } catch (cause) {
    throw new Error(
      `Zoho credentials not found at ${CONFIG_PATH}. This file is machine-local and is not in the repo; ` +
      `on a fresh machine or in CI, supply client_id/client_secret/refresh_token/dc/zoid.`,
      { cause }
    );
  }
}

let cachedToken = null;

async function accessToken() {
  // Access tokens last an hour; a single test run reuses one rather than
  // re-minting per request.
  if (cachedToken && cachedToken.expiresAt > Date.now() + 60_000) return cachedToken.value;
  const cfg = config();
  const res = await fetch(`https://${cfg.dc}/oauth/v2/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "refresh_token",
      client_id: cfg.client_id,
      client_secret: cfg.client_secret,
      refresh_token: cfg.refresh_token
    })
  });
  const body = await res.json();
  if (!body.access_token) throw new Error(`Zoho token refresh failed: ${JSON.stringify(body)}`);
  cachedToken = { value: body.access_token, expiresAt: Date.now() + (body.expires_in ?? 3600) * 1000 };
  return cachedToken.value;
}

async function call(method, path, { body, query } = {}) {
  const token = await accessToken();
  const url = `${API}${path}${query ? `?${new URLSearchParams(query)}` : ""}`;
  const res = await fetch(url, {
    method,
    headers: {
      // Zoho requires this prefix rather than the usual Bearer.
      Authorization: `Zoho-oauthtoken ${token}`,
      Accept: "application/json",
      "Content-Type": "application/json"
    },
    body: body === undefined ? undefined : JSON.stringify(body)
  });
  const text = await res.text();
  let parsed;
  try { parsed = JSON.parse(text); } catch { parsed = { raw: text }; }
  return { status: res.status, body: parsed };
}

export async function listMailboxes() {
  const { body } = await call("GET", "/organization/accounts");
  return (body.data ?? []).map(a => ({
    email: a.primaryEmailAddress ?? a.emailAddress,
    accountId: String(a.accountId),
    zuid: String(a.zuid)
  }));
}

/**
 * Creates a mailbox and returns everything needed to read from and delete it.
 * The caller owns cleanup - use `withDisposableMailbox` unless you have a reason not to.
 */
export async function createMailbox(label = String(Date.now())) {
  const email = `${QA_PREFIX}${label}@vennusign.com`.toLowerCase();
  const password = `Vn$${Math.random().toString(36).slice(2, 10)}Qa9!`;
  const { status, body } = await call("POST", "/organization/accounts", {
    body: {
      primaryEmailAddress: email,
      password,
      displayName: `QA Murphy ${label}`,
      firstName: "QA",
      lastName: "Murphy",
      country: "us"
    }
  });
  const data = body.data ?? {};
  if (!data.accountId) throw new Error(`Zoho mailbox create failed (${status}): ${JSON.stringify(body).slice(0, 300)}`);
  return { email, password, accountId: String(data.accountId), zuid: String(data.zuid) };
}

/**
 * Deleting needs three things right at once, none of them obvious:
 * zoid in the path, zuid as a *query* parameter (a request body is silently
 * ignored and reports "zuid Less than minimum occurence"), and the
 * ZohoMail.organization.ALL scope.
 */
export async function deleteMailbox({ accountId, zuid }) {
  const { zoid } = config();
  const { status, body } = await call("DELETE", `/organization/${zoid}/accounts/${accountId}`, { query: { zuid } });
  const ok = status === 200 && body?.status?.code === 200;
  if (!ok) throw new Error(`Zoho mailbox delete failed (${status}): ${JSON.stringify(body).slice(0, 300)}`);
  return true;
}

/** Reads recent messages for a mailbox. */
export async function messages(accountId, limit = 10) {
  const { body } = await call("GET", `/accounts/${accountId}/messages/view`, { query: { limit } });
  return body.data ?? [];
}

/**
 * Polls a mailbox until a numeric verification code shows up.
 * Returns the code, or throws once `timeoutMs` elapses.
 */
export async function waitForVerificationCode(accountId, { timeoutMs = 120_000, intervalMs = 5_000, digits = 6 } = {}) {
  const pattern = new RegExp(`\\b(\\d{${digits}})\\b`);
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    for (const message of await messages(accountId)) {
      const haystack = `${message.subject ?? ""} ${message.summary ?? ""}`;
      const found = haystack.match(pattern);
      if (found) return found[1];
      // The list view truncates, so fall back to the full body when the subject
      // and summary do not carry the code.
      if (message.messageId) {
        const { body } = await call("GET", `/accounts/${accountId}/folders/${message.folderId}/messages/${message.messageId}/content`);
        const full = JSON.stringify(body.data ?? "");
        const inBody = full.match(pattern);
        if (inBody) return inBody[1];
      }
    }
    await new Promise(resolve => setTimeout(resolve, intervalMs));
  }
  throw new Error(`No ${digits}-digit verification code arrived for account ${accountId} within ${timeoutMs}ms`);
}

/** Removes QA mailboxes left behind by runs that died before cleaning up. */
export async function sweepOrphans() {
  const orphans = (await listMailboxes()).filter(m => m.email?.startsWith(QA_PREFIX));
  const removed = [];
  for (const orphan of orphans) {
    try {
      await deleteMailbox(orphan);
      removed.push(orphan.email);
    } catch (error) {
      // Report rather than throw: one stuck mailbox should not block a run.
      console.warn(`sweepOrphans: could not delete ${orphan.email}: ${error.message}`);
    }
  }
  return removed;
}

/**
 * Runs `fn` with a freshly provisioned mailbox and deletes it afterwards, whether
 * or not `fn` throws. This is the only entry point that guarantees the allocation
 * comes back, so reach for it by default.
 */
export async function withDisposableMailbox(fn, { label, sweepFirst = true } = {}) {
  if (sweepFirst) await sweepOrphans();
  const mailbox = await createMailbox(label);
  try {
    return await fn(mailbox);
  } finally {
    try {
      await deleteMailbox(mailbox);
    } catch (error) {
      // Loud, because a leak permanently costs one of very few allocations.
      console.error(`LEAKED MAILBOX ${mailbox.email} - delete it in the Zoho admin console: ${error.message}`);
    }
  }
}

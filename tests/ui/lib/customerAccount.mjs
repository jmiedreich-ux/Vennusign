/**
 * Signing in as the QA customer, through the real "Sign in with Vennusign" flow.
 *
 * There is no shortcut here on purpose: this drives the same Entra External ID
 * hosted pages a customer uses, so a test that signs in proves customer sign-in
 * works. The three-defect history behind that flow is exactly why it is exercised
 * rather than stubbed.
 *
 * Credentials come from the environment, a machine-local file, or Key Vault
 * (`qa-murphy-entra-email` / `qa-murphy-entra-password`) - see secrets.mjs.
 * `qaCredentials()` returns null rather than throwing when nothing is configured,
 * so a suite can skip honestly on a machine that has no QA account.
 */
import { homedir } from "node:os";
import { join } from "node:path";
import { resolveSecrets, describeSources } from "./secrets.mjs";

const CONFIG_PATH = process.env.VENNUSIGN_QA_ACCOUNT_CONFIG ?? join(homedir(), ".config", "vennusign-qa-account.json");

const DESCRIPTORS = {
  email: { env: "VENNU_QA_EMAIL", file: "email", vault: "qa-murphy-entra-email" },
  password: { env: "VENNU_QA_PASSWORD", file: "password", vault: "qa-murphy-entra-password" }
};

export const qaCredentials = () => resolveSecrets(DESCRIPTORS, { filePath: CONFIG_PATH });
export const qaCredentialSources = () => describeSources(DESCRIPTORS, { filePath: CONFIG_PATH });

/**
 * Signs in through Entra and returns when Back Office has a customer session.
 *
 * Idempotent: an already-signed-in page is left alone, because the entry page
 * redirects an authenticated visitor away from the provider buttons.
 */
export async function signInAsCustomer(page, { email, password }, { entryPath = "/signin" } = {}) {
  await page.goto(entryPath);

  // The entry page renders "Opening secure signup..." while it resolves the public
  // plans and any existing session, so the provider link does not exist on first
  // paint. Checking for it immediately reports "already signed in" for a page that
  // is merely still loading, and the sign-in silently does nothing.
  const provider = page.locator("a.customer-entry__provider--primary");
  const alreadyIn = page.locator(".customer-entry__signout, .customer-onboarding");
  await Promise.race([
    provider.waitFor({ state: "visible", timeout: 30_000 }),
    alreadyIn.first().waitFor({ state: "visible", timeout: 30_000 })
  ]).catch(() => {
    throw new Error(`The customer entry page at ${entryPath} never finished loading: neither a sign-in provider nor a signed-in surface appeared.`);
  });

  // An authenticated visitor is redirected away from the provider buttons.
  if (!(await provider.isVisible())) return;
  await provider.click();

  const emailField = page.getByPlaceholder("Email address");
  const passwordField = page.locator('input[type="password"]');

  // Entra's hosted page bootstraps client-side and reloads itself once for SSO
  // (`sso_reload=true`), so the form is not in the DOM when navigation settles.
  // Probing for a field here instead of waiting for one reports "no email field"
  // on a page that grows one a second later. Wait for whichever step this browser
  // actually lands on: a remembered browser skips straight to the password.
  await Promise.race([
    emailField.waitFor({ state: "visible", timeout: 60_000 }),
    passwordField.waitFor({ state: "visible", timeout: 60_000 })
  ]);

  if (await emailField.isVisible()) {
    await emailField.fill(email);
    await page.getByRole("button", { name: /^next$/i }).click();
  }

  await passwordField.waitFor({ state: "visible", timeout: 60_000 });
  await passwordField.fill(password);
  await page.getByRole("button", { name: /^sign in$/i }).click();

  // Entra may interrupt the redirect with "Stay signed in?". Whether it appears
  // depends on tenant policy and on this browser, so wait for whichever comes
  // first - the prompt, or the redirect that means it was skipped. Probing for the
  // button immediately finds nothing on a page that is still rendering it, and the
  // sign-in then hangs on a prompt nobody answered.
  const leftEntra = url => !url.hostname.includes("ciamlogin.com");
  const stayPrompt = page.getByRole("button", { name: /^(yes|no)$/i }).first();
  await Promise.race([
    stayPrompt.waitFor({ state: "visible", timeout: 30_000 }),
    page.waitForURL(leftEntra, { timeout: 30_000 })
  ]).catch(() => undefined);

  // "No" keeps each run independent of the last.
  if (await stayPrompt.isVisible().catch(() => false)) {
    await page.getByRole("button", { name: /^no$/i }).click();
  }

  await page.waitForURL(leftEntra, { timeout: 60_000 });
}

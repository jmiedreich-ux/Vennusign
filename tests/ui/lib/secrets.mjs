/**
 * Where QA credentials come from, in one place.
 *
 * Three sources, in order, so the same helper works on a developer machine, in
 * CI, and in a one-off shell:
 *
 *   1. environment variables  - explicit, and the only source a throwaway run needs
 *   2. a machine-local JSON file - what a developer machine already has, and no
 *      Key Vault round-trip per secret per run
 *   3. Azure Key Vault via the az CLI - so a fresh machine or CI runner needs only
 *      an Azure identity holding Key Vault Secrets User on the vault
 *
 * None of these values are in the repo, and none are ever written back to disk.
 *
 * The Key Vault fallback shells out to `az`, so it needs the CLI on PATH (or
 * AZ_CLI_PATH). That holds on a Linux CI runner but not everywhere: when it is
 * missing the error names every source that was tried, so the cause is obvious
 * rather than silent.
 */
import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";

export const DEFAULT_KEY_VAULT = process.env.VENNUSIGN_KEY_VAULT ?? "kv-vennusign-dev";

/**
 * Reads one secret from Key Vault, or returns null when the CLI is absent or
 * unauthenticated so a caller can fall through to another source.
 */
export function keyVaultSecret(name, vault = DEFAULT_KEY_VAULT) {
  const azure = process.env.AZ_CLI_PATH ?? "az";
  try {
    return execFileSync(
      azure,
      ["keyvault", "secret", "show", "--vault-name", vault, "--name", name, "--query", "value", "-o", "tsv"],
      { encoding: "utf8", stdio: ["ignore", "pipe", "ignore"] }
    ).trim();
  } catch {
    return null;
  }
}

/**
 * Resolves a set of secrets described as `{ key: { env, file, vault } }`.
 *
 * `file` is the property name inside `filePath`'s JSON; `vault` is the Key Vault
 * secret name. Any of the three may be omitted to exclude that source for that key.
 * Returns null when any key cannot be resolved, so a caller can skip rather than
 * fail a suite that simply is not configured on this machine.
 */
export function resolveSecrets(descriptors, { filePath, vault = DEFAULT_KEY_VAULT } = {}) {
  let fileValues = {};
  if (filePath) {
    try {
      fileValues = JSON.parse(readFileSync(filePath, "utf8"));
    } catch {
      // Absent or unreadable is normal - it is one source of three, not a failure.
    }
  }

  const resolved = {};
  for (const [key, source] of Object.entries(descriptors)) {
    const value =
      (source.env ? process.env[source.env] : undefined) ??
      (source.file ? fileValues[source.file] : undefined) ??
      (source.vault ? keyVaultSecret(source.vault, vault) : null);
    if (!value) return null;
    resolved[key] = value;
  }
  return resolved;
}

/** The sources a missing secret was looked for in, for an honest error or skip message. */
export function describeSources(descriptors, { filePath, vault = DEFAULT_KEY_VAULT } = {}) {
  const envNames = Object.values(descriptors).map(source => source.env).filter(Boolean);
  const parts = [];
  if (envNames.length) parts.push(`environment variables (${envNames.join(", ")})`);
  if (filePath) parts.push(`${filePath} (machine-local, deliberately not in the repo)`);
  parts.push(`Azure Key Vault "${vault}" via the az CLI`);
  return parts.join(", then ");
}

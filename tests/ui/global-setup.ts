import { execFileSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";

/**
 * Prunes rows left behind by POST /api/test/seed before the suite runs.
 *
 * This is not just tidiness. Every screen renders a live /display/{id} iframe on the
 * screens page, so accumulated seed data makes that page progressively heavier until
 * specs time out. Starting from a clean fleet keeps run time flat.
 */
export default function globalSetup() {
  const here = dirname(fileURLToPath(import.meta.url));
  const script = resolve(here, "..", "..", "scripts", "start-ui-test-env.ps1");
  try {
    const output = execFileSync(
      "powershell.exe",
      ["-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", script, "-PruneSeed"],
      { encoding: "utf8" }
    );
    process.stdout.write(`[global-setup] ${output.trim().split("\n").pop() ?? "pruned"}\n`);
  } catch (error) {
    // A prune failure must not mask real test results; report and continue.
    process.stdout.write(`[global-setup] prune skipped: ${(error as Error).message}\n`);
  }
}

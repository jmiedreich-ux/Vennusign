import assert from "node:assert/strict";
import { readdir, readFile } from "node:fs/promises";
import test from "node:test";

const backOffice = new URL("../src/", import.meta.url);
const platformOperations = new URL("../../platform-operations/src/", import.meta.url);

async function sourceFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const files = entries.filter(entry => entry.isFile() && /\.(?:ts|tsx|mjs)$/.test(entry.name));
  return Promise.all(files.map(async entry => ({ name: entry.name, content: await readFile(new URL(entry.name, directory), "utf8") })));
}

test("admin applications use one accessible destructive-review contract instead of browser prompts", async () => {
  const [backOfficeSources, platformSources, backOfficeDialog, platformDialog] = await Promise.all([
    sourceFiles(backOffice),
    sourceFiles(platformOperations),
    readFile(new URL("DestructiveReviewDialog.tsx", backOffice), "utf8"),
    readFile(new URL("DestructiveReviewDialog.tsx", platformOperations), "utf8")
  ]);

  for (const source of [...backOfficeSources, ...platformSources]) {
    assert.doesNotMatch(source.content, /window\.confirm/, `${source.name} still uses a browser confirmation prompt`);
  }
  assert.equal(backOfficeDialog, platformDialog);
  assert.match(backOfficeDialog, /<dialog/);
  assert.match(backOfficeDialog, /showModal\(\)/);
  assert.match(backOfficeDialog, /aria-labelledby/);
  assert.match(backOfficeDialog, /aria-describedby/);
  assert.match(backOfficeDialog, /onCancel=/);
  assert.match(backOfficeDialog, /autoFocus/);
  assert.match(backOfficeDialog, /confirmation === request\.typedConfirmation/);
});

test("irreversible screen unpair requires the exact screen name", async () => {
  const screens = await readFile(new URL("ScreenManagement.tsx", backOffice), "utf8");

  assert.match(screens, /typedConfirmation: screen\.name/);
  assert.match(screens, /cannot be restored from this list/i);
  assert.match(screens, /confirmLabel: "Unpair screen"/);
});

import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("customer UI and messages use the Vennusign product name consistently", async () => {
  const content = (await Promise.all([
    readFile(new URL("index.html", root), "utf8"),
    readFile(new URL("src/App.tsx", root), "utf8"),
    readFile(new URL("src/CustomerOnboardingApp.tsx", root), "utf8"),
    readFile(new URL("src/BillingStatusCard.tsx", root), "utf8"),
    readFile(new URL("src/customerOnboardingApi.ts", root), "utf8")
  ])).join("\n");

  assert.match(content, /Vennusign/);
  assert.match(content, /Vennusign Back Office/);
  assert.match(content, /aria-label="Back Office"/);
  assert.doesNotMatch(content, /\bVennu\b/);
  assert.doesNotMatch(content, /Super Admin|Venue Admin/);
});

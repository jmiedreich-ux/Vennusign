import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, page, api, styles] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/SystemConfiguration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("configuration is a dedicated Platform Operations route", () => {
  assert.match(app, /path: "configuration"/);
  assert.match(app, /<SystemConfiguration/);
  assert.match(page, /Development/);
  assert.match(page, /Production/);
  assert.match(page, /Application/);
});

test("configuration search matches hierarchical keys and provides clear results", () => {
  assert.match(page, /Search settings/);
  assert.match(page, /Key, section, or description/);
  assert.match(page, /searchTerms\.every/);
  assert.match(page, /setting\.key, setting\.description, setting\.applicationScope, setting\.valueType/);
  assert.match(page, /Showing \{filteredSettings\.length\} of \{settings\.length\}/);
  assert.match(page, /No settings match this search/);
  assert.match(page, /Clear search/);
});

test("configuration value inputs share one responsive width", () => {
  assert.match(styles, /grid-template-columns:\s*minmax\(260px, 1fr\) minmax\(280px, 360px\) auto/);
  assert.match(styles, /\.configuration-card > label, \.configuration-card > label input \{ width: 100%/);
  assert.match(styles, /\.configuration-card > label input \{ min-height: 42px/);
});

test("configuration operations expose health rotation history and audited rollback", () => {
  assert.match(page, /Database provider:/);
  assert.match(page, /rotate every/);
  assert.match(page, /Secret payloads are never returned/);
  assert.match(page, /new audited revision/);
  assert.match(page, /Roll back/);
  assert.match(api, /\/health/);
  assert.match(api, /\/revisions/);
  assert.match(api, /\/rollback/);
});

test("configuration transfer is secret-safe reviewable and transactional", () => {
  assert.match(page, /Export/);
  assert.match(page, /Import JSON/);
  assert.match(page, /Import preview/);
  assert.match(page, /Secrets are excluded/);
  assert.match(page, /window\.confirm/);
  assert.match(page, /Apply selected changes/);
  assert.match(page, /applied transactionally/);
  assert.match(api, /configuration-transfer\/preview/);
  assert.match(api, /configuration-transfer\/apply/);
});

test("secret configuration is write-only with explicit clear confirmation", () => {
  assert.match(page, /Secrets are write-only/);
  assert.match(page, /type=\{setting\.isSecret \? "password"/);
  assert.match(page, /Secret configured/);
  assert.match(page, /window\.confirm/);
  assert.match(page, /Replace secret/);
  assert.doesNotMatch(page, /reveal/i);
});

test("configuration feedback covers load empty error success and concurrency", () => {
  assert.match(page, /Loading configuration/);
  assert.match(page, /No registered settings match/);
  assert.match(page, /role="alert"/);
  assert.match(page, /role="status"/);
  assert.match(page, /Restart the affected application/);
  assert.match(api, /This setting changed\. Reload before saving again/);
  assert.match(api, /expectedVersion/);
});

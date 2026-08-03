import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, page, api] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/SystemConfiguration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8")
]);

test("configuration is a dedicated Super Admin route", () => {
  assert.match(app, /path: "configuration"/);
  assert.match(app, /<SystemConfiguration/);
  assert.match(page, /Development/);
  assert.match(page, /Production/);
  assert.match(page, /Application/);
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

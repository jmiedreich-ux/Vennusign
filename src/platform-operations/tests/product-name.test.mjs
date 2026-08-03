import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("internal UI uses the Vennusign product name consistently", async () => {
  const content = (await Promise.all([
    readFile(new URL("index.html", root), "utf8"),
    readFile(new URL("src/App.tsx", root), "utf8")
  ])).join("\n");

  assert.match(content, /Vennusign/);
  assert.match(content, /Vennusign Platform Operations/);
  assert.match(content, /aria-label="Platform Operations"/);
  assert.doesNotMatch(content, /\bVennu\b/);
  assert.doesNotMatch(content, /Super Admin|Venue Admin/);
});

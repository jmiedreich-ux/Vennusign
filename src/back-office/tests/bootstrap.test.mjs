import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

test("Back Office bootstrap uses its own protected endpoint and token header", () => {
  const api = readFileSync(new URL("../src/api.ts", import.meta.url), "utf8");
  const app = readFileSync(new URL("../src/App.tsx", import.meta.url), "utf8");

  assert.match(api, /api\/back-office\/session/);
  assert.match(api, /X-Vennusign-Back-Office-Token/);
  assert.doesNotMatch(api, /X-Vennusign-Platform-Operations-Key/);
  assert.match(app, /sessionStorage\.removeItem\(tokenStorageKey\)/);
  assert.match(api, /invalid or has expired/);
});

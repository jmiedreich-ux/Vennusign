import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

test("venue admin bootstrap uses its own protected endpoint and token header", () => {
  const api = readFileSync(new URL("../src/api.ts", import.meta.url), "utf8");
  const app = readFileSync(new URL("../src/App.tsx", import.meta.url), "utf8");

  assert.match(api, /api\/venue-admin\/session/);
  assert.match(api, /X-Vennu-Venue-Token/);
  assert.doesNotMatch(api, /X-Vennu-Admin-Key/);
  assert.match(app, /sessionStorage\.removeItem\(tokenStorageKey\)/);
  assert.match(api, /invalid or has expired/);
});

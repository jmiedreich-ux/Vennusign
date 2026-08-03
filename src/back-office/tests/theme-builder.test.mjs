import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [operations, theme, api] = await Promise.all([
  readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/ThemeBuilder.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8")
]);

test("venue operations retain basic and advanced theme workflows", () => {
  assert.match(operations, /<ThemeBuilder/);
  assert.match(theme, /saveVenueTheme/);
  assert.match(theme, /saveAdvancedVenueTheme/);
  assert.match(theme, /advancedEnabled/);
  assert.match(api, /venueOperationRequest/);
});

import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const app = await readFile(new URL("../src/App.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");
const styles = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");

test("Back Office keeps account identity separate from the active organization and venue", () => {
  assert.match(app, /Active workspace/);
  assert.match(app, /session\.organizationName/);
  assert.match(app, /session\.venueName/);
  assert.match(app, /Signed in as/);
  assert.match(app, /session\.account\.displayName/);
});

test("venue switching is confirmed, server-authorized, and announced", () => {
  assert.match(app, /window\.confirm/);
  assert.match(app, /selectBackOfficeVenue/);
  assert.match(app, /role="status" aria-live="polite"/);
  assert.match(api, /X-Vennusign-Venue-Id/);
  assert.match(api, /localStorage\.setItem\(venueContextStorageKey, session\.venueId\)/);
  assert.match(api, /response\.status === 401[\s\S]*clearBackOfficeVenueContext/);
  assert.match(app, /Set up or recover an organization and venue/);
});

test("context controls support native keyboard interaction and narrow layouts", () => {
  assert.match(app, /<select[\s\S]*id="workspace-context-select"/);
  assert.match(styles, /workspace-context__controls select:focus-visible/);
  assert.match(styles, /@media \(max-width: 760px\)[\s\S]*workspace-context/);
  assert.match(styles, /text-overflow: ellipsis/);
});

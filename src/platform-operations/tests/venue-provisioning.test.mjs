import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";
import { validateVenueDraft } from "../src/venueProvisioning.mjs";

test("venue draft validation trims the supported profile and keeps secondary language optional", () => {
  const result = validateVenueDraft({
    name: "  Harbor Café ",
    timezone: " America/New_York ",
    type: " Café ",
    primaryLanguage: " en ",
    secondaryLanguage: " "
  });

  assert.equal(result.valid, true);
  assert.deepEqual(result.venue, {
    name: "Harbor Café",
    timezone: "America/New_York",
    type: "Café",
    primaryLanguage: "en",
    secondaryLanguage: undefined
  });
});

test("venue draft validation rejects missing required values and overlong fields", () => {
  const result = validateVenueDraft({
    name: "",
    timezone: "UTC",
    type: "x".repeat(51),
    primaryLanguage: ""
  });

  assert.equal(result.valid, false);
  assert.equal(result.errors.name, "Required");
  assert.equal(result.errors.type, "Maximum 50 characters");
  assert.equal(result.errors.primaryLanguage, "Required");
});

test("venue directory connects the protected create flow and opens the created venue", () => {
  const directory = readFileSync(new URL("../src/VenueDirectory.tsx", import.meta.url), "utf8");
  const api = readFileSync(new URL("../src/api.ts", import.meta.url), "utf8");

  assert.match(directory, /createVenue\(configuration, apiKey, validation\.venue\)/);
  assert.match(directory, /onSelectVenue\(result\.venueId\)/);
  assert.match(directory, /Creates the venue with a Starter trial/);
  assert.match(api, /method: "POST"/);
  assert.match(api, /api\/platform-operations\/venues/);
  assert.match(api, /X-Vennusign-Platform-Operations-Key/);
});

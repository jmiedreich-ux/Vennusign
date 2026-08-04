import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const source = name => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("shared success feedback matches the accessible admin contract", () => {
  const feedback = source("TransientFeedback.tsx");
  assert.match(feedback, /aria-live="polite"/);
  assert.match(feedback, /aria-atomic="true"/);
  assert.match(feedback, /timeoutMs = 7000/);
  assert.match(feedback, /window\.clearTimeout/);
  assert.match(feedback, /aria-label="Dismiss success message"/);
});

test("successful operations use toasts while failures stay in context", () => {
  for (const name of ["ScreenManagement.tsx", "SystemConfiguration.tsx", "VenueDetail.tsx", "TierManagement.tsx", "OnboardingSupport.tsx"]) {
    assert.match(source(name), /TransientFeedback/);
  }
  assert.match(source("VenueDetail.tsx"), /className="matrix-message error" role="alert"/);
  assert.match(source("SystemConfiguration.tsx"), /className="state error" role="alert"/);
});

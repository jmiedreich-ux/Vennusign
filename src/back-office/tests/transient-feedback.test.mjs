import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const source = name => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("shared success feedback is polite dismissible and time bounded", () => {
  const feedback = source("TransientFeedback.tsx");
  assert.match(feedback, /aria-live="polite"/);
  assert.match(feedback, /aria-atomic="true"/);
  assert.match(feedback, /timeoutMs = 7000/);
  assert.match(feedback, /window\.clearTimeout/);
  assert.match(feedback, /aria-label="Dismiss success message"/);
});

test("successful operations use toasts while errors remain inline", () => {
  for (const name of ["AccountSecurity.tsx", "QuickUpdateMode.tsx", "ScreenManagement.tsx", "VideoWallBuilder.tsx", "TapListAdministration.tsx"]) {
    assert.match(source(name), /TransientFeedback/);
  }
  assert.match(source("QuickUpdateMode.tsx"), /className="state error" role="alert"/);
  assert.match(source("AccountSecurity.tsx"), /role="alert" className="account-security__error"/);
});

test("toast motion is removed for reduced-motion users", () => {
  const styles = source("styles.css");
  assert.match(styles, /prefers-reduced-motion: reduce/);
  assert.match(styles, /\.transient-feedback \{ animation: none; \}/);
});

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const source = name => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("monoline icons are decorative and inherit the surrounding color", () => {
  const icons = source("SkyIcon.tsx");
  assert.match(icons, /fill="none"/);
  assert.match(icons, /stroke="currentColor"/);
  assert.match(icons, /strokeWidth="1\.8"/);
  assert.match(icons, /aria-hidden="true"/);
  assert.match(icons, /focusable="false"/);
});

test("empty states expose context and an optional recovery action", () => {
  const emptyState = source("EmptyState.tsx");
  assert.match(emptyState, /title: string/);
  assert.match(emptyState, /message: string/);
  assert.match(emptyState, /action\?: ReactNode/);
  assert.match(source("ScreenManagement.tsx"), /Clear screen filters/);
  assert.match(source("AccountSecurity.tsx"), /No passkeys yet/);
});

test("stable skeletons announce loading and honor reduced motion", () => {
  const skeleton = source("LoadingSkeleton.tsx");
  assert.match(skeleton, /role="status"/);
  assert.match(skeleton, /aria-busy="true"/);
  assert.match(skeleton, /aria-hidden="true"/);
  assert.match(source("styles.css"), /prefers-reduced-motion: reduce[^}]*\.sky-loading-skeleton__row \{ animation: none;/s);
});

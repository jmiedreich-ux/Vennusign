import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const source = name => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("admin icon empty and loading primitives share the Sky UI contract", () => {
  const icons = source("SkyIcon.tsx");
  assert.match(icons, /fill="none"/);
  assert.match(icons, /stroke="currentColor"/);
  assert.match(icons, /aria-hidden="true"/);
  assert.match(source("LoadingSkeleton.tsx"), /aria-busy="true"/);
  assert.match(source("EmptyState.tsx"), /action\?: ReactNode/);
});

test("directory and dashboard empty states provide bounded next actions", () => {
  const directory = source("VenueDirectory.tsx");
  const dashboard = source("OperationalDashboard.tsx");
  assert.match(directory, /No matching venues/);
  assert.match(directory, /Create venue/);
  assert.match(dashboard, /Show all screens/);
  assert.match(dashboard, /Refresh events/);
});

test("loading placeholders preserve accessible labels and remove motion", () => {
  assert.match(source("OperationalDashboard.tsx"), /LoadingSkeleton label="Loading operational dashboard…"/);
  assert.match(source("styles.css"), /prefers-reduced-motion: reduce[^}]*\.sky-loading-skeleton__row \{ animation: none;/s);
});

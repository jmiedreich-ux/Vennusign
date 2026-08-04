import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const source = name => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("mobile operations shell keeps sign-out available", () => {
  const app = source("App.tsx");
  const styles = source("styles.css");

  assert.match(app, /className="mobile-signout"[^>]*onClick=\{signOut\}/);
  assert.match(styles, /\.mobile-signout \{ display: none;/);
  assert.match(styles, /\.mobile-signout \{ display: inline-flex; \}/);
});

test("access failures provide specific recovery guidance", () => {
  const app = source("App.tsx");

  assert.match(app, /reason\.status === 401/);
  assert.match(app, /rejected or has expired/);
  assert.match(app, /reason\.status === 403/);
  assert.match(app, /does not grant Platform Operations access/);
  assert.match(app, /aria-describedby="access-guidance"/);
  assert.match(app, /role=\{error \? "alert"/);
});

test("revenue trend exposes values without hover", () => {
  const dashboard = source("OperationalDashboard.tsx");

  assert.match(dashboard, /className="trend-value">\{currency\.format\(point\.mrr\)\}/);
  assert.match(dashboard, /role="list"/);
  assert.match(dashboard, /active subscriptions/);
  assert.doesNotMatch(dashboard, /title=\{`\$\{currency\.format\(point\.mrr\)/);
});

test("support tables retain headings while scrolling", () => {
  const styles = source("styles.css");

  assert.match(styles, /\.table-wrap \{ max-height:[^}]*overflow: auto;/);
  assert.match(styles, /thead th \{ position: sticky; top: 0;/);
  assert.match(styles, /\.matrix-table thead th:first-child \{ z-index: 3; \}/);
});

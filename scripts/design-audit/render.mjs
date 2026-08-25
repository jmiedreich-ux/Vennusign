// Renders every styling class the back-office actually uses, with the app's own stylesheets, and
// screenshots the result.
//
//   node scripts/design-audit/render.mjs
//     → docs/features/design-system/control-variants.png
//
// The variant list comes from collect.mjs, never from a hand-written array. An earlier version
// hand-picked which classes to draw and silently omitted the Actions button — the owner spotted it
// missing. A picture assembled by choosing what to include is a summary, and a summary is the
// thing this work exists to stop.
//
// The CSS is read from the real stylesheets. If a control looks wrong here, it looks wrong in the
// product; that is what a mock can never promise.

import { readFileSync, writeFileSync } from "node:fs";
import { chromium } from "../../tests/ui/node_modules/playwright/index.mjs";
import { collect } from "./collect.mjs";

const STYLESHEETS = [
  "src/back-office/src/sky-ui-tokens.css",
  "src/back-office/src/styles.css",
  "src/back-office/src/menu-builder.css",
  "src/back-office/src/menus-home.css",
  "src/back-office/src/menu-paste-import.css",
  "src/back-office/src/operations.css",
  "src/back-office/src/quick-update-board.css"
];

const OUTPUT = "docs/features/design-system/control-variants.png";

/**
 * What a class is FOR, inferred from its name. The grouping is the question being asked — five
 * different looks for one job is only visible once they sit together.
 */
function role(className) {
  const name = className.toLowerCase();
  if (/danger|delete|destructive|remove/.test(name)) return "Destructive";
  if (/primary|publish|save|submit|add-tile|apply/.test(name)) return "Main action";
  if (/secondary|later|back|cancel|dismiss|quiet|close/.test(name)) return "Ordinary action";
  if (/link|crumb|restore/.test(name)) return "Plain link";
  return "Everything else";
}

const ORDER = ["Main action", "Ordinary action", "Destructive", "Plain link", "Everything else"];

function escapeHtml(text) {
  return text.replace(/[&<>"]/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" })[c]);
}

function buildPage() {
  const controls = collect().filter(c => c.kind === "button");

  // One row per distinct class, labelled with a real label taken from its own uses.
  const byClass = new Map();
  for (const control of controls) {
    if (!control.className || control.className === "(dynamic)") continue;
    for (const one of control.className.split(/\s+/).filter(Boolean)) {
      if (!byClass.has(one)) byClass.set(one, []);
      byClass.get(one).push(control.label);
    }
  }

  const groups = new Map(ORDER.map(name => [name, []]));
  for (const [className, labels] of byClass) {
    const real = labels.find(l => l && !l.startsWith("[")) ?? className;
    groups.get(role(className)).push({ className, label: real, uses: labels.length });
  }

  const unstyledCount = controls.filter(c => c.className === "").length;
  const sections = ORDER.map(name => {
    const rows = groups.get(name).sort((a, b) => b.uses - a.uses);
    if (!rows.length) return "";
    const items = rows.map(({ className, label, uses }) => `
      <div class="row">
        <span class="name">.${escapeHtml(className)}<em>${uses}&times;</em></span>
        <span class="demo"><button class="${escapeHtml(className)}">${escapeHtml(label)}</button></span>
      </div>`).join("");
    return `<section><h2>${name} <b>${rows.length} different looks</b></h2>${items}</section>`;
  }).join("");

  const bare = ["Save name", "Cancel", "Delete", "Retry last change", "↑"].map(label => `
    <div class="row">
      <span class="name">(no class)</span>
      <span class="demo"><button>${escapeHtml(label)}</button></span>
    </div>`).join("");

  return `<!doctype html><meta charset="utf-8"><style>
${STYLESHEETS.map(path => readFileSync(path, "utf8")).join("\n")}
body{font-family:Inter,system-ui,sans-serif;background:#f8fafc;padding:30px;margin:0}
section{margin:0 0 28px}
h2{font:600 13px/1 Inter;letter-spacing:.07em;text-transform:uppercase;color:#0f172a;margin:0 0 14px}
h2 b{font-weight:500;text-transform:none;letter-spacing:0;color:#94a3b8;margin-left:8px}
.row{display:flex;align-items:center;gap:16px;padding:9px 0;border-bottom:1px solid #eef2f6}
.name{font:500 11.5px/1.4 ui-monospace,SFMono-Regular,monospace;color:#475569;width:240px;flex:none}
.name em{font-style:normal;color:#cbd5e1;margin-left:7px}
.demo{flex:none}
</style>
${sections}
<section><h2>No class at all <b>${unstyledCount} buttons</b></h2>
<p style="font:13px/1.5 Inter;color:#64748b;margin:0 0 12px;max-width:52ch">Never styled, rather than
drifted. A destructive action is indistinguishable from a safe one here.</p>${bare}</section>`;
}

const html = "/tmp/vennusign-control-variants.html";
writeFileSync(html, buildPage());

const browser = await chromium.launch();
const tab = await browser.newPage({ viewport: { width: 820, height: 900 }, deviceScaleFactor: 2 });
await tab.goto(`file://${html}`);
await tab.waitForTimeout(300);
await tab.screenshot({ path: OUTPUT, fullPage: true });
await browser.close();
console.log(`wrote ${OUTPUT}`);

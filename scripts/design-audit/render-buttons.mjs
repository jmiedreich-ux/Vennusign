// Renders every button variant the back-office actually ships, using the app's own stylesheets,
// and screenshots the result. A list of class names cannot answer "should these be one thing?" —
// you have to see them side by side.
//
//   node scripts/design-audit/render-buttons.mjs
//     → docs/features/design-system/button-variants.png
//
// Playwright lives in tests/ui, so run this from the repository root; the import below resolves
// against that project rather than a second install.
//
// The CSS is read from the real stylesheets, never copied. If a button looks wrong here, it looks
// wrong in the product — that is the point, and it is what a mock can never promise.

import { readFileSync, writeFileSync } from "node:fs";
import { chromium } from "../../tests/ui/node_modules/playwright/index.mjs";

const STYLESHEETS = [
  "src/back-office/src/sky-ui-tokens.css",
  "src/back-office/src/styles.css",
  "src/back-office/src/menu-builder.css",
  "src/back-office/src/menus-home.css",
  "src/back-office/src/menu-paste-import.css"
];

const OUTPUT = "docs/features/design-system/button-variants.png";

// Grouped by the job the button does, not by the name it happens to carry — the grouping IS the
// question being asked. Labels are real ones taken from the source.
const GROUPS = [
  {
    title: "Meant to be the main action",
    note: "Five different looks. Nobody chose this; it accumulated.",
    variants: [
      ["action-primary", "Add a menu"],
      ["builder__publish-button", "Publish 3 changes"],
      ["import-primary", "Create menu"],
      ["upgrade-modal__primary", "Upgrade"],
      ["menus-home__add-tile", "Add a menu"]
    ]
  },
  {
    title: "Meant to be the ordinary action",
    note: "Three looks, one of which has no button shape at all.",
    variants: [
      ["action-secondary", "Keep it"],
      ["import-secondary", "Cancel"],
      ["import-back", "Back"],
      ["upgrade-modal__later", "Not now"]
    ]
  },
  {
    title: "Meant to be destructive",
    note: "`.danger` is indistinguishable from Cancel and from Save. That is a hazard, not a nitpick.",
    variants: [
      ["action-danger", "Remove"],
      ["builder__quiet-danger", "Delete section"],
      ["danger", "Delete"]
    ]
  },
  {
    title: "Meant to be a plain link",
    variants: [
      ["builder__link", "View all"],
      ["builder__capacity-link", "Check fit"],
      ["quiet", "Dismiss"]
    ]
  },
  {
    title: "No class at all — 122 of 245 buttons",
    note: "Never styled, rather than drifted. Destructive and safe actions are identical here.",
    variants: [
      ["", "Save name"],
      ["", "Cancel"],
      ["", "Delete"],
      ["", "Add period"],
      ["", "Retry last change"],
      ["", "↑"]
    ]
  }
];

function page() {
  const css = STYLESHEETS.map(path => readFileSync(path, "utf8")).join("\n");
  const rows = GROUPS.map(group => {
    const note = group.note ? `<p class="note">${group.note}</p>` : "";
    const items = group.variants.map(([className, label]) => `
      <div class="row">
        <span class="name">${className ? `.${className}` : "(no class)"}</span>
        <span class="demo"><button${className ? ` class="${className}"` : ""}>${label}</button></span>
      </div>`).join("");
    return `<section><h2>${group.title}</h2>${note}${items}</section>`;
  }).join("");

  return `<!doctype html><meta charset="utf-8"><style>
${css}
body{font-family:Inter,system-ui,sans-serif;background:#f8fafc;padding:30px;margin:0}
section{margin:0 0 30px}
h2{font:600 13px/1 Inter;letter-spacing:.07em;text-transform:uppercase;color:#0f172a;margin:0 0 4px}
.note{font:13px/1.5 Inter;color:#64748b;margin:0 0 12px;max-width:52ch}
.row{display:flex;align-items:center;gap:16px;padding:10px 0;border-bottom:1px solid #eef2f6}
.name{font:500 12px/1.4 ui-monospace,SFMono-Regular,monospace;color:#475569;width:210px;flex:none}
.demo{flex:none}
</style>${rows}`;
}

const html = "/tmp/vennusign-button-variants.html";
writeFileSync(html, page());

const browser = await chromium.launch();
const tab = await browser.newPage({ viewport: { width: 760, height: 900 }, deviceScaleFactor: 2 });
await tab.goto(`file://${html}`);
await tab.waitForTimeout(300);
await tab.screenshot({ path: OUTPUT, fullPage: true });
await browser.close();
console.log(`wrote ${OUTPUT}`);

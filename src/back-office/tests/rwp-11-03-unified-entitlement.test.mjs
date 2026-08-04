import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const read = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const [app, chip, badge, sheet, navigation, lockedPage, inline, sidebar, styles] = await Promise.all([
  read("App.tsx"),
  read("EntitlementLockChip.tsx"),
  read("TierBadge.tsx"),
  read("UpgradeSheet.tsx"),
  read("LockedNavigationItem.tsx"),
  read("LockedSectionPreview.tsx"),
  read("InlineFeatureHint.tsx"),
  read("SidebarUpgradeNudge.tsx"),
  read("styles.css")
]);

test("all locked contexts use one lock chip and shared tier badge", () => {
  for (const surface of [navigation, lockedPage, inline, sidebar, sheet]) {
    assert.match(surface, /EntitlementLockChip/);
  }
  assert.match(chip, /entitlement-lock-chip__icon/);
  assert.match(chip, /<TierBadge/);
  assert.match(badge, /tierPresentation/);
  assert.doesNotMatch(`${navigation}\n${lockedPage}\n${inline}\n${sidebar}`, /import TierBadge/);
});

test("one accessible upgrade sheet owns feature, price, and hosted-checkout launch state", () => {
  assert.match(app, /<UpgradeSheet/);
  assert.doesNotMatch(app, /UpgradeModal/);
  assert.match(sheet, /role="dialog"/);
  assert.match(sheet, /aria-modal="true"/);
  assert.match(sheet, /aria-describedby="upgrade-sheet-benefit"/);
  assert.match(sheet, /previous\?\.focus\(\)/);
  assert.match(sheet, /event\.key === "Escape" && !submitting\.current/);
  assert.match(sheet, /Opening secure checkout/);
});

test("lock chip and upgrade sheet remain responsive and visibly focusable", () => {
  assert.match(styles, /\.entitlement-lock-chip/);
  assert.match(styles, /\.upgrade-sheet-backdrop/);
  assert.match(styles, /@media \(max-width: 600px\) \{ \.upgrade-sheet-backdrop/);
  assert.match(styles, /:where\(button, a, input, select, textarea, summary\):focus-visible/);
});

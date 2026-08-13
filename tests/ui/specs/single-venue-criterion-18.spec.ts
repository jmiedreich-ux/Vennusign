import { test, expect, openAs } from "../fixtures";

/**
 * Acceptance criterion 18, named — the spec Q194 requires from milestone 2 on,
 * re-checked at every UI milestone after this one.
 *
 * "(Group) A single-venue account renders the same components with zero venue
 * affordances."
 *
 * The rule behind it is decision 29, and decision 21 before it: multi-venue is
 * not a second design. It is the same screens with one extra dimension, and below
 * the tier that dimension is *absent* — not disabled, not greyed, not teased.
 * There is no Venues nav, no trust levels, no venue chip, and no upgrade UI beyond
 * a single "Talk to us" at the moment someone tries to add a second venue.
 *
 * This is a leak check, and leaks are the whole risk: a group affordance shipped
 * to a single-venue account is a promise the product cannot keep. Every surface
 * this milestone ships is swept, because the cheapest place for one to appear is
 * a surface nobody thought to look at.
 */

/** Words and controls that only exist above the tier. */
const groupAffordances = [
  "Venues",
  "People",
  "Trust level",
  "Trust levels",
  "Group menu",
  "Head office",
  "Behind 9 days",
  "set by head office",
  "What venues can change",
  "Waiting on a venue",
  "Has local changes",
  "Local menus"
];

const surfaces = ["menu", "/menu/quick-update", "home", "screens", "billing"] as const;

for (const surface of surfaces) {
  test(`criterion 18 — ${surface} renders zero venue affordances for a single-venue account`, async ({ page }) => {
    await openAs(page, "owner", surface);
    // Attached, not visible: below the sidebar breakpoint the rail sits behind
    // the toggle, so its items are present and hidden. This is a leak check —
    // hidden markup leaks just as well as visible markup.
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });

    const body = page.locator("body");

    for (const affordance of groupAffordances) {
      await expect(
        body,
        `${surface} shows "${affordance}", which only exists above the tier`
      ).not.toContainText(new RegExp(`\\b${affordance}\\b`, "i"));
    }

    // The nav is where the group dimension would show first: the design's own nav
    // component lists Venues and People as group-only entries, and neither may
    // render at all here (decision 19's absent-not-greyed, applied to the group).
    await expect(page.locator('[data-testid="nav-item"][data-route="venues"]')).toHaveCount(0);
    await expect(page.locator('[data-testid="nav-item"][data-route="people"]')).toHaveCount(0);
  });
}

test("criterion 18 — the venue's name is a label, never a switcher", async ({ page }) => {
  // Q186: the venue name is a static label everywhere, no caret, never clickable.
  // A caret is the smallest possible venue affordance, and the wireframes carried
  // one, so it is worth asserting rather than assuming.
  await openAs(page, "owner", "menu");

  const eyebrow = page.locator(".menus-home__venue");
  await expect(eyebrow).toBeVisible();
  await expect(eyebrow).not.toContainText("▾");

  // Not a button, not a link, and nothing clickable inside it.
  expect(await eyebrow.evaluate(node => node.tagName)).toBe("P");
  await expect(eyebrow.locator("button, a, select")).toHaveCount(0);
});

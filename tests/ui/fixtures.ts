import { test as base, expect, type Page } from "@playwright/test";

/**
 * The Back Office reads its session token from sessionStorage on first render,
 * so seeding it before any script runs signs the page in without driving a form.
 * Tokens come from the API's BackOffice__Sessions__* configuration.
 */
const tokenStorageKey = "vennusign.back-office.token";

/**
 * "scale" is an owner of a SECOND venue, used only by the shelf-at-scale checks.
 * The default venue accumulates menus from every spec that seeds, so nothing there
 * can assert "exactly this many menus" while the suite runs in parallel.
 */
export type VennuRole = "owner" | "editor" | "publisher" | "scale" | "capacity";

/** Isolation tag of the seeded dataset this run targets. See run-track1-qa.ps1. */
const tag = process.env.VENNU_ISOLATION_TAG ?? "0000";

/**
 * The default dataset keeps the token names the owner acceptance workbook prints, so a
 * reviewer can sign in with exactly what it says. Only extra isolated datasets carry a
 * tag suffix, which is all that is needed to stop lanes sharing an identity.
 */
const baselineTokens: Record<VennuRole, string> = {
  owner: "track1-owner-review",
  editor: "track1-content-editor",
  publisher: "track1-publisher",
  scale: "track1-scale-check",
  capacity: "track1-capacity-check"
};

const tokenFor = (role: VennuRole) =>
  tag === "0000" ? baselineTokens[role] : `track1-${role}-${tag}`;

export const tokens: Record<VennuRole, string> = {
  owner: process.env.VENNU_OWNER_TOKEN ?? tokenFor("owner"),
  editor: process.env.VENNU_EDITOR_TOKEN ?? tokenFor("editor"),
  publisher: process.env.VENNU_PUBLISHER_TOKEN ?? tokenFor("publisher"),
  scale: process.env.VENNU_SCALE_TOKEN ?? tokenFor("scale"),
  capacity: process.env.VENNU_CAPACITY_TOKEN ?? tokenFor("capacity")
};

export const apiBaseUrl = process.env.VENNU_API_URL ?? "https://localhost:7138";

async function signIn(page: Page, role: VennuRole) {
  await page.addInitScript(
    ([key, value]) => window.sessionStorage.setItem(key, value),
    [tokenStorageKey, tokens[role]] as const
  );
}

/**
 * Opens a Back Office hash route already signed in as `role`.
 * Navigating straight to the route avoids paying for click-through navigation.
 */
export async function openAs(page: Page, role: VennuRole, route: string) {
  await signIn(page, role);
  await page.goto(`/#${route}`);
  // Not networkidle: the screens area polls for player status, so the network never
  // goes idle and the wait times out. Waiting for the shell is deterministic, and
  // Playwright's per-locator auto-waiting covers everything after it.
  await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
}

/**
 * Opens a menu in the builder, through the door a person uses.
 *
 * The board is the door: there is no Open button on a card (design README,
 * Navigation), which is why this clicks the board itself. Milestone 3 gave the
 * builder its own address, so a spec that wants one particular menu can also
 * navigate straight to `#/menu/{menuId}` — which is itself worth asserting, since
 * a refresh mid-edit depends on it.
 */
export async function openMenuBuilderAs(page: Page, role: VennuRole, menuId?: string) {
  if (menuId) {
    await openAs(page, role, `/menu/${menuId}`);
  } else {
    await openAs(page, role, "menu");
    await page.getByTestId("menus-home").waitFor();
    const card = page.getByTestId("menu-card").first();
    await card.waitFor();
    await card.getByTestId("open-menu").click();
  }
  await page.getByTestId("menu-builder").waitFor();
}

/**
 * The add-item search opens the moment a section is the active context - there
 * is no button to click to reveal it. This is only needed when an item's editor
 * is already showing (closing it hands the panel back to add mode); if nothing
 * is selected, the search box is already there.
 */
export async function openAddItem(page: Page) {
  const closeItem = page.getByTestId("close-item");
  if (await closeItem.isVisible().catch(() => false)) {
    await closeItem.click();
  }
  await page.getByTestId("add-item-input").waitFor({ state: "visible" });
}

/**
 * The bottom bar's Review first / discard draft / go back to… links and the
 * standalone Publish button all consolidated into one Actions dropdown
 * (#797 follow-on). These helpers open it and drive the item the old direct
 * testid used to reach.
 */
export async function openActionsMenu(page: Page) {
  await page.getByTestId("actions-menu-trigger").click();
  await page.getByTestId("actions-menu").waitFor({ state: "visible" });
}

/** What the old direct "review-first" click used to do: open the review dialog. */
export async function openReview(page: Page) {
  await openActionsMenu(page);
  await page.getByTestId("action-review-publish").click();
}

/**
 * What the old direct "publish" click used to do: publish without lingering on
 * the review dialog. There is no bypass-review path anymore - Review & publish
 * is the only way in - so this drives through it in one call.
 */
export async function publishDraft(page: Page) {
  await openReview(page);
  await page.getByTestId("publish-from-review").click();
}

/** What the old direct "discard-draft" click used to do: open the discard confirm. */
export async function openDiscardDraft(page: Page) {
  await openActionsMenu(page);
  await page.getByTestId("action-discard").click();
}

/** What the old direct "go-back-to" click used to do: open the history/restore dialog. */
export async function openGoBackTo(page: Page) {
  await openActionsMenu(page);
  await page.getByTestId("action-restore").click();
}

/**
 * One named card on the shelf, however many menus the venue happens to have.
 *
 * The default venue accumulates menus from every spec that seeds, so the shelf
 * crosses the scale cutover during a run and back again — search exists above it,
 * the plain grid below, and "N more" hides the tail in between. A spec that
 * assumed one of those states passed for the wrong reason whenever the count moved.
 */
export async function findShelfCard(page: Page, menuName: string) {
  // The shelf renders its container while it is still loading, so waiting for the
  // container alone decides "is there a search box?" before the answer exists —
  // and at scale that leaves the card hidden behind a search nobody filled. The
  // headline only appears once the menus are in.
  await page.getByTestId("shelf-headline").waitFor();

  const search = page.getByTestId("shelf-search");
  if (await search.count()) {
    await search.fill(menuName);
  } else {
    const more = page.getByTestId("shelf-more");
    if (await more.count()) await more.click();
  }

  const card = page.getByTestId("menu-card").filter({ hasText: menuName });
  await card.waitFor();
  return card;
}

export const test = base.extend<{ asOwner: Page }>({
  asOwner: async ({ page }, use) => {
    await signIn(page, "owner");
    await use(page);
  }
});

/*
 * Put away whatever this test seeded.
 *
 * Every spec imports `test` from here, so this reaches all of them without touching a single call
 * site — which matters, because there are 98 of them.
 *
 * It hangs off afterEach rather than a fixture so that a spec calling seed() several times, or
 * calling it outside a fixture's lifetime, is still covered. The import is deferred to keep this
 * module free of a cycle: seed.ts imports tokens from here.
 */
test.afterEach(async () => {
  const { cleanupSeeded } = await import("./seed");
  await cleanupSeeded();
});

export { expect };

/**
 * Create a blank menu and give it a name (M6.5).
 *
 * The name prompt is gone. "Add a menu" opens a route chooser, the blank route
 * creates the menu immediately, and the builder opens with the placeholder name
 * in its crumb, selected, so the first keystroke replaces it. What used to be
 * "click Add a menu, type a name, click Start blank" is now "click Add a menu,
 * click start-from-blank, type over what is already there".
 *
 * Every spec that just needs a menu to exist calls this rather than repeating
 * the sequence, so the next change to the front door edits one place.
 */
export async function createBlankMenu(page: Page, name: string) {
  await page.getByTestId("add-a-menu").first().click();
  await page.getByTestId("add-route-blank").click();
  const nameInput = page.getByTestId("menu-name-input");
  await nameInput.waitFor({ state: "visible" });
  await nameInput.fill(name);
  await nameInput.press("Enter");
  await expect(page.getByTestId("builder-menu-name")).toHaveText(name);
}

/** Open the Add-a-menu chooser and take the paste route into the import. */
export async function startPasteImport(page: Page) {
  await page.getByTestId("add-a-menu").first().click();
  await page.getByTestId("add-route-paste").click();
  await page.getByLabel("Menu text").waitFor({ state: "visible" });
}

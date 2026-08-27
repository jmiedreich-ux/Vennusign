import { test, expect, openAs } from "../fixtures";
import { seed } from "../seed";

test.describe("paste import review", () => {
  test("reviews, resumes, and creates one truthful unpublished menu", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    const seeded = await seed({ label: "paste-import" });
    const pageErrors: string[] = [];
    page.on("pageerror", error => pageErrors.push(error.message));
    await openAs(page, "owner", "/menu/import");

    await page.getByLabel("Menu text").fill(`${seeded.itemName.replaceAll(" ", "   ")}  ${seeded.itemPrice}\nChef note`);
    await page.getByRole("button", { name: "Read menu" }).click();
    expect(pageErrors, "the import route must not crash after creating its resumable URL").toEqual([]);
    await expect(page.getByTestId("menu-import-review")).toBeVisible();
    await expect(page).toHaveURL(/#\/menu\/import\/[0-9a-f-]+$/i);
    await expect(page.getByTestId("safe-match-banner")).toContainText("1 safe match");
    await expect(page.getByRole("heading", { name: "2 items need you" })).toBeVisible();

    await page.setViewportSize({ width: 900, height: 900 });
    await expect(page.getByTestId("menu-import-review")).toBeVisible();
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= document.documentElement.clientWidth), "the 900px supported floor must not overflow horizontally").toBe(true);

    await page.reload();
    await expect(page.getByTestId("safe-match-banner")).toContainText("1 safe match");
    await page.getByRole("button", { name: "Accept 1 safe match" }).click();
    await expect(page.getByRole("heading", { name: "1 item needs you" })).toBeVisible();
    await page.getByTestId("answer-dish").click();
    await expect(page.getByRole("heading", { name: "Where should these items go?" })).toBeVisible();
    // Choosing is confirming now - the name sits on the choice itself, no second screen.
    const savedName = `Imported ${seeded.menuName}`;
    const menuName = page.getByLabel("Menu name");
    await menuName.fill(savedName);
    expect(await menuName.evaluate(element => {
      const style = getComputedStyle(element);
      return { outlineWidth: style.outlineWidth, outlineColor: style.outlineColor, backgroundColor: style.backgroundColor, boxShadow: style.boxShadow };
    })).toEqual({ outlineWidth: "2px", outlineColor: "rgb(9, 111, 145)", backgroundColor: "rgb(251, 253, 255)", boxShadow: "none" });
    await page.evaluate(() => document.documentElement.dataset.skyTheme = "midnight");
    expect(await menuName.evaluate(element => {
      const style = getComputedStyle(element);
      return { outlineWidth: style.outlineWidth, outlineColor: style.outlineColor, backgroundColor: style.backgroundColor, boxShadow: style.boxShadow };
    })).toEqual({ outlineWidth: "2px", outlineColor: "rgb(9, 111, 145)", backgroundColor: "rgb(251, 253, 255)", boxShadow: "none" });
    await page.evaluate(() => delete document.documentElement.dataset.skyTheme);
    await Promise.all([
      page.waitForResponse(response => response.url().includes("/destination/create") && response.request().method() === "PUT"),
      menuName.press("Tab")
    ]);

    await page.reload();
    await expect(page.getByLabel("Menu name")).toHaveValue(savedName);
    await page.getByRole("button", { name: "Create menu" }).click();
    await expect(page.getByTestId("menu-import-complete")).toBeVisible();
    await expect(page.getByText("Not live yet", { exact: true })).toBeVisible();
    await expect(page.getByText("Published screens changed")).toBeVisible();
    await expect(page.getByText("0", { exact: true })).toBeVisible();
    await page.reload();
    await expect(page.getByTestId("menu-import-complete")).toBeVisible();
    await expect(page.getByRole("button", { name: "Review draft in builder" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Done for now" })).toBeVisible();
    await page.setViewportSize({ width: 1920, height: 1080 });
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= document.documentElement.clientWidth), "the wide completion must not overflow horizontally").toBe(true);
  });

  test("below 900px refuses compression and gives a return path", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "mobile", "This is the explicit below-supported-width state.");
    await openAs(page, "owner", "/menu/import");
    await expect(page.getByTestId("menu-import-narrow")).toBeVisible();
    await expect(page.getByRole("heading", { name: "Importing a menu needs a wider window" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Back to menus" })).toBeVisible();
  });

  test("replaces an existing menu as an unpublished draft and resumes the selected target",async({page},testInfo)=>{
    test.skip(testInfo.project.name!=="desktop","The replacement workflow has a separate below-900 refusal.");
    const seeded=await seed({label:"paste-replace"});await openAs(page,"owner","/menu/import");
    await page.getByLabel("Menu text").fill(`REPLACEMENT\nNew ${seeded.menuName} special  19`);await page.getByRole("button",{name:"Read menu"}).click();
    await expect(page.getByRole("heading",{name:"Where should these items go?"})).toBeVisible();
    await page.getByRole("button",{name:new RegExp(seeded.menuName)}).click();
    await expect(page.getByRole("heading",{name:`Replace ${seeded.menuName}?`})).toBeVisible();
    await expect(page.getByText("publishing remains a separate action",{exact:false})).toBeVisible();
    await expect(page.getByText("screens change now")).toBeVisible();await expect(page.getByText(/\d+ items? added · \d+ removed · \d+ changed/)).toBeVisible();await expect(page.getByText(/unpublished (change|changes) already present/)).toBeVisible();await page.reload();
    await expect(page.getByRole("heading",{name:`Replace ${seeded.menuName}?`})).toBeVisible();
    await page.getByRole("button",{name:"Replace menu"}).click();await expect(page.getByTestId("menu-import-complete")).toBeVisible();
    await expect(page.getByText("Not live yet",{exact:true})).toBeVisible();await expect(page.getByText("Published screens changed")).toBeVisible();
    await page.getByRole("button",{name:"Restore the draft from before this import"}).click();await expect(page.getByText("Restore the draft from before this import?")).toBeVisible();
    await page.getByRole("button",{name:"Restore previous draft"}).click();await expect(page.getByRole("status")).toContainText("working draft from before this import has been restored");
  });

  test("thirty semantic near misses render as one group with no preselection", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    const seeded = await seed({ label: "near-match-group" });
    const typo = `${seeded.itemName.slice(0, -1)}x`;
    await openAs(page, "owner", "/menu/import");
    await page.getByLabel("Menu text").fill(Array.from({ length: 30 }, () => `${typo}  ${seeded.itemPrice}`).join("\n"));
    await page.getByRole("button", { name: "Read menu" }).click();

    const group = page.getByTestId("near-match-group");
    await expect(group).toHaveCount(1);
    await expect(group).toContainText("30 pasted items have a similar library name");
    await expect(group.getByRole("button", { name: new RegExp(seeded.itemName, "i") })).toHaveCount(30);
    await expect(page.getByTestId("safe-match-banner")).toHaveCount(0);
    await expect(page.getByText("Nothing is selected for you.")).toBeVisible();
  });

  test("an unreadable line can be left out, and the choices name outcomes not mechanisms", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    await seed({ label: "leave-out" });
    await openAs(page, "owner", "/menu/import");

    await page.getByLabel("Menu text").fill("Ossobuco  24\nA line nobody can read");
    await page.getByRole("button", { name: "Read menu" }).click();
    await expect(page.getByTestId("menu-import-review")).toBeVisible();

    // Decision 10 - never a bare action; it states what replaces it, in the same click. The
    // mechanism-named choice this replaced ("Keep in Imported items") told a first-time operator
    // nothing about what it does or where the thing goes.
    const row = page.getByTestId("question-row").filter({ hasText: "A line nobody can read" });
    await expect(row.getByTestId("answer-section")).toContainText("A section heading");
    await expect(row.getByTestId("answer-dish")).toContainText("Goes in an Imported items group");
    await expect(page.getByText("Keep in Imported items")).toHaveCount(0);

    // The third answer the design always specified, and the only one never built.
    await row.getByTestId("answer-leave-out").click();
    await expect(page.getByRole("heading", { name: "Where should these items go?" })).toBeVisible();
  });

  test("one decision is one row, so a screen holds several", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    await seed({ label: "row-density" });
    await openAs(page, "owner", "/menu/import");

    await page.getByLabel("Menu text").fill("Ossobuco  24\nfirst unreadable line\nsecond unreadable line\nthird unreadable line");
    await page.getByRole("button", { name: "Read menu" }).click();

    const rows = page.getByTestId("question-row");
    await expect(rows).toHaveCount(3);
    // The card this replaced ran to roughly 280px for one decision. Three of those did not share a
    // screen; three rows must.
    const heights = await rows.evaluateAll(nodes => nodes.map(node => node.getBoundingClientRect().height));
    expect(Math.max(...heights), `row heights: ${heights.join(", ")}`).toBeLessThan(180);
  });

  test("reading a menu draws the wait over the page, not instead of it", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    await seed({ label: "reading-state" });
    await openAs(page, "owner", "/menu/import");

    // Ten silent seconds behind an unchanged screen reads as a button that did nothing, which is
    // what the owner reported. Held here so the wait can never quietly become invisible again.
    await page.route("**/api/back-office/menu-imports", async route => {
      if (route.request().method() !== "POST") return route.fallback();
      await new Promise(resolve => setTimeout(resolve, 2500));
      await route.fallback();
    });

    await page.getByLabel("Menu text").fill("Ossobuco  24");
    await page.getByRole("button", { name: "Read menu" }).click();

    // Over the page, not instead of it: what you pasted is still there behind the animation.
    await expect(page.getByText("Reading your menu")).toBeVisible();
    await expect(page.locator(".vennu-loader--modal")).toBeVisible();
    await expect(page.getByLabel("Menu text")).toHaveValue("Ossobuco  24");
    await expect(page.getByTestId("menu-import-review")).toBeVisible({ timeout: 30_000 });
  });

  test("applying a suggestion is visibly doing something, even when it is quick", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    await seed({ label: "apply-visible" });
    await openAs(page, "owner", "/menu/import");

    await page.getByLabel("Menu text").fill("Ossobuco  24\nA line nobody can read");
    await page.getByRole("button", { name: "Read menu" }).click();
    await expect(page.getByTestId("menu-import-review")).toBeVisible();

    const banner = page.getByTestId("import-suggestion");
    if (await banner.count() === 0) test.skip(true, "No suggestion offered - the residue pass needs a key on this environment.");

    // The animation was already correct and finished before it could be seen, which reads as a
    // button that does nothing. It is held open long enough to have been shown.
    await page.getByTestId("suggestion-accept").click();
    await expect(page.getByTestId("suggestion-applying")).toBeVisible();
  });

  test("an accepted suggestion fills the menu name", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    await seed({ label: "suggested-name" });
    await openAs(page, "owner", "/menu/import");

    await page.getByLabel("Menu text").fill("Ossobuco  24\nA line nobody can read");
    await page.getByRole("button", { name: "Read menu" }).click();
    await expect(page.getByTestId("menu-import-review")).toBeVisible();

    const banner = page.getByTestId("import-suggestion");
    if (await banner.count() === 0) test.skip(true, "No suggestion offered - the residue pass needs a key on this environment.");
    const suggested = (await banner.getByRole("heading").innerText()).replace(/^Is this menu called “|”\?$/g, "");

    await page.getByTestId("suggestion-accept").click();
    await expect(page.getByRole("heading", { name: "Where should these items go?" })).toBeVisible();
    // The whole point of the feature. `suggestedMenuName` and `proposedMenuName` are unrelated
    // server-side, and accepting used to set neither - so the name the banner had just offered went
    // nowhere and this field still read "New menu".
    await expect(page.getByLabel("Menu name")).toHaveValue(suggested);
    await expect(page.getByLabel("Menu name")).not.toHaveValue("New menu");
  });

  test("confirming a name you just typed does not conflict with yourself", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    const seeded = await seed({ label: "self-conflict" });
    await openAs(page, "owner", "/menu/import");

    await page.getByLabel("Menu text").fill(`${seeded.itemName.replaceAll(" ", "   ")}  ${seeded.itemPrice}`);
    await page.getByRole("button", { name: "Read menu" }).click();
    await expect(page.getByRole("heading", { name: "Where should these items go?" })).toBeVisible();
    await page.getByRole("button", { name: "Create a new menu" }).click();

    /*
     * The name field saves on blur, which bumps the session revision. Submitting with the keyboard
     * blurs the field first, and the submit handler used to carry the revision as it stood before
     * that - so confirming a name you had just typed came back "This import changed in another
     * window" with only one window open. Keyboard, deliberately: the mouse path suppresses the
     * blur and hides it.
     */
    const name = page.getByLabel("Menu name");
    await name.fill(`Self conflict ${Date.now()}`);
    await name.press("Enter");

    await expect(page.getByText("changed in another window")).toHaveCount(0);
    await expect(page.getByTestId("menu-import-complete")).toBeVisible({ timeout: 30_000 });
  });

  test("a resume that fails says why, and offers a way on", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The review workflow has a separate below-900 refusal.");
    await openAs(page, "owner", "/menu/import");

    // A bodyless failure is the case that used to say "This import is unavailable" and nothing
    // else - no reason, no way forward. It is also the one you get when the app restarts under you.
    await page.route("**/api/back-office/menu-imports/*", route =>
      route.request().method() === "GET" ? route.fulfill({ status: 503, body: "" }) : route.fallback());

    await page.goto("https://localhost:5175/#/menu/import/11111111-1111-1111-1111-111111111111".replace("https://localhost:5175", new URL(page.url()).origin));

    const reason = page.getByTestId("import-unavailable-reason");
    await expect(reason).toBeVisible();
    await expect(reason).toContainText("error 503");
    await expect(reason).toContainText("still saved");
    await expect(page.getByTestId("import-retry")).toBeVisible();
    await expect(page.getByRole("button", { name: "Back to menus" })).toBeVisible();
  });
});

import { test, expect, openAs } from "../fixtures";
import { seed } from "../seed";

test.describe("paste import review", () => {
  test("reads, saves, resumes, accepts only a safe match, and resolves the remaining line", async ({ page }, testInfo) => {
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
    await page.getByRole("button", { name: "Keep in Imported items" }).click();
    await expect(page.getByTestId("import-review-complete")).toBeVisible();
    await expect(page.getByText("No menu has been changed.")).toBeVisible();

    await page.reload();
    await expect(page.getByTestId("import-review-complete")).toBeVisible();
    await page.setViewportSize({ width: 1920, height: 1080 });
    await expect(page.getByTestId("import-review-complete")).toBeVisible();
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= document.documentElement.clientWidth), "the wide review must not overflow horizontally").toBe(true);
  });

  test("below 900px refuses compression and gives a return path", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== "mobile", "This is the explicit below-supported-width state.");
    await openAs(page, "owner", "/menu/import");
    await expect(page.getByTestId("menu-import-narrow")).toBeVisible();
    await expect(page.getByRole("heading", { name: "Importing a menu needs a wider window" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Back to menus" })).toBeVisible();
  });
});

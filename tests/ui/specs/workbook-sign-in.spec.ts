import { test, expect } from "../fixtures";

/**
 * The owner acceptance workbook offers one-click sign-in links per role. They hand the
 * configured token over in the query string; the Back Office must consume it, sign in,
 * and strip it from the URL so nothing is left in the address bar or a copied link.
 */
const roles = [
  { token: "track1-owner-review", displayName: "Track 1 Owner Review" },
  { token: "track1-content-editor", displayName: "Track 1 Content Editor" },
  { token: "track1-publisher", displayName: "Track 1 Publisher" }
];

for (const role of roles) {
  test(`workbook sign-in link signs in as ${role.displayName}`, async ({ page }) => {
    await page.goto(`/?accessToken=${role.token}#/home`);

    // Signed in as the intended identity, with no venue picker in the way.
    await expect(page.locator("body")).toContainText(role.displayName);
    await expect(page.locator('[data-testid="nav-item"]').first()).toBeAttached();

    // The token must not survive in the URL.
    expect(page.url(), "token must be stripped from the address bar").not.toContain("accessToken");
    expect(page.url()).not.toContain(role.token);

    // It must survive a reload, proving it reached session storage rather than
    // being read from the query string on every render.
    await page.reload();
    await expect(page.locator("body")).toContainText(role.displayName);
  });
}

test("an unknown token in the link does not sign anyone in", async ({ page }) => {
  await page.goto("/?accessToken=not-a-configured-token#/home");
  await expect(page.locator("body")).not.toContainText("Track 1 Owner Review");
  expect(page.url()).not.toContain("accessToken");
});

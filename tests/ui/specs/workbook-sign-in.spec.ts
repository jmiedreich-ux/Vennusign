import { test, expect } from "../fixtures";

/**
 * The owner acceptance workbook offers one-click sign-in links per role. The token
 * travels in the URL *fragment*, which browsers never transmit, so it must never
 * appear in any request the page makes. Asserting only that the address bar ends
 * clean is not enough: a query string would already have been sent.
 */
const roles = [
  { token: "track1-owner-review", displayName: "Track 1 Owner Review" },
  { token: "track1-content-editor", displayName: "Track 1 Content Editor" },
  { token: "track1-publisher", displayName: "Track 1 Publisher" }
];

for (const role of roles) {
  test(`workbook sign-in link signs in as ${role.displayName} without transmitting the token`, async ({ page }) => {
    const leaked: string[] = [];
    page.on("request", request => {
      if (request.url().includes(role.token)) leaked.push(request.url());
    });

    await page.goto(`/#/home?accessToken=${role.token}`);

    await expect(page.locator("body")).toContainText(role.displayName);
    await expect(page.locator('[data-testid="nav-item"]').first()).toBeAttached();

    // The credential must never reach the wire, not merely be tidied up afterwards.
    expect(leaked, "the token must never appear in a request URL").toHaveLength(0);

    // And it must not be left in the address bar or a copyable link.
    expect(page.url()).not.toContain("accessToken");
    expect(page.url()).not.toContain(role.token);

    // The route in the fragment must survive having the token stripped out of it.
    expect(page.url()).toContain("#/home");

    // It must survive a reload, proving it reached session storage.
    await page.reload();
    await expect(page.locator("body")).toContainText(role.displayName);
    expect(leaked, "a reload must not resend the token either").toHaveLength(0);
  });
}

test("an unknown token in the link does not sign anyone in", async ({ page }) => {
  await page.goto("/#/home?accessToken=not-a-configured-token");
  await expect(page.locator("body")).not.toContainText("Track 1 Owner Review");
  expect(page.url()).not.toContain("accessToken");
});

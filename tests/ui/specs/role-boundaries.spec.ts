import { test, expect, openAs } from "../fixtures";

/**
 * Workbook cases 2-0 and 2-1 - Content Editor and Publisher boundaries.
 *
 * QA judged these "Manual Review" twice before an agent could decide them. They are
 * fully mechanical: a boundary is a locked nav item plus a refusal panel that appears
 * without the client issuing the underlying request.
 */
const lockedFor: Record<"editor" | "publisher", string[]> = {
  // Screens/schedules/billing/security were all observed locked for the editor.
  editor: ["screens", "billing", "security"],
  publisher: ["billing", "security"]
};

for (const role of ["editor", "publisher"] as const) {
  test(`${role} sees locked navigation for routes it cannot open`, async ({ page }) => {
    await openAs(page, role, "home");

    for (const route of lockedFor[role]) {
      const item = page.locator(`[data-testid="nav-item"][data-route="${route}"]`);
      await expect(item, `${role} should see ${route} locked`).toHaveAttribute("data-unlocked", "false");
      await expect(item).toHaveAttribute("aria-disabled", "true");
    }
  });

  test(`${role} is refused before any request when opening a locked route`, async ({ page }) => {
    const route = lockedFor[role][0];
    const requests: string[] = [];
    page.on("request", request => {
      // Only venue-scoped area calls belong to a route. Billing presentation is
      // fetched shell-wide for upgrade prompts regardless of the current route,
      // so matching on the bare route name would flag legitimate traffic.
      const url = request.url();
      if (url.includes("/api/back-office/venues/") && url.includes(`/${route}`)) requests.push(url);
    });

    await openAs(page, role, route);

    // The refusal must be rendered from the session decision, not discovered by
    // issuing the request and handling a 403.
    const panel = page.getByTestId("locked-panel");
    await expect(panel).toBeVisible();
    await expect(panel).toHaveAttribute("data-route", route);
    expect(requests, "a refused route must not issue its API request").toHaveLength(0);
  });
}

test("owner is not locked out of the routes the roles are", async ({ page }) => {
  await openAs(page, "owner", "home");

  for (const route of ["screens", "menu", "billing"]) {
    await expect(
      page.locator(`[data-testid="nav-item"][data-route="${route}"]`)
    ).toHaveAttribute("data-unlocked", "true");
  }
});

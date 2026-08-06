import { test, expect, openAs, apiBaseUrl, tokens } from "../fixtures";
import { seed } from "../seed";

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

/**
 * The substance of case 2-1. The publisher's boundary is not a locked route - Screens
 * opens - so it has to be expressed per action. Owner acceptance recorded this case as
 * "very confusing", and underneath the confusion was a real defect: Push rendered enabled
 * for a role the server denies `screen.content.target`, so the refusal could only be
 * discovered by pressing it.
 */
test("publisher opens Screens but is refused targeting before selecting it", async ({ page }) => {
  const pushed: string[] = [];
  page.on("request", request => {
    if (request.method() === "POST" && request.url().includes("/push")) pushed.push(request.url());
  });

  await openAs(page, "publisher", "screens");

  // The route genuinely opens; this is not the editor's locked panel.
  await expect(page.getByTestId("locked-panel")).toHaveCount(0);
  await expect(page.getByTestId("screen-fleet-count")).toBeVisible();

  // A permission refusal must not be dressed up as an allowance. The publisher was never
  // told "1 of 1 active screens" - that wording belongs to the owner's allowance case.
  const quota = page.getByTestId("screen-quota");
  await expect(quota).toHaveAttribute("data-limit-reached", "false");
  await expect(quota).not.toContainText(/\d+ of \d+ active screens/);

  const restrictions = page.getByTestId("screen-action-restrictions");
  await expect(restrictions, "denied actions must be named up front").toBeVisible();
  await expect(restrictions).toContainText("Push");

  const card = page.getByTestId("screen-card").first();
  await expect(card.getByTestId("screen-push")).toBeDisabled();
  await expect(card.getByTestId("screen-push")).toHaveAttribute("aria-describedby", "screen-action-restrictions");
  await expect(card.getByTestId("screen-preview"), "preview stays available to a publisher").toBeEnabled();

  await card.getByTestId("screen-more-actions").locator("summary").click();
  await expect(card.getByTestId("screen-unpair"), "unpair is denied to a publisher").toBeDisabled();
  await expect(card.getByTestId("screen-reset"), "recovery stays available to a publisher").toBeEnabled();

  expect(pushed, "a disabled action must never reach the API").toHaveLength(0);
});

/**
 * The button state above is a courtesy; the server is the authority. Before this fix
 * `screen.content.target` was declared, denied in the session payload, and enforced
 * nowhere, so a publisher could push by calling the endpoint directly.
 */
test("the API refuses a publisher push made directly, not just the button", async ({ request }) => {
  const data = await seed({ role: "owner", includeScreen: true, label: "pubpush" });

  const response = await request.post(
    `${apiBaseUrl}/api/back-office/venues/${data.venueId}/screens/${data.screenId}/push`,
    { headers: { "X-Vennusign-Back-Office-Token": tokens.publisher }, ignoreHTTPSErrors: true }
  );

  expect(response.status(), "a denied capability must be enforced server-side").toBe(403);
  const body = await response.json();
  expect(body.capabilityId).toBe("screen.content.target");
  expect(body.message, "the refusal must explain itself").toBeTruthy();
});

test("owner is not locked out of the routes the roles are", async ({ page }) => {
  await openAs(page, "owner", "home");

  for (const route of ["screens", "menu", "billing"]) {
    await expect(
      page.locator(`[data-testid="nav-item"][data-route="${route}"]`)
    ).toHaveAttribute("data-unlocked", "true");
  }
});

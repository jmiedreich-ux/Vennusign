import { test, expect, openMenuBuilderAs } from "../fixtures";
import { seed } from "../seed";
import type { Locator, Page } from "@playwright/test";

async function mouseClick(page: Page, target: Locator) {
  await target.scrollIntoViewIfNeeded();
  const box = await target.boundingBox();
  expect(box).not.toBeNull();
  await page.mouse.click(box!.x + box!.width / 2, box!.y + box!.height / 2);
}

async function expectFitted(page: Page) {
  const canvas = page.getByTestId("canvas");
  const frame = canvas.getByTestId("board-frame");
  await expect(frame).toBeVisible();
  const [canvasBox, frameBox, boardBox, scales] = await Promise.all([
    canvas.boundingBox(), frame.boundingBox(), frame.getByTestId("board").boundingBox(),
    frame.evaluate(element => ({ fit: Number((element as HTMLElement).dataset.fitScale), actual: Number((element as HTMLElement).dataset.boardScale) }))
  ]);
  expect(canvasBox).not.toBeNull();
  expect(frameBox).not.toBeNull();
  expect(boardBox).not.toBeNull();
  expect(scales.fit).toBeGreaterThan(0);
  expect(scales.actual).toBeCloseTo(scales.fit, 5);
  expect(boardBox!.x).toBeGreaterThanOrEqual(frameBox!.x - 1);
  expect(boardBox!.y).toBeGreaterThanOrEqual(frameBox!.y - 1);
  expect(boardBox!.x + boardBox!.width).toBeLessThanOrEqual(frameBox!.x + frameBox!.width + 1);
  expect(boardBox!.y + boardBox!.height).toBeLessThanOrEqual(frameBox!.y + frameBox!.height + 1);
}

for (const viewport of [
  { name: "compact desktop", width: 1280, height: 720 },
  { name: "standard desktop", width: 1440, height: 900 },
  { name: "large desktop", width: 1920, height: 1080 }
]) {
  test(`Northside whole page fits at ${viewport.name}`, async ({ page }) => {
    await page.setViewportSize({ width: viewport.width, height: viewport.height });
    const data = await seed({ showcase: "northside-social", label: `northside-${viewport.width}` });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await expect(page.getByTestId("zoom-controls")).toBeVisible();
    await expectFitted(page);
  });
}

test("Northside real-mouse editing, page switching, zoom, alignment, and persistence", async ({ page }) => {
  test.setTimeout(120_000);
  await page.setViewportSize({ width: 1440, height: 900 });
  const data = await seed({ showcase: "northside-social", label: "northside-exhaustive" });
  await openMenuBuilderAs(page, "owner", data.menuId);

  const canvas = page.getByTestId("canvas");
  const headings = canvas.locator(".board-section-heading");
  const items = canvas.getByTestId("board-item");
  await expect(headings).toHaveCount(2);
  await expect(items).toHaveCount(16);

  for (let index = 0; index < await headings.count(); index++) {
    const target = headings.nth(index);
    await mouseClick(page, target);
    const editor = page.getByTestId("heading-edit");
    await expect(editor).toBeFocused();
    const [targetBox, editorBox] = await Promise.all([target.boundingBox(), editor.boundingBox()]);
    expect(Math.abs(editorBox!.x - targetBox!.x)).toBeLessThanOrEqual(10);
    expect(Math.abs(editorBox!.y - targetBox!.y)).toBeLessThanOrEqual(4);
    await editor.press("Escape");
  }

  for (let index = 0; index < await items.count(); index++) {
    for (const [selector, editorId] of [[".board-item-name", "name-edit"], [".board-item-description", "description-edit"], [".board-item-price", "price-edit"]] as const) {
      const target = items.nth(index).locator(selector);
      await mouseClick(page, target);
      const editor = page.getByTestId(editorId);
      await expect(editor).toBeFocused();
      const [targetBox, editorBox] = await Promise.all([target.boundingBox(), editor.boundingBox()]);
      expect(Math.abs(editorBox!.x - targetBox!.x)).toBeLessThanOrEqual(10);
      expect(Math.abs(editorBox!.y - targetBox!.y)).toBeLessThanOrEqual(4);
      await editor.press("Escape");
    }
  }

  const firstItem = items.first();
  await mouseClick(page, firstItem.locator(".board-item-name"));
  await page.getByTestId("name-edit").fill("Pilsner Reserve");
  await page.getByTestId("name-edit").press("Enter");
  await mouseClick(page, firstItem.locator(".board-item-description"));
  await page.getByTestId("description-edit").fill("Northside test batch");
  await page.getByTestId("description-edit").blur();
  await mouseClick(page, firstItem.locator(".board-item-price"));
  await page.getByTestId("price-edit").fill("6.75");
  await page.getByTestId("price-edit").press("Enter");

  const selectedId = await canvas.getAttribute("data-selected-item");
  expect(selectedId).toBeTruthy();
  const frame = canvas.getByTestId("board-frame");
  const fitScale = Number(await frame.getAttribute("data-fit-scale"));
  await mouseClick(page, page.getByRole("button", { name: "Zoom in" }));
  expect(Number(await frame.getAttribute("data-board-scale"))).toBeCloseTo(fitScale * 1.25, 4);
  await expect(canvas).toHaveAttribute("data-selected-item", selectedId!);
  await mouseClick(page, page.getByRole("button", { name: "Zoom out" }));
  await mouseClick(page, page.getByRole("button", { name: "Zoom out" }));
  expect(Number(await frame.getAttribute("data-board-scale"))).toBeCloseTo(fitScale * 0.75, 4);
  await expect(canvas).toHaveAttribute("data-selected-item", selectedId!);
  await mouseClick(page, page.getByRole("button", { name: "Fit canvas" }));
  await expect(canvas).toHaveAttribute("data-selected-item", selectedId!);
  await expectFitted(page);

  for (const name of ["Wine", "Cocktails", "Beer"]) {
    await mouseClick(page, page.getByTestId("page-tab").filter({ hasText: name }));
    await expect(page.getByTestId("page-name")).toHaveText(name);
  }

  await page.reload();
  await expect(canvas).toContainText("Pilsner Reserve");
  await expect(canvas).toContainText("Northside test batch");
  await expect(canvas).toContainText("6.75");
});

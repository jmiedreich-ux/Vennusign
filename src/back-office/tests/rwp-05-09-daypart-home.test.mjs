import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

test("daypart home composes venue-authoritative operations and essential states", async () => {
  const home = await readFile(new URL("../src/DaypartHome.tsx", import.meta.url), "utf8");
  const app = await readFile(new URL("../src/App.tsx", import.meta.url), "utf8");
  const styles = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");

  assert.match(home, /Promise\.all\(\[\s*loadMealPeriods/);
  assert.match(home, /loadManagedScreens/);
  assert.match(home, /loadMenuEditor/);
  assert.match(home, /updateQuickAvailability/);
  assert.match(home, /updateQuickDailySpecial/);
  assert.match(home, /\?schedule=emergency#\/schedules/);
  assert.match(home, /role="alert"/);
  assert.match(home, /role="status"/);
  assert.match(app, /backOfficeNavigationGroups\.map/);
  assert.match(styles, /@media \(max-width: 860px\)/);
});

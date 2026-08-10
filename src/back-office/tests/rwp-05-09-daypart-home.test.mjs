import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

test("daypart home composes venue-authoritative operations and essential states", async () => {
  const home = await readFile(new URL("../src/DaypartHome.tsx", import.meta.url), "utf8");
  const app = await readFile(new URL("../src/App.tsx", import.meta.url), "utf8");
  const styles = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");

  assert.match(home, /Promise\.all\(\[/);
  assert.match(home, /loadManagedScreens/);
  assert.match(home, /loadMenuEditor/);
  assert.match(home, /capabilities\.includes\("schedule\.entry\.manage"\) \? loadMealPeriods/);
  assert.match(home, /capabilities\.includes\("screen\.device\.view"\) \? loadManagedScreens/);
  assert.match(home, /capabilities\.includes\("content\.item\.update"\) \? loadMenuEditor/);
  assert.match(home, /updateQuickAvailability/);
  // Today's special is an owner-killed concept (Q35): Home carries no widget
  // wired to it any more.
  assert.doesNotMatch(home, /updateQuickDailySpecial/);
  assert.match(home, /\?schedule=emergency#\/schedules/);
  assert.match(home, /role="alert"/);
  assert.match(home, /role="status"/);
  // The shell draws the 76px rail now rather than the grouped sidebar, so the
  // areas come from the rail's own sections; Home is still reached from it.
  assert.match(app, /<NavRail/);
  assert.match(styles, /@media \(max-width: 860px\)/);
});

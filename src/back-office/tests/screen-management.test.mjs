import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [app, operations, screens, walls, api] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/VideoWallBuilder.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8")
]);

test("venue operations compose screen targeting pairing and video walls", () => {
  assert.match(operations, /<ScreenManagement/);
  assert.match(screens, /claimPairingCode/);
  assert.match(screens, /Select one screen target/);
  assert.match(screens, /selectedScreenId/);
  assert.match(screens, /Push structured content/);
  assert.match(screens, /acknowledgement pending/);
  assert.match(screens, /player stale\/offline/);
  assert.match(screens, /window\.setInterval\(poll, 10_000\)/);
  assert.doesNotMatch(screens, /pushAllManagedScreens/);
  assert.match(screens, /loadScreenOverflow/);
  assert.match(walls, /saveVideoWall/);
  assert.match(api, /api\/back-office\/screens\/pairing/);
});

test("screen creation and pairing use the structured server allowance decision", () => {
  assert.match(app, /decisions=\{session\.capabilityDecisions\}/);
  assert.match(operations, /decision\.capabilityId === "screen\.device\.pair"/);
  assert.match(screens, /pairDecision\?\.reasonCode === "allowance\.reached"/);
  assert.match(screens, /pairDecision\?\.parameters\.used/);
  assert.match(screens, /pairDecision\?\.parameters\.limit/);
  assert.match(screens, /busyId === "new" \|\| !pairingAllowed/);
  assert.match(screens, /busyId === "pair" \|\| !pairingAllowed/);
  assert.match(screens, /reason\.status === 409/);
  assert.doesNotMatch(app, /maxScreens=\{billing\?\.currentTier\?\.maxScreens\}/);
});

test("screen lifecycle recovery is explicit safe and capacity-aware", () => {
  assert.match(screens, /setManagedScreenArchived/);
  assert.match(screens, /resetManagedScreen/);
  assert.match(screens, /unpairManagedScreen/);
  assert.match(screens, /useDestructiveReview/);
  assert.match(screens, /healthFilter/);
  assert.match(screens, /expired/);
  assert.match(screens, /already claimed/);
  assert.match(api, /setManagedScreenArchived/);
  assert.match(api, /unpairManagedScreen/);
});

test("screen replacement preserves a logical screen through preview and confirmation", () => {
  assert.match(screens, /Replace a player/);
  assert.match(screens, /Review replacement/);
  assert.match(screens, /Confirm player replacement/);
  assert.match(screens, /previewScreenReplacement/);
  assert.match(screens, /completeScreenReplacement/);
  assert.match(screens, /old player credential will stop working immediately/);
  assert.match(screens, /Unpair screen/);
  assert.doesNotMatch(screens, /Unpair for replacement/);
});

test("content delivery distinguishes request receipt application recovery and failure", () => {
  assert.match(screens, /authoritativeRevision/);
  assert.match(screens, /appliedRevision/);
  assert.match(screens, /deliveryState/);
  assert.match(screens, /acknowledgement pending/);
  assert.match(screens, /deliveryFailureCode/);
  assert.doesNotMatch(screens, /future acknowledgement contract/);
});

test("screen actions expose deliberate preview and identity save cancellation", () => {
  assert.match(screens, /Preview selected screen/);
  assert.match(screens, /Close preview/);
  assert.match(screens, /Unsaved screen identity changes/);
  assert.match(screens, /Save changes/);
  assert.match(screens, /Cancel changes/);
  assert.doesNotMatch(screens, /onBlur=\{\(\) => save\(screen\)\}/);
});

test("screens workflows separate daily setup and capacity work", () => {
  assert.match(screens, />Setup</);
  assert.match(screens, />Daily</);
  assert.match(screens, /Capacity &amp; walls/);
  assert.match(screens, /open=\{setupOpen\}/);
  assert.match(screens, /setSetupOpen\(!current\.some/);
  assert.match(screens, /await refresh\(\); setSetupOpen\(false\)/);
  assert.match(screens, /Collapsed after your first active screen/);
});

test("layout controls stay draft-only until the operator applies them", () => {
  assert.match(screens, /presentationDrafts/);
  assert.match(screens, /screenPresentationHasChanges/);
  assert.match(screens, /Nothing changes on the TV until you apply/);
  assert.match(screens, />Apply to TV</);
  assert.match(screens, />Discard changes</);
  assert.doesNotMatch(screens, /onChange=\{event => \{[\s\S]{0,180}void save\(updated\)/);
});

test("video wall editing and removal require deliberate recovery-safe actions", () => {
  assert.match(walls, /editingName/);
  assert.match(walls, /Edit wall/);
  assert.match(walls, /useDestructiveReview/);
  assert.match(walls, /Cancel edit/);
});

test("video wall builder follows the typed screen wall capability", () => {
  assert.match(operations, /capabilities\.includes\("screen\.wall\.coordinate"\)/);
  assert.match(screens, /videoWallEnabled \? <VideoWallBuilder/);
});

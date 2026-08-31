# The Wall Planner — how a release is spread across a group of screens

- **Status:** Owner-approved design, 2026-08-29. Not an implementation milestone.
- **Area:** Screens / Display Delivery (the display side of the Content Platform).
- **Builds when:** the planner module and its fixture lab are pure TypeScript with no delivery
  dependencies, so they can be built once the Content Release / `RenderedPage` contract is settled
  (renewal planning session 4, M2) and land with the renewal **M4** renderer work. Wiring it into
  delivery — screen groups, the schedule in the package, delivery states, the player — is the
  renewal's screens / player-packaging milestone (**M5** as currently sequenced; the order is still
  open, renewal §13.8). See `docs/architecture/content-platform-architecture-renewal.md` (issue
  #939; on branch `feature/content-platform-m0-architecture-renewal` at the time of writing) and
  the Theme Studio bundle (`docs/design/theme-studio/` on branch `agent/theme-studio-handoff`).
- **Supersedes:** the "video wall" meaning of `Screens.WallGroup` / `WallPosition`; the packing in
  `DisplayController.ComputeFrameStarts` / `SliceSections`; Menus M10 T2 as a harness over that
  packing; Menus Q54 (independent page-cycling per TV, 2026-08-07 — "lockstep sync is a later
  nicety"; this is that later).
- **Does not touch:** how many items fit on one screen. That is the theme definition's capacity and
  the shared renderer's job (Theme Studio). This document starts where the renderer stops.

Plain summary: a restaurant's menu is rendered into pages by the theme. The Wall Planner takes
that ordered list of pages and a group of screens the operator has arranged in order, and works out
which page each screen shows in each time slot, so that a section flows left to right across the
wall, spills back to screen 1 when the wall runs out, and every screen flips at the same moment —
with no server needed to keep it running.

---

## 1. Decisions settled 2026-08-29

Each is a rule. Where later work disagrees with a rule here, the rule wins until the owner changes it.

| # | Decision |
|---|---|
| **WP-1** | **A screen group is a thing the operator makes in Back Office**: a named set of screens in an explicit left-to-right order, and a release is assigned to the group. It is not inferred from which screens share a menu, and the menu does not say which screen a page goes to. (`Screens.WallGroup` + `WallPosition` and the Back Office video-wall editor already hold this data; their *meaning* changes — see §9, §11. Per the Engineering Bible §9.1 a wall position is where a screen is installed, never which player output it is.) |
| **WP-2** | **Priority order when goals conflict: structure > same text size > avoid rotation > balance.** The operator's pages and sections stay as named and ordered; text is the same size on every screen (Menus M10 T1, Option A); rotation is avoided where it can be; "every screen looks equally full" is the first thing sacrificed. The planner enforces the first and third by construction (§4); the second is a constraint on themes published for wall use (TS-C3, §9). |
| **WP-3** | **When a section has more pages than the wall, only the spill rotates.** Apps 3 pages on 2 screens: screen 1 alternates p1 → p3, screen 2 holds p2. Later sections join the same rotation as more pages. (Rule "X" in the owner review; the whole-wall slide "Y" and last-screen overflow "Z" were rejected.) |
| **WP-4** | **Every screen in a group shows the same number of items, drawn the same way.** A bigger TV shows bigger text; a smaller one smaller. No correction for physical size, no per-TV capacity. If physical-size matching is ever wanted, it is a change to how a screen's capacity is worked out, not to this planner. |
| **WP-5** | **Build the planner as a Content Platform piece, not into the current C# display path.** Design now, build as a pure TypeScript module with fixture tests alongside the M4 renderer work, wire it into delivery in the renewal's screens / player-packaging milestone (M5 as sequenced; order open). No planner code is written against `DisplayController` — it would be retired with it. |
| **WP-6** | **A screen with nothing new to show holds what it was showing** (same page id, so no fade). |
| **WP-7** | **A screen with nothing to hold** (spare screens in the first slot) shows the theme's **filler page** if the definition declares one, otherwise it **mirrors page 1**. A one-page menu on a three-screen wall shows the same page three times. *Owner judgment call recorded; may be revisited when the first real wall runs.* |
| **WP-8** | **A group shares one canvas format.** *Canvas format* here means Theme Studio's "fixed design surface": resolution class plus orientation (e.g. `landscape-1080p`), safe area excluded; a screen's `WidthPixels` / `HeightPixels` are the seed for deriving it. A screen whose reported geometry does not match the group's format is refused at group edit, not silently included. A portrait screen in a landscape group needs a portrait theme; mixed-orientation groups are out of scope for v1 (Q-WP-4). |
| **WP-9** | **A screen never shows a new release until its whole package is on the device and verified** — every page, image and video, checksum checked. Until then it keeps showing the last good release. A half-downloaded package is never shown, not even one page of it. |
| **WP-10** | **A wall switches together.** The server sets the release's start time only once every screen in the group has reported *received*. An offline screen does not hold the wall hostage: after a grace period the wall goes ahead, and the missing screen catches up when it returns (reported as *stale* until then). Supersedes Menus Q54. |
| **WP-11** | **The last good package never expires.** A screen keeps showing it for as long as it takes — marked *stale* in diagnostics — and only a newer, complete, verified package replaces it. The Engineering Bible already requires this ("fail safe on display by retaining the last valid state"); today's 7-day cut-off in `displayCache.mjs` violates it and is a defect — §11. |
| **WP-12** | **Every delivery step is timestamped and reportable per screen, per package** — requested, sent, downloading (with bytes), received, applied, failed — so Back Office can say "screen 2 took 3m 40s, 48 MB" and "screen 3 is still on version 11". |

---

## 2. Where it sits

The renewal's chain, with the planner's place marked:

```
content type -> data model (menu.v1) -> content instance -> theme revision
    -> immutable Content Release
        -> shared renderer: release + canvas format -> ordered PAGES        (Theme Studio owns)
        -> Wall Planner:    pages + screen group   -> WALL SCHEDULE         (this document)
        -> render package = pages + schedule + assets + version             (Display Delivery owns)
    -> screens: download whole package, verify, apply at start time, report
```

- **Language and home:** TypeScript, `packages/wall-planner`, beside the shared renderer package the
  Theme Studio plan names (`packages/canvas-renderer`, `packages/publication` —
  `THEME_STUDIO_IMPLEMENTATION_PLAN.md` §4 in the Theme Studio bundle). One implementation, called
  by: publish-time packaging (the same Node / headless-Chromium worker the publication gate already
  needs), the Back Office layout preview ("this page takes three screens; Salads is on its own" —
  the follow-on slice Menus M10 T2 names), and the fixture lab (§10). **The player never calls
  it**: it reads the pre-resolved slots in the package (§3.2, §5). Theme Studio has no screen-group
  concept today (assignment stays outside it, per its decisions); a wall preview in its test
  matrix is a request to that plan, not an assumption here. This is how Menus M10 T2's rule
  "never a JavaScript copy of the real packing" is kept: there is only one packing, and it is the
  TypeScript one.
- **Module:** Display Delivery in the modular-monolith layout (renewal §8). The schedule is part of
  `RenderPackages`; delivery states are part of `ContentDeployments`.
- **What the planner is not:** it never sees pixels, fonts, item text or images. It cannot make a
  page fit; that was decided before it runs. It is a pure function and must stay one.

---

## 3. Contract

### 3.1 Input

```ts
type PageId = string;          // stable within a release; from the renderer

interface RenderedPage {
  id: PageId;
  sourcePageId: string;        // the operator-named page this frame came from
  sectionIds: string[];        // sections with at least one item on this page, in order
  continues: boolean;          // true when this page carries on a section from the previous page
                               // (Menus Q137: heading repeats for guests; "2 OF 2" counter is
                               // back-office only — the fixture lab and layout preview show it)
}

interface ScreenGroup {
  id: string;
  canvasFormat: string;        // e.g. "landscape-1080p"; every member matches (WP-8)
  screens: { screenId: string; position: number }[];   // operator order: any distinct integers;
                               // the planner sorts ascending — gaps allowed, duplicates rejected
}

interface PlanOptions {
  releaseId: string;           // the Content Release these pages came from; copied into the schedule
  dwellSeconds: number;        // one value per release for v1 (see §12, Q-WP-3)
  fillerPageId?: PageId;       // theme-declared page for spare screens (WP-7); optional
}
```

A release yields one `RenderedPage[]` per canvas format. The planner requires at least one page;
the renderer guarantees it (an empty menu still renders the theme background and title). It also
requires a non-empty group with distinct positions, and throws otherwise — these are §10's error
cases. Column = rank of `position` after sorting, never the raw number: today's `WallPosition` is
1-based (`VideoWallService` assigns `index + 1`) and archive / unpair leave gaps, so the seed data
needs no renumbering.

### 3.2 Output

```ts
interface WallSchedule {
  plannerVersion: string;      // algorithm version; part of package identity
  releaseId: string;
  groupId: string;
  groupRevision: string;       // hash of the ordered screen ids; a re-plan is a new package
  dwellSeconds: number;
  slotCount: number;           // K
  screens: {
    screenId: string;
    position: number;
    slots: PageId[];           // length K; holds and fillers already resolved
  }[];
  cells: {                     // for preview and diagnostics only; the player ignores it
    slot: number; position: number; kind: 'page' | 'hold' | 'filler' | 'mirror';
  }[];
}
```

Pre-resolved on purpose: the player does one sum and one lookup (§5). It decides nothing.

The schedule carries **no start time**. It is fixed at publish and covered by the package
checksum. `startAt` is a group-level cutover fact owned by Display Delivery (WP-10, §5, §8): set
once the wall is ready, sent to each screen as a separate small message `{ packageId, startAt }`,
and stored by the player beside the verified package — never inside it.

---

## 4. The dealing rule

Picture the wall as a row of N screens and time as a sequence of slots. Fill from screen 1, slot 1,
moving right; when a slot is full, move to the next slot.

1. **Group pages into runs.** A run is a maximal sequence of consecutive pages where each page
   after the first has `continues = true`. A section split over three pages is a run of three; a
   page that holds several whole sections is a run of one. The run is the unit kept together —
   this is the operator's structure (WP-2).
2. **A run's pages go side by side, left to right, in order.** Never reversed, never skipped.
3. **If a run does not fit in the screens left in this slot, but would fit a fresh slot, it moves
   to the next slot.** The screens it left empty become holds (rule 5).
4. **If a run is longer than the whole wall, it spills:** it fills the rest of this slot, then
   wraps to screen 1 of the next slot, and so on (WP-3).
5. **An empty screen holds the page it showed in the previous slot** (WP-6). In the first slot
   there is nothing to hold: it shows the filler page if declared, otherwise page 1 (WP-7).
6. **All screens flip together** at every slot boundary; the wall's cycle is K slots long, then
   repeats.

N = 1 collapses to today's single-screen rotation. Rule 3 is the Menus #961 frame rule ("a section
joins the current frame if it fits; if it would fit a fresh frame the current frame closes early;
only a section larger than a whole frame is split") lifted one level, from items-in-a-frame to
pages-on-a-wall. That is why it is trusted.

### 4.1 Reference algorithm

```ts
function planWall(pages: RenderedPage[], group: ScreenGroup, opts: PlanOptions): WallSchedule {
  const ordered = [...group.screens].sort((a, b) => a.position - b.position);
  const N = ordered.length;
  if (N === 0) throw new Error('empty screen group');
  if (new Set(ordered.map(s => s.position)).size !== N) throw new Error('duplicate screen position');
  const runs = groupIntoRuns(pages);                 // rule 1
  const slots: (PageId | null)[][] = [emptySlot(N)];
  let s = 0, c = 0;                                   // cursor: slot, column

  for (const run of runs) {
    const remaining = N - c;
    if (run.length > remaining && run.length <= N) { // rule 3: defer to a fresh slot
      slots.push(emptySlot(N)); s++; c = 0;
    }
    for (const page of run) {                         // rules 2 and 4
      if (c === N) { slots.push(emptySlot(N)); s++; c = 0; }
      slots[s][c++] = page.id;
    }
  }

  const cells = [];
  for (let k = 0; k < slots.length; k++) {            // rule 5: resolve holds and fillers
    for (let p = 0; p < N; p++) {
      if (slots[k][p] !== null) { cells.push({ slot: k, position: p, kind: 'page' }); continue; }
      if (k > 0) { slots[k][p] = slots[k - 1][p]; cells.push({ slot: k, position: p, kind: 'hold' }); }
      else if (opts.fillerPageId) { slots[k][p] = opts.fillerPageId; cells.push({ slot: k, position: p, kind: 'filler' }); }
      else { slots[k][p] = pages[0].id; cells.push({ slot: k, position: p, kind: 'mirror' }); }
    }
  }

  return {
    plannerVersion: PLANNER_VERSION, releaseId: opts.releaseId, groupId: group.id,
    groupRevision: hashOf(ordered.map(s => s.screenId)),
    dwellSeconds: opts.dwellSeconds, slotCount: slots.length,
    screens: ordered.map((sc, i) => ({ ...sc, slots: slots.map(slot => slot[i]) })),
    cells,
  };
}
```

`groupIntoRuns` is a plain fold over `continues`; `emptySlot(N)` is an array of N nulls; `hashOf` is any stable hash of the ordered ids. Nothing here depends on time, randomness or
the environment: the same input always yields the same schedule, and the schedule is small enough
to be diffed by eye in a test.

### 4.2 Properties that must always hold

These become property tests, not just examples:

- Every page appears exactly once as a `page` cell. Holds, fillers and mirrors are extra showings,
  never replacements.
- Reading the `page` cells slot-major, then left to right, yields exactly the input page list in
  order — across runs as well as within them. Nothing is reordered, reversed or skipped (WP-2).
- Non-page cells only ever trail: within a slot, every `page` cell is left of every hold/filler/mirror.
- Every screen's `slots` has length K; K ≥ ⌈pages / N⌉ and K ≤ number of runs + ⌈pages / N⌉.
- With N = 1, the schedule is the page list in order, K = pages, no non-page cells.
- A hold shows the same page id as the cell above it — so the §5 fade rule ("fade only when the
  page id changes") yields no fade on a hold. That rule is new to this player; today's fades on
  the set of section ids (`DisplayLayout.tsx` `pageSignature`, #961), which is why two frames of
  one split section do not fade today.

### 4.3 Worked examples

**Two screens, Apps has 3 pages.**

| slot | screen 1 | screen 2 |
|---|---|---|
| 1 | A1 | A2 |
| 2 | A3 | A2 *(hold)* |

**Two screens; Apps 3 pages, Mains 2, Desserts 1.** Apps (3) is longer than the wall — spills.
Mains (2) does not fit the one screen left in slot 2 but fits a fresh slot — defers. Desserts (1)
fits where the cursor is.

| slot | screen 1 | screen 2 |
|---|---|---|
| 1 | A1 | A2 |
| 2 | A3 | A2 *(hold)* |
| 3 | M1 | M2 |
| 4 | D1 | M2 *(hold)* |

**Three screens, one section of 7 pages.**

| slot | screen 1 | screen 2 | screen 3 |
|---|---|---|---|
| 1 | p1 | p2 | p3 |
| 2 | p4 | p5 | p6 |
| 3 | p7 | p5 *(hold)* | p6 *(hold)* |

**Three screens; Apps 2 pages, Mains 2 pages** — why the unit is the run, not the operator page.
Apps fills two screens; Mains (2) does not fit the one screen left but fits a fresh slot, so it
defers and stays side by side. Had the unit been the operator's whole page (4 frames), Mains would
have been split across the slot boundary.

| slot | screen 1 | screen 2 | screen 3 |
|---|---|---|---|
| 1 | A1 | A2 | *filler, or A1 (mirror)* |
| 2 | M1 | M2 | A1 / filler *(hold)* |

**Three screens, one page.** `p1 · p1 · p1` (mirror), or `p1 · filler · filler`. K = 1, nothing
rotates.

---

## 5. Keeping screens in step, and switching releases

No messages pass between TVs. Each computes its own position from its own clock:

```
k = floor((now − startAt) / dwellSeconds) mod slotCount
show mySlots[k]; fade only when mySlots[k] !== mySlots[previous k]
```

- Two screens on the same wall stay in step to within their clock error — TVs sync to a time
  server, so this is well under a fade. There is no server round trip per page turn, ever.
- **Start time is set by the server** (WP-10) once every screen in the group that is not
  excluded (see Grace) reports *received*. It is the first slot boundary of the group's *current*
  schedule (the latest package for this group that already has a start time) at or after
  `max(received) + one dwell`; a group with no current schedule uses `max(received) + one dwell`
  as is. Aligning to the outgoing boundary means the old page is never cut mid-read — Menus Q203's
  "swap at the next page turn" — and every screen begins the new release at slot 1 at the same
  moment. The server **pushes** `{ packageId, startAt }` to every screen over the player's
  existing realtime channel (SignalR, as `contentUpdated` is today) the instant it is set; the
  heartbeat reply carries it too as the backstop for a missed push. One dwell is therefore ample
  lead for every screen, including the one whose *received* report set the time. A screen that
  learns it late still lands in step — the slot sum puts it on the right slot — it just joins
  late, and diagnostics record that.
- **Grace:** a screen with no heartbeat for `offlineAfterSeconds` when readiness is checked is
  not waited for. Any other screen that has not yet reported *received* — `downloading`, or
  `failed` with a retry pending — is waited for until `requested + readyCeilingSeconds`, then the
  wall goes ahead without it. A screen the wall went ahead without is **excluded**: it applies the
  package on its own when it has it, and reports *stale* until then. Both are settings; proposed
  defaults in §12 (Q-WP-2).
- **Before `startAt`** a screen keeps its previous schedule. A screen with no previous package
  (first ever) shows the new package's slot 1 statically until `startAt`.
- **Clock fallback:** the clock is *trusted* once the platform reports a time sync (or the first
  sync since boot). Until then, and if a trusted clock later jumps by more than one dwell between
  two consecutive reads, the player runs a local timer from its last known slot rather than
  freezing or jumping; on a cold boot with no last known slot it starts at slot 1. The sync step
  itself is never treated as a suspicious jump. When the clock is trusted again the player rejoins
  the slot sum at its next local slot boundary, not mid-dwell. It may drift from its neighbours
  until then; diagnostics say so. The server time already carried by the never-emitted `SyncTick`
  event (§11) is the natural source for "trusted".
- **Adding, removing or reordering a screen** re-plans the group: a new package (new
  `groupRevision`) and a new start time, aligned to the outgoing schedule's boundary exactly like
  a release. The existing `video-wall-updated` notification is the trigger.

---

## 6. Things that must not move the plan

- **86 / sold-out and other operational state.** The state response keeps the item's footprint.
  The renewal's own example (§4.3 "State is not layout": keep the row, replace the price with
  "Sold out", dim the name — "or use another approved response"), Menus decision A3 (the board
  says **Sold out**, drawn by the theme) and the Theme Studio mock (row kept, price replaced with
  SOLD OUT, row dimmed and struck through — `mock-source/app/page.tsx`, `globals.css`) all keep
  the row, as today's layouts do. Removing an item is a publish (Menus A4: *Available off* rides
  the publish), which re-plans like any release. So the page list is unchanged and the plan
  stands. **Constraint handed to Theme Studio (TS-C1):** a state response may never change how
  much space an item takes; if it could, live state would re-paginate a wall and screens would
  disagree. **TS-C2:** the shared renderer must expose pagination (release + canvas format →
  `RenderedPage[]`) separately from drawing one named page, so a live-mode player draws the page
  it is told to and never re-paginates (§7).
- **A screen going dark.** The plan comes from the group *definition*, not live health. The others
  carry on; the gap is visible in diagnostics. Re-planning on health would make walls jump every
  time a TV blinked.
- **A slow or offline screen at cutover** (WP-10): it is skipped for the start time, not re-planned
  around.

---

## 7. Always something on screen

The product promise is that a screen is never blank. The package is designed so the server is not
needed to keep a wall running:

- **The package is self-contained:** pages, schedule, dwell, assets. The start time arrives
  separately once the wall is ready (§5) and is stored beside the package. After that, everything
  the screen needs to keep rotating — and to stay in step with its neighbours — is on the device.
- **Never expire the last good package** (WP-11). Keep showing it; mark it *stale*; replace it
  only with a newer complete, verified one.
- **Apply only a complete package** (WP-9). Pages and assets downloaded and checksummed; otherwise
  the old one stays up. Never a half-package.
- **Boot straight into the cached package.** Power cut, TV back on, no network: the menu is up in
  seconds — no pairing screen, no spinner.
- **The app is installed, not fetched.** In the TV-app runtime the code is on the device. For the
  web player the shell is cached the way media already is (the media service worker in
  `mediaCache.mjs`), so a plain browser link to a live server is not the offline story.
- **State persists offline.** The last known 86 state stays applied; it updates the moment the
  connection returns.
- **What this does not solve, by design:** a change published while a screen is offline reaches it
  when it reconnects. The screen shows the last thing it was told was true, and Back Office shows
  that screen as *stale* / not yet applied — never as done.

Pre-rendered (static / hybrid) output from Theme Studio makes the cached package cheaper to show
but nothing here depends on it. In every output mode the page list — page ids and which items sit
on which page — is computed once, at packaging, and shipped in the package. A live-mode player
draws a named page from that assignment; it never runs the renderer's pagination step (TS-C2), so
it can never arrive at a different page count from its neighbours.

---

## 8. Delivery states

Per screen, per package (identity per §9), each transition timestamped. The seed is the existing
`dbo.ScreenContentDeliveries` (`ScreenContentDeliveryService`, receipt endpoint
`DisplayController.ContentReceipt`, posted by the player as Received then Applied; Back Office
already renders "Revision N: Received · applied M" from it) — this keeps its Requested / Received
/ Applied / Failed / Superseded / Recovered rows and adds what is missing:

| State | Meaning | Data |
|---|---|---|
| `requested` | Publish produced the package and targeted this screen | `at` |
| `sent` | The screen was told the package exists | `at` |
| `downloading` | Transfer in progress | `startedAt`, `bytesReceived`, `bytesTotal`, per-asset progress |
| `received` | Complete on the device, checksums verified | `at` |
| `applied` | On screen, at or after `startAt` | `at` |
| `failed` | A step failed | `reason`, `attempts`, `nextRetryAt` |
| `superseded` | A newer package for this screen was requested before this one applied | `at` (kept from today) |
| `recovered` | Applied from the device cache after a restart or outage | `at` (kept from today) |

Plus per screen: `showing` (package identity currently on screen; empty on a screen that has
never applied one) and `stale` (true when `showing` is not the latest package targeted at it
**whose `startAt` has passed** — a screen still on the previous package before `startAt` is not
stale, that is §5's normal cutover; only a screen that missed the wall's switch reads stale).

Derived, for Back Office and diagnostics: **delivery time** = `received − requested`; **wall
readiness** = every screen that is not excluded (§5 Grace) has reported `received`; the group's
`startAt` once set. This is the "requested / received / applied evidence" the renewal §6 requires
— `requested`, `received` and `applied` keep the renewal's meaning — with `sent`, `downloading`
(bytes) and the `stale` flag added because media-heavy packages take minutes and the owner wants
to see each step (WP-12), not guess it. `docs/architecture/player-delivery-reliability.md` already
says the UI must not call a change applied until the player says so; §8 is the data behind that.

---

## 9. What this needs from the rest of the platform

Feeds renewal planning session 4 ("theme and release contract") and M2 ("release tuple / package
contract"):

- **From the renderer / Theme Studio:** for each canvas format, an ordered `RenderedPage[]` with
  stable page ids, `sourcePageId`, `sectionIds` and `continues` — Theme Studio's "Continuation —
  generated only when capacity is exceeded" and "Keep section together" are exactly this. An
  optional **filler page** declaration (WP-7). Constraint TS-C1 (§6).
  **TS-C3 (WP-2, WP-4):** a theme published for wall use must not list a type-shrinking move
  among its permitted overflow strategies — same text size on every screen is a theme property,
  not something the planner can enforce.
- **From the release:** `releaseId`; `dwellSeconds`; the set of canvas formats it was rendered
  for.
- **From Screens:** the group — id, canvas format, ordered screen ids (WP-1, WP-8). Back Office
  already has the editor: `VideoWallBuilder.tsx` over `GET/PUT/DELETE /video-walls`
  (`BackOfficeScreensController`, `VideoWallService`), writing `WallGroup` / `WallPosition`,
  gated by the `screen.wall.coordinate` capability. It is reused as the group editor; wording
  changes from "video wall" to "screen group", and its fixed 2x1 / 3x1 / 2x2 layouts become an
  ordered row of any length (§11, Q-WP-5). `WidthPixels` / `HeightPixels` on `Screen` seed the
  canvas format; `HeroDwellSeconds` is unrelated (hero rotation, Menus Q9). Menus Q162 deferred
  the "split across screens" fix action until its flow was designed — this is that design; the
  action becomes "assign to a screen group" once the editor and layout preview exist, and is
  still not a button before then.
- **From Display Delivery:** package identity = release + renderer implementation version +
  `plannerVersion` + canvas format + group id + `groupRevision` (so a re-plan under §5 is a new
  package, with its own delivery rows); the schedule inside it; delivery states (§8); the
  start-time setter and push (§5). The renderer *contract* version is already pinned in the
  release (renewal §6); the implementation version is listed separately because a renderer fix at
  the same contract can change the pages.
- **From the player:** download-verify-apply (§7), receiving and storing the start time (§5), the
  slot sum, the clock fallback, state reporting (§8). It contains no layout logic.

---

## 10. Testing and verification

- **Unit, table-driven:** every worked example in §4.3, plus: N = 1; N = 2..4 with runs of length
  1..N+2 in every order; all-single-page runs; one run longer than 2N; filler declared vs not;
  empty group (error); positions 1-based, with gaps, and with duplicates (duplicates: error); a
  run of length exactly N at every cursor position.
- **Property tests:** the invariants in §4.2, over generated page lists and group sizes.
- **Clock:** the slot sum at boundaries, before `startAt`, across a dwell change, and under the
  fallback timer.
- **Fixture lab (what Menus M10 T2 becomes):** give it a menu shape (sections × item counts ×
  density) and a group size; it runs the real renderer and the real planner and renders the wall
  as N side-by-side screenshots per slot. A packing regression is a failed screenshot, not a
  restaurant's complaint. Back-office surfaces show the "N OF M" counter on continuation pages;
  guest pages repeat the heading (Menus Q137). It is a Display Delivery verification tool built
  with the planner; it is *not* the renewal's §7 step 2 "fixture and preview lab" (a model /
  binding lab that precedes the `menu.v1` proof), though it may take that lab's fixtures as its
  menu shapes.
- **Verification bar, inherited from Menus #958–#961:** a change to the planner or the renderer is
  not done until it has been read from real screenshots — of the fixture lab for the algorithm,
  and of a deployed group for the delivery path.
- **Delivery:** a media-heavy package on a slow link shows `downloading` with moving byte counts,
  the old release stays on screen, the wall starts together at `startAt`; pull the network on one
  screen and the others go ahead after the grace period while it reports `stale`.

---

## 11. What happens to the current code

| Today | Disposition |
|---|---|
| `DisplayController.ComputeFrameStarts` / `SliceSections` / `ExpandVirtualPages` (C#, item-count packing, photo grid only) | Retire in the renewal's screens / player-packaging milestone, when display consumes Content Release. No new work against it (WP-5). |
| `Screens.WallGroup` / `WallPosition` meaning "split one page's items across screens by cumulative capacity" | Meaning changes to WP-1 (screen k shows what the schedule says); columns are reused. Positions are 1-based today (`VideoWallService` assigns `index + 1`) and archive / unpair (`ScreenManagementService`) leave gaps; the planner sorts by position and uses the rank as the column, so no renumbering is needed. |
| `VideoWallBuilder.tsx` + `/video-walls` (`BackOfficeScreensController`, `VideoWallService`): name, layout 2x1 / 3x1 / 2x2 (max 4), ordered members; `screen.wall.coordinate` capability; `video-wall-updated` notification | Reused as the WP-1 group editor. Wording "video wall" → "screen group"; the layout dropdown becomes an ordered row of any length (a 2x2 grid is out of scope for v1 — Q-WP-5); the notification becomes the re-plan trigger (§5). |
| `dbo.ScreenContentDeliveries` / `ScreenContentDeliveryService` / `ContentReceipt` (Requested, Received, Applied, Failed, Superseded, Recovered; 90-day purge; Back Office status line) | Kept as the seed for §8. Adds `sent`, `downloading` with bytes, the `stale` flag, and package identity as the key instead of a revision number. |
| `SyncTick` (server notifier `NotifyScreenSyncTickAsync` / `NotifyVenueSyncTickAsync` with `serverTimeMs`; client handler records it; **never emitted** by any server code) | Reused as the server-time source for the §5 clock trust rule. Not a page-turn tick — the slot sum needs none. |
| `ExpandVirtualPages` wall arithmetic (slices from `itemOffset + frameStart` with capacity `min(screenCapacity, frameEnd − frameStart)`, so a non-first wall screen on an early-closed frame reads into the next frame; untested, per Done Records #960/#961) | Known defect, recorded. Fix only if a customer wall hits it before M4; otherwise retired with the controller. |
| `displayCache.mjs` 7-day `displayContentCacheMaxAgeMs` | **Defect against WP-11.** Small "stabilize now" fix: remove the expiry, keep the version check. Separate PR. |
| `boardFitScale.mjs` shrink-to-fit (0.4 floor) | Stays as the live safety net until the renderer's capacity model replaces it. Menus M10 T1 (Option A) proceeds as decided. |
| Menus M10 T2 (virtual screen over the C# packing) | Re-pointed: becomes the fixture lab in §10, built over the real renderer + planner, not over `ComputeFrameStarts`. |
| Menus M10 T3 (lone-page merge) | Unchanged: on hold, never silent display-side behaviour. The planner does not merge pages. |
| Per-screen dwell timers (`usePageRotation.ts`), no shared clock — Menus Q54 accepted the drift | Replaced by the slot sum (§5); Q54 superseded. |
| Fade keyed on section ids (`DisplayLayout.tsx` `pageSignature`, #961) | Replaced by "fade when the page id changes" (§4.2, §5). |
| Nine layout components in `src/display/src/layouts` | Superseded by the theme definition + shared renderer (Theme Studio plan); not this document's concern beyond noting the planner never references a layout kind. |

---

## 12. Open questions for the owner

Recorded, not blocking; each runs on the stated default until answered.

- **Q-WP-1 — Filler behaviour (WP-7).** Default: theme filler page if declared, else mirror page 1.
  Alternative: theme background only. Revisit when the first real wall runs.
- **Q-WP-2 — Grace defaults (WP-10).** Proposed: `offlineAfterSeconds = 120` (no heartbeat →
  not waited for); `readyCeilingSeconds = 900` (not yet `received` → wait up to 15 minutes from
  `requested`, then go). Both venue-level settings.
- **Q-WP-3 — Per-page dwell.** v1 uses one dwell per release. A theme or operator may later want a
  hero page to sit longer; that is a per-slot dwell array in the schedule and a slightly different
  slot sum. Not designed here.
- **Q-WP-4 — Mixed canvas formats in one group (WP-8).** Out of scope for v1. If wanted, the
  release renders pages per format and the planner runs per format with page *counts* possibly
  differing — which breaks "same page on the same screen" and needs its own design.
- **Q-WP-5 — Grid walls (2x2).** The planner models a wall as one left-to-right row. Today's
  editor offers 2x2. Reading order on a grid (row-major? column-major?) is a design question;
  v1 refuses a 2x2 group rather than guessing.

---

## 13. Not in scope

- Deciding how many items fit on a screen (renderer / theme capacity).
- Merging lone pages (M10 T3, on hold).
- Static / hybrid pre-rendered output (Theme Studio; unproven; optional here).
- Screens that are not in a group: they are a group of one and need nothing new.
- Player hardware, the box player, or the TV-app runtime itself beyond the rules in §7.

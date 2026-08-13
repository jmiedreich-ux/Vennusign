# Menus Build — Owner Decision Record

- **Build:** Menus (first build under the post-reset model)
- **Decided:** 2026-08-07, owner Q&A session
- **Design authority:** the owner's Menus design bundle (decisions.md, 36 numbered decisions, hi-fi M1/M2/M2c, lo-fi wireframes M1a–M3 and MV1–MV4b, README handoff). Approved as implementation authority; to be copied verbatim into this folder. Where any other document disagrees with `decisions.md`, the decisions win.

## Working model (replaces tracks/phases)

The unit of work is a **build**, named by product area. A build is delivered in numbered **slices** — functional vertical slices per the Track 1 retrospective (approved 2026-08-07, `docs/retrospectives/track-1-lessons-learned-retrospective-report.md`). Screen work starts as wireframe mocks from the owner, is written up, then passed to the agent for planning and implementation.

## Decisions

| # | Question | Decision |
|---|---|---|
| 1 | Item library vs per-menu items | **Minimal library now.** Item + Placement tables land in slice 1 so items can appear on several boards ("Also on Late Night", shared edits). Full library UI remains out of scope, as the design README states. |
| 2 | Save model | **Draft/publish spine adopted.** 86/availability commits instantly, venue-scoped, never queued. All other edits queue per menu and ship on explicit Publish, with attributable history ("Go back to…"). Replaces autosave outright. |
| 3 | History retention | **Tier-configurable.** Retention depth is a per-tier setting (fits the existing allowance/entitlement model). Owner will supply the numbers later; build it as configuration, not a constant. |
| 4 | Build scope | **Single-venue only:** M1, M1a, M1b, M2, M2a, M2b, M2c, M3. Multi-venue (MV1–MV4b, trust levels) is a later build; per decision 29 it is invisible below tier, so nothing in this build may leak it. |
| 5 | Import routes | **Paste + start-blank only** in this build. The shared confirm step is built once so spreadsheet, photo (needs an OCR provider decision), and POS routes plug in later. |
| 6 | Legacy item fields | **Drop in migration.** HappyHourPrice, QuantityAvailable, Tags, IsPopular, per-item translations do not migrate to the library model. Time-based pricing re-enters later via Schedules (decision 35). |
| 7 | Design authority | **Approved.** The bundle is the implementation authority and lives in `docs/design/approved/menus/` — the first content under `approved/`, satisfying the retrospective's design-before-implementation gate. |
| 8 | Token additions | **Approved.** `proposed-token-additions.css` merges into `sky-ui-tokens.css`; components consume variables, never raw values. Nothing existing changes. |
| 9 | Hidden vs locked (decision 4 reach) | **Menus follows decision 4 strictly now** (out-of-plan = absent). The app's existing upgrade/marketing surfaces (UpgradeModal, SidebarUpgradeNudge, LockedSectionPreview, EntitlementLockChip) are untouched; marketing/upgrade discovery is reworked at a later stage. |
| 10 | Board font | **Playfair Display, self-hosted** (default pending owner objection). Font files bundled in the repo so boards render identically on offline displays; used only in board content, never UI chrome. |
| 11 | Track 1 tail | **Retrospective folded in** (approved and merged); **everything else archived.** PR #663 closed unmerged with dispositions recorded on the PR. Acceptance results 2-1/3-1 superseded by the redesign; 3-0 / `screen.content.target` enforcement remains a known gap on master, tracked as a hygiene item for builds touching Screens. |

## Decisions — round 2 (2026-08-07, planning Q&A)

| # | Question | Decision |
|---|---|---|
| 12 | App shell | **New 76px icon nav rail app-wide.** The rail from the hi-fis becomes the shell for the whole back office; other areas keep their current content inside it unchanged. Decision 19's nav gating is built once, in the shell. |
| 13 | TV player | **Amended twice 2026-08-13:** this remains the target architecture, but display-player integration, geometry, and geometry-driven pagination are no longer part of Milestone 4. Milestone 4 builds only the published guest projection and non-visual delivery foundations. Canvas/theme layout, rendering, cycling, cutover, live 86 behavior, reconnect, and device proof require separately owner-planned work. |
| 14 | 86 reset | **Stays off until a person turns it back on.** No automatic morning reset. The "Off right now" list carries the age so nothing is forgotten. The legacy `AvailabilityResetUtc` concept does not migrate. |
| 15 | Happy hour | **Parked.** Dropping `HappyHourPrice` means items cannot show a happy-hour price until Schedules-owned pricing is designed (decision 35). The schedule machinery itself stays. Accepted consequence. |
| 16 | "Not in use" | **Manual action.** The card ⋯ menu gains a sixth, plain-language item ("Put away") that moves a menu to the Not-in-use strip. *Approved deviation:* the design's verbatim ⋯ menu was five items; it is now six. Opening a put-away menu works like any other; putting it on a screen returns it to the shelf. |
| 17 | Acceptance cadence | **Per slice.** Every slice ends with a short owner workbook (5–10 minutes) before the next slice starts; the schema-only slice gets a demo script instead. |

## Known gaps carried into planning

- `screen.content.target` is declared but enforced nowhere on master (was fixed only on archived #663).
- Device-reported screen geometry is **confirmed missing** (verified 2026-08-07): `Screen.cs` has no resolution/size fields and the display heartbeat reports none. Play and the publish chips need it (decision 13) — a slice must extend the heartbeat payload and Screen model.
- The item-library migration drops legacy fields (decision 6 above) — migration script must be explicit about what is discarded.
- Photo import blocked on an OCR/AI provider + cost decision. POS import route blocked on nothing (Clover/Square/Toast sync exists) but is deferred with it.

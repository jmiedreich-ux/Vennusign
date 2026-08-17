# Vennusign Session Handoff

Updated 2026-08-14, for Menus 6-A3 owner acceptance.

## 2026-08-14 — Menus 6-A1 accepted product candidate

- Product `547aea7` implements the complete 6-A1 outcome: a relational, resumable paste-import session; bounded deterministic parsing; conservative safe matching; grouped unselected semantic candidates; dependency-aware answer invalidation; fallback and reversible section promotion; direct review UI; expiry, revision, tenant and concurrency guards; and no menu mutation.
- Owner acceptance passed 7/7 at `2026-08-14T03:43:13.198Z`; durable evidence is `docs/features/menus/m6a1-acceptance-record.json`. Independent review approved exact head `4b6206d` after the records-only closeout. PR #716 merged to `master` as `ac4cc98` at `2026-08-14T03:45:49Z`; issue #714 is closed and the remote feature branch is deleted.
- Executed evidence: focused import API/parser/service/controller/migration tests 20/20; LocalDB repository/invariant tests 5/5 with the Azure override removed; full LocalDB 110/110 earlier in the candidate; Back Office units 203/203; production build passed; focused import Playwright 3/3 applicable with three intentional project skips; Impeccable detector clean. The full Playwright gate is **not a pass** because parallel Test API seeding against one LocalDB produced missing seeded sections/items; tracked separately as issue #715. Azure/external integration remains skipped by standing owner policy, and CI remains suspended.
- Behavior search used for the multiplier: `rg -n "menu-import|MenuImport|paste import|Paste what you have|Accept safe matches|Imported items" src tests docs/features/menus --glob '!**/node_modules/**' --glob '!**/dist/**'`. The changed locations are the import aggregate/migration/repository/service/controller, direct Back Office route and UI, fixture cleanup, focused API/LocalDB/browser tests, and the 6-A1 workbook. Existing Add-a-menu, create/replace, publishing, POS and screen paths remain unchanged because they belong to 6-A2/6-A3 or later flows.
- Explicitly deferred: menu creation (6-A2), replacement/locking/snapshots/restore (6-A3), spreadsheet/photo/POS import, publishing, mobile support below the 900px refusal floor, and keyboard-specific interaction design/testing.

**6-A2 claim.** Issue #718 is claimed on `feature/menus-m6a2-create-import` from merged `master` (`0a52304`). Its outcome is create-only: a resolved import confirms exactly one unpublished working menu atomically and idempotently, then truthfully says Not live yet. 6-A3 replacement remains excluded.

**6-A2 implementation checkpoint.** The path/invariant/test matrix is on issue #718. Product head `b1e62c4` is pushed in draft PR #719. It adds migration 069, atomic/idempotent create confirmation, transaction-local permission and current allowance enforcement, persisted destination/name/completion state, menu-scoped imported price overrides through builder/history paths, truthful completion UI, invariants, LocalDB/API/static/Playwright coverage, and an Impeccable APPROVE verdict. Full API was 478/479; the sole failure is the pre-existing unrelated E2E layout expectation (`default` versus current `photo_grid`). Azure/external tests remain skipped by owner exception.

**6-A2 review result.** Independent engineering review approves exact PR head `3c69b7b` with product `b1e62c4`; the Impeccable finish review also approves. The short owner workbook is `docs/features/menus/m6a2-acceptance-workbook.html`.

**6-A2 owner acceptance.** The owner accepted all 6/6 workbook cases against product `b1e62c4` at `2026-08-14T04:39:39.105Z`; the durable record is `docs/features/menus/m6a2-acceptance-record.json`. During acceptance the owner flagged the shared heavy black focus halo. The correction replaces it everywhere with one contrast-safe 2px dark-sky ring, with a focused static regression and a computed-style Playwright assertion.

**6-A2 merge closeout.** Exact-head independent review approved `95f6e5c`. PR #719 merged to `master` as `b27159dee0d20600daab14ad0b0d280c4dbd5e72` at `2026-08-14T04:45:58Z`; issue #718 closed one second later and the remote feature branch is absent. The acceptance-requested focus treatment now uses one contrast-safe 2px ring with no black halo, including the intentionally light paste controls in Midnight.

**6-A3 implementation checkpoint.** Issue #720 is claimed on `feature/menus-m6a3-replace-import` from merged/closed 6-A2 master (`d703e42`). Product `bf77919` is pushed in draft PR #721. Migration 070 and the replacement aggregate persist the selected target/revision and server-computed published-versus-working facts, create one immutable complete pre-import checkpoint under the same SQL transaction as replacement, resolve permission/current item allowance/snapshot retention under locks, preserve menu identity/theme/pages/assignments/published version/availability, keep pasted shared-item prices menu-scoped, and refuse stale targets or stale restores without mutation. Completion remains Not live yet and exposes deliberate restore of the previous working draft.

Executed evidence: Release API build passed with seven existing warnings; focused API/migration tests 51/51; MenuImport LocalDB 12/12 with Azure unset; fresh LocalDB migration 070 plus replacement/restore regression passed; Back Office production build passed; static 204/204; focused Playwright desktop passed create, replacement/restore and near-match cases while mobile passed the below-900 refusal, with inverse project cases intentionally skipped; `git diff --check` passed; Impeccable detector returned `[]`. CI is suspended and Azure/external integration remains skipped by owner policy.

Behavior search: `rg -n "MenuImportDestinations|ConfirmReplaceAsync|SetReplaceDestinationAsync|RestoreReplacementAsync|CompletedSnapshotId|ImportedPriceOverride" src tests --glob "*.cs" --glob "*.ts" --glob "*.tsx" --glob "*.sql"`. Changed consumers are the import aggregate/repository/service/controller, Back Office API/surface, migration/invariants, UI fixture cleanup and focused tests. Existing builder duplication/history/publish price consumers remain unchanged because they already preserve `ImportedPriceOverride`; publishing and non-paste import routes remain out of scope.

**6-A3 review result.** Independent engineering review and the Impeccable finish review approve exact product head `58e8258`. Review fixes add migration 071 and complete deterministic working-menu fingerprints under transaction locks, real child-edit conflict regressions for confirm and restore, immutable completed-session provenance, exact nullable price-override restoration, refreshed conflict facts, and plain-language added/removed/changed confirmation detail. Focused API/migration tests pass 51/51; MenuImport LocalDB passes 12/12 with Azure unset; Back Office build/static pass 204/204; focused desktop replacement Playwright passes 1/1. CI and Azure/external integration remain skipped by owner policy. Residual non-blocking risk: the working projection is duplicated in three SQL paths and future fields must update all three.

**6-A3 owner acceptance.** The owner accepted all 7/7 workbook cases against product `58e8258` at `2026-08-14T05:40:26.942Z`; durable evidence is `docs/features/menus/m6a3-acceptance-record.json`.

**6-A3 merge closeout.** Acceptance-record head `61bdd29` was independently approved. PR #721 merged to `master` as `c32fda22f5bd843ffcc2e8015089c7ab9c2d22ec` at `2026-08-14T05:42:02Z`; issue #720 closed one second later, the remote feature branch is absent, and `origin/master` contains product `58e8258`. The active claim is released.

**Exact next action.** Stop. Menus milestone 7 remains marked “needs redesign and planning”; no successor milestone is approved or claimed. Resume only from a fresh owner planning decision.

## 2026-08-13 — Menus Slice 6-A paste-import design approval

- The owner approved `VennuSign_-_Paste_import_storyboard_v4.pptx`. Canonical authority is now `docs/design/approved/menus/paste-import/`: the storyboard, compact customer-flow image, editable Mermaid confirmation sequence and rendered sequence.
- Approved decisions are synchronized into `docs/design/approved/menus/decisions.md` decisions 33 and 37–43. Conservative matching permits automatic identity only for case/punctuation/spacing normalization; ambiguous rows are never preselected. Parsing/review persist a resumable import session, and final confirmation is the only atomic/idempotent menu mutation. Destination is chosen after review. Screens remain unchanged until Publish.
- Owner-approved product decisions: dependency-aware answer preservation; server-computed unpublished-change breakdown; one `Imported items` fallback with reason metadata; explicit reversible line-to-section promotion; all historical replacement snapshots with tier/configuration policy; tier/configuration import-session retention; and no silent cross-menu price mutation.
- Replacement preserves menu identity, theme, assignments, published snapshot and active 86 state. Completion says `Not live yet` and offers `Review draft in builder` or `Done for now`. Below the 900px supported floor, preserve the session and offer a wider-window handoff.
- Keyboard-specific interaction design/testing remains excluded. Semantic controls, accessible names/relationships, visible focus and screen-reader-compatible status/error announcements remain required.
- Slice 6 was already merged through PR #711 as `3429684`. Slice 6-A1 is now claimed as issue #714 on `feature/menus-m6a1-paste-review`; implementation has not started.
- Owner approved splitting implementation into three sequential vertical milestones: **6-A1 paste/parse/review** (resumable resolved session; no menu mutation), **6-A2 create new menu** (atomic/idempotent confirmation and truthful completion), and **6-A3 replace existing menu** (target locking/conflicts, snapshots/restore and preservation invariants). Each includes schema, API, UI, Playwright coverage and its own owner workbook. Do not split by technical layer.
- The 6-A1 readiness audit is complete and published on issue #714. It selects a separate import-session aggregate (`IMenuImportRepository`), migration 068 with relational session/line/question/candidate/answer tables and a tier-resolved retention allowance, a pure parser/matcher, `api/back-office/menu-imports`, and an isolated Back Office import route. The existing Add-a-menu flow stays discoverable and unchanged until 6-A2 can provide a real create outcome; 6-A1's route is directly testable but not advertised. Existing menu/item/price/POS writes remain unchanged because 6-A1 is read-only with respect to menu content.

**Exact next action.** On issue #714 and `feature/menus-m6a1-paste-review`, implement migration 068 and its migration-resource assertions plus the import-session invariants in the automatic LocalDB sweep. Then continue repository → parser → API → UI → Playwright in the audited order. 6-A2 cannot start until 6-A1 is merged and its owner workbook accepted; 6-A3 has the same dependency on 6-A2.

**Implementation checkpoint, 2026-08-13.** Migration 068 now defines the relational import-session aggregate and tier-resolved retention allowance without touching menu content; core import records, three automatic model invariants, and the deterministic paste parser/matcher are present. Focused migration/parser tests pass 45/45 and the Release solution build passes with pre-existing warnings. A punctuation-boundary normalization regression was observed failing and fixed. LocalDB execution is **NOT A PASS**: the integration fixture attempted an invalid `sqladmin` login, so migration application and invariant execution remain untested until the approved LocalDB connection is restored. Next implementation action is `IMenuImportRepository` plus its LocalDB tests; do not treat the credential failure as a skipped passing suite.

## 2026-08-13 — Menus Slice 6 product candidate

- Product candidate `e5364a50ef29a8c4c119ebaf4ec5413662025149` implements issue #710 on `feature/menus-s6-86-board`. The approved authority image is `docs/design/approved/menus/86-board-7b.png`; later owner decisions override its illustrative Undo with confirmation before every 86.
- The separate 86 board reads only published menus assigned to screens, repeats shared items once per published placement, searches that same bounded set, commits availability venue-wide, and reports proven reach with the existing offline/stale classifier. Carryover review never auto-restores. Single restore and atomic restore-all both confirm first.
- Start blank stays on Menus Home. The existing ceiling-locked menu transaction now creates Page 1 and Section 1 atomically; the builder opens on that section with the add-item row focused. Duplicate/invalid/ceiling refusal behavior remains at the existing enforcement boundaries.
- Behavior search: `rg -n "Quick Update|QuickUpdate|SetAvailability|availability|isAvailable|IsAvailable|New menu|Start blank|createMenu|create menu" src tests --glob '!**/node_modules/**' --glob '!**/dist/**'`. Changed: Menus Home/card entry points, App routing, the content availability service/repository/API for atomic restore-all, and the existing menu-create transaction. Unchanged deliberately: builder availability remains the full-editor alternative; Daypart Home is a separate dashboard summary; POS inventory and Tap availability are separate writers/domains; board engine remains the guest projection consumer.
- Executed evidence: Release solution build passed; Back Office production build passed; Back Office units 202/202; focused API availability tests 3/3; focused LocalDB tests 2/2; affected Menus Playwright 12/12; isolated environment/sign-in 3/3, Slice 6 3/3, and navigation/entitlements 5/5; Impeccable detector clean. The blank-section and atomic restore-all LocalDB regressions were each observed failing with their fix removed and passing after restoration. `git diff --check` passed.
- Broad Playwright is **NOT A PASS**: one run used an invalid isolation tag, and later monolithic attempts were invalidated by orphaned worker contention or hit the ten-minute command ceiling without a final report. A final one-worker attempt of only `single-venue-criterion-18.spec.ts` also produced no report before its three-minute command ceiling, so the newly added Quick Update surface in that sweep remains **UNTESTED by that named spec**; Quick Update itself passed its focused 3/3 and the affected Menus group passed 12/12. All orphaned processes and services were stopped. CI and external/Azure/device/mobile/player tests are not run by policy/scope.
- Explicit exclusions: Board View/Play #709, display player, geometry/pagination, canvas/theme work, unplaced items, Slice 6-A import, Slice 7 redesign, and claimed mobile support.
- Independent review found and the implementation now closes three boundary defects: availability-only staff use one bounded read model; restore-all selects and updates only delivered snapshot items inside one locked SQL transaction; and returned/notified reach is derived from each screen's exact delivered menu version rather than working assignments. Focused API tests passed 13/13 before the final reach change, the rebuilt ContentService set passed 10/10 after it, the LocalDB hidden-item boundary passed 1/1, Back Office units passed 202/202, production build passed, and the Release solution build passed.
- Final review of `e5364a5` found no remaining product-code defect and requested two test-integrity regressions that deliberately invert assignment and delivery truth. Both are now present: published delivery after assignment removal still notifies/returns the screen, while a staged assignment without matching delivered content does not. Both failed against the old assignment helper (2/2 failed) and pass against the current delivered-version helper; the complete focused `ContentServiceLogicTests` set passes 12/12.
- Closure update: the owner instructed this bounded test closeout be committed without another review or owner workbook. PR #711 subsequently merged as `3429684`. Slice 6-A was blocked at this historical point; its design is now approved by the newer handoff section above. Slice 7 remains unplanned.

## 2026-08-13 — Menu Builder page-action crumb refinement (local, uncommitted)

- Owner requested the page action menu move off the standalone ellipsis between the page and section path. The active page crumb is now the page-action trigger (`Page name` + trailing caret); the inert `/ Section name` path remains unchanged, and section-row actions remain in the Sections rail.
- The menu is anchored under its owning page crumb and labels its scope explicitly: **Rename page**, **Duplicate page**, divider, destructive **Delete page**. Delete continues through the existing confirmation and guarded page lifecycle.
- Selecting a page tab now returns that page to Whole page view, replacing the former second meaning of clicking the page crumb.
- Search used to establish the behavior surface: `rg -n "page.*action|Rename page|Duplicate page|Delete page|pageMenu|ellipsis|MoreHorizontal|breadcrumb" src/back-office/src/MenuBuilder.tsx src/back-office/src --glob '*.tsx' --glob '*.css'`. Only the builder breadcrumb owns this page-action pattern; section rail actions and unrelated administration deletes were deliberately unchanged.
- Evidence: `npm run build` in `src/back-office` passed. `npx playwright test specs/menu-pages.spec.ts --project=desktop` passed 21/22, with one unrelated LocalDB seed deadlock; the affected paths then passed serially 7/7, and the final focused crumb/menu case passed 1/1. `git diff --check` passed (line-ending warnings only). Full Playwright, mobile, other roles/tiers beyond the existing capability-hidden case, and CI are **UNTESTED** for this bounded one-off.

## Menus M4 content/delivery foundations — review remediation, 2026-08-13

- Implemented scope: the existing guest board projection remains the sole filtering boundary; availability impact now derives affected screens from each assigned menu's latest published snapshot, deduplicates screens, and never uses draft-only placement rows as on-screen truth. Push, push-all, reset, and unpair require `screen.content.target`; reset and unpair retain their dedicated recovery/device gates.
- Copy paths: off and back-on distinguish zero, one, many, offline, stale, and mixed targets. Availability age uses venue-calendar today/yesterday/weekday forms.
- Review of `0480568` returned REQUEST_CHANGES because the first implementation used working placements, overstated back-on delivery, and treated stale as immediate. Those findings are remediated locally with draft-add/draft-remove, duplicate reach, on/off, offline, stale, and mixed tests.
- Executed evidence before first review: Release solution build passed; API units 420/420; Back Office units 202/202; production build passed; focused engine/model 65/65; focused API 13/13; affected Playwright 1/1; Impeccable detector clean. The broad Menu Builder attempt was **not a pass**: 27 passed before shared seed data reached the 50-menu ceiling, with one unrelated long-edit timeout.
- Post-remediation focused evidence: builder model 35/35, production build passed, focused API 14/14. Full affected gates and exact-SHA re-review remain next.
- Deferred: geometry, pagination, canvas/theme layout, `src/display`, playback, cutover, player 86 timing, reconnect, the 10-second line, and device compatibility.

## Current State

- Working model is **features and milestones** — read `AGENTS.md` first; the phase/track/WP workflow is retired.
- **All future tracks were cancelled by the owner on 2026-08-07** ahead of a full planning reset. Phase 13 is the final numbered phase; former Phases 14–16 are canceled, their ideas held in the unprioritized Backlog — TBD. RWP-13.06 is closed as not planned (#466), and #656–#662 are closed as `NOT_PLANNED`. The queued RWP-10.02, 11.03, 11.04, 13.04 and 13.05 packages are void as plans and survive only as history.
- Tracks 0 and 1 are complete and owner-closed. The Track 1 retrospective report sits on **PR #667 (open, unmerged)**; its process changes are already folded into `AGENTS.md`.
- **The planning reset produced the Menus feature.** Design authority: `docs/design/approved/menus/` (`decisions.md` wins conflicts). All 208 register questions are resolved in `docs/features/menus/open-questions.md`; the six-milestone plan in `docs/features/menus/milestone-plan.md` is reconciled with every answer.
- **Milestone 1 is merged.** PR #685 merged to `master` on 2026-08-09 as `cd449a3`, on 13 green exact-head checks at `2977bc3`; branch `feature/menus-m1-spine` is deleted, issue #684 closed. It was reworked five times: independent reviews #2 through #6 each returned REQUEST_CHANGES and each found real defects. All are closed, every one with a regression test verified to fail with its fix reverted.
- **Milestone 1 is accepted** (owner, 2026-08-09). Milestone 1 shipped no new UI, and `AGENTS.md` gives a schema-only milestone a demo script rather than a workbook walk: `scripts/run-m1-demo.ps1` passes 12 of 12, including customer-visible assertions of what each screen is actually showing. `m1-acceptance-record.json` stays **superseded** — it was signed 2026-08-08 against the authored-draft implementation — and is kept as history; this note is the acceptance record. **Milestone 2 is unblocked.**
- **Milestone 3 and its M3-A Slices 1–3-A are merged and owner-accepted.** Slice 3-A closed through PR #706 as `cdfd2bb`; issue #704 is closed and its branch is deleted. Its one-time independent-review, Playwright, and CI exception is exhausted and does not apply to successor work.
- **Milestone 4 content and delivery foundations are merged and owner-accepted.** PR #708 merged as `43ce604`; issue #707 is closed and the branch is deleted. Published guest projection, truthful 86 impact, venue-relative availability age, and screen-write authorization hygiene shipped. Geometry, canvas/theme layout, `src/display`, playback, live cutover, player 86, reconnect, the 10-second line, and device compatibility remain deferred.
- **Owner planning sequence synchronized from `VennuSign Planning` on 2026-08-13.** Every Slice 5 Board View + Play row is `Out of scope / Blocked`; Slice 5 is not next and its deferred bundle is #709. Slice 6 Quick Update + blank creation is next. The owner supplied and resolved the “86 board”: authored menu/section rail, one available tile per published placement, confirmation before every 86, search limited to published menus assigned to screens, global availability/restore, venue-day carryover review with no auto-restore, honest offline/stale reporting, and no unplaced items. Blank creation remains a separate Menus Home flow in Slice 6. These are planning-sheet Q12–Q20, not feature-register Q12–Q20. Paste import/matching/replacement is Slice 6-A; Menu Home completion is Slice 7 after redesign.
- **Initial Slice 6 behavior search:** `rg -n "Quick Update|QuickUpdate|SetAvailability|availability|isAvailable|IsAvailable|New menu|Start blank|createMenu|create menu" src tests --glob '!**/node_modules/**' --glob '!**/dist/**'`. Product locations requiring reconciliation are `DaypartHome.tsx` (existing 86 board), `MenuBuilder.tsx` (builder availability), `MenusHome.tsx`/`App.tsx`/`CustomerOnboardingApp.tsx` (menu creation), `back-office/src/api.ts` (legacy menu and content routes), `BackOfficeMenusController.cs`, the content controller/service/repository availability path, POS inventory writers, board-engine guest filtering, and their API/LocalDB/browser tests. Platform Operations tap-list availability is a different Tap domain and stays unchanged unless the implementation audit proves a shared contract impact.
- **Milestone 2 is merged and accepted.** Owner ran the acceptance workbook 2026-08-10: 11 of 11 Pass, closure "Accept Milestone 2", record in `docs/features/menus/m2-acceptance-record.json`. One independent review, three blocking defects, all fixed at `4c61aa2`; the owner waived the second review that the first had asked for and closed the milestone on it. **Milestone 3 is unblocked.** Detail in §Milestone 2 — built and accepted.
- **The register has one open question again: Q209**, deferred by the owner at M2 acceptance. The ⋯ card actions sit over the board and, now that Q98 removed the venue-name strip, they cover guest content — the first item's price on the accepted build. It ships on its provisional default until settled.
- **The save model is settled: the draft is derived, not authored** (owner decision, milestone-plan §The save model). The live rows are the working state; the screens show the last published snapshot; the draft is the computed difference. Migration 058 creates no draft table, and the legacy editor now writes through `Items`/`Placements` so no path can change a screen without a publish.
- Backlog issues #670–#683 hold the owner's out-of-scope decisions; do not silently implement them.
- Not yet approved, and inputs to any further planning: `docs/architecture/built-foundations-spec.md`, and the proposed product-surface inventory under `docs/design/proposed/` (Markdown plus a searchable HTML companion). Design references only.

## Read First

1. `docs/features/menus/milestone-plan.md` — milestones, cross-cutting rules, design follow-ups, quality gates.
2. `docs/features/menus/open-questions.md` — recorded owner answers; they govern over older prose.
3. `docs/design/approved/menus/README.md` + `decisions.md` — the design authority.
4. [M3-A Slices 4–6 planning workbook](https://docs.google.com/spreadsheets/d/1DCtCrn5NAXCTNt5csmrjAOJvcCws7l9fdsnGQUCHFkM/edit) — the owner's planning workspace. Agents plan in GitHub and the controlled repository records and do not edit the Sheet unless explicitly asked. Owner decisions from the Sheet must be synchronized into the repository before implementation.

## Exact Next Action

1. **Complete the claimed Slice 6 path audit, land the owner-supplied source image,
   then implement issue #710 on `feature/menus-s6-86-board`.** `PRODUCT.md` now holds
   owner-provided product truth. Reconcile every current availability entry point found
   by the recorded `rg` search (`DaypartHome`, builder, menu API, content API/service/
   repository, tests) and the existing Menus Home create path. Do not pull deferred
   Board View/Play #709, Slice 6-A import, or Slice 7 Menu Home redesign into Slice 6.

2. Slice 2 is owner-accepted. Its first review blockers were a case-only section
   rename no-op and stale page-history response overwrite; both have focused tests
   with observed red/green evidence. The owner waived further review. Item-change
   history remains with its later M3-A owner; #701 and #702 track the other explicit
   follow-ups.

3. Standing owner decisions carried out of Milestone 1: audit record kept as is (#677),
   legacy columns kept, and the three menu capabilities to become separately grantable
   (#686).

4. The screen-conflict rule, settled 2026-08-09: a screen another menu now owns is never
   touched by a stale act, and the conflict is always named — publish leaves it alone and
   reports it, restore refuses.

5. The shelf rule, settled the same way: nothing puts a menu on a screen except a
   deliberate, ceiling-checked put-back, and nothing takes a menu off the shelf while a
   screen is still showing it. "Still on a screen" means the **published** snapshot names
   one that no other menu has since been given — not merely that an assignment row exists
   — so putting a menu away requires take off, publish, then put away. A shelved menu
   stays editable and its draft stays discardable; only a restore that would put a screen
   back is refused.

## Verification

Exact-head GitHub Actions were green at `2977bc3` — 13 checks across
`phase02-tests` and `ui-regression`, Playwright included — and that is the head
that merged. Earlier commits on the branch carried `[skip ci]` at the owner's
instruction; that no longer applies.

Local runs against real LocalDB and a running product cover what CI's standing
exception skips: unit tests, the data integration suite on a database migrated
from scratch, both UI suites, the Playwright specs and the owner demo. At the
merged head the local runs were 412 unit, 56 data integration, 109 back-office and
98 platform-operations; the Playwright specs and the demo runner were covered by
CI rather than locally.

Local execution and independent review together caught defects green CI missed,
including a phantom assignment count from PowerShell turning an empty JSON array
into `$null`, a migration-script list test failing since script 052, a publish that
recorded a shipped set from a different reading of the menu than the snapshot it
committed, a torn read of the published snapshot and its version, and a menu that
could be shelved with its take-off still pending — leaving a screen showing content
no remaining act could clear. Every one is fixed with a regression test **verified
to fail with its fix reverted**; that check is part of closing a finding, not an
optional extra.

## After Milestone 1 — a retrospective item the owner named

Five consecutive independent reviews (#2 through #6) each found real defects in
work that had just been declared finished, and the throughline is consistent: the
tests written with a fix prove the case its author had in mind and stop there,
rather than attacking the next step in the sequence — publish twice, assign a
put-away menu, change only the letter casing, shelve a menu between a take-off and
the publish that carries it. Review #6 is the sharpest instance: the trap it found
was written up as a *passing test*, which asserted the refusal it hit and never
asked what the screen was still showing. Reviews are catching what the author's own
tests do not. **Decide what to change about how work is verified before it goes to
review** — owner instruction. Taken up and completed 2026-08-09: the invariant sweep
and customer-visible acceptance assertions were adopted (see the remediation section
below) and the rules are folded into `AGENTS.md` §How to Work a Task and §Where a
test lives. This item is closed.

## Menus M1 verification remediation — 2026-08-09

Merged to `master` and pushed 2026-08-09 with `[skip ci]` (CI suspended by owner
decision; local verification was the gate).

**What this established.**

- **LocalDB is the default database everywhere**, in tests and in CI. Azure is reached
  only by setting `VENU_TEST_AZURE_SQL_CONNECTION_STRING` for that run. A gitignored
  `app.settings.json` used to supply an Azure connection string, so every "local"
  integration run silently went to a shared remote database: 96 seconds against Azure
  versus 4 on LocalDB, non-hermetic, and flaky in a way that read as product flakiness.
  The settings file is still read for its other toggles but can no longer choose the
  database. The fixture creates and migrates the database itself, so a fresh machine
  needs no setup.
- **A run with no database fails.** Fifty-three `if (!fixture.IsAvailable) { return; }`
  guards are gone. A suite that cannot reach a database is not a passing suite.
- **The in-memory repository decides nothing.** It re-implemented seven refusals in C#,
  and eight of the nine unit tests over it had a twin in the SQL suite — two under the
  same test name. It drifted, which is why review #6's defect survived 412 green unit
  tests. It is now storage plus an explicit failure seam: it is told when to fail and
  never judges. Refusals are asserted where they are enforced, in SQL.
- **Unit tests keep only what has no database in it**: the publish retry loop and its
  four-attempt bound, and refusal wording as the pure function it is.
- **`ModelInvariants` runs after every integration test**, against whatever state that
  test left behind, via the `InvariantCheckedTests` base class — no author action, by
  design. Seven rules, each traceable to the review that paid for it. It found a real
  defect on its first run: a publish could record a `ChangeCount` its own shipped set
  did not contain, because the two were separate parameters. The count is now derived
  from the shipped set inside the statement, so they cannot disagree; `PublishAsync` no
  longer takes it.
- **`GET content/screens/showing`** answers what a screen is showing, from the
  delivery rows and the published snapshot, never from the assignments. The milestone's
  central claim had no read behind it, which is why the demo could report 12 of 12
  while a screen sat stranded. The demo now asserts the screen at checks 4, 6, 8c and 8d.

**What was assumed.** That the screen/venue/pairing domains keep only the shared
invariants (tenant scope, one menu per screen) and get no domain rules of their own —
this work did not study them, and inventing invariants there would manufacture
confidence rather than earn it. Say so before adding any.

**Left deliberately, and for whom.**

- **The `sqladmin` password for `dev-vennusign.database.windows.net` is recoverable from
  this public repository's history** (added in `cf730c5`, removed in `05e35cc`). Removing
  the file did not unpublish the secret. **Rotate it on the Azure side** — owner action;
  no branch change fixes it.
- **Milestone 1's owner acceptance: recorded.** The owner accepted 2026-08-09 with the
  demo run (12 of 12, screen assertions asserting) as the acceptance record; no
  separate re-run was required.
- **Browser validation of rendered content waits for milestone 4** and is written into
  `milestone-plan.md` as a gate there. No screen work, no browser work.
- Door enumeration, one-read-one-lock for paired values, and records-in-the-same-commit
  were recorded as written guidance rather than gates, by owner decision.

**Open question.** `Measure-Api` in the demo runner had the same latent PowerShell array
trap that once produced a phantom assignment count, and it surfaced again here: 5.1
emits a JSON array as one object, and the shape changes with row count, so a reader
correct against one row starts lying at thirteen. All list reads now go through
`Expand-Api`. Other scripts in `scripts/` have not been audited for the same pattern.

## Migration chain squashed to a baseline — 2026-08-09

Merged to `master` and pushed 2026-08-09 with `[skip ci]` (CI suspended; local
verification was the gate — the four proofs below).

`src/Vennu.Data/Scripts/` holds one file: `001_baseline.sql`, the fifty-nine migrations
in the order DbUp applied them. Every statement in it already ran, so it is a collapse
rather than a rewrite. New migrations continue from 059.

**Deleting a migration never un-applies it.** DbUp decides what to run by journal name,
so a database that ran the old chain would see the baseline as new work and fail on its
first CREATE TABLE. `DatabaseMigrator.BaselineExistingDatabase` records the baseline as
applied wherever the superseded chain is already recorded, and executes nothing against
such a database. A database part-way through the old chain is **refused** with a message
telling the operator to finish on the previous release first — marking it complete would
leave it permanently short of whatever it never reached.

**Proved, not assumed.** A reference database was built from the old chain and
fingerprinted (1,166 lines: columns with types, nullability, defaults and collation;
indexes with filters and included columns; foreign keys with actions; check and default
constraints; seeded row counts). Results:

- fresh database from the baseline vs the old chain — **no material difference**;
- a database with the old chain journaled, migrated by the new code — **schema changed by
  0 lines**;
- a database stranded mid-chain — refused;
- a control, the old chain against itself in two databases — 0 differences, which is what
  makes the comparison trustworthy;
- eight concurrent migrations against one database — exactly one baseline row.

**Two pieces of dead work removed.** Script 012 created `dbo.MenuItemTranslations` and 058
dropped it; 013 added `MenuItems.AvailabilityResetUtc` with its index and 058 dropped
those. Every new database built both and demolished them. The baseline never creates them.

**One accepted difference.** Eleven tables declare `DEFAULT NEWID()` without naming the
constraint, so SQL Server generates the name from the object id. Creating one table fewer
shifts those ids, so a database built from the baseline carries different `DF__` names
than one built from the old chain. Nothing in the codebase reads a generated constraint
name. Naming them explicitly would make fresh databases deterministic and is worth doing
the next time this file is opened.

**A defect this introduced and then fixed.** The first version checked the journal and
then inserted as two steps. Startup calls the migrator concurrently, so the first real
database got seven identical rows, and adding a lock hint to the check still allowed two.
It is now serialised behind a named application lock — verified with eight concurrent
migrations. This is the third read-then-write race this session; the owner filed
"one read, one lock for paired values" as guidance rather than a gate, and the evidence
now argues for a gate.

**Still open.** `AuthorityRoles`, `AuthorityRolePermissions` and `LayoutTemplates` are
created and seeded but read by no product code — only by a test asserting the script's
text. That is Track 1 scoped-authority work and owner-closed, so it was left alone rather
than judged on grep. If they are genuinely dead, the correct removal is a **new** migration
that drops them, so existing and fresh databases converge; deleting them from the baseline
would only change new databases.

## Milestone 2's first design decision — the theme model

Owner correction, 2026-08-09, recorded against Q86 and Q98. **Menu themes and shell themes
are categorically different things**, and the code currently confuses them.

- A **menu theme** is attached to a menu. A venue may have many. None exist yet; they are
  built later in the theme editor. Milestone 2 ships **no named looks** — the render
  engine consumes a theme definition so later themes need no engine change.
- A **shell theme** is the software's own look — today's sky blue, a dark variant later.
  That is what "venue theme" should mean, and it is **milestone 2's actual theme
  deliverable**: nav rail, tokens, chrome. One ships, built so others can be added.
- **A menu with no theme attached is a valid state.** The engine renders it — plainly and
  badly, which is acceptable — but never blank, never a silently invented fallback, never
  a failure.
- **A menu theme is created in the theme editor and attached in the menu editor.** The
  menu editor never authors a theme. The theme editor (`ThemeBuilder.tsx`, route `themes`)
  is the existing surface.
- A venue-name title strip on the TV, if it exists, belongs to the **theme editor**. The
  Menus render engine neither draws one nor assumes one.

What the code says today, which contradicts that:

- **No menu-theme table exists.** `git grep -cE "CREATE TABLE dbo\.(MenuThemes|BoardThemes)"`
  against the baseline returns 0.
- **`Menus.Theme` is free text** — `NVARCHAR(40) NOT NULL DEFAULT N'coastal'` — naming a
  look that was never built, with no table behind it. Since an unthemed menu is a valid
  state, `NOT NULL DEFAULT 'coastal'` is now wrong twice over: it forbids the blank case
  and defaults to a fiction. Whatever the model becomes, that column changes.
- **`dbo.VenueThemes` holds board-render fields** (`BoardBackgroundColor`, `SectionColors`,
  `GlowColor`, `TitleFont`, `ItemFont`): menu-theme data under the venue-theme name, one
  row per venue. Read by `DisplayContentResponse` and by the back-office and
  platform-operations theme contracts, so moving it is not free.

This is the recurring shape — one name carrying two meanings, and a value with no referent.
**Settled — owner decision, 2026-08-09: milestone 2 defers the MenuThemes table.** The
table arrives with the first milestone that reads one (M3's picker / the theme editor),
so its shape is designed when its real user exists. M2 ships migration 059 making
`Menus.Theme` an honest nullable attachment slot: default dropped by dynamic constraint
lookup, `'coastal'` removed from rows **and** stored snapshots (else every menu wakes
with a phantom theme draft change), and `RestoreSnapshotSql`'s `ISNULL(t.Theme, m.Theme)`
fixed so a null theme restores as null — regression test verified to fail with the fix
reverted. `VenueThemes` keeps its board-render fields untouched until the milestone that
moves them.

## Milestone 2 readiness pass — 2026-08-09

The owner asked that M2 be put through the dev process before coding. Three exploration
sweeps (frontend/shell, design authority, content API/test harness) plus a structural
design pass. Findings and decisions, all recorded in issue **#687**:

- **Owner decisions:** defer the MenuThemes table (above); the render engine lives at
  **`src/board-engine/`** — a new top-level shared folder, imported by relative path
  from back-office in M2 and the display player in M4 (the platform-operations
  cross-app import is the precedent). The engine imports nothing from either app; data
  arrives as props.
- **"Spine" is retired; the model is named "content"** (owner, 2026-08-09). The data
  model and API are *content* — items, placements, availability — and "menu" is the
  operational context using it, which the capability IDs already said
  (`content.item.update`, `content.menu.manage`). Landed as milestone 2's step 0,
  before any frontend client was written against the old name: route
  `api/back-office/menu-spine` → **`api/back-office/content`**;
  `BackOfficeMenuSpineController` → `BackOfficeContentController`; `MenuSpineService`
  → `ContentService`; `MenuSpineContracts` → `ContentContracts`;
  `IMenuLibraryRepository`/`MenuLibraryRepository` → `IContentRepository`/
  `ContentRepository`; `FakeMenuLibraryRepository` → `FakeContentRepository`; the test
  classes and the demo runner with them. **Historical names stay as history**:
  milestone 1's title, the `feature/menus-m1-spine` branch, PR #685, the
  `058_create_menu_item_library_spine.sql` header inside the frozen baseline, and the
  recorded register answers are not rewritten.
- **Step gates, not a testing phase** (owner, 2026-08-09). Tests are written with each
  step and each step ends on its own green gate before the next starts — schema on both
  a fresh and a previously-migrated database; the API exercised with real requests
  before any UI consumes it; the engine on its render invariants; the shell on both app
  builds plus existing nav specs; the shelf on new Playwright specs. The full local
  gate, review and workbook run at close. Recorded on #687.
- **Backend gaps M2 must fill before the shelf UI can be honest:** no frontend client
  for the content API exists at all; no menus-list read (the legacy `GET /menus` drags
  every section and item and loses "MP" price fidelity); nothing exposes a published
  snapshot to render; `HistoryEntryResponse` carries no `Version` so Go back to… is
  unreachable; no duplicate operation exists (semantics owner-settled in Q20). Route
  shapes are in #687.
- **Named to settle inside the milestone, not silently:** the never-published card
  state, and the Duplicate name-collision/length default.
- **Test facts:** the 20-screen/13-menu seed (Q176) does not exist — it enters as
  `POST /api/test/seed/scale` composing product write paths against a dedicated scale
  venue, never fixture SQL that re-implements snapshot JSON. `navigation-shell.test.mjs`
  hard-codes the current 9-route/4-group nav and changes with the rail, deliberately,
  in the same PR. The running 18-criteria checklist now exists at
  `docs/features/menus/acceptance-criteria.md`.
- **Token batch-2** now has its artifact:
  `docs/design/approved/menus/proposed-token-additions-batch-2.css` (Q178, including
  the `#2a78d6` selection token; board palette deliberately excluded — it belongs to
  menu-theme definitions).
- **Stale records corrected in this pass:** this file's "not pushed" notes (both
  batches are on `origin/master`), the provisional Q86/Q98 framing, the completed
  retrospective instruction, the register header's "Deferred: Q86", the design README's
  five-item card menu (Q195), eyebrow colour (Q184), icon instruction (Q185) and
  criterion-4 wording (Q187), the batch-1 token file's "NOT APPROVED" header
  (build-decision 8), and `PROJECT_STATUS.md`'s validation policy (CI suspended).

## Milestone 2 — merged and accepted — 2026-08-10

PR **#689**, issue **#687**, branch `feature/menus-m2-shell-render` (deleted). 84 files,
+6,563/−311, in thirteen commits: step 0 retired the word "spine", then schema, content
API, engine, shell, shelf, browser specs, workbook, critique, review fixes. Everything
before the merge from master carries `[skip ci]` (CI suspended by owner decision).

**What it delivers.** The 76px icon rail and shell theme; `src/board-engine/` — a pure
board renderer shared by both apps, laid out once at 1920×1080 and scaled; Menus home,
where every card is a live render of what that menu's screens are showing, with the
scale cutover at seven; the six card actions; four new content reads/writes; a
20-screen/13-menu Playwright fixture; and criterion 18's named spec.

**Owner acceptance, 2026-08-10.** 11 of 11 Pass across four journeys, closure "Accept
Milestone 2". Record kept verbatim at `docs/features/menus/m2-acceptance-record.json`,
including the owner's screenshot. Criteria 5, 6, 8 and 18 are now confirmed against the
running build, not only by their specs. One note came out of it — **Q209**, deferred.

**What the review process has established, and it is worth carrying into M3.** Browser
and screenshot verification caught four defects that unit tests did not, and three of
them were the most serious in the milestone: boards that rendered blank because board
type had been set from card-sized measurements, never-published cards claiming "5
changes not published", a locked chip spilling out of the rail, and a filter counting
three while the shelf drew two. This is the same failure mode M1's retrospective named.
Owner instruction, recorded: **from M3, browser assertions ship with the surface, not a
step later.**

**The second review was waived — owner decision, 2026-08-10.** The first review closed by
requiring a fresh review of the resulting head; the owner judged that review sufficient to
close the milestone and declined a second. Recorded plainly because the consequence is
real: the three fixes at `4c61aa2` were verified by their own tests and by the owner's
acceptance run, but they were not themselves independently reviewed. Milestone 1 needed
five reviews and milestone 2 needed one, which is the evidence the owner weighed.

**Known and deliberate.** Four test failures sit outside the Menus suites — the three on
**#688** plus an E2E pairing assertion found in this run, all four verified pre-existing
by stashing every M2 change and re-running. `#688` now covers all four; neither suite is
in the routine gate, which is the actual defect.

## Milestone 3 readiness pass — 2026-08-10

The owner asked that M3 go through the dev process before coding, as M2 did, and — being
asleep — that ambiguity be resolved by judgment rather than left blocking. Every call
below is **provisional and cheap to overturn**; each names its reasoning.

### The complete user behaviour

*A person opens one menu, changes what it says — a price, a name, an item, a section,
the order things sit in — sees the board change as they type because the canvas is the
board, and then decides, deliberately, to put it on the screens.*

Immediately before: the shelf card (M2). Immediately after: the screens, via Publish;
or back to the shelf via the breadcrumb. The same behaviour lives in three other places
that must agree — the shelf's card render, the publish diff, and (from M4) the TV.

### Path map

**In:** card click · `#/menu/{menuId}` deep link **(new — the builder gets an address)** ·
back from Play · redirect after create (M6). **Unvalidated today:** all of them; the
builder does not exist.

**States that must render:** empty menu (no sections) · section with no items · item with
no price (quiet flag, publish not blocked, Q113) · 86'd item (selectable, editable,
red-tinted panel, Q104) · nothing selected (inspector holds its place, Q106) · never
published · put-away menu open for editing · loading · API error · save failure (amber
byline, retry, Publish blocked, Q197) · 401 mid-edit (holds the change, sends after
sign-in, Q199) · permission denied · no screens paired ("No screens yet", Q101).

**Refusals the UI must speak:** ceiling reached (items per menu) · name blank or >200
(reverts on blur, Q119) · description >1000 · publish conflict (a screen another menu now
owns) · publish while a save is unconfirmed · stale act after someone else published.

**Out:** Publish · breadcrumb to the shelf · Play (visible, honest blocked state, Q102) ·
browser refresh mid-edit · leave and return.

### Invariants M3 gains

- **An item appears at most once on a board.** The schema enforces once per *section*
  (`UQ_Placements_SectionItem`) but not once per *menu*, so Q112's "picking it jumps
  instead of duplicating" is currently a UI promise with nothing behind it.
- **No two placements in a section share a sort order** — otherwise board order depends
  on a tiebreaker nobody chose.
- **A deleted section leaves no placement behind**, and never deletes an item.
- **Every placement's section belongs to its menu** — already enforced by
  `FK_Placements_SectionOnMenu`; asserted so a future schema edit cannot quietly drop it.

### Ready

- The derived-draft model means **the builder needs no draft plumbing at all**:
  `MenuSnapshot.Diff` already compares name, theme, dwell, loop warning, screens,
  sections, items and placements, so every builder edit produces its own draft change
  and the count cannot disagree with what Publish ships.
- `IContentRepository` already carries most of the writes: `CreateItemOnMenuAsync`,
  `CreatePlacementAsync`, `RemovePlacementAsync`, `ReorderPlacementsAsync`,
  `UpdateItemAsync`, `GetItemsAsync` (the library search), `GetPlacementsForItemAsync`
  ("also on Late Night"), `GetWorkingSnapshotAsync` (the canvas's board).
- The board engine renders the canvas as-is — `BoardSurface = "preview"` already exists
  for the annotations flag (Q135).
- Design authority is production-detailed: four columns at 212/flex/296, the six
  inspector controls, the publish bar, the selection ring `#2a78d6` (already a token).

### Decisions taken in this pass

1. **Design follow-up 1 is Q5, not Q76** — the milestone plan cites the wrong question.
   Q76 is refresh cadence; **Q5** carries the flag ("the editing flow must feel easy —
   possibly a quick price-change mode — design follow-up required before slice 3 builds
   the inspector flow").
   **Resolved without inventing a mode:** a shared item's inspector states the fact
   quietly and permanently under the price — *"Also on Late Night and Brunch — they show
   the new price when you publish them"* — reusing Q123's locked vocabulary (two names,
   then "on 3 boards"). No dialog, no confirmation step. A modal on every price edit is
   the opposite of "feels easy", and a separate quick-price *mode* is undesigned, named
   in no milestone, and would be the second editor that decision 15 and M2c's read-only
   rule both exist to refuse.
2. **The builder gets its own address**, `#/menu/{menuId}` — closing the note M2 left in
   `App.tsx`. Refresh mid-edit and Back both survive, which the DoD navigation group
   requires and today's `editingMenuId` React state cannot do.
3. **Menu themes: still no table, and no attach write.** The picker ships and shows
   Q86's empty state from `GET content/menu-themes` → `[]`. A theme that cannot exist
   cannot be attached, and creating an empty table with no writer repeats exactly the
   dead-schema problem the migration baseline flagged (`AuthorityRoles`,
   `LayoutTemplates`). Table and attach land with the theme editor that first writes one.
4. **`BackOfficeMenusController` is retired**, its builder-relevant writes consolidated
   onto `api/back-office/content` — one base for one model, finishing step 0's rename.
   `run-m1-demo.ps1` and `BackOfficeMenusControllerTests` move with it. The legacy
   `MenuSectionsEditor`, `MenuItemsEditor` and `QuickUpdateMode` components go in the
   same PR, with their specs **rewritten, not deleted** — `menu-save-race.spec.ts` guards
   a real stale-overwrite race and must be re-expressed against the builder's save model.
5. **Sections are deleted, not archived** (Q96). `MenuSections.IsActive` loses its last
   writer; the migration hard-deletes any `IsActive = 0` section and its placements,
   names what it discards, and drops the column. Leaving the column and its
   `IsActive = 1` filters would mean a future writer of 0 silently changes a live board.
6. **Reorder becomes one guarded write.** Both section and placement reorder today read
   the current set, validate completeness in C#, then write — unlocked. A concurrent add
   between the two makes the write describe a set that no longer exists. This is the
   **fourth** instance of this codebase's most common defect shape ("two values that must
   describe the same instant are read once, under one lock").

### Gaps M3 must close before the builder can be honest

- No working-board read: the canvas needs the menu **as it stands**, not the published
  board the shelf draws. `GetWorkingSnapshotAsync` exists; nothing exposes it.
- No section delete-that-releases-placements; no placement remove wired to a UI;
  no library search read; no "which boards is this item on" read.
- `ReorderPlacementsAsync` trusts a partial list: omitted placements keep stale sort
  orders and can collide. The service validates completeness, but outside the write.
- `MenuItemManagementService.ReorderAsync` reports "Menu section does not exist" for a
  section that exists and is merely empty.
- Undo/redo has no model. Design: every builder mutation is a command carrying its
  inverse; ⌘Z issues the inverse write; session-scoped, capped, never persisted, never
  named in settings (decision 7). A failed inverse says so rather than clobbering.
- ⌘K (Q121), the "Viewing as" list (Q101), the bulk-place drawer (Q95/Q124) and the
  publish bar's per-screen chips with the ≤6 cutover (Q161/Q167/Q168) have no code.

### Records that state something untrue (fixed at M3 start)

- `milestone-plan.md` design follow-up 1 cites **Q76**; the flag is **Q5**.
- `README.md` (design authority) M2 inspector still lists "two checkboxes (Feature on
  the board, Add a photo)" and calls them "**Six controls total**" — Q107 and Q108 put
  both out of scope, so the inspector has four.

## Milestone 3 — built and gated — 2026-08-10

Branch `feature/menus-m3-builder`, issue **#690**. Three steps, a gate each, then a
critique pass and the full gate.

**What it delivers.** The four-column builder at its own address `#/menu/{menuId}`:
a section rail that navigates, a canvas that IS the preview, an inspector of four
controls, and the publish bar. Adding items with search that jumps rather than
duplicates. Undo and redo. ⌘K over the board. The theme picker's honest empty
state. Migration 061, which moved two rules the product only promised into the
schema. Nine content endpoints where every rule is decided inside the statement
that writes it.

**Retired with it**, per AGENTS.md: `MenuSectionsEditor`, `MenuItemsEditor`,
`QuickUpdateMode`, the legacy section/item routes and their client, and four
back-office specs — with their specs **rewritten, not deleted**.

**Six decisions taken by judgment**, all provisional and recorded in #690, because
the owner asked that ambiguity not block progress overnight. The one most worth an
owner's eye is Q5's design follow-up: shared-price editing "must feel easy", and
the resolution was a quiet statement of fact under the price rather than a
confirmation step or a separate quick-price mode.

**What the browser caught that 190 unit tests could not**: board type six times too
large, a second page header stacked over the builder, a selection ring that never
drew, duplicate exports the typecheck passed and the bundler refused, and — from
milestone 2's shelf — an amber strip that swallowed clicks, so the one menu you
most wanted to open was the one you could not.

**The critique pass** against `docs/design/approved/menus/README.md` §M2/§M2a found
five gaps and all five are closed: redo was a disabled button, "Viewing as" was a
label rather than a dropdown, the 86 note had no time, and "go back to…" and
"Review first" were missing from the publish bar.

**Not shipped, and named:** the overflow warning ("Two words over — wraps to 3
lines on Patio") needs reported screen geometry, which arrives in milestone 4.
Cross-section drag waits for milestone 5 (Q103) — **within-section item drag does
not, and was missing until the review; it ships now.** Keyboard reorder (#672) and the
full add-row keyboard flow (Q122) stay backlogged; the rail keeps its ↑/↓ buttons
until #672 lands, because replacing them with a drag handle first would remove
reordering from keyboard users entirely.

**Gate.** 433 API unit · 89 data integration on a fresh migration with the
invariant sweep · 190 back office · 98 platform operations · 118 Playwright across
desktop and mobile · 21/21 builder API checks over real HTTP · M1 demo 12/12. The
four failures on **#688** remain, all pre-existing.

## Milestone 3 — what the independent review cost — 2026-08-10

The owner's chosen agent reviewed PR #691 at `d521cd4` and returned
**REQUEST_CHANGES with seven findings. Every one was real**, verified here before
anything was changed. An eighth was found in the review prompt itself. All eight
are fixed at `b59d2d1`, each with a Playwright spec that was **run with its own fix
reverted and observed to fail**.

**The gate had a hole exactly the shape of the worst finding.** `npm run build`
failed with three `TS2322`s on `inert`, and nothing ran it: `validate.ps1` built
`src/display` and had never heard of `src/back-office`. 190 unit tests and 130
Playwright specs were green against a branch whose back office did not compile,
because the dev server transforms per module and never type-checks the project.
`validate.ps1` now builds and tests **both** front ends. That is the durable fix;
the `inert` typing itself is one declaration file.

**Two recorded answers were named in the milestone plan and simply not built.**
Q197 (a failed save retries automatically, Publish waits) and Q199 (a 401 shows a
sign-back-in prompt, holds the change, sends it after). Both existed as copy — the
amber byline was drawn — with no mechanism under them. A byline that says
"retrying…" while nothing retries is worse than an error, because it is a promise.

**Undo was a blind overwrite.** It sent the whole previously-captured row
unconditionally, so undoing your own price edit erased a colleague's name,
description and price along with it, silently. Inverses now carry the values they
expect to find, compared under the lock that writes; `item_changed` comes back in
the server's words. The catch block had claimed this protection for weeks — it
could never fire, because an unconditional write does not fail.

**Three named M3 behaviours were absent**: Q96's rename-by-clicking-the-canvas-
heading (the heading was a `<p role="presentation">`), Q124's "Add many at once"
drawer, and Q103's within-section item drag — where the pill was drawn correctly
on hover and on selection all along and dragged nothing, with `reorderMenuItems`
imported by the builder and called from nowhere.

**Two lessons worth carrying, both about evidence.**

1. *A do-not-file list is an authority claim, and mine was wrong.* The review
   prompt told the reviewer that item drag was milestone 5 "per Q103". Q103 defers
   only **cross-section** moves. An in-scope gap was placed out of bounds, in the
   same document that warned the reviewer against citing a recorded answer they
   had not read. **Quote the register entry into the prompt itself** — if the words
   have to be pasted, the mistake cannot survive being written down.
2. *A spec can pass against the defect it names.* The first version of the 86-note
   spec 86'd two items and asserted each had a note — which the one-shared-string
   bug also satisfies, because both rows got a note. It needed a test-support
   endpoint to backdate one 86 before the notes could differ at all. Written and
   run, it would have been evidence of nothing. **Revert the fix and watch the spec
   fail** is not a formality; it is the only thing that distinguishes the two.

And one the browser caught that nothing else could: moving the 86 notes into a
`useMemo` below the loading early-return changed the hook count and blanked the
entire application. 190 unit tests passed and the production build completed while
the app rendered nothing at all.

## M3-A Slice 1 reconstruction — 2026-08-11

Independent review established that Slice 1 had been tested only in an uncommitted
working tree at Slice 0 SHA `179de5f`. All 49 dirty files were first preserved in
local safety commit `4aa0168`; the reviewable Slice 1 tree was then reconstructed on
`feature/menus-m3a-s1-pages`. Partial Slice 2 page-history/section-reassignment work
and the Slice 6 import landing remain only in that safety commit and are not claimed
as shipped. Issue #696 owns Slice 1.

The work-plan dependency was made explicit: page-shaped Test API seed support lands
with Slice 1 because the separate Test API must delegate to the real page schema and
product endpoints introduced here. The review's test-integrity findings are closed:
page reorder uses a stepped real pointer and was observed failing with `onDrop`
disabled; the dead/mojibake selector is gone; Q181 singular/zero copy is enforced;
and browser coverage now includes populated deletion, Cancel, copied content and
cross-menu naming. LocalDB now asserts exact FK/unique SQL errors and concurrent
page-item uniqueness. Pre-commit focused evidence: page Playwright 12/12, page
LocalDB 2/2, back office 196/196 and production build. CI remains suspended.

Exact next action: commit and push Slice 1, open its PR, rerun gates at the committed
SHA, then obtain an independent exact-SHA review before owner testing.

## Boundaries

- Milestones 1 and 2 are merged and accepted, so milestone 3 may start. Milestones 4–6 stay closed until their predecessor is merged and accepted in turn.
- Do not revive any cancelled track, phase or void work package without fresh owner approval.
- Do not implement backlog issues #670–#683 without owner scheduling.
- Design follow-ups (milestone-plan §Design follow-ups) must be resolved before the milestone that consumes them.

## Milestone 3 owner-acceptance remediation — 2026-08-10

**What this established.** The owner's first workbook findings are implemented.
Available items now show a plain availability switch; only the 86 state is a red
panel. The visible board handle is a real hit target, pointer drag works at human
speed, a scale-correct insertion line follows it, and the order survives refresh.
Section delete lives on each rail row, keeps its confirmation and library-release
message, selects the previous surviving section (or the first if the deleted row
was first), and leaves the empty-board add affordance when appropriate. Canvas-
heading rename is the only section-name editor. Case 6 is pre-seeded with Harbor
Lemonade on Acceptance Menu and Harbor Evening Menu; case 15 uses on-screen Undo.

**Evidence.** Each product regression was observed red against its unfixed code:
the available panel existed; a slow drag from the visible handle failed while a
row-centre drag passed; the rail-row delete did not exist; the duplicate field did;
and neutralising the deletion fallback left every remaining rail row unselected.
Restored fixes pass. The fixture ran twice and returned exactly one shared placement
on each named menu. Final local gate: back office 190/190 plus production build;
Playwright 142 passed / 12 explicit skips; builder API 21/21; M1 demo 12/12;
Data integration 91/91; .NET Debug retained only #688's known DataAccess 228/3 and
API 433/1 failures; .NET Release solution and display production builds passed.
CI remains suspended and was not used as a gate.

**Assumed and deliberately bounded.** The owner reaffirmed that mobile interactions
are out of scope (Q158/#681), so desktop handle drag and bulk placement are the M3
interaction gates; their mobile Playwright variants are explicit skips. Existing
mobile crash/layout coverage stays. Keyboard remains out of scope exactly as already
recorded; existing handlers were not removed. D1's truncated visual notes were not
chased, by owner instruction.

**Left for the owner.** Rerun `docs/features/menus/m3-acceptance-workbook.html`.
The existing record remains "Needs adjustment" and M3 does not merge on it. After
this set is handed back, provide the deferred visual notes for the promised second
pass. Milestones 4–6 remain blocked.

## M3-A Slice 1 owner-remediation — 2026-08-11

**What this established.** The page rail, overflow section picker, populated-page
deletion, and screen-assignment workflow were reworked from the owner's 4 Pass / 3
Needs Adjustment record. Add-page naming now uses the page-tab visual language;
section chips stay on one line with long names truncated and overflow in a bounded
More menu; populated deletion explicitly offers move or delete-sections while
retaining library items; and assignment management is a viewport-bounded,
scrolling panel with screen geometry/current page context, staged Save/Cancel,
and a recoverable nested rotate/replace choice. Delete-sections is enforced by the
product API/repository transaction, not only presented by the UI.

**Evidence.** Each new customer-visible regression was observed failing with its
production fix removed, then restored: page-name typography, non-wrapping chips,
delete-without-moving, and nested-choice focus recovery. Final local evidence:
back office 196/196; production build passed (existing Vite chunk advisory only);
desktop `menu-pages.spec.ts` 14/14; focused API 5/5; Test API 8/8. The Release
solution build passed with 21 pre-existing warnings. Azure/external integration
tests were skipped under the standing owner exception; the LocalDB deletion
regression was added but is UNTESTED in this run.

**Exact next action.** Push the committed remediation head to PR #697, obtain an
independent exact-SHA review, then regenerate the owner workbook against that SHA
and rerun only the three previously adjusted cases plus one surrounding-flow check.

## M3-A Slice 1 independent-review remediation — 2026-08-11

**What this established.** PR #697's exact-head REQUEST_CHANGES findings were
fixed across their full paths. Migration 062 now separates the placement column
addition from its carry update; legacy assignment test callers carry an exact page;
restore again refuses a screen acquired by another menu while preserving valid
cross-menu rotations; and snapshot expectations use the screen-plus-page identity.
Page deletion keeps the decision open after a move conflict and can recover by
deleting the page's sections. Assignment Save keeps staged choices after a refusal,
retries only transient failures, and exact-pair removal is idempotent. Capacity has
an inspectable Check fit result; six-section overflow, long-name identity, screen
location/status and cross-menu labels match the approved page workflow.

**Paths and evidence.** New, existing, duplicate-placement, populated/empty delete,
move conflict, Cancel, failed Save, retry-safe removal, same-menu and cross-menu
assignment, unassigned/landscape/portrait capacity, section overflow and focus
recovery are covered. The corrected migration carried a customer-shaped legacy
menu, section, item, placement, screen and assignment into Page 1 and
`DBCC CHECKCONSTRAINTS` returned no violation; the disposable database was removed.
Release solution build passed with existing analyzer warnings and no errors; back office 197/197 and
production build passed; desktop page Playwright 16/16; Test API 8/8; focused
snapshot tests 17/17; LocalDB data integration 94/94. DataAccess remained at its
known #688 baseline, 228 passed / 3 failed. CI remains suspended.

**Boundaries.** The untracked owner workbook was not modified or committed. Azure
and other external-service tests remain skipped under the standing exception.
Future tier authority and theme-authored fit measurements remain owned by their
later milestones; Slice 1 continues to use the documented maximum-tier defaults
and its deterministic fit model.

**Exact next action.** Commit and push this remediation to PR #697, obtain a fresh
independent exact-SHA review, and only after approval regenerate the owner workbook
for owner acceptance.

The exact-SHA review then found that Screen Assignments Save still issued one HTTP
write per screen. It now sends one batch to a single database transaction: every
screen and mode is validated before any assignment changes, replace/rotate/remove
and attributable history commit together, and any stale screen or refusal rolls the
whole Save back. LocalDB asserts a valid first change plus an invalid later screen
leaves no assignment behind; the browser regression keeps the staged UI recoverable
and verifies the prior screen owner remains unchanged.

## M3-A Slice 1 owner closure — 2026-08-11

The owner explicitly approved Slice 1 after an extended live visual pass against
the final nine exported M3-A screens. The page rail, section rail, canvas scrolling,
connected Screen Assignments surface, and canvas inline editing were reconciled in
the real browser. Section headings and every item name, description and price now
edit in place using the rendered theme typography. The exhaustive browser regression
creates three sections with twelve items each, edits all 111 fields while scrolling,
then refreshes and verifies persistence. Removing the canvas scroll-coordinate fix
made the test fail with displacement exactly equal to `scrollTop`; restoring it
returned the test to green.

Some section CRUD behavior assigned to Slice 2 was delivered early at the owner's
direction during this acceptance pass: section selection, inline rename, add,
real-pointer reorder, populated delete with reassignment, and delete confirmation.
Slice 2 must gap-audit and reuse it; it must not rebuild or double-claim it. Page
history remains Slice 2 work.

Local closure gate on the accepted tree: Release solution build passed with 0
warnings and 0 errors; back office unit 197/197 and production build passed;
LocalDB data integration 97/97; desktop `menu-pages.spec.ts` 18/18; desktop
`menu-builder.spec.ts` 40/40. The API suite passed 436/438: the Azure credential
test is excluded by standing owner policy and the existing #688 pairing-layout
expectation remains the other known baseline failure. Mobile and external-service
tests were not run by owner scope. CI remains suspended.

Exact next action: commit and push the accepted Slice 1 tree, obtain a fresh
independent exact-SHA review on PR #697, then merge and release the tracker claim.

The owner then explicitly waived any further independent review and directed that
Slice 1 be closed and Slice 2 begun. The interrupted review found no product
failure, but identified two controlled-record inconsistencies. They were corrected
before merge: owner override O1 in `decisions.md` and the action inventory now
authorize the accepted canvas inline editing, and the obsolete generated workbook
that named an earlier SHA and stale seeded menu was removed. The owner-approved
product candidate remains `1c52c2658966864d175b8666b0fc4722197afe92`;
the closure commit changes authority/acceptance records only.

## M3-A Slice 3 — implementation checkpoint — 2026-08-12

**Claim.** GitHub issue #703 is open. Branch
`feature/menus-m3a-s3-board-add-item` was created from `master` at `8bbafc2`; the
tracker claim is committed at `14600fd`.

**What is established locally, not yet complete.** Migration 066 widens the page
history vocabulary for item add/reorder/move/remove. Existing create, place and
reorder transactions now write page-attributed history; a guarded cross-section
move validates both live section orders and moves/history-logs atomically; removal
is page-scoped and preserves other-page placements. The existing board UI now calls
those paths, supports cross-section and empty-section drops, confirms “Remove from
this page” naming the page, accepts name then optional price in the add row, and
evaluates capacity against the typed draft item. Accepted board, geometry, fit,
selection, inline editing, inspector and page-history surfaces were reused.

**Executed evidence.** Back-office unit tests passed 197/197. Back-office production
build passed with the existing Vite chunk advisory. The Debug solution build passed
after updating the legacy removal regression. Focused LocalDB integration tests
passed 3/3: cross-menu preservation, cross-section atomic move/history, and same-menu
cross-page removal isolation. The first database attempt selected the ambient
`VENU_TEST_AZURE_SQL_CONNECTION_STRING` and correctly failed authentication; the
successful run cleared that variable for the process and used LocalDB. No credential
value was read or printed.

**Not done.** API/controller refusal and permission tests, full invariant write-path
coverage, complete desktop Playwright scenarios, exact UI styling/focus recovery,
real-browser inspection, red-with-fix-reverted demonstrations, Release/full local
gates, acceptance workbook, independent review, owner acceptance, push/PR and merge
are all outstanding. Nothing in this checkpoint is accepted or ready to merge.

**Exact next action.** Add focused API tests for create/place/reorder/move/remove,
including stale orders, cross-page/cross-venue identifiers, idempotent removal and
author/history mapping; then finish the desktop Playwright path matrix before the
bounded Impeccable browser pass.

## M3-A Slice 3 — gate and first-review result — 2026-08-12

Implementation candidate `a110bf51205ff31f428f283438caf047b00f4dd2` completed the
pre-review process in the owner-specified order: Release solution build, back-office
198/198 unit tests and production build, focused API 56/56, focused LocalDB item
rules/invariants, all 214 discovered Playwright cases across an isolated fixture
(Menus mobile explicitly skipped under Q158), and a browser visual audit against
the approved M3-A authority. The visual audit caught and corrected an add-result
popup clipped by the publish bar. The full gate also repaired stale fixture PageId,
isolated screen-key, token and screen-id assumptions. CI remains suspended.

The independent exact-SHA review then returned REQUEST_CHANGES, as intended: literal
search did not satisfy punctuation/spacing near-match, malformed source orders could
include the moved item, remove Undo appended instead of restoring order, route-boundary
coverage was incomplete, and controlled records were stale. Remediation now adds
canonical punctuation-insensitive ranking and a visible selected suggestion, resolves
the Enter/search race on both name and price, refuses duplicate/malformed guarded
orders, restores exact order on remove Undo with compensating removal on refusal,
and adds real-browser malformed/permission and Undo/Redo paths. Focused remediation
Playwright is 5/5 and focused LocalDB search/move refusal is 3/3.

**Still outstanding.** Rerun the affected/full gates on the remediation tree, commit
a new exact SHA, obtain independent re-review, then prepare the owner acceptance
workbook. No owner acceptance, PR, push or merge has occurred.

**Exact next action.** Complete remediation validation, commit the new candidate,
and send that exact SHA to the independent reviewer.

## M3-A Slice 3 — second-review remediation — 2026-08-12

The independent re-review of `c5679427d890a2d05e6824c6c55cd38f76012583`
returned REQUEST_CHANGES for one destructive concurrency window, incomplete active-
suggestion semantics, and records that still described already-completed gate work as
outstanding. Remove Undo/Redo now uses one database-guarded transition: it proves the
exact expected section order and page-wide absence/presence under the same locks that
insert or delete the placement and write history. A second actor's re-add, move,
reorder or removal therefore returns `order_stale` without changing placement or
history. The add input now exposes a combobox controlling a listbox with an explicit
active option.

Executed on the remediation tree: Release solution build succeeded; back-office unit
tests passed 198/198 and its production build succeeded; focused LocalDB stale Undo,
stale Redo and adjacent item rules passed 3/3; focused desktop Playwright passed 4/4,
including the accessible relationship and the second-actor stale Undo. A single-worker
68-case desktop attempt passed its first 29 cases, then LocalDB began aborting concurrent
session reads and the remaining cases failed at application setup; it is infrastructure
evidence, not a product pass, and is not counted. The earlier complete 214-case isolated-
shard gate remains the broad regression evidence; the changed paths have fresh focused
coverage. CI remains suspended and Azure/external integrations remain owner-exempt.

**Exact next action.** Commit this remediation candidate and obtain independent review
of that exact SHA. Owner acceptance remains after approval; no push, PR or merge has
occurred.

The third independent review requested keyboard arrow navigation for the add-result
combobox. The owner reaffirmed on 2026-08-12 that keyboard is out of scope. This is
already the controlling Menus rule in `milestone-plan.md` and Q122/#673 specifically
defers the add-row arrow/Enter flow. No new keyboard behavior or test will be built.
The structural semantics still apply: the listbox owns only result options, Create is
outside it, and expanded state describes the visible suggestion popup.

The third review's other product finding is fixed: the public guarded transition
route resolves the authoritative items-per-menu ceiling and the SQL transaction
counts distinct menu items under its placement lock. Restoring an item already on
another page does not increase that distinct count; adding a genuinely new menu item
at the limit returns `ceiling_reached` with no placement or history write. The focused
LocalDB regression is 2/2 with the stale inverse test, the service boundary regression
is 1/1, Release/build/unit gates remain green, and the four isolated desktop Menus
shards pass 68/68. The repository-wide isolated gate discovered 220 cases: 142 passed
and 78 were explicit mobile/keyboard scope skips, with no failures. One first attempt
at the stale-Undo browser case exposed that its second-actor POST asserted only HTTP
200, even though `already_on_board` also returns 200; it could therefore invoke Undo
before the asynchronous removal completed. The test now waits for removal and proves
the response is `placed` in the sibling section before Undo. That case and its full
shard pass in fresh isolated venues.

The external independent review recorded on issue #703 superseded the earlier COMMENT
and returned REQUEST_CHANGES against `e596d21e10f665a6232891aa78d17309a6b2bd21`.
It found three blockers: Enter treated any substring search hit as a near match and
silently discarded a typed price; price lacked an add-route server bound and the owner
corrected its maximum from 40 to 12 characters; and the selected-row removal control
from groups E/H was absent. It also found asymmetric canonicalisation, an empty painted
listbox, generic transition ceiling copy, and an unusable acceptance workbook notes flow.

The remediation in product SHA `0e7c54c94a62a51960c405693ddf42208a5bbafe`
makes reuse require canonical equality, announces when an
existing item's shared price wins, centralises SQL canonical search (including `&`),
adds migration 067 with refusal-before-narrowing and historical snapshot preservation,
enforces the 12-character API/domain/UI boundary, adds the selected-row removal action,
omits the empty listbox, uses tier-aware ceiling copy, and repairs workbook notes,
screenshots, advancement, gated acceptance and fixture instructions. Release/build,
198/198 unit, focused API/migration 2/2, LocalDB and focused browser pass. The full
isolated Playwright gate ran 16 fresh-environment shards: 220 cases discovered,
142 passed, 78 explicit mobile/keyboard scope skips, and zero failures.

Independent re-review APPROVED exact product SHA
`0e7c54c94a62a51960c405693ddf42208a5bbafe`. Its only remaining finding was
documentation-only: the workbook exposed inert screenshot inputs. Those controls are
removed and the workbook now states plainly that its JSON exports outcomes and notes;
any screenshots are saved separately. No further product review is required.

Owner acceptance exported `m3-a-s3-acceptance-record.json` against the reviewed product
SHA and returned **Not accepted**: case 1 Pass; case 2 Fail because add-item search
results disappear and do not restore across query changes/reopen; case 3 Needs
Adjustment but explicitly deferred by the owner to later planned Canvas work (#704);
case 4 Not run; case 5 Needs Adjustment because the stale-Undo setup was unclear.
The stale concurrency behavior already has deterministic Playwright coverage, but the
workbook must explain the expected refusal and setup more clearly.

The focused search investigation found no product-code failure: the owner fixture
promised `Old-Fashioned`, `Aussie Burger` and `Classic Burger` but contained none of
them, and the live `Old` API query therefore correctly returned `[]`. The fixture now
seeds all three as library-only items. Focused Playwright covers prefix results,
no-match, delete-back restoration, close/reopen restoration, punctuation reuse and
substring-safe creation; it passes 1/1 in a fresh isolated venue. Per owner direction,
no unrelated or broad test suites were rerun.

The owner reran acceptance: search and stale Undo passed. Whole page remains explicitly
deferred to #704, and the not-run removal workbook case was waived by the owner's final
acceptance. The sole requested close-out change makes Undo/Redo notices name the exact
item and page; focused desktop Playwright passes 1/1 at
`73074e030cf9c2d172b435aaeadfd0638bdb0793`. The owner accepted Slice 3, waived all
further review, and instructed merge with no CI. PR #705 is the closure PR.

PR #705 merged to `master` as `a3a421339670a3807a0c8418a2551752a1dcaaca`;
issue #703 is closed and the completed branch is deleted.

**Exact next action.** Begin no successor until its owner-approved plan exists.

## M3-A Slice 3-A — implementation handoff — 2026-08-13

Issue #704 was repurposed with owner approval for the bounded UI refinement between
Slices 3 and 4. The implementation is on
`feature/menus-m3a-s3a-builder-refinements`, based on accepted `master` SHA
`370bd9a4a0003769e9dbeb6c2b84afeab05578d5`.

The builder now replaces the repeated section-chip row with a `Page › Section`
context, compacts history and keeps `View all` beside its heading, uses borderless
page tabs with the sky-blue active underline, installs the Signal V and route labels
in the fixed 76px rail, and allows the Sections/History and Item panels to collapse
independently. Panel state is stored browser-wide under
`vennusign.menu.builder.panels` and survives reload and moving between menus; storage
refusal falls back to visit-local state.

The owner explicitly excluded canvas-renderer changes, an expandable app rail, custom
keyboard navigation, application-wide renaming, and all Slice 4 inspector,
availability and 86 behavior. The acceptance workbook at
`docs/features/menus/m3-a-s3a-acceptance-workbook.html` is for owner acceptance only;
an agent must not complete or sign it.

Local evidence recorded before publication: Back Office production build passed,
198/198 Back Office tests passed, diff/whitespace checks passed, and 16/16 focused
Slice 3-A contract assertions passed. The two affected Playwright specs compile and
enumerate, but browser execution in the authoring workspace was **UNTESTED** because
the Linux workspace lacks the repository's Windows LocalDB harness and its browser
download returned an empty archive. On 2026-08-12 the owner gave a one-time waiver of
independent agent and Playwright review for this special slice. The waiver applies
only to Slice 3-A and does not create a standing exception.

**Exact next action.** Pull and start Slice 3-A for the owner's acceptance workbook.
Do not perform agent review or Playwright review under this one-time owner waiver.

## M3-A Slice 3-A — owner adjustment handoff — 2026-08-12

During owner acceptance, four bounded builder refinements were requested: collapsed
Sections and Item rails now retain only their arrow control; page History follows the
section list instead of pinning to the rail bottom and no longer prints an empty-state
sentence; the screen-assignment control now presents a clearer status with a distinct
`Manage screens` action label; and the menu-name pencil now opens an inline editor that
persists the trimmed, venue-scoped name while refusing duplicates.

Affected Release API build, Back Office production build, and 10/10 focused API unit
tests passed. `git diff --check` passed. Per the owner's Slice 3-A exception, Playwright,
CI, and another independent review were not run.

**Exact next action.** Owner confirms the four acceptance adjustments on the running
Back Office, then Slice 3-A can be merged; do not begin Slice 4.

The owner's follow-up layout pass further reduced both rename editors to a single
underline, made History occupy the remaining expanded section rail without publication
metadata, restored centered vertical `Sections` and `Items` identities in collapsed
desktop rails, and moved active-page renaming into the top context beside the menu name
instead of replacing its tab. The Back Office production build and Impeccable layout
scan passed; browser execution remains waived for this slice.

The final breadcrumb correction keeps the top bar menu-only, places page rename in
the canvas context immediately before its three-dot actions, and uses the clearer
`Page / Section` hierarchy recommended by the Impeccable layout pass. The menu-name
editor explicitly suppresses the shared focus halo so its active treatment is one
underline rather than a box.

The owner confirmed the result is good and authorized merge and Slice 3-A close-out.
PR #706 is the closure PR. The accepted product head is
`b7f29481046d28f3d61878dd3b09e7d9c5ed56bc`; no further review, Playwright, or CI is
required under the one-time owner exception.

PR #706 merged to `master` as `cdfd2bbf7ad0d2211ebbd0d5c5914dff754a6583`;
issue #704 is closed and the local and remote completed branches are deleted. Slice
3-A is closed. The one-time review, Playwright, and CI exception is exhausted.

**Exact next action.** Do not begin Slice 4 until its owner-approved plan exists.

## Release engineering foundations — dev deploy pipeline stood up — 2026-08-16/17

Separate track from Menus: a branching/versioning/deployment discussion produced
significant additions to `docs/design/progressive-customer-cutover-concept.md`
(branching model — master/release/X.Y/hotfix; dev/stage version folders and version
chooser; git tag and codename conventions; automated MAJOR/MINOR/PATCH versioning
with AI-assisted release classification; per-component selective release; and the
Application Discovery Service, ADS, giving VR continuous (app, version) -> healthy
instance resolution). All still concept-stage per that document's own status line —
not approved scope.

Ahead of that, a real dev deploy pipeline was stood up and proven working today.
`.github/workflows/deploy-dev.yml` builds and deploys `api`, `back-office`,
`display`, `po` to their `vennusign-dev-*` App Services on push to `master`, gated
so PR-time test workflows and the deploy workflow never share a trigger. Backing
Azure OIDC identity (`vennusign-github-actions-dev`, federated credential trusting
`repo:jmiedreich-ux/Vennusign:ref:refs/heads/master`, `Website Contributor` scoped to
`rg-basic-website` only) was created out of band, not via any script in the repo.

`board-engine` was found to have no independent deploy target — it is a shared
source library imported by `back-office` (and likely `display`) via a tsconfig/vite
path alias, not a standalone app. The `vennusign-dev-board-engine` App Service and
its subdomain, created earlier in the same session before this was discovered,
remain unused; nobody has decided yet whether to remove them.

The first real deploy run surfaced three problems, all now fixed directly in Azure
(none via a repo-tracked script yet):

- `vennusign-dev-api` had no `ConnectionStrings__VennuDatabase` app setting. Set to
  point at the existing `dev_vennusign` database on `dev-vennusign.database.windows.net`
  (australiaeast; a different region from the App Services' Central US, not yet
  addressed). The firewall already allowed Azure services, so this alone should have
  been sufficient.
- The three static SPA apps (`back-office`, `display`, `po`) are Vite builds with no
  server process, deployed onto Node-runtime App Services that had nothing to execute.
  Fixed by setting the startup command to `pm2 serve /home/site/wwwroot --no-daemon --spa`
  on all three.
- `vennusign-dev-api` still failed to start after the connection string fix, with
  zero application log output across several restarts. Traced to
  `DatabaseMigrator.Run` (`src/Vennu.Data/DatabaseMigrator.cs`): it opens a SQL
  session and blocks on `sp_getapplock` (session-scoped, 180s timeout) before doing
  anything else, and produces no console output until *after* that lock is acquired.
  Repeated restarts during troubleshooting almost certainly piled up orphaned
  session locks from hard-killed containers, compounding the wait each time. Also
  changed `linuxFxVersion` from `DOTNETCORE|10.0` to `DOTNETCORE|9.0` to match the
  app's `net9.0` target (harmless, but not confirmed to have been the actual
  blocker), and raised `WEBSITES_CONTAINER_START_TIME_LIMIT` to 600 to give the
  migration headroom. Letting one restart run undisturbed, without further
  interruption, is what actually resolved it.

Fixed in the repo: `src/Vennu.Data/DatabaseMigrator.cs` now logs before
`EnsureDatabase`, before and after lock acquisition (with elapsed wait time), and on
lock release — commit `84e7699`. Previously this whole sequence was silent, which is
why the above took so long to diagnose: DbUp's own `.LogToConsole()` only starts
after the lock is already held.

Current state: all four dev apps confirmed live and serving real content —
`dev.api.vennusign.com` (`/health/version` 200), `dev.back-office.vennusign.com`,
`dev.display.vennusign.com`, `dev.po.vennusign.com`.

Not yet done: none of the Azure-side fixes above (connection string, `pm2 serve`
startup command, runtime pin, `WEBSITES_CONTAINER_START_TIME_LIMIT`) are captured
anywhere in the repo or as infrastructure-as-code — they exist only as live Azure
App Service configuration. `theme-studio`'s dev subdomain and App Service exist with
no application deployed to it yet. `stage`/`app` tiers have no custom domains, OIDC
identity, or deploy workflow at all yet — today's work covers `dev` only, and
deliberately does not yet implement the version-folder/branching model from the
concept doc above, since that remains unapproved design.

**Exact next action.** Decide whether to capture the App Service configuration
(connection string, startup commands, runtime, timeout) as infrastructure-as-code
or leave it as manual Azure state; then continue either with the GitHub issue
backlog or with formalizing the release-engineering concept into an approved
work package.

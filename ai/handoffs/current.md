# Vennusign Session Handoff

## Current State

- Track 0 and Track 1.01–1.05: complete, merged, exact-head validated and closed (see execution record below).
- **Track 1 owner acceptance ran on 2026-08-06.** Result: 17 Pass, 1 Needs Adjustment (2-1), 2 Fail (3-0, 3-1). Closure recorded: **Needs adjustment**, with the owner's note "huge leap forward, much more stable, but now we need to focus on UX usability on many of the screens." Track 1 remains open; the UX/screen changes are Track 1 work.
- Diagnosis of the acceptance findings (verified against the live DB and API, then by driving the real pages):
  - 3-0 and 3-1 failed because journey 2 leaves the reviewer signed in as Publisher and journey 3 assumed Owner; a Publisher sees a permission message where the Owner sees the allowance count. A workbook defect, not a product one.
  - Real product defects underneath: `screen.content.target` was declared and denied in session payloads but enforced nowhere (a Publisher could push via the API); per-screen action authority was invisible in the UI (Push looked available, failed on use); the `[0000]` isolation tag leaked into customer-facing display names; UI spec 3-0 asserted the banner's shape, not its agreement with the visible fleet.
- **Defect fixes are written but uncommitted and unverified.** Both builds pass (API 0 errors, back office `tsc -b` clean); the Playwright suite has NOT been run against them. Changed files:
  - `src/Vennu.Api/Controllers/BackOffice/BackOfficeScreensController.cs` — `RequireCapability` added: push and push-all (`screen.content.target`), reset (`screen.delivery.recover`), unpair (`screen.device.unpair`).
  - `src/back-office/src/ScreenManagement.tsx`, `VenueOperations.tsx` — Push/Reset/Unpair disable from the session decisions with a named restriction notice (`screen-action-restrictions`); `screen-fleet-count` testid added.
  - `scripts/start-ui-test-env.ps1`, `scripts/run-track1-qa.ps1` — baseline (tag 0000) display names no longer carry the isolation tag.
  - `tests/ui/specs/screen-capacity.spec.ts` — banner numbers must agree with the rendered fleet count.
  - `tests/ui/specs/role-boundaries.spec.ts` — new: publisher opens Screens but Push/Unpair are disabled up front; direct publisher push to the API must 403 with `screen.content.target`.
  - `docs/acceptance/track-1-owner-acceptance.html` — rewritten to v3: each case carries its own one-click sign-in link (fragment token), every step has a place-marker checkbox, 2-1 and 3-1 directions corrected to match the real UI, record schema v3 (v2 records still import).
- **Owner directive (2026-08-06): more screen/UX changes come first; run NO tests until those land.** The unverified fixes above wait behind that.
- Five agreed usability themes drive the UX work: role identity not first-class; generic permission refusals; three kinds of "no" indistinguishable on controls; product-vs-capability vocabulary split; scattered disclosure on Screens.
- `PRODUCT.md` now exists at repo root (owner-interviewed 2026-08-06). Binding facts: sky identity is the brand; primary user is the owner in interrupted bursts with occasional desk sessions; success is confidence at a glance, fast safe editing, honest limits, and learnability.
- Direction mocks, first set (`docs/design/track2-mocks/`, uncommitted): three structural directions as clickable pages — instrument bench (`home.html`, `screens.html`, `menu.html`, shared `bench.css`), venue floor map (`home-venue.html`), wayfinding bands (`home-wayfinding.html`). Saved as examples at the owner's request; no direction chosen; DESIGN.md deliberately not written.
- Git: branch `chore/release-track1-desktop-lock`; **PR #663 is still open** (lock release + workbook clickability, commits `c05d286`/`998bfd6`/`a93d883`). All of this session's work sits uncommitted on that branch. Do not push or merge from this session; the owner said another agent will handle publishing.
- Environment fully stopped (ports 7138/5174/5175/5176 free).
- Future-track implementation: blocked pending explicit owner closure of Track 1. RWP-13.06: held. Phase 14 and later: paused.

## UI Concept Exploration (Local, Uncommitted)

- The owner requested independent, non-implementation mockups for Home, Screens, and Menu Builder. These are exploratory images only; no direction has been approved for implementation.
- `docs/design/concepts/sky-instrument-bench-independent/` contains the first independent set: a calm, sky-colored operations workspace emphasizing venue health, publish state, and causal screen diagnostics.
- `docs/design/concepts/live-proofing-table/` contains the second set: a deliberately unconventional print-proofing metaphor where yellow means waiting, green means confirmed, coral means attention, and draft-versus-live impact is explicit.
- Both sets were created from the conversation's product requirements without using `docs/design/track2-mocks/`. The owner stated that `docs/design/track2-mocks/` is not available as source material for this exploration; future agents must not inspect, reuse, reproduce, or infer designs from it when extending the independent sets.
- The strongest product idea shared by both independent sets is causal support evidence: show what changed, when it was saved and published, which screens acknowledged it, when a player stopped checking in, the likely cause, and the safe next action. Preserve the distinction between customer-facing recovery guidance and owner-facing root-cause evidence.
- No code, product contract, roadmap, tracker entry, or implementation approval resulted from this exploration. The next UI step, only when the owner resumes it, is to compare the sets and select or combine a direction before creating implementation scope.
- These files and this handoff update are local and uncommitted. The owner explicitly said another agent will handle publishing; do not push from this session.

## Read First

1. `docs/acceptance/track-1-owner-acceptance.html` — the v3 workbook (open in a browser; the `.md` is its outdated predecessor).
2. `docs/design/track2-mocks/` and `docs/design/concepts/` — the two mock efforts the owner is comparing (note the independence rule above).
3. `PRODUCT.md` — confirmed product truth.
4. `ai/handoffs/2026-08-05-track-1-planning-handoff.md` — approved Track 1 decisions and process.

## Track 1 Execution Record

| RWP | Issue | PR | Result |
|---|---|---|---|
| Track 1.01 | #640 | #645 | merged as `a729f4dd75468c1f69570d53f44b81dcd86a4945`; Actions 31044305223 passed |
| Track 1.02 | #641 | #646 | merged as `06e12569b4f4ecb196a3dbf49a4a924798626376`; Actions 31044938623 passed |
| Track 1.03 | #642 | #647 | merged as `cd12f25a58bed509c2082f94b3bafe8974228cdc`; Actions 31045701930 passed |
| Track 1.04 | #643 | #648 | merged as `58dcf33a62d391275cfb985301aa5e9544c91262`; Actions 31047859910 passed |
| Track 1.05 | #644 | #650 | merged as `6915ef2b402ce146d8ff01bf7ad767e3cbb1295e`; Actions 31049451685 passed |
| Acceptance QA automation | — | #654 | merged as `b16d849`; 44 Playwright tests + 5 hosted-agent lanes |

Integration, Azure SQL, live-provider, hosted-infrastructure, credentialed, physical-device and cross-system tests remain intentionally skipped. Seven follow-up issues (#656–#662) are labelled `before-track-2`; none block Track 1 closure.

Test infrastructure notes that remain true: `tests/ui` (Playwright) asserts the mechanical cases and is gated in CI by `.github/workflows/ui-regression.yml`; `run-track1-qa.ps1` keeps the 5 subjective cases on hosted agents (~$1.70/run). `start-ui-test-env.ps1` is the local-only environment for Playwright and owner acceptance; `run-track1-qa.ps1` publishes tunnels whose CORS blocks localhost — the two cannot be shared and both bind ports 7138/5174/5175/5176. `POST /api/test/seed` (Development only) gives specs private rows; `-PruneSeed` removes them and the Playwright global setup runs it automatically.

## Governing Rules

- Tracks are open-ended. RWPs are grouped into scheduled chunks of up to five and execute sequentially inside each scheduled chunk.
- A successor never starts until its predecessor is merged, closed, validated on the default branch and handed off.
- Track 1 closure requires explicit owner approval; the 2026-08-06 record is "Needs adjustment", so acceptance feedback becomes Track 1 RWPs.
- Do not implement any future track until the owner explicitly closes Track 1.
- Light future-track planning cannot be marked complete until current and earlier-track feedback is incorporated or explicitly ruled out.

## Exact Next Action

1. Owner compares the mock sets (`docs/design/track2-mocks/` and `docs/design/concepts/`) and picks a direction, a blend, and which screens change.
2. Implement the chosen screen/UX changes as Track 1 work.
3. Only then run verification: `dotnet build`, back-office build, full Playwright suite (the new role-boundary and capacity specs assert behaviour only the unverified fixes provide), then re-run owner acceptance with the v3 workbook.
4. Decide commit/PR strategy with the owner: PR #663 merge; a separate PR for defect fixes + workbook v3 once verified; whether mocks/PRODUCT.md/concepts are committed at all.

## Boundaries

- **No test runs until the screen changes land (owner directive).**
- Do not push or merge from a working session; PR #663 stays open until the owner decides. Publishing is handled by the agent the owner designates.
- Do not treat any mock set as an approved design or write DESIGN.md before the owner picks a direction.
- Do not inspect, reuse, reproduce, or infer designs from `docs/design/track2-mocks/` when extending the independent concept sets.
- Do not start future-track implementation or resume RWP-13.06 unchanged.
- Do not claim full onboarding or unbuilt role, tier, allowance, add-on, rollout or locale management interfaces.
- Do not claim a hosted deployment, production accounts, physical-device proof or live-provider integration from the local acceptance fixture.
- `docs/Google/` contains an OAuth client secret the owner still needs to rotate; the Browser Use API key should be rotated as well.

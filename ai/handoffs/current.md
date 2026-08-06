# Vennusign Session Handoff

## Current State

- Track 0 industry and product architecture: complete and closed.
- Track 1.01 through Track 1.04: implemented, merged, exact-head validated and closed.
- Track 1.05: complete, merged through PR #650, exact-head validated by Actions run 31049451685 and verified on `master` at `6915ef2b402ce146d8ff01bf7ad767e3cbb1295e`.
- **Exact next action: the owner runs Track 1 acceptance against `master`.** PR #654 is merged as `b16d849`, the desktop session lock is released, and no agent holds Track 1. Sequential schedules may resume.
- Start the acceptance environment with `scripts/start-ui-test-env.ps1`, then open `docs/acceptance/track-1-owner-acceptance.html`. Its Start here section now offers a one-click sign-in per role; the token travels in the URL fragment and is removed before the page renders.
- Track 1 closure: blocked only on owner acceptance after Track 1.05 merges. The acceptance QA gate now passes all 19 cases, so owner judgment is the only remaining step.
- PR #654 carries the Track 1 acceptance QA automation and nine defect fixes. All 13 required checks are green on `d77dae5` and the branch is mergeable. It has had no independent review: the review recorded in-session was written by the agent that authored the diff and did not read the full change, which is tracked as issue #659.
- Seven follow-up issues (#656 to #662) are labelled `before-track-2`. None block Track 1 closure.
- Track 1 acceptance QA is automated in two halves. `tests/ui` (Playwright, 44 tests) asserts the 14 mechanical cases on every commit at no cost and is gated in CI by `.github/workflows/ui-regression.yml`. `scripts/run-track1-qa.ps1` retains only the 5 subjective cases (4-1, 5-0, 6-1, 6-2, 6-3) on hosted agents, at roughly $1.70 a run against $9.40 before the split.
- Use `scripts/start-ui-test-env.ps1` for Playwright work; it is local-only. `run-track1-qa.ps1` publishes cloudflared tunnels and sets CORS to those public origins, which blocks a browser running on localhost. The two environments cannot be shared, and both bind ports 7138/5174/5175/5176.
- Test data isolation: `POST /api/test/seed` (Development only) creates a private menu, section, item and optional screen so specs run in parallel without sharing rows. Seeded rows are pruned by `scripts/start-ui-test-env.ps1 -PruneSeed`, which the Playwright global setup runs automatically.
- Future-track implementation: blocked pending explicit owner approval of Track 1 closure.
- Light planning for any future track may remain provisional, but cannot be marked complete until Track 1 feedback and potential changes from earlier tracks are evaluated.
- RWP-13.06: held; do not resume unchanged.
- Phase 14 and later: paused.

## Read First

1. `docs/acceptance/track-1-owner-acceptance.md` — executable owner review, local fixtures, direct links, expected results and result record.
2. `docs/work-packages/RWP-01.05-track-validation-handoff.md` — Track 1.05 validation and implementation handoff.
3. `ai/handoffs/2026-08-05-track-1-planning-handoff.md` — complete approved Track 1 decisions and process.

## Track 1 Execution Record

| RWP | Issue | PR | Result |
|---|---|---|---|
| Track 1.01 | #640 | #645 | merged as `a729f4dd75468c1f69570d53f44b81dcd86a4945`; Actions 31044305223 passed |
| Track 1.02 | #641 | #646 | merged as `06e12569b4f4ecb196a3dbf49a4a924798626376`; Actions 31044938623 passed |
| Track 1.03 | #642 | #647 | merged as `cd12f25a58bed509c2082f94b3bafe8974228cdc`; Actions 31045701930 passed |
| Track 1.04 | #643 | #648 | merged as `58dcf33a62d391275cfb985301aa5e9544c91262`; Actions 31047859910 passed |
| Track 1.05 | #644 | #650 | merged as `6915ef2b402ce146d8ff01bf7ad767e3cbb1295e`; Actions 31049451685 passed |

Integration, Azure SQL, live-provider, hosted-infrastructure, credentialed, physical-device and cross-system tests remain intentionally skipped.

## Track 1.05 Corrected Gaps

- Screen create/pair UI now consumes the server `screen.device.pair` decision instead of billing-tier `maxScreens` as browser authority.
- Session decisions preserve message keys, structured parameters, correlation IDs, resolved locales and conditions through the Back Office API so allowance explanations include server-owned `used` and `limit` details.
- A focused Track 1 UI contract test protects canonical navigation, server decision projection and removal of client capacity authority.
- A deterministic local fixture and launcher provide Owner, Content Editor and Publisher review profiles plus Offline, allowance, unavailable, temporary-block and locale-fallback scenarios.
- Affected customer copy contains no new migration or legacy-preservation promise.

No additional Track 1 RWP is required by automated validation. Owner feedback may still create additional Track 1 RWPs before closure.

## Governing Rules

- Tracks are open-ended. RWPs are grouped into scheduled chunks of up to five and execute sequentially inside each scheduled chunk.
- A successor never starts until its predecessor is merged, closed, validated on the default branch and handed off.
- Fix every clear bounded gap inside the active RWP and revalidate it.
- Track 1 implementation execution stops after Track 1.05; owner acceptance is the next action.
- If acceptance needs larger changes, create additional Track 1 RWPs and place a later chunk onto the schedule.
- Do not implement any future track until the owner explicitly closes Track 1.
- Light future-track planning cannot be marked complete until current and earlier-track feedback is incorporated or explicitly ruled out.

## Exact Next Action

1. Owner performs `docs/acceptance/track-1-owner-acceptance.md` and records Pass / Fail / Needs Adjustment.
2. Close Track 1 only after explicit owner approval; otherwise prepare the next Track 1 scheduled chunk.
3. Keep future-track implementation blocked while light planning remains provisional under the feedback-evaluation rule.

## Boundaries

- Do not start future-track implementation.
- Do not resume RWP-13.06 unchanged.
- Do not claim full onboarding or unbuilt role, tier, allowance, add-on, rollout or locale management interfaces.
- Do not claim a hosted deployment, production accounts, physical-device proof or live-provider integration from the local acceptance fixture.

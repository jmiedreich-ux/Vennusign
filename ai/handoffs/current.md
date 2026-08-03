# Vennu Session Handoff

## Work Package
- ID: Issue-407
- Status: In Review
- Execution mode: Collaborative

## Git State
- Branch: `issue/407-screen-quota-gating`
- Issue: #407
- Pull request: #408
- CI state: pending on head `ca14e01`

## Completed This Session
- Confirmed the Screens GET path incorrectly calls the add-screen entitlement guard and hides assigned screens at quota.
- Confirmed manual create and Venue Admin pairing do not consistently enforce the same tier limit.
- Confirmed the Screens page renders the video-wall section without using the `video_wall` session capability.
- Consulted UX Form Validation and Button guidance and documented the UI/function gap analysis.
- Created and claimed Issue #407 with explicit writable and prohibited scope.
- Removed add permission from the screen read path so assigned/onboarding screens remain visible at quota.
- Added a typed tier-limit signal, enforced it before manual create and Venue Admin pairing, and mapped it to HTTP 409.
- Added billing-derived quota usage and disabled known-blocked add/pair actions while retaining server authority.
- Gated the video-wall builder on the effective `video_wall` session capability.
- Added focused service, controller, entitlement, and frontend regression coverage.
- Used `sqlcmd` to transactionally remove all local Development venue-owned data: 3 venues and 4 assigned screens plus dependent records.
- Reset onboarding venue/screen pointers while preserving the organization, customer identity, subscription tiers, and global definitions.
- Added automatic Display recovery for a stale persisted screen ID after local database reset.

## Validation
- Venue Admin focused tests passed 3/3; complete frontend tests passed 41/41; production build passed.
- Focused entitlement test passed 1/1; complete `Vennu.DataAccess.Tests` Unit category passed 151/151.
- Focused screen API/service tests passed 12/12; complete `Vennu.Api.Tests` Unit category passed 335/335.
- Affected Release builds passed during test execution; only pre-existing unrelated nullable warnings were emitted.
- UX accessibility check passed applicable semantics, keyboard, focus, label, contrast, and control-name checks.
- Integration tests remain excluded under the standing owner instruction.
- GitHub Actions and ChatGPT review are intentionally not started.
- Local database verification: venues 0, assigned screens 0, onboarding venue/screen links 0, organizations 1, customer users 1, subscription tiers 5.
- Focused Display pairing tests passed 4/4 and the Display production build passed.
- SQL verified the newly paired screen is Online and assigned to the correct 1-screen Restaurant Starter trial; the stale pre-fix API process was rebuilt and restarted from the active branch on ports 7138/5192.

## Remaining Work
- Owner repeats onboarding from venue setup, pairs the first display, and verifies the new screen appears in Screens.
- Owner verifies quota text and disabled add/pair controls at the plan limit.
- Owner verifies video-wall UI is absent without `video_wall` and appears when enabled.
- Apply any owner corrections on this same branch.
- Only after the owner explicitly says “do number 4”, commit, push, open the PR, run CI, obtain ChatGPT review, merge, and finalize records.

## Known Risks or Blockers
- Client quota state is anticipatory; server entitlement checks must remain authoritative.
- Concurrent add requests still rely on the existing repository/count model; no database-level quota reservation was introduced.
- Existing unrelated local changes in `src/Vennu.Api/Vennu.Api.csproj` and `docs/Google/` must remain untouched.

## Exact Next Action
- Reload `http://localhost:5175/pair`, resume onboarding at venue setup, pair the replacement display, then verify fleet visibility, quota blocking, and video-wall capability visibility.

## Do Not Redo or Reverse
- Do not alter public player registration or onboarding progression.
- Do not add lifecycle behavior from issue #345.
- Do not commit, push, open a PR, or run CI before owner manual testing approval.
- Do not include the unrelated `src/Vennu.Api/Vennu.Api.csproj` or `docs/Google/` changes in Issue #407.

# Vennu Session Handoff — Issue-407 Manual Test Gate

## Work Package
- ID: Issue-407
- Status: Ready for Owner Manual Testing
- Execution mode: Collaborative

## Git State
- Branch: `issue/407-screen-quota-gating`
- Issue: #407
- Pull request: none
- Commit: none; implementation is intentionally uncommitted
- CI and ChatGPT review: intentionally not started

## Implemented
- Screen reads no longer invoke add-screen permission, so existing and onboarding-paired screens remain visible at quota.
- Manual Venue Admin creation and pairing enforce the authoritative tier screen limit before mutation.
- Tier-limit failures use a typed exception and HTTP 409 response instead of an unhandled 500.
- Screens displays plan usage and disables add/pair actions when the known finite quota is reached; unlimited plans remain supported.
- Video-wall administration renders only when the effective `video_wall` capability is enabled.
- Focused entitlement, service, controller, and frontend regression tests were added.

## Validation
- Venue Admin focused tests: 3/3 passed.
- Venue Admin complete frontend tests: 41/41 passed.
- Venue Admin TypeScript/Vite production build: passed.
- Focused entitlement test: 1/1 passed.
- `Vennu.DataAccess.Tests` Unit category: 151/151 passed.
- Focused API/service/controller tests: 12/12 passed.
- `Vennu.Api.Tests` Unit category: 335/335 passed.
- Affected Release builds passed during test execution.
- UX accessibility check passed applicable semantics, keyboard, focus, label, contrast, and control-name checks.
- Integration, Azure SQL, external-service, TV, Display, and unrelated Admin validation were intentionally skipped.

## Manual Test
1. Restart the API and Venue Admin app from `issue/407-screen-quota-gating`.
2. Open Screens for the venue used during onboarding and confirm the paired screen is listed.
3. Confirm usage reads `1 of 1 screens used · Plan limit reached` for a one-screen plan.
4. Confirm Add screen and Pair screen are disabled at the limit.
5. Confirm the video-wall section is absent when `video_wall` is disabled and visible when enabled.
6. Report corrections for this same branch.

## Risks
- Client quota state is anticipatory; server checks remain authoritative.
- The existing count-before-create approach does not add database-level reservation for concurrent requests.
- Unrelated local `src/Vennu.Api/Vennu.Api.csproj` and `docs/Google/` changes remain in the workspace and must not enter Issue #407.

## Exact Next Action
Owner performs the manual Screens-page checks and reports results. Do not commit, push, open a PR, run CI, review, merge, or completion documentation unless the owner explicitly says “do number 4”.

## Do Not Redo or Reverse
- Do not move the add-screen guard back into screen reads.
- Do not expose video-wall UI without `video_wall`.
- Do not implement issue #345 lifecycle scope here.
- Do not include unrelated local workspace changes.

# Issue-407 — Screen Quota and Video-Wall Gating

## Status

Complete through PR #408, merged as `50eb67377d8af5afbc927034d9f93916896147ec`.

## Execution Mode

Collaborative

## Evidence

When a venue reaches its tier screen limit, `ScreenManagementService.GetAsync` calls `EnsureCanAddScreenAsync` and throws before returning existing screens. The onboarding-paired screen therefore does not appear in Venue Admin. `CreateAsync` and the Venue Admin pairing claim do not call the guard, so those mutation paths are not consistently bounded. The Screens page also renders the Pro video-wall section without checking the effective `video_wall` capability.

## Scope

- Keep assigned-screen reads available at and above quota.
- Enforce active entitlement and tier `MaxScreens` before Venue Admin manual create and pairing mutations.
- Map quota exhaustion to a controlled conflict response.
- Display finite screen usage/limit and disable add/pair actions when the known limit is reached; retain unlimited-tier support and server authority.
- Render video-wall administration only when `video_wall` is effectively enabled.
- Add focused API/service and Venue Admin frontend regression coverage.
- Recover Display pairing when a locally persisted screen ID no longer exists by registering and storing a replacement before requesting a fresh code.
- Reset all venue-owned data in the local Development `VennuSign` database for a clean onboarding/manual-test run, preserving global catalog/configuration and customer identity records.

## Boundaries

- Do not implement delete, archive, unpair, replace, transfer, or other lifecycle work from issue #345.
- Do not alter subscription schema, tier catalog, onboarding progression, or public player registration.
- Local data cleanup is authorized only for `(localdb)\MSSQLLocalDB` database `VennuSign`; no shared or production data cleanup is authorized.
- Preserve unrelated local changes to `src/Vennu.Api/Vennu.Api.csproj` and `docs/Google/`.

## UX Best-Practices Consultation

The Form Validation and Button patterns were consulted. Applicable guidance:

- preserve readable existing data when a mutation is unavailable;
- place concise quota guidance beside affected controls;
- use native disabled states when the blocking state is known while retaining server validation;
- announce errors and successful state changes semantically;
- communicate quota state with text rather than color alone;
- keep labels and keyboard operation intact.

## UI/Function Gap Analysis

- **Primary goal and hierarchy:** show the existing fleet first, then explain whether another screen can be created or paired.
- **Navigation:** remain on the existing Screens route; no additional route or redundant navigation is needed.
- **Actions/data:** reading, editing, and pushing existing screens remain available. Create and pair consume the shared tier screen quota.
- **Essential states:** loading, empty, loaded, finite quota available, quota reached, unlimited quota, server rejection, and create/pair success.
- **Validation and feedback:** native field constraints remain; known quota disables submit actions; API rejection remains authoritative and must produce actionable alert text.
- **Destructive actions:** none are added in this correction.
- **Accessibility/responsiveness:** explicit usage text, alert/status semantics, labeled controls, keyboard operation, and existing responsive forms are preserved.
- **API/authorization/entitlement:** billing presentation supplies anticipatory `MaxScreens`; API entitlement checks remain authoritative; session `video_wall` capability controls Pro UI visibility; video-wall service enforcement remains unchanged.

## Acceptance Criteria

- A venue at its limit can load all assigned screens, including the onboarding-paired screen.
- Venue Admin manual creation and pairing cannot exceed authoritative `MaxScreens`.
- Quota exhaustion returns HTTP 409 rather than an unhandled exception.
- The Screens page displays finite usage/limit, disables add and pair at the known limit, and supports unlimited tiers.
- The video-wall builder is absent without `video_wall` and available with it.
- A deleted persisted Display screen recovers from pairing-code HTTP 404 without requiring manual browser storage cleanup.
- Focused affected-area non-integration tests and builds pass; integration tests remain skipped.

## Validation Plan

- Focused `Vennu.DataAccess.Tests` entitlement tests.
- Focused `Vennu.Api.Tests` screen-management/controller tests.
- Venue Admin screen-management frontend tests and production build.
- Affected .NET Release builds only; no integration or unrelated TV/admin/display validation.
- Stop after local validation for owner manual testing. Do not commit, push, open a PR, or run CI until the owner explicitly says “do number 4”.

## Local Validation Evidence

- Focused Venue Admin screen-management frontend tests passed: 3/3.
- Complete Venue Admin frontend tests passed: 41/41.
- Venue Admin TypeScript and Vite production build passed.
- Focused entitlement test passed: 1/1.
- Complete `Vennu.DataAccess.Tests` Unit category passed: 151/151.
- Focused screen service/controller tests passed: 12/12.
- Complete `Vennu.Api.Tests` Unit category passed: 335/335.
- Release builds for the affected API, data, and test projects passed as part of test execution. Existing nullable warnings outside this issue remain unchanged.
- Integration, Azure SQL, external-service, TV, Display, and unrelated Admin validation were intentionally not run.
- UX accessibility check passed keyboard, semantics, focus, labels, contrast, and control-name checks. Its generic field-error recommendation is not applicable to the quota alert because quota is a form-level server rule and native field validation remains in place.
- GitHub Actions and ChatGPT review are intentionally not started before manual testing approval.
- GitHub Actions `phase02-tests` run #30784731394 passed API, data-access, Display, Venue Admin, docs, and stable `build-and-test` gates on implementation/reconciliation head `689d06e`; unrelated Admin, dev-control, and TV jobs were correctly skipped.
- Display pairing now recovers from a deleted persisted screen ID by re-registering and replacing browser storage; focused pairing tests passed 4/4 and the Display production build passed.
- Owner-approved local cleanup ran through `sqlcmd` in one transaction: 3 venues, 4 venue-assigned screens, and their venue-owned dependent records were deleted; onboarding `VenueId` and `FirstScreenId` links were reset.
- Post-cleanup verification returned zero venues, zero venue-assigned screens, and zero onboarding venue/screen links. One organization, one customer user, and five subscription tiers were preserved.
- Manual testing recreated screen `69d83590-10b9-49ad-a4fd-c13795d296f3` for venue `E50CF53B-DFF3-461F-9048-3125BA799970`; SQL verification showed Online status and the correct Restaurant Starter usage of 1/1. The observed GET exception came from a stale pre-fix API process, which was stopped and rebuilt/restarted from the active branch on ports 7138/5192.
- Final exact-head GitHub Actions `phase02-tests` run #30784846933 passed on `8104747c2a4263fffa6aa877e66c3eb41b72beb4`.
- ChatGPT recorded `CHATGPT APPROVED` on the reviewed head with no blocking findings or unresolved comments.
- PR #408 merged on 2026-08-03; issue #407 closed and the implementation branch was deleted.

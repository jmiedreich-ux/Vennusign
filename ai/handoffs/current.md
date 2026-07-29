# Vennu Session Handoff

## Work Package

- ID: WP-05.04
- Status: Complete pending GitHub Actions, review, and merge
- Branch: `wp/05.04-availability-menu-badges`
- Issue: #65

## Completed

- Added venue-scoped availability, optional quantity, tags, and bestseller updates.
- Normalized and bounded comma-separated menu badges.
- Reused `ItemAvailabilityChanged` when availability changes.
- Reused venue content updates for quantity and badge content.
- Added responsive controls, visible badges, and focused tests.

## Validation

- Local admin production build and 8 frontend tests passed.
- Tracker JSON and `git diff --check` passed.
- GitHub Actions is required on the exact reviewed head.
- Integration-type tests are intentionally skipped.

## Exact Next Action

Publish the branch and PR, then wait for exact-head GitHub Actions before review and merge.

## Do Not Redo or Reverse

- Do not recreate issue #65.
- Do not move tier prompts or quick update mode forward.
- Do not replace the established availability/content notification contracts.
- Do not run integration-type tests.

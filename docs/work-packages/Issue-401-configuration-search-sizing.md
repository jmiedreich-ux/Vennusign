# Issue-401 — Configuration Search and Field Sizing

## Status

In Review

## Execution Mode

Collaborative

## UX Guidance and Gap Analysis

Filter Panel and List View guidance applies.

- Goal: quickly find a setting without knowing its full hierarchical key.
- Search matches every entered term against the full key, any colon-delimited segment, description, application scope, and value type; therefore `CustomerAuthentication`, `EmailDelivery`, or both work without a separate taxonomy UI.
- Existing environment and application filters remain authoritative and search refines their loaded result set immediately.
- Feedback includes visible result count, distinct server-filter empty state, search no-results state, and a keyboard-accessible clear action.
- Search never includes configured values or secrets.
- Draft values and save/history actions remain attached to the same setting when filtering.
- Setting text, password, and numeric inputs use one consistent responsive width and height, collapsing to full width on smaller screens.

## Scope

- Client-side hierarchical configuration search.
- Result count and clearable no-results behavior.
- Consistent responsive setting input dimensions.
- Focused Admin tests and production build.

## Validation

- Admin tests passed 82/82.
- Admin production build passed.
- WCAG AA filter-panel review reported no issues for labeling, keyboard access, focus, status, and clear behavior.
- GitHub Actions pending.

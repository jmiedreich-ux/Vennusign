# RWP-00.10 — Iconography, Empty States, and Loading Skeletons

## Outcome

Back Office and Platform Operations share a small Sky UI foundation for monoline SVG icons, actionable empty states, and loading skeletons that hold their space while data loads.

## Accepted Scope

- Add decorative, current-color SVG icons with consistent stroke geometry and no font or emoji dependency.
- Add reusable empty-state and loading-skeleton components to both admin applications.
- Apply them to representative high-use account, screen, video-wall, venue-directory, dashboard, and feedback surfaces.
- Preserve every existing inline error, destructive review, authorization, route, API, and entitlement boundary.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | Loading and empty views must explain system state without competing with the primary workflow. | Skeletons stay visually quiet; empty states use one icon, title, explanation, and at most one bounded action. |
| Navigation and required actions | Empty results must offer the safest relevant recovery when one exists. | Screen and venue filters can be cleared, setup can be opened, venues can be created, and commercial events can be refreshed without route changes. |
| Essential states | Loading, empty, filtered-empty, error, permission, and populated states must remain distinct. | Skeletons are used only while loading; contextual empty states render only after authoritative empty results; existing errors remain inline. |
| Validation and destructive actions | Foundation components must not bypass validation or replace confirmation. | Actions call existing handlers only; destructive-review flows are unchanged. |
| Feedback | Loading status must be announced without repeated visual text. | Skeleton containers use `role=status`, `aria-busy`, and screen-reader-only labels; decorative rows are hidden. |
| Accessibility | Icons cannot become unlabeled controls, and motion must be optional. | SVGs inherit color and are hidden from assistive technology; action text remains explicit; reduced-motion removes skeleton animation. |
| Responsiveness | States must fit narrow mobile and desktop surfaces without layout jumps. | Viewport-safe grids, bounded widths, and stable rows preserve layout across supported breakpoints. |
| API, data, authorization, and entitlements | Presentation foundations must not alter authoritative behavior. | No endpoint, payload, persistence, tenancy, permission, billing, or entitlement change is included. |

## Validation

- Back Office Node tests: 81 passed.
- Back Office production build: passed.
- Platform Operations Node tests: 98 passed.
- Platform Operations production build: passed.
- Git diff whitespace validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted browser/mobile rendering, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

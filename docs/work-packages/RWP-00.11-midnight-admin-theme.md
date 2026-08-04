# RWP-00.11 — Midnight Admin Theme

## Outcome

Back Office and Platform Operations share an opt-in Midnight variant built on the established Sky UI token contract. One persistent, accessible control switches both applications between Sky and Midnight without changing product behavior.

## Accepted Scope

- Add a high-contrast dark palette through semantic Sky UI tokens.
- Persist one validated `sky` or `midnight` preference in local browser storage and apply it before React renders.
- Expose the switch on authenticated, loading, sign-in, and customer-onboarding entry surfaces in both admin applications.
- Move existing white component surfaces onto the shared raised-surface token so the dark variant remains coherent.
- Reuse the established monoline SVG icon contract and preserve all current status labels and non-color cues.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | Operators need a lower-luminance admin workspace without losing Sky UI hierarchy. | Midnight changes semantic background, raised surface, text, border, focus, and status tokens while preserving layout and hierarchy. |
| Navigation and required actions | Theme choice must be available before and after authentication without adding navigation complexity. | A fixed, viewport-safe toggle appears on every admin entry route and reports the destination theme in its label. |
| Essential states | Sky, Midnight, first visit, stored preference, blocked storage, loading, error, and authenticated states must remain deterministic. | Unknown or unavailable preferences fall back to Sky; a valid preference is applied before rendering and storage failure does not block the visual change. |
| Validation and destructive actions | Theme choice must not submit forms, affect data, or require confirmation. | The control is `type=button`; no form, validation, destructive-review, or authoritative product state changes. |
| Accessibility | Choice, focus, contrast, and status meaning cannot depend on color alone. | The control exposes `aria-pressed`, explicit labels, monoline sun/moon icons, a three-pixel focus ring, dark native control scheme, and retained status labels/icons. |
| Responsiveness | The control must remain usable without obscuring narrow admin surfaces. | Desktop shows icon and destination label; narrow viewports retain a 42-pixel icon control with the full accessible name. |
| API, data, authorization, and entitlements | Theme preference must remain local presentation state. | No endpoint, payload, persistence model, tenant, permission, billing, or entitlement change is included. |

## Validation

- Back Office Node tests: 84 passed.
- Back Office TypeScript and Vite production build: passed.
- Platform Operations Node tests: 99 passed.
- Platform Operations TypeScript and Vite production build: passed.
- Git diff whitespace, token-source, preference, and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

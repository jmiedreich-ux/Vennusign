# RWP-07.01 — Display Theme Font Bundling

## Outcome

Every non-system font exposed by a display theme is packaged with the player and loaded deterministically without a runtime Google Fonts dependency. Georgia and Arial remain intentional device-safe system fallbacks.

## Accepted Scope

- Package the existing Inter, decorative theme, and Noto multilingual families as player dependencies.
- Compile the exact weights already used by player themes into the production bundle.
- Ask the browser font set to load every required face during player startup.
- Remove remote Google Fonts connections from the player document.
- Preserve existing theme choices, content contracts, fallback families, and offline media caching.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | Theme typography must render consistently on browsers and TV shells, including after an offline restart. | All non-system theme families are compiled into versioned player assets; theme hierarchy and font selections are unchanged. |
| Navigation and required actions | Font delivery must require no operator navigation or setup. | Faces load automatically at player startup; no new action, route, or configuration is introduced. |
| Essential states | Startup, cached/offline, slow-load, and font-load failure states must remain usable. | Local faces are requested during startup, the service worker continues caching font/style responses, and existing system fallbacks remain available if a face cannot load. |
| Validation and destructive actions | Font delivery must not add input or destructive behavior. | No form, validation rule, confirmation flow, or destructive action changes. |
| Accessibility | Multilingual content, zoom, reduced motion, and readable fallbacks must remain supported. | Noto SC, KR, JP, and Arabic regular/bold faces are bundled; font loading adds no animation and retains the existing language-specific fallback stacks. |
| Responsiveness | Typography must remain stable across supported browser and TV viewport sizes. | Existing responsive type scales and layout bounds are unchanged; only font asset delivery changes. |
| API, data, authorization, and entitlements | Font delivery must remain a presentation concern. | No endpoint, payload, persistence, tenant, permission, billing, or entitlement change is included. |

## Validation

- Display Node tests: 136 passed.
- Display TypeScript and Vite production build: passed.
- Production output contains locally emitted font assets and no Google Fonts document dependency.
- Git diff whitespace and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual rendering, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

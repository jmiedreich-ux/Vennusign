# RWP-05.10 — Visual-First Screens Fleet

## Outcome

Back Office Screens now presents the venue fleet as responsive live-preview cards. Each card keeps Preview and Push visible, shows health and device context at a glance, and places identity, layout, registration, recovery, and destructive lifecycle controls behind labeled secondary disclosures.

## Accepted Scope

- Render active screens as lazy-loaded, non-interactive live thumbnails backed by the existing venue-scoped display URL.
- Show a bounded archived-state placeholder instead of loading an inactive player preview.
- Keep exact Preview and Push actions visible on every active screen card.
- Make a card-level Push an explicit target choice while preserving the existing protected screen delivery API and receipt states.
- Move identity/layout editing into a labeled disclosure and retain lifecycle actions in the existing overflow menu.
- Preserve setup, pairing, replacement, capacity, video-wall, search, filter, delivery, and destructive-review behavior.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals | Operators must recognize screens visually and perform daily delivery without scanning form rows. | Responsive cards lead with the current player view, name, location/layout, health, platform, and version. |
| Navigation and hierarchy | Daily actions must be obvious; setup and configuration must remain available without competing with them. | Preview and Push are visible card actions; Edit display and identity and More actions are labeled secondary disclosures. |
| Required actions | Preview, Push, identity/layout edit, registration, restore, reset, archive, and unpair must remain available. | All actions are preserved in their screen context; exact preview expands within its card and Push selects that same screen explicitly. |
| Essential states | Loading, empty, filtered-empty, online, offline, stale, archived, selected, busy, delivery, and preview states must be understandable. | Existing skeleton/empty/delivery states remain; thumbnails add labeled health badges, archived placeholders, and a non-color selected-card outline. |
| Validation and feedback | Push must identify one authorized target and edits must remain draft-first. | Card Push sets its screen as the selected target before using the established push route; identity and presentation drafts retain save/cancel and apply/discard controls. |
| Destructive actions | Routine delivery must not sit beside reset, archive, or unpair without separation and review. | Lifecycle actions remain under More actions and retain their existing caution or typed-confirmation dialogs. |
| Accessibility | Thumbnail motion cannot trap focus or replace textual status; actions need labels and visible focus. | Miniature iframes are non-interactive and hidden from assistive technology; textual health/device data, native disclosures, labeled action groups, and shared focus rings remain authoritative. |
| Responsiveness | Cards, actions, previews, and menus must work from desktop through narrow mobile widths. | The fleet uses auto-fitting cards, compact action rows, stacked headings, full-width exact previews, and the existing narrow overflow treatment. |
| API, data, authorization, and entitlements | No new endpoint, tenant scope, authority, or feature bypass may be introduced. | Cards reuse existing venue-scoped screen, display, delivery, entitlement, and destructive-review contracts; no API, schema, persistence, or authorization change is included. |

## Validation

- Back Office Node tests: 99 passed.
- Back Office production build: passed.
- Git whitespace validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, live-player delivery, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

# RWP-05.09 — Daypart Home and Navigation Shell

## Outcome

Back Office opens on a venue-time-aware operations home and organizes existing routes into collapsible task groups. Operators can see the current and next daypart, scan screen health, manage bounded 86 controls and today’s special, and reach the established emergency workflow without leaving the venue-scoped shell.

## Accepted Scope

- Add an implemented Home route backed by existing protected venue operations.
- Group navigation into Operate, Design & delivery, Connect, and Account sections.
- Present the API-resolved venue-local time, active/next meal period, and enabled daypart timeline.
- Show compact screen status summaries linked to the full Screens workflow.
- Provide bounded quick availability and daily-special actions using existing menu APIs.
- Provide an entitlement-aware entry to the existing emergency broadcast controls.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | Daily operators need current venue state and high-frequency actions before configuration tools. | Home leads with venue-local daypart context, then timeline, live screens, 86 board, and special; configuration remains in secondary grouped navigation. |
| Navigation | Routes must be scannable, collapsible, capability-aware, and preserve existing deep links. | Four native disclosure groups organize all implemented routes; locked navigation behavior remains intact and Home becomes the deterministic default. |
| Required actions | Operators need screen management, availability, specials, daypart setup, and emergency access. | Home links to full Screens/Menu/Schedules workflows, directly uses the established quick-update endpoints, and routes emergency work to the reviewed broadcast panel. |
| Essential states and feedback | Loading, empty, error, success, permission, locked, and populated states must be explicit. | Skeleton loading, retryable load error, actionable empty dayparts/screens/menu, inline mutation error, polite success status, and capability-disabled actions are all represented. |
| Validation | Invalid or failed operations cannot be presented as live. | Special text is length-bounded; existing API validation remains authoritative; failed mutations retain prior UI state and say no live setting changed. |
| Destructive actions | Emergency activation/cancellation cannot be duplicated or bypass existing review. Availability changes must remain reversible. | Home does not activate broadcasts; it opens the established confirmation-protected emergency panel. 86 controls expose current state and a Restore action. |
| Accessibility | Structure, keyboard navigation, state meaning, focus, and screen status cannot depend on color. | Native headings, lists, disclosure controls, explicit status text, `aria-pressed`, live regions, labels, and the shared focus contract are used throughout. |
| Responsiveness | Daily actions must remain usable on narrow operational devices. | Two-column content collapses to one column, headers/actions stack, special input becomes fluid, and existing minimum control sizes remain in force. |
| API, data, authorization, and entitlements | Browser time and route state cannot grant authority. | Meal periods use server-resolved venue time; all data comes from protected venue APIs; existing session scope and capability checks guard menu/scheduling actions; no endpoint, schema, or entitlement change is included. |

## Validation

- Back Office Node tests: 86 passed.
- Back Office TypeScript and Vite production build: passed.
- Git whitespace and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, live-player delivery, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

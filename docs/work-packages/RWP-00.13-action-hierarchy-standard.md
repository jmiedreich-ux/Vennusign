# RWP-00.13 — Action Hierarchy and Button Placement Standard

## Outcome

Back Office and Platform Operations now share an explicit action hierarchy: each migrated surface identifies one primary action, recovery stays secondary, destructive actions move behind a keyboard-accessible overflow, long-form apply controls remain reachable, and applied venue-theme changes expose a bounded server-backed Undo.

## Accepted Scope

- Add shared action-primary, secondary, danger, overflow, sticky-bar, and applied-undo presentation contracts to both admin applications.
- Move Back Office screen reset/archive/unpair and theme reset actions into overflow menus.
- Keep Back Office theme and screen-presentation apply controls reachable on long forms.
- Move Platform Operations configuration clear/history actions behind overflow while retaining one Save action.
- Keep reviewed configuration-import actions reachable on long previews.
- Add applied-state Undo for saved/reset/preset venue themes through existing protected theme APIs.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | Operators must identify the next action without competing primary buttons or adjacent destructive controls. | Migrated action surfaces use one explicit primary class; recovery is secondary; theme reset and screen/configuration destructive work moves under More actions. |
| Navigation | Overflow actions must remain discoverable and work without a custom pointer-only menu. | Native `details`/`summary` controls expose labeled More actions menus with normal keyboard and focus behavior. |
| Required actions | Save/apply, cancel/discard, history, reset, archive, unpair, clear, and Undo must remain available in their correct context. | Long-form bars preserve primary and secondary actions; overflow groups retain all lifecycle actions; applied-theme feedback presents Undo immediately after success. |
| Essential states and feedback | Busy, disabled, draft, applied, undo-success, undo-failure, and narrow-layout states must be visible. | Existing busy/draft/status states remain; utility styles standardize disabled controls; theme Undo reports restored or unchanged active state; responsive rules stack controls. |
| Validation | Undo cannot be optimistic and destructive overflow cannot bypass established review. | Undo calls the protected basic/advanced theme save API and clears only after server success; reset, archive, unpair, and clear keep their existing destructive-review flows. |
| Destructive actions | Dangerous actions must not compete with routine work and must retain consequence review. | Reset theme, archive/unpair screen, and clear configuration move behind labeled overflow; existing confirmation dialogs remain authoritative. |
| Accessibility | Hierarchy, danger, state, and discoverability cannot depend on color or hover. | Text labels, semantic native disclosures, minimum 42px controls, shared visible focus, explicit status regions, and Undo text accompany visual hierarchy. |
| Responsiveness | Sticky and overflow controls must remain usable without covering content on narrow screens. | Action rows stack to full width at 600px, sticky bars use a smaller bottom inset, and open menus become surface-width below their trigger. |
| API, data, authorization, and entitlements | Presentation changes cannot introduce new authority or client-only rollback. | All operations reuse existing venue/support authenticated APIs, authorization, entitlement, and review contracts; no API, schema, or persistence change is included. |

## Validation

- Back Office Node tests and production build: passed.
- Platform Operations Node tests and production build: passed.
- Git whitespace and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, live-player delivery, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

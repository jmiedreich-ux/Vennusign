# RWP-05.04 — Back Office Navigation and Menu Lifecycle

Status: Complete in the proposed merge state

Issue: #344

Mode: Sequential

Branch: `rwp/05.04-back-office-menu-lifecycle`

## Goal

Make Back Office menu work operational rather than placeholder-driven. Authorized operators can choose or create a menu, manage section and item lifecycles and ordering safely, recover failed drafts, perform bounded Quick Update changes, and enter entitled POS administration without leaving the canonical workspace.

## Bounded Scope

- Make Menu the default Back Office destination and remove placeholder Home and Settings routes.
- Expose POS administration only when the venue has the POS integration entitlement.
- Add menu selection and creation, section and item ordering, and recoverable archive/restore actions.
- Add explicit draft, saving, saved, failed, and retry states to editing workflows.
- Add Quick Update search, filters, bounded bulk changes, and undo/recovery.
- Add a menu-item lifecycle migration and venue-scoped API operations.

No new POS provider connection protocol, public customer workflow, player behavior, Phase 14 planning, or integration environment is included.

## UI and Function Gap Analysis

The analysis used W3C guidance for [error prevention](https://www.w3.org/WAI/WCAG22/Understanding/error-prevention-legal-financial-data.html), [status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages), [input assistance](https://www.w3.org/WAI/WCAG22/Understanding/input-assistance), and [confirming continued action](https://www.w3.org/WAI/WCAG22/Techniques/general/G168).

| Area | Gap | Implemented result |
| --- | --- | --- |
| Goals | The shell led with placeholder pages and did not make the active menu workflow obvious. | Menu is the default operational workspace; the entitled POS destination is visible from the same shell. |
| Navigation | Home and Settings suggested unavailable capabilities, while POS had no authorized entry point. | Placeholder routes are removed, Menu and Quick Update remain task focused, and POS is gated by `pos_integration`. |
| CRUD actions | Operators could edit some records but could not create menus, reorder items, or manage item lifecycle. | Menu creation, menu selection, section/item editing and ordering, and archive/restore actions are available within venue scope. |
| Essential states | Draft, saving, success, failure, retry, empty, filtered-empty, and provider states were incomplete. | Editors preserve drafts, announce save progress/result, expose retry controls, distinguish empty results, and show POS loading/configured/disconnected/error states. |
| Validation | Names, prices, URLs, selection limits, and exact ordering lacked consistent client or server boundaries. | Client limits mirror protected API rules; server services trim and validate names, enforce ownership, require exact item order sets, and reject invalid provider URLs. |
| Destructive actions | Section removal was immediate and item removal was unavailable. | Section and item archive actions require confirmation and remain recoverable through restore. |
| Accessibility and responsiveness | Async changes and selection counts were not consistently announced, and dense controls compressed on narrow screens. | Native labeled controls remain keyboard operable, `role=status` and `role=alert` announce outcomes, destructive actions use explicit names, and toolbars/editors stack responsively. |
| API, data, authorization, and entitlement | Recoverable lifecycle, ordering, and POS entry needed durable data and protected boundaries. | Migration 053 adds `MenuItems.IsActive`; venue-scoped endpoints enforce Back Office authorization; archived items stay editable but are excluded from display/overflow/Quick Update; POS navigation and APIs remain entitlement gated. |

## Implementation Result

- Menu selection and creation are first-class Back Office tasks.
- Section and item archives are deliberate and reversible; ordering persists deterministically.
- Item drafts survive failed requests and provide explicit retry actions.
- Quick Update searches and filters active items, limits bulk changes to 25, and can restore the previous availability state.
- Square and Clover status/import administration is accessible only through the entitled POS route; Toast setup guidance remains visible without inventing an unsupported browser connection flow.
- Runtime display and screen-capacity reads exclude archived items while Back Office management retains them for restore.

## Validation

- Back Office focused Node tests, including navigation, menu lifecycle, editor, and Quick Update behavior.
- Back Office TypeScript/Vite production build.
- Focused .NET unit tests for menu creation, lifecycle, exact ordering, protected controller delegation, and the embedded migration.
- Exact reviewed PR head validated through affected-area GitHub Actions; the migration/shared contract change requests the repository's widened non-integration validation.

Azure SQL, external POS services, credentialed provider flows, hosted infrastructure, containers, physical devices, signing/store access, cross-system behavior, and all other integration-type tests are intentionally skipped under the standing project policy.

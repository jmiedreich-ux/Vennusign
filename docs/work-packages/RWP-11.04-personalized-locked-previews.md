# RWP-11.04 — Personalized Locked Previews

## Outcome

Locked theme and layout opportunities now preview the active venue’s own authorized menu content instead of generic bars. The preview is read-only, explicitly non-authoritative, bounded to a small active-content sample, and loaded through the existing venue-scoped menu snapshot contract.

## Accepted Scope

- Personalize locked `all_layouts`, `white_label`, and `html_editor` previews with the active venue’s menu name, daily special, sections, item names, prices, and availability.
- Prefer the active menu, order active sections and items by their existing sort order, and bound the preview to two sections with three items each.
- Preserve generic placeholders for unrelated locked capabilities.
- Cover loading, error, no-content, available, and sold-out presentation states without introducing editing or navigation controls.
- Reuse the existing protected menu snapshot; add no endpoint, query parameter, schema, persistence, authorization, or entitlement behavior.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals | A locked layout/theme preview should demonstrate value with recognizable venue content, not an anonymous skeleton. | Eligible previews show the active venue’s authorized menu name, daily special, sections, items, prices, and availability. |
| Navigation and hierarchy | Personalization must support the existing locked-feature decision without acting like an unlocked editor. | Content remains inside the existing locked section beside the shared lock chip and upgrade sheet; it exposes no links, selection, editing, or hidden navigation. |
| Required actions | Operators still need only the established upgrade review and defer actions. | The preview is non-interactive; the shared entitlement chip opens the upgrade sheet and Not now retains the existing dismissal behavior. |
| Essential states | Loading, recoverable error, no active content, personalized content, daily special, and sold-out states must be understandable. | Each state has explicit text; unavailable items show Sold out plus a non-color strike treatment, and successful content is labeled as a preview using the current venue. |
| Validation and feedback | The preview must not imply that an upgrade, layout, or menu change has been applied. | The board says “Your content · preview only,” performs no writes, and leaves venue data and entitlements unchanged. |
| Destructive actions | No content mutation or destructive control belongs in a locked preview. | No create, update, delete, archive, reset, or unpair action is introduced. |
| Accessibility | Real content and state messages must be available to assistive technology, with meaning independent of color. | The preview has a venue-specific accessible label, semantic section names, status messages, textual Sold out state, readable contrast, and no focusable preview controls. |
| Responsiveness | Personalized content must remain legible beside the upgrade explanation and on narrow phones. | The established locked panel stacks at its existing breakpoint; preview sections collapse from two columns to one below 480 px. |
| API, data, authorization, and entitlements | Personalization may use only data already authorized for the active venue and must not weaken tenant isolation or unlock a feature. | The existing `loadMenuEditor` request and `X-Vennusign-Back-Office-Token` supply the current venue snapshot. No new endpoint, browser-selected venue authority, entitlement mutation, or cross-tenant lookup is added. |

## Validation

- Back Office Node tests: 105 passed.
- Back Office production build: passed.
- Git whitespace validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Azure SQL, live Stripe, hosted-browser end-to-end, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release. After that sequence, the 18-item approved remediation queue is complete; held RWP-13.06 remains excluded.

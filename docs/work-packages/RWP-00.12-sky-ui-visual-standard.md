# RWP-00.12 — Sky UI Visual Standard

## Outcome

Back Office and Platform Operations apply the owner-approved Sky visual language consistently across their existing surfaces. The shared token contract now drives page gradients, navigation, raised cards, primary actions, badges, focus, icon weight, and motion preferences without changing product behavior.

## Accepted Scope

- Apply the shared Sky and Midnight semantic tokens to both admin shells and representative high-use surfaces.
- Standardize primary actions on Sky Blue with Midnight Slate text, preserving destructive and caution treatments.
- Standardize badges and status pills while retaining explicit labels and reserved semantic colors.
- Apply one visible three-pixel focus treatment to links, buttons, form controls, and disclosure controls.
- Keep monoline SVG icons consistent and remove nonessential transitions for reduced-motion users.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | Both admin applications need the same recognizable surface, action, and content hierarchy. | Shared gradients, raised-card borders/shadows, sidebar colors, primary actions, and badges now use one semantic token source in Sky and Midnight. |
| Navigation and required actions | Existing routes and required controls must stay discoverable while primary actions become visually consistent. | Navigation structure is unchanged; established create, save, push, sign-in, and continue actions use the Sky primary pairing while secondary and destructive actions keep their roles. |
| Essential states and feedback | Loading, empty, success, warning, error, permission, locked, and populated states must remain distinguishable. | Existing state components remain intact; status pills retain labels and reserved live/warning/off colors, and empty/loading/toast foundations continue to provide explicit state text. |
| Validation | Form validation and authoritative server results must remain in context. | No validation path changed. Inputs use shared surfaces and borders, while current inline error, pending, retry, and success behavior remains authoritative. |
| Destructive actions | Visual rollout must not weaken confirmation or make destructive controls resemble primary actions. | Danger and caution selectors are excluded from the Sky action rule; the established review dialogs and typed confirmation contracts are unchanged. |
| Accessibility | Text contrast, focus, status meaning, icons, and motion cannot depend on color or animation alone. | Primary actions retain the tested 10.25:1 Slate-on-Sky pairing, all interactive elements receive the shared focus ring, icons remain decorative/current-color, status labels remain explicit, and reduced motion suppresses nonessential transitions. |
| Responsiveness | The visual standard must preserve narrow admin workflows and touch targets. | Existing responsive shells and grids remain unchanged; shared rules do not introduce fixed content widths, and current mobile action stacking and 42-pixel controls remain in force. |
| API, data, authorization, and entitlements | A visual standard must remain presentation-only. | No endpoint, payload, persistence, tenant, authorization, billing, or entitlement behavior changed. |

## Validation

- Back Office Node tests: 85 passed.
- Back Office TypeScript and Vite production build: passed.
- Platform Operations Node tests: 100 passed.
- Platform Operations TypeScript and Vite production build: passed.
- Git whitespace and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

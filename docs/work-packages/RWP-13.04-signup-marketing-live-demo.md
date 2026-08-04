# RWP-13.04 — Signup and Marketing Page with Live Demo

## Outcome

The public signup route now explains Vennusign through an interactive, self-contained product preview before account creation. It presents bounded product proof, public plan data, and the authoritative display-pairing story while preserving the existing secure customer authentication and onboarding journey.

## Accepted Scope

- Replace the short signup introduction with a responsive marketing experience.
- Add an interactive guest-screen preview for representative service periods.
- Present factual product proof points grounded in implemented Vennusign capabilities.
- Render public plan price, venue/screen limits, and trial availability from the existing anonymous plan contract.
- Explain the physical-display pairing sequence and the distinction between paired and Online.
- Keep the returning-customer sign-in route concise and preserve all existing authentication actions.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | A prospective customer must understand the product, see it in action, understand pricing, and reach secure signup without guessing. | The page leads with the first-screen outcome, provides Start setup and Try the live demo actions, then presents proof, preview, pairing, pricing, and the existing account card. |
| Navigation | Marketing exploration must not hide the account action or disrupt the separate returning-customer path. | In-page links move directly to the live demo or account card; `/signin` retains a compact welcome and the same authentication card. |
| Required actions | Visitors need to explore the preview and start account setup; returning users need provider, passkey, and email-link sign-in. | Three service-period controls update the preview, while the existing Google, Apple, passkey, and email-link actions remain unchanged. |
| Essential states and feedback | Interactive selection, plan-loaded, plan-empty, sign-in busy/error/success, and preview-only meaning must be explicit. | Pressed state and a polite live region expose demo selection; plan absence has a status message; existing authentication notice, alert, and busy states are preserved; preview copy states that no venue changes occur. |
| Validation | Marketing must not imply that a preview starts entitlement or that illustrative content is customer data. | Pricing comes only from the public plan API; the page explicitly says plan authority is confirmed during onboarding and that the preview cannot start a trial or subscription. |
| Destructive actions | No marketing or signup-preview action may mutate or destroy customer state. | The experience performs no request, storage, subscription, venue, or screen mutation. Existing sign-out behavior remains outside the unauthenticated surface. |
| Accessibility | Product exploration must be keyboard operable, semantically structured, and understandable without color or animation. | Native buttons, headings, lists, explicit pressed state, a polite live region, visible focus, textual screen states, and the shared reduced-motion contract are used. |
| Responsiveness | The experience must work from narrow phones through desktop marketing layouts. | Proof and pairing grids collapse to one column, headers stack, demo controls become full-width rows, and the preview uses fluid type and spacing. |
| API, data, authorization, and entitlements | Anonymous marketing may read public plans but cannot infer identity, tenant, entitlement, or live device state. | The new component consumes only `PublicOnboardingPlan[]`; it introduces no endpoint or persistence and leaves customer authentication, tenant ownership, provider redirects, webhook authority, and pairing APIs unchanged. |

## Validation

- Back Office Node tests: passed.
- Back Office TypeScript and Vite production build: passed.
- Git whitespace and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, live Stripe/provider flows, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, live-player delivery, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

# Phase 11 — Upgrade Prompts & Billing UX

## Approved Objective

Deliver a self-serve upgrade funnel that shows concrete feature benefits without interrupting current workflows, limits every surface to one dismissible prompt, and connects a two-step upgrade journey to Stripe Checkout and hosted billing management.

## Sequential Work Packages

1. **WP-11.01 — Upgrade Experience Contract and Tier Badges**
   Add the shared feature-benefit catalog, tier presentation metadata, prompt selection contract, session-scoped dismissal state, and reusable informational tier badge without placing prompts in product workflows.
2. **WP-11.02 — Locked Navigation and Section Previews**
   Keep eligible locked destinations visible with tier badges, add non-blocking locked-section previews with one concrete benefit and soft CTA, and route interactions into the shared upgrade context.
3. **WP-11.03 — Inline Feature Hints**
   Add one contextual, dismissible feature hint per relevant panel, enforce deterministic priority when several features are locked, and preserve every unlocked workflow.
4. **WP-11.04 — Sidebar Upgrade Nudge**
   Add the quiet sidebar nudge with deterministic seven-second rotation, progress dots, per-feature session dismissal, reduced-motion behavior, and no overlap with another active prompt.
5. **WP-11.05 — Upgrade Modal and Tier Value Summary**
   Add the dismissible bottom-sheet upgrade modal with feature-specific benefit, current tier, target-tier feature pills, monthly/annual presentation, one upgrade CTA, and a “Maybe later” exit.
6. **WP-11.06 — Stripe Checkout Session Foundation**
   Add the authenticated venue-scoped checkout-session contract, validate active public tier/price mappings, create Stripe Checkout through an injectable gateway, and return only an allowlisted hosted URL.
7. **WP-11.07 — Checkout Launch and Entitlement Return**
   Connect the upgrade CTA to Checkout, add bounded success/cancel return states, refresh the authoritative subscription and feature resolution after webhook reconciliation, and avoid optimistic entitlement changes.
8. **WP-11.08 — Billing Portal and Subscription Status**
   Add an authenticated Stripe Billing Portal session contract plus self-service billing entry, current plan/trial/past-due guidance, and downgrade-at-period-end messaging without custom payment-management UI.
9. **WP-11.09 — HaaS Contract Billing Guardrails**
   Model HaaS bundle term metadata, validate eligible 18/24/36-month checkout requests, expose remaining-term/buyout disclosure, and keep actual early-cancel collection dependent on confirmed Stripe billing events.
10. **WP-11.10 — Phase 11 Validation and Closure**
    Validate prompt limits, dismissal, benefit copy, locked-workflow continuity, checkout/portal authorization, webhook-driven entitlements, subscription states, HaaS guardrails, security boundaries, and reproducible non-integration builds; synchronize closure records.

## Governing Boundaries

- Complete packages sequentially and keep each independently testable and mergeable.
- Show concrete feature benefits rather than generic tier advertising.
- Never block an existing workflow or hide a known locked feature.
- Show at most one upgrade suggestion per screen; all suggestions must be dismissible for the current browser session.
- Keep the upgrade journey to prompt, modal, and one Checkout CTA.
- Use Stripe-hosted Checkout and Billing Portal; never collect or persist payment-card data.
- Treat webhook-processed subscription state as authoritative; never unlock features optimistically from a return URL.
- Reuse the Phase 03 billing catalog, subscription lifecycle, webhook idempotency, and feature-resolution services.
- Never expose Stripe secret keys, raw provider payloads, internal price identifiers, or unvalidated redirect destinations to the browser.
- Do not implement Phase 12 POS integration or later product behavior.
- Stripe network tests, Azure SQL tests, and all other integration-type or external-service tests remain skipped under the standing repository-owner instruction.

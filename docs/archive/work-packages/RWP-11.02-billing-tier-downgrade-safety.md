# RWP-11.02 — Billing Tier and Downgrade Safety

## Scope

Complete the approved billing-tier vertical slice in Back Office: show claim-bound usage and limits, compare tiers, disclose lost features, block unsafe targets at the API boundary, review financial impact before leaving Vennusign, recover from cancelled/stale/pending provider returns, and keep HaaS separate.

## UI and function gap analysis

| Area | Previous gap | Completed behavior |
| --- | --- | --- |
| Goal and navigation | Billing showed the current subscription but did not support an informed tier decision. | The existing Billing route now includes usage, plan comparison, and an explicit tier review; no duplicate navigation surface was added. |
| Read and selection actions | A customer could not compare current use to target limits or select a concrete target. | Active screens and organization venues are shown against current and target limits; each public tier has a deterministic current, upgrade, downgrade, or start state. |
| Validation | Browser choice did not communicate whether a downgrade was operationally possible. | The server blocks targets below active screen or venue usage in both presentation and action endpoints. The UI gives exact corrective actions. |
| Financial and destructive safety | A tier change could leave Vennusign without a deliberate impact review. | A modal dialog lists target limits, known lost features, provider authority, and interval where applicable. “Keep current plan” receives initial focus and closing commits nothing. |
| Essential states | Provider cancellation, delayed webhooks, and stale returns were not recoverable as one journey. | A bounded session record reports pending, applied, stale, refresh-error, and authoritative-success states. Access is never inferred from a return URL. |
| Permissions and entitlement support | Eligibility needed to remain bound to the signed-in venue and organization. | The API derives the venue from the Back Office claim, organization usage from persisted membership, and feature loss from tier mappings. Clients cannot select another tenant. |
| Accessibility | Status and modal behavior needed explicit keyboard and announcement behavior. | The native dialog traps focus, Escape is deliberately handled, the least-destructive action is focused, errors use alerts, and asynchronous confirmation uses a polite status region. |
| Responsiveness | Usage, tier cards, and financial review needed to remain operable on narrow screens. | Comparison cards wrap, usage/impact grids collapse to one column, and pending/review actions stack on narrow layouts. |
| HaaS separation | Software tiers and fixed hardware terms could be visually conflated. | HaaS remains in its own section, endpoint, service, and persistence model; tier decisions do not mutate the hardware contract. |

The interaction follows [WCAG 2.2 Success Criterion 3.3.4](https://www.w3.org/WAI/WCAG22/Understanding/error-prevention-legal-financial-data.html) by supporting review and cancellation before a financial action, and uses [status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html) for asynchronous confirmation without moving focus. Dialog behavior follows the [WAI-ARIA modal dialog pattern](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/). Stripe remains the hosted system for [customer portal subscription management](https://docs.stripe.com/customer-management) and [webhook-driven subscription state](https://docs.stripe.com/billing/subscriptions/webhooks).

## Acceptance evidence

- Server tier decisions distinguish start/current/upgrade/downgrade and block screen or venue overages.
- Checkout and targeted portal actions repeat server eligibility checks.
- The UI discloses limits, lost features, price/interval or provider review, and preserves the current plan by default.
- Pending provider decisions are stored without payment data and resolve only from authoritative refreshed billing state.
- Back Office focused Node tests and its production build pass locally.
- Affected-area GitHub Actions is authoritative for API and data tests at the exact PR head.
- Azure SQL, live Stripe/webhooks, external services, credentials, hosted infrastructure, containers, physical devices, signing/store access, cross-system, and all other integration-type tests are skipped.

## Completion record

Completed in the implementation pull request. Issue: #348. Phase 14 and later remain paused.

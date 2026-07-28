# Vennu Session Handoff

## Work Package

- ID: WP-03.07
- Status: Complete pending final CI, ChatGPT review, and merge
- Execution mode: Sequential automation

## Git State

- Branch: `wp/03.07-stripe-subscription-events`
- Issue: #24
- Pull request: Pending

## Delivered

- Transport-neutral Stripe subscription event contract and handler.
- Idempotent handling for subscription creation, updates, paid invoices, and deletion.
- Tier lookup by configured Stripe price ID.
- Venue/subscription identity safeguards and feature-cache invalidation.
- Focused unit coverage for supported transitions and duplicate rejection.

## Validation

- Local .NET validation unavailable because the runtime does not contain the .NET SDK.
- Integration-type tests intentionally skipped under standing owner instruction.
- GitHub Actions must pass restore, Release build, display production build, and unit tests against the final head.

## Exact Next Action

Publish WP-03.07, inspect required non-integration CI, record ChatGPT approval, and merge if all required checks pass. Then mark Phase 03 complete without starting Phase 04.

## Do Not Redo or Reverse

- Do not bypass the persistent Stripe event idempotency service.
- Do not allow a Stripe subscription ID to move silently between venues.
- Do not add the HTTP webhook endpoint, signature verification, or Stripe SDK in WP-03.07.
- Do not begin Phase 04 before WP-03.07 merges.

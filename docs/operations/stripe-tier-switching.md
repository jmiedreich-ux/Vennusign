# Stripe Tier Switching

## Configuration

Venue tier switching uses the same least-privilege API key configured under `Stripe:Revenue:ApiKey`.
The key must be an `sk_` or `rk_` key with permission to read subscriptions and update subscription items.

## Behavior

- Only existing Stripe-linked venue subscriptions can be switched.
- The target tier must be active and have a monthly price mapping.
- The current Stripe billing interval is preserved: monthly subscriptions use the target monthly price and annual subscriptions use the target annual price.
- Stripe applies prorations when the price changes.
- Local tier state is updated only after Stripe accepts the change.
- If local persistence or operational-event recording fails, the service restores the prior local tier and Stripe price using compensating updates.
- Successful changes invalidate cached entitlements and appear in the recent commercial events feed.

## Operational Notes

- A failure stating that manual reconciliation is required means local rollback did not persist; compare the Stripe subscription price with `VenueSubscriptions.TierId` before retrying.
- Raw Stripe payloads and API credentials are never returned by the admin API or stored in operational events.
- Integration-type tests are intentionally skipped under the standing repository-owner instruction.

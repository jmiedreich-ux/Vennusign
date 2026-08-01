# Phase 13 Tier Entitlements

Tier records are the policy authority for no-card trial duration, expiry behavior, maximum venues/screens, prices, and feature access. A zero-day tier offers no trial. Checkout initiation never changes entitlement; only persisted subscription state updated by the existing Stripe webhook pipeline grants paid access.

Venue operations treat `active` or an unexpired `trialing` subscription as entitled. Screen creation checks the current tier capacity before persistence. Feature resolution continues to require authoritative subscription status and the tier feature matrix. WP-13.05/13.06 will consume these policies during onboarding rather than duplicate them.

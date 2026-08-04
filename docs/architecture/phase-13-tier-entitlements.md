# Phase 13 Tier Entitlements

Tier records are the policy authority for no-card trial duration, expiry behavior, maximum venues/screens, prices, and feature access. A zero-day tier offers no trial. Checkout initiation never changes entitlement; only persisted subscription state updated by the Stripe webhook pipeline grants paid access.

`OrganizationSubscriptions` is the authoritative commercial owner for an organization: one Stripe customer, one Stripe subscription, one tier, and one status/period record. New Checkout sessions carry `organization_id` in both Checkout and subscription metadata and reuse the organization's known Stripe customer. Webhooks prefer organization metadata and persist the Stripe customer/subscription mapping; legacy `venue_id` metadata remains accepted and is promoted to organization ownership when the venue is attached to an organization.

Migration 044 conservatively backfills organizations that have exactly one legacy venue subscription. Organizations with multiple legacy venue subscriptions are deliberately left for explicit reconciliation instead of choosing an arbitrary commercial owner. `VenueSubscriptions` remains a compatibility projection for existing Back Office, CRM, and feature consumers and remains the fallback only for venues that are not attached to an organization. Organization writes synchronize projections for every attached venue; the organization row remains authoritative.

Organization and venue operations treat `active` or an unexpired `trialing` subscription as entitled. Venue creation/attachment checks `MaxVenues` before mutation, and screen creation checks the organization tier's `MaxScreens`. Feature resolution prefers organization status and tier, preventing a stale venue projection from granting access. WP-13.05/13.06 consume these policies during onboarding rather than duplicating them.


## Track 0 capability and packaging reset

Track 0 / issue #488 supersedes further onboarding implementation until the owner approves a complete capability matrix and commercial packaging model. The existing organization-level subscription authority remains the technical baseline, but the current feature catalog and gates must be audited before they are treated as approved product policy.

Every capability receives one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage/quantity limit, or internal rollout flag. These concerns must not be represented as interchangeable boolean feature keys.

Industry selection configures defaults, terminology, starter content, recommendations, and relevant capability presentation. It does not grant or deny entitlement. An organization owns a primary industry; venues may select their own business type so mixed organizations remain possible.

### Restaurant baseline

Manual menu availability, desktop/mobile Quick Update, menu management, screen pairing and targeting, preview and immediate publish, delivery/status confirmation, recovery, and basic restaurant presentation are core operations. Who may perform them is controlled by authorization. Availability itself is domain state.

Scheduling, dayparts, recurring specials, advanced presentation, coordinated screens, multi-venue sharing, brand controls, approvals, audit history, analytics, and organization dashboards are candidate tier-bundled capabilities pending cross-industry review.

POS synchronization, AI-assisted content/design, managed hardware coverage, premium analytics/data, custom integrations, and enterprise identity are candidate independent add-ons only where they provide separable value or cost. Venue, screen, user, storage, history, AI-usage, and connection quantities are limits rather than feature flags.

No capability classification or packaging candidate in this section authorizes implementation. Track 0 must complete the remaining native-industry profiles, live gate inventory, customer-journey validation, owner approval, and bounded implementation planning first.

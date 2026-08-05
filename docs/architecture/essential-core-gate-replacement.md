# Essential core gate replacement

Track 1.04 establishes one authority path for Back Office actions. Authentication identifies the actor, organization, venue and protected system role. It does not mint feature or capability claims. The server resolves every registered capability through the decision contract and returns structured results to the session; the browser projects those results but cannot grant access.

## Runtime path

1. `CustomerBackOfficeAuthenticationHandler` validates the customer session and membership, then records identity, scope and a protected system-role key.
2. `BackOfficeCapabilityDecisionInputProvider` resolves rollout, entitlement, permission, add-on, allowance, resource and request dimensions from typed policy data and request context.
3. `CapabilityDecisionService` produces a stable result and localized reason for every canonical capability.
4. `BackOfficeSessionController` returns the complete structured decision set. Navigation uses canonical `domain.resource.action` IDs and never treats a route name as a capability.
5. `RequireCapabilityAttribute` reevaluates state immediately before an affected controller action. A blocked action returns a structured 403 with decision, reason, category, message, resolution, retry interval and correlation ID.

Client navigation is an explanation and discoverability layer, not an enforcement layer. Calling an endpoint directly cannot bypass the fresh server decision.

## Typed persistence

DbUp script `057_create_typed_capability_access.sql` creates:

- the canonical capability-definition catalog;
- rollout controls with global, organization and venue scope;
- organization capability entitlements;
- add-on attachments;
- organization or venue allowances and usage;
- layout templates with optional canonical capability requirements.

Universal-core and governance definitions do not require a commercial entitlement row. Advanced-native capabilities require explicit entitlement and add-on attachment. Deferred capabilities are unavailable unless the rollout model is deliberately changed. Missing or malformed state fails closed.

The pre-existing generic feature catalog may still describe billing offers for the unchanged pricing/provider surface, but it is not consulted by authentication, session route availability, operational mutations, player delivery or recovery. Generic feature keys, tier-feature joins and venue overrides therefore cannot grant an action. Their platform presentation is commercial metadata only and must not be used as an authorization input.

## Essential loop and allowances

The universal core includes content creation and editing, preview, screen pairing, publishing, confirmation, replacement, unpublishing and delivery recovery. Quantities are represented by typed allowances. Additive actions such as `screen.device.pair` fail with `allowance.reached` when exhausted.

Correction and recovery remain usable at the limit. The decision provider explicitly exempts update/archive, replace/unpublish, retry/restore, unpair and delivery-recover actions from an exhausted allowance while still requiring identity, permission, rollout, entitlement, add-on, state and valid request dimensions.

## UI truth table

| Server decision | Navigation/action behavior | Recovery text |
| --- | --- | --- |
| `allowed` | visible and enabled | none |
| `allowed-with-conditions` | visible and enabled | conditions remain server supplied |
| `denied` | visible but disabled where discoverability matters | permission or allowance resolution |
| `unavailable` | hidden when irrelevant; otherwise disabled with reason | product access or required service |
| `temporarily-blocked` | disabled with temporary state | retry guidance and optional interval |

The Back Office preserves the existing Sky-blue Operate visual system. Locked surfaces use semantic text, focus-safe controls, `aria-disabled`, localized server messages and a sign-in recovery action when identity context is missing. Loading, empty, network-error and retry states remain explicit in the affected operational surfaces.

## Validation boundary

Focused browser tests cover canonical navigation decisions, locked states, operational composition and customer-session claims. Focused server tests cover typed allowance denial, correction/recovery exemptions, temporary rollout retry metadata, membership-to-role mapping, session projections and mutation denial. Azure SQL, external-provider and integration tests are intentionally excluded from this RWP.

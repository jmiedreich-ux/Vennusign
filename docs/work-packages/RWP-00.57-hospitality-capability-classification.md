# RWP-00.57 — Hospitality Capability Classification

## Status

Complete in this proposed merge state.

## Issue

- #532

## Objective

Consolidate every approved Hospitality concern into exactly one primary Track 0 classification, resolve duplicate and ambiguous tier/add-on boundaries, preserve the required manual core, and prepare the authoritative handoff to subscription-tier mapping. Documentation only.

## Dependency verified

- RWP-00.56 is merged, verified, closed, and released.
- RWP-00.55 remains the required-core authority.
- The merged Hospitality industry, subtype, terminology, operating, required, and optional records are authoritative.
- RWP-00.58 — Hospitality Subscription Tier Mapping (#533) is next.

## Delivered

- Added `track0/industries/hospitality-capability-classification.md`.
- Assigned one primary classification to every Hospitality concern: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.
- Consolidated required manual property information, hours, states, notices, amenities, services, outlets, events, directories, wayfinding, languages, targeting, publishing, delivery confidence, current exception visibility, and recovery as core.
- Classified advanced Vennusign workflow, governance, coordination, wayfinding, localization, analytics, and enterprise administration as tier candidates.
- Classified external synchronization, maps/positioning, automated translation, AI, identity connections, managed hardware, connectivity, monitoring, and services as independent add-ons.
- Resolved mixed boundaries for personalization, advanced wayfinding, localization/AI, analytics, enterprise identity, and managed service.
- Kept permissions, represented state, limits, privacy, source authority, commercial access, and rollout separate.
- Defined the rules RWP-00.58 must follow when mapping tiers.
- Applied project-local Impeccable `clarify` and `harden` guidance.

## Classification result

No required RWP-00.55 capability is premium. Manual daily operation and recovery remain core.

Vennusign-authored advanced capabilities may be tier entitlements. Separately purchased external connections, consumption-backed services, managed hardware, connectivity, monitoring, and related services remain independent add-ons. Counts and consumption are limits. Authority is permission. Represented business and system values are state. Experiments and safe-delivery controls are rollout flags.

## Ambiguity resolutions

- **Personalization:** Vennusign audience workflow is tiered; external guest/profile data connection is an add-on.
- **Advanced wayfinding:** Vennusign map/directory workflow is tiered; external maps, positioning, sensors, and live routing are add-ons.
- **Localization and AI:** review and terminology workflow is tiered; automated translation and AI consumption are add-ons.
- **Analytics:** advanced Vennusign analytics is tiered; external operational data feeds are add-ons.
- **Enterprise identity:** enterprise administration is tiered; SSO/SCIM/directory connections are add-ons.
- **Managed service:** hardware, connectivity, monitoring, installation, replacement, and service levels are add-ons.

## Impeccable result

Future classification and packaging surfaces must explain the customer outcome before internal classification terms and distinguish unavailable entitlement, missing permission, disconnected source, exceeded limit, unsupported object, and rollout state.

They must keep the included manual fallback visible, use progressive disclosure for commercial and technical detail, and preserve keyboard access, assistive technology, non-color status, 200% zoom, long names, localization expansion, right-to-left readiness, phone through large desktop, and the approved Sky Blue administrative direction.

No UI or implementation contract was created.

## Validation

Documentation-only review confirmed:

- every concern has exactly one primary classification;
- duplicate concepts were consolidated;
- ambiguous mixed concerns have explicit tier/add-on separation;
- permissions do not determine commercial access;
- limits do not grant capabilities;
- state values are not feature flags;
- required manual operation remains core;
- RWP-00.58 has a clear authoritative input.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product implementation, including UI, API, schema, migrations, billing, entitlements, permissions, privacy systems, localization, analytics, PMS, event, room-booking, transport, POS, guest-service, access, gaming, mapping, emergency, weather, translation, AI, identity, hardware, connectivity, monitoring, and managed services.

## Exact next action

After this RWP is merged, verified on `master`, issue #532 is closed, and the claim is released, execute **RWP-00.58 — Hospitality Subscription Tier Mapping** (#533).

RWP-00.58 must map the approved tier-entitlement candidates into coherent subscription tiers while keeping required core capabilities in the base operating tier, external connections as independent add-ons, and limits separate. It remains documentation-only and hands off to RWP-00.59.
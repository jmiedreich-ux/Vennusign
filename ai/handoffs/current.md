# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning; implementation paused
- Branch/PR: documentation record only; no product implementation claim
- Active implementation WP/RWP: none
- Phase 14 and later: paused

## Why Track 0 Exists

The completed WP/RWP inventory does not yet produce consistent end-to-end customer journeys. Vennusign has also mixed domain state, permissions, commercial entitlements, quantity limits, add-ons, and internal rollout flags under the broad idea of “features.” Track 0 resolves that product architecture before more onboarding or feature implementation.

WP/RWP records remain bounded implementation units inside completion tracks; their completion alone does not prove that an entire product area is complete.

## Approved Classification Model

Every capability must have one primary classification:

1. Core capability
2. Permission
3. Product/domain state
4. Tier entitlement
5. Independent add-on
6. Usage or quantity limit
7. Internal rollout flag

Example: manually marking a menu item unavailable is a core operational capability acting on product state. It is not a commercial feature flag. The permission system controls who may perform the action. POS-driven automatic availability may be an add-on or tier-bundled integration.

## Supported-Industry Direction

Native profiles are developed first for Restaurant; Bar, brewery, and nightlife; Café, bakery, and dessert; Food truck and concession; Hospitality; and Entertainment and attractions. Other approved industries initially remain compatible through the shared signage foundation.

Industry affects defaults, terminology, starter content, recommendations, and relevant capability presentation. Industry is not itself a tier or entitlement. Organizations have a primary industry, while venues may have their own business type to support mixed organizations.

## Completed Industry Profile

Restaurant is the first approved native profile.

Core restaurant operations include menu/category/item management; prices, descriptions, images, and dietary labels; manual availability; mobile and desktop Quick Update; basic specials; pairing and screen management; explicit screen targeting; preview and immediate publish; delivery confirmation; online/outdated detection; recovery/restore; basic restaurant layouts/themes; business hours; and understandable error recovery.

Candidate tier capabilities include scheduling, dayparts, recurring specials, advanced presentation, campaigns, coordinated screens/video walls, multi-venue sharing, brand controls, reusable libraries, approvals, advanced permissions, history, analytics, and organization dashboards.

Candidate add-ons include POS synchronization, AI-assisted content/design, managed hardware coverage, premium analytics/data, custom integrations, and enterprise identity where independently justified.

Venue/screen/user counts, storage, retained history, AI usage, and integration connections are limits rather than feature flags.

## Exact Next Action

Profile **Bar, brewery, and nightlife** using Restaurant as the baseline. Document only its distinct business types, goals, daily operations, content, screen purposes, roles, integrations, and capability-classification differences.

After all native profiles are approved, inventory and classify every existing feature key, entitlement check, permission, override, limit, and locked UI surface. Then propose tiers and add-ons for owner approval.

## Boundaries

- Do not start product implementation from issue #488.
- Do not resume RWP-13.06 implementation until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Preserve the Sky Blue visual direction.
- Integration and external-system tests remain skipped under the standing owner instruction.

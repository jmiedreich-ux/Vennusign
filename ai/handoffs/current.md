# Vennusign Session Handoff

## Current State

- Track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Native-industry gate: complete
- RWP-00.75: merged and verified
- RWP-00.76: complete in proposed merge state
- Product implementation: paused
- RWP-13.06 and Phase 14+: paused

## RWP-00.76 Result

The factual current-product inventory is at `track0/consolidation/EXISTING_PRODUCT_INVENTORY.md`.

It records the current session capability keys, effective feature keys, route and locked-surface consumers, browser tier slugs, billing/provider authority, tier selection and downgrade checks, screen/venue limits, feature limit values, HaaS contract terms, claims/context authority, support overrides, product-state controls, external service boundaries, and rollout/configuration evidence.

Key factual ambiguities carried into reconciliation include:

- `pos_integration` is both a route capability and effective feature key;
- Menu access checks `menus` while its prompt uses `quick_update`;
- `all_layouts` is reused across Tap list, Screens, and Themes;
- `happy_hour` and `video_wall` each have commercial-feature and product-state representations;
- `multi_location` coexists with permission-bound authorized venue contexts;
- screen/venue plan limits, feature `limitValue`, layout capacity, and HaaS term limits govern different domains;
- the browser receives flat session capabilities rather than a normalized per-action permission model;
- locked UI does not receive one structured reason model for entitlement, permission, limit, state, source, support, or rollout conditions.

These are observations, not remediation decisions.

## Exact Next Action

Execute **RWP-00.77 — Capability Reconciliation & Gap Analysis (#553)** after RWP-00.76 is merged, issue #552 is closed, `master` is verified, and the claim is released.

Map `EXISTING_PRODUCT_INVENTORY.md` to `CROSS_INDUSTRY_MODEL.md`. Identify missing, duplicate, obsolete, or misclassified mechanisms, including permissions represented as entitlements, product state represented as feature flags, and inconsistent organization/venue inheritance. Record recommendations only.

## Boundaries

Do not change product behavior, keys, gates, permissions, limits, billing, rollout, API, schema, migrations, or integrations. Do not resume RWP-13.06 or Phase 14+. Azure SQL, live Stripe, and all integration/external-system tests remain skipped.

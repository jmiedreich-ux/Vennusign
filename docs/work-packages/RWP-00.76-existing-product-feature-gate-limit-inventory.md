# RWP-00.76 — Existing Product Feature, Gate & Limit Inventory

## Issue

#552

## Mode and status

- Execution mode: Sequential
- Scope: documentation and factual product analysis only
- Dependency: RWP-00.75 merged, closed, verified, and released
- Result: complete in proposed merge state

## Objective

Inventory the current product’s feature keys, session capability checks, permissions/context controls, support overrides, usage and quantity limits, locked UI surfaces, rollout/configuration controls, authority, scope, and known consumers without changing live behavior.

## Delivered

`track0/consolidation/EXISTING_PRODUCT_INVENTORY.md` records:

- Back Office route capability keys and upgrade presentation keys;
- the current twelve-key effective feature catalog and four browser tier slugs;
- server-authoritative tier, subscription, Checkout, Billing Portal, and webhook behavior;
- current `MaxScreens` and `MaxVenues` limits and factual usage fields;
- feature `limitValue`, screen layout capacity, and HaaS contract terms as distinct limit domains;
- session, venue-context, claim, billing, support, source, and destructive-review authority boundaries;
- the locked navigation, preview, hint, nudge, upgrade-sheet, tier-decision, and billing-status surface family;
- product-state fields that affect actions but are not commercial entitlements;
- POS provider, Stripe, HaaS, player/delivery, and source synchronization boundaries;
- current rollout/configuration evidence and explicit absence of a normalized customer-visible rollout catalog;
- observed duplicate identifiers and ambiguous mechanisms, without making reconciliation decisions reserved for RWP-00.77.

## Validation

- Reviewed current `navigation.mjs`, `upgradeExperience.mjs`, `App.tsx`, `api.ts`, billing presentation contracts, tier decision evaluator, merged Phase 11/RWP-11 implementation evidence, screen/player remediation, and POS/HaaS boundaries.
- Distinguished browser presentation from server authority.
- Recorded exact known feature and capability keys rather than inferring future keys.
- Marked unexposed or non-normalized controls as unknown rather than claiming absence.
- Project-local Impeccable guidance applied to inventory of locked, permission, limit, unavailable, stale, disconnected, and recovery presentation.
- No live feature, permission, override, limit, rollout, billing, or product behavior changed.
- Azure SQL, live Stripe, and all integration/external-system tests remain skipped.
- GitHub Actions is authoritative for lightweight documentation validation.

## Handoff

After merge, issue closure, default-branch verification, and release, RWP-00.77 — Capability Reconciliation & Gap Analysis (#553) is the exact next item.

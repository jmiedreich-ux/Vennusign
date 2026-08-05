# RWP-00.31 — Café, Bakery & Dessert Required Capabilities

## Status

- **Track:** Track 0 — Capability, Packaging, and Entitlement Architecture
- **Issue:** #506
- **Execution mode:** Sequential within the Café, Bakery & Dessert stream
- **Scope:** Documentation and planning only
- **Dependency:** RWP-00.30 merged, verified, closed, and released
- **Branch:** `rwp/00.31-cafe-bakery-dessert-required-capabilities`
- **Result:** Complete in this proposed merge state

## Objective

Define the smallest viable inherited and Café-specific capability set required for ordinary daily operation without a premium tier or paid integration.

## Accepted scope completed

- Defined required menu and product management.
- Defined rapid manual availability, sell-out, batch, return, freshness, period, preorder, pickup, closure, and reopening communication.
- Defined required hours, business-day, service-period, service-context, and operating information.
- Defined public preorder, custom-order, and pickup presentation while excluding private order and fulfillment state.
- Defined required screen pairing, purpose, targeting, preview, and safe scope.
- Defined per-target publish confirmation and delivery confidence.
- Defined correction, conflict, retry, undo, restoration, and basic included-history requirements.
- Defined object-scoped permissions and authority boundaries.
- Defined first-use, empty, validation, permission, source, offline, delivery, concurrent-edit, success, failure, accessibility, responsive, environmental, and recovery states.
- Classified each concern as core, product state, permission, tier candidate, add-on candidate, limit, or rollout flag.
- Kept essential manual operation core across every approved subtype.

## Classification result

- **Core:** ordinary content/product management, manual operational updates, screen pairing and explicit targeting, preview, immediate publish, per-target confirmation, correction, delivery awareness, and recovery.
- **Product/domain state:** represented venue, business-day, period, service-context, item, option, batch, freshness, availability, preorder, pickup, screen-purpose, source, target, publication, delivery, and restoration values.
- **Permission:** edit, change-state, target, publish, override, restore, view restricted detail, and local administration authority.
- **Tier candidates:** recurring schedules, reusable rotations, campaigns, approvals, orchestration, advanced presentation, extended history, analytics, loyalty workflow, and optimization.
- **Add-on candidates:** POS, inventory, production, ordering, payment, fulfillment, loyalty, weather, event, translation, AI, hardware, monitoring, and other external services.
- **Limits:** quantities, consumption, and retention windows.
- **Rollout:** temporary internal release controls only.

## Impeccable result

The required future Operate experience prioritizes the venue and current service context, urgent guest-facing truth, affected products or batches, selected screens, publish result, and recovery. Daily manual tasks remain visible and usable when optional workflow or integrations are absent.

Planning includes first-use, no-content, no-screen, empty-period, validation, permission, stale-source, conflict, offline, outdated, partial-delivery, failure, concurrent-edit, success, undo, correction, and restoration states, with phone/desktop, localization, 200% zoom, keyboard, assistive-technology, non-color, reduced-motion, glare, distance, low-light, and crowding requirements.

## Validation

- Reviewed against issue #506, Restaurant inheritance, the merged RWP-00.27–00.30 Café documents, Track 0 classification policy, and project-local Impeccable guidance.
- Every issue-listed required capability is covered.
- Ordinary operation remains possible without paid integrations or premium workflow.
- No product, UI, API, schema, migration, billing, entitlement, feature-gate, ordering, payment, production, inventory, fulfillment, analytics, AI, hardware, or integration implementation is included.
- Documentation-only Actions are authoritative on the exact reviewed PR head.
- Integration and external-system tests remain skipped under the standing owner instruction.

## Shared-record pending queue

After merge, reconcile onto current `master`:

- mark Café complete through RWP-00.31;
- set RWP-00.32 as the exact next Café item;
- record the required-capability result in project status and current handoff;
- release the RWP-00.31 claim and claim RWP-00.32 only after verification;
- preserve all concurrent industry updates.

## Handoff

**RWP-00.32 — Café, Bakery & Dessert Optional Capabilities** (#507) is next. It must define optional native workflow, external service, managed service, tier, add-on, and limit candidates without weakening the required core.

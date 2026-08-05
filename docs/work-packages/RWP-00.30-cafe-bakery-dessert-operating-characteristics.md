# RWP-00.30 — Café, Bakery & Dessert Operating Characteristics

## Status

- **Track:** Track 0 — Capability, Packaging, and Entitlement Architecture
- **Issue:** #505
- **Execution mode:** Sequential within the Café, Bakery & Dessert stream
- **Scope:** Documentation and product planning only
- **Dependency:** RWP-00.29 merged and verified
- **Branch:** `rwp/00.30-cafe-operating-characteristics`
- **Result:** Complete in this proposed merge state

## Objective

Define the Café, Bakery & Dessert operating characteristics that differ meaningfully from Restaurant, including early hours, business-day boundaries, service periods, batch production, freshness guidance, rotating products, sell-outs, preorders, pickup, seasonal demand, counter and optional table service, subtype rhythms, screen purposes, source authority, recovery, and capability-classification boundaries.

## Accepted scope completed

- Defined independent venue, service-period, content, item, batch, preorder-window, pickup-context, source, screen, publication, and delivery state.
- Defined early, pre-dawn, cross-midnight, timezone, and business-day treatment without inventing a scheduler.
- Defined overlapping service periods and mixed service contexts.
- Defined batch and freshness treatment without unsupported quality, safety, quantity, or readiness claims.
- Defined rotating, daily, limited, and seasonal products and separated manual core operation from advanced workflow and external services.
- Preserved canonical availability, sell-out, return, preorder, and pickup distinctions.
- Defined public preorder and pickup information without implementing ordering or private fulfillment state.
- Defined counter, optional table, and mixed-service operation.
- Defined screen-purpose, source-authority, stale-source, conflict, multi-venue, bulk-action, delivery-confidence, and recovery boundaries.
- Mapped all approved subtypes to operating rhythms and daily information priorities.
- Classified core, state, permission, tier, add-on, limit, and rollout concerns without approving commercial packaging.
- Applied project-local Impeccable `shape` planning for future Operate-mode surfaces and hardened the planning states, ranges, accessibility, responsiveness, localization, and recovery requirements.

## Classification result

- **Core:** manual product/content management; rapid operational updates; explicit targeting and preview; immediate publish; per-target confirmation; correction, supersession, undo, restoration; offline, stale, conflict, partial-delivery, and failure awareness; accessible customer-authored content; manual fallback without integrations.
- **State:** industry, subtype, timezone, business day, periods, items, options, batches, freshness guidance, availability, expected return, preorder and pickup context, source/freshness, targets, publication, delivery, and restoration points.
- **Permission:** authority to edit, approve, target, publish, override, bulk-change, restore, or view restricted detail.
- **Tier candidates:** recurring schedules, reusable rotations, campaigns, approvals, advanced presentation, coordination, history, analytics, loyalty workflow, and optimization.
- **Add-on candidates:** POS, inventory, production, ordering, payment, fulfillment, loyalty, supplier, weather, event, traffic, translation, hardware, monitoring, and AI services.
- **Limits:** quantities and retention windows.
- **Rollout:** temporary internal release, migration, compatibility, and disable controls only.

## Validation

- Reviewed against `AGENTS.md`, issue #505, the merged RWP-00.27–00.29 Café records, Restaurant inheritance, Track 0 classification policy, and the project-local Impeccable skill.
- Every issue-listed characteristic is addressed and tied to defaults, terminology, content, screen purpose, or classification.
- No jurisdiction-specific rule or unsupported business claim was introduced.
- No product, UI, API, schema, migration, billing, entitlement, feature gate, ordering, payment, production, inventory, fulfillment, analytics, hardware, AI, or integration implementation is included.
- Documentation-only GitHub Actions are authoritative on the exact reviewed PR head.
- Integration and external-system tests remain skipped under the standing owner instruction.

## Shared-record pending queue

After merge, reconcile onto current `master`:

- mark Café complete through RWP-00.30;
- set RWP-00.31 as the exact next Café item;
- record the operating-characteristics result in project status and current handoff;
- release the RWP-00.30 claim and claim RWP-00.31 only after verification;
- preserve every concurrent industry update.

## Completion gate

RWP-00.30 is complete only after exact-head validation and review, PR merge, issue closure, verification on `master`, shared-record synchronization, tracker release, and handoff to RWP-00.31.

## Handoff

**RWP-00.31 — Café, Bakery & Dessert Required Capabilities** (#506) is next. It must define the smallest viable inherited and industry-specific capability set that remains core without a premium tier or paid integration.

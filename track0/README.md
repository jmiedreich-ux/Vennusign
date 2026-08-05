# Track 0 Execution Packet

## Purpose

This directory is the compact context packet for Track 0 industry-planning RWPs. Agents should use it instead of repeatedly loading broad repository history.

## Required reading for every Track 0 RWP

1. `track0/README.md`
2. `track0/CAPABILITY_MODEL.md`
3. `track0/RESTAURANT_BASELINE.md`
4. `track0/INDUSTRY_TEMPLATE.md`
5. `track0/CAPABILITY_MATRIX.md`
6. The current GitHub RWP issue
7. The current industry file under `track0/industries/`

Read additional repository documents only when the current issue explicitly requires them or when a conflict cannot be resolved from this packet.

## Execution model

- Complete one RWP at a time in strict sequence.
- Use a dedicated branch and PR.
- Merge, close, verify, and release the claim before starting the next RWP.
- Industry work is documentation and product-planning only until explicit owner approval authorizes implementation.
- RWP-13.06 and Phase 14+ remain paused during Track 0.
- Integration and external-system tests remain skipped under the standing owner instruction.

## Native-industry roadmap and consolidation gate

| Industry | Final validation RWP | State |
| --- | --- | --- |
| Bar, Brewery & Nightlife | RWP-00.26 | Complete and verified. |
| Café, Bakery & Dessert | RWP-00.38 | Complete and verified. |
| Food Truck & Concession | RWP-00.50 | Complete and verified. |
| Hospitality | RWP-00.62 | Complete and verified. |
| Entertainment & Attractions | RWP-00.74 | Complete and verified. |

The all-industry gate is satisfied. RWP-00.75 may begin only in the separately approved consolidation stream. That stream must refresh current `master`, verify every endpoint and released claim, and then proceed strictly sequentially. Completed industry streams must not invent more work, start product implementation, or begin consolidation themselves.

## Delta rule

Every native industry inherits the Restaurant baseline. Document only meaningful differences in:

- business and venue types;
- terminology;
- daily operations;
- content and screen purposes;
- roles and permissions;
- integrations;
- defaults and recommendations;
- capability classifications;
- onboarding, dashboard, and analytics needs.

Do not restate inherited behavior unless the current industry changes, removes, or qualifies it.

## Impeccable requirement

The project-local Impeccable skill applies to Track 0 planning whenever an RWP defines or changes UI-facing behavior, including onboarding, dashboards, navigation, screen presentation, locked states, action hierarchy, responsive behavior, accessibility, or customer journeys.

For those RWPs, the agent must:

- consult the project-local Impeccable skill before drafting UI-facing recommendations;
- use its vocabulary and workflow to shape, audit, adapt, harden, and polish the specification;
- record the relevant accessibility, responsive, hierarchy, state, and recovery considerations in the RWP output;
- preserve the approved Sky Blue direction;
- avoid inventing implementation details that are not required for planning.

Impeccable consultation does not authorize product implementation.

## Expected outputs per RWP

- Update the current industry document.
- Update `CAPABILITY_MATRIX.md` when classifications or packaging candidates change.
- Record unresolved owner decisions.
- Update the next handoff reference.
- Keep changes bounded to the current issue.
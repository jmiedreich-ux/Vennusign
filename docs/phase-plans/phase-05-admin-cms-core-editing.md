# Phase 05 — Admin CMS Core Editing

## Approved Objective

Deliver the mobile-responsive venue-admin interface used to manage menus and screens. Feature-gated controls must use the established effective-feature service and show informative, dismissible tier prompts rather than errors.

## Sequential Work Packages

1. **WP-05.01 — Menu Domain and Persistence Foundation**
   Persist venue menus, ordered sections, menu items, and translations with the fields required by the approved roadmap.
2. **WP-05.02 — Menu Editor Read Model and Section Management**
   Add venue-scoped menu loading plus create, rename, order, activate, expand, and collapse section journeys.
3. **WP-05.03 — Inline Menu Item Editing and Sync**
   Add bounded inline editing for item text and prices with the existing notification abstraction.
4. **WP-05.04 — Availability, Quantity, and Menu Badges**
   Add availability, limited quantity, dietary/allergen tags, bestseller state, and their content-contract behavior.
5. **WP-05.05 — Tier-Aware Venue Admin Patterns**
   Apply badges, visible disabled fields, previews, dismissible prompts, and the one-prompt-per-screen rule.
6. **WP-05.06 — Quick Update Mode**
   Add the mobile-first daily-special and one-scroll availability journeys plus the midnight-reset boundary.
7. **WP-05.07 — Screen Management Core**
   Add venue screen registration URLs, naming, location, health, and manual content push.
8. **WP-05.08 — Screen Targeting and Overflow Visualization**
   Add single-screen/broadcast targeting and deterministic density/overflow previews.
9. **WP-05.09 — Video Wall Builder**
   Add tier-gated wall groups, supported positions, and 2x1, 3x1, and 2x2 configurations.
10. **WP-05.10 — Phase 05 Validation and Closure**
    Validate critical menu, quick-update, screen, and tier-aware journeys and synchronize phase documentation.

## Governing Boundaries

- Complete packages sequentially and keep each slice independently testable and mergeable.
- Reuse `HasFeatureAsync` and the established notification abstraction.
- Do not pull Phase 06 display-template implementation, Phase 08 scheduling, Phase 09 pairing, Phase 12 POS integration, or Phase 13 translation automation into Phase 05.
- Integration-type tests remain skipped under the standing repository-owner instruction.

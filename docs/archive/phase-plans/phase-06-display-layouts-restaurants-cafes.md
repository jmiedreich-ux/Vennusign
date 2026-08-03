# Phase 06 — Display Layouts — Restaurants & Cafes

## Approved Objective

Deliver the first production-ready restaurant and cafe display layouts, the all-tier basic theme builder, and the player capabilities required for responsive multi-screen and offline-resilient rendering.

## Sequential Work Packages

1. **WP-06.01 — Display Layout Contract and Registry Foundation**
   Define the typed board-layout contract, layout registry, shared display primitives, and deterministic fallback behavior while keeping the existing player boot sequence unchanged.
2. **WP-06.02 — Photo Grid Core Layout**
   Render venue branding and menu items as responsive food-photo cards with CDN-backed images, loading placeholders, names, descriptions, and prices.
3. **WP-06.03 — Photo Grid Merchandising States**
   Add popular ribbons, unavailable/sold-out presentation, allergen and dietary indicators, and happy-hour price presentation driven only by the existing content payload.
4. **WP-06.04 — Photo Grid Density and Multi-Screen Overflow**
   Implement 2x2, 3x2, 4x2, and 3x3 density modes plus deterministic screen-position slicing and bounded overflow behavior.
5. **WP-06.05 — Classic Diner Core Layout**
   Deliver the warm-cream, high-contrast two- and three-column text layout with section grouping, legible typography, and responsive TV-safe spacing.
6. **WP-06.06 — Classic Diner Pricing and Daily Special**
   Add aligned prices, dot leaders, category headers, and the full-width daily-special banner using the Phase 05 Quick Update content contract.
7. **WP-06.07 — Basic Theme Domain and Persistence**
   Persist venue-scoped background color, accent color, and approved font selection; expose validated read and update operations without pulling in Phase 07 advanced theme controls.
8. **WP-06.08 — Basic Theme Builder and Live Preview**
   Add all-tier admin controls, six quick swatches, full color selection, three approved font choices, exact TV preview, and push-to-all behavior through the established notification abstraction.
9. **WP-06.09 — Player Media Caching and Offline Resilience**
   Add lazy CDN image loading, content and media cache versioning, offline fallback, cache invalidation, and recovery behavior without changing the player's normal online boot path.
10. **WP-06.10 — Phase 06 Validation and Closure**
    Validate critical Photo Grid, Classic Diner, theme, overflow, real-time update, and offline journeys; synchronize roadmap, status, tracker, and handoff documentation.

## Governing Boundaries

- Complete packages sequentially and keep each slice independently testable and mergeable.
- Reuse the Phase 05 menu, screen-position, Quick Update, effective-feature, and notification contracts.
- Treat `photo_grid`, `classic_diner`, and the basic theme builder as all-tier capabilities according to the approved roadmap.
- Consume existing availability and `isHappyHour` state; do not implement Phase 08 scheduling or Phase 11 POS synchronization in Phase 06.
- Do not pull Phase 07 advanced layouts, advanced neon theme controls, or expanded font libraries into this phase.
- Keep layout selection additive through the registry so the player boot sequence remains stable as later layouts are introduced.
- Integration-type tests remain skipped under the standing repository-owner instruction.

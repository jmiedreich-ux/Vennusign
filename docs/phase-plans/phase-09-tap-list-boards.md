# Phase 09 — Tap List Boards — Breweries & Bars

## Approved Objective

Make Vennu sellable to breweries and taprooms with an honest beer-specific tap domain, three distinct board styles, venue administration, and no-keyboard pairing-code setup built on the existing screen and pairing foundations.

## Sequential Work Packages

1. **WP-09.01 — Tap Domain and Persistence**
   Add venue-scoped tap categories and tap items with names, style, ABV, IBU, description, price, glass/name colors, availability, coming-soon state, deterministic ordering, and relational constraints without administration or rendering.
2. **WP-09.02 — Tap List Administration and Availability**
   Add protected tier-visible category/item CRUD, exact reorder, color inputs, availability and coming-soon controls, and bounded validation.
3. **WP-09.03 — Classic Chalkboard Drinks Core**
   Add the category-priced Classic Chalkboard layout with drinks title, two-column cocktail list, import/domestic beer sections, and unavailable-state treatment.
4. **WP-09.04 — Classic Chalkboard Administration and TV Polish**
   Add layout selection/preview, category-price controls, chalk illustrations, TV-safe scaling, reduced motion, and offline/realtime validation.
5. **WP-09.05 — Tap Strips Core**
   Add the three-column strip layout with tap numbers, deterministic rotating hand-lettered fonts, style, ABV, price, name glow, and availability states.
6. **WP-09.06 — Tap Strips Administration and Motion Polish**
   Add exact preview, supported color controls, sequential draw-in motion with reduced-motion fallback, TV overflow guidance, and recovery validation.
7. **WP-09.07 — Digital Tap Board Core**
   Add the two-column digital card layout with wood texture, beer-glass SVG color, beer details, price, and six-card page capacity.
8. **WP-09.08 — Digital Tap Overflow and Brewing States**
   Add deterministic multi-page overflow, Now Brewing treatment, rotation/recovery, exact preview, and offline/realtime validation.
9. **WP-09.09 — Pairing Code Registration Completion**
   Complete the `/pair` TV journey, three-second status polling, automatic code regeneration after ten-minute expiry, protected admin claim flow, and automatic display redirect using the existing pairing APIs.
10. **WP-09.10 — Phase 09 Validation and Closure**
    Validate tap domain/admin, all three layouts, availability, motion/accessibility, overflow, pairing expiry/claim/redirect, realtime, offline, tier, and recovery journeys; synchronize closure records.

## Governing Boundaries

- Complete packages sequentially and keep each independently testable and mergeable.
- Keep tap items separate from menu items and preserve venue scoping throughout persistence and APIs.
- Use ordered DbUp migrations with deterministic indexes and relational ownership constraints.
- Add layouts through the existing additive registry and consume the existing display/realtime/cache paths.
- Apply entitlement behavior through established effective-feature and soft-lock patterns.
- Build pairing on the existing screen pairing-code persistence and endpoints; do not create a parallel registration system.
- Preserve all restaurant, bar, scheduling, broadcast, and promotion behavior.
- Do not implement Phase 10 billing UX, Phase 11 POS integration, Phase 12 multilingual behavior, or later AI/platform work.
- Integration-type tests remain skipped under the standing repository-owner instruction.

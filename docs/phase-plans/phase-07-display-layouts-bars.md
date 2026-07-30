# Phase 07 — Display Layouts — Bars

## Approved Objective

Deliver the Pro-tier Neon Chalkboard, full theme builder, Split Layout, and Daily Special Hero experience that make Vennu sellable to bars and upscale restaurants, while preparing multilingual font assets without implementing Phase 13 translation behavior.

## Sequential Work Packages

1. **WP-07.01 — Advanced Theme Domain and Preset Foundation**
   Extend venue-scoped theme persistence with validated neon title/glow/background values, bounded section colours, glow intensity, approved title/item fonts, and five deterministic presets without changing Phase 06 basic-theme defaults.
2. **WP-07.02 — Full Theme Builder Controls and Preview**
   Add Pro-tier preset selection, advanced colour/font controls, section colour editing, glow intensity, and exact player-backed preview with the established soft-lock tier pattern.
3. **WP-07.03 — Neon Chalkboard Core Layout**
   Register the Neon Chalkboard additively and render the chalk surface, neon frame/corners, menu columns, pricing, and section-colour dividers using only display content and advanced theme values.
4. **WP-07.04 — Neon Motion, Texture, and Accessibility**
   Add bounded irregular title flicker, glow breathing, chalk draw-in, scanlines, and reduced-motion behavior without affecting player boot, realtime, or offline caching.
5. **WP-07.05 — Noto Font Preloading**
   Preload the approved Noto Sans SC, KR, JP, and Arabic display assets with deterministic fallbacks, cache-compatible delivery, and no Phase 13 translation UI or rendering logic.
6. **WP-07.06 — Split Layout Domain and Core Rendering**
   Persist validated per-screen 40/60 or 50/50 ratios, register the Pro layout, and render popular-item hero imagery beside the complete text menu.
7. **WP-07.07 — Split Layout Administration and TV Polish**
   Add tier-aware admin selection and ratio controls, exact preview, responsive TV-safe behavior, and focused overflow/allergen/pricing validation.
8. **WP-07.08 — Daily Special Hero Core**
   Register the Pro hero layout and render one daily featured item with full-screen media, description, price, Today Only badge, and deterministic secondary items.
9. **WP-07.09 — Hero Rotation and Administration**
   Add bounded secondary rotation with an eight-second default dwell, validated admin controls, reduced-motion behavior, and stable recovery after realtime or offline updates.
10. **WP-07.10 — Phase 07 Validation and Closure**
    Validate advanced themes, Neon Chalkboard, motion accessibility, Noto delivery, Split Layout, Daily Special Hero, realtime, tier, and offline journeys; synchronize closure records.

## Governing Boundaries

- Complete packages sequentially and keep each independently testable and mergeable.
- Preserve the Phase 06 additive layout registry, player boot sequence, realtime events, content cache, media cache, and basic-theme compatibility.
- Apply Pro-tier access through the existing effective-feature and soft-lock UI patterns; do not hide locked controls or return user-facing authorization errors.
- Reuse existing menu popularity, availability, tags, pricing, happy-hour, daily-special, screen, and theme contracts before extending them.
- Store schema changes only through ordered DbUp migrations.
- Noto work is font delivery only; Phase 13 owns translation editing and bilingual rendering.
- Do not implement Phase 08 scheduling, Phase 09 tap lists, Phase 12 POS, or Phase 15 AI behavior.
- Integration-type tests remain skipped under the standing repository-owner instruction.

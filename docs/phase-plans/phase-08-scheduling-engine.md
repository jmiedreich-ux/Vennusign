# Phase 08 — Scheduling Engine

## Approved Objective

Let venue menus run indefinitely with no daily staff action by resolving venue-local meal periods, happy hours, playlists, emergency broadcasts, and date-bounded promotions, then pushing due content changes through the established SignalR player path.

## Sequential Work Packages

1. **WP-08.01 — Meal Period Domain and Persistence**
   Add venue-scoped meal periods with unique names, local start/end times, active-day masks, enabled state, deterministic ordering, and relational constraints without evaluation or administration.
2. **WP-08.02 — Venue Timezone Schedule Resolver**
   Add a pure deterministic resolver that converts UTC through the venue IANA timezone, handles overnight windows and daylight-saving boundaries, and selects one active meal period by explicit precedence.
3. **WP-08.03 — Meal Period Administration**
   Add protected venue-scoped CRUD, day/time controls, enablement, conflict guidance, and tier-visible patterns without activating player content.
4. **WP-08.04 — Scheduled Content Activation**
   Add layout, menu-filter, and theme targets plus a 60-second hosted evaluator that pushes only state transitions through the existing SignalR and cache paths.
5. **WP-08.05 — Happy Hour Scheduling and Manual Override**
   Add Pro-tier happy-hour windows, active days, automatic evaluation, and explicit force-on/force-off/automatic modes using existing item pricing.
6. **WP-08.06 — Happy Hour Administration and Display**
   Add tier-aware happy-hour controls, active-window banner, and countdown derived from authoritative schedule state.
7. **WP-08.07 — Playlist Domain and Player Rotation**
   Add ordered per-screen slides, supported slide types, validated dwell, optional windows, admin reordering, and stable player rotation.
8. **WP-08.08 — Emergency Broadcast**
   Add venue-wide or screen-targeted full-screen broadcasts with bounded duration, automatic expiry, and deterministic recovery.
9. **WP-08.09 — Date-Range Promotions**
   Add venue-local inclusive promotion ranges, target layouts/content, precedence rules, and automatic activation/expiry.
10. **WP-08.10 — Phase 08 Validation and Closure**
    Validate timezone, overnight, DST, transition-only pushes, happy hour, playlists, broadcasts, promotions, realtime, offline, tier, and recovery journeys; synchronize closure records.

## Governing Boundaries

- Complete packages sequentially and keep each independently testable and mergeable.
- Evaluate all schedules from UTC using the existing venue IANA timezone; never use host-local time.
- Persist schema changes only through ordered DbUp scripts with venue-scoped foreign keys and deterministic indexes.
- Keep schedule resolution pure and unit-testable; isolate timers, persistence, and SignalR delivery at existing infrastructure boundaries.
- Push only effective-state transitions, not every evaluator tick.
- Preserve current manual editing, layouts, themes, realtime events, content/media caches, and offline recovery.
- Apply Pro-tier behavior through existing effective-feature and soft-lock patterns.
- Do not implement Phase 09 tap lists, Phase 12 POS, Phase 13 translation, or Phase 15 AI behavior.
- Integration-type tests remain skipped under the standing repository-owner instruction.

# Scheduling and Live-Control Contract

## Authority and precedence

The API remains authoritative for venue scope, schedule persistence, venue-local timezone resolution, target membership, entitlement, and live notifications. Browser time and browser-selected identifiers never grant authority. Emergency broadcasts are the highest-priority content override. Eligible promotions can override layout; meal periods and happy hour influence scheduled content; eligible playlist slides rotate in saved order.

## Operator safety

- Schedule tasks are separated into deep-linkable tabs with keyboard navigation and an overview that explains precedence.
- Screen-specific work requires an explicit, currently authorized screen target. Empty and failed target states remain visible and fail closed.
- Meal-period and playlist ordering is persisted server-side as a complete bounded order.
- Meal-period responses include the current and next server-resolved period plus venue-local server time; the browser only presents that state.
- Creation/editing reports success or actionable errors. Delete, archive, emergency activation, and emergency cancellation require deliberate confirmation.
- Emergency feedback says that delivery was queued. It does not claim player acknowledgement because the notification transport exposes no acknowledgement receipt.
- Recent broadcast history remains available for recovery context.

## Data and compatibility

Existing scheduling tables already contain enabled state, sort order, venue-local windows, targeting, promotion priority, and broadcast lifecycle data. RWP-08.01 adds no database schema migration. Existing create/update/delete routes remain compatible; the meal-period order route is additive.

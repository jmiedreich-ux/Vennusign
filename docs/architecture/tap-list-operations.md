# Tap-List Operational Contract

Tap categories and items remain venue-scoped through Back Office authentication and controller policy. Feature/tier authorization, category membership, field validation, complete ordering, persistence, and venue-wide screen notification remain server-authoritative.

- Categories cannot be deleted while taps reference them. The UI exposes the dependency count and the service/controller retain the conflict boundary.
- Tap description is an existing optional field, trimmed and limited to 1,000 characters by the service and consumed by display layouts. No migration is required.
- Search and category filtering are presentation-only. Move operations always submit the complete canonical venue order.
- Tap Strips placement is one-based canonical item order: positions 1–12 are visible and later positions are explicit overflow.
- Bulk availability is intentionally limited to 25 selected rows and persists each row through the existing authorized update contract.
- Successful writes queue a venue screen refresh. The UI does not claim player acknowledgement because the notification contract exposes no receipt.

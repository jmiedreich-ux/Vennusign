# Phase 12 — POS Integration

## Approved Objective

Deliver venue-scoped POS connections and near-real-time menu synchronization through one provider abstraction, building Square first, Toast second, and Clover third while preserving the existing menu, availability, pricing, SignalR, and display contracts.

## Sequential Work Packages

1. **WP-12.01 — POS Connection Domain and Provider Contracts**
   Add the venue-scoped connection domain, encrypted-credential persistence boundary, provider/status enums, provider-neutral catalog/inventory result contracts, `IPosProvider` abstraction, migration, dependency registration, and focused repository/service tests without an external provider call.
2. **WP-12.02 — Square OAuth Connection Flow**
   Add claim-bound Square connect/callback/disconnect endpoints with cryptographic state correlation, server-configured allowlisted redirects, encrypted token storage, Venue Admin connection status, and an injectable Square OAuth gateway; exclude catalog sync.
3. **WP-12.03 — Square Catalog Import**
   Add an injectable Square catalog gateway, deterministic category/item/modifier mapping into the existing venue menu domain, idempotent full import, conflict/error reporting, and Venue Admin import progress/result guidance; exclude inventory webhooks.
4. **WP-12.04 — Unified POS Webhook Intake and Idempotency**
   Add `POST /api/webhooks/pos/{provider}`, provider signature verification, bounded payload handling, persistent provider event IDs, immediate accepted responses with a durable in-process work queue, and provider-neutral dispatch without applying menu mutations.
5. **WP-12.05 — Square Inventory and Price Realtime Sync**
   Map confirmed Square item/inventory events to existing venue items, apply availability and price changes idempotently, publish `ItemAvailabilityChanged` or `ContentUpdated`, and preserve venue/provider ownership boundaries.
6. **WP-12.06 — Toast Provider and Webhook Sync**
   Add Toast connection configuration, provider adapter, approved webhook registration/status guidance, catalog/availability mapping through shared contracts, GUID event deduplication, and existing notification reuse; exclude polling.
7. **WP-12.07 — Toast Polling Resilience**
   Add the documented hourly, cancellation-aware Toast availability poller with per-connection isolation, overlap prevention, backoff/health telemetry, provider throttling, and idempotent application through the same sync service.
8. **WP-12.08 — Clover OAuth and Catalog Provider**
   Add claim-bound Clover OAuth, merchant-scoped encrypted connection state, catalog import through `IPosProvider`, and provider-neutral Venue Admin connection/import status without inventory webhooks.
9. **WP-12.09 — Clover Inventory Webhooks and Provider Conformance**
   Add verified Clover inventory events, availability/price propagation, provider conformance tests proving Square/Toast/Clover share the abstraction, and operational connection/sync health without changing existing providers.
10. **WP-12.10 — Phase 12 Validation and Closure**
    Validate OAuth/state/token boundaries, catalog idempotency, unified webhook routing, deduplication, inventory/price propagation, SignalR latency contracts, Toast resilience, provider isolation, security, migration inventory, and reproducible non-integration builds; synchronize closure records and marketplace operational notes.

## Governing Dependencies

- Implement packages strictly in the listed order.
- Reuse the Phase 05 menu and item repositories/services rather than creating a parallel POS menu domain.
- Reuse the existing `ContentUpdated` and `ItemAvailabilityChanged` notifications and the current display patch/reload behavior.
- Reuse processed-event idempotency patterns, but keep POS provider event namespaces separate from Stripe event IDs.
- Keep raw access/refresh tokens encrypted at rest and out of logs, browser responses, issue text, and repository configuration.
- Provider SDKs or HTTP clients remain behind injectable gateways; domain/application tests must not require a live provider.
- A provider may mutate only menu items belonging to the authenticated/connected venue and its recorded external mappings.

## Phase Boundaries

- No future roadmap capabilities beyond the approved Phase 12 POS scope.
- No custom POS sales, payments, orders, or staff workflows beyond approved catalog, availability, and price synchronization.
- No external provider, Azure SQL, hosted infrastructure, container, or cross-system integration tests under the standing owner exception.
- Marketplace submission, production provider approval, and live webhook registration are operational activities; document them but do not simulate success.
- Do not use fire-and-forget `Task.Run` request work. A bounded queue must own asynchronous webhook processing and shutdown behavior.

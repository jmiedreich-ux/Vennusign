# Phase 12 POS Foundation

## Boundary

`Vennu.Core.Models` owns the venue-scoped POS connection model and stable provider/status values. `Vennu.Data` owns persistence, credential-protection application boundaries, and provider-neutral catalog/inventory contracts. Provider HTTP clients and OAuth transport remain future `Vennu.Api` infrastructure behind `IPosProvider`.

## Credential handling

- Callers submit plaintext provider credentials only to `IPosConnectionService` inside the server process.
- The service requires `IPosCredentialProtector` to return a non-empty value different from the plaintext before persistence.
- The API implementation uses ASP.NET Core Data Protection with the versioned purpose `Vennu.PosCredentials.v1`.
- Repository entities contain protected values; `PosConnectionSummary` deliberately contains no credential or token property.
- Deployment must persist and protect the ASP.NET Core Data Protection key ring before OAuth is enabled. Provider credentials, decrypted tokens, and Data Protection keys must not enter logs, browser contracts, source control, issues, or pull requests.

## Provider abstraction

`IPosProvider` exposes cancellation-aware catalog and inventory snapshot operations through provider-neutral contracts. Square, Toast, and Clover implementations may translate their provider payloads into these contracts, but they may not leak provider SDK types into the menu domain.

WP-12.01 makes no provider call and performs no menu mutation. OAuth, import, webhooks, realtime application, and resilience remain assigned to later Phase 12 packages.

## Square OAuth boundary

WP-12.02 uses a ten-minute, Data Protection-backed, single-use OAuth state to correlate the anonymous Square callback to the venue that initiated the flow. Only authenticated Venue Admin requests may start or disconnect a connection. Square transport is isolated behind `ISquareOAuthGateway`; authorization, token, and revoke endpoints are restricted to official HTTPS Square hosts, while callback and Venue Admin return destinations are server-owned HTTPS configuration. Tokens flow only from the gateway into the established credential-protection service.

## Square catalog import boundary

WP-12.03 translates the paged Square Catalog response into `PosCatalogResult` behind `ISquareCatalogGateway` and `IPosProvider`. `PosCatalogImportService` is the only menu-mutation boundary. It resolves the connected venue, unprotects the access token only for the provider call, and persists source ownership in `PosCatalogMappings`, whose unique key is venue, provider, entity type, and external identifier.

The importer owns one mapped `Square Catalog` menu and reuses the existing menu, section, and item repositories. Categories map to sections, item variations map to items, and modifiers map to the owning item while their names are projected into existing presentation tags. Reruns update mapped records; missing local targets, category changes, incomplete pages, and unsupported currency/price data are reported as conflicts rather than silently remapped. Webhooks, inventory events, and realtime broadcasts remain later work.

## Unified webhook intake boundary

WP-12.04 adds one bounded `POST /api/webhooks/pos/{provider}` ingress. Provider-specific `IPosWebhookVerifier` implementations authenticate the exact payload before returning a provider-neutral envelope. Square verification uses the server-configured HTTPS notification URL and HMAC-SHA256 key; signatures and keys are never persisted or logged.

Verified envelopes persist to `PosWebhookEvents` before `202 Accepted`. The unique `(Provider, ProviderEventId)` key is the replay boundary. A database claim uses locking, oldest-first ordering, a five-minute processing lease, and delayed failed retries. The singleton signal only wakes the in-process worker; the database remains authoritative across process restarts. `IPosWebhookEventDispatcher` invokes registered provider-neutral handlers, but WP-12.04 deliberately registers none, so accepted events prove intake/idempotency without mutating menu state. WP-12.05 adds the first mutation handler.

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

WP-12.02 uses a ten-minute, Data Protection-backed, single-use OAuth state to correlate the anonymous Square callback to the venue that initiated the flow. Only authenticated Back Office requests may start or disconnect a connection. Square transport is isolated behind `ISquareOAuthGateway`; authorization, token, and revoke endpoints are restricted to official HTTPS Square hosts, while callback and Back Office return destinations are server-owned HTTPS configuration. Tokens flow only from the gateway into the established credential-protection service.

## Square catalog import boundary

WP-12.03 translates the paged Square Catalog response into `PosCatalogResult` behind `ISquareCatalogGateway` and `IPosProvider`. `PosCatalogImportService` is the only menu-mutation boundary. It resolves the connected venue, unprotects the access token only for the provider call, and persists source ownership in `PosCatalogMappings`, whose unique key is venue, provider, entity type, and external identifier.

The importer owns one mapped `Square Catalog` menu and reuses the existing menu, section, and item repositories. Categories map to sections, item variations map to items, and modifiers map to the owning item while their names are projected into existing presentation tags. Reruns update mapped records; missing local targets, category changes, incomplete pages, and unsupported currency/price data are reported as conflicts rather than silently remapped. Webhooks, inventory events, and realtime broadcasts remain later work.

## Unified webhook intake boundary

WP-12.04 adds one bounded `POST /api/webhooks/pos/{provider}` ingress. Provider-specific `IPosWebhookVerifier` implementations authenticate the exact payload before returning a provider-neutral envelope. Square verification uses the server-configured HTTPS notification URL and HMAC-SHA256 key; signatures and keys are never persisted or logged.

Verified envelopes persist to `PosWebhookEvents` before `202 Accepted`. The unique `(Provider, ProviderEventId)` key is the replay boundary. A database claim uses locking, oldest-first ordering, a five-minute processing lease, and delayed failed retries. The singleton signal only wakes the in-process worker; the database remains authoritative across process restarts. `IPosWebhookEventDispatcher` invokes registered provider-neutral handlers, but WP-12.04 deliberately registers none, so accepted events prove intake/idempotency without mutating menu state. WP-12.05 adds the first mutation handler.

## Square realtime mutation boundary

WP-12.05 registers the first dispatcher handler for Square `inventory.count.updated` and `catalog.version.updated`. The external merchant ID must resolve exactly one connected Square venue before the handler repeats venue/provider ownership in the catalog-mapping lookup. Confirmed in-stock quantities update the existing item, publish `ItemAvailabilityChanged` for availability transitions, and publish `ContentUpdated` whenever the displayed quantity changes. Catalog-version events refresh through the injectable Square provider and apply only supported USD prices, emitting one venue content refresh for changed items. Equal values, unsupported records, missing mappings, ambiguous merchants, and unknown or disconnected merchants are no-ops; persistence failures remain retryable through the WP-12.04 queue.

## Toast provider and webhook boundary

WP-12.06 adds an injectable Toast Menus V2 gateway and translates published menus into the existing provider-neutral catalog contracts. Toast access tokens are accepted only through a venue-claim-bound connection endpoint, protected immediately by the established credential service, and omitted from all response contracts. The restaurant GUID is the external merchant boundary and is repeated on mapping lookups.

Toast `menus_updated`, `in_stock`, `out_of_stock`, and `low_quantity` events enter the existing durable queue. The verifier requires a GUID event identifier and restaurant GUID and validates `Toast-Signature` with the event-category subscription secret over the exact body plus payload timestamp. Menu events import through the shared catalog service; stock events update only the connected venue's mapped item and reuse `ItemAvailabilityChanged` and `ContentUpdated`. Webhook subscription is an external Toast approval and developer-portal operation; the API reports that status honestly and never simulates registration. Polling remains WP-12.07.

## Toast polling resilience boundary

WP-12.07 adds a configurable hourly fallback over Toast's official stock inventory search resource. The hosted service prevents overlapping cycles, orders due venue IDs deterministically, throttles between locations, honors shutdown cancellation, and isolates every location attempt. Each provider call is restricted to the connected venue's recorded Toast item GUIDs; the complete response is applied idempotently through the same `IToastInventorySyncService` used by verified stock webhooks.

Poll health is persisted on the POS connection as last attempt/success, consecutive failures, next attempt, and a bounded error code. Exponential retry starts at five minutes and caps at the hourly interval. Browser status may expose those credential-free fields, but provider tokens, raw response bodies, and exception text remain server-confidential. Authentication failures transition the connection to reauthorization-required; transient failures retain the connection and its scheduled retry.

## Clover OAuth and catalog boundary

WP-12.08 implements Clover's high-trust v2 authorization-code flow. Only an authenticated Back Office venue claim can mint the existing ten-minute, protected, single-use state; the anonymous callback must return that state, the configured Clover client ID, a bounded merchant ID, and an authorization code. Authorization and token exchange accept only the documented Clover sandbox or regional production HTTPS hosts and exact `/oauth/v2/authorize` and `/oauth/v2/token` paths. Access and refresh tokens are protected through the shared credential service, while both provider-supplied Unix expirations are persisted explicitly. Status responses expose merchant, state, and access-token expiry only—never either token.

`CloverCatalogGateway` scopes category, item, and modifier reads to the connected merchant under `/v3/merchants/{mId}` and accepts only official Clover API host roots. It expands item categories and modifier groups, resolves modifiers by their owning group, converts fixed prices from cents, and uses a deterministic synthetic category only for genuinely uncategorized items. Variable prices remain import conflicts through the shared importer rather than being guessed. `CloverPosProvider` exposes the result through `IPosProvider`; the shared venue/provider-owned mapping and menu mutation boundary remains unchanged. Clover inventory webhooks and realtime availability/price mutations remain exclusively WP-12.09.

## Clover inventory and provider conformance boundary

WP-12.09 extends the unified verifier contract to support a bounded batch because one Clover callback may group multiple updates under multiple merchant IDs. The Clover verifier compares the configured `X-Clover-Auth` key in fixed time, requires the configured app ID, caps merchant and event counts, accepts only inventory item keys, and derives a deterministic SHA-256 replay ID from app, merchant, object, operation, and provider timestamp. Each accepted update is normalized before entering the existing durable `PosWebhookEvents` queue; unrelated Clover event types do not gain mutation behavior. Clover receives its documented immediate `200 OK` after durable enqueue, while the existing Square and Toast endpoints retain `202 Accepted`.

The realtime handler resolves exactly one connected Clover merchant, repeats venue/provider ownership in the mapping lookup, and retrieves current non-delete item state through the official-host-only inventory gateway. Availability, tracked whole quantity, and supported fixed USD price changes update the existing menu item idempotently and reuse `ItemAvailabilityChanged` and `ContentUpdated`. Delete events mark only an existing mapped Clover item unavailable. Credential-free status reports last attempt/success and bounded failure state, while webhook registration and its initial verification-code receiver remain explicit Clover Developer Dashboard operations rather than simulated API success.

Provider conformance coverage now fixes Square, Toast, and Clover identities and verifies catalog cancellation/context propagation. Toast and Clover inventory paths must preserve provider merchant and item ownership; Square's unsupported snapshot method remains explicit because its realtime implementation uses the provider's catalog/inventory webhook contract established in WP-12.05.

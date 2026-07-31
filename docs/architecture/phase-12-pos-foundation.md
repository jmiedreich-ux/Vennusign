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

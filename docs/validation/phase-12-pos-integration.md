# Phase 12 — POS Integration Validation

## Result

Phase 12 closed after GitHub Actions run #699 passed on exact reviewed head `9ecb88d2db80f53192496477d218b6a78ac4006b`. Integration-type and live-provider tests were intentionally skipped under the standing repository-owner instruction.

## Acceptance Matrix

| Journey | Repeatable non-integration evidence |
| --- | --- |
| Venue-scoped connection domain, encrypted credentials, and provider-neutral contracts | `PosConnectionServiceTests`, `PosConnectionRepositoryTests`, `DataProtectionPosCredentialProtectorTests`, `PosProviderConformanceTests` |
| Claim-bound Square and Clover OAuth state, callback ownership, token protection, and exact official hosts | `ProtectedPosOAuthStateServiceTests`, `SquareOAuthConnectionServiceTests`, `SquareOAuthGatewayTests`, `CloverOAuthConnectionServiceTests`, `CloverOAuthGatewayTests` |
| Idempotent Square, Toast, and Clover catalog translation through the existing menu domain | `PosCatalogImportServiceTests`, provider catalog gateway tests, `PosCatalogMappingRepositoryTests`, provider conformance tests |
| Bounded webhook routing, signature verification, durable enqueue, replay deduplication, retry, and provider isolation | `PosWebhooksControllerTests`, provider webhook verifier tests, `PosWebhookEventRepositoryTests`, `PosWebhookEventDispatcherTests` |
| Venue/provider-owned availability, quantity, and supported price propagation with existing SignalR notifications | Square, Toast, and Clover realtime sync handler tests; `ToastInventorySyncServiceTests`; `SignalRScreenUpdateNotifierTests` |
| Hourly Toast polling, overlap prevention, cancellation, throttling, backoff, health, and per-connection isolation | `ToastPollingServiceTests`, `ToastInventoryGatewayTests` |
| Protected Venue Admin routes and credential-free connection/sync status | `Phase12CriticalJourneyTests`, Square/Clover controller authorization tests |
| Contiguous embedded migrations 035–039 | `MigrationResourceTests`, `Phase12CriticalJourneyTests` |

## Required Validation

- Dependency restore and complete Release build.
- Super Admin, Venue Admin, and display production builds and frontend tests.
- Android TV and Fire TV debug package builds plus Samsung Tizen and LG webOS static validation.
- All tests marked `Category=Unit`, including the Phase 12 critical journey.
- GitHub Actions review of the exact PR head.

## Marketplace and Operator Notes

- Square, Toast, and Clover production access, marketplace approval, credentials, and webhook registration remain external operator activities.
- Production callback and webhook URLs must be HTTPS and configured only in the corresponding provider portal.
- Provider status surfaces report required external setup; they do not simulate provider approval or webhook-registration success.
- Secrets remain server-side and encrypted at rest. Browser responses, repository configuration, logs, issues, and PRs must remain credential-free.
- Before a live rollout, an operator must confirm provider scopes, callback URLs, webhook subscriptions, secret rotation, venue/merchant ownership, and rollback/disconnect procedures in the target environment.

## Explicitly Skipped

- Azure SQL and every other integration-type test.
- Live Square, Toast, or Clover OAuth, catalog, inventory, webhook, polling, marketplace, and approval checks.
- Tests requiring external services, credentials, hosted infrastructure, containers, or cross-system integration.

## Boundaries

This closure package adds consolidated validation and operational evidence only. It does not add POS sales, order, payment, staff, or provider behavior, and it does not begin Phase 13 multilingual work.

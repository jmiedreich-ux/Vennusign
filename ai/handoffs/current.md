# Vennu Session Handoff

## Work Package

- ID: WP-12.06
- Status: Review
- Execution mode: Sequential

## Git State

- Branch: `wp/12.06-toast-provider-webhook-sync`
- Issue: #306
- Pull request: pending
- CI state: pending

## Completed This Session

- Added venue-scoped Toast connection configuration and credential-free status guidance.
- Added an injectable official-host-only Toast Menus V2 gateway and provider-neutral catalog translation.
- Generalized the existing catalog import service to retain provider ownership for Square and Toast.
- Added Toast message-signature verification using the event-category subscription secret, exact body, and payload timestamp.
- Added durable `menus_updated`, `in_stock`, `out_of_stock`, and `low_quantity` handling through the existing POS queue and notification contracts.
- Added focused verifier, gateway, ownership, idempotency, mapping, and notification tests.

## Files Changed

- Toast API transport, verification, sync handler, Venue Admin controller/contracts, dependency registration, configuration placeholders, tests, and Phase 12 documentation.

## Decisions

- Treat Toast webhook subscription as a manual provider approval/developer-portal operation and report that status honestly.
- Use the Toast event GUID as the existing provider/event replay key.
- Use the restaurant GUID as the external connection boundary and item GUID as the catalog/stock mapping key.
- Keep polling out of WP-12.06; WP-12.07 owns the resilient polling fallback.

## Validation

- Commands: `git diff --check`; assignment JSON parse; focused local .NET tests attempted.
- Results: local .NET SDK is unavailable; GitHub Actions is authoritative.
- Skipped: live Toast, credentialed, Azure SQL, hosted infrastructure, container, and cross-system integration tests under the standing instruction.

## Remaining Work

- Publish the PR, allow required GitHub Actions checks to complete, review the exact head, and merge if green.

## Known Risks or Blockers

- Toast partner approval, credentials, subscription registration, and live payload validation remain operational activities outside this package.

## Exact Next Action

- Publish WP-12.06, validate the exact head through GitHub Actions, and perform the mandatory ChatGPT review.

## Do Not Redo or Reverse

- Do not add polling to this package or claim that Toast webhook registration is automatic.
- Do not weaken the restaurant/provider ownership checks or expose the protected credential.

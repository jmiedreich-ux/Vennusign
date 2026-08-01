# Vennu Session Handoff

## Work Package

- ID: WP-12.07
- Status: Review
- Execution mode: Sequential

## Git State

- Branch: `wp/12.07-toast-polling-resilience`
- Issue: #315
- Pull request: pending
- CI state: pending

## Completed This Session

- Added a configurable hourly Toast polling host with overlap prevention, deterministic due-venue ordering, cancellation, and per-location isolation.
- Added official-host-only Toast inventory search translation over each venue's recorded item GUIDs.
- Refactored Toast stock application into one venue/provider-owned idempotent service shared by webhook and polling paths.
- Added persisted last-attempt/success, bounded error code, failure count, next-attempt, exponential backoff, and reauthorization telemetry.
- Added credential-free Venue Admin polling health and focused non-integration gateway, sync, poller, recovery, and migration tests.

## Decisions

- Poll only recorded Toast item GUIDs through the stock search resource so successful responses are complete snapshots, including `IN_STOCK` recovery.
- Treat incomplete/invalid snapshots as failures rather than guessing availability.
- Keep raw provider errors and response payloads out of telemetry and logs; persist bounded error codes only.
- Retain transient connections and retry with five-minute exponential backoff capped at one hour; authentication failures require reauthorization.

## Validation

- Local checks: `git diff --check`; application and assignment JSON parse.
- GitHub Actions is authoritative for affected .NET build, migration inventory, and unit coverage.
- Live Toast, credentialed, Azure SQL, hosted-infrastructure, container, and cross-system integration tests are intentionally skipped.

## Remaining Work

- Publish the implementation PR, allow impact-based GitHub Actions to complete, review the exact head, and merge if green.

## Known Risks or Blockers

- Live Toast scopes, credentials, rate limits, payloads, and network behavior remain external operational validation.

## Exact Next Action

- Publish WP-12.07, validate the exact head through GitHub Actions, and perform the mandatory ChatGPT review.

## Do Not Redo or Reverse

- Do not poll unowned item identifiers or fall back to raw error text.
- Do not create overlapping cycles or bypass the shared inventory mutation service.

# Phase 02 Vertical-Slice Validation

## Scope

This document records the Phase 02 milestone evidence across the admin/API context and the display client context.

## Automated Evidence

| Capability | Evidence |
|---|---|
| Create venue and screen | `ApiE2ETests.PairingFlow_CanBeDrivenThroughHttpApi` |
| Create and claim pairing code | `ApiE2ETests.PairingFlow_CanBeDrivenThroughHttpApi` |
| Load paired display content | `ApiE2ETests.PairingFlow_CanBeDrivenThroughHttpApi` |
| Heartbeat sets `LastSeen` and `Online` | `ApiE2ETests.PairingFlow_CanBeDrivenThroughHttpApi` and `Phase02VerticalSliceTests.PairedDisplay_TransitionsOnlineThenOfflineWhenHeartbeatBecomesStale` |
| Missing heartbeat produces `Offline` | `Phase02VerticalSliceTests.PairedDisplay_TransitionsOnlineThenOfflineWhenHeartbeatBecomesStale` plus heartbeat-monitor boundary tests |
| Display joins `screen:{screenId}` and rejoins after reconnect | WP-02.10 display SignalR lifecycle tests |
| Correct pushed events reach the screen or venue group | WP-02.12 notifier routing tests for `ContentUpdated`, `ThemeUpdated`, `ItemAvailabilityChanged`, and `SyncTick` |
| Full repository validation | `./scripts/validate.ps1` in PR CI |

## Two-Context Manual Validation

Use a development API and two separate browser contexts.

### Context A — Admin/API caller

1. Create a venue.
2. Register a screen.
3. Generate a pairing code for the screen.
4. Claim the code for the venue.
5. Publish a `ContentUpdated` notification through `IScreenUpdateNotifier` for that screen.
6. Observe screen status while heartbeats are active and after heartbeats stop.

### Context B — Display client

1. Open the display route with the registered screen ID.
2. Confirm paired content loads without an error or page reload.
3. Confirm the SignalR connection reaches `connected` and joins `screen:{screenId}`.
4. Confirm the pushed `ContentUpdated` payload changes the rendered display without reloading.
5. Confirm heartbeat requests occur every 30 seconds and keep the screen `Online`.
6. Stop or close the display client.
7. After the configured stale threshold and monitor interval, confirm the screen becomes `Offline`.

## Expected Results

- The paired screen loads its display content.
- A notification targeted to another screen does not alter this display.
- A notification targeted to this screen updates it without a reload.
- Active heartbeats maintain `Online` status and update `LastSeen`.
- A heartbeat older than 90 seconds is transitioned to `Offline` by the monitor.

## Accepted Limitation

The connected GitHub execution environment can create code and inspect CI but cannot launch two interactive browser contexts. The manual procedure above must therefore be executed in a running development environment. Automated tests cover each contract and the combined HTTP lifecycle; PR CI is the authoritative automated validation record.

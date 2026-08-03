# Vennu — Next Steps

## Current Status

The project is still in **Phase 02 — Core Backend & Real-Time Engine**, but the initial backend API slice is now largely implemented.

Completed Phase 02 backend pieces:

- `POST /api/venues`
- `POST /api/screens`
- `POST /api/screens/pairing-code`
- `GET /api/screens/pairing/{code}/status`
- `POST /api/screens/pairing/{code}/claim`
- `GET /api/display/{screenId}/content`
- `POST /api/display/{screenId}/heartbeat`
- `VennuHub` mapped at `/hubs/vennu`
- Screen key generation using `sc-{6 chars}`
- Pairing code generation using 6 digits
- Unit and E2E test coverage for the initial pairing, heartbeat, and display-content flow

## Immediate Goal

Finish the Phase 02 vertical slice so the milestone is true in practice:

> A screen can boot, fetch content, connect to SignalR, send heartbeats, and receive real-time updates.

## Start Here

### 1. Display SPA boot flow

Create or complete the display app under `src/display`.

The display app should:

1. Read `screenId` from the route, e.g. `/display/{screenId}`.
2. Fetch `GET /api/display/{screenId}/content` on load.
3. Render a minimal board using the returned payload.
4. Connect to SignalR at `/hubs/vennu`.
5. Call `JoinScreen(screenId)` after connecting.
6. Start a 30-second heartbeat loop using `POST /api/display/{screenId}/heartbeat`.
7. Show offline/error state if the content fetch fails.

Keep this simple. Do not build final layouts yet.

### 2. SignalR client event handling

Add client handlers for these events, even if the first implementation only logs or updates simple local state:

- `ContentUpdated`
- `ThemeUpdated`
- `ItemAvailabilityChanged`
- `SyncTick`

### 3. Backend notification abstraction

Add a small service around `IHubContext<VennuHub>` so controllers and future services do not directly depend on SignalR internals.

Suggested shape:

- `IScreenUpdateNotifier`
- `NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken)`
- `NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken)`
- `NotifyThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken)`
- `NotifyItemAvailabilityChangedAsync(Guid screenId, Guid itemId, bool available, CancellationToken cancellationToken)`

Use existing group naming conventions:

- `screen:{screenId}`
- `venue:{venueId}`

### 4. Heartbeat monitor hosted service

Add a background service that marks screens offline when they have not been seen recently.

Roadmap behavior:

- Heartbeat interval: every 30 seconds from the display client
- Offline threshold: no heartbeat for 90 seconds
- Status update: set screen `Status` to `Offline`

This may require adding repository support to query stale online screens or update status in bulk.

### 5. Manual Phase 02 validation

Validate with two browser/client contexts:

1. Display client:
   - Load `/display/{screenId}`.
   - Confirm it fetches content.
   - Confirm it joins the SignalR screen group.
   - Confirm it sends heartbeats.

2. Admin/API caller:
   - Create venue.
   - Register screen.
   - Create pairing code.
   - Claim pairing code.
   - Trigger a test SignalR push.

Success criteria:

- Display shows the screen content.
- `LastSeen` updates through heartbeat.
- Screen status becomes `Online`.
- A SignalR push reaches the display tab.

## After Phase 02

Move to **Phase 03 — Tier System & Feature Flags** only after the display boot and real-time slice is validated.

Phase 03 starts with:

1. DbUp migrations for:
   - `Feature`
   - `SubscriptionTier`
   - `TierFeature`
   - `VenueSubscription`
   - `VenueFeatureOverride`
2. Models and repositories for those entities.
3. `HasFeatureAsync(venueId, featureKey)` feature resolution service.
4. `GetFeatureSetAsync(venueId)`.
5. Initial tier and feature seed data.

## Notes

- The workspace targets `.NET 9`.
- Keep `Vennu.DataAccess` generic; Vennu-specific repository logic belongs in `Vennu.Data`.
- Keep schema changes in DbUp scripts only.
- Do not add `src/display` as a Visual Studio Website project. Run it separately with npm.

# Player Delivery Reliability Contract

Player identity, venue ownership, active state, and push authorization remain server-authoritative. Pairing establishes identity; it does not prove that the player has connected. Online state is derived from heartbeat activity and must recover automatically in Back Office without a manual page refresh.

- Back Office refreshes the authoritative screen list every ten seconds while visible and immediately after the page regains visibility. Onboarding uses the same cadence for the first paired screen.
- Operators must select one active, authorized screen before pushing. A push uses the existing structured screen-content contract and never sends raw command/debug text for display rendering.
- API acceptance means the request was queued for the selected screen. Until the server exposes a durable player acknowledgement contract, the UI must not label the change applied or delivered. Offline and failed requests remain retryable against the same selected target.
- Persistent content-change events, including theme/configuration, playlist, tap-list, scheduled content, promotions, and manual push, cause the player to reload its authoritative screen content. Event metadata never replaces current content.
- The player periodically recovers authoritative content in addition to SignalR reconnect behavior, covering missed events and delayed configuration changes.
- Web, Android/Fire TV, Tizen, and webOS player shells suppress document scrolling. Android re-enters immersive fullscreen when started and whenever window focus returns; lock-task mode remains the explicit managed-device policy.
- Device-hosted SignalR, heartbeat timing, platform fullscreen behavior, and end-to-end delivery acknowledgement require the separately approved integration/device environment and are not asserted by unit tests.

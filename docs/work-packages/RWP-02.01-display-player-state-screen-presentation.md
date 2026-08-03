# RWP-02.01 — Display Player State-Screen Presentation

## Outcome

The player presents loading, routing, provisioning, content-load, and unexpected-failure states as intentional high-contrast TV screens instead of unstyled dark-screen text. Offline and live-connection states explain what content remains visible and how recovery occurs.

## Required implementation

- Reuse one player state-screen component for player loading and failure surfaces.
- Make content-load failures retryable without restarting the player shell.
- Preserve cached-content playback while showing the age of the saved content and automatic recovery guidance.
- Show connecting, reconnecting, and degraded live-update states without obscuring menu content.
- Provide a restrained heartbeat cue and disable its animation when reduced motion is preferred.
- Preserve fullscreen/no-scroll player behavior, content precedence, cache isolation, realtime recovery, and device-shell boundaries.

## UI and function gap analysis

- **Goal and hierarchy:** the current operating state, plain-language explanation, and only relevant recovery action are visible at TV distance. The Vennusign player identity is secondary to the state title.
- **Navigation and actions:** player state screens do not introduce navigation. Recoverable content errors expose one `Try again` action; unexpected failures expose `Reload player`. Loading and provisioning remain automatic. Invalid delivery-token and invalid-route states provide guidance without unsafe mutation.
- **Essential states:** loading, route missing, content missing, API/network failure, provisioning, provisioning failure, unexpected render failure, connected, connecting, reconnecting, degraded, cached/offline, and restored network content are distinct. Cached content is never presented as live.
- **Validation and destructive actions:** no user input or destructive action is introduced. Retry starts a fresh abortable load cycle and does not clear cached content, pairing identity, or device state.
- **Feedback:** offline copy includes deterministic saved-content age and tells operators that new updates resume automatically. Connecting/reconnecting/degraded banners remain above content; the healthy connected state is announced but visually unobtrusive.
- **Accessibility:** semantic status/alert roles, `aria-busy`, polite connection announcements, high-contrast type, visible keyboard focus, non-color text labels, and an explicit `prefers-reduced-motion` override are included. Decorative heartbeat marks are hidden from assistive technology.
- **Responsiveness:** typography and spacing scale from browser/mobile preview widths through TV canvases; safe-area insets and the existing no-scroll fullscreen contract are preserved.
- **API, data, authorization, and entitlement support:** no endpoint, payload, authorization, tenant, or entitlement change is required. Existing resilient content loading, screen-bound cache, SignalR state, heartbeat, and receipt flows remain authoritative.

## Acceptance evidence

- `PlayerStateScreen` supplies the shared visible presentation for display loading/error, routing, provisioning, and unexpected failures.
- Display content `api-error` and `not-found` states can retry through a new abortable load attempt.
- Ready state retains `cachedAt`; offline messaging reports saved-content age and recovery behavior.
- Connection-state presentation distinguishes connected, connecting, reconnecting, and degraded behavior.
- Heartbeat animation is removed under `prefers-reduced-motion: reduce`.
- Focused display tests cover presentation decisions, offline age, connection transitions, and reduced-motion behavior.

## Validation

- Display Node tests: 135 passed locally.
- Display TypeScript/Vite production build: passed locally.
- Patch whitespace validation: passed.
- Exact-head affected-area GitHub Actions is authoritative before merge.

## Skipped integration testing

Hosted-player/browser automation, physical TV/device shells, live SignalR and heartbeat services, Azure SQL, external-service, credentialed, hosted-infrastructure, container, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Queue and boundaries

- Issue: #448.
- Branch: `rwp/02.01-display-state-screens`.
- First item in the approved 18-RWP remediation queue.
- RWP-00.06 / #449 becomes next only after this PR merges, issue #448 closes, the default branch is verified, and the claim is released.
- RWP-13.06 / #466 remains held; Phase 14+ remains paused.

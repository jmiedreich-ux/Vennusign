# RWP-10.01 — Player Runtime, Targeting, and Realtime Delivery Reliability

## Outcome

Player and Back Office behavior now recover status/content automatically, maintain kiosk-safe presentation, and require one explicit authorized screen target for structured pushes. Delivery feedback states what the current transport proves and does not misrepresent queue acceptance as player application.

## UI and Function Gap Analysis

The review applied WAI-ARIA dialog/alert-dialog patterns plus WCAG 2.2 error-identification, input-assistance, status-message, keyboard, and responsive-control guidance.

| Surface | Goal, navigation, and actions | Essential states and validation | High-impact safety | Accessibility and responsiveness | Required support |
| --- | --- | --- | --- | --- | --- |
| Onboarding activation | Show the first paired screen becoming usable without operator refresh. | Unpaired, paired/offline, reconnecting, Online, polling error with last known state. Pairing is not labeled Online. | No destructive action; retry remains the existing pairing flow. | Text status is not color-only; automatic refresh does not steal focus. | Server-authorized onboarding snapshot, heartbeat-derived online state, visibility recovery. |
| Screen targeting and push | Select exactly one active screen, preview that screen, then push structured content. | No screens, no selection, selected online/offline, inactive target, pending, queued, failed, retry, changed fleet. | Push fails closed without an explicit active target; retry preserves the target; no all-screen shortcut. | Native labeled selector and buttons, `aria-pressed` row selection, status/alert live regions, narrow-screen stacking. | Venue-scoped screen list and push authorization, structured content API, active-state validation. |
| Player runtime | Render the current screen in fullscreen without scrollbars and recover theme/config/content changes. | Loading, live, reconnecting, offline cache, stale/missed event recovery, unsupported managed lock-task policy. | Event metadata cannot replace display content; periodic recovery restores authoritative state. | Status is visually hidden while healthy and visible when offline; safe-area-aware notice; scroll suppression across shells. | Pairing identity, content snapshot, SignalR events, heartbeat, local cache, platform fullscreen APIs. |
| Delivery truth | Explain whether a request is pending, queued, offline, or failed. | Request time provides correlation for the operator session; queued is not called applied. Player-applied acknowledgement is explicitly unsupported by the current transport. | No false success; failure and offline states retain retry. Partial cross-screen delivery is impossible because one target is required. | Status/alert semantics expose changes without relying on color. | A future durable receipt/version contract is required before exposing acknowledged/applied or conflict states. |

## Validation

- Back Office Node tests: 55 passed locally.
- Back Office production build: passed locally.
- Display Node tests: 124 passed locally.
- Display production build and Android/Tizen/webOS packaging are delegated to exact-head affected-area GitHub Actions because the local TypeScript/compiler and device toolchains are unavailable.
- Manual integration checklist (skipped pending approved environment): pair a player and observe Online transition; disconnect/reconnect and observe status recovery; change theme/configuration and observe target-only refresh; push to each platform shell; verify immersive fullscreen and no scrollbars; simulate offline/retry; inspect correlation and any future acknowledgement receipt.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests.

## Research References

- [WAI-ARIA Authoring Practices: Dialog (Modal)](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/)
- [WAI-ARIA Authoring Practices: Alert and Message Dialogs](https://www.w3.org/WAI/ARIA/apg/patterns/alertdialog/)
- [WCAG 2.2 error identification](https://www.w3.org/WAI/WCAG22/Understanding/error-identification)
- [WCAG 2.2 input assistance](https://www.w3.org/WAI/WCAG22/Understanding/input-assistance)
- [WCAG 2.2 status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages)

## Completion

- Issue: #423
- Mode: Sequential
- Branch: `rwp/10.01-player-runtime-reliability`
- Next approved package after merge and claim release: RWP-11.02 / issue #348

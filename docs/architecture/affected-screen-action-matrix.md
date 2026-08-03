# Affected-Screen Action Matrix

| Surface | Primary/save | Cancel/revert | Destructive confirmation | Retry/refresh | Navigation/preview | Result |
| --- | --- | --- | --- | --- | --- | --- |
| Screen Management | Add, pair, Save changes, push | Cancel changes, cancel replacement | Archive, reset, unpair, replacement confirmation | Failed push retry; polling/online refresh | Explicit selected-screen Preview and close | Completed in RWP-00.05 |
| Account Security | Add passkey, Save name | Browser authenticator cancel remains native | Remove passkey confirmation and server lockout guard | Retry passkey inventory; mutation refresh | Account Security route | Completed in RWP-00.05 |
| Theme Builder | Save basic/full, apply preset | Draft remains local until save; reset is separately confirmed | Venue-wide reset confirmation | Retry initial theme controls | Screen-selectable player-backed preview | Completed in RWP-00.05 |
| Menu editor | Explicit saves and reorder | Drafts remain until save | Delete confirmations | Retry menus/last change | Menu navigation | Already complete; no change |
| Quick Update | Save/push and availability actions | Undo availability | Bulk change confirmation | Refresh current state before retry | Quick Update route | Already complete; no change |
| Scheduling | Save meal periods/playlists/overrides | Editable drafts | Delete/override confirmations | Recoverable error guidance | Deep-linked task navigation | Already complete; no change |
| Tap list | Save/reorder | Draft remains local | Delete confirmations | Retry last change | Tap-list navigation | Already complete; no change |
| Billing/onboarding | Hosted action/resume | Hosted cancellation | Subscription confirmation remains provider-authoritative | Resume/retry states | Canonical routes | Approved exclusion: separate commercial workflow |

Preview is read-only and never calls the push API. Authorization, venue scope, passkey lockout safety, replacement, and delivery receipt boundaries remain unchanged.

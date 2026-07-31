# Phase 10 — TV Apps & Platform Distribution

## Approved Objective

Package the existing React display player for Android TV, Amazon Fire TV, Samsung Tizen, and LG webOS with no-keyboard pairing, startup recovery, platform/version visibility, and a bounded HaaS pre-registration path without forking player behavior.

## Sequential Work Packages

1. **WP-10.01 — Platform Launch Contract and Player Bridge**
   Add a shared platform launch/configuration contract that identifies platform and app version, selects pairing or display startup, and preserves the existing browser player path without native packaging.
2. **WP-10.02 — Android TV and Fire TV Shell Foundation**
   Add the thin Kotlin WebView shell, TV launcher manifest, network policy, remote-safe focus defaults, and deterministic loading/error states for Android TV and Fire TV.
3. **WP-10.03 — Android Pairing and Durable Launch State**
   Connect the shell to the Phase 09 pairing journey, persist the claimed display target safely, and recover pairing/display navigation across activity recreation and process restart.
4. **WP-10.04 — Android Boot and Lifecycle Recovery**
   Add opt-in boot launch, foreground/lifecycle recovery, network reconnection, and bounded update/reload behavior without changing the React player.
5. **WP-10.05 — Android Kiosk and Operator Escape**
   Add documented pinned/device-owner kiosk behavior, protected operator exit, screen-awake handling, and safe fallback when device-owner privileges are unavailable.
6. **WP-10.06 — Android and Fire Distribution Profiles**
   Add reproducible Google TV and Amazon Fire build variants, versioning, release metadata, store-readiness assets/checklists, and CI validation without publishing or storing signing secrets.
7. **WP-10.07 — Samsung Tizen Package**
   Add the Tizen web-app manifest, hosted-player launcher, remote/navigation policy, pairing bootstrap, build scripts, and simulator/static validation without store submission credentials.
8. **WP-10.08 — LG webOS Package**
   Add the webOS application manifest, hosted-player launcher, pairing bootstrap, lifecycle handling, build scripts, and simulator/static validation without store submission credentials.
9. **WP-10.09 — HaaS Pre-Registration and Fleet Version Health**
   Add protected pre-registration delivery metadata, platform/app-version heartbeat reporting, outdated-screen visibility, and a zero-pairing preconfigured startup contract.
10. **WP-10.10 — Phase 10 Validation and Closure**
    Validate shared-player parity, pairing/launch recovery, Android/Fire lifecycle and kiosk behavior, Tizen/webOS packages, version health, HaaS pre-registration, security boundaries, and reproducible non-integration builds; synchronize closure records.

## Governing Boundaries

- Complete packages sequentially and keep each independently testable and mergeable.
- Keep the React display SPA authoritative; platform wrappers host or launch it and must not fork layout, realtime, scheduling, cache, or pairing logic.
- Reuse Phase 09 pairing endpoints and the established protected claim flow.
- Never commit signing keys, store credentials, production device-owner secrets, or proprietary SDK binaries.
- Treat actual app-store enrollment, signing, submission, certification, and physical-device testing as release operations outside code completion unless safe test credentials and hardware are explicitly provided.
- Preserve browser display behavior and every Phase 06–09 critical journey.
- Do not implement Phase 11 billing UX, Phase 12 POS, or later product behavior.
- Integration-type tests and external store/device tests remain skipped under the standing repository-owner instruction.

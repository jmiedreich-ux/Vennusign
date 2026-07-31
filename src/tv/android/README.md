# Vennu Android TV / Fire TV shell

This is a thin Kotlin WebView host for the authoritative hosted Vennu display player. It supplies platform and app-version metadata at document start; display layouts, pairing, realtime behavior, scheduling, and offline player behavior remain in `src/display`.

## Build

Use JDK 17 and Android SDK 35, then run the project with Gradle 8.9+ or open this directory in Android Studio. The default variant targets Android TV and identifies itself as `android_tv`. Fire-specific distribution variants are intentionally deferred to WP-10.06.

The shell:

- exposes a TV launcher entry and requires Leanback while making touch optional;
- permits only HTTPS navigation to the configured Vennu display origin;
- provides deterministic loading, error, and D-pad-focusable retry states;
- injects `window.__VENNU_PLATFORM__` before the player document executes.

No signing key, store credential, device-owner configuration, or production secret belongs in this directory.

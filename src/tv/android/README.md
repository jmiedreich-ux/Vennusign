# Vennu Android TV / Fire TV shell

This is a thin Kotlin WebView host for the authoritative hosted Vennu display player. It supplies platform and app-version metadata at document start; display layouts, pairing, realtime behavior, scheduling, and offline player behavior remain in `src/display`.

## Build

Use JDK 17, Android SDK 35, and Gradle 8.9+ or open this directory in Android Studio.

Unsigned development builds:

```text
gradle :app:assembleGoogleTvDebug :app:assembleFireTvDebug --no-daemon
```

The `googleTv` flavor uses application ID `com.vennu.tv.googletv` and platform `android_tv`; `fireTv` uses `com.vennu.tv.firetv` and platform `fire_tv`. Override release inputs with `VENNU_VERSION_CODE`, `VENNU_VERSION_NAME`, and `VENNU_BASE_URL`, or the corresponding Gradle properties `vennuVersionCode`, `vennuVersionName`, and `vennuBaseUrl`.

The shell:

- exposes a TV launcher entry and requires Leanback while making touch optional;
- permits only HTTPS navigation to the configured Vennu display origin;
- provides deterministic loading, error, and D-pad-focusable retry states;
- injects `window.__VENNU_PLATFORM__` before the player document executes.

No signing key, store credential, device-owner configuration, or production secret belongs in this directory.

import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const androidRoot = new URL('../../tv/android/', import.meta.url);
const readAndroid = (path) => readFile(new URL(path, androidRoot), 'utf8');
const [manifest, activity, appGradle, networkPolicy, bootReceiver, launchState, kiosk] = await Promise.all([
  readAndroid('app/src/main/AndroidManifest.xml'),
  readAndroid('app/src/main/java/com/vennu/tv/MainActivity.kt'),
  readAndroid('app/build.gradle.kts'),
  readAndroid('app/src/main/res/xml/network_security_config.xml'),
  readAndroid('app/src/main/java/com/vennu/tv/BootReceiver.kt'),
  readAndroid('app/src/main/java/com/vennu/tv/LaunchStatePreferences.kt'),
  readAndroid('app/src/main/java/com/vennu/tv/KioskController.kt')
]);

test('declares a remote-first TV launcher with a strict network policy', () => {
  assert.match(manifest, /android\.software\.leanback/);
  assert.match(manifest, /android\.hardware\.touchscreen[\s\S]*android:required="false"/);
  assert.match(manifest, /android\.permission\.INTERNET/);
  assert.match(manifest, /android\.intent\.category\.LEANBACK_LAUNCHER/);
  assert.match(manifest, /android:usesCleartextTraffic="false"/);
  assert.match(manifest, /android:networkSecurityConfig="@xml\/network_security_config"/);
  assert.match(networkPolicy, /cleartextTrafficPermitted="false"/);
});

test('keeps kiosk opt-in with lock-task and immersive fallback paths', () => {
  assert.match(manifest, /android:lockTaskMode="if_whitelisted"/);
  assert.match(activity, /getQueryParameter\(KIOSK_QUERY\)/);
  assert.match(kiosk, /FLAG_KEEP_SCREEN_ON/);
  assert.match(kiosk, /isLockTaskPermitted/);
  assert.match(kiosk, /activity\.startLockTask\(\)/);
  assert.match(kiosk, /PINNING_REQUESTED/);
  assert.match(kiosk, /IMMERSIVE_FALLBACK/);
  assert.match(kiosk, /IMMERSIVE_STICKY/);
});

test('protects operator escape with Android device credential', () => {
  assert.match(activity, /onKeyLongPress/);
  assert.match(activity, /KeyEvent\.KEYCODE_BACK/);
  assert.match(activity, /keyguard\.isDeviceSecure/);
  assert.match(activity, /createConfirmDeviceCredentialIntent/);
  assert.match(activity, /result\.resultCode == Activity\.RESULT_OK/);
  assert.match(activity, /kioskController\.deactivate\(\)/);
  assert.doesNotMatch(activity, /OPERATOR_EXIT_PIN|password|secret/i);
});

test('keeps boot launch opt-in and starts the shell only after explicit enablement', () => {
  assert.match(manifest, /android\.permission\.RECEIVE_BOOT_COMPLETED/);
  assert.match(manifest, /android\.permission\.ACCESS_NETWORK_STATE/);
  assert.match(manifest, /android:name="\.BootReceiver"/);
  assert.match(manifest, /android\.intent\.action\.BOOT_COMPLETED/);
  assert.match(bootReceiver, /Intent\.ACTION_BOOT_COMPLETED/);
  assert.match(bootReceiver, /isBootLaunchEnabled/);
  assert.match(launchState, /getBoolean\(BOOT_LAUNCH_ENABLED, false\)/);
  assert.match(activity, /getQueryParameter\(BOOT_QUERY\)/);
});

test('bounds lifecycle and network recovery until a successful commit', () => {
  assert.match(activity, /registerDefaultNetworkCallback/);
  assert.match(activity, /if \(playerState == PlayerState\.ERROR\) requestAutomaticRecovery/);
  assert.match(activity, /timeAway >= STALE_FOREGROUND_MS/);
  assert.match(activity, /automaticReloads >= MAX_AUTOMATIC_RELOADS/);
  assert.match(activity, /AUTOMATIC_RELOAD_COOLDOWN_MS/);
  assert.match(activity, /automaticReloads = 0/);
  assert.match(activity, /webView\.onPause\(\)/);
  assert.match(activity, /webView\.onResume\(\)/);
});

test('injects the shared platform contract at document start', () => {
  assert.match(activity, /WebViewCompat\.addDocumentStartJavaScript/);
  assert.match(activity, /window, "__VENNU_PLATFORM__"/);
  assert.match(activity, /BuildConfig\.TV_PLATFORM/);
  assert.match(activity, /BuildConfig\.VERSION_NAME/);
  assert.match(activity, /readScreenId\(\)/);
  assert.match(activity, /screenId: \$\{JSONObject\.quote\(it\)\}/);
  assert.match(activity, /settings\.javaScriptEnabled = true/);
  assert.match(activity, /settings\.domStorageEnabled = true/);
});

test('persists only a trusted valid screen route and resumes it after restart', () => {
  assert.match(activity, /getSharedPreferences\(LaunchStatePreferences\.NAME, MODE_PRIVATE\)/);
  assert.match(activity, /recordTrustedNavigation/);
  assert.match(activity, /if \(!isAllowed\(url\)\) return/);
  assert.match(activity, /DISPLAY_PATH\.matchEntire/);
  assert.match(activity, /UUID\.fromString/);
  assert.match(activity, /putString\(LaunchStatePreferences\.SCREEN_ID, screenId\)/);
  assert.match(activity, /"\/display\/\$\{Uri\.encode\(it\)\}"/);
});

test('clears corrupted state and supports a narrow same-origin re-pair route', () => {
  assert.match(activity, /remove\(LaunchStatePreferences\.SCREEN_ID\)/);
  assert.match(activity, /url\.path == "\/pair"/);
  assert.match(activity, /getQueryParameter\(RESET_QUERY\) == "1"/);
  assert.doesNotMatch(activity, /addJavascriptInterface/);
});

test('has deterministic loading, HTTPS navigation, error, and D-pad focus behavior', () => {
  assert.match(activity, /url\.scheme == "https"/);
  assert.match(activity, /shouldOverrideUrlLoading/);
  assert.match(activity, /onPageCommitVisible/);
  assert.match(activity, /onReceivedError/);
  assert.match(activity, /retryButton\.requestFocus\(\)/);
  assert.match(activity, /webView\.requestFocus\(\)/);
});

test('keeps distribution signing and platform variants outside the shell foundation', () => {
  assert.match(appGradle, /https:\/\/display\.vennu\.app/);
  assert.match(appGradle, /android_tv/);
  assert.match(appGradle, /androidx\.webkit:webkit/);
  assert.doesNotMatch(appGradle, /signingConfig|storeFile|storePassword|keyAlias|keyPassword/);
});

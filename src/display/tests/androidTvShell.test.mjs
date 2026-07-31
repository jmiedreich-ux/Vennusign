import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const androidRoot = new URL('../../tv/android/', import.meta.url);
const readAndroid = (path) => readFile(new URL(path, androidRoot), 'utf8');
const [manifest, activity, appGradle, networkPolicy] = await Promise.all([
  readAndroid('app/src/main/AndroidManifest.xml'),
  readAndroid('app/src/main/java/com/vennu/tv/MainActivity.kt'),
  readAndroid('app/build.gradle.kts'),
  readAndroid('app/src/main/res/xml/network_security_config.xml')
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

test('injects the shared platform contract at document start', () => {
  assert.match(activity, /WebViewCompat\.addDocumentStartJavaScript/);
  assert.match(activity, /window, "__VENNU_PLATFORM__"/);
  assert.match(activity, /BuildConfig\.TV_PLATFORM/);
  assert.match(activity, /BuildConfig\.VERSION_NAME/);
  assert.match(activity, /settings\.javaScriptEnabled = true/);
  assert.match(activity, /settings\.domStorageEnabled = true/);
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

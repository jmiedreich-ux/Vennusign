import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import { readPlatformBootstrap, resolvePlatformLaunch } from '../src/platformLaunch.mjs';

const read = path => readFile(new URL(path, import.meta.url), 'utf8');
const [
  app,
  pairing,
  provisioning,
  display,
  android,
  kiosk,
  androidBuild,
  tizenLauncher,
  tizenConfig,
  webosLauncher,
  webosConfig,
  workflow
] = await Promise.all([
  read('../src/App.tsx'),
  read('../src/PairingPage.tsx'),
  read('../src/ProvisioningPage.tsx'),
  read('../src/DisplayPage.tsx'),
  read('../../tv/android/app/src/main/java/com/vennu/tv/MainActivity.kt'),
  read('../../tv/android/app/src/main/java/com/vennu/tv/KioskController.kt'),
  read('../../tv/android/app/build.gradle.kts'),
  read('../../tv/tizen/launcher.js'),
  read('../../tv/tizen/launcher.config.js'),
  read('../../tv/webos/launcher.js'),
  read('../../tv/webos/launcher.config.js'),
  read('../../../.github/workflows/phase02-tests.yml')
]);

test('all TV platforms launch the authoritative shared player without forking display behavior', () => {
  for (const platform of ['android_tv', 'fire_tv', 'tizen', 'webos']) {
    assert.equal(resolvePlatformLaunch('/', { platform, appVersion: '1.0.0' }).pathname, '/pair');
  }
  assert.match(app, /<DisplayPage/);
  assert.match(display, /loadDisplayContentResilient/);
  assert.match(androidBuild, /https:\/\/display\.vennu\.app/);
  assert.match(tizenConfig, /https:\/\/display\.vennu\.app/);
  assert.match(webosConfig, /https:\/\/display\.vennu\.app/);
});

test('pairing and HaaS provisioning stay keyboard-free bounded and bridge-only', () => {
  assert.match(pairing, /PAIRING_POLL_INTERVAL_MS/);
  assert.match(pairing, /error\.status === 410/);
  assert.match(pairing, /window\.location\.replace/);
  assert.doesNotMatch(pairing, /<input|<form/);
  assert.match(app, /window\.__VENNU_PLATFORM__/);
  assert.match(provisioning, /claimPreRegisteredScreen/);
  assert.match(provisioning, /window\.location\.replace/);
  assert.equal(
    readPlatformBootstrap('?vennuPlatform=webos&vennuVersion=1.0.0&vennuProvision=secret')
      .provisioningToken,
    undefined
  );
});

test('Android Fire Tizen and webOS retain bounded recovery and operator escape behavior', () => {
  assert.match(android, /registerDefaultNetworkCallback/);
  assert.match(android, /MAX_AUTOMATIC_RELOADS/);
  assert.match(android, /createConfirmDeviceCredentialIntent/);
  assert.match(kiosk, /isLockTaskPermitted/);
  assert.match(tizenLauncher, /keyCode === 10009/);
  assert.match(tizenLauncher, /window\.location\.replace/);
  assert.match(webosLauncher, /keyCode === 461/);
  assert.match(webosLauncher, /webOSRelaunch/);
  assert.match(webosLauncher, /visibilitychange/);
});

test('GitHub Actions reproduces package validation and explicitly skips integration tests', () => {
  assert.match(workflow, /assembleGoogleTvDebug/);
  assert.match(workflow, /assembleFireTvDebug/);
  assert.match(workflow, /node src\/tv\/tizen\/scripts\/validate\.mjs/);
  assert.match(workflow, /node src\/tv\/webos\/scripts\/validate\.mjs/);
  assert.match(workflow, /--filter "Category=Unit"/);
  assert.match(workflow, /Integration Tests Skipped/);
});

test('package sources exclude embedded signing credentials and generated package configuration', () => {
  const packageSources = [
    android,
    kiosk,
    androidBuild,
    tizenLauncher,
    tizenConfig,
    webosLauncher,
    webosConfig
  ].join('\n');
  assert.doesNotMatch(
    packageSources,
    /storePassword|keyPassword|private\.key|certificate-profile|developerMode|BEGIN PRIVATE KEY/i
  );
});

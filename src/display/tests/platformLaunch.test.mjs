import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  readPlatformBootstrap,
  resolvePlatformLaunch,
  supportedTvPlatforms
} from '../src/platformLaunch.mjs';

const app = await readFile(new URL('../src/App.tsx', import.meta.url), 'utf8');

test('preserves the browser route and web version fallback', () => {
  assert.deepEqual(resolvePlatformLaunch('/display/screen-1'), {
    platform: 'browser', appVersion: 'web', pathname: '/display/screen-1'
  });
});

test('starts TV platforms at pairing and resumes a persisted display target', () => {
  assert.deepEqual(resolvePlatformLaunch('/', { platform: 'fire_tv', appVersion: ' 1.2.3 ' }), {
    platform: 'fire_tv', appVersion: '1.2.3', pathname: '/pair'
  });
  assert.deepEqual(resolvePlatformLaunch('/pair', {
    platform: 'android_tv', appVersion: '2.0', screenId: 'screen/one'
  }), {
    platform: 'android_tv', appVersion: '2.0', pathname: '/display/screen%2Fone'
  });
});

test('lets an explicit pairing route override durable TV launch state', () => {
  assert.deepEqual(resolvePlatformLaunch('/pair', {
    platform: 'android_tv', appVersion: '2.0', screenId: 'screen-one'
  }, true), {
    platform: 'android_tv', appVersion: '2.0', pathname: '/pair'
  });
});

test('supports only the approved TV identifiers and feeds pairing registration', () => {
  assert.deepEqual(supportedTvPlatforms, ['android_tv', 'fire_tv', 'tizen', 'webos']);
  assert.equal(resolvePlatformLaunch('/', { platform: 'unknown' }).platform, 'browser');
  assert.match(app, /window\.__VENNU_PLATFORM__/);
  assert.match(app, /platform=\{launch\.platform\}/);
  assert.match(app, /appVersion=\{launch\.appVersion\}/);
});

test('accepts only approved sanitized hosted-wrapper query metadata', () => {
  assert.deepEqual(readPlatformBootstrap('?vennuPlatform=tizen&vennuVersion=%202.4.0%20'), {
    platform: 'tizen', appVersion: '2.4.0'
  });
  assert.equal(readPlatformBootstrap('?vennuPlatform=browser'), undefined);
  assert.equal(readPlatformBootstrap('?vennuPlatform=unknown&vennuVersion=bad'), undefined);
});

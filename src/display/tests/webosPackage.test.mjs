import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const root = new URL('../../tv/webos/', import.meta.url);
const [manifestText, launcher, config, ignore, workflow, icon, largeIcon] = await Promise.all([
  readFile(new URL('appinfo.json', root), 'utf8'),
  readFile(new URL('launcher.js', root), 'utf8'),
  readFile(new URL('launcher.config.js', root), 'utf8'),
  readFile(new URL('.gitignore', root), 'utf8'),
  readFile(new URL('../../../.github/workflows/phase02-tests.yml', import.meta.url), 'utf8'),
  readFile(new URL('icon.png', root)),
  readFile(new URL('large-icon.png', root))
]);
const manifest = JSON.parse(manifestText);

function pngDimensions(image) {
  assert.equal(image.subarray(1, 4).toString('ascii'), 'PNG');
  return [image.readUInt32BE(16), image.readUInt32BE(20)];
}

test('declares a bounded LG webOS web package', () => {
  assert.equal(manifest.id, 'com.vennu.tv.webos');
  assert.equal(manifest.type, 'web');
  assert.equal(manifest.main, 'index.html');
  assert.equal(manifest.disableBackHistoryAPI, true);
  assert.equal(manifest.resolution, '1920x1080');
  assert.deepEqual(pngDimensions(icon), [80, 80]);
  assert.deepEqual(pngDimensions(largeIcon), [130, 130]);
});

test('launches shared pairing with approved metadata and lifecycle behavior', () => {
  assert.match(config, /https:\/\/display\.vennu\.app/);
  assert.match(launcher, /origin\.protocol !== 'https:'?/);
  assert.match(launcher, /vennuPlatform', 'webos'/);
  assert.match(launcher, /vennuVersion/);
  assert.match(launcher, /keyCode === 461/);
  assert.match(launcher, /webOSRelaunch/);
  assert.match(launcher, /visibilitychange/);
  assert.match(launcher, /window\.location\.replace/);
});

test('validates statically without committing signing or package output', () => {
  assert.match(workflow, /node src\/tv\/webos\/scripts\/validate\.mjs/);
  assert.match(ignore, /\*\.ipk/);
  assert.match(ignore, /\.ares/);
  assert.doesNotMatch(`${manifestText}\n${launcher}\n${config}`, /password|private.key|certificate-profile|developerMode/i);
});

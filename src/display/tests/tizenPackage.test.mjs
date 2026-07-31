import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const root = new URL('../../tv/tizen/', import.meta.url);
const [manifest, launcher, config, ignore, workflow] = await Promise.all([
  readFile(new URL('config.xml', root), 'utf8'),
  readFile(new URL('launcher.js', root), 'utf8'),
  readFile(new URL('launcher.config.js', root), 'utf8'),
  readFile(new URL('.gitignore', root), 'utf8'),
  readFile(new URL('../../../.github/workflows/phase02-tests.yml', import.meta.url), 'utf8')
]);

test('declares a Samsung TV web package with bounded privileges', () => {
  assert.match(manifest, /tizen:profile name="tv-samsung"/);
  assert.match(manifest, /tv\.inputdevice/);
  assert.match(manifest, /application\.launch/);
  assert.match(manifest, /access origin="https:\/\/display\.vennu\.app"/);
});

test('launches shared pairing with approved platform metadata and remote exit', () => {
  assert.match(config, /https:\/\/display\.vennu\.app/);
  assert.match(launcher, /origin\.protocol !== 'https:'?/);
  assert.match(launcher, /vennuPlatform', 'tizen'/);
  assert.match(launcher, /vennuVersion/);
  assert.match(launcher, /keyCode === 10009/);
  assert.match(launcher, /window\.location\.replace/);
});

test('validates statically without committing signing or package output', () => {
  assert.match(workflow, /node src\/tv\/tizen\/scripts\/validate\.mjs/);
  assert.match(ignore, /\*\.wgt/);
  assert.match(ignore, /profiles\.xml/);
  assert.doesNotMatch(`${manifest}\n${launcher}\n${config}`, /certificate-profile|password|private.key/i);
});

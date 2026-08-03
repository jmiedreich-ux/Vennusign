import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const root = new URL('../', import.meta.url);
const [manifest, html, config, launcher, ignore] = await Promise.all([
  readFile(new URL('config.xml', root), 'utf8'),
  readFile(new URL('index.html', root), 'utf8'),
  readFile(new URL('launcher.config.js', root), 'utf8'),
  readFile(new URL('launcher.js', root), 'utf8'),
  readFile(new URL('.gitignore', root), 'utf8')
]);

assert.match(manifest, /tizen:profile name="tv-samsung"/);
assert.match(manifest, /package="VennuTvPkg"/);
assert.match(manifest, /https:\/\/display\.vennu\.app/);
assert.match(manifest, /tv\.inputdevice/);
assert.match(manifest, /<name>Vennusign TV<\/name>/);
assert.match(html, /launcher\.config\.js/);
assert.match(html, /launcher\.js/);
assert.match(html, /<title>Vennusign TV<\/title>/);
assert.match(html, /Starting Vennusign TV/);
assert.match(config, /https:\/\/display\.vennu\.app/);
assert.match(launcher, /origin\.protocol !== 'https:'?/);
assert.match(launcher, /vennuPlatform', 'tizen'/);
assert.match(launcher, /vennuVersion/);
assert.match(launcher, /keyCode === 10009/);
assert.match(launcher, /Vennusign player origin must use HTTPS/);
assert.match(ignore, /\*\.wgt/);
assert.doesNotMatch(`${manifest}\n${config}\n${launcher}`, /certificate-profile|password|private.key/i);

console.log('Tizen package static validation passed.');

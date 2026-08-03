import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const root = new URL('../', import.meta.url);
const [manifestText, html, config, launcher, ignore, icon, largeIcon] = await Promise.all([
  readFile(new URL('appinfo.json', root), 'utf8'),
  readFile(new URL('index.html', root), 'utf8'),
  readFile(new URL('launcher.config.js', root), 'utf8'),
  readFile(new URL('launcher.js', root), 'utf8'),
  readFile(new URL('.gitignore', root), 'utf8'),
  readFile(new URL('icon.png', root)),
  readFile(new URL('large-icon.png', root))
]);
const manifest = JSON.parse(manifestText);

function pngDimensions(image) {
  assert.equal(image.subarray(1, 4).toString('ascii'), 'PNG');
  return [image.readUInt32BE(16), image.readUInt32BE(20)];
}

assert.equal(manifest.id, 'com.vennu.tv.webos');
assert.equal(manifest.type, 'web');
assert.equal(manifest.main, 'index.html');
assert.equal(manifest.vendor, 'Vennusign');
assert.equal(manifest.title, 'Vennusign TV');
assert.equal(manifest.disableBackHistoryAPI, true);
assert.equal(manifest.resolution, '1920x1080');
assert.equal(manifest.icon, 'icon.png');
assert.equal(manifest.largeIcon, 'large-icon.png');
assert.match(manifest.version, /^\d+\.\d+\.\d+$/);
assert.deepEqual(pngDimensions(icon), [80, 80]);
assert.deepEqual(pngDimensions(largeIcon), [130, 130]);
assert.match(html, /launcher\.config\.js/);
assert.match(html, /launcher\.js/);
assert.match(html, /<title>Vennusign TV<\/title>/);
assert.match(html, /Starting Vennusign TV/);
assert.match(config, /https:\/\/display\.vennu\.app/);
assert.match(launcher, /origin\.protocol !== 'https:'?/);
assert.match(launcher, /vennuPlatform', 'webos'/);
assert.match(launcher, /vennuVersion/);
assert.match(launcher, /keyCode === 461/);
assert.match(launcher, /Vennusign player origin must use HTTPS/);
assert.match(launcher, /webOSRelaunch/);
assert.match(launcher, /visibilitychange/);
assert.match(ignore, /\*\.ipk/);
assert.doesNotMatch(`${manifestText}\n${config}\n${launcher}`, /password|private.key|certificate-profile|developerMode/i);

console.log('LG webOS package static validation passed.');

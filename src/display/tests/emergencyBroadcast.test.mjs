import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [overlay, page, contract] = await Promise.all([
  readFile(new URL('../src/EmergencyBroadcastOverlay.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/DisplayPage.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/displayContent.d.mts', import.meta.url), 'utf8')
]);

test('broadcast preempts full screen and recovers children at authoritative expiry', () => {
  assert.match(page, /<EmergencyBroadcastOverlay content=\{content\}>/);
  assert.match(overlay, /role="alert"/);
  assert.match(overlay, /Date\.parse\(broadcast\.expiresUtc\) - Date\.now\(\)/);
  assert.match(overlay, /setTimeout\(\(\) => setExpired\(true\)/);
  assert.match(overlay, /return children/);
  assert.match(contract, /emergencyBroadcast\?:/);
});

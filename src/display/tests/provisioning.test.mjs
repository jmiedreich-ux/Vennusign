import assert from 'node:assert/strict';
import test from 'node:test';
import { claimPreRegisteredScreen } from '../src/provisioning.mjs';

test('exchanges the one-time delivery token without pairing', async () => {
  let request;
  const result = await claimPreRegisteredScreen(
    'https://api.example.com/',
    'delivery-token',
    'android_tv',
    '4.1.0',
    async (input, init) => {
      request = { input, init };
      return new Response(JSON.stringify({
        screenId: 'screen-1',
        screenKey: 'sc-abc123',
        venueId: 'venue-1',
        displayPath: '/display/screen-1'
      }), { status: 200, headers: { 'Content-Type': 'application/json' } });
    }
  );

  assert.equal(request.input, 'https://api.example.com/api/screens/pre-registration/claim');
  assert.deepEqual(JSON.parse(request.init.body), {
    token: 'delivery-token',
    platform: 'android_tv',
    appVersion: '4.1.0'
  });
  assert.equal(result.displayPath, '/display/screen-1');
});

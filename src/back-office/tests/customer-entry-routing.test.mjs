import test from 'node:test';
import assert from 'node:assert/strict';
import { authenticatedCustomerDestination, safeLocalReturnPath } from '../src/customerEntryRouting.mjs';

const snapshot = complete => ({ progress: { goLive: complete } });

test('incomplete customers always resume the canonical Back Office onboarding route', () => {
  assert.equal(authenticatedCustomerDestination('/signin', '/', snapshot(false)), '/onboarding');
  assert.equal(authenticatedCustomerDestination('/signup', '/screens', snapshot(false)), '/onboarding');
});

test('completed customers enter a validated Back Office destination', () => {
  assert.equal(authenticatedCustomerDestination('/signin', '/screens', snapshot(true)), '/screens');
  assert.equal(authenticatedCustomerDestination('/signin', '/onboarding', snapshot(true)), '/');
  assert.equal(authenticatedCustomerDestination('/onboarding', '/onboarding', snapshot(true)), undefined);
  assert.equal(authenticatedCustomerDestination('/onboarding', '/screens', snapshot(true)), '/screens');
});

test('external and protocol-relative returns cannot escape the local application', () => {
  assert.equal(safeLocalReturnPath('https://attacker.example', '/onboarding'), '/onboarding');
  assert.equal(safeLocalReturnPath('//attacker.example', '/onboarding'), '/onboarding');
  assert.equal(safeLocalReturnPath('/\\attacker.example', '/onboarding'), '/onboarding');
});

test('a customer who has gone live is not sent back to onboarding when the display is offline', () => {
  // progress.goLive is the achievement, latched on the first Online heartbeat and never
  // cleared. Displays that are powered down overnight report paired-offline the next morning;
  // that must not read as unfinished onboarding.
  const wentLiveNowOffline = { firstScreenStatus: 'paired-offline', goLiveAchievedUtc: '2026-08-19T02:00:00Z', progress: { goLive: true } };
  assert.equal(authenticatedCustomerDestination('/signin', '/', wentLiveNowOffline), '/');
  assert.equal(authenticatedCustomerDestination('/signin', '/screens', wentLiveNowOffline), '/screens');

  const neverLive = { firstScreenStatus: 'paired-offline', progress: { goLive: false } };
  assert.equal(authenticatedCustomerDestination('/signin', '/screens', neverLive), '/onboarding');
});

test('a customer already on the onboarding page is never redirected to it again', () => {
  // The regression this exists for: returning the current path makes the caller
  // replace() the URL it is already on, so the page reloads, resolves the same
  // answer, and replaces again - forever. Every unfinished step must be covered,
  // because every one of them lands the visitor on /onboarding.
  for (const step of ['account', 'plan', 'venue', 'first-screen', 'go-live']) {
    const incomplete = { currentStep: step, progress: { goLive: false } };
    assert.equal(
      authenticatedCustomerDestination('/onboarding', '/onboarding', incomplete), undefined,
      `step ${step} must not redirect to the page it is already on`);
    assert.equal(
      authenticatedCustomerDestination('/onboarding', '/screens', incomplete), undefined,
      `step ${step} must stay put even when a return path was requested`);
  }

  // Arriving from anywhere else still routes to onboarding, which is the point of it.
  assert.equal(authenticatedCustomerDestination('/signin', '/screens', { progress: { goLive: false } }), '/onboarding');
});


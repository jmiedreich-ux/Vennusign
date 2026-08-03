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

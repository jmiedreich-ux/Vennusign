import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const read = path => readFile(new URL(path, import.meta.url), 'utf8');
const [dashboard, api, adminController, publicController, service, migration] = await Promise.all([
  read('../src/OperationalDashboard.tsx'),
  read('../src/api.ts'),
  read('../../Vennu.Api/Controllers/PlatformOperations/PlatformOperationsScreensController.cs'),
  read('../../Vennu.Api/Controllers/ScreensController.cs'),
  read('../../Vennu.Api/Services/HaasPreRegistrationService.cs'),
  read('../../Vennu.Data/Scripts/032_add_screen_pre_registration.sql')
]);

test('fleet health remains explicit across the API contract and operational dashboard', () => {
  assert.match(api, /outdatedScreens/);
  assert.match(api, /versionStatus: "current" \| "outdated" \| "unknown"/);
  assert.match(dashboard, /Screens outdated/);
  assert.match(dashboard, /Update \$\{screen\.appVersion/);
});

test('HaaS creation stays protected venue scoped and stores only one-time token hashes', () => {
  assert.match(adminController, /Authorize\(Policy = PlatformOperationsAuthenticationDefaults\.AuthorizationPolicy\)/);
  assert.match(adminController, /Route\("api\/admin\/venues\/\{venueId:guid\}\/screens"\)/);
  assert.match(adminController, /HttpPost\("pre-registrations"\)/);
  assert.match(service, /RandomNumberGenerator\.GetBytes\(32\)/);
  assert.match(service, /PreRegistrationTokenHash = Hash\(token\)/);
  assert.doesNotMatch(migration, /BootstrapToken/);
  assert.match(migration, /PreRegistrationTokenHash CHAR\(64\)/);
});

test('public provisioning is one-time platform bound and returns only a display route', () => {
  assert.match(publicController, /HttpPost\("pre-registration\/claim"\)/);
  assert.match(publicController, /StatusCodes\.Status401Unauthorized/);
  assert.match(service, /screen\.PreRegistrationExpiresUtc <= now/);
  assert.match(service, /string\.Equals\(screen\.Platform, platform/);
  assert.match(service, /ClaimPreRegisteredAsync/);
  assert.match(service, /\$"\/display\/\{screen\.Id\}"/);
});

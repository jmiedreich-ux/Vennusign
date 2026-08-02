import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const read = path => readFile(new URL(path, import.meta.url), "utf8");
const [app, onboardingApp, onboardingApi, api, handler, legacy, options, program] = await Promise.all([
  read("../src/App.tsx"), read("../src/CustomerOnboardingApp.tsx"),
  read("../src/customerOnboardingApi.ts"), read("../src/api.ts"),
  read("../../Vennu.Api/VenueAdmin/CustomerVenueAdminAuthenticationHandler.cs"),
  read("../../Vennu.Api/VenueAdmin/VenueAdminAuthenticationHandler.cs"),
  read("../../Vennu.Api/VenueAdmin/VenueAdminAuthenticationOptions.cs"),
  read("../../Vennu.Api/Program.cs")
]);

test("persisted customer sessions are the primary membership-checked venue path", () => {
  assert.match(program, /CustomerAuthenticationScheme/);
  assert.match(handler, /CustomerAuthenticationDefaults\.SessionCookieName/);
  assert.match(handler, /GetOrganizationMembershipAsync/);
  assert.match(handler, /GetVenueMembershipAsync/);
  assert.match(handler, /MembershipCapability\.ManageVenueContent/);
  assert.match(handler, /GetFeatureSetAsync/);
  assert.match(handler, /AuthenticationSourceClaim, "customer-session"/);
  assert.match(api, /credentials: "include"/);
  assert.match(api, /headers\.delete\("X-Vennu-Venue-Token"\)/);
  assert.match(app, /customerSessionAccess/);
  assert.match(app, /Sign in with your customer account/);
  assert.match(onboardingApp, /!requested\.startsWith\("\/\/"\)/);
  assert.match(onboardingApp, /window\.location\.replace\(returnPath\)/);
  assert.match(onboardingApi, /body: JSON\.stringify\(\{ email, returnPath \}\)/);
  assert.match(onboardingApi, /encodeURIComponent\(returnPath\)/);
});

test("legacy compatibility is explicitly bounded and secondary", () => {
  assert.match(options, /LegacySessionsEnabled/);
  assert.match(options, /LegacySessionsRetireAfterUtc/);
  assert.match(options, /Enabled/);
  assert.match(options, /ExpiresUtc/);
  assert.match(options, /RevokedUtc/);
  assert.match(legacy, /FixedTimeEquals/);
  assert.match(legacy, /Legacy venue access has been retired/);
  assert.match(app, /temporary legacy venue link/);
  assert.match(app, /Legacy venue access token/);
});

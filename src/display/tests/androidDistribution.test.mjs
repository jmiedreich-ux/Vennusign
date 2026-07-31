import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const androidRoot = new URL('../../tv/android/', import.meta.url);
const appGradle = await readFile(new URL('app/build.gradle.kts', androidRoot), 'utf8');
const gitignore = await readFile(new URL('.gitignore', androidRoot), 'utf8');
const workflow = await readFile(new URL('../../../.github/workflows/phase02-tests.yml', import.meta.url), 'utf8');
const google = JSON.parse(await readFile(new URL('distribution/google-tv.json', androidRoot), 'utf8'));
const fire = JSON.parse(await readFile(new URL('distribution/amazon-fire-tv.json', androidRoot), 'utf8'));

test('defines distinct Google TV and Fire TV distribution identities', () => {
  assert.match(appGradle, /create\("googleTv"\)/);
  assert.match(appGradle, /applicationIdSuffix = "\.googletv"/);
  assert.match(appGradle, /quotedBuildConfig\("android_tv"\)/);
  assert.match(appGradle, /create\("fireTv"\)/);
  assert.match(appGradle, /applicationIdSuffix = "\.firetv"/);
  assert.match(appGradle, /quotedBuildConfig\("fire_tv"\)/);
  assert.equal(google.applicationId, 'com.vennu.tv.googletv');
  assert.equal(fire.applicationId, 'com.vennu.tv.firetv');
});

test('centralizes version and hosted-player build inputs', () => {
  assert.match(appGradle, /VENNU_VERSION_CODE/);
  assert.match(appGradle, /VENNU_VERSION_NAME/);
  assert.match(appGradle, /VENNU_BASE_URL/);
  assert.equal(google.versionSource, fire.versionSource);
});

test('assembles both unsigned profiles in GitHub Actions', () => {
  assert.match(workflow, /gradle-version: "8\.9"/);
  assert.match(workflow, /assembleGoogleTvDebug/);
  assert.match(workflow, /assembleFireTvDebug/);
  assert.match(workflow, /--no-daemon --stacktrace/);
});

test('excludes release artifacts and signing material', () => {
  assert.match(gitignore, /\*\.apk/);
  assert.match(gitignore, /\*\.aab/);
  assert.match(gitignore, /\*\.jks/);
  assert.match(gitignore, /\*\.keystore/);
  assert.doesNotMatch(appGradle, /storePassword|keyPassword|signingConfig/);
});

import assert from 'node:assert/strict';
import fs from 'node:fs';
import test from 'node:test';
import { materializeTemplate, validateManifest } from '../scripts/releaseManifest.mjs';

const template = fs.readFileSync(new URL('../../../docs/operations/release/release-manifest.template.json', import.meta.url), 'utf8');
const sha = 'a'.repeat(40);

test('validates a materialized canonical manifest', () => {
  const manifest = materializeTemplate(template, sha, 'build-100');
  assert.deepEqual(validateManifest(manifest), []);
});

test('rejects missing and incompatible declarations with actionable errors', () => {
  const manifest = materializeTemplate(template, sha, 'build-100');
  manifest.components = manifest.components.filter(component => component.id !== 'api');
  manifest.components.find(component => component.id === 'tizen-shell').compatibility = '>=2.0 <1.0';
  const errors = validateManifest(manifest);
  assert.ok(errors.some(error => error.includes("Missing required component 'api'")));
  assert.ok(errors.some(error => error.includes('tizen-shell: compatibility')));
});

test('rejects a syntactically valid shell range that excludes the native bridge', () => {
  const manifest = materializeTemplate(template, sha, 'build-100');
  manifest.components.find(component => component.id === 'tizen-shell').compatibility = '>=2.0 <3.0';
  assert.ok(validateManifest(manifest).some(error => error.includes('does not include native-bridge 1.0.0')));
});

test('requires changed versions to advance and carried artifacts to remain identical', () => {
  const previous = materializeTemplate(template, sha, 'build-100');
  const current = structuredClone(previous);
  current.productVersion = '0.1.1';
  current.components.forEach(component => { component.state = 'carried-forward'; });
  current.components.find(component => component.id === 'api').state = 'changed';
  current.components.find(component => component.id === 'hosted-player').artifact.buildId = 'rebuilt-without-change';
  const errors = validateManifest(current, previous);
  assert.ok(errors.some(error => error.includes('api: changed components must increase version')));
  assert.ok(errors.some(error => error.includes('hosted-player: carried-forward components')));
});

test('requires TV build numbers to increase for changed shells', () => {
  const previous = materializeTemplate(template, sha, 'build-100');
  const current = structuredClone(previous);
  current.productVersion = '0.2.0';
  current.components.forEach(component => { component.state = 'carried-forward'; });
  const shell = current.components.find(component => component.id === 'webos-shell');
  shell.state = 'changed';
  shell.version = '0.2.0';
  assert.ok(validateManifest(current, previous).some(error => error.includes('webos-shell: buildNumber must increase')));
});

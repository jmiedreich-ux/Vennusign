import fs from 'node:fs';

export const requiredComponents = [
  'back-office', 'platform-operations', 'api', 'heartbeat-monitor', 'scheduled-content',
  'hosted-player', 'android-fire-shell', 'tizen-shell', 'webos-shell', 'native-bridge',
  'database', 'infrastructure', 'configuration-schema'
];

const semver = /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$/;
const contract = /^[1-9]\d*\.\d+$/;
const schemaVersion = /^\d{8}\.\d{2}$/;
const commit = /^[0-9a-f]{40}$/;
const range = /^>=(\d+)\.(\d+) <(\d+)\.(\d+)$/;

export function validateManifest(manifest, previous = null) {
  const errors = [];
  if (manifest?.schemaVersion !== 1) errors.push('schemaVersion must be 1.');
  if (!semver.test(manifest?.productVersion ?? '')) errors.push('productVersion must be semantic versioning.');
  if (!contract.test(manifest?.configurationSchemaVersion ?? '')) errors.push('configurationSchemaVersion must be major.minor.');
  if (!schemaVersion.test(manifest?.database?.schemaVersion ?? '')) errors.push('database.schemaVersion must be YYYYMMDD.NN.');
  if (manifest?.database?.compatibility !== 'expand-and-contract') errors.push('database.compatibility must be expand-and-contract.');

  const components = Array.isArray(manifest?.components) ? manifest.components : [];
  const ids = new Set();
  for (const component of components) {
    if (ids.has(component.id)) errors.push(`Duplicate component '${component.id}'.`);
    ids.add(component.id);
    if (!semver.test(component.version ?? '')) errors.push(`${component.id}: version must be semantic versioning.`);
    if (!['changed', 'carried-forward'].includes(component.state)) errors.push(`${component.id}: state must be changed or carried-forward.`);
    if (!commit.test(component.artifact?.sourceCommit ?? '')) errors.push(`${component.id}: artifact.sourceCommit must be a 40-character lowercase commit SHA.`);
    if (!component.artifact?.buildId) errors.push(`${component.id}: artifact.buildId is required.`);
    if (component.artifact?.immutable !== true) errors.push(`${component.id}: artifact must be immutable.`);
    if (component.kind === 'tv-shell' && (!Number.isInteger(component.buildNumber) || component.buildNumber < 1)) errors.push(`${component.id}: a positive buildNumber is required.`);
    if (['tv-shell', 'player'].includes(component.kind) && !validRange(component.compatibility)) errors.push(`${component.id}: compatibility must be a non-empty >=major.minor <major.minor range.`);
    if (component.kind === 'api' && (!Number.isInteger(component.contractMajor) || component.contractMajor < 1)) errors.push(`${component.id}: contractMajor is required.`);
  }
  for (const id of requiredComponents) if (!ids.has(id)) errors.push(`Missing required component '${id}'.`);

  for (const procedure of manifest?.database?.procedureContracts ?? []) {
    if (!/^[A-Za-z][A-Za-z0-9_]*$/.test(procedure.name ?? '') || !contract.test(procedure.version ?? '')) errors.push('Stored-procedure contracts require a valid name and major.minor version.');
  }

  if (previous) validateProgression(manifest, previous, errors);
  return errors;
}

function validRange(value) {
  const match = range.exec(value ?? '');
  if (!match) return false;
  return Number(match[1]) < Number(match[3]) || Number(match[1]) === Number(match[3]) && Number(match[2]) < Number(match[4]);
}

function validateProgression(current, previous, errors) {
  if (compareSemver(current.productVersion, previous.productVersion) <= 0) errors.push('productVersion must increase.');
  if ((current.database?.schemaVersion ?? '') < (previous.database?.schemaVersion ?? '')) errors.push('database.schemaVersion cannot decrease.');
  const old = new Map(previous.components.map(component => [component.id, component]));
  for (const component of current.components) {
    const prior = old.get(component.id);
    if (!prior) continue;
    const versionComparison = compareSemver(component.version, prior.version);
    if (component.state === 'changed' && versionComparison <= 0) errors.push(`${component.id}: changed components must increase version.`);
    if (component.state === 'carried-forward' && (versionComparison !== 0 || JSON.stringify(component.artifact) !== JSON.stringify(prior.artifact))) errors.push(`${component.id}: carried-forward components must retain version and artifact identity.`);
    if (component.kind === 'tv-shell' && component.state === 'changed' && component.buildNumber <= prior.buildNumber) errors.push(`${component.id}: buildNumber must increase.`);
  }
}

function compareSemver(left, right) {
  const a = left.split(/[+-]/, 1)[0].split('.').map(Number);
  const b = right.split(/[+-]/, 1)[0].split('.').map(Number);
  for (let index = 0; index < 3; index += 1) if (a[index] !== b[index]) return a[index] - b[index];
  return 0;
}

export function materializeTemplate(templateText, sourceCommit, buildId) {
  if (!commit.test(sourceCommit)) throw new Error('SOURCE_COMMIT must be a 40-character lowercase commit SHA.');
  if (!buildId?.trim()) throw new Error('BUILD_ID is required.');
  return JSON.parse(templateText.replaceAll('${SOURCE_COMMIT}', sourceCommit).replaceAll('${BUILD_ID}', buildId));
}

if (process.argv[1] && process.argv[1].endsWith('releaseManifest.mjs')) {
  const templatePath = process.argv[2] ?? 'docs/operations/release/release-manifest.template.json';
  const manifest = materializeTemplate(fs.readFileSync(templatePath, 'utf8'), process.env.SOURCE_COMMIT, process.env.BUILD_ID);
  const previous = process.env.PREVIOUS_MANIFEST ? JSON.parse(fs.readFileSync(process.env.PREVIOUS_MANIFEST, 'utf8')) : null;
  const errors = validateManifest(manifest, previous);
  if (errors.length) { console.error(errors.join('\n')); process.exit(1); }
  process.stdout.write(`${JSON.stringify(manifest, null, 2)}\n`);
}

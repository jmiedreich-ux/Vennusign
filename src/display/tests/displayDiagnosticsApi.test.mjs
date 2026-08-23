import assert from 'node:assert/strict';
import test from 'node:test';
import { buildDisplayDiagnosticsUrl, loadServerDiagnostics } from '../src/displayDiagnosticsApi.mjs';

test('builds the diagnostics URL against the configured API origin', () => {
  assert.equal(
    buildDisplayDiagnosticsUrl('https://api.example.com/', 'screen/1'),
    'https://api.example.com/api/display/screen%2F1/diagnostics'
  );
});

test('returns the server payload on success', async () => {
  const fetchImpl = async () => new Response(JSON.stringify({ screenId: 's1', status: 'Online' }), {
    status: 200,
    headers: { 'Content-Type': 'application/json' }
  });
  const result = await loadServerDiagnostics('https://api.example.com', 's1', fetchImpl);
  assert.equal(result.kind, 'ok');
  assert.equal(result.diagnostics.status, 'Online');
});

test('reports not-found without throwing', async () => {
  const fetchImpl = async () => new Response('', { status: 404 });
  const result = await loadServerDiagnostics('https://api.example.com', 's1', fetchImpl);
  assert.equal(result.kind, 'not-found');
});

test('reports a network failure as an error result, not a thrown exception', async () => {
  const fetchImpl = async () => { throw new Error('offline'); };
  const result = await loadServerDiagnostics('https://api.example.com', 's1', fetchImpl);
  assert.equal(result.kind, 'error');
});

test('reports a non-2xx, non-404 response as an error result', async () => {
  const fetchImpl = async () => new Response('', { status: 500 });
  const result = await loadServerDiagnostics('https://api.example.com', 's1', fetchImpl);
  assert.equal(result.kind, 'error');
});

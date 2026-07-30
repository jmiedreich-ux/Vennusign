import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const page = await readFile(new URL('../src/DisplayPage.tsx', import.meta.url), 'utf8');
const frame = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const photo = await readFile(new URL('../src/layouts/photoGrid.css', import.meta.url), 'utf8');
const diner = await readFile(new URL('../src/layouts/classicDiner.css', import.meta.url), 'utf8');

test('player applies persisted and live theme values through shared variables', () => {
  for (const key of ['backgroundColor', 'accentColor', 'fontFamily']) {
    assert.match(frame, new RegExp(`theme\\.${key}`));
  }
  assert.match(frame, /contrastColor/);
  assert.match(frame, /--vennu-foreground/);
  assert.match(frame, /--vennu-accent-foreground/);
  for (const css of [photo, diner]) {
    assert.match(css, /--vennu-background/);
    assert.match(css, /--vennu-accent/);
    assert.match(css, /--vennu-font-family/);
  }
});

test('player-backed preview applies draft theme without heartbeat or realtime side effects', () => {
  assert.match(page, /preview\.get\('preview'\) === 'theme'/);
  assert.match(page, /previewTheme \? \{ \.\.\.content, theme: previewTheme \} : content/);
  assert.match(page, /if \(previewTheme\)/);
});

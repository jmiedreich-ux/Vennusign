import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const vite = readFileSync(new URL("../vite.config.ts", import.meta.url), "utf8");
const packageJson = JSON.parse(readFileSync(new URL("../package.json", import.meta.url), "utf8"));

test("venue admin local development uses HTTPS for secure customer sessions", () => {
  assert.match(vite, /basicSsl\(\)/);
  assert.match(vite, /https:\s*true/);
  assert.equal(typeof packageJson.devDependencies["@vitejs/plugin-basic-ssl"], "string");
});

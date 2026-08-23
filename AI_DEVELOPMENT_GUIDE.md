# Vennusign AI Development Guide

## Purpose

`AGENTS.md` is the authoritative process policy. Read this guide only when implementation needs concise architecture or coding context; it does not repeat governance rules.

## Architecture Map

- `Vennu.Api`: HTTP contracts, controllers, authentication composition, SignalR, and hosted services.
- `Vennu.Core.Models`: shared domain models.
- `Vennu.Data`: Vennusign repositories, persistence behavior, and DbUp migrations.
- `Vennu.DataAccess`: generic reusable provider infrastructure.
- `src/platform-operations`: internal Platform Operations application.
- `src/back-office`: customer and venue operations application.
- `src/display`: hosted player SPA.
- `src/tv`: platform wrappers and distribution packages.

## Implementation Approach

1. Read the claimed issue/package and inspect existing behavior and contracts.
2. Define the smallest vertical slice and its affected areas.
3. Preserve tenancy, authorization, entitlement, provider authority, and migration compatibility.
4. Add focused non-integration tests beside the behavior.
5. Update only task-relevant architecture/operations records.
6. Let impact-based Actions provide authoritative validation.

## Engineering Rules

- Prefer established services and repositories over parallel abstractions.
- Keep customer, organization, venue, and screen ownership server-derived.
- Keep provider callbacks/webhooks authoritative where the existing design requires them.
- Use DbUp for schema changes and preserve migration ordering.
- Keep local secrets in supported environment/configuration providers, never repository files.
- Do not replace working code solely for style consistency.

## Local Toolchain

Verified 2026-08-22 by running each command. **Check before claiming something is missing** — `command -v <tool>` takes a second, and a tool absent from the Linux `PATH` is usually the Windows install under `/mnt/c`, not a missing tool. Reporting a CLI as unavailable and designing around it has cost real sessions.

- **Node** — `node` / `npm` / `npx` are v22.23.2 / npm 10.9.8 on the Linux side, symlinked from `~/.local/bin` into `~/.local/share/node-v22.23.2-linux-x64/`. Windows Node at `/mnt/c/Program Files/nodejs/node.exe` is a fallback only; its `npx.cmd` fails from bash. Environment variables reach a Windows process only through `WSLENV`.
- **Invoking Node tooling** — call the JS entry point, not a wrapper, from the directory whose `node_modules` you need: `node node_modules/@playwright/test/cli.js test …`, `node node_modules/typescript/bin/tsc --noEmit -p tsconfig.json`, `node node_modules/vite/bin/vite.js build`.
- **Playwright** — 1.62.1, run from `tests/ui`. Chromium is a real Linux binary at `~/.cache/ms-playwright/chromium-*/chrome-linux64/chrome`, so a change to a screen can be rendered and screenshotted rather than reasoned about. The signed-in specs share one QA account and one paired screen and must run `--workers=1`.
- **.NET** — `/usr/bin/dotnet` has no SDK. Use `/mnt/c/Program Files/dotnet/dotnet.exe` (SDK 9.0.313); it builds `Vennu.Api` and runs the full suite.
- **SQL** — `/mnt/c/Program Files/Microsoft SQL Server/Client SDK/ODBC/170/Tools/Binn/SQLCMD.EXE` (v15) reaches `(localdb)\MSSQLLocalDB` and, with the `sql-dev-*` credentials in Key Vault `kv-vennusign-dev`, the dev database directly. LocalDB is the default target and needs no credential; see `AGENTS.md` for the rule that a test never carries one.
- **Also present** — `az`, `gh`, `git`, `python3`, `rg`, `curl`, `wget`, `tar`, `tmux`, `kubectl`, `docker`.
- **Genuinely absent** — `jq`, `unzip`, `zip`, `make`, `gcc`, `java`, `go`, `rustc`, `ruby`, `php`, `yarn`, `pnpm`, `terraform`, `fd`, `fzf`. Use `gh --jq` or `python3` for JSON, and `python3 -c "import zipfile; …"` for archives.

## Task-Scoped Reading

Use `docs/README.md` to locate current architecture, operations, component, or research material. Do not read archived roadmaps, completed packages, validation records, or handoffs unless the task explicitly requires historical research.

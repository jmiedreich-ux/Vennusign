# Built Foundations Specification

**Purpose:** the authoritative inventory of what already exists and is proven, written as prep for resuming work. Anything Track 2 plans on top of this list does not need to be re-decided or re-built; anything *not* on this list must not be assumed.

**Proof boundary (per retrospective finding 9):** "Built" below means merged to `master` and validated by exact-head Actions runs, with the last green suites reporting 56 UI (Playwright), 108 Back Office and 102 Platform Operations tests. Work that exists but is unmerged or unverified is listed separately in §8 and is *not* foundation. Integration, Azure SQL, live-provider, hosted-infrastructure, credentialed, physical-device and cross-system behavior remain intentionally unproven.

---

## 1. Capability, entitlement and authority model (Track 1's core deliverable)

- **53 canonical action capabilities** in `src/Vennu.Core.Models/CapabilityModel.cs`, across 11 domains (screen 7, publishing 7, content 7, account 5, schedule 4, organization 4, branding 4, analytics 4, workflow 3, support 3, localization 3), each typed **Core / Advanced / Governance** with a domain and operation kind (Read / Change / Administration).
- **Server-authoritative decisions.** Every session resolves each capability to one of four decisions: `allowed`, `denied` (permission), `unavailable` (not in plan), `temporarily-blocked` (rollout). These are the product's three-kinds-of-"no" plus yes, and the distinction is a contract, not styling.
- **Structured decision payload** (Back Office session endpoint `GET /api/back-office/session` → `capabilityDecisions[]`): `capabilityId`, `decision`, `reasonCode` (e.g. `decision.allowed`, `allowance.reached`, `permission.required`), `category`, `messageKey`, resolved `message`, structured `parameters` (e.g. `used`/`limit`), `correlationId`, resolved `locale`, `resolution` hint (e.g. `remove_or_increase_allowance`, `sign_in_again`), `retryAfterSeconds`, `conditions`, `isAllowed`.
- **Enforcement primitive:** `[RequireCapability("...")]` (`src/Vennu.Api/BackOffice/RequireCapabilityAttribute.cs`) — an action filter that authorizes per-request and on refusal returns **403 with the same structured decision shape** (capabilityId, decision, reasonCode, localized message, resolution, correlationId). 13 of the 15 Back Office controllers carry it at class or action level.
- **Policy resolution** (`CapabilityAccessPolicyRepository`): one query joins capability definitions with organization **entitlements** (time-bounded, revocable), **add-on attachments**, **allowances** (org- or venue-scoped `LimitValue` with usage; `screen.device.pair` usage is computed live from non-archived screens), and **rollouts** (time-windowed, venue/org/global precedence). Known gap: allowance usage for other capabilities is a stored counter (`CapabilityAllowanceUsage`), not derived.
- **Roles with genuinely different authority**, proven by fixture: Organization Owner (full), Content Editor (edits content, no publishing/screens), Publisher (publishes/recovers, no content editing). Role boundaries render as locked navigation + refusal panels *before* any request is issued — the client never discovers a "no" by receiving a 403 it could have predicted.
- **Localization of decisions:** messages resolve through a fallback chain (e.g. `fr-CA → fr → en-US`) via `Accept-Language`; reason codes and parameters never change with locale.

## 2. Screens and delivery (truth-first contract)

- Screen lifecycle: create, **6-digit pairing codes** (expiring, single-use), pre-registration, **player replacement** (preserves logical screen, content, history, wall position), archive/restore, unpair, reset.
- **Delivery evidence model:** authoritative revision vs applied revision per screen; states `requested / received / applied / recovered / superseded / offline / failed`; **"applied" is only ever claimed when the applied revision equals the authoritative one.** Offline screens catch up on reconnect — re-pushing is unnecessary by design.
- Heartbeats drive status (`Online`/`Offline`/stale detection); display preview supports `?preview=observer` so looking at a screen never heartbeats it Online.
- Capacity: pairing allowance blocks **only** add/pair; every action on existing screens remains available at the limit (contract asserted by tests).
- Video wall grouping/coordination exists behind `screen.wall.coordinate` (Advanced).

## 3. Content, scheduling and integrations

- **Menus:** menus → sections → items (name, description, price, availability, dietary info, translations). Save-race protection: per-item draft revision tracking prevents a slow save from overwriting newer edits.
- **Schedules workspace:** meal periods, happy hour (currently the live example of `temporarily-blocked`), playlists, date-range promotions, emergency broadcasts (override everything). Server resolves all scheduling in the venue's timezone; the browser is never scheduling authority.
- **POS integrations:** Clover, Square and Toast controllers exist behind `content.source.synchronize` (Advanced; the live example of `unavailable`/not-in-plan). Webhook worker is resilient: a failed cycle logs and retries instead of terminating the host; webhook event claiming uses explicit `READ COMMITTED` to avoid isolation-level leaks on pooled connections.
- **Surfaces in the solution:** `back-office` (37 components, React), `display` (player web app), `tv`, `admin`, `venue-admin`, `platform-operations`, over `Vennu.Api` / `Vennu.Data` / `Vennu.DataAccess` (RepoDb; FluentMapper mappings are complete for all 20+ entities after the Track 1 fixes).

## 4. Test and validation infrastructure

| Layer | What exists |
|---|---|
| UI (deterministic) | `tests/ui` — Playwright, 11 spec files / 56 tests at last green run, desktop + mobile (Pixel 7) projects, covering the 14 mechanical acceptance cases: sign-in, navigation/entitlements, role boundaries, screens, capacity, offline push, menu save-race, isolated menu items, keyboard/focus, mobile navigation, workbook sign-in |
| UI (subjective) | `scripts/run-track1-qa.ps1` — 5 hosted-agent lanes (copy quality, localization judgment, responsive, shell quality) at ~$1.70/run, with per-lane fault isolation and cancel-on-exit |
| API/back office | `tests/Vennu.Api.Tests` (108 Back Office node/contract tests reported), `Vennu.DataAccess.Tests`, `Vennu.Data.IntegrationTests` (intentionally skipped in CI) |
| Platform ops | 102 tests reported at last green run |
| CI | `.github/workflows/ui-regression.yml` gates every commit (windows-latest, LocalDB start+poll, npm/Chromium caching); docs-only PRs skip Phase 02 (#668) |

- **Test-data isolation:** the separately deployed `Vennu.TestApi` owns `POST /api/test/seed`, runs locally/staging but not production, and authenticates with a generated environment secret. It has no product/data/domain dependency and delegates every state change to authorized product API endpoints over HTTP. `start-ui-test-env.ps1 -PruneSeed` removes seeded rows and Playwright global setup runs it automatically.
- **Isolation tags:** the fixture machinery can stamp parallel datasets (`-0000-0000-<tag>-` GUIDs, tagged tokens/emails/screen keys) so multiple environments share one database without sharing rows.
- **Two runnable environments, not interchangeable:** `start-ui-test-env.ps1` is local-only and binds product API 7138, Test API 7140, Back Office 5174 and display 5175; `run-track1-qa.ps1` publishes cloudflared tunnels with tunnel-origin CORS for hosted agents.

## 5. Acceptance and evidence machinery

- **Deterministic fixture:** Harbor Acceptance Venue with Owner/Editor/Publisher tokens (`track1-owner-review` etc.), Acceptance Menu → Featured → Harbor Lemonade 4.50 and Acceptance Screen `sc-t1demo` (Offline). A separate `track1-capacity-check` venue carries the consumed 1-of-1 allowance so ordinary Playwright seeding can exercise real screen-create endpoints in parallel. Idempotent; reapplied on every environment start.
- **Owner acceptance workbook** (`docs/acceptance/track-1-owner-acceptance.html`): self-contained HTML, 20 cases in 8 journeys, one-click fragment-token sign-in, localStorage persistence, JSON export/import with a versioned schema, closure decision gated on complete + noted results. (v3 with per-case sign-in and per-step checkboxes exists but is stranded — §8.)
- **Result record:** `vennusign.track1.owner-acceptance` JSON schema; the 2026-08-06 owner record (17 Pass / 1 NA / 2 Fail, closure "Needs adjustment") is the current acceptance baseline.

## 6. Design and product-definition assets (input material, not yet authority)

- Two independent mock sets exist for the Home/Screens/Menu-builder redesign: `docs/design/track2-mocks/` (three structural directions: instrument bench, venue floor map, wayfinding bands — on PR #663) and `docs/design/concepts/` (sky-instrument-bench-independent, live-proofing-table — merged via the concept commits). By owner rule, agents extending the independent sets must not inspect `track2-mocks`.
- `PRODUCT.md` (owner-interviewed): sky identity binding; owner-in-interrupted-bursts primary user; truth-first positioning; five product principles. **Currently only on PR #663, not master.**
- Five agreed UX usability themes (role identity, generic refusals, indistinguishable "no"s on controls, vocabulary split, scattered disclosure) recorded in the handoff and retrospective.
- **Nothing under `docs/design/approved/` yet** — no direction has owner approval, so per the new design gate there is no implementation authority.

## 7. Process and governance foundations

- `AGENTS.md` gates: session locks, controlled living records, ChatGPT review gate, exact-head validation, and (new, #666) the **Track Acceptance → Retrospective → Closure gate**: a track closes only when owner acceptance is acceptable *and* an owner-approved retrospective exists.
- `RWP-01.06` retrospective report is **written and pending owner decision** (branch `agent/track-1-retrospective-report`), with 12 root-caused findings and a required Track 2 adoption matrix (design-before-implementation, functional vertical slices, tests-with-implementation, independent review, Release–Capability–Tier Matrix, honest proof boundaries). That branch also carries templates for the front-end design workflow package and the release/tier matrix.
- Track 1 execution record: RWPs 01.01–01.05 merged via PRs #645–#650, acceptance automation via #654, each exact-head validated.

## 8. Exists but is NOT foundation yet (do not build on top without resolving)

| Item | State |
|---|---|
| PR #663 (`chore/release-track1-desktop-lock`, commit `70ca509`) | **Open and CONFLICTING with master** (its lock-release commits collide with merged #664). Carries the acceptance-defect fixes — `RequireCapability` on push/push-all/reset/unpair, UI gating of Push/Reset/Unpair from decisions, isolation-tag display-name fix, tightened capacity spec, publisher-boundary + direct-403 specs — plus workbook v3, `PRODUCT.md` and the `track2-mocks`. All **unverified** (builds pass; suite never run, by owner directive). |
| `screen.content.target` enforcement | On master this capability is still declared, denied to Publisher, and **enforced nowhere**; push/reset/unpair are gated only by `screen.device.view`. The fix is in #663. |
| Owner acceptance results 2-1 / 3-0 / 3-1 | Must each be linked to a resolved change or explicit accepted disposition before closure. |
| Issues #656–#662 (`before-track-2`) | Seed-prune safety, focused server tests, generated-output policy, independent review of #654, case 5-0 to Playwright, hosted-agent cost policy, duplicate test hooks — all open; the retrospective requires them resolved before Track 2 implementation. |
| Retrospective report | Owner decision **Pending**; branch unmerged. |
| Design direction | No owner-approved direction; `docs/design/approved/` does not exist. |
| Release–Capability–Tier Matrix | Template exists on the retrospective branch; the matrix itself is not drafted or approved. |
| `docs/Google/` OAuth client secret, Browser Use API key | Still need rotation by the owner. |

## 9. Sequencing already directed by the owner

1. **All future tracks are cancelled (2026-08-07) ahead of a full planning reset** — no Track 2 plan exists; RWP-13.06 and the former `before-track-2` issues (#656–#662) are closed as not planned; queued work packages are void as plans.
2. Screen/UX changes first — no test runs until they land (2026-08-06); this and the remaining Track 1 items (three unresolved acceptance results, retrospective decision #665, PR #663 disposition) are the only sanctioned work pending the reset.
3. This spec, the retrospective report, and the proposed product-surface inventory are inputs to the planning reset.

# Track 1 Owner Acceptance Review

## Review state

- Implementation status: Track 1.01 through Track 1.05 complete, merged and exact-head validated.
- Closure status: **owner acceptance pending**. Track 1 does not close automatically when this package merges.
- Future work: implementation of every future track remains blocked until the owner approves Track 1 closure. Light planning may remain provisional, but cannot be marked complete until Track 1 feedback is evaluated.

## What the owner is reviewing

The owner reviews customer-visible clarity, useful recovery, navigation, messages, permission boundaries and product intent. Automated checks cover schema, decision permutations, endpoint enforcement, scope inheritance, audit behavior, catalogs and implementation contracts that do not benefit from manual repetition.

Use a dedicated local Development database only. The repository does not contain or invent production credentials, and no hosted acceptance deployment is asserted.

## Local preparation

Prerequisites:

- Windows with SQL Server LocalDB and `sqlcmd`;
- .NET 9 SDK;
- Node.js 22 or later;
- trusted local ASP.NET/Vite development certificates.

From the repository root:

1. Run `powershell -ExecutionPolicy Bypass -File scripts/start-track1-acceptance.ps1`.
2. Wait for the API to report that it is listening on `https://localhost:7138`.
3. In a separate PowerShell window, run:

   `sqlcmd -S "(localdb)\MSSQLLocalDB" -d VennuSign -E -b -i docs\acceptance\track-1-owner-fixture.sql`

4. Open [Back Office home](https://localhost:5174/#/home), accept the local certificate warning if shown, expand **Use configured venue access**, and enter one of the tokens below.
5. Keep the API, Back Office and Display windows open for the review.

The SQL fixture is idempotent for its named records and restores the fixture menu item, one-screen allowance, Offline screen state and temporary-block scenario. It does not delete unrelated local data. Rerun it whenever the named baseline needs to be restored.

## Prepared local accounts and scenario

| Account | Configured token | Expected authority |
|---|---|---|
| Track 1 Owner Review | `track1-owner-review` | Organization Owner; all customer permissions, subject to capability access, state and allowance |
| Track 1 Content Editor | `track1-content-editor` | Content editing, theme editing and preview; no publish authority |
| Track 1 Publisher | `track1-publisher` | Preview, publish, confirm, replace, unpublish and delivery recovery; no content editing |

Fixture identifiers:

| Item | Value |
|---|---|
| Organization | `72000000-0000-0000-0000-000000000001` — Track 1 Acceptance Organization |
| Venue | `73000000-0000-0000-0000-000000000001` — Harbor Acceptance Venue |
| Screen | `74000000-0000-0000-0000-000000000001` — Acceptance Screen |
| Initial player state | Offline, one active screen, pair allowance `1 of 1` |
| Menu baseline | Acceptance Menu → Featured → Harbor Lemonade, `$4.50` |

These are deterministic local acceptance fixtures, not production accounts.

## Review journeys

### 1. Essential operating loop

1. Sign in with `track1-owner-review`.
2. Open [Menu](https://localhost:5174/#/menu).
3. Find **Harbor Lemonade**, change its description or price, save, reload, and confirm the saved value remains.
4. Open [Screens](https://localhost:5174/#/screens).
5. Select **Acceptance Screen**, choose **Preview selected screen**, and confirm the exact display preview opens without changing the screen.
6. Choose **Push structured content** while the screen is Offline.
7. Confirm the UI says the latest revision will recover after reconnect rather than claiming the TV already applied it.
8. In PowerShell, reconnect the fixture player:

   `Invoke-RestMethod -Method Post -Uri 'https://localhost:7138/api/display/74000000-0000-0000-0000-000000000001/heartbeat' -ContentType 'application/json' -Body '{"status":"Online"}'`

9. Within the 10-second UI poll, confirm the screen becomes Online. Push again and confirm requested/received/applied status remains distinct rather than reporting false success.
10. Use **More actions → Reset connection**, approve the review dialog, and confirm the screen returns to an Offline/reconnect recovery state.

Expected result: edit, preview, selected-target push, offline queueing, reconnection and reset recovery are usable; destructive actions require deliberate confirmation; no client claims device application without authoritative evidence.

### 2. Content Editor versus Publisher

1. Sign out and enter `track1-content-editor`.
2. Confirm [Menu](https://localhost:5174/#/menu) opens and the fixture item can be edited.
3. Confirm publishing/screen delivery controls are unavailable rather than silently failing.
4. Sign out and enter `track1-publisher`.
5. Confirm [Menu](https://localhost:5174/#/menu) is locked with a permission explanation.
6. Confirm [Screens](https://localhost:5174/#/screens) opens and preview/publish/recovery controls are available.

Expected result: editing and publishing are separate authorities. Locked navigation carries a reason, does not become a dead action, and never grants access from a route name.

### 3. Screen capacity and core recovery

1. Sign in with `track1-owner-review` and open [Screens](https://localhost:5174/#/screens).
2. Expand **Setup**.
3. Confirm the page reports `1 of 1 active screens` and explains that the allowance has been reached.
4. Confirm **Add screen** and **Pair screen** are disabled from the structured server `screen.device.pair` decision.
5. Confirm Preview, Push, Reset, Archive, Restore, Replace and Unpair recovery actions for the existing screen are not disabled by the exhausted pair allowance.

Expected result: capacity blocks only the quantity-increasing pair/create action. Existing core correction and recovery remain usable.

### 4. Advanced and temporarily blocked capabilities

1. With the Owner account, open [POS integrations](https://localhost:5174/#/pos).
2. Confirm it is locked as unavailable because current access does not include `content.source.synchronize`; Menu and Screens remain usable.
3. Open [Schedules](https://localhost:5174/#/schedules) and select **Happy hour**.
4. Confirm the seeded `schedule.promotion.automate` rollout is presented as temporarily unavailable with retry guidance, while basic schedule access remains available.

Expected result: advanced access does not disable essential core, and unavailable versus temporarily blocked are visibly different states.

### 5. Translation and fallback

Run both PowerShell requests and inspect `capabilityDecisions`:

1. `Invoke-RestMethod -Uri 'https://localhost:7138/api/back-office/session' -Headers @{'X-Vennusign-Back-Office-Token'='track1-owner-review';'Accept-Language'='fr-CA'}`
2. Repeat with `Accept-Language` set to `en-US`.
3. In the `fr-CA` response, confirm allowed decisions use **Cette action est disponible.**
4. Confirm messages absent from `fr-CA` fall back through `fr` and then `en-US`; for example, the POS entitlement explanation falls back to **Your current access does not include this action.**

Expected result: stable reason codes and structured parameters do not change with locale; only the resolved product message changes, with deterministic fallback.

### 6. Navigation and overall product judgment

1. At desktop width and a narrow mobile width, visit [Home](https://localhost:5174/#/home), [Menu](https://localhost:5174/#/menu), [Schedules](https://localhost:5174/#/schedules), [Screens](https://localhost:5174/#/screens), [Themes](https://localhost:5174/#/themes), [POS](https://localhost:5174/#/pos), [Billing](https://localhost:5174/#/billing) and [Security](https://localhost:5174/#/security).
2. Use keyboard navigation for links, grouped navigation, tabs, dialogs and forms.
3. Confirm loading, empty, denied, unavailable, temporarily blocked, offline, error and recovery states have readable text and no color-only meaning.
4. Record whether the experience is clear, useful, recoverable and aligned with the approved Version 1 product intent.

Expected result: every visible action is wired or truthfully unavailable; keyboard focus remains visible; customer copy uses capability outcomes rather than implementation or migration language.

## Result record

Record one of **Pass**, **Fail**, or **Needs Adjustment** for each row.

| Journey | Result | Notes / evidence |
|---|---|---|
| Essential operating loop |  |  |
| Content Editor versus Publisher |  |  |
| Screen capacity and recovery |  |  |
| Advanced and temporarily blocked capabilities |  |  |
| Offline, reconnect and delivery status |  |  |
| Translation and fallback |  |  |
| Navigation, responsive and keyboard behavior |  |  |
| Overall product intent |  |  |

Any failed or adjustment item is evaluated as a Track 1 change. A clear bounded correction may be applied directly; larger work becomes one or more additional Track 1 RWPs grouped into a later scheduled chunk of up to five.

## Automated evidence

| RWP | Issue | PR | Merged commit | Exact-head Actions |
|---|---|---|---|---|
| 1.01 | [#640](https://github.com/jmiedreich-ux/Vennusign/issues/640) | [#645](https://github.com/jmiedreich-ux/Vennusign/pull/645) | `a729f4dd75468c1f69570d53f44b81dcd86a4945` | [31044305223](https://github.com/jmiedreich-ux/Vennusign/actions/runs/31044305223) |
| 1.02 | [#641](https://github.com/jmiedreich-ux/Vennusign/issues/641) | [#646](https://github.com/jmiedreich-ux/Vennusign/pull/646) | `06e12569b4f4ecb196a3dbf49a4a924798626376` | [31044938623](https://github.com/jmiedreich-ux/Vennusign/actions/runs/31044938623) |
| 1.03 | [#642](https://github.com/jmiedreich-ux/Vennusign/issues/642) | [#647](https://github.com/jmiedreich-ux/Vennusign/pull/647) | `cd12f25a58bed509c2082f94b3bafe8974228cdc` | [31045701930](https://github.com/jmiedreich-ux/Vennusign/actions/runs/31045701930) |
| 1.04 | [#643](https://github.com/jmiedreich-ux/Vennusign/issues/643) | [#648](https://github.com/jmiedreich-ux/Vennusign/pull/648) | `58dcf33a62d391275cfb985301aa5e9544c91262` | [31047859910](https://github.com/jmiedreich-ux/Vennusign/actions/runs/31047859910) |
| 1.05 | [#644](https://github.com/jmiedreich-ux/Vennusign/issues/644) | [#650](https://github.com/jmiedreich-ux/Vennusign/pull/650) | `6915ef2b402ce146d8ff01bf7ad767e3cbb1295e` | [31049451685](https://github.com/jmiedreich-ux/Vennusign/actions/runs/31049451685) |

Track 1.05 full affected-area validation passed through GitHub Actions. Integration, Azure SQL, live-provider, hosted-infrastructure, credentialed, device and cross-system tests remain intentionally skipped.

## Bounded gaps corrected in Track 1.05

- Screen create/pair controls no longer use billing-tier `maxScreens` as browser authority. They use the structured server `screen.device.pair` decision and fail closed when it is absent.
- The Back Office session projection now preserves message keys, structured parameters, correlation IDs, resolved locales and conditions, allowing the UI to explain `used` and `limit` without recreating the rule.
- Customer-facing configured-access and placeholder copy no longer promises a migration or legacy-preservation path.

No additional Track 1 RWP is required by the automated validation result. Owner feedback may still add Track 1 RWPs before closure.

## Explicitly deferred interfaces and boundaries

- Role, role-assignment and support-grant management UI: later authority-administration work.
- Tier, entitlement, add-on, allowance and rollout management UI: later commercial/platform-administration work.
- Locale preference and translation-catalog management UI: later localization administration.
- Full signup, guided setup, starter content, first-publish guidance, trials and interrupted-flow recovery: Track 8 onboarding.
- Pricing, checkout changes, billing-provider behavior and real external providers: outside Track 1.
- Physical device, hosted deployment and provider integration proof: not claimed by this local/non-integration review.

The existing generic commercial catalog may still describe billing offers, but it is not an action-authority source. Physical removal or renaming of presentation-only commercial records is not required for Track 1 correctness and must not be treated as permission or entitlement authority.

## Closure decision

After completing the result record, the owner chooses one outcome:

- **Approve Track 1 closure** — Track 1 closes; provisional future-track planning may be finalized after Track 1 feedback is evaluated.
- **Needs adjustment** — record the adjustment on issue [#644](https://github.com/jmiedreich-ux/Vennusign/issues/644); bounded fixes are corrected and larger items become additional Track 1 RWPs for a later scheduled chunk.

Future-track implementation never begins automatically from this package.

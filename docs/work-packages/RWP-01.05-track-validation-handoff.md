# RWP-01.05 — Track 1 Validation and Owner Handoff

## Result

Track 1.01 through Track 1.04 now form one typed capability, decision, permission, allowance, state and recovery foundation. RWP-01.05 validates that combined foundation and prepares the owner acceptance gate. Track 1 is not closed by this package; closure still requires owner approval after acceptance testing.

## Combined completeness review

| Area | Evidence and disposition |
| --- | --- |
| Capability identity | `CapabilityId`, `Version1CapabilityRegistry` and reconciliation tests require canonical `domain.resource.action` IDs and keep routes, tiers, providers, states and labels separate. |
| Server decisions | The decision engine covers allowed, allowed-with-conditions, denied, unavailable and temporarily-blocked results with stable reasons, message keys, parameters, correlation and recovery guidance. |
| Authority and scope | Protected roles, scoped assignments and downward-only inheritance separate actor permission from product availability. Content Editor and Publisher authority remain distinct. |
| Persistence | DbUp migrations 052 through 054 establish typed role/permission, commercial capability, allowance, add-on, rollout and audit persistence. Exact-head Actions must validate migration inventory and affected data projects. |
| Essential core | Content editing, availability, preview, screen pairing, publishing and recovery paths use canonical capability decisions while screen quantity remains an allowance. Core recovery is not sold as an upgrade. |
| Customer UI | Back Office navigation and affected controls consume structured server decisions. Locked, denied, unavailable, temporarily blocked, loading, error and recovery states retain explanations and reachable actions. |
| Player | The player continues to load authoritative content, apply structured realtime changes, distinguish online/recovering/degraded/offline states and recover from cached content. |
| Support and audit | Support context requires the Support Operator role plus an explicit bounded grant and writes entry or denial evidence. |
| Old authority removal | Generic feature resolution, browser entitlement catalogs and mixed route/feature authority are no longer the governing decision path. Historical names may remain only in archives or non-authoritative compatibility records. |

## Automated validation

- Back Office Node tests: 105 passed locally.
- Display/player Node tests: 136 passed locally.
- Exact-head GitHub Actions must pass the affected Release builds, focused .NET tests, migration inventory checks and documentation validation before merge.
- Azure SQL, live providers, hosted infrastructure, devices and all integration/external-system tests remain skipped under the standing owner direction.

## Owner acceptance setup

The repository supplies deterministic roles and scenarios, but it does not contain deployable owner passwords or a shared hosted acceptance environment. Do not place credentials in this record. Before owner testing, provision or reset these identities in the selected test environment using its normal secure identity workflow:

| Scenario identity | Required role/state |
| --- | --- |
| Free Owner | Organization Owner; Free commercial access; one-screen allowance; paired online screen; editable menu. |
| Content Editor | Content Editor at the test venue; no Publisher role. |
| Publisher | Publisher at the same venue; no broader organization administration. |
| Capacity Owner | Organization Owner; one-screen allowance already consumed. |
| Advanced-Locked Owner | Organization Owner; core allowed; one representative advanced capability unavailable. |
| Offline Owner | Organization Owner; paired screen with saved content, then player disconnected. |
| French-Canadian Owner | Organization Owner; `fr-CA` locale with one `fr-CA`, one `fr`, and one `en-US` fallback message scenario. |

Local development routes are Back Office `https://localhost:5174`, player pairing `https://localhost:5175/pair`, and API `https://localhost:7138`. In a hosted test environment, use the equivalent environment URLs. Reset by restoring the deterministic scenario fixture or recreating the development database; never reuse a production customer.

## Numbered owner tests

Record each result as **Pass**, **Fail**, or **Needs Adjustment**, with a screenshot and the displayed correlation ID when a server decision is involved.

1. **Free operating loop** — Sign in as Free Owner, open Menu, change an item, preview it, pair the permitted screen, publish, confirm player application, replace the published revision, unpublish, republish and recover from one deliberate failed save. Expect every core step to remain reachable, explicit and recoverable without an upgrade requirement.
2. **Editor versus Publisher** — As Content Editor, edit and save content, then attempt publish. Expect editing to succeed and publishing to be denied with a useful explanation. Repeat as Publisher; expect publish authority at the assigned venue without organization-administration access.
3. **Screen capacity** — As Capacity Owner, start pairing another screen. Expect a clear allowance-reached explanation that distinguishes quantity from permission and preserves management/recovery of the existing screen.
4. **Advanced capability** — As Advanced-Locked Owner, open the representative advanced surface. Expect the advanced action to explain why it is unavailable while Menu, Preview, Screen and core recovery remain usable.
5. **Offline publishing and recovery** — As Offline Owner, disconnect the player, publish a new revision, confirm the UI reports receipt versus application accurately, reconnect the player and verify automatic authoritative reload, online status recovery and applied-revision confirmation.
6. **Localized decisions** — As French-Canadian Owner, trigger the three prepared decision messages. Expect deterministic `fr-CA` to `fr` to `en-US` fallback without exposing a raw message key as customer copy.
7. **Navigation and actions** — Traverse Home, Menu, Quick Update, Screens, Scheduling, Themes, POS and Account. Expect implemented destinations only, visible actions with correct enabled/blocked explanations, keyboard focus, narrow-layout usability, no dead buttons and no browser-only authority decision.
8. **Overall product judgment** — Record whether capability explanations, core/advanced separation, recovery guidance and customer wording match the approved product intent. Any bounded defect returns to Track 1; any new product decision is recorded for owner review.

## Player online/offline controls

For local testing, stop the display dev server or disable the player network connection to enter offline/degraded state; restart or reconnect it to recover. Do not alter the API database to simulate online status. Confirm that cached content is labeled as cached/offline and that reconnection replaces it only with authoritative content.

## Deferred interfaces and remaining limits

- Full role, tier, allowance, add-on, rollout and locale management interfaces remain deferred to their approved future tracks.
- Full signup/onboarding, pricing, billing and live provider journeys are outside Track 1.
- Real device/store packages and external infrastructure are not validated in this RWP.
- A hosted acceptance environment and secure scenario-account provisioning are deployment prerequisites, not repository credentials. If those are unavailable when owner testing begins, record an additional Track 1 RWP for acceptance-environment provisioning and place it in the next scheduled chunk.

## Closure rule

If the owner passes all applicable journeys and explicitly approves closure, Track 1 may close. Failures or adjustments remain Track 1 work. Additional RWPs are ordered into a later scheduled chunk of up to five; a track is not limited to five RWPs. Light planning for a future track may overlap acceptance, but cannot be marked complete until effects from the current and earlier tracks have been evaluated.

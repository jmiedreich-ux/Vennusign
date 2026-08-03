# RWP-13.03 — Onboarding Ownership and Navigation Unification

## Result

Customer onboarding now has one owner and one canonical route: Back Office `/onboarding`. Authentication callbacks pass through that resolver, incomplete customers resume the persisted current task, and completed customers continue only to a validated local Back Office path. Platform Operations stays protected and read-only.

## UI and function gap analysis

| Concern | Gap | Completed behavior |
| --- | --- | --- |
| Goals and hierarchy | Signup, sign-in, onboarding, support, and Back Office entry did not clearly communicate their roles. | Signup/sign-in identify authentication intent; the checklist identifies Back Office onboarding; Platform Operations labels itself read-only support. |
| Navigation | Active sessions could follow a requested route before loading onboarding progress. | Authentication returns through `/onboarding`; server progress decides incomplete versus complete routing before a Back Office destination is used. |
| Customer actions | Completion and pairing had status refresh but no direct workspace transition. | Paired customers receive a primary Open Back Office action plus secondary status refresh; Back Office rechecks membership and saved venue. |
| Essential states | Missing/stale/removed access could look like a generic temporary signup failure. | Loading, signed-out, incomplete, completed, pending checkout, paired-offline, online, missing journey, changed access, error, retry, and support recovery have explicit safe behavior. |
| Validation | Local return validation was duplicated inline and provider callbacks could bypass the journey resolver. | One pure resolver rejects external/protocol-relative/backslash returns and always routes incomplete accounts to the canonical journey. |
| Destructive/support actions | Support ownership could imply customer-context entry. | Platform Operations remains GET-only, performs no impersonation or customer mutation, and explicitly states the boundary. No destructive onboarding action was added. |
| Accessibility | Intent, progress, asynchronous changes, and completion needed predictable semantics. | Ordered timeline/focus remain intact; status and alert roles remain; completion actions follow reading order; copy distinguishes authentication, onboarding, operations, and support. |
| Responsiveness | Completion controls needed a narrow-layout treatment. | Primary and secondary completion actions stack at the existing responsive breakpoint. |
| API/data/authorization/entitlement | Browser routes could not be allowed to select tenant or progress. | Persisted onboarding, membership, subscription/webhook, venue, and screen status remain server-authoritative; no new client-owned identifier or state was added. |

## Validation

- Back Office Node tests pass (60/60) and its production build passes locally.
- Platform Operations Node tests pass (86/86); its local production build is delegated because the local TypeScript compiler is unavailable.
- Exact-head affected-area GitHub Actions is authoritative for both frontends and repository records.
- Live providers/email, Stripe/webhooks, browser/device/authenticator, Azure SQL, hosted infrastructure, containers, signing/store, cross-system, and all other integration-type tests are skipped.

Completion evidence remains in the implementation PR. Issue: #421. Phase 14 and later remain paused.

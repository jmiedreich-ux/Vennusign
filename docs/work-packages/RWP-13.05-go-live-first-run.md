# RWP-13.05 — Go-Live and First-Run Experience

## Outcome

The first-screen journey now makes six-digit pairing easier to enter, celebrates go-live only after the authoritative player heartbeat reports Online, offers safe starter-menu draft entry points, and presents a clear first-run checklist for menu, theme, scheduling, and screen delivery work.

## Accepted Scope

- Improve six-digit pairing-code entry, progress, readiness, and recovery guidance.
- Distinguish paired-offline from authoritative Online and celebrate only the latter.
- Offer Restaurant, Cafe, Bar/Brewery, and blank starter-menu paths after go-live.
- Prefill a reviewed menu name without implicitly creating content or mutating venue state.
- Present first-run next steps linking to existing protected Back Office workflows.
- Preserve server-authoritative heartbeat, authentication, venue, entitlement, and pairing contracts.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals and hierarchy | New customers must finish pairing confidently, know whether the player is truly live, and understand what to do next. | Pairing leads with the code task and live digit progress; the go-live panel separates waiting from Online, then reveals starter and first-run guidance only after Online. |
| Navigation | Next steps must enter existing authorized workflows without creating a parallel setup system. | Starter and checklist links open the existing Menu, Themes, Schedules, and Screens routes; Back Office independently rechecks customer membership and venue context. |
| Required actions | Customers need to enter a code, retry status, open Back Office, choose a starter path, or begin blank. | The bounded pairing form, manual status refresh, membership-checked Back Office action after pairing, four starter choices, and four first-run links cover those actions. |
| Essential states and feedback | Empty/partial/ready code, pairing pending/error, paired-offline, Online, last-seen, and starter-selected states must be explicit. | Digit progress and submit readiness are textual; existing error/notice handling remains; server status drives waiting/Online copy and device facts; the menu editor announces that a starter is only a draft prefill. |
| Validation | Only six digits may be submitted and illustrative starters cannot become live content implicitly. | Input strips non-digits and bounds length to six; the submit control requires six digits; the established server claim remains authoritative; starter values use a three-value allowlist and require explicit Create menu submission. |
| Destructive actions | First-run assistance must not overwrite, activate, or remove content. | No destructive action is introduced. Starter selection performs no request or storage write and creates no menu, section, item, schedule, theme, or delivery. |
| Accessibility | Code status and go-live meaning cannot depend on color or motion; actions must be keyboard accessible. | Native form controls and links, described input, live text progress, explicit Online wording, semantic lists/headings, visible focus, and reduced-motion suppression support assistive and keyboard use. |
| Responsiveness | Pairing and next steps must work on phones and operational tablets. | The code field is fluid, starter/checklist grids collapse from four columns to two and then one, and existing completion actions stack on narrow screens. |
| API, data, authorization, and entitlements | Browser state cannot claim a screen is Online or create tenant content outside protected APIs. | Celebration and revealed guidance depend solely on `firstScreenStatus === "online"` from the onboarding snapshot; claim and menu mutations retain their existing authenticated, venue-authorized endpoints; no contract or schema changes are included. |

## Validation

- Back Office Node tests: passed.
- Back Office TypeScript and Vite production build: passed.
- Git whitespace and generated-artifact validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Hosted-browser visual regression, Azure SQL, live identity/Stripe/provider flows, credentialed pairing, hosted infrastructure, containers, physical displays, signing/store, cross-system, live-player heartbeat/delivery, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.

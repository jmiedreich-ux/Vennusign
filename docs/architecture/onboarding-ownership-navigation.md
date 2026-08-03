# Onboarding Ownership and Navigation

Back Office owns the single customer onboarding journey at `/onboarding`. `/signup` and `/signin` are authentication entry surfaces, not alternate onboarding state machines. Platform Operations exposes only a protected read projection and never renders customer forms or enters a customer workspace.

## Entry and exit contract

| Source | Canonical behavior |
| --- | --- |
| Public signup or sign-in | Google, Apple, and email-link callbacks return through `/onboarding`; passkey completion resolves there in the current app. |
| Back Office sign-in link | The desired local Back Office path is nested in the canonical onboarding return. External, protocol-relative, and backslash paths fail closed. |
| Incomplete customer | The server snapshot selects Account, Plan, Venue, First Screen, or Go Live. Any requested Back Office path is deferred and the browser resumes `/onboarding`. |
| Completed customer | Authentication continues to the validated local Back Office path, defaulting to `/`. A deliberate visit to `/onboarding` keeps the completed checklist visible. |
| Checkout return | `/onboarding` reloads persisted progress; the Stripe return parameter is explanatory only and verified webhook subscription state remains authoritative. |
| Pairing transition | Pairing and presence reload the same server snapshot. Once paired, the customer may open Back Office even while the player is offline; membership and venue selection are rechecked there. |
| Saved or stale link | Browser step parameters never choose progress. Missing, unauthorized, or changed access preserves existing data, creates no replacement journey, and offers refresh/sign-in/support recovery. |
| Platform Operations | Operators can search, refresh, and copy non-secret diagnostic context. There are no customer mutations, impersonation, or duplicate forms. Future context-affecting support actions require separate authorization and audit design. |

## Authority boundaries

The customer session identifies the user. The onboarding API derives the journey, organization, venue, subscription, and first screen from persisted authorities. Back Office separately checks active organization/venue membership before opening a workspace. No client route, saved link, provider return, or pairing event can supply another tenant or mark a step complete.

## UX guardrails

The five steps retain one consistent order and one current server-selected task. Authentication copy distinguishes signing in from starting, async changes use status messages, errors remain alerts, and the completion actions follow the reading/focus order. These choices follow [WCAG consistent navigation](https://www.w3.org/WAI/WCAG22/Understanding/consistent-navigation.html), [focus order](https://www.w3.org/WAI/WCAG22/Understanding/focus-order.html), and [status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html).

## Validation boundary

Focused route-resolution, return-path, source-contract, Back Office, and Platform Operations tests are non-integration validation. Live identity providers, email delivery, Stripe/webhooks, browser automation, authenticators, Azure SQL, hosted infrastructure, containers, physical devices, signing/store, cross-system, and all other integration-type tests remain skipped unless separately approved.

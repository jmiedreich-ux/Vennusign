# Vennusign customer authentication — decisions on record

Decisions settled with the owner on customer login and MFA. These supersede the
existing hand-built `CustomerAuthentication` system in `src/Vennu.Api` and
`src/Vennu.Data` (Google/Apple OIDC, Passkeys, email-link, TOTP step-up) as the
long-term design — see Migration note at the end for what stays and what is
replaced.

## Identity provider

**1 · Microsoft Entra External ID is the identity provider, not a hand-rolled system.**
Chosen over Auth0, Clerk, Stytch, and WorkOS because Vennusign is already fully
on Azure — same billing, same ecosystem, and the same federated-identity pattern
already used for GitHub Actions deploys extends naturally here. Free to 50,000
monthly active users, well beyond current scale.

**2 · Three sign-in options are offered: Google, Apple, and "Sign in with Vennusign."**
All three route through Entra External ID. "Sign in with Vennusign" is Entra's
local-account flow — any email address, not tied to Gmail or an Apple ID — with
Passkey as its passwordless method and email + password/OTP as fallback.

**3 · Entra is never surfaced as a brand.** No Microsoft or Entra logos, no
"powered by" text, no redirect through a visibly Microsoft-branded domain the
customer would notice. The experience reads as Vennusign's own login end to end.

**4 · The internal Vennu identity remains authoritative.** Whichever provider
verifies someone, it resolves through an external-identity link (provider +
subject → `CustomerUser.Id`) to Vennusign's own account record. Sessions, VR
routing, PO, and billing all key off that internal ID — never off which provider
was used to prove who someone is. This is the same shape `ExternalIdentity` /
`CustomerAccountService` already use for Google/Apple today; Entra slots into
that same seam rather than replacing it.

**5 · The backend still owns session state.** Entra verifies identity at login and
redirects back with a token, the same OIDC callback shape already implemented
for Google/Apple (`CustomerOidcEvents.cs`). Vennusign's own backend issues and
owns the session from that point forward — "is this customer logged in" stays a
query against Vennusign's own database, never a call out to Entra.

## Multi-factor authentication

**6 · MFA is mandatory in `app` (production), with one exemption: Passkey.** A
Passkey is already two factors in one — the device you have plus the biometric
or PIN you are/know — so it satisfies MFA on its own, no separate step. Every
other method (Google, Apple, Vennusign email/password/OTP) requires a TOTP
authenticator-app code before access is granted, every login, not only for
step-up on sensitive actions.

**7 · MFA policy is a configuration table entry, not hardcoded branching.** Whether
MFA is required is read from a per-environment configuration row — not an
`if/else` baked into the login code path — so the dev/stage exemption below is a
data change, not a code change, and the policy is auditable rather than buried
in logic.

**8 · Dev and stage exempt MFA and log in automatically.** Testers are not put
through the full provider + MFA ceremony to reach a disposable test environment.
This is the same dev-only-switch principle already recorded for thin-client
testing in `progressive-customer-cutover-concept.md`, applied here: a config
row, not a code fork, and it must be structurally incapable of reaching `app`.
Concretely, the config table's `app` row cannot express "MFA off" — the option
does not exist at that environment, not merely defaulted away.

**9 · TOTP, not email, is the second factor.** An authenticator-app code is
generated locally, refreshes every 30 seconds, and needs no inbox and no network
round trip — distinct from the magic-link method, which does. Confusing the two
was the source of "checking email every day" — TOTP does not have that cost.

## Where email link still fits

**10 · Email link is a fallback, never a default or an equal fourth button.**
It exists for account recovery and for people with no other option on a given
device. The login screen must never present it with the same visual weight as
the primary method.

**11 · The login screen remembers the last method used and defaults to it.** One
prominent button for a returning visitor's own prior choice; every other option
— including email link — sits behind a secondary "more ways to sign in"
disclosure. See the hi-fi mockup, `Login Hi-Fi.html`, for the exact shape of
this — Sign-in-remembered, Sign-in-first-visit, and the MFA step-up screen.

## Migration note

*(Not decided; recorded so it is not lost.)* The existing custom system in
`src/Vennu.Data/Services/Customer*.cs` and `src/Vennu.Api/CustomerAuthentication/`
— Google/Apple OIDC handling, `CustomerPasskeyService`, the email-link
request/redeem flow, and the TOTP step-up controller — was built before this
decision and is not automatically retired by it. Whether it is replaced wholesale
by Entra, kept as a fallback, or partially reused (the session/assurance model
in `CustomerSessionService` in particular, which decision 4 and 5 above assume
stays) is implementation scope, not decided here.

No implementation should begin from this document alone beyond what the owner
has explicitly authorized; work-package scoping and issue governance remain
necessary per `AGENTS.md`.

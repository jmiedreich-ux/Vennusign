# Phase 13 Customer Authentication Foundation

## Decision

WP-13.02 adds passwordless customer authentication without replacing the legacy Back Office token scheme. Customer identity remains product-owned; Google, Apple, and email links prove control and create a bounded opaque customer session.

## External provider boundary

- Google and Apple use ASP.NET Core OpenID Connect authorization-code handlers with HTTPS metadata, issuer/audience/lifetime/signature validation, state correlation, nonce validation, PKCE, and ten-minute remote timeouts.
- Provider configuration is environment-owned. A provider cannot be challenged unless explicitly enabled, and enabled providers require a client ID and client secret. Apple client-secret generation/rotation is an operational deployment responsibility; no key material is stored in the repository.
- OIDC tokens are validated and immediately discarded. `SaveTokens` is disabled, and access/refresh/ID tokens are not persisted in customer or session records.
- Provider subject is the durable external key. A new subject requires a verified provider email. Automatic linking to an existing email is allowed only when that existing customer email is already verified; unverified collisions fail closed.
- ASP.NET's protected correlation/nonce cookies and protected state carry the one-time callback boundary. Only bounded local return paths are accepted, preventing callback open redirects.

## Email-link boundary

- Email links are fallback/recovery for an existing active, verified customer account; they do not create a public signup account in this package.
- Requests return the same accepted response whether or not the email exists. Delivery failures are logged server-side without changing that response.
- The raw 256-bit token is sent only to `IEmailLoginDelivery`; only its SHA-256 hash is stored.
- Tokens expire after fifteen minutes by default and are atomically consumed once with an update lock and expiry predicate.
- Production delivery is an explicitly enabled HTTPS adapter configured outside source. Disabled environments do not print or return raw links.

## Session boundary

- Successful provider or email-link proof issues a new random 256-bit opaque session token. The raw value exists only in the secure `__Host-` cookie; the database stores a SHA-256 hash.
- Cookies are `Secure`, `HttpOnly`, host scoped, path `/`, and `SameSite=Lax`.
- Sessions have configurable absolute and idle lifetimes, bounded to at most 90 days, and an explicit revoke timestamp. Last-seen persistence is throttled by a touch interval.
- Authentication resolves the token hash, active session, idle/absolute lifetime, and persisted active customer user on every request. The principal contains identity/display/authentication-method facts only; organization roles and capabilities are not accepted from browser input.
- Passkeys, TOTP, recovery codes, and step-up/recent-authentication policy remain WP-13.03.

## Compatibility

- Platform Operations API-key and config-backed Back Office token schemes remain separate and unchanged.
- WP-13.09 owns the compatibility and retirement path for `BackOffice:Sessions`.

## WP-13.09 Back Office migration and retirement

Back Office authorization now prefers the persisted HttpOnly customer session. The server resolves an explicit venue-selection header only after loading that venue and verifying the authenticated user has an active organization or venue membership with content-management capability; otherwise it uses the user's saved onboarding venue. It then emits the existing venue claim and entitlement-derived UI capabilities. A caller cannot choose another organization's venue by supplying an identifier.

The Back Office access screen sends unauthenticated customers through the existing passwordless sign-in surface with a validated local return path. Email links, Google/Apple OIDC, and passkey completion preserve that return path; protocol-relative or non-local values fall back to `/onboarding`.

Config-backed `BackOffice:Sessions` remain a temporary parallel authentication scheme only. Operations can disable all legacy entries, set a global retirement instant, or disable/revoke/expire individual entries. Accepted tokens retain constant-time comparison and remain configuration-only; they are never persisted, logged, returned, or documented. After the compatibility window is confirmed unused, remove the legacy scheme, header, options, and temporary access disclosure in a separately reviewed change; customer-session authorization remains the durable path.
- Trial, Stripe entitlement, signup/onboarding state, and customer UI remain WP-13.04 through WP-13.08.

## UI and function gap analysis

Not applicable. WP-13.02 introduces API authentication flows and secure cookie transport but no page or screen. Public signup/sign-in and onboarding screens remain WP-13.05 and must receive their own UX review and complete UI/function analysis.

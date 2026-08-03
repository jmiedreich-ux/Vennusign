# Phase 13 Strong Authentication

## Trust boundaries

- The browser owns private passkey keys; Vennu stores credential identifiers, public keys, user handles, and monotonic signature counters only.
- WebAuthn creation/assertion options are protected at rest, expire after five minutes, are user/type bound, and are atomically consumed before verification.
- FIDO2 attestation and assertion verification is delegated to the maintained `Fido2` library with configured relying-party domain and exact allowed origins.
- TOTP secrets are generated with a cryptographic RNG and protected with ASP.NET Data Protection. The clear secret is returned only during authenticated enrollment.
- Recovery codes are cryptographically random, shown once, SHA-256 hashed at rest, and atomically single-use.

## Session assurance

Primary OIDC/email authentication records `Primary` assurance. Passkey sign-in issues `Strong` assurance. TOTP or recovery-code verification updates the current opaque session with a step-up timestamp. Factor enrollment requires an authenticated session whose initial authentication or most recent step-up is within ten minutes.

## Passkey management and local development

Back Office Account & Security lists only passkey name, identifier, creation date, and last-used date. Registration, rename, and removal use the authenticated customer session; mutations require recent authentication. Removing the last passkey is blocked unless the account retains verified email recovery. Credential IDs, public keys, user handles, counters, challenges, and authenticator responses never appear in the management projection or logs.

Local WebAuthn development uses Back Office `https://localhost:5174`, API `https://localhost:7138`, relying-party ID `localhost`, and exact origin `https://localhost:5174`. These values live only in `appsettings.Development.json`; accept the local HTTPS certificates before testing. Base/production configuration uses exact HTTPS `https://app.vennu.com` with RP ID `app.vennu.com`. Startup validation rejects wildcard, insecure, path-bearing, cross-domain, or localhost production settings.

Expected browser cancellation, timeout, unsupported-browser, missing-credential, expired challenge, and verification failures provide non-sensitive recovery guidance. Live browser/authenticator verification remains an integration/device test and is intentionally skipped under the standing owner exception.

Recovery-code step-up is deliberately auditable as `RecoveryCode`; it restores access but does not silently become a passkey or TOTP assertion. Password login, SMS recovery, and automatic email collision linking remain prohibited.

## Compatibility

Existing external identities, email links, opaque customer cookies, tenant memberships, and configuration-backed legacy Back Office sessions are unchanged. Public enrollment and factor-management screens are deferred to their approved Phase 13 UI packages.

# Phase 13 Strong Authentication

## Trust boundaries

- The browser owns private passkey keys; Vennu stores credential identifiers, public keys, user handles, and monotonic signature counters only.
- WebAuthn creation/assertion options are protected at rest, expire after five minutes, are user/type bound, and are atomically consumed before verification.
- FIDO2 attestation and assertion verification is delegated to the maintained `Fido2` library with configured relying-party domain and exact allowed origins.
- TOTP secrets are generated with a cryptographic RNG and protected with ASP.NET Data Protection. The clear secret is returned only during authenticated enrollment.
- Recovery codes are cryptographically random, shown once, SHA-256 hashed at rest, and atomically single-use.

## Session assurance

Primary OIDC/email authentication records `Primary` assurance. Passkey sign-in issues `Strong` assurance. TOTP or recovery-code verification updates the current opaque session with a step-up timestamp. Factor enrollment requires an authenticated session whose initial authentication or most recent step-up is within ten minutes.

Recovery-code step-up is deliberately auditable as `RecoveryCode`; it restores access but does not silently become a passkey or TOTP assertion. Password login, SMS recovery, and automatic email collision linking remain prohibited.

## Compatibility

Existing external identities, email links, opaque customer cookies, tenant memberships, and configuration-backed legacy Back Office sessions are unchanged. Public enrollment and factor-management screens are deferred to their approved Phase 13 UI packages.

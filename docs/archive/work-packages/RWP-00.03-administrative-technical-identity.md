# RWP-00.03 — Administrative Surface and Technical Identity Migration

## Status

Complete in the proposed merge state under issue #422 on branch `rwp/00.03-administrative-technical-identity`; the Sequential claim releases only after merge.

## Goal

Make **Back Office** the customer operational application and **Platform Operations** the internal Vennusign console in both user-facing and canonical technical contracts, while keeping legacy callers fail-closed during a bounded compatibility window.

## Canonical Mapping

| Concern | Canonical identity | Temporary compatibility identity |
| --- | --- | --- |
| Customer application | Back Office | Venue Admin |
| Internal application | Platform Operations | Super Admin / Admin |
| Customer API prefix | `/api/back-office` | `/api/venue-admin` |
| Internal API prefix | `/api/platform-operations` | `/api/admin` |
| Customer auth header | `X-Vennusign-Back-Office-Token` | `X-Vennu-Venue-Token` |
| Internal auth header | `X-Vennusign-Platform-Operations-Key` | `X-Vennu-Admin-Key` |
| Customer auth role | `BackOffice` | `BackOffice` accepted only through explicit compatibility mapping |
| Internal auth role | `PlatformOperations` | `PlatformOperations` accepted only through explicit compatibility mapping |
| Customer configuration scope | `BackOffice` | `VenueAdmin` migrated by DbUp |
| Internal configuration scope | `PlatformOperations` | `Admin` migrated by DbUp |

Legacy route and header use is non-sensitive operational telemetry. Compatibility never changes the canonical authorization conclusion, tenant, organization, or venue selected by the server.

## UI and Function Gap Analysis

- **Audience and purpose:** Back Office is for organization and venue operators; Platform Operations is for Vennusign internal support and platform staff. The application name is not an organization membership role.
- **Hierarchy and navigation:** retain current routes and tasks, but label shells, page titles, navigation landmarks, sign-in guidance, and support handoffs consistently with the canonical application identity.
- **Identity and context:** this package distinguishes the application name from the signed-in person. The full active organization/venue context indicator and selector remains the separately approved RWP-05.06 / issue #419 and is intentionally not duplicated here. No browser-supplied context becomes authoritative.
- **CRUD and high-impact actions:** this migration does not add or remove CRUD behavior. Existing confirmation, audit, entitlement, and tenant boundaries remain unchanged.
- **Essential states:** loading, empty, error, success, permission, session-expired, legacy-access, and provider-return states use the canonical application names without hiding recovery guidance.
- **Validation and destructive actions:** no destructive data action is introduced. Unsafe or unrecognized legacy claims, headers, routes, or persisted values fail closed.
- **Accessibility:** page titles and landmark names clearly identify Back Office or Platform Operations; repeated controls retain consistent accessible names. This follows W3C guidance for consistent identification, accessible names, and landmark regions.
- **Responsiveness:** the longer application and context labels must wrap without obscuring navigation or actions on narrow screens.
- **API/data/auth/entitlements:** canonical routes, headers, schemes, policies, claims, configuration keys, and persisted scopes are server-owned. Compatibility aliases preserve HTTP method, query, fragment, return-path validation, membership checks, and venue tenancy. Entitlements are unchanged.

## Migration Sequence

1. Inventory the old-to-new mapping and supported compatibility consumers.
2. Add canonical identifiers with narrow legacy route, header, cookie, configuration, and persisted-value compatibility.
3. Apply ordered, rerunnable DbUp backfill and verification for configuration scopes and keys.
4. Cut application code, tests, local development, CI, telemetry, and active documentation to canonical names.
5. Verify legacy use is observable without secrets or identifiers and document removal conditions.
6. Remove only compatibility paths proven unused; package/store IDs and historical migration resource names remain compatibility boundaries until their external consumers can be verified.

## Compatibility Removal Conditions

- Legacy API routes and headers: remove only after supported clients show zero legacy hits for the documented observation window.
- Legacy configuration keys/scopes: remove readers only after every supported environment has applied the migration and validation query returns zero legacy rows.
- Existing customer session cookie: accept until its maximum configured absolute lifetime has elapsed after canonical issuance begins.
- Android/Tizen/webOS package IDs, published domains, DbUp journal resource names, and historical migrations: retain until store/domain/deployment ownership and upgrade paths are verified in separately authorized external validation.

## Validation Plan

- Full non-integration validation because shared authentication, routes, configuration, migrations, projects, and workflows change.
- Focused unit and static tests for route aliases, canonical/legacy header parity, fail-closed claims and roles, configuration fallback, DbUp migration contracts, shell identity/context, local development, telemetry, and remaining legacy-match classifications.
- Skip Azure SQL, live identity/provider, hosted infrastructure, credentialed browser, container, physical-device, signing/store, and cross-system tests under the standing owner instruction.

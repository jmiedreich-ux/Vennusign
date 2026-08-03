# Administrative Application Identity

## Decision

The customer operational application is **Back Office**. The internal Vennusign support and platform console is **Platform Operations**. These are application identities, not customer membership roles.

Canonical technical contracts use `BackOffice` and `PlatformOperations` in namespaces, types, authentication schemes, policies, claims, configuration keys/scopes, telemetry dimensions, and build/service labels. Canonical HTTP prefixes are `/api/back-office` and `/api/platform-operations`.

## Authorization Boundary

- Back Office derives the signed-in customer, organization membership role, organization, venue, and capabilities on the server. A header or route value cannot select an unauthorized tenant or venue.
- Platform Operations requires the configured internal key and its explicit permission claims. Back Office membership never grants Platform Operations access.
- Canonical and legacy transports produce the same canonical principal and policy result. Unknown or conflicting values fail closed.
- Application names remain distinct from organization roles such as owner, administrator, manager, editor, and viewer.

## Compatibility Boundary

Legacy `/api/venue-admin` and `/api/admin` routes and legacy `X-Vennu-*` headers are bounded aliases. They preserve methods and payloads, emit non-sensitive compatibility telemetry and a `Deprecation: true` response header, and never broaden authorization. Database configuration values are migrated by ordered DbUp scripts with idempotent preconditions and verification.

The previous DbUp embedded-resource names remain stable because they are journal identifiers, not active product identity. Published package IDs and domains remain stable until external store/domain upgrade paths can be verified; changing those values without provider evidence would create a new application identity or break deployed clients.

## UI Contract

Both shells identify the application in the document title and navigation landmark. Authenticated context keeps the application name, signed-in person, membership role, organization, and venue visually and semantically separate. This supports consistent identification and accessible landmark navigation without changing existing operational actions.

## Removal and Forward Fix

Compatibility aliases are removed only after supported environments report no legacy use for the approved observation window and persisted validation finds no legacy configuration values. If canonical clients have written new state, rollback must not delete or reinterpret it; deploy a forward fix while keeping the previous alias available.

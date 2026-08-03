# RWP-00.02 — User-Facing Product Name Standardization

## Status

Complete in the proposed merge state under GitHub issue #418.

## Goal

Present one product identity—Vennusign—across internal, customer-facing, player, packaged TV, authentication, and provider-guidance surfaces while preserving technical compatibility for the separately approved identity migration.

## Scope

- Replace visible standalone `Vennu` product labels with `Vennusign` in Admin, Venue Admin, Display, Android, Tizen, and webOS experiences.
- Update page titles, player status and error messages, authentication display names, TOTP issuer labels, and POS-provider guidance.
- Add focused static regression checks for the web applications and packaged TV surfaces.
- Update active repository guidance and handoff records to use the current product name.

## UI and Function Gap Analysis

- **Goals:** every operator, customer, and viewer-facing surface identifies the same product without changing behavior.
- **Navigation and CRUD:** navigation structure, routes, controls, and CRUD actions are unchanged; only product identity text changes.
- **Essential states:** page titles plus loading, success, pairing, provisioning, offline, permission, and error states use Vennusign consistently.
- **Validation and destructive actions:** no form validation or destructive action changes are introduced.
- **Accessibility:** consistent labels and descriptive page titles support predictable identification in line with WCAG 2.2 [Consistent Identification](https://www.w3.org/WAI/WCAG22/Understanding/consistent-identification) and [Page Titled](https://www.w3.org/WAI/WCAG22/Understanding/page-titled).
- **Responsiveness and devices:** layout and interaction structure are unchanged; the longer Vennusign label is covered across browser and packaged-player source checks.
- **API, data, authorization, and entitlements:** no endpoint, payload, authorization, entitlement, or schema behavior changes. Only user-facing provider guidance and authenticator relying-party/issuer display names change.

## Technical Compatibility Boundary

This package deliberately does not rename `Vennu.*` projects, assemblies, namespaces, `X-Vennu-*` headers, cookies, data-protection purposes, routes, package/application IDs, domains, database objects, or deployment identifiers. Those changes require compatibility aliases, staged deployment, rollback evidence, and migration cleanup under RWP-00.03.

## Acceptance Criteria

1. Internal and external screens display Vennusign rather than standalone Vennu branding.
2. Display/player and packaged-TV titles, statuses, and errors use Vennusign consistently.
3. Authentication and provider-integration guidance presents Vennusign to users.
4. Focused source regressions prevent the old standalone visible name from returning in the covered surfaces.
5. Technical identifiers remain stable for the separately approved migration package.
6. Required affected-area non-integration checks pass on the exact pull-request head.

## Validation Evidence

- Local Admin tests: 83 passed.
- Local Venue Admin tests: 42 passed.
- Local Display tests: 123 passed.
- Local Tizen and webOS static validation passed.
- `git diff --check` and assignment JSON parsing passed.
- Local frontend production builds were unavailable because dependency installation did not provide the TypeScript compiler; GitHub Actions is authoritative.
- Exact-head affected-area GitHub Actions validation is required before merge.
- Skipped under the standing owner instruction: Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests.

## GitHub

- Issue: #418
- Branch: `rwp/00.02-product-name-standardization`
- Pull request: pending

## Next

RWP-00.03 — Administrative Surface and Technical Identity Migration / issue #422.

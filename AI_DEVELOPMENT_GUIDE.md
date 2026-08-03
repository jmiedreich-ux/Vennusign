# Vennusign AI Development Guide

## Purpose

`AGENTS.md` is the authoritative process policy. Read this guide only when implementation needs concise architecture or coding context; it does not repeat governance rules.

## Architecture Map

- `Vennu.Api`: HTTP contracts, controllers, authentication composition, SignalR, and hosted services.
- `Vennu.Core.Models`: shared domain models.
- `Vennu.Data`: Vennusign repositories, persistence behavior, and DbUp migrations.
- `Vennu.DataAccess`: generic reusable provider infrastructure.
- `src/platform-operations`: internal Platform Operations application.
- `src/back-office`: customer and venue operations application.
- `src/display`: hosted player SPA.
- `src/tv`: platform wrappers and distribution packages.

## Implementation Approach

1. Read the claimed issue/package and inspect existing behavior and contracts.
2. Define the smallest vertical slice and its affected areas.
3. Preserve tenancy, authorization, entitlement, provider authority, and migration compatibility.
4. Add focused non-integration tests beside the behavior.
5. Update only task-relevant architecture/operations records.
6. Let impact-based Actions provide authoritative validation.

## Engineering Rules

- Prefer established services and repositories over parallel abstractions.
- Keep customer, organization, venue, and screen ownership server-derived.
- Keep provider callbacks/webhooks authoritative where the existing design requires them.
- Use DbUp for schema changes and preserve migration ordering.
- Keep local secrets in supported environment/configuration providers, never repository files.
- Do not replace working code solely for style consistency.

## Task-Scoped Reading

Use `docs/README.md` to locate current architecture, operations, component, or research material. Do not read archived roadmaps, completed packages, validation records, or handoffs unless the task explicitly requires historical research.

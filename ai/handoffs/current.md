# Vennu Session Handoff

## Work Package

- ID: WP-05.01
- Status: In progress; implementation ready for authoritative CI validation
- Branch: `wp/05.01-menu-domain-persistence`
- Issue: #57
- Pull request: Pending creation from the published branch

## Completed

- Claimed WP-05.01 in the tracker and project status.
- Added Menu, MenuSection, MenuItem, and MenuItemTranslation domain models.
- Added a venue-scoped menu repository contract and implementation with deterministic ordering.
- Added migration 012 with composite venue ownership foreign keys, ordering constraints, price and quantity checks, translation uniqueness, and supporting indexes.
- Registered the repository and added focused repository and migration-resource unit tests.

## Validation

- `git diff --check` passed.
- Local .NET build and unit tests could not run because the environment does not provide `dotnet`.
- Local GitHub CLI is unavailable, so publication uses the authenticated Git remote and connected GitHub integration.
- GitHub Actions is the authoritative build and test environment for this package.
- Integration-type tests were intentionally skipped under the standing repository-owner instruction.

## Changed Files

- `PROJECT_STATUS.md`
- `tracker/assignments.json`
- `docs/work-packages/WP-05.01-menu-domain-persistence.md`
- `src/Vennu.Core.Models/Menu*.cs`
- `src/Vennu.Data/Repositories/IMenuRepository.cs`
- `src/Vennu.Data/Repositories/MenuRepository.cs`
- `src/Vennu.Data/Extensions/ServiceCollectionExtensions.cs`
- `src/Vennu.Data/Scripts/012_create_menu_domain.sql`
- `tests/Vennu.DataAccess.Tests/MenuRepositoryTests.cs`
- `tests/Vennu.Api.Tests/MigrationResourceTests.cs`
- `ai/handoffs/current.md`
- `ai/handoffs/archive/2026-07-29-wp-05.01-publication-blocked.md`

## Exact Next Action

Publish the branch, open a draft PR linked to #57, then inspect and address all required non-integration GitHub Actions results.

## Do Not Redo or Reverse

- Do not recreate issue #57.
- Do not discard the prepared WP-05.01 implementation.
- Do not run integration-type tests.
- Do not begin WP-05.02 before WP-05.01 passes required CI, receives ChatGPT approval, and merges.

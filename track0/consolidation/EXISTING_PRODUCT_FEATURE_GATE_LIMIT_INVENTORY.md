# Existing Product Feature, Gate & Limit Inventory

## Status and scope

RWP-00.76 records the factual current-product implementation found in the repository after RWP-00.75 normalized the target cross-industry model. This is static repository analysis only. It does not query production data, approve current packaging, change a live gate, or make reconciliation recommendations reserved for RWP-00.77.

The inventory covers:

- feature catalog keys and initial tier assignments;
- entitlement resolution and commercial authority;
- direct runtime feature checks and known consumers;
- venue support overrides;
- quantity and metered-usage limits;
- authorization and scope controls that coexist with entitlement;
- locked and upgrade-oriented UI surfaces;
- internal configuration controls that may resemble rollout mechanisms;
- factual gaps where a catalog key exists but no direct runtime enforcement consumer was found by repository search.

## Current authority chain

| Concern | Current source of authority | Scope | Current behavior | Known consumers |
| --- | --- | --- | --- | --- |
| Commercial subscription owner | `OrganizationSubscriptions`, with `VenueSubscriptions` retained as compatibility fallback | Organization first; unattached legacy venue fallback | `active` or unexpired `trialing` status grants access evaluation. Organization tier/status takes precedence over a venue projection. | `FeatureResolutionService`, `VenueEntitlementService`, onboarding/billing services, Back Office billing presentation |
| Feature catalog master switch | `Features.IsActive` | Global feature definition | Inactive features resolve disabled with source/reason `master-switch`; active features continue to tier/override evaluation. | `FeatureResolutionService`, Platform Operations feature matrix |
| Tier feature assignment | `TierFeatures` | Subscription tier | Active entitled subscription plus an enabled tier-feature row resolves the key enabled; optional `LimitValue` is carried into the entitlement. | `FeatureResolutionService`, `UsageMeteringService`, feature-aware services and UI |
| Venue feature override | `VenueFeatureOverrides` | Venue + feature | Active non-expired override is applied after tier resolution and therefore wins enabled/disabled state. Override does not carry the tier `LimitValue`. | Platform Operations venue support detail and override endpoints; `FeatureResolutionService` |
| Entitlement cache | In-memory cache in `FeatureResolutionService` | Venue feature set | 60-second sliding cache; explicit invalidation occurs after an override mutation. | Every `HasFeatureAsync` / `GetFeatureAsync` consumer |
| Screen and venue allowance | `SubscriptionTiers.MaxScreens`, `SubscriptionTiers.MaxVenues` | Screens per venue; venues per organization | `-1` is unlimited. Creation checks occur before mutation. Archived screens are excluded from screen count. | Venue/screen provisioning and onboarding paths through `VenueEntitlementService` |
| Metered usage allowance | `TierFeatures.LimitValue` plus monthly `FeatureUsage` records | Venue + feature + UTC calendar month | Limit must parse as a non-negative integer. Consumption fails when the monthly allowance is exhausted. | `UsageMeteringService`; current seeded use is `ai_translation = 1` on Restaurant Starter |

## Feature catalog and initial tier assignments

The initial catalog is seeded by `src/Vennu.Data/Scripts/004_create_feature_tier_core.sql`; `video_wall` is added by migration 014. These are factual current keys, not Track 0 approval of their classification or package placement.

Tier abbreviations: **S** Starter, **RS** Restaurant Starter, **P** Pro, **B** Business.

| Key | Label | Category | Initial tier assignment | Direct runtime enforcement found | Known product consumers or presentation surfaces |
| --- | --- | --- | --- | --- | --- |
| `photo_grid` | Photo Grid | layouts | S, RS, P, B | No direct `HasFeatureAsync` consumer found | Feature matrix, billing/upgrade feature lists, layout presentation metadata |
| `classic_diner` | Classic Diner | layouts | S, RS, P, B | No direct consumer found | Feature matrix and layout/upgrade presentation |
| `basic_scheduling` | Basic Scheduling | scheduling | S, RS, P, B | No direct consumer found | Feature matrix and billing/upgrade presentation; scheduling services exist independently |
| `allergen_badges` | Allergen Badges | content | S, RS, P, B | Yes | `MenuItemManagementService` blocks tag changes when disabled; `MenuSectionManagementService` exposes capability state to the editor |
| `analytics` | Analytics | analytics | S, RS, P, B | No direct consumer found | Feature matrix and billing/upgrade presentation; analytics endpoints are not shown as using `HasFeatureAsync` by repository search |
| `meal_periods` | Meal Periods | scheduling | RS, P, B | No direct feature-resolution check found in the controller/service paths reviewed | Back Office meal-period endpoints and editor/dashboard surfaces are authorization-scoped but not directly feature-gated in reviewed code |
| `bilingual_display` | Bilingual Display | localization | RS, P, B | No direct consumer found | Feature matrix and upgrade presentation; language configuration exists separately |
| `ai_translation` | AI Translation | ai | RS with `LimitValue = 1`; P, B unlimited in seed | Metering infrastructure exists; no direct product action consumer found in reviewed search | `UsageMeteringService`, usage snapshots, billing/upgrade presentation |
| `quick_update` | Quick Update | operations | RS, P, B | Yes | `QuickUpdateService` requires the key for daily-special and availability changes; menu editor exposes capability state |
| `all_layouts` | All Layouts | layouts | P, B | No direct backend consumer found | Personalized locked preview, upgrade opportunities, feature matrix |
| `happy_hour` | Happy Hour | scheduling | P, B | Yes | `HappyHourService` read state and write enforcement; menu-item happy-hour pricing; menu editor capability state; Back Office happy-hour surfaces |
| `pos_integration` | POS Integration | integrations | P, B | No common `HasFeatureAsync` consumer found | Square, Toast, Clover, POS configuration surfaces and upgrade presentation; provider controllers are authorization-scoped |
| `staff_app` | Staff App | operations | P, B | No direct consumer found | Feature matrix and upgrade presentation |
| `ai_custom_builder` | AI Custom Builder | ai | B | No direct consumer found | Feature matrix and upgrade presentation |
| `multi_location` | Multi-location | operations | B | No direct feature check found | Organization/venue administration and upgrade presentation exist independently |
| `white_label` | White Label | branding | B | No direct backend consumer found | Personalized locked preview and upgrade presentation |
| `html_editor` | HTML Editor | content | B | No direct backend consumer found | Personalized locked preview and upgrade presentation |
| `video_wall` | Video Wall | layouts | P, B | Yes | `VideoWallService` exposes enabled state on reads and requires the key for create/update/remove operations |

## Direct runtime gate inventory

### `FeatureResolutionService`

Source: `src/Vennu.Data/Services/FeatureResolutionService.cs`.

Resolution order:

1. Load all feature definitions.
2. Initialize every key disabled; inactive definitions are marked `master-switch`.
3. Resolve the authoritative organization subscription when the venue belongs to an organization; otherwise use the legacy venue subscription.
4. Accept `active` or unexpired `trialing` status.
5. Enable active features assigned to the resolved tier and carry `TierFeatures.LimitValue`.
6. Apply active venue overrides last.
7. Cache the complete venue feature set for 60 seconds.

The effective result is venue-scoped even when the commercial subscription is organization-owned.

### Explicit service checks

| Source | Key(s) | Read behavior | Write behavior | Failure form |
| --- | --- | --- | --- | --- |
| `src/Vennu.Api/Services/QuickUpdateService.cs` | `quick_update` | Not applicable | Daily-special and item-availability operations require entitlement | `ArgumentException` with feature-required message |
| `src/Vennu.Data/Services/HappyHourService.cs` | `happy_hour` | Returns schedule/state plus `Entitled`; when disabled, state is resolved without the saved schedule | Schedule updates require entitlement | `ArgumentException` |
| `src/Vennu.Api/Services/MenuItemManagementService.cs` | `happy_hour`, `allergen_badges` | Existing values can be read through menu/editor paths | Changing happy-hour price or tags requires the corresponding key | `ArgumentException` |
| `src/Vennu.Data/Services/MenuSectionManagementService.cs` | `happy_hour`, `allergen_badges`, `quick_update` | Returns capability booleans to the menu editor | Does not itself enforce all editor writes | Capability state is presentation data |
| `src/Vennu.Api/Services/VideoWallService.cs` | `video_wall` | Returns `Enabled` and any existing groups | Save/remove requires entitlement | `ArgumentException` |
| `src/Vennu.Data/Services/UsageMeteringService.cs` | arbitrary catalog key | Reads monthly usage only after entitlement resolution | Consumption requires enabled feature and valid numeric limit | Disabled, invalid-limit, and limit-reached `InvalidOperationException`s |

Repository search did not identify direct `HasFeatureAsync` enforcement for every seeded key. That is a factual inventory result, not a conclusion about whether the key should remain, move, or be removed.

## Overrides and support controls

Sources:

- `src/Vennu.Data/Scripts/005_create_venue_feature_overrides.sql`
- `src/Vennu.Data/Services/VenueFeatureOverrideManagementService.cs`
- `src/Vennu.Api/Controllers/PlatformOperations/PlatformOperationsVenuesController.cs`
- `src/platform-operations/src/VenueDetail.tsx`

Current behavior:

- Platform Operations may enable or disable one active feature for one venue.
- A non-empty reason is mandatory and limited to 500 characters.
- Optional expiry must be in the future.
- Applying/removing an override invalidates that venue's cached feature set.
- Operational events record `override_applied` and `override_removed`.
- Override resolution occurs after tier assignment and therefore wins the boolean enabled state.
- The override model is venue-scoped even when commercial authority is organization-scoped.
- The override does not preserve or replace a tier feature's `LimitValue`; an override-enabled feature resolves with no limit value.

Authority is the Platform Operations authentication policy. No separate per-action override permission was found on the reviewed controller.

## Quantity and usage limits

### Subscription-tier structural limits

Sources: `SubscriptionTier`, tier migrations, `TierManagementService`, `VenueEntitlementService`.

| Limit | Current unit and scope | Enforcement | Current seed behavior |
| --- | --- | --- | --- |
| `MaxScreens` | Non-archived screens per venue | `EnsureCanAddScreenAsync` before screen creation | Starter 2; Restaurant Starter 1; Pro 6; Business unlimited (`-1`) |
| `MaxVenues` | Venues per organization | `EnsureCanAddVenueAsync` before venue creation/attachment | Added by later Phase 13 tier evolution; values are tier-managed rather than feature keys |

The screen check requires an authoritative active/trialing subscription, resolves organization tier first, and excludes screens whose status is `Archived`.

### Feature usage limits

Sources: `TierFeatures.LimitValue`, `UsageMeteringService`, feature-usage repository and monthly records.

- Scope is venue + feature + UTC month.
- Only positive consumption amounts are accepted.
- A null limit is treated as unlimited.
- A non-null limit must be an integer greater than or equal to zero.
- Limit exhaustion prevents additional consumption.
- The initial seed gives Restaurant Starter `ai_translation` a monthly value of `1`; no other seeded key has a `LimitValue` in migration 004.

### Other validation bounds

The repository includes many object validation bounds—name lengths, tag counts, prices, video-wall member counts, schedule windows, and similar domain validation. They are not commercial feature or subscription limits and are not inventoried as entitlement allowances.

## Authorization and scope inventory

Entitlement is not the only access control currently present.

| Surface | Authority | Scope behavior |
| --- | --- | --- |
| Back Office / Venue Admin controllers | `BackOfficeAuthenticationDefaults.AuthorizationPolicy` | `[BackOfficeVenueScope]` constrains venue routes; reviewed Happy Hour, Meal Period, Theme, POS, playlist, and related controllers use this pattern |
| Platform Operations feature matrix | `PlatformOperationsAuthenticationDefaults.AuthorizationPolicy` | Global tier-feature assignment administration; mutations are attributed to an admin identifier and audited |
| Platform Operations venue support / overrides / tier switch | Platform Operations policy | Venue-specific support actions, overrides, support detail, provisioning, and legacy venue-tier switch |
| System configuration read | `Configuration:read` | Environment and optional application scope |
| System configuration edit | `Configuration:edit` | Definition + environment with optimistic concurrency |
| System configuration rollback | `Configuration:admin` | Definition + environment + revision |
| Secret configuration values | Additional claim `vennusign:configuration_permission = secrets` | Required even when the caller has configuration edit authority |
| Customer onboarding/authentication | Customer authorization policies and organization identity context | Organization/customer-specific flows; commercial state is resolved separately |

The reviewed Back Office controllers generally enforce authentication and venue scope at the controller boundary. Some feature-specific write restrictions occur deeper in services; other catalog keys currently appear only in presentation or administrative data.

## Feature-matrix administration

Sources:

- `src/platform-operations/src/FeatureMatrix.tsx`
- `src/Vennu.Api/Controllers/PlatformOperations/PlatformOperationsFeaturesController.cs`
- feature-matrix service/repositories and audit migration 009.

Current behavior:

- Displays every feature against every tier.
- Allows enable-all, clear-all, individual checkbox changes, discard, review, and confirmation.
- Shows impact counts and affected tier names before saving.
- Writes tier-feature assignments and records recent audit entries.
- Successful changes cause effective entitlements to be recalculated through subsequent resolution/cache refresh behavior.
- The matrix is a commercial-administration surface; it does not itself distinguish Track 0 core, add-on, state, permission, limit, or rollout classifications.

## Locked and upgrade UI inventory

Sources include:

- `src/back-office/src/EntitlementLockChip.tsx`
- `src/back-office/src/LockedSectionPreview.tsx`
- `src/back-office/src/InlineFeatureHint.tsx`
- `src/back-office/src/SidebarUpgradeNudge.tsx`
- `src/back-office/src/upgradeExperience.mjs`
- corresponding Platform Operations components
- `src/back-office/src/lockedPreview.mjs`
- billing/tier decision cards and dialogs.

Current presentation patterns:

- A lock chip names the opportunity and required tier and can open upgrade options.
- Locked-section previews support personalized menu examples for `all_layouts`, `white_label`, and `html_editor`.
- Inline hints and sidebar nudges surface upgrade opportunities using feature keys and tier metadata.
- Billing and tier-decision UI presents screen/venue allowances and upgrade/downgrade consequences.
- Feature matrix and venue support UI expose administrative enabled/disabled state and overrides.

Impeccable review finding for inventory purposes: the components have accessible names and explicit action labels, but the current locked/upgrade model is keyed primarily to commercial entitlement. Repository search did not establish a shared presentation contract that always distinguishes entitlement lock, missing permission, unavailable product state, disconnected integration, exhausted limit, unsupported context, or rollout restriction. That distinction remains for RWP-00.77 reconciliation rather than implementation here.

## Configuration and rollout-like controls

No general customer-facing rollout-flag framework equivalent to the feature catalog was found in the reviewed repository search.

The closest implemented control family is system configuration:

- definition-driven values by environment and application scope;
- required/secret metadata;
- optimistic version checks;
- revision history and rollback;
- restart-required and export-policy metadata;
- separate read, edit, admin, and secret claims.

Source: `src/Vennu.Api/Controllers/PlatformOperations/SystemConfigurationController.cs` and the `Vennu.Data.Configuration` services/models.

These settings may enable or alter operational behavior, but RWP-00.76 does not classify individual settings as rollout flags because no complete definition catalog was enumerated in the issue scope output and the current controller is a generic configuration authority.

## Current factual inconsistencies and unknowns carried to RWP-00.77

These are inventory observations only:

- The database catalog represents heterogeneous concepts—layouts, daily operation, analytics, localization, AI, integration, branding, and multi-location behavior—as the same boolean feature type.
- Several keys have direct runtime enforcement (`quick_update`, `happy_hour`, `allergen_badges`, `video_wall`); several appear only in tier/upgrade presentation or have no direct feature-resolution consumer found.
- `meal_periods` has a catalog assignment and full controller/service surface, but the reviewed path is authorization-scoped without a direct entitlement check.
- Basic scheduling and analytics are assigned even to Starter, while separate scheduling/analytics surfaces do not consistently reveal the catalog as their authority.
- POS has one generic catalog key while provider-specific controllers and configuration flows exist.
- Venue overrides can supersede organization-derived tier access and can remove a seeded usage limit when enabling a feature.
- The effective feature set is venue-scoped, while commercial ownership is organization-scoped.
- The feature matrix can enable or disable any catalog key for any tier without a stored primary Track 0 classification.
- Locked UI is feature/tier oriented; a universal reason/state model was not found.
- No complete general rollout-flag registry was found.

RWP-00.77 must map these facts to `track0/consolidation/CROSS_INDUSTRY_MODEL.md`, identify gaps and incorrect classifications, and record remediation recommendations without changing product behavior.

## Validation and boundaries

- Reviewed current `master` after merged RWP-00.75.
- Examined feature/tier migrations, resolution, overrides, usage metering, structural limits, authorization controllers, direct `HasFeatureAsync` consumers, feature administration, and locked/upgrade presentation.
- Applied project-local Impeccable guidance to the factual locked-state presentation review.
- No UI, API, schema, migration, billing, entitlement, permission, limit, override, configuration, or rollout behavior was changed.
- Azure SQL and all integration/external-system tests remain skipped under the standing owner instruction.

## Handoff

After RWP-00.76 is merged, issue #552 is closed, `master` is verified, and the claim is released, execute **RWP-00.77 — Capability Reconciliation & Gap Analysis (#553)**. RWP-00.77 may recommend remediation but must not implement it.
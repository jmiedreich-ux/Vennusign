# Capability, Entitlement and Authority Foundation

## Canonical capability contract

A capability identifies one product action or outcome. Its stable identifier uses exactly three lowercase segments:

```text
domain.resource.action
```

The Version 1 registry is `Version1CapabilityRegistry` in `Vennu.Core.Models`. A definition contains the stable ID, one approved domain, a product classification, an operation kind, and locale-neutral name and description message keys.

The approved domains are `content`, `publishing`, `screen`, `schedule`, `workflow`, `organization`, `localization`, `analytics`, `branding`, `account`, and `support`. Publishing remains its own domain because previewing, publishing, confirming, replacing, unpublishing, retrying, and restoring releases are distinct from authoring content.

Capability IDs never contain tier names, industry names, provider names, routes, or customer-facing labels. Packaging may assign a capability differently without renaming it. Industry may change terminology and defaults without changing access. A provider or add-on may be required for an action without becoming part of the capability ID.

## Separate typed decisions

The following are not capabilities:

| Type | Meaning |
| --- | --- |
| Permission | What an actor may do. |
| Role | A protected or custom collection of permissions. |
| Role assignment | A role attached to an actor and scope, optionally for a bounded time. |
| Product state | Current facts such as available, paired, online, draft, published, stale, or conflicted. |
| Allowance | A typed quantity, counting scope, consumption, and enforcement rule. |
| Add-on/service | An independently attached external, metered, hardware, or managed service. |
| Layout/template | Presentation catalog content and compatibility metadata. |
| Rollout control | An internal availability mechanism, never customer entitlement. |
| Navigation | A task destination derived from useful actions, not authority for those actions. |

## Current feature-key disposition

Every seeded generic feature key has one explicit disposition. Where a broad key represented multiple actions, its one disposition is to split those actions into the listed canonical capabilities and any separate typed state.

| Current key | Disposition | Canonical target |
| --- | --- | --- |
| `photo_grid` | Layout/template | `LayoutTemplate:photo_grid` |
| `classic_diner` | Layout/template | `LayoutTemplate:classic_diner` |
| `basic_scheduling` | Capability | `schedule.entry.manage` |
| `allergen_badges` | Capability | `content.item.dietary_information_manage` |
| `analytics` | Split capabilities | Delivery-health, operations, and portfolio analytics actions |
| `meal_periods` | Split capabilities | Core `schedule.entry.manage`; advanced `schedule.rotation.manage` |
| `bilingual_display` | Capability | Core `localization.variant.manage` |
| `ai_translation` | Add-on/service | Automated-translation add-on supporting `localization.translation.automate` |
| `quick_update` | Split capabilities | Core `content.item.availability_update`; advanced bounded bulk update |
| `all_layouts` | Layout/template | Advanced layout catalog supporting `branding.layout.manage` |
| `happy_hour` | Split capability/state | `schedule.promotion.automate`; promotion activation remains product state |
| `pos_integration` | Add-on/service | Point-of-sale attachment supporting `content.source.synchronize` |
| `staff_app` | Removal | Dormant presentation key; the client used never grants an action |
| `ai_custom_builder` | Add-on/service | Automated-content-assistance add-on |
| `multi_location` | Split capabilities/context | Authorized venue context plus organization governance actions |
| `white_label` | Capability | `branding.standard.manage` |
| `html_editor` | Deferred capability | `branding.custom_content.manage`, pending its bounded safety contract |
| `video_wall` | Split capability/state | `screen.wall.coordinate`; configured/enabled remains product state |

The executable form of this table is `CurrentConceptReconciliation`. Tests require every seeded key exactly once and require every referenced capability to exist in the canonical registry.

## Current mechanism disposition

| Current mechanism | Typed disposition and replacement owner |
| --- | --- |
| Back Office route keys (`menus`, `scheduling`, `tap_list`, `screens`, `themes`, `pos_integration`) | Navigation destinations. Track 1.04 derives their presentation from server decisions; endpoints authorize individual actions. |
| Back Office `session.capabilities` strings | A presentation projection only. Track 1.02 introduces structured server decisions; Track 1.04 removes this projection as authority. |
| `MembershipCapability` enum and membership-role mapping | Permission input. Track 1.03 replaces mixed capability naming with scoped permissions and role assignments. |
| `Features` and `TierFeatures` | Commercial access assignment to canonical capabilities. Track 1.04 removes generic feature authority. |
| `VenueFeatureOverrides` | Explicit scoped entitlement or allowance exception with actor, reason, start, expiry, and audit. Product state is never overridden here. |
| `FeatureUsages` and string `LimitValue` | Typed allowances and consumption. Quantity never grants a capability or permission. |
| `FeatureResolutionService`, `HasFeatureAsync`, and boolean entitlement snapshots | Replaced by the structured server decision engine in Track 1.02 and removed as authority in Track 1.04. |
| Feature Matrix and tier-feature support tools | Replaced by typed commercial capability assignment; role, state, allowance, add-on, layout, and rollout remain separate. |
| Platform Operations session keys (`dashboard`, `venues`, `tiers`, `features`) | Administrative navigation, protected by platform permissions. |
| Product values (`isAvailable`, online/offline, paired/unpaired, draft/published, source freshness/conflict, delivery revision) | Domain/system state with a state-specific reason and recovery action. |
| Screen and venue quantities | Typed allowances. Layout capacity and HaaS contract terms remain separate product/service facts. |
| Provider configuration and internal switches | Add-on/configuration state or internal rollout control. Neither is a capability or tier assignment. |

## Classification rule

The registry classifies each product action as universal core, advanced native, governance, or deferred. This is product metadata, not a tier name or billing rule. Universal core preserves manual create, edit, preview, pair, publish, confirm, replace, unpublish, retry, restore, and recovery. Advanced and governance actions may be assigned commercially later without changing their identifiers. Deferred actions cannot be treated as available until their bounded product and safety contract is approved.

## Deterministic validation

Focused tests enforce:

- exact lowercase three-segment syntax;
- unique IDs and matching domains;
- a distinct `publishing` domain;
- absence of packaging, industry, provider, route, and display labels in IDs;
- one typed disposition for all 18 current seeded feature keys;
- registered targets for every capability disposition;
- navigation classification for all current route keys.

The registry is deterministic and requires no database or external service. Track 1.02 consumes it as the sole capability identifier source.

## Server decision and reason contract

`CapabilityDecisionResult` is the server-owned action decision. It returns:

- `allowed`, `allowed_with_conditions`, `denied`, `unavailable`, or `temporarily_blocked`;
- one stable primary reason code and category;
- the canonical capability ID;
- a locale-neutral message key and structured parameters;
- a request correlation ID and locale;
- applicable resolution, retry guidance, and non-blocking conditions.

The decision engine requires exactly one result for identity/context, rollout, entitlement, permission, add-on, allowance, resource state, and request validity. Missing or duplicate dimensions fail closed as `decision.input_incomplete`. An unknown capability fails closed as `capability.unknown`.

The ordered primary-reason selection is deterministic: identity/context, rollout, entitlement, permission, add-on, allowance, resource state, then request validity. The order does not combine the dimensions; it selects the first customer-actionable blocking result while retaining the typed category. In particular, a permission restriction is never described as product state, and an add-on or state problem is never described as a tier upgrade.

Batch evaluation accepts multiple canonical capabilities and preserves input order and correlation. It is the contract used by navigation and dashboard projections. A preview improves presentation only; it never authorizes a mutation.

`CapabilityActionAuthorizer.RequireAllowedAsync` deliberately calls the input provider on every invocation. A state-changing endpoint calls it immediately before changing data, so an earlier browser preview or session result cannot grant authority. Blocked mutations receive `CapabilityDecisionDeniedException` with the complete structured decision. Track 1.03 supplies scoped permissions to the permission dimension, and Track 1.04 connects the authorizer to the affected endpoints and removes the generic gate as authority.

## Repository message catalogs

Capability decision messages are embedded from `Vennu.Data/Resources/CapabilityMessages`. Server results return stable message keys and parameters; presentation may resolve those keys using `ICapabilityMessageCatalog`.

Locale fallback is deterministic. For example, `fr-CA` resolves through `fr-CA`, then `fr`, then `en-US`. Unknown or invalid locales use `en-US`. If no catalog contains a key, the key itself is returned instead of inventing or silently substituting unrelated copy. Customer-authored multilingual screen content remains separate from these product-interface catalogs.

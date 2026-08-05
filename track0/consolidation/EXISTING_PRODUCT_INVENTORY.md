# Existing Product Feature, Gate & Limit Inventory

## Status and method

RWP-00.76 records the factual current product mechanisms visible in the default branch and the merged implementation history. It does not decide whether any mechanism is correct, change a live gate, add a key, remove a lock, alter a limit, or approve future packaging. Reconciliation recommendations belong to RWP-00.77.

The inventory distinguishes five authorities:

- **session capability authority** returned by the Back Office session endpoint;
- **effective feature authority** returned by the billing presentation endpoint;
- **tier and subscription authority** returned by server-side billing state and provider-confirmed events;
- **permission and context authority** resolved from authenticated organization/venue claims and server checks;
- **product state authority** stored on domain objects and delivery/source records.

Browser components present these decisions but do not become their authority.

## Current Back Office route capability checks

Source: `src/back-office/src/navigation.mjs` and `src/back-office/src/App.tsx`.

| Route | Session capability key | Upgrade presentation key | Current consumer | Observed behavior |
| --- | --- | --- | --- | --- |
| Home | none | none | Back Office router | Always present after an authorized session. |
| Menu | `menus` | `quick_update` | Navigation and route guard | Opens when `session.capabilities` contains `menus`; otherwise locked presentation is shown. |
| Schedules | `scheduling` | `meal_periods` | Navigation and route guard | Opens when `scheduling` is present; otherwise shows a locked opportunity. |
| Tap list | `tap_list` | `all_layouts` | Navigation and route guard | Opens when `tap_list` is present. The commercial prompt uses `all_layouts`, so route access and advertised benefit are not the same identifier. |
| Screens | `screens` | `all_layouts` | Navigation and route guard | Opens when `screens` is present. Premium layout presentation may be shown separately. |
| Themes | `themes` | `all_layouts` | Navigation and route guard | Opens when `themes` is present. Locked theme/layout previews use the premium layout feature. |
| POS integrations | `pos_integration` | `pos_integration` | Navigation and route guard | Both capability and commercial feature currently use the same text key. |
| Billing | none | none | Back Office router | Available to an authorized venue session; billing actions are server rechecked. |
| Account & security | none | none | Back Office router | Available to an authorized session; individual operations have server authority. |

`canOpenBackOfficeRoute` performs a client presentation check against `session.capabilities`. The API and data layer remain responsible for authorization of each operation. A hidden or locked navigation item is not proof that the server permits or denies an operation.

## Current effective feature catalog

Source: `src/back-office/src/upgradeExperience.mjs`, billing presentation `EffectiveFeatures`, locked-surface components, and merged Phase 11/RWP-11 remediation.

| Feature key | Current title | Required tier slug in presentation catalog | Current presentation/consumer | Factual classification before reconciliation |
| --- | --- | --- | --- | --- |
| `meal_periods` | Meal periods | `restaurant_starter` | Scheduling panel, locked navigation/section, upgrade sheet | Effective feature key used for commercial presentation and server-resolved access. |
| `bilingual_display` | Bilingual displays | `restaurant_starter` | Menu/design opportunity | Effective feature key. |
| `ai_translation` | Menu translation | `restaurant_starter` | Menu opportunity | Effective feature key; external/metered behavior is not represented by this browser catalog alone. |
| `quick_update` | Quick Update | `restaurant_starter` | Menu route opportunity and inline prompt | Effective feature key; Menu route itself checks `menus`. |
| `all_layouts` | All display layouts | `pro` | Design panel, tap-list/screens/themes commercial presentation, locked previews | Effective feature key reused across multiple route/surface contexts. |
| `happy_hour` | Happy hour | `pro` | Scheduling opportunity and Happy Hour administration | Effective feature key; `HappyHourSnapshot` also returns `isEntitled`. |
| `pos_integration` | POS integration | `pro` | POS route and menu/connect opportunity | Effective feature and session capability text are currently aligned. |
| `staff_app` | Staff mobile app | `pro` | Menu/operations opportunity | Presentation catalog key; no separate current route was found in Back Office navigation. |
| `video_wall` | Video walls | `business` | Design opportunity and video-wall administration | Effective feature key; current video-wall snapshot separately returns `enabled`. |
| `multi_location` | Multi-location control | `business` | Operations/portfolio opportunity | Effective feature key; venue context switching is also constrained by authorized session contexts. |
| `white_label` | White label | `business` | Design opportunity | Effective feature key. |
| `html_editor` | Custom HTML | `business` | Design opportunity | Effective feature key. |

The browser selects upgrade opportunities only where `effectiveFeatures[featureKey].enabled === false`. Optional `limitValue` is returned with each effective feature but is not interpreted as a permission or product-state value.

## Current tier presentation and billing state

Source: `src/back-office/src/upgradeExperience.mjs`, `src/back-office/src/api.ts`, `src/Vennu.Api/Contracts/BackOffice/BackOfficeBillingPresentationResponse.cs`, and `src/Vennu.Data/Services/BillingTierDecisionEvaluator.cs`.

### Tier slugs presently recognized by the browser presentation catalog

- `starter`
- `restaurant_starter`
- `pro`
- `business`

The server returns current and available tiers with authoritative IDs, names, slugs, monthly price, `MaxScreens`, `MaxVenues`, direction, selectability, blocking reasons, and lost feature keys. The browser catalog supplies labels, badge tone, benefit copy, and recommended tier placement for the known feature keys. It does not create tier records or entitlement authority.

### Subscription state

The current billing presentation recognizes:

- `trialing`
- `active`
- `past_due`
- `canceled`
- `cancelAtPeriodEnd`
- `canManageBilling`
- trial and current-period dates

Checkout and Billing Portal returns are informational. Pending tier decisions are stored locally only to explain provider progress. Access is refreshed from Vennusign and remains webhook/server authoritative; no browser return parameter grants a feature.

### Tier direction and downgrade eligibility

The server evaluator returns `start`, `current`, `upgrade`, or `downgrade`. A target tier is blocked when:

- it is the current tier;
- active screen usage exceeds `MaxScreens`;
- organization venue usage exceeds `MaxVenues`.

Lost feature keys are returned separately and sorted for review. Eligibility is rechecked by the server before Checkout or a targeted Billing Portal session is opened.

## Current quantity and usage limits

| Limit/usage field | Authority/source | Scope | Current consumer/behavior |
| --- | --- | --- | --- |
| `MaxScreens` | Subscription tier | Target/current tier | Compared with active screen count before plan selection. Negative values are treated as unbounded by the evaluator. |
| `MaxVenues` | Subscription tier | Organization | Compared with organization venue count before plan selection. Negative values are treated as unbounded. |
| `ActiveScreens` | Billing usage summary | Authorized organization/current context | Used for downgrade blocking and billing presentation. |
| `CurrentScreenLimit` | Billing usage summary | Current tier | Presented with current usage. |
| `OrganizationVenues` | Billing usage summary | Organization | Used for downgrade blocking and billing presentation. |
| `CurrentVenueLimit` | Billing usage summary | Current tier | Presented with current usage. |
| feature `limitValue` | Effective feature summary | Feature-specific | Returned to the browser as a string; no universal client enforcement was found. |
| screen layout capacity | Screen/layout preview | Individual target layout | Product/display capacity calculation, not a subscription entitlement. Overflow preview reports total, visible, and overflow items. |
| HaaS term months | HaaS contract/bundle | Separate hardware/service contract | Recognized terms are 18, 24, and 36 months; separate from software feature access. |
| HaaS remaining months and estimated buyout | HaaS contract | Separate contract | Disclosure and contract status; not a feature limit. |

No normalized current implementation was found for user, storage, history, AI, translation, integration, report, export, transaction, support, or consumption allowances. Their existence as Track 0 candidates does not mean the product currently enforces them.

## Current permission and context controls

| Control | Current authority | Scope/consumer | Observation |
| --- | --- | --- | --- |
| Customer session and legacy venue link | Back Office session endpoint | Back Office bootstrap | Invalid/expired access is rejected; browser tokens do not select arbitrary authority. |
| Selected venue context | Server-validated authorized contexts; `X-Vennusign-Venue-Id` request hint | Organization/venue switcher | Browser stores the selected venue ID, but the server returns the authorized session and rejects removed access. |
| Session capabilities | Server-generated session | Route presentation and component access | Text keys drive client route presentation; operation endpoints must still authorize. |
| Organization/venue claims | Server authentication/authorization | Back Office API operations | Establish object scope and account context. |
| Billing administration | Server and provider state | Checkout/Billing Portal/HaaS actions | Server resolves tier, usage, customer/subscription, and allowed hosted URL. |
| Support tier and overrides | Platform/Super Admin support surfaces from merged billing reconciliation | Organization/venue support administration | Support may manage authoritative tier/override records; customer browser does not create them. Exact current override keys require server/data inventory during implementation planning. |
| Destructive/high-scope review | Client review dialog plus server operation | Venue switch and destructive actions | The review improves safety but does not replace server permission. |
| Source/local override | Domain/source services | Product data and synchronization | Override state is separate from commercial entitlement and requires source authority/freshness. |

The current Back Office session exposes a flat `capabilities: string[]`; the browser does not receive a normalized per-action permission matrix. The product has server-side claims and object-scoped authorization, but Track 0 reconciliation must determine where session capabilities, permissions, and entitlements have been collapsed into the same strings.

## Current locked and upgrade UI surfaces

Source: `App.tsx`, `LockedNavigationItem.tsx`, `LockedSectionPreview.tsx`, `InlineFeatureHint.tsx`, `SidebarUpgradeNudge.tsx`, `EntitlementLockChip.tsx`, `UpgradeSheet.tsx`, `TierDecisionDialog.tsx`, `BillingStatusCard.tsx`, and merged RWP-11.01 through RWP-11.04.

- locked navigation items retain a visible destination and tier/lock semantics;
- locked section previews show bounded read-only context and an upgrade action;
- inline feature hints place one mapped opportunity in a relevant panel;
- sidebar nudges rotate eligible opportunities and support session dismissal;
- the upgrade sheet presents benefit, current/target tier, interval, and hosted billing continuation;
- the tier decision dialog presents direction, usage conflicts, lost features, and least-destructive guidance;
- locked theme/layout previews may use authorized venue content but remain read-only;
- billing status displays authoritative subscription/provider state;
- dismissals are browser-session presentation state and never change entitlement.

The current UI does not consistently receive a structured reason code distinguishing entitlement lock, missing permission, limit reached, unsupported industry/context, product state, disconnected integration, or rollout restriction. Some distinctions are inferred from separate endpoint shapes or component context.

## Current product-state gates that are not commercial entitlements

The following observed fields affect behavior or presentation but are domain/system state:

- menu item `isAvailable`, `availabilityResetUtc`, and `quantityAvailable`;
- menu/section/item `isActive`;
- Happy Hour `isActive`, `mode`, schedule enablement, and override mode;
- playlist, promotion, emergency broadcast, tap category/item, meal period, and screen enablement/state;
- screen online/last-seen, authoritative/applied revision, and delivery state;
- source connection status, external-action-required guidance, source freshness, and conflict;
- video-wall snapshot `enabled`;
- subscription/provider status and HaaS contract status;
- checkout return/pending local state;
- active venue context and authorized context list.

These values must not be converted into generic premium locks merely because they enable or disable a UI action.

## Current external and separately delivered mechanisms

- POS provider types currently exposed by Back Office: `square`, `toast`, and `clover`.
- Stripe-hosted Checkout and Billing Portal are external billing services; returned URLs are allowlisted and do not grant local access.
- HaaS bundles/contracts are separately modeled from software subscriptions.
- Player/screen delivery, source synchronization, and POS connection states are authoritative operational/integration state, not browser feature flags.

## Current configuration and rollout controls

The inspected Back Office sources expose runtime configuration such as API base URL, hosted billing URL validation, provider price/configuration requirements, and client storage keys. No customer-visible universal rollout-flag catalog was found in the Back Office session or billing presentation contracts.

Internal feature rollout may exist elsewhere in application configuration, deployment settings, or server code, but RWP-00.76 found no evidence that such controls should be represented as customer entitlements. The factual inventory therefore records rollout as an internal control family with no approved customer-facing key set.

## Known duplication and ambiguity recorded without recommendation

- `pos_integration` is both a session capability key and an effective feature key.
- `quick_update` is an effective feature key while the Menu route checks `menus`.
- `all_layouts` is reused as the upgrade key for Tap list, Screens, and Themes despite those routes checking different session capability keys.
- `video_wall` appears as a commercial feature and a separate snapshot `enabled` product/system value.
- `happy_hour` appears as an effective feature and `HappyHourSnapshot.isEntitled`, alongside schedule and override product state.
- `multi_location` is a commercial feature while authorized venue contexts and venue switching are permission/context mechanisms.
- tier `MaxScreens`/`MaxVenues` and screen layout capacity are both called limits but govern different domains.
- effective feature `limitValue` has no normalized type or universal enforcement contract in the browser.
- support overrides are authoritative server data but are not exposed as a normalized customer-visible inventory.

These are observations only. RWP-00.77 decides the normalized mapping and remediation recommendations.

## Inventory boundaries and confidence

This inventory covers the current Back Office capability/navigation model, upgrade catalog, billing presentation contract, tier decision evaluator, locked UI family, provider-authoritative billing path, HaaS separation, current product-state examples, and merged implementation evidence.

It does not claim that a text search alone proves the absence of a control. Where no normalized key was found, the inventory states that the current inspected contracts do not expose one. RWP-00.77 must reconcile these mechanisms with the normalized Track 0 model before implementation packages are proposed.

## Handoff

RWP-00.77 maps this factual inventory to `CROSS_INDUSTRY_MODEL.md`, identifies missing, duplicate, obsolete, or incorrectly classified mechanisms, and recommends bounded remediation without changing product behavior.

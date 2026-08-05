# Capability Reconciliation & Gap Analysis

## Status

RWP-00.77 maps the factual current product inventory to the normalized Track 0 model. It records remediation recommendations only. No key, gate, permission, limit, API, schema, billing rule, UI, or product behavior is changed.

## Executive result

The current product has a sound foundation in server-authoritative billing, provider-confirmed entitlements, venue-scoped sessions, separate HaaS contracts, explicit screen delivery state, and reusable locked-surface patterns. The main gap is not missing commercial machinery; it is that current identifiers collapse several Track 0 concepts into flat strings and route-level decisions.

The product currently mixes:

- capability availability with commercial entitlement;
- route visibility with operation permission;
- broad feature names with individual outcomes;
- plan limits with layout/product capacity;
- industry-specific presentation with canonical capability identity;
- locked presentation with incomplete reason semantics.

Before implementing new industries or final packaging, the existing mechanisms should be normalized around a server-resolved decision contract that independently reports capability, permission, product state, add-on connection, limit, privacy/rights, support, and rollout reasons.

## Reconciliation matrix

| Current mechanism | Normalized mapping | Gap | Recommendation | Priority |
| --- | --- | --- | --- | --- |
| `session.capabilities: string[]` | Capability availability plus possibly permission/entitlement | Flat strings do not explain authority, commercial access, scope, or reason. | Introduce a server-resolved capability decision per capability and scope, with separate permission and entitlement inputs. Retain compatibility projection during migration. | Critical foundation |
| Route keys `menus`, `scheduling`, `tap_list`, `screens`, `themes`, `pos_integration` | Navigation capability/presentation | Route access is broader than individual actions and is not a complete permission model. | Keep routes task-oriented; authorize each operation server-side. Return route availability as a derived view, not the primary entitlement record. | High |
| `quick_update` gated at Restaurant Starter | Required manual rapid update | Normalized Track 0 requires rapid manual availability/state correction as core. | Split essential manual rapid update from advanced bulk, automation, staff-app, or workflow outcomes. Keep the essential action core. | Critical |
| `meal_periods` gated at Restaurant Starter | Core manual schedules plus advanced automation | The key combines ordinary schedule maintenance with automatic switching. | Separate manual hours/service-period scheduling from advanced recurrence, conflict detection, automatic transitions, and orchestration. | Critical |
| `all_layouts` used for Tap list, Screens, and Themes | Advanced presentation/layout catalog | One feature key gates unrelated routes and suggests layouts grant tap-list/screen management. | Keep screen management and required presentation core; define separate advanced layout/library/custom-design capabilities. Do not use one layout key as a proxy for whole-route access. | Critical |
| `happy_hour` effective feature and `HappyHourSnapshot.isEntitled` | Advanced scheduled promotion candidate plus schedule/product state | Entitlement is duplicated in feature and domain response; ordinary price/state editing may be trapped. | Make ordinary price/special editing core; treat automatic scheduled price switching as advanced. Resolve entitlement once in the capability decision layer and return operating state separately. | High |
| `video_wall` feature and snapshot `enabled` | Advanced multi-screen coordination plus product configuration state | Commercial access and configured/enabled state can be confused. | Return distinct `access`, `configured`, `enabled`, `healthy`, and `limit` states. | High |
| `multi_location` feature plus authorized venue contexts | Portfolio coordination entitlement plus permission/context | Authorized access to multiple venues and paid coordination are separate. | Always allow authorized venue switching within the organization’s allowed venue count; gate advanced cross-venue bulk, inheritance, analytics, and governance independently. | Critical |
| `pos_integration` as both session capability and effective feature | Independent add-on candidate | Add-on commercial attachment and permission/configuration/connection state are collapsed. | Model POS as an attachable add-on instance with provider, scope, permission, configuration, connection, source freshness, and limits. Derive route presentation from those states. | Critical |
| `bilingual_display` | Core manual language variants plus advanced localization | A single feature can trap basic accessible multilingual operation behind a plan. | Keep manual alternate-language content and per-language preview core; reserve automated translation, workflow, terminology libraries, and portfolio localization for tier/add-on packaging. | High |
| `ai_translation` | Independent metered AI/translation add-on candidate | Browser tier catalog implies a simple tier feature without consumption/source/review policy. | Model AI/automated translation separately with consumption limits, privacy, review, source labeling, retention, and manual fallback. | High |
| `staff_app` | Alternative client/advanced workflow | No separate route or factual current capability consumer was found. | Confirm whether this key has a live server consumer. If not, mark dormant and either retire it or define a bounded implementation package after owner approval. | Medium |
| `white_label` | Advanced brand governance or add-on | Broad term is ambiguous: screen branding, domain, app, support, or managed service. | Decompose into explicit outcomes and keep mandatory accessibility/system attribution obligations separate. | Medium |
| `html_editor` | Advanced custom presentation | High security and support risk is hidden behind a broad feature key. | Replace with a bounded custom-content capability contract covering sanitization, preview, accessibility, CSP, support, export, and downgrade. | High |
| Tier slugs `starter`, `restaurant_starter`, `pro`, `business` | Candidate commercial archetypes | Restaurant-specific slug and hard-coded browser catalog conflict with cross-industry packaging. | Keep current slugs as legacy identifiers during migration. Introduce stable capability IDs independent of tier names and move display metadata to server-managed catalog. | Critical |
| `MaxScreens` and `MaxVenues` | Usage/quantity limits | Only two normalized allowances exist; organization/venue pooling and active-screen protection are unresolved. | Preserve as initial limit types but move to typed allowance records with scope, consumed quantity, enforcement mode, grace, remediation, and downgrade behavior. | Critical |
| effective feature `limitValue: string` | Feature-specific limit | Untyped string cannot support consistent enforcement, comparison, or accessible explanation. | Replace with typed limit decisions and units; maintain legacy string only as display compatibility. | Critical |
| screen layout capacity/overflow | Product/layout capacity state | Called a limit but unrelated to subscription allowance. | Name and present it as layout capacity/overflow, never as plan usage. | Medium |
| HaaS 18/24/36 terms and contract status | Independent managed hardware/service add-on | Correctly separate from software tiers but may still appear beside them. | Preserve separation. Add explicit attachment scope, fulfillment/support state, and cancellation/export boundaries in later implementation planning. | Confirmed alignment |
| provider-authoritative Checkout/Billing Portal/webhooks | Commercial entitlement authority | Strong alignment; browser pending state is explanatory only. | Preserve. Capability decisions must continue to derive from server/provider-confirmed state, never URL return parameters. | Confirmed alignment |
| support tier/override controls | Exception governance | Override scope, reason, expiry, audit, and customer presentation are not normalized in the inventory. | Define override records with capability/limit target, organization/venue scope, reason, actor, start/expiry, audit, and precedence. Do not use overrides for product state. | High |
| product values such as `isAvailable`, schedule mode, source status, screen delivery state | Product/domain or system state | Some UI patterns may treat disabled actions generically. | Keep these values out of commercial feature catalogs. Provide state-specific explanation and recovery. | Critical |
| internal configuration/rollout | Internal rollout flag | No normalized customer-visible catalog exists, which is appropriate, but reason presentation is unspecified. | Keep rollout internal. When it affects a customer, return a safe temporary-unavailability reason without exposing internal flag names or implying an upgrade. | High |

## Missing normalized capabilities

The current inspected contracts do not expose normalized identifiers for several Track 0 core or future capability families:

- industry, subtype, descriptive traits, and terminology preferences;
- generic content/state operations beyond Restaurant-oriented menu models;
- Hospitality property/amenity/service/event/wayfinding contexts;
- Entertainment venue/attraction/exhibit/program/session/queue/admission contexts;
- Food Truck operation/unit/location/event/service-point contexts;
- Café batch/freshness/preorder/pickup contexts;
- explicit source authority, freshness, conflict, and local-override decision contracts across all objects;
- normalized correction, supersession, expiry, unpublish, undo, and restore capabilities;
- manually authored alternate-language coverage as universal core;
- typed privacy/rights/safety restrictions;
- per-action permission decisions;
- typed add-on attachment and connection decisions;
- typed limit decisions beyond screens and venues;
- organization default/local override/inheritance state;
- structured locked/unavailable reason codes.

These absences do not authorize immediate implementation. They become inputs to bounded implementation packages after RWP-00.81 owner approval.

## Duplicate or obsolete candidates

### Duplicate/overloaded

- `all_layouts` is overloaded across unrelated routes.
- `pos_integration` duplicates commercial and route capability identity.
- `happy_hour` and `video_wall` duplicate access and enabled/configured state.
- `multi_location` overlaps authorized context and advanced portfolio outcomes.
- screen/venue limits, feature limit strings, layout capacity, and HaaS terms are all described as limits despite different semantics.

### Potentially dormant or incomplete

- `staff_app` has presentation metadata but no separate Back Office route or confirmed current consumer in the inspected inventory.
- `white_label` and `html_editor` have broad catalog labels without a normalized bounded outcome contract.
- `bilingual_display` and `ai_translation` do not distinguish manual language support from automated service.

A later implementation package must verify server/data consumers before deleting or migrating any key.

## Permission represented as entitlement

The current flat session capability model can make permission and entitlement look identical to the browser. The following must be separate in the target contract:

- organization has capability access;
- venue/object is within attachment scope;
- user has action permission;
- object is in a compatible state;
- required add-on is attached/configured/connected;
- usage remains within allowance;
- privacy/rights/safety policy permits action;
- rollout/support state permits operation.

The final decision is the intersection of these independent evaluations, not a single feature boolean.

## Product state represented as feature flag

The target model must prohibit feature keys for:

- sold-out, unavailable, closed, paused, delayed, canceled, relocated, limited, unknown, or reopening state;
- configured/enabled status of a video wall, schedule, integration, promotion, playlist, or screen;
- source connected/stale/conflicting/overridden status;
- screen online/offline/delivery/revision state;
- subscription or HaaS provider status;
- current venue context.

These values may disable an action but require product-state guidance, not upgrade copy.

## Organization and venue inheritance gaps

The current product supports authorized venue contexts and organization-level screen/venue usage, but the inspected contracts do not expose a general inheritance model for capabilities, add-ons, limits, templates, terminology, sources, or local overrides.

Recommended target behavior:

- organization commercial access is inherited unless an add-on or capability explicitly attaches locally;
- permissions remain actor-and-scope specific;
- organization defaults seed local state but never silently overwrite local content or authority;
- local overrides are explicit, reversible, and show inherited versus effective value;
- mixed-industry venues retain local terminology and object models while sharing canonical capability IDs;
- limit consumption identifies counting unit, scope, pool, and responsible objects;
- downgrade and removal preserve active screens, content, history, source configuration, and export/recovery options according to RWP-00.79 policy.

## Target decision contract recommendation

A future server response should resolve each capability for a specific actor and scope with fields conceptually equivalent to:

- stable capability ID and outcome;
- `included`, `commercialAccess`, and required tier/add-on reference;
- `permitted` and required action permission;
- compatible product state;
- add-on attachment/configuration/connection/source freshness;
- typed limit, consumed quantity, remaining quantity, and enforcement mode;
- privacy/rights/safety restriction;
- rollout/support availability;
- effective organization/venue inheritance and local override;
- one primary reason code, supporting reason details, safe actions, and recovery;
- server-authoritative timestamp/version.

This is an architecture recommendation, not an API implementation specification.

## Impeccable locked-state guidance

Future surfaces must not show every denial as a padlock or upgrade. Use distinct, accessible states:

- **Upgrade available** — explain outcome, current access, target package/add-on, and hosted review.
- **Ask an administrator** — permission restriction with responsible role/scope.
- **Resolve usage** — limit reached with counted objects and safe remediation.
- **Connect or repair source** — add-on/configuration/connection/freshness problem.
- **Update operating state** — sold out, closed, paused, delayed, or other domain condition.
- **Unavailable here** — unsupported context/industry with neutral fallback.
- **Restricted** — privacy, rights, safety, or policy boundary without commercial upsell.
- **Temporarily unavailable** — internal rollout/support condition with retry/status guidance.

Every state requires visible text, matching accessible name, persistent labels, specific verb-object actions, mobile/desktop behavior, keyboard/focus support, long-name/localization resilience, and a recovery path.

## Recommended remediation sequence

1. Define stable canonical capability IDs and a typed decision/reason model.
2. Separate essential manual core from advanced automation in `quick_update`, `meal_periods`, language, and Happy Hour behavior.
3. Separate route visibility from operation permission and commercial access.
4. Decompose overloaded keys, especially `all_layouts`, `multi_location`, `pos_integration`, `video_wall`, and `happy_hour`.
5. Introduce typed add-on attachment/connection and typed limit decisions.
6. Introduce organization/local inheritance and override records.
7. Migrate browser catalog metadata to server-managed capability/tier/add-on presentation while preserving legacy key compatibility.
8. Update locked/unavailable surfaces to consume reason codes.
9. Verify and retire dormant keys only after consumer and data migration analysis.
10. Add industry object/state capabilities in completion-track order after owner approval.

## Handoff

RWP-00.78 uses this reconciliation to propose unified customer-outcome tier bundles and an independent add-on catalog. It must preserve the normalized core, keep permissions and state separate, avoid pricing values, and carry all unresolved owner decisions forward.

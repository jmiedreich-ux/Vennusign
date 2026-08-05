# Cross-Industry Customer Journey Validation

## Status

RWP-00.80 validates representative customer journeys against the normalized capability model, factual inventory, reconciliation, tier/add-on architecture, and limits/inheritance policy. It records planning gaps only and does not implement UI or product behavior.

## Overall result

**PASS WITH IMPLEMENTATION GAPS RECORDED.**

The consolidated architecture supports a coherent end-to-end experience across all six native profiles when the following contracts are honored:

- first value before forced pricing or integration setup;
- one canonical capability model with industry-aware presentation;
- independent permission, entitlement, product-state, add-on, limit, privacy/rights, and rollout decisions;
- server-authoritative access and provider confirmation;
- explicit target, source, freshness, delivery, and recovery state;
- typed limits and visible inheritance/override sources;
- essential manual operation and recovery preserved through failure, downgrade, and add-on removal.

The principal implementation gap is the absence of a normalized server capability-decision/reason contract and corresponding UI state system. Existing product flows can serve as migration foundations but do not yet express the full cross-industry model.

## Journey 1 — Signup, organization, industry and subtype

### Goal

A new customer establishes identity and a minimal real organization/venue context without making commercial access depend on industry selection.

### Validated flow

1. Create or recover a customer account.
2. Create/select an organization.
3. Add the first venue/property/operation using a neutral label appropriate to the chosen industry.
4. Select a primary industry and one primary subtype, with optional descriptive traits and neutral fallback.
5. Explain that industry/subtype changes terminology, recommendations, starter content, screen-purpose suggestions, and dashboard emphasis—not plan access.
6. Save progress and allow return/resume.

### Required states

Loading, invitation, existing organization, duplicate name, permission denied, invalid/expired session, unsupported subtype, neutral fallback, save conflict, offline/interruption, and successful context creation.

### Gaps/recommendations

- Add canonical industry/subtype state and effect-preview contracts.
- Preserve content, screens, permissions, limits, add-ons, sources, and history when changing selection.
- Avoid asking pricing/integration questions before the customer has a meaningful venue context.

## Journey 2 — First-screen onboarding and first value

### Goal

Reach one verified useful screen quickly.

### Validated flow

1. Ask only the minimum venue context, screen purpose, and local terminology.
2. Pair a new screen, select an authorized existing screen, or deliberately defer pairing with clear consequences.
3. Offer industry/subtype-aware starter content and one real operating update.
4. Preview the exact intended target.
5. Publish explicitly.
6. Show server-authoritative request/delivery state without equating API acceptance, pairing, online, received, and applied.
7. Provide retry, correction, and restore.
8. Introduce plan/add-on discovery contextually after one useful confirmed outcome.

### Required states

No screen, pairing code expired, paired but offline, unauthorized target, stale screen, publish pending, partial delivery, failed, applied, recovered, no acknowledgement available, save/resume, and manual fallback.

### Gaps/recommendations

- Preserve current pairing/delivery-state foundations.
- Add normalized core capability decisions so essential first-screen actions are never trapped by legacy advanced keys.
- Keep pricing secondary; explain upgrades only for genuinely advanced outcomes.

## Journey 3 — Daily industry-aware operation

### Goal

An operator makes a time-sensitive public update from mobile or desktop.

### Validated examples

- Restaurant/Bar: mark an item or tap sold out, change a special, hours, event, doors, entry, or last-call information.
- Café: update batch, freshness, sold-out, next-batch, expected return, preorder, pickup, or service-period information.
- Food Truck: update item/service-point/operation availability, current location/event, queue/pickup guidance, closure, relocation, or reopening.
- Hospitality: update property/amenity/outlet/service/event/meeting-space hours, state, notice, wayfinding, relocation, or urgent public information.
- Entertainment: update program/session/attraction/exhibit, delay, queue/wait/capacity/admission guidance, route, closure, cancellation, relocation, or reopening.

### Validated flow

1. Establish current scope and object.
2. Show authoritative current value, source, freshness, local override, and affected screens.
3. Use a specific verb-object action.
4. Validate timing, wording, scope, privacy/rights, and target.
5. Preview and publish.
6. Confirm intended versus delivered result.
7. Support correction, expiry, supersession, unpublish, undo, and restore.

### Gaps/recommendations

- Expand the current Restaurant-oriented object model into canonical industry object/state contracts.
- Separate product state from feature locks.
- Preserve essential rapid manual operation in Operate.

## Journey 4 — Permission-restricted action

### Goal

A user understands why an action is unavailable and who can resolve it.

### Validated flow

1. Show the content/context where safe.
2. State that the user lacks the specific action permission for the specific scope.
3. Identify the responsible role or administrator without exposing sensitive membership.
4. Allow safe navigation, copy/reference, or request workflow where approved.
5. Recheck server authority after role/context changes.

### Gaps/recommendations

- Replace flat browser capability inference with per-action permission decisions.
- Never show an upgrade prompt for a permission problem.
- Support mixed organization/venue permissions and local authority.

## Journey 5 — Source disconnect, stale data and manual fallback

### Goal

Operations continue safely when an external source or add-on is unavailable.

### Validated flow

1. Show source identity, last successful refresh, freshness/coverage, current connection state, and affected values.
2. Distinguish disconnected, unauthorized, configuration error, stale, conflicting, rate-limited, provider incident, and unsupported conditions.
3. Preserve last-known-good value with clear age/authority.
4. Offer permitted local override/manual operation.
5. Show conflict resolution and whether local values will be overwritten after reconnect.
6. Reconnect/retry and verify source recovery.
7. Restore or intentionally retain local override.

### Gaps/recommendations

- Add typed add-on attachment/configuration/connection/source decisions.
- Do not describe disconnect as a missing tier when the add-on is commercially attached.
- Make manual fallback core.

## Journey 6 — Upgrade software tier

### Goal

A customer reviews a higher native software outcome without deceptive urgency.

### Validated flow

1. Begin from an advanced outcome the customer attempted or explored.
2. Explain current access and the target outcome archetype.
3. Show gained native capabilities, changed allowances, prerequisites, and effective timing.
4. Distinguish any separately required add-on.
5. Preserve in-progress work.
6. Recheck eligibility server-side.
7. Open hosted Checkout/Billing Portal.
8. Treat return state as informational.
9. Refresh provider/server-authoritative access and show pending, applied, stale, canceled, or error state.

### Gaps/recommendations

- Replace hard-coded browser tier/feature catalog with server-managed stable capability metadata.
- Preserve existing hosted billing and webhook authority.
- Hide detailed pricing pressure until first value, while always allowing voluntary Billing access.

## Journey 7 — Attach and configure an add-on

### Goal

A customer purchases or activates an external/managed outcome at the correct scope.

### Validated flow

1. Explain add-on outcome, provider/region/rights eligibility, prerequisites, attachment scope, limits, privacy, support responsibility, and manual fallback.
2. Confirm organization/venue/object scope.
3. Complete commercial action where required.
4. Assign an authorized administrator.
5. Configure provider/source without exposing secrets.
6. Connect and validate identity, freshness, coverage, and first synchronization.
7. Resolve conflicts/overrides.
8. Monitor connection and recovery.

### Gaps/recommendations

- Introduce typed add-on instance and attachment records.
- Keep commercial status, configuration, connection health, source state, permission, and consumption separate.
- HaaS remains a separate contract/service flow.

## Journey 8 — Limit warning and limit reached

### Goal

A customer understands usage and resolves a constraint without losing public operation.

### Validated flow

1. Show typed unit, scope, included/consumed/reserved/remaining quantity, pool mode, counted objects, and calculation time.
2. Explain whether the attempted action is informational, warning, soft-stopped, hard-stopped, read-only, provider-metered, or contract-managed.
3. Offer least-destructive actions: archive/reassign/reduce/export/repair, review an allowance extension or tier, or contact the responsible administrator/support.
4. Preserve correction, unpublish, public-output safety, and recovery.
5. Recalculate after remediation.

### Gaps/recommendations

- Replace string `limitValue` with typed decisions.
- Distinguish subscription allowance from layout capacity and contract term.
- Add object-level usage detail and pooling source.

## Journey 9 — Downgrade software tier

### Goal

A customer safely reviews and completes a downgrade.

### Validated flow

1. Recalculate target capability and allowance decisions.
2. Show lost advanced outcomes, typed usage conflicts, consuming objects, inheritance/local override impact, active-screen/public-output risk, scheduled work, history/export/retention effects, and attached add-on prerequisites.
3. Offer remediation, scheduled downgrade, grace/read-only, export, or cancellation according to approved policy.
4. Recheck immediately before hosted provider action.
5. Apply only after server/provider confirmation.
6. Preserve essential core, safe correction, unpublish, active-screen protection, and recovery.

### Gaps/recommendations

- Current screen/venue checks are a good starting point but require generalized typed conflict handling.
- Do not automatically delete, unpublish, disconnect, or flatten local overrides.

## Journey 10 — Remove an add-on

### Goal

A customer understands the effect of stopping an external or managed service.

### Validated flow

1. Show effective date, provider disconnect, source freshness, dependent workflows/content/analytics, manual fallback, retained configuration, export/retention/deletion, reconnect path, hardware/service obligations, and support state.
2. Resolve active conflicts and overrides.
3. Confirm removal with exact scope.
4. Revoke credentials and stop synchronization at the authoritative time.
5. Preserve customer-authored content and core manual operation.

### Gap

Current product needs a normalized removal/reconnect contract per add-on family.

## Journey 11 — Multi-venue and mixed-industry organization

### Goal

An authorized operator switches context or governs multiple sites without losing local meaning.

### Validated flow

1. Show organization and current venue/property/operation persistently.
2. Switch only to server-authorized contexts with destructive-review guidance for unsaved work.
3. Preserve local industry terminology, timezone, objects, content, state, sources, add-ons, and screens.
4. Show inherited defaults, local overrides, exceptions, and effective values.
5. Use neutral canonical terms in shared portfolio views with local labels where needed.
6. Preview safe bulk actions and report per-site mixed results and restoration.

### Gaps/recommendations

- Preserve current authorized venue-context switching.
- Add general inheritance/override/effective-value contracts.
- Separate basic authorized switching from paid Portfolio coordination.

## Journey 12 — Support exception and recovery

### Goal

Support grants a bounded exception or restores service without corrupting product state.

### Validated flow

1. Identify capability/allowance target, scope, current source, reason, customer impact, and approving authority.
2. Record start/expiry/review dates and precedence.
3. Explain the customer-safe effective result.
4. Audit changes and notify appropriate actors.
5. Revert/expire safely and recalculate access/usage.

### Gaps/recommendations

- Normalize support override records with scope, reason, expiry, audit, and precedence.
- Never use an entitlement override for sold-out, closed, stale, disconnected, failed-delivery, or other product/system state.

## Journey 13 — Privacy, rights, safety or unsupported restriction

### Goal

A user receives truthful restriction guidance without an inappropriate commercial upsell.

### Validated flow

1. State that the action/content/context is restricted or unsupported.
2. Give the permitted safe alternative or responsible review path.
3. Avoid exposing sensitive policy detail or implying that a higher tier bypasses the restriction.
4. Preserve accessible public communication and emergency/manual fallback within approved boundaries.

### Gap

The normalized decision contract needs typed privacy/rights/safety/unsupported reason families and safe presentation metadata.

## Cross-journey Impeccable validation

All future UI-facing implementation must meet:

- clear current scope and hierarchy;
- task-first and exception-first presentation;
- specific verb-object actions;
- persistent labels and matching accessible names;
- complete loading, first-use, empty, validation, permission, entitlement, add-on, limit, source, product-state, privacy/rights, rollout, partial, failure, success, and recovery states;
- safe confirmation for destructive/high-scope actions;
- intended versus effective result and authoritative timing;
- keyboard navigation, focus management, screen-reader semantics, reduced motion, high contrast, mobile/desktop responsiveness, 200% zoom, localization expansion, long customer names, and realistic dynamic content;
- approved Sky Blue administrative direction;
- no false urgency, hidden consequences, or generic lock for the wrong condition.

## Validation summary by architecture area

| Area | Result | Remaining implementation gap |
| --- | --- | --- |
| Industry/subtype setup | Pass | Canonical state/effect preview and preservation contract |
| First-screen onboarding | Pass | Core decision model and complete delivery states |
| Daily operation | Pass | Cross-industry object/state APIs and UI |
| Permissions | Pass | Per-action scoped permission decisions |
| Source/add-on recovery | Pass | Typed add-on/source decisions |
| Upgrade/provider flow | Pass | Server-managed capability catalog and outcome metadata |
| Limits | Pass | Typed allowance/usage/enforcement API |
| Downgrade | Pass | Generalized conflict, grace, retention, and active-screen policy implementation |
| Multi-venue/mixed industry | Pass | Inheritance/override/effective-value model |
| Support exceptions | Pass | Normalized exception records |
| Privacy/rights/restriction | Pass | Typed restriction reasons and governance implementation |
| Accessibility/responsiveness | Pass as planning contract | Focused implementation and validation per package |

## Residual owner decisions

The journeys remain dependent on owner decisions for final tier names/placement, pricing/trials/contracts, numeric allowances/pooling/overage/grace, add-on provider/prerequisite/service policy, retention/export/deletion, exception governance, privacy/rights/safety obligations, player/hardware service commitments, and implementation order.

## Handoff

RWP-00.81 assembles the final Track 0 owner decision package, explicit approval points, and recommended implementation-package sequence. It must not authorize implementation without owner approval.

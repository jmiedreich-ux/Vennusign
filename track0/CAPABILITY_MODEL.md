# Track 0 Capability Model

Every capability or related concern must have exactly one primary classification.

1. **Core capability** — essential product behavior required for a viable daily operation.
2. **Permission** — who may perform an action.
3. **Product/domain state** — the current state of a business object or operation.
4. **Tier entitlement** — commercial access bundled into a subscription tier.
5. **Independent add-on** — separately valuable or separately costly capability.
6. **Usage or quantity limit** — a count, allowance, retention window, storage amount, or consumption boundary.
7. **Internal rollout flag** — temporary operational control for rollout, migration, compatibility, or emergency disablement.

## Rules

- Industry and venue subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements.
- Essential daily operations must not be made impractical through tier gating.
- Permissions do not determine commercial access.
- Product state is not a feature flag.
- Limits are not capabilities.
- A capability may have secondary relationships, but only one primary classification.

## Canonical example

Manual item availability is a core operational capability acting on product state. Authorization determines who may change it. POS-driven automatic availability may be a tier-bundled integration or independent add-on. Item availability itself is not a commercial feature flag.

## Packaging discipline

Tier and add-on proposals remain candidates until cross-industry normalization and explicit owner approval. No Track 0 industry RWP authorizes billing, entitlement, feature-gate, API, schema, migration, or UI implementation.

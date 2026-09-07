# Vennue Product Delivery Process

## Purpose

Deliver complete, understandable customer workflows with less late design, rework and owner-testing labor. This process applies to every implementation track after Track 1. Architecture may be broad, but planning, implementation and acceptance must remain functionally focused.

## Product decision model

The following decisions are related but must remain distinct:

| Decision | Question |
|---|---|
| Release | When is the capability complete and supported? |
| Tier | Which subscribed customers receive it? |
| Add-on | Can it be purchased separately? |
| Rollout control | Which eligible customers receive it during deployment? |
| Product state | Is it available, preview, planned, unavailable or temporarily disabled? |

A version is not a tier. Each release may serve multiple tiers, and every offered tier must provide a coherent usable experience.

## Required stages and gates

### 1. Product Release Analysis

Before detailed track planning is complete:

- define the minimum complete customer experience for V1 and the intended boundaries for V1.1, V2 and later;
- map every committed capability to a release, tier treatment, optional add-on treatment, launch state and dependencies;
- distinguish commitments from tentative ideas;
- identify tier promises that depend on unavailable release work;
- decide how future or unavailable capabilities appear;
- obtain owner approval of the Release–Capability–Tier Matrix.

Output: an approved matrix based on `docs/templates/release-capability-tier-matrix.md`.

### 2. Functional Track Definition

Choose a customer-facing function or end-to-end workflow, not merely a shared architectural concern. Define “near complete” for that area, including:

- entry, happy, denial, error and recovery paths;
- every affected screen and navigation route;
- authority, entitlement, allowance and product-state behavior;
- desktop, mobile, keyboard and accessibility behavior;
- data/API dependencies and operational states;
- focused automation, internal usability review and owner checkpoint.

Foundation work may support several areas, but each functional slice remains the unit of progress and acceptance. Complete one slice before moving to the next unless the owner explicitly approves an overlap.

### 3. Front-End Design and Workflow Phase

Required before customer-facing implementation begins:

1. Document the user goal, starting point, steps and completion outcome.
2. Define information hierarchy, navigation, terminology and primary/secondary actions.
3. Cover loading, empty, success, validation, error, denial, offline/stale and destructive-action states where applicable.
4. Cover desktop, mobile, keyboard, focus and accessibility behavior.
5. Produce a reviewable repository artifact:
   - interactive HTML/CSS prototype when workflow or interaction matters;
   - PNG mockups for quick visual alternatives;
   - Markdown flow and state matrix in all cases.
6. Compare alternatives when the workflow is materially unclear.
7. Record owner feedback and explicit approval.

Store proposed work under `docs/design/proposed/`. On approval, place or clearly designate the authoritative package under `docs/design/approved/`. The approved workflow, prototype/mockup and state matrix together are implementation authority. Figma is optional and is not a dependency.

Implementation may start only when the relevant design package is approved. Minor visual refinement may continue later; workflow, hierarchy and major interaction decisions may not be silently redesigned during implementation.

### 4. Implementation and Validation

- Build one approved functional vertical slice at a time.
- Scaffold deterministic journey tests before or with implementation.
- Add the narrowest focused test at the owning layer for every behavior and correction.
- Keep hosted-agent lanes only for genuinely subjective judgments; invalid or truncated output fails.
- Run a product walkthrough after the first customer-visible slice.
- Require independent, complete review for large or high-risk changes.
- State proof boundaries, skipped tests and residual risk in every handoff.

### 5. Internal Readiness and Owner Acceptance

Before owner acceptance:

- deterministic and focused tests pass;
- no known mechanical failures remain;
- an internal product/UX review checks the approved design and owner examples;
- discrepancies are resolved or explicitly dispositioned;
- the owner receives a change-focused acceptance package covering changed/risk areas plus a stable smoke path.

Owner acceptance is a judgment and approval gate, not the first defect-discovery stage.

### 6. Retrospective and Closure

After acceptable owner testing, review planning, design, implementation, validation, rework, labor and acceptance. Record concrete next-track changes and their verification. A track closes only after owner acceptance is acceptable, the retrospective is approved, follow-up work is recorded, and controlled records are synchronized.

## Change control

- A requested change to an approved design is classified as clarification, minor refinement or workflow change.
- Clarifications and minor refinements update the approved package and affected acceptance criteria.
- Workflow changes return to the Front-End Design and Workflow Gate and require renewed owner approval before implementation continues.
- Moving a capability between releases, tiers or add-on treatment requires updating the matrix and checking dependent workflows.
- Do not expose partial future capability merely because underlying architecture exists.

## Required evidence per track

- approved Release–Capability–Tier rows for the scope;
- functional boundary and near-complete definition;
- approved design/workflow package;
- executable journey and test-ownership matrix;
- implementation PRs and exact-head validation;
- independent review record where required;
- internal readiness record;
- owner acceptance result;
- retrospective and closure decision.

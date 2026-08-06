# Track 1 Lessons Learned and Retrospective Report

**Track:** Track 1 — Capability, Entitlement and Authority Foundation  
**RWP:** [RWP-01.06 / issue #665](https://github.com/jmiedreich-ux/Vennusign/issues/665)  
**Report date:** 2026-08-06  
**Evidence cutoff:** 2026-08-06  
**Owner decision:** Pending — Approved / Needs Adjustment / Rejected

## Executive conclusion

The focused-track process was a clear improvement. Track 1 converted a broad, mixed feature-gating system into a typed capability, entitlement, permission, scope, allowance, add-on, rollout and product-state foundation. Five planned RWPs were completed sequentially, reviewed, merged and exact-head validated. The process produced stronger architecture, clearer ownership and better traceability than the earlier phase-based approach.

However, Track 1 was declared implementation-complete too early. The automated and hosted-agent acceptance gate reached 19 of 19 passing cases, while the owner result was 17 Pass, 1 Needs Adjustment and 2 Fail. Owner testing then exposed product defects, usability weaknesses and test-quality gaps that required substantial diagnosis, correction and repeated testing.

The principal lesson is not that more owner testing is needed. The owner was carrying work that should have been completed earlier by product-journey review, deterministic automation, focused unit tests and independent review. Track 2 should preserve owner acceptance as a judgment gate while sharply reducing its role as a defect-discovery and debugging stage.

## Outcome

Track 1 delivered:

- canonical action capabilities separated from permissions, product states, allowances, add-ons, layouts, rollout controls and navigation;
- structured server decisions with stable reasons, message keys, parameters, conditions and correlation IDs;
- scoped roles and assignments with server-side authority;
- typed entitlement, allowance, add-on, rollout and layout foundations;
- essential-core protection and fresh endpoint authorization;
- Back Office navigation and action presentation driven by server decisions;
- deterministic acceptance fixtures and a combined automated/hosted-agent acceptance runner.

This outcome is strategically sound and should be retained.

Track 1 is not yet closed. The current owner record remains 17 Pass, 1 Needs Adjustment and 2 Fail, and seven follow-up issues are marked for resolution before Track 2.

## Lifecycle and evidence timeline

| Stage | Evidence and result | Lesson |
|---|---|---|
| Track 0 and Track 1 planning | The product concepts were separated correctly and five Track 1 RWPs were approved with completeness matrices. | The focused-track model improved architectural coherence. |
| RWP-01.01–01.04 | PRs #645–#648 implemented the model, server decision contract, scoped authority and gate replacement. Exact-head Actions passed for each. | Sequential, bounded implementation and per-RWP validation worked. |
| RWP-01.05 | PR #650 performed combined validation and fixed screen-capacity authority, session decision projection and customer-copy gaps. | A combined-track validation package caught cross-RWP integration gaps and should remain mandatory. |
| Initial handoff | RWP-01.05 stated that no additional RWP was required by automated validation and passed 108 Back Office tests plus the full non-integration Actions gate. | Green component and contract tests were treated as stronger proof of customer readiness than they actually provided. |
| Acceptance automation and rework | PR #654 added Playwright automation and fixed menu updates, API-host termination, pooled-connection isolation, UI layout, refusal rendering, menu-save timing, screen-status heartbeats, dialog focus and mobile navigation. Latest results reported 56 UI, 108 Back Office and 102 Platform Operations tests passing. | Acceptance automation was built after implementation and immediately uncovered defects that should have had earlier test ownership. |
| Automated/agent acceptance | All 19 cases passed: 14 deterministic Playwright cases and 5 hosted-agent cases. | The gate was useful but still overestimated owner readiness. |
| Owner acceptance | Owner result: 17 Pass, 1 Needs Adjustment, 2 Fail; overall Needs Adjustment. | Mechanical correctness and agent judgment did not fully represent usability, product intent or real owner interaction. |
| Post-acceptance review | Issues #656–#662 recorded seed-pruning risk, missing focused tests, generated-output ambiguity, incomplete independent review, remaining agent-dependent localization, hosted-agent cost truncation and duplicate test hooks. | Test infrastructure, review independence and repository policy needed their own readiness gate before owner testing. |

## What worked and should continue

1. **Focused tracks and explicit boundaries.** Track 1 stayed centered on capability, entitlement and authority instead of mixing unrelated product areas.
2. **Sequential RWPs.** The order from canonical model to server decisions, scoped authority, replacement and combined validation was logical and reduced uncontrolled parallel change.
3. **Architecture before implementation.** The distinction among capabilities, permissions, product states, allowances, add-ons and rollout controls corrected a major source of product inconsistency.
4. **Server authority.** Moving final decisions to the server and returning structured reasons provides a durable foundation for UI consistency.
5. **Exact-head validation.** Each RWP had identifiable PRs, commits and Actions runs, making the implementation auditable.
6. **A combined validation RWP.** RWP-01.05 found cross-package gaps that per-RWP checks did not.
7. **Prepared fixtures and direct acceptance journeys.** Deterministic accounts, links and expected outcomes made eventual testing reproducible.
8. **Owner judgment remained authoritative.** The process did not automatically close the track because automated and agent QA was green.
9. **Late findings were recorded rather than hidden.** Issues #656–#662 preserve technical and process debt for explicit action.

## Material findings and root causes

### 1. Implementation completeness was measured mainly by layers, not by coherent customer journeys

**Evidence:** The planning completeness standard named UI, navigation, actions, states, responsive behavior and customer journeys. Despite this, acceptance uncovered menu-save failure, screen status being changed by thumbnails, generic refusal errors, missing mobile collapse behavior and focus problems.

**Root cause:** The completeness matrix was broad but not tied to a small set of executable end-to-end journeys owned from the beginning of each RWP. Individual layers could pass while the full behavior remained wrong.

**Labor effect:** The same areas had to be rediscovered, diagnosed, fixed and retested late.

**Required change:** Every Track 2 RWP must name affected owner journeys and provide an executable happy path, denial path, recovery path and mobile/keyboard path before implementation begins.

### 2. Acceptance automation arrived after implementation

**Evidence:** PR #654 added Track 1 Playwright acceptance automation after RWPs 01.01–01.05 were already merged and then fixed multiple product defects.

**Root cause:** The test plan described what automation should prove, but deterministic acceptance cases were not required to exist before or alongside implementation.

**Labor effect:** Defects accumulated across the five-RWP batch and were discovered together, increasing diagnosis and regression scope.

**Required change:** Write or scaffold deterministic acceptance cases before each Track 2 implementation RWP. A package cannot be considered complete until its affected journey cases pass.

### 3. Test-layer ownership was incomplete

**Evidence:** Issue #657 records three server defects covered only through Playwright: RepoDb mappings, POS worker resilience and isolation-level behavior.

**Root cause:** End-to-end coverage was accepted as sufficient proof for defects whose natural ownership is focused server or data-access tests.

**Labor effect:** Failures were slower to isolate and future regressions would be harder to diagnose.

**Required change:** Every behavioral correction must add the narrowest stable automated test at its owning layer, with end-to-end coverage retained only for the customer contract.

### 4. Automated and agent QA did not match the owner's definition of acceptable

**Evidence:** The automated/agent result was 19 of 19 pass while the owner result was 17 Pass, 1 Needs Adjustment and 2 Fail.

**Root cause:** The gate combined mechanical assertions with hosted-agent judgments, but it lacked an explicit calibration pass against owner expectations for clarity, workflow usefulness and visual/product quality.

**Labor effect:** A green gate created a false readiness signal and owner testing became the first reliable assessment of higher-level usability.

**Required change:** Before Track 2 implementation, create owner-approved examples and rejection criteria for subjective outcomes. Before formal owner acceptance, run a short internal product-journey review against those examples.

### 5. Subjective hosted-agent testing was too expensive and not fully reliable

**Evidence:** Issue #661 records a model lane truncated by its cost cap while returning a completed state without the required JSON. Issue #660 shows a mechanical localization case still assigned to an agent.

**Root cause:** Mechanical and subjective responsibilities were not separated aggressively enough, and lane success was inferred too readily from provider completion.

**Labor effect:** Failed lanes required reruns and manual interpretation; operating cost and timing became unpredictable.

**Required change:** Move every deterministic case to Playwright. For remaining subjective lanes, validate the response contract, enforce a cost/model policy, and treat truncation or invalid JSON as a failed gate.

### 6. Independent review was not genuinely independent or complete

**Evidence:** Issue #659 states the PR #654 review was performed by the authoring agent, a nonexistent preflight command was skipped, and only a small portion of the roughly 2,300-line change was read.

**Root cause:** The process recorded a review activity without proving reviewer independence, full diff coverage or completion of required checks.

**Labor effect:** Risks survived into the post-merge period, and the confidence attached to the review was overstated.

**Required change:** Large or high-risk Track 2 changes require a non-author review record containing reviewed SHA, files or areas covered, commands actually run, findings and residual risks. A failed or partial review cannot satisfy the gate.

### 7. Test infrastructure and repository policy were not ready before acceptance

**Evidence:** Issue #656 identifies unsafe name-pattern seed pruning; #658 identifies contradictory treatment of generated display output; #662 identifies duplicated test selectors.

**Root cause:** The acceptance harness was implemented under time pressure as part of the late QA cycle rather than treated as a maintained product with safety and consistency requirements.

**Labor effect:** Review produced new cleanup work and created risk that future tests will be fragile or destructive.

**Required change:** Track 2 needs a test-harness readiness checklist covering explicit seed identity, isolated databases, idempotent reset, unique selectors, generated-output policy and reproducible local startup.

### 8. The batch size amplified late discovery

**Evidence:** Five RWPs were completed before one owner review. Cross-RWP validation helped, but acceptance then uncovered failures spanning data access, worker resilience, UI state, timing, responsive behavior and accessibility.

**Root cause:** The batch had no lightweight owner/product checkpoint after the first real vertical slice.

**Labor effect:** Corrections occurred after a large surface had accumulated, forcing wider regression testing.

**Required change:** Keep sequential batches of up to five, but add a short non-acceptance product walkthrough after the first customer-visible vertical slice and whenever a later RWP materially changes it.

### 9. “No additional work required” was stated too strongly

**Evidence:** RWP-01.05 recorded that automated validation required no additional RWP; subsequent acceptance and review produced major fixes and seven before-Track-2 issues.

**Root cause:** Documentation did not clearly distinguish “no gaps found by this test boundary” from “customer-ready and complete.”

**Labor effect:** Status language encouraged premature confidence and complicated closure decisions.

**Required change:** All future handoffs must state the proof boundary, untested risks and confidence level. Only owner approval plus retrospective closure may use the word complete for a track.

## Labor drivers

The repository does not provide reliable person-hour totals, so this report does not invent them. The avoidable labor is evidenced by:

- building acceptance automation after implementation;
- diagnosing and correcting at least nine distinct defects during QA;
- repeating affected Playwright, Back Office, Platform Operations and owner journeys;
- rerunning a hosted lane after cost-cap truncation;
- reviewing a large QA PR after merge and creating seven additional issues;
- reconciling disagreement between green automated/agent results and owner outcomes;
- maintaining both manual/agent and deterministic coverage for cases that should be deterministic.

Track 2 should record cycle counts and effort at each gate so the next retrospective can quantify improvement.

## Required Track 2 adoption matrix

| Improvement | Adoption point | Owner | Verification |
|---|---|---|---|
| Define owner-visible outcomes, examples and rejection criteria | Before detailed planning completes | Product owner + planner | Owner approval recorded in Track 2 plan |
| Map every RWP to happy, denial, recovery, mobile and keyboard journeys | Before implementation | RWP author | Completeness matrix contains executable cases and links |
| Scaffold deterministic acceptance tests with the RWP | Before implementation | Implementer | Tests exist or are explicitly marked pending against approved behavior |
| Add focused tests at the owning layer for every behavior/fix | Before RWP completion | Implementer | Review maps each behavior to unit/contract/E2E ownership |
| Perform a first-vertical-slice product walkthrough | During implementation, before the batch accumulates | Product owner + implementer | Findings recorded and resolved/scoped |
| Validate test-harness safety and reproducibility | Before automated validation | QA owner | Isolated database, explicit seed IDs, idempotent reset, unique selectors and startup proof |
| Enforce independent, complete review for large/high-risk changes | Before merge | Non-author reviewer | Reviewed SHA, coverage, actual checks, findings and risks recorded |
| Move all mechanical cases out of hosted-agent lanes | Before automated validation | QA owner | Agent lanes contain only subjective judgments |
| Validate hosted-agent contracts and cost policy | Before automated validation | QA owner | Invalid/truncated output fails; model and cap are explicit |
| Run an internal customer-journey readiness review | Before owner acceptance | Product/UX reviewer | No known mechanical failures; subjective findings dispositioned |
| Give owner a change-focused acceptance package | Before owner acceptance | QA owner | Owner retests changed/risk areas plus a short smoke path, not the entire internal matrix |
| Report evidence boundaries and confidence honestly | Every handoff | RWP author | Handoff lists passed, skipped, unproven and residual risks |
| Record cycles and effort | Throughout Track 2 | Track coordinator | Per-RWP counts for test runs, defects, rework and owner cycles |
| Resolve #656–#662 | Before Track 2 implementation | Track 1 follow-up owners | Issues closed with validated changes or explicitly ruled out by owner |

## Existing follow-up work before Track 2

- [#656](https://github.com/jmiedreich-ux/Vennusign/issues/656) — make seed pruning explicitly identify test data.
- [#657](https://github.com/jmiedreich-ux/Vennusign/issues/657) — add focused tests for Track 1 server defects.
- [#658](https://github.com/jmiedreich-ux/Vennusign/issues/658) — decide and enforce the generated display-output policy.
- [#659](https://github.com/jmiedreich-ux/Vennusign/issues/659) — complete an independent review of PR #654.
- [#660](https://github.com/jmiedreich-ux/Vennusign/issues/660) — move localization case 5-0 to Playwright.
- [#661](https://github.com/jmiedreich-ux/Vennusign/issues/661) — resolve hosted-agent model/cost-cap behavior.
- [#662](https://github.com/jmiedreich-ux/Vennusign/issues/662) — make screen test hooks unique.

The 1 Needs Adjustment and 2 Fail owner results must also be individually linked to resolved changes or an explicit accepted disposition before Track 1 closure.

## Recommended Track 2 process

1. Agree on the intended customer outcome and explicit non-goals.
2. Approve representative owner-visible examples and rejection criteria.
3. Build the completeness and journey matrix.
4. Assign each requirement to its narrowest test layer.
5. Scaffold deterministic journey tests and safe fixtures.
6. Implement sequentially.
7. Validate the first customer-visible vertical slice with a short product walkthrough.
8. Complete focused, contract and end-to-end tests for each RWP.
9. Run independent review and exact-head Actions.
10. Perform a combined-track journey and architecture validation.
11. Conduct an internal product/UX readiness review.
12. Give the owner a concise, change-focused acceptance package.
13. Resolve findings and retest only the affected risks plus a stable smoke suite.
14. Produce the retrospective and obtain explicit closure approval.

## Owner decision

Select one:

- **Approved** — the findings and Track 2 adoption matrix are accepted.
- **Needs Adjustment** — specify required changes to this report.
- **Rejected** — provide the reason and required replacement direction.

Owner decision: **Pending**

Track 1 remains open until the owner acceptance findings are acceptably resolved, this retrospective is approved, required follow-up work is recorded or dispositioned, controlled records are synchronized and the owner explicitly approves Track 1 closure.

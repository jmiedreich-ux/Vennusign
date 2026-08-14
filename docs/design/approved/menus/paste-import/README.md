# Menus paste import — approved design authority

Approved by the owner on 2026-08-13 for Menus Milestone 6-A.

## Authority order

1. `../decisions.md`, including decisions 33 and 37–43, governs behavior.
2. `paste-import-storyboard-v4.pptx` is the approved customer-flow storyboard.
3. `confirm-sequence.mmd` is the editable technical confirmation sequence; `confirm-sequence.png` is its rendered reference.
4. `paste-import-flow.png` is the compact customer-flow orientation image.

Where an older Menus artifact disagrees with this bundle, this bundle wins. Values shown as examples in the storyboard—especially expiry times—are illustrative unless a tier/configuration default is separately approved.

## Fixed product behavior

- Paste is read into a persisted import session. Nothing creates or replaces menu working rows before final confirmation.
- Review shows unresolved identity and parsing decisions first. Already-settled inventory is available through progressive disclosure.
- Only case, punctuation, and spacing normalization may match automatically. Semantic similarity is always an operator decision; no near-match row is preselected.
- Every pasted line remains traceable. Unresolved content lands once in `Imported items`; parser-cause metadata remains available for explanation and diagnostics.
- The operator chooses **Create a new menu** or **Replace an existing menu** after review. Replacement locks and rechecks the target only at confirmation.
- Confirmation is one atomic, idempotent transaction. Replacement preserves menu identity, theme, assignments, published snapshot, and active availability/86 state. Screens remain unchanged until a later Publish.
- Completion says **Not live yet** and offers **Review draft in builder** or **Done for now**. It never implies automatic navigation or publication.
- At widths below the supported 900px floor, the flow refuses compression, preserves the session, shows its resolved absolute expiry, and offers a resumable wider-window handoff.
- Keyboard-specific interaction design and testing are excluded for this milestone. Semantic controls, accessible names and relationships, visible focus, and screen-reader-compatible status/error announcements remain required.

## Owner-approved product decisions

- Preserve review answers whose source line and candidate identity remain unchanged; invalidate only dependency-affected answers and explain what was cleared.
- Compute replacement's unpublished-change delta on the server against the published snapshot and expose a plain-language breakdown.
- Keep one customer-facing `Imported items` fallback section; retain per-line parser reason codes.
- Allow an eligible unreadable line to become a section only through an explicit reversible review action.
- Preserve all historical replacement snapshots. A centralized configuration table resolved by subscription tier controls stored scope, retention, restore eligibility, and tier limits.
- Resolve import-session retention from the same configuration/tier model. Only a successful user mutation may renew expiry; passive reads do not.
- Paste import never silently changes another menu's price. A shared-price model must create a menu-scoped override/copy or refuse until the operator explicitly chooses a broader action.

## Implementation boundary

This bundle approves design; it does not claim implementation. Before coding, synchronize the milestone issue, tracker claim, branch, acceptance workbook, and test/path matrix. The implementation must validate refresh/resume, target conflict, lock loss, permission and tier changes, expiry, retry/idempotency, maximum-size refusal, and new versus replacement outcomes.

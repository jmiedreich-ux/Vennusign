# RWP-00.08 — Destructive-Action Confirmation Standardization

## Outcome

Back Office and Platform Operations use one accessible consequence-review dialog contract instead of native browser confirmation prompts. Every destructive or high-impact action names the target, explains the result, and offers explicit cancel/confirm controls; irreversible screen unpairing additionally requires the exact screen name.

## Required implementation

- Replace all `window.confirm` calls in both admin applications.
- Use one matching review-dialog implementation and presentation contract in each independently built admin bundle.
- Preserve the existing action, API, error, success, authorization, tenant, and entitlement behavior behind each confirmation.
- Provide title, consequence, action-specific confirmation label, safe initial focus, Escape cancellation, and visible keyboard focus.
- Require exact-name typed confirmation for screen unpairing only.
- Add source-contract coverage that forbids browser prompts and guards the typed-unpair boundary.

## UI and function gap analysis

- **Goal and hierarchy:** the target and decision appear first, followed by concrete impact and the action controls. The review surface is modal so the user cannot accidentally continue interacting with the underlying form.
- **Navigation and required actions:** Cancel receives initial focus and closes without mutation; the action-specific confirmation performs the existing operation. Escape maps to Cancel. No application route or surrounding navigation changes.
- **Essential states:** closed, open, caution, danger, typed-confirmation incomplete, typed-confirmation complete, action busy, existing inline success, and existing inline error states are covered. Concurrent review requests safely cancel an older unresolved request.
- **Validation:** the dialog connects title and consequence with `aria-labelledby` and `aria-describedby`; focused tests require native-dialog modality, Escape handling, initial safe focus, exact typed matching, identical cross-app implementations, and zero `window.confirm` use.
- **Destructive actions:** screen replacement, archive, reset, unpair, passkey removal, wall removal, playlist removal, promotion archive, meal-period deletion, tap deletion, theme reset, emergency activation/cancellation, configuration rollback/import/clear, and venue switching all disclose their specific consequences. Unpair confirmation remains disabled until the exact screen name is entered.
- **Feedback:** API calls still begin only after confirmation and retain their existing localized success/error messages. Cancellation causes no request, optimistic state, or global notification.
- **Accessibility:** native `<dialog>` modality, programmatic name/description, visible focus ring, safe initial focus, keyboard submission, Escape cancellation, and non-color danger/caution text are included. The form remains usable at narrow viewport widths.
- **Responsiveness:** dialog width is capped but shrinks to the viewport with a 16-pixel outer margin. Actions wrap naturally through the existing flex layout without changing page breakpoints.
- **API, data, authorization, and entitlement support:** no endpoint, payload, persistence, authorization, tenant, or entitlement change is required. All existing server-side guards remain authoritative.

## Acceptance evidence

- Both admin bundles contain the same `DestructiveReviewDialog` and `useDestructiveReview` contract.
- No Back Office or Platform Operations TypeScript/JavaScript source uses `window.confirm`.
- Every previous native-confirmation call awaits the review result before calling its existing operation.
- Screen unpair passes `typedConfirmation: screen.name`; no other action requires typed confirmation.
- Existing focused action tests were updated to require the standardized review flow.

## Validation

- Back Office production build: passed locally.
- Back Office Node tests: 72 passed locally.
- Platform Operations production build: passed locally.
- Platform Operations Node tests: 89 passed locally.
- Patch whitespace validation: passed.
- Exact-head affected-area GitHub Actions is authoritative before merge.

## Skipped integration testing

Browser automation, live mutation verification, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Queue and boundaries

- Issue: #451.
- Branch: `rwp/00.08-destructive-review-dialogs`.
- RWP-05.08 / #452 becomes next only after this PR merges, issue #451 closes, `master` is verified, and the claim is released.
- Action placement/overflow ownership remains in RWP-00.13; transient feedback remains in RWP-00.09.
- RWP-13.06 / #466 remains held; Phase 14+ remains paused.

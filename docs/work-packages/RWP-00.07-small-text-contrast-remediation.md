# RWP-00.07 — Small-Text Contrast Remediation

## Outcome

Small white-surface help text in both admin applications meets WCAG AA, and locked navigation remains fully readable instead of lowering the opacity of labels, descriptions, and tier badges. Locked entries retain text and add an explicit non-color lock cue.

## Required implementation

- Replace the failing `#71827B` small-text step (`4.05:1` on white) with the shared Sky muted-text token (`#64748B`, at least `4.5:1` on white).
- Remove container opacity from Back Office locked links and shared locked-navigation controls.
- Preserve disabled-control opacity because it communicates native interaction state and is outside the locked-navigation finding.
- Add a visible lock cue without changing the upgrade action, tier badge, route capability, or entitlement behavior.
- Add focused computed-contrast and source-contract tests for both admin applications.

## UI and function gap analysis

- **Goal and hierarchy:** small descriptions remain subordinate but readable. Locked feature titles, descriptions, and tier badges retain their normal hierarchy instead of being uniformly faded below AA.
- **Navigation and required actions:** every existing locked route and upgrade-preview button stays in the same order and remains actionable through the existing upgrade context. A lock cue reinforces the already-present locked meaning; no route or action is added or removed.
- **Essential states:** unlocked, active, locked, hover, focus, and disabled states remain distinct. This RWP removes opacity only from locked navigation; true disabled form controls keep their existing disabled presentation.
- **Validation:** computed WCAG contrast tests require the shared `#64748B` small-text step to meet `4.5:1` on white. Source guards reject the old failing color and any restored locked-container opacity.
- **Destructive actions:** no destructive action or confirmation changes. RWP-00.08 retains ownership of destructive dialogs.
- **Feedback:** locked entries continue opening the established upgrade context, and tier badges remain visible. No transient or error feedback changes; RWP-00.09 retains ownership of toasts.
- **Accessibility:** small text passes normal-text AA; locked status is conveyed through visible text/tier context and a lock cue, not color or opacity alone. The decorative lock is hidden from assistive technology because the adjacent text already supplies the meaning. Existing keyboard and focus behavior is preserved.
- **Responsiveness:** no layout dimension or breakpoint changes. The inline lock cue participates in the existing flex layout and remains adjacent to its title at narrow widths.
- **API, data, authorization, and entitlement support:** no endpoint, payload, persistence, authorization, tenant, or entitlement change is required. Existing capability evaluation and upgrade selection remain authoritative.

## Acceptance evidence

- All `#71827B` small-text declarations in affected admin sources use `var(--sky-small-text)`.
- `--sky-small-text` resolves to approved Slate-muted `#64748B`.
- Back Office `nav a.locked` and both `.locked-navigation-item` implementations no longer lower container opacity.
- Both shared locked-navigation components render the same visible lock cue and retain their tier badge and upgrade callback.
- Focused tests compute contrast and prevent regression to the old color or opacity pattern.

## Validation

- Back Office production build: passed locally.
- Back Office Node tests: 70 passed locally.
- Platform Operations production build: passed locally.
- Platform Operations Node tests: 89 passed locally.
- Patch whitespace validation: passed.
- Exact-head affected-area GitHub Actions is authoritative before merge.

## Skipped integration testing

Browser automation, hosted rendering, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Queue and boundaries

- Issue: #450.
- Branch: `rwp/00.07-small-text-contrast`.
- RWP-00.08 / #451 becomes next only after this PR merges, issue #450 closes, `master` is verified, and the claim is released.
- Broader Sky rollout, icons, destructive dialogs, and toast behavior remain in their separately ordered RWPs.
- RWP-13.06 / #466 remains held; Phase 14+ remains paused.

# RWP-00.06 — Shared Design Tokens and Palette Consolidation

## Outcome

Back Office and Platform Operations consume one shared Sky UI token contract covering the approved palette, semantic status colors, typography, spacing, radius, focus, and component foundations. Existing component styling remains intact for the later bounded visual rollout in RWP-00.12.

## Required implementation

- Encode the owner-approved Sky UI palette and contrast-safe action pairings in one token source.
- Provide semantic aliases for page, card, text, controls, actions, focus, and reserved status presentation.
- Provide shared typography, spacing, radius, and component-foundation scales.
- Import the same source into Back Office and Platform Operations without changing application behavior.
- Add focused contract tests that prevent palette drift and the prohibited white-on-sky primary action pairing.
- Preserve the boundary around the later Sky visual rollout, Midnight theme, contrast remediation, iconography, and action-hierarchy packages.

## UI and function gap analysis

- **Goal and hierarchy:** both admin applications previously declared unrelated page colors and type foundations, while individual components repeated raw values. The new shared contract establishes one named hierarchy: Ice/white surfaces, Slate text and anchors, Sky fills/focus, Cyan secondary states, and reserved status colors.
- **Navigation and required actions:** this foundation does not restructure navigation or introduce actions. It gives later navigation and action packages stable semantic aliases without changing current routes, labels, or capability visibility.
- **Essential states:** tokens cover default, raised, hover, focus, live, off, warning, emergency, and promotion presentation. Status colors are explicitly reserved and must remain paired with text or an icon.
- **Validation:** focused tests verify every locked owner color, both application imports, semantic action pairing, focus, typography, spacing, and radius foundations. Both affected applications build and pass their Node suites.
- **Destructive actions:** no destructive control or data mutation is introduced. The later confirmation and action-hierarchy RWPs retain ownership of destructive presentation and behavior.
- **Accessibility:** primary action text is Midnight Slate on Sky (`10.25:1` in the approved reference); white-on-sky (`1.74:1`) is forbidden by a contract test. Sky is not exposed as light-surface text. Focus tokens define a three-pixel Sky outline with separation from the control edge. Semantic state colors are never intended as the only cue.
- **Responsiveness:** spacing and type tokens are relative units, radii are bounded, and both existing responsive shells continue to compile. No viewport-specific behavior changes in this foundation.
- **API, data, authorization, and entitlement support:** no endpoint, payload, persistence, authorization, tenant, or entitlement change is required. The contract is static CSS consumed at build time.

## Approved token source

- Sky Blue `#87CEEB`: primary fills, spines, and focus only.
- Ice White `#F8FAFC`: page surface; pure white remains the raised card surface.
- Midnight Slate `#0F172A`: primary text, anchors, and text on Sky.
- Soft Cyan `#E0F2FE`: hover, badge, tag, and secondary fills.
- Light Gray Blue `#E2E8F0`: borders and dividers.
- Reserved semantic colors: live `#178A52`, off `#B03A33`, warning `#C9871A`, emergency `#C22E26`, and promotion `#7C5CBF`.

## Acceptance evidence

- `src/back-office/src/sky-ui-tokens.css` is the shared token source.
- Back Office imports it directly; Platform Operations imports that same file.
- Both application roots consume the shared page, text, and font aliases.
- Contract tests reject white primary text on Sky and verify the locked palette.
- Component-level visual conversion remains explicitly outside this RWP and is owned by RWP-00.12.

## Validation

- Back Office production build: passed locally.
- Back Office Node tests: 68 passed locally.
- Platform Operations production build: passed locally.
- Platform Operations Node tests: 87 passed locally.
- Patch whitespace validation: passed.
- Exact-head affected-area GitHub Actions is authoritative before merge.

## Skipped integration testing

Browser automation, hosted rendering, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Queue and boundaries

- Issue: #449.
- Branch: `rwp/00.06-shared-sky-design-tokens`.
- RWP-00.07 / #450 becomes next only after this PR merges, issue #449 closes, `master` is verified, and the claim is released.
- RWP-00.12 owns the complete Sky visual rollout; RWP-00.11 owns the Midnight variant.
- RWP-13.06 / #466 remains held; Phase 14+ remains paused.

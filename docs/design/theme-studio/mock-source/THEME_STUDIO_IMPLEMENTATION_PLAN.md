# Vennue Theme Studio and Template Repair Agent

## Product design and implementation plan

Status: interactive product-design draft  
Initial data model: `menu.v1`  
Primary contract: one Canvas Render Definition, one shared renderer, one visual truth

## 1. Decision record

All architecture and workflow decisions supplied in the product brief remain in force. One visual decision changed after review:

- The charcoal application background is replaced by the repository-defined VennueSign Sky UI identity.
- Reason: the owner rejected the charcoal background and identified the existing repository as the visual authority.
- Impact: application surfaces use pale blue-gray, white, navy and Sky blue. The signage canvas remains independent and may use any template palette.
- No renderer, definition, validation, output-mode or repair-agent decision changes.

## 2. Creator workflow

1. Create a template.
2. Choose the data model and fixed design surface.
3. Start from a tested template, generated structure or blank definition.
4. Assemble data-aware components and basic decorative elements.
5. Bind structured components to model fields.
6. Apply reusable global theme tokens.
7. Define pages, variants, capacity and overflow strategies.
8. Test fixtures, boundary cases and actual customer data.
9. Resolve actionable diagnostics manually or through a repair proposal.
10. Select output mode per page or screen in the publish flow.
11. Publish; this single action runs authoritative validation before creating the immutable definition and its artifacts.

## 2.1 Canonical ownership and redundancy rules

- Edit and Test are the only canvas modes. Test is not repeated as a top-bar or rail action.
- Fields exposes the selected data model and bindings inside Edit; Components contains only model-aware objects; Elements contains unbound visual primitives.
- Selecting any rail tool returns the canvas to Edit. Test removes the editing panels and uses the full workspace for dataset preview and diagnostics.
- Test mode owns fixture, boundary and actual-customer variation review.
- Publish owns the one authoritative headless-Chromium gate and the output-mode decision.
- Version history is read-oriented traceability. It does not create a second publishing path or list unapproved repair proposals as saved versions.
- Repair review has one decision before application and one return action after application.
- New-template setup is one connected three-step flow. Resolution, orientation, safe area and starting point persist into the same Studio draft; there is no duplicate review screen.
- Planned data models are disabled until their schemas exist. A blank starting point remains publication-blocked until its first structured component is added.
- Menu-to-screen assignment and operational publishing status remain outside Theme Studio in their established VennueSign homes.

## 3. Screen inventory

### Connected prototype screens

| Surface | Purpose | Entry | Exit |
| --- | --- | --- | --- |
| Theme Studio | Design, bind, style and inspect the definition | Open a template | Test, publish, history |
| New template | Data model, display and starting-point setup | `New` | Create draft |
| Test matrix | Compare fixtures, customer data, outputs and resolutions | Test mode | Open a dataset or repair warnings |
| Repair review | Explain and approve schema-valid JSON Patch changes | Diagnostic | Apply to draft or keep original |
| Publish version | Choose page output mode and run the sole authoritative publication gate | Publish | Published result or return to Studio |
| Version history | Trace saved drafts, publications, renderer requirements and artifacts | Version link | Open a saved revision or compare publications |

### Studio rail states

| Rail state | Required capability |
| --- | --- |
| Fields | Browse `menu.v1`, actual customer data and available fields |
| Layouts | Choose structural presets and edit global theme tokens |
| Components | Add model-aware repeaters, titles, images and states |
| Elements | Add decorative text, images, logos, lines and shapes |
| Assets | Manage images, logos, fonts and publication availability |
| Pages | Define page order, continuation behavior and per-page output eligibility |
| Variants | Define ordered default, dense, sold-out and promotional variants |

### Inspector states

- Properties: data source, layout, ordering, pagination, formatting and overflow.
- Style: token inheritance, typography, fill and effects.
- Rules: required content, minimum type size, maximum density and supported output modes.

## 4. Frontend package boundaries

```text
apps/theme-studio
  Studio shell, panels, inspector, test workflow and repair review

packages/canvas-schema
  Versioned TypeScript types, JSON Schema, migrations and JSON Patch validation

packages/data-models
  menu.v1 schema, fixture generators, formatting and binding metadata

packages/canvas-renderer
  Deterministic DOM/SVG renderer and layout measurements

packages/editor-overlay
  Selection, guides, resize handles and coordinate transforms

packages/validation
  Browser diagnostics, boundary-data orchestration and result model

packages/publication
  Output-mode planning, artifact manifest and renderer compatibility rules
```

The Menu Editor, Theme Studio, validation workers, static generator and players import the same renderer package. No consumer reimplements layout rules.

## 5. Canvas Render Definition v1

The first schema should include:

- `schemaVersion`
- `dataModelVersion`
- `rendererCompatibility`
- fixed canvas dimensions and safe area
- theme token references and optional local overrides
- pages and regions
- structured components and decorative elements
- bindings, formatters and fallbacks
- variants and activation conditions
- capacity declarations
- ordered overflow strategies with permitted limits
- supported output modes
- locked/protected constraints
- referenced asset and font manifests

Every component receives a stable identifier so diagnostics, history and repair patches can point to exact objects.

## 6. Persistence model

Recommended logical records:

- `Theme`: identity and ownership.
- `ThemeRevision`: mutable draft or immutable published theme-token version.
- `Template`: identity, data model and ownership.
- `TemplateRevision`: Canvas Render Definition, lifecycle state and schema version.
- `TemplatePublication`: immutable definition revision, renderer requirement and validation report.
- `PublishedArtifact`: mode, checksum, dimensions, storage key and generation state.
- `Asset`: font/image/logo metadata, licensing state and checksum.
- `ValidationRun`: scope, datasets, resolutions, modes, renderer version and results.
- `RepairRun`: input revision, diagnostics, attempt/cost state, proposed patch and explanation.
- `RepairDecision`: creator approval/rejection and resulting draft revision.

Theme and template revisions remain separate internally even when the Studio edits them in one experience.

## 7. Product API surface

Illustrative endpoint groups:

- `GET/POST /api/theme-studio/templates`
- `GET/PATCH /api/theme-studio/templates/{id}/draft`
- `POST /api/theme-studio/templates/{id}/drafts`
- `GET /api/theme-studio/templates/{id}/versions`
- `POST /api/theme-studio/templates/{id}/validate`
- `POST /api/theme-studio/templates/{id}/repairs`
- `POST /api/theme-studio/repairs/{id}/apply`
- `POST /api/theme-studio/templates/{id}/publish`
- `GET/POST /api/theme-studio/assets`
- `GET /api/theme-studio/data-models/{version}`
- `GET /api/theme-studio/templates/{id}/fixtures`

The API rejects patches that fail schema validation, target a published revision or weaken a protected constraint without an explicit creator-authorized change.

## 8. Validation pipeline

### Immediate browser validation

- Runs after meaningful edits with debouncing.
- Uses the shared renderer and exact font metrics.
- Produces object-addressed diagnostics with pixel measurements.
- Tests the active dataset and canvas configuration.
- Never represents itself as the publication gate.

### Authoritative validation

1. Freeze the draft input bundle.
2. Resolve exact fonts, assets, schema and renderer version.
3. Generate boundary and model-specific datasets.
4. Render supported resolutions in headless Chromium.
5. Test static, live and hybrid equivalence where declared.
6. Record clipping, overflow, missing bindings, asset failures and compatibility results.
7. Store a deterministic report with checksums.
8. Block publication on unresolved required-content failures.

## 9. Template Repair Agent

### Inputs

- Draft Canvas Render Definition
- Data-model schema and binding metadata
- Exact renderer diagnostics
- Protected constraints
- Available variants and permitted overflow strategies
- Attempt, latency and cost budget

### Output contract

- RFC 6902 JSON Patch only
- Plain-language explanation per operation
- Expected diagnostic effect
- Constraints confirmed as preserved
- Remaining unresolved conflicts

### Execution

1. Create an isolated draft revision.
2. Request a structured repair proposal.
3. Validate the patch against allowed paths and protected constraints.
4. Apply it only to the isolated revision.
5. Render and run authoritative validation.
6. Retry within fixed attempt and cost limits when improvement is possible.
7. Present before/after output, the exact patch and validation delta.
8. Require creator approval before copying changes to the working draft.
9. Require the normal publication gate afterward.

Renderer defects must be reported as renderer defects. The agent must not compensate by damaging the template.

## 10. Output modes

- Static: validate and render the full approved artifact; players download and cache it.
- Live: players combine the immutable definition with current data through the shared renderer.
- Hybrid: static base plus explicitly declared live regions.

Mode is selected per page or screen. A template declares supported modes and may recommend a default. Automatic selection is allowed only when the result is unambiguous.

## 11. Delivery sequence

### Milestone 1 — contracts and deterministic renderer

- Lock `menu.v1` and Canvas Render Definition v1.
- Build schema validation and migrations.
- Extract the shared DOM/SVG renderer.
- Create deterministic fixture and screenshot tests.

Acceptance: the same definition produces equivalent Studio, static and player output.

### Milestone 2 — editor foundation

- Full-screen shell, rail, canvas overlay and inspector.
- Selection, positioning, typography, bindings and token editing.
- Undo/redo transaction model and autosave.
- Pages, regions and structured menu components.

Acceptance: a creator can build and save a valid two-column menu without editing JSON.

### Milestone 3 — variation and safety

- Variants, capacity declarations and overflow strategies.
- Immediate diagnostics and complete test matrix.
- Actual-customer-data test source.
- Accessible diagnostic navigation from issue to object.

Acceptance: long, empty, missing, sold-out and maximum-content cases are testable and never silently clip.

### Milestone 4 — publication and artifacts

- Authoritative headless-Chromium validation.
- Static/live/hybrid output planning.
- Artifact generation, checksums and renderer compatibility.
- Immutable publications and version history.

Acceptance: publication fails safely or creates a traceable definition and complete artifacts.

### Milestone 5 — repair agent

- Structured Responses API integration.
- Patch allowlist and protected-constraint enforcement.
- Isolated repair drafts, retry budget and before/after review.
- Approval, rejection and audit history.

Acceptance: the agent can repair supported layout failures without modifying published definitions or protected constraints.

### Milestone 6 — additional data models

- Extract reusable model registration and fixture interfaces.
- Add Cinema, Tap Board and Bakery model packages independently.
- Add model-specific components, formatters and diagnostics.

Acceptance: adding a model does not fork the renderer or editor architecture.

## 12. Testing strategy

- Unit: schemas, formatters, migrations, variant selection and overflow ordering.
- Renderer: deterministic layout metrics and visual snapshots.
- Integration: Studio save/load, validation, repair and publication state machines.
- Equivalence: Studio, static, live and hybrid output comparisons.
- Boundary: empty, typical, busy, maximum, long, missing and unavailable content.
- Compatibility: supported renderer versions, resolutions and fonts.
- Security: patch path allowlist, draft-only mutation and asset authorization.
- Accessibility: keyboard editing, focus order, names, contrast and non-color status cues.

## 13. Decisions still requiring owner approval

- Exact Canvas Render Definition v1 field names.
- Whether global theme tokens can be overridden per page and at what scopes.
- Initial repair attempt, latency and cost ceilings.
- Renderer compatibility policy: exact version versus minimum supported version.
- Which output modes ship in the first production release.
- Whether generated layouts are included in the first Theme Studio milestone or follow the manual editor.

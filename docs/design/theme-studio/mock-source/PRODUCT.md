# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Primary users are Vennue theme creators and advanced operators building reliable, reusable digital-signage templates. They work in a focused desktop editing environment and need approachable visual controls without sacrificing precision, structured-data awareness, or publishing safety.

## Product Purpose

Theme Studio creates and maintains structured, versioned Canvas Render Definitions for digital signage. Success means a creator can design once, test against realistic content variation, repair failures safely, and publish a definition that renders equivalently in previews, generated images, web players, and managed-device players.

## Positioning

Theme Studio combines approachable visual editing with report-designer data binding and deterministic publication constraints. Unlike a freeform illustration canvas or webpage builder, its components understand versioned data models and cannot silently publish required content that clips or becomes incompatible.

## Operating Context

Creators choose a data model and screen format, start from a template, generated layout, or blank canvas, add data-aware components, bind and style them with global theme tokens, test realistic boundary datasets, resolve diagnostics, and publish a versioned definition. The initial data model is `menu.v1`; later models include Cinema, Tap Board, Bakery, and others. Theme Studio may also open from an actual customer menu and test that customer's data.

## Capabilities and Constraints

- React and TypeScript in Vennue's existing frontend environment, likely Vite.
- A shared TypeScript DOM/SVG renderer is authoritative across Studio, Menu Editor, validation, artifacts, web player, and managed player.
- Definitions are versioned JSON validated by JSON Schema; agent repairs are schema-valid JSON Patch operations.
- Real HTML/CSS/SVG handles structured, text-heavy rendering. React/SVG overlays provide selection, guides, and resize handles.
- Theme tokens and template definitions remain separate internally even when creators experience them as one workflow.
- Static-image, live-rendered, and hybrid output modes use the same definition, data, fonts, dimensions, and renderer version and must remain visually equivalent.
- Publishing mode is selected per screen or page. Templates declare supported modes and may recommend a default.
- Required content never silently clips. Publication is blocked when permitted overflow strategies cannot produce a safe render.
- Immediate browser validation supports editing; authoritative headless-Chromium validation gates publication.
- Identical inputs must render deterministically.
- The Template Repair Agent operates on a new draft revision, respects protected constraints, explains proposed changes, shows before and after, and requires creator approval.
- Open implementation decisions: exact initial Canvas Render Definition schema, persistence tables, renderer package boundaries, retry/cost limits for repair, and renderer version compatibility policy.

## Brand Commitments

The product is Vennue Theme Studio within VennueSign. The experience must feel professional but approachable: Canva's approachability, Figma's precision, and a report designer's understanding of structured data. The workspace uses VennueSign's repository-defined Sky UI identity: a pale blue-gray editing field, white raised panels, a navy tool rail and Sky blue action/selection accents. It retains a bright accurate canvas, compact panels, minimal borders, strong selection outlines, color-coded bindings and diagnostics, and progressive disclosure.

## Evidence on Hand

The session brief supplies the product architecture, workspace anatomy, creation flow, component model, inspector behavior, overflow rules, testing datasets, output modes, versioning requirements, validation model, and Template Repair Agent contract. Prototype content and customer claims beyond these requirements are illustrative and must not be presented as production evidence.

## Product Principles

- One renderer, one visual truth.
- Make structured design approachable without hiding its rules.
- Test the variation, not only the ideal example.
- Block unsafe publication and explain the exact recovery.
- Agent assistance proposes transparent, reversible, constraint-respecting changes.

## Accessibility & Inclusion

The Studio must support keyboard focus, legible contrast, accessible labels, and non-color-only diagnostic meaning. The rendered signage template must include accessibility checks appropriate to text legibility and supported displays.

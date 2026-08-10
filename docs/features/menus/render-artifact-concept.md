# Pre-rendered Display Artifact Concept

- **Status:** Product concept for evaluation; not yet an approved change to the Menus milestone plan or rendering contract.
- **Area:** Menus — board render engine / display player
- **Recorded:** 2026-08-09

## Concept

Most Vennusign screen content is visually static for the majority of the time, regardless of the selected theme or screen format. The display player therefore does not necessarily need to continuously reconstruct the full board from HTML/CSS and structured layout data.

Instead, the Vennusign rendering engine can treat a published board as a **compiled display artifact**.

At publish/render time, the engine takes the structured screen definition, menu content, theme, typography, layout rules and target display geometry and produces a high-definition raster image at the target screen resolution. The player then displays that completed image fullscreen rather than independently reproducing the static board layout.

Conceptually:

`Screen definition + content + theme + target geometry -> render/compile -> display artifact -> player`

For example, a 1920x1080 screen receives a 1920x1080 artifact and a 3840x2160 screen receives a 3840x2160 artifact. The source definition remains resolution-independent; the delivered artifact is optimized for the target display.

HTML/CSS or another layout technology may still be used internally by the rendering engine. The important architectural distinction is that it is an **authoring/rendering implementation detail**, not necessarily the final runtime representation used by the player.

## Why this fits digital signage

A menu board, welcome board, directory, promotion or similar signage surface may remain visually unchanged for minutes or hours. Re-running a complete browser layout continuously on every player provides little value during those periods.

Pre-rendering means the computationally complicated part happens when the visual state changes rather than for the entire time that state is displayed.

The player is given an explicit visual answer: **this is what this screen should look like.**

This strengthens the product's what-you-see-is-what-the-screen-gets behavior because the final output is produced centrally by the same rendering rules rather than being independently interpreted by each display device.

## Hybrid compositor model

The proposal should not require every screen element to become part of one static image.

The preferred direction is a hybrid compositor:

1. The render engine produces a static background artifact containing the portions of the board that do not need continuous runtime updates.
2. Truly dynamic content remains as separately described runtime layers.
3. The player composites those layers over the static artifact.

Potential dynamic layers include:

- video;
- animation;
- live clocks;
- tickers;
- weather;
- live pricing or other frequently changing data;
- other widgets whose state must change without recompiling the entire board.

A typical board could therefore have 90–95% of its visual surface represented by the pre-rendered artifact while only the small dynamic portion requires active runtime rendering.

## Render invalidation

A display artifact is regenerated only when something that affects its visual output changes. Examples include:

- menu content changes and is published;
- theme changes;
- target resolution/orientation changes;
- layout or pagination changes;
- a scheduled visual state changes;
- a data dependency that is part of the static artifact changes;
- an immediate state such as item availability changes and affects guest-visible content.

The exact invalidation rules must be designed before implementation. In particular, the existing Menus requirement that an availability (`86`) change removes an item immediately must remain true; pre-rendering cannot turn an immediate product behavior into a publish-only behavior.

## Device-specific artifacts

Publishing can generate artifacts for the geometry actually required by assigned screens rather than assuming one universal bitmap.

Examples:

- landscape Full HD: 1920x1080;
- portrait Full HD: 1080x1920;
- landscape UHD/4K: 3840x2160;
- other supported reported geometries as required.

Multiple screens with identical geometry and identical visual state should be able to share the same immutable artifact rather than producing duplicate files.

An artifact should be versioned/content-addressed so a player can determine whether it already has the exact required output and avoid downloading it again.

## Expected advantages

### Deterministic visual output

Fonts, spacing, wrapping, CSS/browser differences and device-specific rendering behavior are resolved before delivery. Players with different hardware should display the same pixels for the static portion of the board.

### Lightweight player

The player can focus on artifact delivery, fullscreen presentation, dynamic-layer composition, page transitions, health reporting and recovery rather than carrying the full authoring/rendering environment for every static frame.

### Predictable hardware requirements

A low-cost player and a high-powered player can produce substantially identical static output because neither is responsible for reconstructing the board layout.

### Fast steady-state display

Once the artifact is downloaded, displaying an unchanged screen requires very little work. The artifact can remain cached locally and continue displaying through temporary connectivity loss.

### Stronger preview fidelity

The back-office preview and the guest display can be based on the same render output. Preview-only annotations remain a separate concern and must never be baked into the guest artifact.

### Easier artifact validation

A render can be inspected, hashed, cached and potentially compared visually before delivery. This provides a concrete object representing exactly what a published screen version should show.

## Architectural implications

This concept changes the role of the board render engine from primarily a reusable client-side component into a **render compiler plus runtime composition contract**.

The authoritative data remains structured. Vennusign must never make the generated image the editable source of truth. A generated artifact is disposable output that can always be reproduced from the published structured state, theme and rendering version.

The player should know at minimum:

- which published version it should display;
- which artifact(s) correspond to that version and its geometry;
- artifact integrity/version information;
- page/dwell behavior;
- dynamic layers, if any;
- what to display while an updated artifact is downloading;
- how to fall back when an artifact cannot be obtained.

Publishing should remain atomic from the guest's perspective. A player must not show a mixture of artifacts or layers from different published versions.

## Relationship to current Menus milestones

This concept directly intersects the approved Menus plan:

- **Milestone 2 — Board render engine v1:** the renderer should be designed so its output can become a compiled artifact rather than assuming the player must always execute the same HTML/CSS representation.
- **Milestone 3 — Canvas as preview:** preview fidelity should be evaluated against the artifact-producing renderer so the canvas remains an honest preview.
- **Milestone 4 — Player:** this is where the delivery/runtime contract becomes critical. The existing requirements for page dwell, immediate 86 behavior, publish swap at the next page turn, guest-visible pagination and device-reported geometry must all be preserved.
- **Theme editor work:** themes remain structured definitions consumed by the renderer. A new theme should not require player-specific rendering code.

This document does **not** supersede `milestone-plan.md`, `open-questions.md`, or the approved design authority. It records the concept so it can be evaluated before the affected rendering/player implementation is locked in.

## Questions to resolve before adoption

1. Where does artifact generation execute: server service, dedicated rendering worker, or another controlled environment?
2. Which image format is preferred for signage output (for example PNG, WebP, AVIF, or format selected by content characteristics)?
3. Is each paginated board page a separate artifact or are pages packaged together?
4. Which content types qualify as runtime dynamic layers versus artifact invalidations?
5. How quickly must an immediate 86 produce and deliver replacement artifacts, and should that path have a specialized fast render?
6. How are artifacts cached, versioned, retained and cleaned up?
7. What does a player show when a new artifact fails to download or fails integrity validation?
8. Can identical artifacts be safely shared across screens/venues through content addressing without weakening tenant isolation?
9. What exact geometry variants are generated when a screen has not yet reported reliable geometry?
10. How is the render engine version included in the artifact identity so renderer upgrades cannot leave stale visual output cached indefinitely?
11. Which dynamic layers require synchronization with page turns or published versions?
12. What automated visual/regression proof is required to guarantee that preview output and delivered guest artifacts match?

## Principle

**Vennusign should send the player the finished visual result whenever the content does not require the player to calculate it continuously.**

The player remains capable of runtime composition where the product genuinely needs dynamic behavior, but static signage should be treated as compiled output rather than repeatedly interpreted layout code.

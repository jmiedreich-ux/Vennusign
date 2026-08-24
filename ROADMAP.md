# Vennusign Roadmap

Where every workstream is, on one page. Vennusign is not developed sequentially: several
workstreams move at once, some pause while others catch up, and some wait on a prerequisite
another stream has not shipped. This document is the map of that.

**What this is.** The living index of workstreams, their milestones, and their current
position. It is updated at milestone completion, alongside `PROJECT_STATUS.md`, the tracker and
the handoff.

**What this is not.** Not a design authority, not a plan, not a status log. Each workstream
links to its own records; the detail lives there. `PROJECT_STATUS.md` carries the narrative of
recent work; this carries the shape of all of it.

**Codenames are deliberate.** A codename names a layer or a body of work so it can be referred to
independently of whichever feature is using it. The owner finds them easier to point at than
descriptive names, and they survive renames of the things underneath.

---

## The eras

### Foundational Work — Milestones 1 to 13 · complete

*Formerly "Phases 1–13".* Renamed by the owner on 2026-08-21.

The first era built the system's foundation: data access, the API, the content model, display
layouts, scheduling, tap lists, TV platform distribution, billing, POS integration, and customer
identity, tenancy, authentication and onboarding. The development discipline was still forming
during this era, and the result was a lot of UI rework later — but the foundation itself proved
solid. The recurring experience of the reset era has been finding things already there and ready:
`SupportAccessGrants`, the composite tenancy keys, `ReleaseVersionMetadata`, the pairing flow,
the capability model. That is what this era delivered.

Former Phases 14, 15 and 16 were cancelled as phases on 2026-08-07. Their feature ideas survive in
the backlog and require fresh owner approval before planning.

Records: `docs/archive/phase-plans/`, `docs/architecture/phase-*.md`, and the `Historical
Reference` section of `PROJECT_STATUS.md`. Research-only, per `AGENTS.md`.

### The Great Reset — 2026-08-07 onward · in progress

Adopted after the Track 1 retrospective: the phase/track/work-package model retired, all future
tracks cancelled, and the **features-and-milestones** working model put in its place. A feature
is delivered in numbered milestones that each ship whole — schema, API, UI and Playwright
together — and each ends in owner acceptance before the next starts.

The reset's first feature was **Menus**, and building it is what surfaced the need for the
development discipline now in `AGENTS.md` and `docs/MILESTONE_EXECUTION.md`. At the end of Menus
M3-A the owner concluded that the work just completed needed to be rebuilt on stronger ground,
and the reset widened: Keystone was named as the permanent cutover layer, and Theme Studio was
identified as a prerequisite for going further into the menu display.

This era is still open.

---

## Workstreams

Design work lives in the Claude Design project **Vennusign screen mockups** (referred to below as
*Design*). Its hub page `Vennusign Back Office.dc.html` links every area. Approved material is
mirrored into the repository under `docs/features/<feature>/` (amended 2026-08-24 — approved design
used to land in a separate `docs/design/approved/` tree; it now lands alongside the feature's own
tracking); anything only in Design is by definition not approved.

| Codename | What it is | Position | Gate |
|---|---|---|---|
| **Menus** | Back-office menu feature: library, builder, publish, 86 board, import | **M6-A3 complete.** M5 parked. M7+ unplanned. | Paused — Theme Studio is a prerequisite for further display work |
| **Theme Studio** | Reusable theme authoring: row-level style, state behaviour, validation, assistant, Menu Builder handoff | **Hi-fi in Design** — TS1–TS5, a theme-editor hi-fi, identity and rail options, a 28-state storyboard and a PSA review. Nothing in the repo. | Design authority → question register → milestone plan |
| **Keystone** | Progressive-cutover thin layer: TenantContext, VDS, ADS, Product Router, Webhook Receiver | **Designed, not approved.** 49 decisions proposed, 34 questions answered, six milestones planned. | Owner approval of the authority; tier-and-cost before any deployment |
| **Screens** | Screen fleet management: home, one screen, pairing | **Hi-fi in Design, current** — S1/S2/S3. No developer handoff, no authority, no plan. | Developer handoff "when we return to that area" |
| **Onboarding** | Sign-up through first live screen, and the tier ladder | **Nine hi-fi frames in Design, not approved.** Keystone decision 48 makes it its own app. | Owner approval; resolving the Free-tier go-live consequence |
| **Platform Operations** | The operator console: release board, rollouts, cohort health, organizations, versions, windows | **Ten screens exported in Design.** Confirmed as its own app with its own API (Keystone decision 38). | Design authority; depends on Keystone M2 for anything it writes |
| **Mosaic** | Release codename for v1.0 — the first version that ships through Keystone | **Named only.** | Everything above |

Release codenames are separate from feature codenames: Keystone is a feature, Mosaic is a
release. The first version is Mosaic.

---

## Menus

**Records and design authority:** `docs/features/menus/` (36 decisions + paste-import 37–43) ·
register 209 questions, one open (Q209, provisional).

| Milestone | Scope | Status |
|---|---|---|
| 1 | Item library, draft/publish spine, assignment | complete · 2026-08-09 |
| 2 | App shell, render engine, Menus home | complete · 2026-08-10 |
| 3 | Builder, adding items | complete · 2026-08-11, on a remediated record at owner instruction |
| 3-A (s0–s3a) | Builder refinement sub-sequence: pages, search, Signal V rail | complete · 2026-08-12 |
| 4 | Content and delivery foundations | complete · 2026-08-13 |
| 5 | Board view + Play | **parked** · #709 |
| 6 | Quick Update (86 board) + blank creation | complete · 2026-08-13 |
| 6-A1 | Paste, parse, review | complete · 2026-08-14 |
| 6-A2 | Create from import | complete · 2026-08-14 |
| 6-A3 | Replace from import | complete · 2026-08-14 |
| 7+ | Spreadsheet import, photo import, POS import, item library UI, multi-venue, Schedules-owned time pricing, fallback-card authoring | **named, not planned** |

**Why it is paused.** Further work on the menu's display — the builder's canvas, Board View,
Play, and anything in `src/display` — depends on how a theme is defined and rendered. Theme
Studio settles that. Until it does, building more display would build against a theme model that
is about to change.

**Backlog:** #670–#683 (register out-of-scope decisions, copy debt, accessibility debt), #686,
#695, #701, #702, #709, #710.

---

## Keystone

**Records:** `docs/features/keystone/` · authority `docs/design/proposed/keystone/decisions.md`
(49, **proposed**) · register 34 questions, 31 answered, 3 deferred · procedure
`docs/MILESTONE_EXECUTION.md` · diagrams in Design `cutover/` — `Cutover Architecture v2` is
current; v1 and its Mermaid source are superseded. Note that v2 predates the naming decision
and still says "enforcement point," which decision 3 retires; it should read Product Router.

Keystone is the permanent name for the layer that routes each customer's traffic to the version
they are assigned to. It cannot roll itself out progressively, so it deploys all-at-once,
backward-compatible only, with immediate rollback as the sole recovery path — which is what the
name records.

| Milestone | Builds | Acceptance | Status |
|---|---|---|---|
| 1 | `Vennu.Tenancy` — TenantContext, path, token; API resolution | demo script | planned |
| 2 | Version Discovery Service | demo script | planned |
| 3 | Application Discovery Service | demo script | planned |
| 4 | Front ends adopt the tenant path; pre-auth app split | workbook | planned |
| 5 | Product Router | workbook | planned |
| 6 | POS Webhook Receiver | demo script | planned |

**Gates.** The design authority is in `proposed/` and must move to `approved/` before M1 starts.
M2, M3, M5 and M6 each stand up an App Service, and tier and plan cost are deliberately deferred
— they can be built and accepted locally but not deployed until the owner accepts that cost.

**Parked inside Keystone:** connection-membership scope (#742), device auto-re-pair, the
deploy-pipeline conversation (four named assumptions to test).

**Prerequisites owned elsewhere:** `VENNU_COMPONENT_VERSION` must be set by deployment — **#754**,
successor to #726, which closed on 2026-08-21 having set source commit and build id but leaving
`componentVersion` at `0.0.0-local` because no release-versioning scheme exists to source it from;
the pipeline must be able to produce a per-version target (Q32, a prerequisite feature); durable
secrets off Data Protection (decision 37, `Vennu.Api` area).

**Product changes Keystone surfaced but does not own:** signing in should establish an
organization rather than a venue, and onboarding should be its own app (decision 48). Both belong
to the back-office/authentication area.

---

## Theme Studio

**Records:** Design `themes/` — `TS1 Entry and setup`, `TS2 Theme editor`, `TS3 Settings and
styling`, `TS4 Proving, repair and save`, `TS5 Building from blank`, `Theme Editor Hi-Fi`, identity
options D/E/E2, rail options A/B. Design `uploads/` — the 28-state storyboard (`.pptx`) and the PSA
review (`.docx`). Drive — *VennueSign Theme Studio — UI Storyboard*. In the repository: nothing
beyond a reserved `theme-studio` subdomain and App Service.

**Note:** Design's `themes/notes.md` still reads "nothing designed yet." It is stale — the folder
beside it holds five screens and a hi-fi. It does usefully record what exists in the product today
(`src/back-office/src/ThemeBuilder.tsx`: venue-wide styling, six swatches, a Pro/Business advanced
mode) and that the current builder shows disabled Pro controls, which contradicts Menus decision 4.

A separate application for authoring reusable themes. The storyboard's six flows: enter and set
up (bind to a data model such as `menu.v1`, define the display surface); build the row (repeater,
fields, row-level style, connectors, availability as a state field); validate and save (the
Long-text case, safe repair, authoritative validation); design with the assistant (proposals as
inspectable definition patches, applied only on approval); diagnose and repair (symptom-led, with
an Ops report filed asynchronously); and Menu Builder handoff (the saved theme is a draft input,
and publishing the menu is the only live-screen action).

**Why it is a prerequisite.** Menus' builder canvas, Board View and Play all render a theme. Theme
Studio defines what a theme is — row-level style independent of any one field, state behaviour
rather than a standalone sold-out variant, a saved-theme boundary distinct from menu publication.
Building more display before that is settled means rebuilding it after.

**Open questions on record** (from `themes/notes.md`): audience; whether themes stay venue-wide;
brand kit versus presets; group-level themes.

**Gate.** The storyboard is in UI review against five named questions. It needs a design
authority landed in `docs/features/theme-studio/`, then a question register, then a milestone
plan. None exist yet.

---

## Screens

**Records:** Design `screens/` — `Screens Hi-Fi.dc.html` (**current**: S1 screens home, S2 one
screen, S3 pairing), two earlier option rounds, and `backlog.md` (Wall view, Map view — both
parked with their blockers named). Nothing in the repository.

Fleet management for a venue's screens. Design's README records the next step plainly: "a
developer handoff for Screens, to be written when we return to that area."

**Why it matters now.** Three open issues are screen-lifecycle defects that a Screens design would
settle rather than patch: #741 and #746 (anonymous screen creation, nothing reaps unclaimed
screens, every `/pair` visit registers a new one) and #753 (a screen with no venue is invisible
to every PO listing). Keystone decision 8 — pre-auth writes nothing — also lands here: the
screen row appears at claim, not at `/pair`.

**Gate.** The developer handoff, then a design authority landed in `docs/features/screens/`.

---

## Onboarding

**Records:** Design `onboarding/not-approved/` — `Onboarding Hi-Fi.dc.html` (nine frames) and
`notes.md`, which is a real decision record. Repository: `docs/design/proposed/` holds the
flow-to-live-menu wireframe. The product has a working onboarding flow today inside
`src/back-office` (`CustomerOnboardingApp.tsx`).

Sign-up through first live screen. The notes record the tier ladder from RWP-00.79 — **Free ·
Operate · Coordinate · Portfolio · Enterprise**, names editable — with no prices, allowances or
trial durations set, and the outcome-bundle names from RWP-00.22 parked as a possible subtitle
layer. They also record corrections: a new venue has no menu and no prices; getting a menu in
uses the approved Menus routes; language selection is out; the first-run checklist belongs on a
Back Office home that is not yet designed.

**Two things converge here.** Keystone decision 48 says onboarding should be its own app, because
`main.tsx` is already a two-way switch between it and back office over exactly the pre-auth
routes. And the notes name a consequence still unresolved: Free excludes menu workflows, so the
go-live menu routes in frame 7 imply Operate or a trial — a Free account needs its own go-live
screen.

**Gate.** Owner approval of the nine frames, and the Free-tier go-live consequence resolved.

---

## Platform Operations

**Records:** Design `exports/platform-operations/` — ten screens: release board, release detail,
rollout progress, cohort health, organizations, organization profile, customer versions, register
a venue, version inventory, window schedule. Repository: `docs/design/proposed/platform-operations/`
(`po-screens.html`). The product has a PO frontend today (`src/platform-operations`) with no backend
of its own.

The operator console. Keystone settled its shape: PO is an application plus its own API, side by
side, not version-routed (decision 38); support access originates in PO and executes on the
version-routed customer surface (decision 39); and the venue-scoped controllers currently under
`Vennu.Api/Controllers/PlatformOperations/` are product API wearing a PO label (#747).

**Dependency.** Everything PO *writes* — assignments, the default-version pointer, registration —
goes through VDS, which is Keystone M2. PO's read-only surfaces can be designed and built ahead of
that; its release-orchestration backend cannot.

**Gate.** A design authority landed in `docs/features/platform-operations/`, and Keystone M2
for the write paths.

---

## Design queue

Concepts and proposals that exist but have no workstream. Each needs an owner decision before it
becomes one.

| Item | Where | State |
|---|---|---|
| **Atlas** — the always-current site | `docs/features/atlas/decisions.md` (57, **approved 2026-08-22**, amended the same day with 48–57) · `docs/features/atlas/m1-plan.md` | A site **built from** the repo and GitHub on every merge — never hand-maintained — rendering the roadmap, each workstream's authority, register, plans and records, the procedure, handoff and status, with open issues per workstream label pulled at build time. The three artifacts from the Keystone work are its prototype. Authority approved 2026-08-22. Seven milestones recorded: M1 the generator (its own repository, a versioned Action), M2 Vennusign adopting it, M2.1 the feature planning page rebuilt as drawn paths, M3 write-back — all done — then M4 the page's second pass (next), M5 the register as data, M6 tasks as GitHub issues. |
| Product surface feature inventory | `docs/design/proposed/product-surface-feature-inventory.md` | 18 domains, 133 capabilities; a design reference, not a roadmap |
| Back Office home | named in Onboarding's notes as where the first-run checklist belongs | not designed |
| Customer support diagnostic agent | `docs/design/customer-support-diagnostic-agent-concept.md` | exploratory concept |
| Branded authentication email | `docs/design/branded-authentication-email-concept.md` | records a conflict with authentication decision 3 |
| Multi-venue menus | Design `menus/Multi-Venue Menus.dc.html` (MV1–MV4b) | wireframes; Menus M7+ |
| Dashboard wireframes | `docs/design/proposed/*.png` | wireframes only |
| Competitor signage references | `docs/design/competitor-signage-references.md` | research |

---

## Candidate workstreams from the issue backlog

Open issues that are not defects in an existing workstream but the seed of something without
one. Each is a cluster, not a single ticket; none is approved. Listed so they are visible as
shapes rather than scattered numbers.

| Candidate | Issues | What it actually is |
|---|---|---|
| **Release versioning** | #754 | The manifest `productVersion`, per-component semver and `v{x.y.z}` tags the cutover concept describes. #726 closed without it, and Keystone cannot move a customer until it exists. The first concrete piece of the release model, and nobody owns it. |
| **Legacy retirement and environment hygiene** | #744, #748, #749, #750, #751, #752 | Retiring `dbo.MenuItems` and the POS catalog wiring, deciding `LayoutTemplates`, confirming two never-written tables, stopping 17 idle App Services, and making Murphy the owner of environment drift after a test ran `DELETE FROM dbo.Venues` against whatever an environment variable pointed at. Six issues, one theme: the foundation era left things running that nothing uses. |
| **Test harness integrity** | #688, #715, #735, #751 | Pre-existing failures outside the routine gate, parallel Playwright seeding colliding on one LocalDB, sign-in unexercisable on localhost, and a destructive test with no guard. The gate is local verification while CI is suspended, so the harness *is* the gate — and four issues say it is not trustworthy. |
| **Authentication hardening** | #723, #727, #737 | A changed provider subject locks a customer out permanently with a 500; a disabled provider shows raw JSON; the sign-in rework shipped with no browser coverage. Authentication has an approved authority (`docs/features/authentication/`) and, as of 2026-08-24, a scaffolded workstream carrying it forward (`docs/features/authentication/workstream.json`) — placeholder content, not yet planned. |
| **Display diagnostics** | #738 | A view to understand a screen without a debugger. Pairs with the customer-support diagnostic agent concept in the design queue — one is the surface, the other the reasoning behind it. |

Already mapped to a workstream and not repeated here: #741, #746, #730, #753 (Screens); #729
(Onboarding); #742, #743, #747 (Keystone); #724, #725, #732, #733 (public site); #670–#686,
#695, #701, #702, #709 (Menus backlog).

**Stale, should be closed:** #692 (Menus M3-A slice 0) and #710 (Menus Slice 6) are both
delivered per `PROJECT_STATUS.md` and still open.

---

## Cross-cutting, not a workstream

- **CI** is suspended by owner decision (2026-08-09). Local verification is the gate until the
  owner restores it.
- **Murphy**, the QA agent, runs against deployed environments on demand. Its findings are filed
  as issues (#723–#727, #746–#753).
- **Public site** (`src/www`) is live on dev and is out of the version equation. Open: #724,
  #725, #732, #733.
- **Docs** need consolidation; the owner has named this as its own project, not yet started.
- **Knowledge**, a training corpus for agents, support and developers, was discussed and parked.

---

## Maintaining this document

- Update at milestone completion, in the same batch as `PROJECT_STATUS.md`, the tracker and the
  handoff. Not after every commit.
- A workstream's position here must agree with its own records. If they disagree, the
  workstream's records are right and this is wrong.
- A new workstream gets a row in the table and a section below it when it has a design authority
  or a plan. Before that it is in the design queue.
- Codenames, once used in a record, are not renamed. Add a note if the thing underneath changes.

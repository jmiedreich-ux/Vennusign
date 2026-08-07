# Menus Build — Open Questions Register

- **Status:** sitting 3 in progress (2026-08-07) — minor lane: Q93, Q94, Q116–Q124, Q129, Q130, Q144–Q150 resolved (Q94 owner-answered: near-miss rows offer a picker of other close matches; Q120 and Q122 out of scope → backlog). Sitting 2 (2026-08-07) — all 16 BLOCKING questions resolved (Q83–Q178): 12 accepted recommended, Q83 owner-answered (resolve at import; reconciliation flag), Q86 deferred, Q98 and Q133 out of scope → backlog. RESUME AT Q87 — remaining are 78 *important* + 32 *minor*, walked in document order, skipping already-answered. Sitting 1 (2026-08-07): Q1–Q82 resolved or deferred. Awaiting owner answers (generated 2026-08-07 from the 15-agent design interrogation: 333 raw findings, deduped to the list below)
- **Authority context:** `docs/design/approved/menus/` + `build-decisions.md` (17 decisions). Nothing here re-asks those.

## How to answer

Every question has three valid answers:

1. **Accept the recommended default** — say `rec`.
2. **Your own answer** — a sentence is enough.
3. **Decide later** — say `defer`. Deferrals stay tracked here; if a slice cannot wait, the recommended default is used *provisionally* and flagged in that slice's acceptance workbook so you see the consequence live and can overturn it cheaply.

Shorthand works: `Q1 rec, Q5 defer, Q7: one menu per screen`. Unanswered questions are treated as **defer**, never as silent acceptance.

**BLOCKING** = slice 1 or 2 needs the answer (or uses the provisional default) before that work starts. *important* = needed before the slice that builds it. *minor* = cosmetic/copy, cheap to change late.

## The structural holes — the product model itself

### Q1 · BLOCKING

**How does a menu get ONTO a screen in this build — where does someone pick which screens a menu shows on?**

Every artifact shows menus already on screens (publish chips, "On 3 screens", take-off dialog) but no surface anywhere assigns one. The codebase has no assignment at all: DisplayController serves the venue's first active menu, Screen.cs has no content-target field, and the Screens area was never designed for it. Slice 2's cards and take-off dialog need the link to exist.

*Recommended:* First Publish doubles as the assignment act — a screen picker with all screens pre-ticked; the publish-bar chips become that choice thereafter; the menu↔screen link lands as its own table in slice 1; Screens gains a matching control later.

*Answer:* Accepted recommended (2026-08-07): first Publish doubles as the assignment act — screen picker, all screens pre-ticked; publish-bar chips are the choice thereafter; assignment is its own table in slice 1.

<sub>decisions.md 10/16/35; README data model; DisplayController.cs; Screen.cs [decisions-doc, readme-handoff, wf-import-actions, code-display, code-api-data]</sub>

### Q2 · BLOCKING

**Can a screen ever carry more than one menu at a time, or is it exactly one menu per screen until Schedules arrives?**

The design never shows two menus on one screen but never forbids it; the answer shapes the schema, publish chips, and player. Guessing wrong means a migration later.

*Recommended:* Exactly one menu per screen, stored as a separate assignment record (not a field on Screen) so Schedules can point several menus at a screen later without migration.

*Answer:* Accepted recommended (2026-08-07): exactly one menu per screen, stored as a separate assignment record so Schedules can multiplex later without migration.

<sub>Menus.dc.html; decisions.md 35 [code-display]</sub>

### Q3 · BLOCKING

**On upgrade day, do existing menu-layout TVs keep showing their menu (auto-migrated as assigned-and-published, redrawn by the new board renderer) or go to the venue fallback until someone publishes?**

Today TVs auto-show the venue's first active menu through legacy layouts. The new model requires explicit assignment and publish, and nobody has ever published.

*Recommended:* Auto-migrate: current active menu becomes assigned and published to that venue's menu screens — no TV goes dark, accepting the visual change to the new board themes.

*Answer:* Moot (2026-08-07): owner notes the system is not live in production, so no customer TVs exist to go dark. Migration only keeps dev/acceptance fixtures sensible — seeded menus are auto-marked assigned+published so tests and demos work.

<sub>DisplayController.cs GetMenusAsync…IsActive [code-display, code-api-data]</sub>

### Q4 · BLOCKING

**On a screen showing a menu board, do playlists, meal-period switching and date promotions still apply, or is the board the only content (with emergency broadcasts the sole override)?**

Four existing mechanisms fight for a screen's content today (EmergencyBroadcast, PlaylistSlide, MealPeriod, DateRangePromotion); time-based switching collides with criterion 4's "no content change without a deliberate act". A playlist 'menu' slide would also cut the board's page cycle mid-way.

*Recommended:* Board only: emergency broadcast still takes over (cycling continues underneath; Play ignores broadcasts); playlists, meal-periods and promotions keep working only on screens not set to a board.

*Answer:* Owner decision (2026-08-07), broader than recommended: playlists, meal-period switching, and date promotions are OUT OF SCOPE entirely — backlog features for the future. Emergency broadcast remains the sole override. A screen this build shows its board or the venue fallback.

<sub>PlaylistRotation.tsx; DisplayController.cs; EmergencyBroadcastOverlay.tsx; api.ts [code-display, code-backoffice]</sub>

### Q5 · BLOCKING

**When a shared item's price is edited in one menu's builder, what happens on the other menus it sits on — same change joins each menu's own draft queue, instant update on first publish, or per-menu prices?**

The item library means one item on several menus with "shared edits", but decision 2 queues edits per menu and criterion 2 forbids one menu's publish touching another menu's screens. Slice 1's schema depends on the answer.

*Recommended:* The edit joins the draft queue of every menu the item is placed on; each menu's screens change only when that menu publishes.

*Answer:* Owner (2026-08-07): confirmed one item = one shared price across all menus it sits on (per-menu exceptions like happy hour are future features). Mechanics provisional per recommendation: the edit joins each affected menu’s draft queue; each menu’s screens change only on its own publish. FLAG: the editing flow must feel easy — possibly a quick price-change mode — design follow-up required before slice 3 builds the inspector flow.

<sub>decisions.md 2; README criteria 2/4; build-decisions 1 [decisions-doc]</sub>

### Q6 · BLOCKING

**At migration, do identically-named items on different menus merge into one shared library item, or stay separate?**

Merge wrongly and an 86 or edit silently changes a board nobody intended; never merge and the library's shared-edit benefit starts empty (and an 86 on one "Margherita" won't hide the other). Decision 33's name-matching covers imports only.

*Recommended:* Merge only exact name+price+description matches, with the migration script listing every merge; anything that differs stays separate items.

*Answer:* Moot (2026-08-07): no production data exists, so there is no migration day. Slice 1 creates the new schema fresh and re-seeds dev/acceptance fixtures; no merge policy needed. (Slice-plan language updated accordingly.)

<sub>README data model (Placement); api.ts MenuItem; decisions.md 33 [readme-handoff, code-backoffice]</sub>

### Q7 · BLOCKING

**Should slice 1's Item schema support an ordered list of sizes, each with an optional price (blank renders nothing; a short entry like "MP" renders verbatim), with the inspector showing one price row per size?**

The Play hi-fi renders glass/bottle prices ("14 / 54", "— / 98") and MP items; the data model lists sizes[]; today's DB has one required non-negative decimal per item and the M2 inspector draws one Price box. This decides the slice-1 schema.

*Recommended:* Yes — one-or-more named sizes with nullable price from day one; the inspector edits the default size this build, stacking rows for sized items.

*Answer:* Accepted recommended (2026-08-07): items carry one-or-more named sizes with optional price from day one; blank renders nothing, text like "MP" renders verbatim; inspector shows a price row per size.

<sub>README data model; M2c hi-fi; M2 inspector; 012_create_menu_domain.sql [readme-handoff, m2-hifi, code-api-data]</sub>

### Q8 · BLOCKING

**Do you ratify the designer's own lean that board layout zones are a fixed set the theme provides (full width, two columns, sidebar), not free-form placement?**

Explicitly left unsettled in the M2b wireframe ("I'd take fixed"); the slice-2 render engine and slice 5 both depend on it, and switching later means rewriting the engine.

*Recommended:* Fixed — keeps boards looking designed, makes reflow per screen tractable, and is the only version the drawn UI depicts.

*Answer:* Accepted recommended (2026-08-07): fixed zones — themes provide the zone set, sections snap into them. The designer’s own lean is ratified.

<sub>Menus.dc.html M2b "Still to settle" [wf-additems, code-display, record-consistency]</sub>

### Q9 · BLOCKING

**Where does page dwell come from — is it one per-menu number defaulting to 8s, uniform across pages (equal-width timeline blocks), with X4's "Shorten the dwell" fix the only way to change it this build?**

"8s each · 40s" implies uniform, but the hi-fi draws unequal timeline blocks and the README says width = dwell. The codebase's per-screen HeroDwellSeconds is a different feature. This decides schema for the slice-4 player and slice-5 Play.

*Recommended:* Yes — per-menu dwell, default 8s, uniform; equal-width blocks (drawn widths were illustrative); HeroDwellSeconds stays with hero rotation; no general dwell control this build.

*Answer:* Owner (2026-08-07): dwell must be a configurable option. Recorded as a per-menu setting, default 8s, uniform across pages this build (per-page timing stays future); the control needs a small design spot since none is drawn — flagged for the slice-5 design pass.

<sub>M2c hi-fi transport; README timeline; Screen.cs HeroDwellSeconds; X4 fix [m2c-hifi, wf-additems, wf-scale, code-display]</sub>

### Q10 · BLOCKING

**Daily Special — live on boards today, editable from Quick Update, absent from the new design and NOT on decision 6's drop list — drop it in the migration?**

An instant text push violates decision 1 (only 86 is immediate). The migration script must name everything discarded; Menu.DailySpecial, its endpoint, and Home's specials card all exist today.

*Recommended:* Drop it and name it in the migration script; a special later becomes an ordinary featured item you publish.

*Answer:* Owner (2026-08-07): Daily Special is backlogged for the future — removed from this build like playlists/meal-periods/promotions, not carried into the new schema.

<sub>Menu.cs DailySpecial; QuickUpdateMode.tsx; DaypartHome.tsx; build-decisions 6 [code-backoffice, code-api-data]</sub>

### Q11 · BLOCKING

**For venues with a POS connected: keep two-way availability sync live (attributed to the provider name, e.g. "Clover", shown like a person's), but suspend POS price/quantity writes until the deferred POS-pricing design?**

Clover/Square/Toast handlers write price, quantity and availability straight to live items today, bypassing any draft. Quantity is dropped and POS pricing deferred, but a POS sold-out flip is a fact from the till — though auto-on genuinely bends decision 14's "until a person turns it back on", so it's your call.

*Recommended:* Yes — availability sync stays live both ways with provider attribution; price/quantity writes stop in slice 1; catalog mappings carry over to the new item IDs.

*Answer:* Owner (2026-08-07): POS sync to menu items is out of scope entirely for now — no price, quantity, or availability writes from POS into the new item tables — until that area is dug into later. POS integration surfaces otherwise untouched.

<sub>CloverRealtimeSyncHandler.cs; ToastInventorySyncService.cs; decisions.md 1; build-decisions 14 [wf-additems, code-api-data]</sub>

### Q12 · BLOCKING

**What does "Review" show? Three surfaces link to a review of queued changes (M1 card "Review →", M2 "Review first", X1 at scale) but the surface is drawn nowhere.**

The simplest honest version is a sheet listing each queued change (what, before → after, who, when) over the builder, with the target-screen summary and Publish/Back; M1's "Review →" (whole amber bar as one click target) opens the builder with it up.

*Recommended:* Yes — one change-list sheet reusing the slice-1 draft-queue data, used by all three entry points; visual before/after diffing stays out.

*Answer:* Accepted recommended (2026-08-07): one change-list sheet (what, before → after, who, when + target screens + Publish) shared by all three entry points. No visual diffing this build.

<sub>README publish bar + pending bar; M1/M2/X1 drawings [readme-handoff, m1-hifi, m2-hifi, wf-import-actions, wf-scale]</sub>

### Q13 · BLOCKING

**Venues have no logo anywhere in the system — until a logo upload exists, is the generated fallback card a monogram (venue initial in the board style) plus the venue name?**

Decision 14 specifies a "generated logo-and-name card" but Venue.cs has no logo field and no upload flow exists; slice 2 must render the card regardless.

*Recommended:* Yes — monogram + name now, plus a nullable logo slot in the schema so a later build fills it without redesigning the card.

*Answer:* Owner (2026-08-07): venues should upload a logo during onboarding; when none exists the system generates one (the initial-and-name card). Scope addition: Venue gains a nullable logo + a minimal upload point (lands with the fallback card in slice 2; onboarding integration when onboarding is built).

<sub>decisions.md 14; Venue.cs (no logo) [decisions-doc, readme-handoff, code-display, code-api-data]</sub>

### Q14 · BLOCKING

**Between the slice-1 schema migration and the slice-3 builder, must the current menu editor keep fully saving, or may it go read-only?**

"Keeping the current editor compiling" is ambiguous; read-only kills two autosave Playwright specs at slice 1 and leaves menus uneditable for a slice or two, but a save bridge is throwaway work.

*Recommended:* Keep it saving via a thin bridge onto the new tables (every slice must leave master releasable); the autosave specs retire with the editor in slice 3.

*Answer:* Moot (2026-08-07): nothing is live, so no save bridge — the old editor may go dark between slices 1 and 3; its autosave Playwright specs retire at slice 1.

<sub>slice-plan slice 1; menu-save-race.spec.ts; build-decisions 2 [code-api-data]</sub>

### Q15 · BLOCKING

**For the window between slice 3 (builder) and slice 4 (real geometry), may the canvas render against a stand-in 1920×1080 labeled honestly ("your screens haven't reported yet"), with the description wrap-warning absent until real geometry lands?**

Decision 13 forbids representative sizes, but the approved plan ships the builder one slice before device geometry exists; the wrap warning ("wraps to 3 lines on Patio") cannot be truthfully computed until slice 4.

*Recommended:* Yes — labeled stand-in, label removed the moment slice 4 lands; wrap warning lights up only with real reports; Play stays absent until slice 5.

*Answer:* Accepted recommended (2026-08-07): labeled 1920×1080 stand-in for the one-slice window; label removed when real geometry lands; wrap warning only ever computes from real reports.

<sub>slice-plan slice 3 vs decisions.md 13; build-decisions known gaps [record-consistency, m2-hifi]</sub>

### Q16 · important

**Is history retention measured in VERSIONS (decisions.md 8: "how many versions you keep") or DAYS (the drawn footer: "Your plan keeps the last 30 days")?**

Direct conflict inside the bundle; your decision 3 set retention as tier-configurable with numbers TBD but never the unit, and the slice-1 entitlement config needs one.

*Recommended:* Versions — a per-tier count of published versions per menu, oldest pruned; decisions.md wins on conflict, and the footer sentence becomes "Your plan keeps your last N versions."

*Answer:* Owner (2026-08-07): re-affirmed retention is configurable per tier. Unit not chosen — provisionally VERSIONS (decisions.md wins the bundle conflict); config stores a per-tier version count. Open to flip to days before slice 1 hardens.

<sub>decisions.md 8 vs Menus.dc.html Go-back footer; build-decisions 3 [decisions-doc, wf-import-actions]</sub>

### Q17 · important

**Should retention only ever shorten the "Go back to…" list, never deleting a version any screen is still actually showing?**

An offline screen can be showing a version retention would otherwise prune; multi-venue later depends on the same rule.

*Recommended:* Yes — prune history visibility, never a version something is still running.

*Answer:* Owner (2026-08-07): data retention is a whole topic needing its own Q&A session soon — backlogged for this build. Provisional: nothing is pruned at all this build; the per-tier retention config exists but is effectively unlimited until the retention discussion.

<sub>build-decisions 3; MV wireframes "Behind 9 days" [multivenue-fwdcompat]</sub>

### Q18 · important

**Is the draft genuinely one communal queue per menu — anyone's Publish ships everyone's queued changes, last edit to a field wins, no locking or live sync, counts refresh on open/focus, byline shows the latest author, with per-change attribution in Review and history?**

Two people (or two tabs) share one queue; "Draft saved 10:42am by Alex" implies multi-author. Nothing specifies collisions or mid-edit publishes.

*Recommended:* Yes to all — shared queue, last-write-wins per field, attribution keeps it honest; live presence can come later.

*Answer:* Owner (2026-08-07), overrides recommendation: single-editor lock — one user edits a menu’s draft at a time. Shared/concurrent editing is a future enhancement, possibly tier-gated. Lock mechanics (acquire on entering builder, inactivity timeout, takeover warning) are implementation details.

<sub>decisions.md 2; M2 publish bar [decisions-doc, readme-handoff, m2-hifi, code-backoffice, code-api-data, wf-scale]</sub>

### Q19 · important

**Confirm undo's reach: Ctrl+Z / the ↺ button reverse draft edits only (never an 86 — that reverses only via its toast Undo or the switch), and the undo stack clears on Publish so "Go back to…" is the only path back afterwards?**

86 changes real screens instantly; a keystroke silently flipping one would change TVs from what someone thought was text editing.

*Recommended:* Yes to both — every live change stays behind its own deliberate act; publish is a clean boundary.

*Answer:* Accepted recommended (2026-08-07): Ctrl+Z reverses draft edits only, never an 86; undo stack clears on Publish — "Go back to…" is the only path back afterwards.

<sub>decisions.md 3/7; M2 top bar [decisions-doc, m2-hifi]</sub>

### Q20 · important

**Duplicate: does it copy the menu's current draft state into a new never-published menu named "<Name> copy" (shelf, no screens) that PLACES THE SAME library items — so later shared edits ripple to both — rather than forking independent copies?**

Under the library model each choice surprises a different user; sharing is the point of the library and "Also on…" makes it visible; someone wanting a different price creates a new item.

*Recommended:* Yes — shared items, draft state, "<Name> copy", lands on the shelf on no screens.

*Answer:* Owner (2026-08-07, incl. follow-up): duplicates place the SAME shared items; a price edit on a shared item changes it across all menus holding it, and menus are versioned like transactions so published versions form an audit timeline of what price was on screen when. Exact delivery moment for the other menus’ screens rides the Q5 design follow-up (provisional: each menu updates on its own publish).

<sub>M1b ⋯ menu; build-decision 1 [decisions-doc, readme-handoff]</sub>

### Q21 · important

**Should an import appear as ONE pending change ("Replaced from import · 45 items") in the bar and history, and should a never-published menu's card read "Never published · not on a screen" instead of counting dozens of changes?**

The pending bar counts changes and history lists field-level entries; a 45-item import or brand-new menu breaks field-level counting.

*Recommended:* Yes — one entry per import; never-published cards get the status line.

*Answer:* Accepted recommended (2026-08-07): an import is ONE pending change ("Replaced from import · 45 items") in bar, Review and history; never-published menus read "Never published · not on a screen".

<sub>decisions.md 30/32; M1 pending bar [decisions-doc]</sub>

### Q22 · important

**Define "current" for the headline math: a screen is current when its applied revision equals the latest published one (drafts never make it non-current); a screen showing the venue fallback counts as current ("· N showing your venue card" appended when applicable); screens running non-menu content (playlists, tap lists) stay out of the Menus sentence entirely?**

Decision 12's promise ("you can always tell whether your screens are current") never defines the word for fallback, non-menu, or draft-holding screens; the codebase's applied-vs-authoritative model already matches this.

*Recommended:* Yes to all three.

*Answer:* Accepted recommended (2026-08-07): all three confirmed — current = showing latest published version; fallback counts as current; non-menu screens stay out of the sentence.

<sub>decisions.md 12; X2 "18 of 20 current"; ScreenContentDeliveryService [decisions-doc, wf-scale]</sub>

## Plans, permissions and the app shell

### Q23 · BLOCKING

**Before slice 2 gates the nav: should "Menus" be its own new feature key in the tier matrix (not piggybacking the quick_update flag), with the Menus-area matrix rows drafted and approved now, and every existing account keeping Menus at launch?**

Nothing today connects a SubscriptionTier to any capability — the Release–Capability–Tier matrix was never drafted — and the current nav item borrows the quick_update flag, which the design treats as a separate switch. Gating turns on the day slice 2 ships.

*Recommended:* Yes — dedicated Menus feature key, matrix rows drafted for the Menus area only, enabled on all current tiers at launch so gating bites only through deliberate plan changes later.

*Answer:* Owner (2026-08-07): no migration concerns; every tier will most likely have Menus — the tiering applies to FUNCTIONS INSIDE Menus, not the area. Nav gates nothing at launch; the Release–Capability–Tier matrix rows get drafted around intra-Menus functions in a later pricing pass.

<sub>decisions.md 19; navigation.mjs capabilityId; built-foundations-spec.md; CapabilityAccessPolicyRepository [decisions-doc, code-backoffice, code-api-data]</sub>

### Q24 · BLOCKING

**May we add three new capability IDs — content.menu.manage (nav gate), publishing.history.view, content.menu.import — granted by default to every role that currently holds content.item.update?**

Decision 8 makes history its own capability; none exists, and import and the area gate have no IDs. New IDs touch the registry, DB seed, and role grants; nobody should lose access on upgrade day.

*Recommended:* Yes — three IDs, auto-granted alongside today's item-edit capability.

*Answer:* DEFERRED (2026-08-07). Provisional: the three capability IDs (content.menu.manage, publishing.history.view, content.menu.import) land auto-granted to item-editing roles so gating can be wired; flagged in slice-1 demo for review.

<sub>CapabilityModel.cs; SystemRoleRegistry; decisions.md 8 [code-api-data]</sub>

### Q25 · BLOCKING

**What does the rail's "Settings" open, given Billing and Account & security have no rail slot and several current labels can't fit under a 56px icon?**

The 76px rail becomes the app-wide shell, but no settings route exists; existing areas need short labels (e.g. "POS", "Taps").

*Recommended:* Settings opens exactly the two existing pages — Billing and Account & security — unchanged inside the shell; other areas keep their routes with short labels (POS, Billing, Account, Taps); no new Settings area this build.

*Answer:* DEFERRED (2026-08-07). Provisional: rail Settings opens existing Billing + Account & security unchanged; short labels elsewhere; no new pages. Flagged in slice-2 acceptance workbook.

<sub>M1 Hi-Fi v2 rail; navigation.mjs; BackOfficeNav.dc.html [readme-handoff, m1-hifi, code-backoffice, record-consistency]</sub>

### Q26 · BLOCKING

**For non-Menus areas below tier: does the rail item stay visible in the normal style, clicking through to the existing LockedSectionPreview content — with the SidebarUpgradeNudge simply not carried into the rail (its host sidebar is deleted and it physically doesn't fit)?**

Only Menus goes invisible below tier (decision 9 scopes decision 4). "Untouched" upgrade surfaces and "replaced shell" collide for the nudge; no locked rail treatment is drawn.

*Recommended:* Yes — visible rail item → existing locked-preview content; the nudge stops rendering until the planned upgrade/marketing rework; other upgrade surfaces stay live.

*Answer:* DEFERRED (2026-08-07). Provisional: below-tier non-Menus areas stay visible in the rail and click through to the existing locked preview; the sidebar upgrade nudge disappears with the sidebar. Flagged in slice-2 acceptance workbook.

<sub>build-decisions 9/12; App.tsx LockedNavigationItem; SidebarUpgradeNudge.tsx [m1-hifi, readme-handoff, code-backoffice, record-consistency]</sub>

### Q27 · important

**What's in the avatar menu — name/role header, workspace/venue switcher (shown only when there's more than one), Sign out — keeping the existing switch-confirmation dialog but reworded now drafts are server-side?**

The README puts switching "under the avatar" but the popover is never drawn; today sign-out is one accidental click and the switch warning ("save unfinished edits") reads wrong once drafts autosave to the server.

*Recommended:* Yes — name/role, switcher, Sign out in that order; keep confirm-and-recheck, reword: access is rechecked, drafts are safe.

*Answer:* Accepted recommended (2026-08-07): avatar menu = name/role, venue-workspace switcher (only when more than one), Sign out; switch confirmation kept and reworded for server-side drafts.

<sub>README M1; App.tsx identity button [m1-hifi, code-backoffice]</sub>

### Q28 · important

**On pages NOT being redesigned, do the current top header ("Venue workspace / Secure session") and the "Active workspace" banner stay, or does the new shell remove them app-wide?**

The M1 hi-fi has none of this chrome; "other areas keep their current content unchanged" is ambiguous between content and shell.

*Recommended:* Treat them as shell: remove both app-wide (the avatar takes over identity and switching); every area's own headings and content stay untouched.

*Answer:* Accepted recommended (2026-08-07): old top chrome (workspace header + active-workspace banner) removed app-wide; the avatar owns identity and switching; each area’s own content untouched.

<sub>App.tsx; build-decisions 12 [code-backoffice]</sub>

### Q29 · important

**Confirm the purpose-built static-content home for below-tier venues is NOT in this build — they simply have no Menus nav item and keep whatever they use today?**

Decision 19 references that screen but it appears in no artifact or slice.

*Recommended:* Yes — out of scope; it arrives with the deferred upgrade/marketing rework.

*Answer:* Owner (2026-08-07), approved deviation from decisions.md 19: there is NO separate purpose-built static-content home. The Content area is one surface whose functions are tier-gated — each tier sees only the functions necessary for what it can do (consistent with Q23: area present for all tiers, functions tiered).

<sub>decisions.md 19; slice-plan slice 2 [decisions-doc, readme-handoff]</sub>

### Q30 · important

**What is the durable top-level nav label — "Menus" (every breadcrumb and headline in the design), "Menu" (the current nav and the hi-fi rail), or "Content"?**

The designer recorded this as an open question; the rail label becomes the area's durable name everywhere from slice 2.

*Recommended:* "Menus" — matches the area's own naming throughout the bundle, and the area holds several menus.

*Answer:* Owner (2026-08-07), overrides recommendation: the durable top-level label is "Content". The rail item and area name are Content; the objects inside remain menus ("Summer Menu", "Add a menu"). Aligns with the product-surface inventory’s Content-area architecture and leaves room for future content types.

<sub>github.md open questions; navigation.mjs; M1 Hi-Fi rail [readme-handoff, record-consistency]</sub>

### Q31 · important

**Should builder, Board view, Play and Quick Update each get their own web address (sub-routes under #/menu — /build, /quick, /play), shipped as the segmented-control tabs on one menu-scoped route (no separate staff nav entry until the users/roles work)?**

Only #/menu exists today, so refresh/Back would dump users on the shelf; the designer left tabs-vs-nav-entries open, and a builder-hidden staff route is genuinely a permissions feature.

*Recommended:* Yes — sub-addresses with old #/menu bookmarks landing on the shelf; tabs as drawn; the permissions-driven split waits for the roles build.

*Answer:* Owner (2026-08-07), overrides recommendation: one address with tabs in memory — no sub-routes this build; refresh/Back return to the shelf. Sub-addresses can come later if it annoys.

<sub>navigation.mjs; github.md open question; Menus.dc.html M3 "Still to settle" [readme-handoff, code-backoffice, wf-additems, record-consistency]</sub>

### Q32 · important

**Should the 86 toggle require only the lighter (currently unused) content.item.availability_update capability, so bartenders can use Quick Update without price/wording rights — with everything else keeping the edit capability?**

Today every menu endpoint, including quick-availability, demands full item-edit; the registry already defines the lighter capability.

*Recommended:* Yes — gate the M3 toggle (and only it) with availability_update.

*Answer:* Accepted recommended (2026-08-07): the 86 toggle (and only it) gates on the lighter content.item.availability_update capability; everything else keeps full item-edit.

<sub>BackOfficeMenusController.cs RequireCapability; CapabilityModel.cs [code-api-data, decisions-doc]</sub>

### Q33 · important

**Prune the three upgrade-catalog entries that sell removed features — happy_hour, bilingual_display, ai_translation — so nobody pays for capabilities the migration deletes?**

Decision 9 keeps upgrade surfaces untouched, but after decisions 6/15 these entries start a paid checkout for features that no longer exist.

*Recommended:* Yes — remove just those three; everything else waits for the marketing rework.

*Answer:* Accepted recommended (2026-08-07): remove the happy_hour, bilingual_display and ai_translation upgrade-catalog entries; all other upgrade surfaces wait for the marketing rework.

<sub>upgradeExperience.mjs upgradeCatalog; build-decisions 6/15 [code-backoffice]</sub>

### Q34 · important

**Do the three onboarding starter-menu links (Restaurant / Cafe / Bar, deep-linking into the old editor) go away, with new customers landing on the "Let's get your menu in." empty state instead?**

The new M1 empty state has no template concept; the routes are paste and start-blank.

*Recommended:* Yes — retire the starter links and name pre-fill; the empty state IS the onboarding (decision 17).

*Answer:* Owner (2026-08-07): starter templates are a LAUNCH REQUIREMENT, not retired — the product is not live, and by launch the empty state offers "start from a template" alongside paste/blank. Old-editor deep-links retire with the old editor. Template content TBD.

<sub>CustomerOnboardingApp.tsx:293; decisions.md 17 [code-backoffice]</sub>

### Q35 · important

**Home's two menu widgets: rewire the 86-board card to venue-level availability (dropping the "n remaining" sub-line), and remove the Today's-special card with the daily-special feature?**

Home isn't redesigned, but both cards are wired to concepts the migration deletes (quantityAvailable, "the active menu", DailySpecial) — "unchanged" is impossible.

*Recommended:* Yes — same 86 card with new wiring; specials card removed.

*Answer:* Owner (2026-08-07): out of scope — no design work on Home. Mechanical consequence only: widgets wired to deleted concepts (quantity remaining, Today’s special, "active menu") are stubbed/removed so Home compiles and renders; nothing else changes.

<sub>DaypartHome.tsx:109-117; build-decisions 6 [code-backoffice]</sub>

### Q36 · important

**Should the internal platform-operations menu tooling go read-only for menus this build, rather than keeping instant live edits that bypass the draft spine?**

Ops-side edits write straight to live tables — a screen would change without a publish, contradicting criterion 4; the slice-1 migration breaks those endpoints anyway.

*Recommended:* Read-only this build; support edits return later through the same draft/publish API.

*Answer:* Owner (2026-08-07), corrects the premise: the intended platform-ops model is (a) log in AS the venue in read-only mode, and (b) publish/change data only by assuming the local user WITH customer permission granted via a code system. That impersonation+consent system is out of scope and BACKLOGGED. This build: old direct-edit ops menu endpoints retire with the old tables; ops has no menu write path.

<sub>PlatformOperationsMenusController.cs; platform-operations QuickUpdateMode.tsx [code-api-data]</sub>

### Q37 · important

**Below 1440px, confirm the responsive plan: the 76px rail stays fixed down to 760px (current hamburger-drawer collapse below that); the M1 grid drops 4-up→3-up below ~1280 (1024 floor); the builder supports down to 1280 with only the canvas narrowing; Play keeps its sidebar and scales the board — nothing stacks, no horizontal scroll; anything narrower waits for the flagged mobile pass?**

The hi-fis are fixed 1440×940 frames and mobile is explicitly out of scope, but 1280–1366 laptops are normal desktop use; "always 4-up" at 1100px makes boards unreadable.

*Recommended:* Yes to all — the smallest departure from the hi-fis, matching how the app already degrades.

*Answer:* Accepted recommended (2026-08-07): rail fixed; shelf 4→3-up below ~1280; builder to 1280 by narrowing the canvas; Play scales; no horizontal scroll; <~1024 waits for the mobile pass.

<sub>README responsive; styles.css:78 @760px; M1/M2/M2c fixed frames [readme-handoff, m1-hifi, m2-hifi, m2c-hifi, code-backoffice]</sub>

### Q38 · minor

**May transactional banners (Stripe checkout return, plan confirmation) still appear above the new Menus home, while inline UPGRADE hints are suppressed on Menus surfaces only?**

The M1 hi-fi shows no banners; upgradeExperience maps two hints to the menu panel, which would sit over the redesigned shelf.

*Recommended:* Yes — keep the rare transactional banners everywhere; suppress inline hints on Menus only.

*Answer:* Owner (2026-08-07): out of scope — banner behavior unchanged; upgrade hints that target replaced/deleted surfaces simply have nowhere to render. Revisited in the marketing rework.

<sub>App.tsx banners; upgradeExperience.mjs [code-backoffice]</sub>

## Data model, migration and forward-compat

### Q39 · BLOCKING

**Since slice 1 creates the Menu table fresh, should it record owner as "owner type + owner id" (always 'venue' this build, invisible in the UI) so group menus later need no rewrite of Menu and every FK against it?**

Multi-venue requires venue-or-group ownership; retrofitting a required VenueId later is the single most expensive migration. Costs nothing in a fresh table; decision 29's no-leak rule is preserved.

*Recommended:* Yes — owner type + id, defaulted to venue; trust-level and venue-state tables stay entirely in the multi-venue build.

*Answer:* Accepted recommended (2026-08-07): Menu records owner type + owner id (always venue this build, invisible in UI).

<sub>README data model "owner (venue | group)"; Menu.cs VenueId [multivenue-fwdcompat, readme-handoff]</sub>

### Q40 · important

**Should the new Item table mirror the same owner shape (owner type + id, always 'venue' this build), so group-authored items later slot in without re-scoping the library?**

A group menu's placements must reference items usable across venues; hard venue-only constraints would force re-scoping every Placement and 86 row later.

*Recommended:* Yes — same columns, same reasoning; 86 stays item × venue either way.

*Answer:* Accepted recommended (2026-08-07): Item mirrors the same owner shape; 86 remains item × venue.

<sub>slice-plan slice 1; decisions.md 20 [multivenue-fwdcompat]</sub>

### Q41 · important

**Should each publish-target row record a target type ('screen' now, 'venue' possible later) plus an extensible status (room for "waiting on acceptance", empty accepted-by/at slots)?**

Multi-venue sends target venues that accept on their own schedule; a hard FK to Screen with done/failed would mean a parallel pipeline later.

*Recommended:* Yes — parallels the applied-vs-authoritative model already in the codebase; invisible this build.

*Answer:* Accepted recommended (2026-08-07): publish-target rows carry target type (screen now, venue-capable) and an extensible status.

<sub>README PublishEvent; slice-plan slice 1 [multivenue-fwdcompat]</sub>

### Q42 · important

**Lock in that every DraftChange is structured data — exact item/section id, named field, typed before/after values — with the human sentence generated from it, never the other way round?**

Multi-venue's conflict preview ("your $16 over their $17") can only be computed from structured rows; free-text labels make it impossible forever. An optional nullable "whose draft" scope column now also lets a venue's local draft coexist on a group menu later without re-keying.

*Recommended:* Yes — structured rows plus the nullable scope column; sentences rendered from data.

*Answer:* Accepted recommended (2026-08-07): DraftChange is structured (entity id, named field, typed before/after) with sentences generated from data; includes nullable draft-scope column. Enables the Q20 price-audit timeline.

<sub>slice-plan DraftChange; MV2a conflict preview; decisions.md 26 [multivenue-fwdcompat]</sub>

### Q43 · important

**Lock in that an item keeps the same identity forever — published versions snapshot values but never re-mint item ids — so 86s (and later venue overrides) survive every publish by construction?**

If publishing cloned items into snapshot copies, every 86 would silently lose its anchor on the next publish — the exact bug criterion 3 forbids.

*Recommended:* Yes — reject any per-version item-copying design in review.

*Answer:* Accepted recommended (2026-08-07): item identity is permanent; versions snapshot values, never re-mint items; per-version copying designs rejected in review.

<sub>MV2a/MV3 "86s always survive a publish"; slice-plan slice 1 [multivenue-fwdcompat]</sub>

### Q44 · important

**For history entries like "Patio caught up at 10:51": is copying the terminal per-screen outcome onto the PublishEvent enough, letting the raw delivery rows keep their existing 90-day purge?**

The delivery counter carries no content identity and purges at 90 days; tier history may keep versions longer.

*Recommended:* Yes — snapshot the terminal state per publish; keep the purge; history stays truthful without an ever-growing table.

*Answer:* DEFERRED (2026-08-07) into the backlogged data-retention discussion. Provisional: publish records snapshot final per-screen outcomes; raw logs keep their 90-day purge.

<sub>ScreenContentDeliveryService.cs 90-day DELETE; slice-plan slice 3 [code-api-data]</sub>

### Q45 · important

**Migration of hidden data: archived/inactive menus land in the "Not in use" strip (chip date = last-updated); archived/inactive items become unplaced library items; archived sections aren't recreated (their items land unplaced); nothing deleted, nothing resurfaced, all named in the migration script?**

The new world has no archive concept (criterion 5 bans the word); today's data has isActive flags and archive/restore lifecycles that must land somewhere explicit.

*Recommended:* Yes to all.

*Answer:* Owner (2026-08-07): fresh start — new tables begin with seed/demo data only; old tables stay untouched but unused. A carry script remains possible any time before the old tables retire.

<sub>MenuSectionsEditor/MenuItemsEditor archive flows; build-decisions 16; slice-plan slice 1 [code-backoffice, code-api-data]</sub>

### Q46 · important

**Board theme: a per-menu field (Coastal / Classic dark) chosen in the builder, entirely separate from the existing venue-wide VenueTheme — which keeps governing legacy layouts and tap boards, and is never consulted by the board renderer — with migrated menus starting on Classic dark (closest to today's dark boards)?**

Two theming systems will coexist; the Themes area's live-push changes must not fight the board on one TV, and no theme picker is drawn anywhere.

*Recommended:* Yes — per-menu theme, Themes area untouched, migrated menus start Classic dark.

*Answer:* Owner (2026-08-07): confirmed — they are two different things. Venue Themes and per-menu board looks are separate systems; the board renderer never consults the venue theme.

<sub>README data model; VenueTheme.cs; DisplayController theme push [readme-handoff, code-display, code-backoffice, code-api-data]</sub>

### Q47 · important

**Which app do production TVs actually run — src/tv or src/display — so slice 4 targets that one (renderer built shared so the other can adopt it later)?**

The repo has two display front-ends and the handoff maps Play to both; the answer decides where slice 4's work lands.

*Recommended:* Target the one production TVs run (please name it — we'd guess src/tv), renderer shared.

*Answer:* Owner (2026-08-07): src/display is the main player — it runs on the web and inside the TV apps. Slice 4 targets src/display; TV apps inherit via the embedded player.

<sub>github.md screen map A3; repo src/display + src/tv [readme-handoff]</sub>

### Q48 · important

**If a board screen is offline for a very long time, does it keep showing the last published board indefinitely rather than expiring to an error card?**

Today's player discards cached content after 7 days and shows "Display unavailable" — a working TV showing a correct menu would error purely from a week of bad network.

*Recommended:* Keep — a published board never expires on the player; the fallback appears only when nothing is published.

*Answer:* Accepted recommended (2026-08-07): a published board never expires on the player regardless of offline duration; fallback only when nothing is published; back office reports offline honestly.

<sub>displayCache.mjs 7-day max age [code-display]</sub>

### Q49 · important

**Should guest-facing board screens never show connection/offline status text (the board keeps rendering from cache; back-office chips carry the honesty)?**

Today's player draws "Offline — showing saved content…" boxes on the TV; the design puts delivery truth in back-office chips and never on a board.

*Recommended:* Suppress — no status chrome on boards; guests never read our plumbing.

*Answer:* Accepted recommended (2026-08-07): no connection/status chrome on guest-facing boards; delivery honesty lives in the back-office chips.

<sub>player.css .player-status--offline; displayPresentation.mjs [code-display]</sub>

### Q50 · important

**Slice-4 geometry: include safe area in the heartbeat where the platform exposes it, but defer the per-screen overscan-correction field and control entirely until Screens work designs it?**

The wireframe lists safe area in the report and puts the correction "under Screens" — an area out of scope; nothing in the six slices consumes the correction and its authorship rule is unresolved.

*Recommended:* Yes — report safe area opportunistically; omit overscan correction (no field, no UI) this build.

*Answer:* Owner (2026-08-07): overscan correction backlogged for the future. Heartbeat reports what platforms expose (resolution, orientation, safe area when available); no correction field or UI this build.

<sub>Menus.dc.html M2c geometry note; README Screen model; slice-plan slice 4 [record-consistency, readme-handoff, code-display]</sub>

### Q51 · important

**Keep Harbor Acceptance Venue as this build's acceptance venue, with its fixture script rewritten to the new model (same stable IDs) in the same PR as the slice-1 migration?**

The fixture MERGEs into tables the migration restructures; every per-slice workbook needs the seeded venue immediately.

*Recommended:* Yes — same venue, users and menu restated in the new schema, updated in the migration PR.

*Answer:* Owner (2026-08-07, incl. follow-up): AUTH REWORK — the login system is reworked in a dedicated later build (backlogged); until then development runs with open access (auto-session as the venue owner, no login ceremony). The capability model keeps working underneath. Acceptance workbooks lose sign-in steps; login-dependent Playwright specs retire or adapt to auto-auth.

<sub>docs/acceptance/track-1-owner-fixture.sql [code-api-data]</sub>

### Q52 · minor

**Defer the venue "default user" field entirely to the multi-venue build (nothing in slices 1–6 reads it; decision 27 only requires it when a venue joins a group)?**

The README data model lists it as required, but adding a required setup field now touches venue creation outside Menus, and single-venue escalation has nowhere to go.

*Recommended:* Yes — defer; the multi-venue build adds it nullable and enforces it at group setup.

*Answer:* Owner (2026-08-07), overrides recommendation: add the venue default-user field NOW — required at venue creation per decision 27, pointing at one of the venue’s users; seeded venues get their owner. Escalation behavior still arrives with multi-venue.

<sub>decisions.md 27; README data model; Venue.cs [decisions-doc, multivenue-fwdcompat]</sub>

### Q53 · minor

**When multi-venue arrives, is "the group" simply your existing Organization (Coast & Co = one org, its 12 venues), or could one org contain several menu groups (brands)?**

Owner columns added now must point at something; the codebase already has Organization with venues attached.

*Recommended:* Treat Organization as the group; a brand split would be its own build with its own migration either way.

*Answer:* Accepted recommended (2026-08-07): Organization is the group; owner columns will point at it when multi-venue arrives; brand-splitting would be its own future build.

<sub>MV1 nav; Organization.cs; Venue.OrganizationId [multivenue-fwdcompat]</sub>

### Q54 · minor

**Is independent page-cycling per TV acceptable this build — two side-by-side TVs on the same menu may drift out of step — with lockstep sync a later nicety?**

A SyncTick event already exists if lockstep is ever wanted; side-by-side walls can look broken, across-the-room pairs are fine.

*Recommended:* Independent this build.

*Answer:* Accepted recommended (2026-08-07): independent page-cycling per TV this build; lockstep sync is a later nicety.

<sub>signalRTypes.ts syncTick [code-display]</sub>

### Q55 · minor

**For item names/descriptions in Chinese/Japanese/Korean/Arabic, is falling back from Playfair to the already-bundled Noto fonts (a plainer look beside Playfair) acceptable?**

Playfair has no CJK/Arabic glyphs and menus today contain those scripts; per-script serif companions are a later polish item.

*Recommended:* Yes — bundled Noto fallback, mixed look accepted.

*Answer:* DEFERRED (2026-08-07). Provisional: bundled Noto fallback for non-Latin scripts; per-script serif companions revisited later.

<sub>build-decisions 10; notoFonts.mjs [code-display]</sub>

## Menus home (M1)

### Q56 · BLOCKING

**With only paste + start-blank shipping, what do the empty state and Add-a-menu tile show — a single "Paste text" card wearing the drawn highlight treatment plus the "or start from a blank board" link (same centered layout, headline "Let's get your menu in." verbatim), and tile sub-copy "Paste your menu / or start blank"?**

The hi-fi advertises Photo (highlighted) and Spreadsheet in both the tile and empty state; decision 4's spirit forbids dead affordances, and routes return unchanged as they ship. Header button and tile open the same chooser.

*Recommended:* Yes — trim to Paste (highlighted) + blank link everywhere; never render a route that doesn't work.

*Answer:* Accepted recommended (2026-08-07): empty state and Add tile show only shipping routes (Paste highlighted + start-blank link; template card when templates land); dead routes never render.

<sub>M1 Hi-Fi v2 empty state + tile; decisions.md 4/17; build-decisions 5 [decisions-doc, readme-handoff, m1-hifi, wf-import-actions, record-consistency]</sub>

### Q57 · BLOCKING

**May we draft the full catalogue of M1 headline + sub-line sentences (all current; some offline; nothing published; zero screens paired; mixed menus per screen; fallback showing) in the drawn two-part shape, for you to sign off word-for-word at the slice-2 workbook?**

Exactly one headline scenario is drawn; the sub-line "All 3 screens are set to Summer Menu" only works when they match. Mixed-menu form: "Bar and Patio are showing Summer Menu · Lobby is showing Late Night" (summarize the normal, name the exception).

*Recommended:* Yes — engineer drafts, owner approves exact wording before it ships.

*Answer:* Accepted recommended (2026-08-07): full headline sentence catalogue drafted by the engineer; owner approves exact wording at the slice-2 workbook.

<sub>M1 Hi-Fi v2 headline; decisions.md 12 [m1-hifi, wf-import-actions]</sub>

### Q58 · BLOCKING

**Drop the "Keep more →" upgrade link from the Go back to… footer, keeping only the factual retention sentence (with the account's real number)?**

An upgrade-discovery affordance drawn inside Menus directly conflicts with your decision 9 deferring upgrade discovery.

*Recommended:* Yes — plain sentence only; the link returns with the marketing rework.

*Answer:* Owner (2026-08-07): say nothing there for now — the Go back to… panel carries neither the retention sentence nor any upsell link. (Retention messaging returns with the retention discussion / marketing rework.)

<sub>Menus.dc.html Go-back footer vs build-decisions 9; decisions.md 4 [wf-import-actions, record-consistency]</sub>

### Q59 · important

**Is "Put away" offered only for menus on zero screens (absent from the ⋯ menu while live), so taking a live menu down always goes through the Take-off dialog and its fallback preview first?**

Allowing it live would empty screens without the consequence-stating dialog decision 10 requires. One reviewer preferred disabled-with-reason; absence matches the inapplicable-action rule and most reviews.

*Recommended:* Yes — absent while on a screen; take it off first, then put it away.

*Answer:* Owner asked for a workflow; recorded proposal (2026-08-07): the ⍏ menu shows exactly one state-matched action — on TVs → "Take off the screens" (with preview); off TVs → "Put away"; in the Not-in-use strip → "Put back on the shelf" (and placing it on a screen also returns it). Never both at once; one click can never blank a TV.

<sub>decisions.md 10; build-decisions 16 [decisions-doc, readme-handoff, m1-hifi, wf-import-actions, record-consistency]</sub>

### Q60 · important

**Card thumbnail and counts: both show the PUBLISHED menu (what the TVs show) — the thumbnail being page 1 rendered by the shared board engine at 16:9 — falling back to the draft only for never-published menus (an empty blank menu renders theme background + title)?**

A menu with pending changes could show either state; the shelf answers "what are my screens showing" and the amber bar already flags the draft. A 41-item menu can't fit a thumbnail, so page 1 as the TV shows it.

*Recommended:* Yes — published wins, page 1, engine-rendered, zero special cases.

*Answer:* Accepted recommended (2026-08-07): card thumbnail + counts show the published menu (page 1, engine-rendered, 16:9); draft shown only for never-published menus; the amber bar covers pending changes.

<sub>M1 card + "7 sections / 41 items"; README assets [readme-handoff, m1-hifi, wf-import-actions]</sub>

### Q61 · important

**Shelf order: menus currently on screens first, then the rest by most recently edited — no manual card reordering this build?**

"Menus in order" never says which; the hi-fi's arrangement could be status order or coincidence, and no drag-to-reorder is drawn.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): shelf order = on-screen menus first, then most recently edited; no manual reordering this build.

<sub>README M1 grid; M1 Hi-Fi v2 [m1-hifi]</sub>

### Q62 · important

**When menus already exist, do both Add-a-menu affordances (header button and grid tile) open one modal reusing the exact empty-state route-card component?**

The empty state IS the route picker, but the populated-shelf chooser is never drawn; one picker to build and maintain.

*Recommended:* Yes — one modal, both entry points.

*Answer:* Accepted recommended (2026-08-07): one add-a-menu chooser modal (the empty-state route cards) opened by both the header button and the grid tile.

<sub>M1 tile + header button [wf-import-actions]</sub>

### Q63 · important

**Does the onboarding takeover ("Let's get your menu in.") trigger only at zero menus ANYWHERE — when every menu is merely put away, the page stays a shelf (grid with just the Add tile, Not-in-use strip below)?**

Manual Put away creates a state that is neither the populated shelf nor "no menus yet"; the takeover would strand put-away menus behind onboarding.

*Recommended:* Yes — takeover only at zero menus total.

*Answer:* Accepted recommended (2026-08-07): onboarding takeover only at zero menus total; put-away menus keep the normal shelf.

<sub>M1 Hi-Fi v2 second frame; build-decisions 16 [m1-hifi, wf-import-actions]</sub>

### Q64 · important

**Not-in-use strip: build v2 as drawn — always expanded, no count line, no caret, chips wrapping onto more rows — with the collapse ▾ and count arriving only with the ≥7-menu at-scale shelf (X2 draws it there)?**

v1 and the X2 scale sheet have a count and collapse; v2 dropped both. Reconciling: quiet strip at small venues, collapsible at scale.

*Recommended:* Yes — v2 verbatim at small scale; X2's collapsible variant at the scale cutover.

*Answer:* Accepted recommended (2026-08-07): always-open strip at small scale (v2 verbatim); collapsible with count past ~6 menus (X2 variant).

<sub>M1 Hi-Fi v1 vs v2; X2 strip [m1-hifi, wf-import-actions, wf-scale]</sub>

### Q65 · important

**Not-in-use chip date: last-on-a-screen date when one exists, else the day it was put away (bare-date format as v2 draws); clicking a chip opens the menu in the builder like any card?**

v1 labelled it "last on a screen" but a never-published menu can now be put away and has no such date.

*Recommended:* Yes to both.

*Answer:* Accepted recommended (2026-08-07): chip date = last-on-a-screen when it exists, else put-away date; chips open the builder like cards.

<sub>M1 Hi-Fi v1/v2 chips; build-decisions 16 [m1-hifi, readme-handoff, record-consistency]</sub>

### Q66 · important

**Do Not-in-use chips get the same ⋯ menu as shelf cards (inapplicable items absent), so Duplicate / Go back to… are reachable for put-away menus?**

The annotation recommends duplicating old menus, but chips have no drawn control to do it.

*Recommended:* Yes — same menu, inapplicable rows absent.

*Answer:* Owner (2026-08-07): chips just open the menu — no ⍏ on Not-in-use chips; all actions happen from inside (or once back on the shelf).

<sub>Menus.dc.html M1 strip annotation [wf-import-actions]</sub>

### Q67 · important

**When "Go back to…" is used on a menu that already has queued changes, does the restore REPLACE the existing draft — with a one-line warning in the picker ("This replaces your 3 unpublished changes") — rather than stacking on top?**

One queue per menu means the restore draft and pending edits can't coherently coexist; "go back to how this looked" should mean exactly that.

*Recommended:* Replace, with the named warning before it commits.

*Answer:* Accepted recommended (2026-08-07): a restore replaces the existing draft, with the named warning ("This replaces your N unpublished changes") before committing.

<sub>decisions.md 2/11; Go-back footer [decisions-doc, wf-import-actions]</sub>

### Q68 · important

**"Take it off" commits immediately on confirm (like 86), rather than queueing as a draft change?**

The dialog reads as an immediate act and shows the consequence; queueing it would make the dialog a lie. It is itself the deliberate act decision 1 requires.

*Recommended:* Immediate.

*Answer:* Owner (2026-08-07), overrides recommendation with a cleaner rule: 86 is temporary and instant; take-off is PERMANENT and therefore queues as a draft change shipped by Publish. Dialog copy shifts to future tense; take-off appears as one line in the Review sheet.

<sub>Menus.dc.html take-off dialog; decisions.md 1 [wf-import-actions]</sub>

### Q69 · important

**After take-off with a screen offline, do the card and headline name the window honestly — e.g. "not on a screen · Lobby is still showing it until it reconnects"?**

Until Lobby reconnects it physically shows a menu that is officially off; no drawn form covers the state.

*Recommended:* Yes — amber dot with that phrase; headline names Lobby as the exception.

*Answer:* Accepted recommended (2026-08-07): the off-but-offline window is named honestly — amber dot, "not on a screen · Lobby is still showing it until it reconnects"; headline names the exception.

<sub>take-off dialog offline line; decisions.md 5 [wf-import-actions]</sub>

### Q70 · important

**Cut the history rows' "preview" affordance this build — every row shows "Go back to this" on hover, and the restore already lands as an inspectable draft?**

One drawn row has a button, another a bare grey "preview"; a real read-only version preview is render-engine work in slice 2.

*Recommended:* Yes — cut it; version preview can come later without rework.

*Answer:* Accepted recommended (2026-08-07): preview affordance cut; restore-as-draft is the inspection path; version preview can come later.

<sub>Menus.dc.html Go-back list [wf-import-actions]</sub>

### Q71 · important

**When "Go back to…" reaches the bottom of what the plan retains, does the list just end — no "older changes aren't kept" note, and no dimmed unusable rows (the drawn 50%-opacity row doesn't ship)?**

Retention is a limit on a capability you have — between decision 5 (name limits) and decision 4 (plan boundaries invisible); a row you can see but not use is the ghost decision 4 bans.

*Recommended:* Yes — it just ends; entries beyond retention don't appear.

*Answer:* Accepted recommended (2026-08-07): the history list just ends at retention; no notes, no dimmed rows.

<sub>decisions.md 4/5/8; Go-back dimmed row [decisions-doc, wf-import-actions]</sub>

### Q72 · important

**For a menu on zero screens, is "Take off the screens" simply absent from its ⋯ menu (the status line already says "Not on a screen"), with the same absent-when-inapplicable rule for every ⋯ item?**

Decision 5's named states are for things blocking you, not actions with no object.

*Recommended:* Yes — hidden, not greyed.

*Answer:* Accepted recommended (2026-08-07): inapplicable ⍏ actions are absent, never greyed — the rule for every ⍏ item.

<sub>decisions.md 5/10; M1b ⋯ menu [decisions-doc, wf-import-actions]</sub>

### Q73 · minor

**"Check the screens" is plain navigation to the existing Screens area — label constant regardless of state, still shown at zero screens (the headline, not the button, carries the state)?**

The button is drawn with no destination; Screens is explicitly not redesigned.

*Recommended:* Yes.

*Answer:* Owner (2026-08-07, after challenge): CUT the "Check the screens" button — the headline carries the state, the rail carries the door. May return with the future Screens redesign.

<sub>M1 Hi-Fi v2 header; navigation.mjs #/screens [m1-hifi, wf-import-actions, code-backoffice]</sub>

### Q74 · minor

**Confirm the shelf interaction defaults: the whole card cell is the door except ⋯ and the amber bar (own targets); hover lift + pointer; token focus rings with card, ⋯ and Review tabbable; chips and rail items likewise; the V logo tile clicks to Home — exact styling reviewed in the slice-2 impeccable pass?**

No hover, focus or keyboard state is drawn anywhere on M1; implementers would otherwise invent each one.

*Recommended:* Accept — standard conventions applied consistently.

*Answer:* Accepted recommended (2026-08-07): standard hover/focus/keyboard conventions applied consistently; whole card is the door except ⍏ and the amber bar; styling reviewed at the slice-2 design pass.

<sub>README "the board is the door"; M1 Hi-Fi v2 [m1-hifi, wf-import-actions]</sub>

### Q75 · minor

**Is it acceptable that the card shows no extra "updating…" state — the three drawn status variants only, with online-but-behind moments staying in the amber wording and per-screen delivery detail living in the builder's chips?**

Between Publish and every screen applying it there's a brief unnamed window.

*Recommended:* Yes — three variants as specified.

*Answer:* Accepted recommended (2026-08-07): three card status variants only; the applying window lives in the builder chips.

<sub>README component sheet status variants [m1-hifi]</sub>

### Q76 · minor

**Is refresh-on-window-focus plus a roughly once-a-minute background refetch honest enough for M1's live claims this build (no push)?**

A stale page could state "Lobby is offline" falsely minutes later; real-time push isn't needed until the player slice proves delivery.

*Recommended:* Yes — focus + interval refetch.

*Answer:* Accepted recommended (2026-08-07): focus + ~1-minute interval refresh; no push this build.

<sub>M1 headline live facts; decisions.md 12 [m1-hifi]</sub>

### Q77 · minor

**M1 loading/failure: existing skeleton pattern inside the new shell while loading; on a screen-status failure, degrade to a neutral headline ("Your menus") with cards still shown?**

Neither state is drawn; the codebase has LoadingSkeleton.tsx and error-card patterns.

*Recommended:* Yes — degrade the headline, never block the shelf.

*Answer:* Accepted recommended (2026-08-07): skeletons while loading; on status failure the headline degrades to neutral and cards still render.

<sub>LoadingSkeleton.tsx [m1-hifi]</sub>

### Q78 · minor

**When nothing is put away, the Not-in-use strip doesn't render at all; and chip thumbs are just theme-background swatches (no live board render at 34×21)?**

A labelled empty strip and unreadable micro-renders both cost more than they give.

*Recommended:* Yes to both.

*Answer:* Accepted recommended (2026-08-07): strip absent when empty; chip thumbs are theme swatches, not micro renders.

<sub>M1 Hi-Fi v2 strip + chip swatches [m1-hifi]</sub>

### Q79 · minor

**Confirm there is no way to delete a menu this build — "Put away" is the terminal state?**

Menus accumulate forever; destroying attributable history deserves its own designed moment later.

*Recommended:* Yes — no delete.

*Answer:* Owner (2026-08-07), overrides recommendation: ADD DELETE this build. Spec confirmed 2026-08-07: "Delete forever" in the ⍏ only for menus on zero screens; hard confirmation naming the destroyed menu and history; shared library items survive.

<sub>M1b ⋯ menu (six items, no delete) [readme-handoff]</sub>

### Q80 · minor

**With zero screens paired, is Publish still allowed (it just versions the menu), with the readiness line "Nothing is showing this menu yet — it goes live when you put it on a screen"?**

A venue can hold a draft with no screens; the sentence-that-scales is never drawn for none.

*Recommended:* Yes.

*Answer:* Owner (2026-08-07), overrides recommendation: publish is BLOCKED with zero screens paired — a real named state per decision 5 (e.g. "Pair a screen to publish"), not a silent absence. First Publish (Q1) always has at least one screen to pick.

<sub>Menus.dc.html one-screen publish bar [wf-import-actions]</sub>

## Getting a menu in (M1a: paste, confirm, start-blank, looks)

### Q81 · BLOCKING

**Confirm the paste grammar: trailing number(s) = price ($ and decimals optional); "14 / 54" = one item with two sizes (drawn confirm question); "name — text price" splits on one line; an unpriced non-caps line under an item is its description; anything unparseable becomes a confirm question — a line is NEVER silently dropped?**

Only the caps-line-becomes-section rule is stated; real pastes contain all of these.

*Recommended:* Yes to all five — "never silently drop a line" is the invariant.

*Answer:* Accepted recommended (2026-08-07): all five paste-grammar rules confirmed; a pasted line is never silently dropped.

<sub>Menus.dc.html paste sample + note [wf-import-actions]</sub>

### Q82 · BLOCKING

**For paste (no photo to crop), does the evidence slot beside each confirm question hold the original pasted line(s) verbatim in monospace — defining the contract that every route supplies an evidence fragment, image or text?**

The crop-beside-question pattern is the confirm step's signature and the shared shell is built against paste this build; the drawn examples are all photo-born.

*Recommended:* Yes — same slot, medium changes. Question shapes for paste: unreadable price, two-price lines, and the grouped near-miss check; cleanly parsed rows never shown (decision 18).

*Answer:* Accepted recommended (2026-08-07): paste evidence = original line(s) verbatim in monospace; every route supplies an evidence fragment (image or text).

<sub>Menus.dc.html confirm cards; decisions.md 18 [wf-import-actions, record-consistency]</sub>

### Q83 · BLOCKING

**Skipped confirm questions: what does the canvas flag look like, does a guessed value render on the board, and can a flagged menu publish?**

"They're flagged on the canvas until you fix them" is never drawn, and the Burrata card ships a machine-guessed price.

*Recommended:* Amber dot + "we guessed this" note on the item and in its inspector; the guess renders normally (better than a hole); Publish allowed with one quiet publish-bar line ("2 items still flagged").

*Answer:* Owner (2026-08-07): flagged/guessed items are resolved **at import** — "when items get imported they are flagged after the fact and should never reach this point." A machine guess must never render on a live board. Stronger than the recommendation (which allowed publishing with a rendered guess). FLAG — reconciliation needed with the drawn "Skip these for now" path (README M1a): skipping must force resolution before publish or the skip path goes; design pass required before slice 6 builds the confirm step.

<sub>Menus.dc.html confirm footer [wf-import-actions]</sub>

### Q84 · BLOCKING

**Where does re-import into an EXISTING menu begin, given the ⋯ menu is fixed at six items with no import row?**

Decision 32's replace-by-import is in scope for slice 6 but no drawn surface starts it; the only import entry creates a new menu.

*Recommended:* A destination line on the route surface itself — "This will be a new menu · or replace one you have ▾" — keeping one import flow and the six-item ⋯ menu.

*Answer:* Accepted recommended (2026-08-07): destination line on the import route surface itself — "This will be a new menu · or replace one you have ▾" — one import flow, six-item ⋯ menu unchanged.

<sub>decisions.md 32; build-decisions 16; slice-plan slice 6 [wf-import-actions]</sub>

### Q85 · BLOCKING

**Start-blank (named everywhere, drawn nowhere): does it create a "New menu" draft, show Pick-a-look, and land in the builder with one empty section (e.g. "Menu", renameable inline) and the add-item row focused — the shelf card existing at once as "Never published · not on a screen"?**

Zero drawn frames for a route that ships this build; zero-section boards need never exist.

*Recommended:* Yes — exactly that, Coastal default theme.

*Answer:* Accepted recommended (2026-08-07): full flow — New menu draft, Pick-a-look, builder with one empty renameable section and the add-item row focused; shelf card exists at once as "Never published · not on a screen"; Coastal default theme.

<sub>Menus.dc.html "or start from a blank board"; build-decisions 5 [wf-import-actions]</sub>

### Q86 · BLOCKING

**Which board looks ship, under what names? The bundle names four ("Classic dark", "Paper, two-up", "With photos", "Coastal") while the slice plan ships two.**

The Pick-a-look step and the slice-2 render engine both need the exact list; "With photos" implies photo support the plan never mentions, and the picker must show only looks that exist (decision-4 style).

*Recommended:* Two this build — Coastal (the light paper look in the hi-fis) and Classic dark; "With photos" arrives with photo support.

*Answer:* DEFERRED (2026-08-07). Provisional per recommendation: Coastal + Classic dark this build; "With photos" arrives with photo support. Flagged in the slice-2 acceptance workbook.

<sub>Menus.dc.html "Pick a look" vs slice-plan slice 2 vs M2 inspector [wf-import-actions, record-consistency]</sub>

### Q87 · important

**Does the confirm step ALWAYS appear as the one consistent ending — with zero questions when the parse is clean ("We read all 6 items.", the Name field, "Done — open in the builder") — rather than clean imports skipping straight to the shelf, and may we settle the headline variants for partial cases ("We read 45 items. Just a name check.")?**

Decision 18 (confirm only what we were unsure of) and decision 30 (all routes converge on the confirm step) pull opposite ways at zero questions; the step also hosts the menu-name field, which needs a home either way.

*Recommended:* Always appears, zero-question form when clean; variants drafted for the workbook.

<sub>decisions.md 18/30; confirm headline [decisions-doc, wf-import-actions]</sub>

### Q88 · important

**Where does a pasted (or blank) menu's name come from — an editable Name field on the confirm step, pre-filled from the paste's first line when it reads like a title, otherwise "New menu", editable later inline in the builder breadcrumb?**

Nothing in the paste flow collects a name, but the shelf card, breadcrumb and history need one immediately; caps lines are sections by rule, so never guess a title from one.

*Recommended:* Yes — editable Name on confirm, best-guess pre-fill, never a blank name on the shelf.

<sub>Menus.dc.html confirm sub-line; README M1a [decisions-doc, wf-import-actions]</sub>

### Q89 · important

**Pick-a-look sequencing: shown once right after the import confirm (or immediately on start-blank), defaulting to Coastal — with a zero-item board previewing a few sample lines — and slice-wise: engine themes in slice 2, builder entry in slice 3, creation-time picker in slice 6?**

The panel is drawn but never sequenced and assigned to no slice; "previews use your items" fails for blank boards.

*Recommended:* Yes to all.

<sub>Menus.dc.html "Pick a look — at creation, and after"; slice-plan [wf-import-actions, record-consistency]</sub>

### Q90 · important

**Is near-miss matching library-only this build — a venue's first-ever import shows no name check, and in-file near-duplicates land as typed for the builder to fix?**

On first import the library is empty; in-file dedupe is rare in real pastes and would complicate slice 6.

*Recommended:* Yes — library-only.

<sub>decisions.md 33; near-miss card [wf-import-actions]</sub>

### Q91 · important

**"See all 45": a read-only list of everything parsed, grouped by section, unsure rows highlighted amber, no inline editing (the builder stays the one place to edit)?**

The button exists but its surface is never drawn.

*Recommended:* Yes — exactly that.

<sub>Menus.dc.html confirm header [wf-import-actions]</sub>

### Q92 · important

**Confirm the import lifecycle: the draft is created the moment the route commits ("Use this"); abandoning the confirm behaves identically to "Skip these for now"; the confirm is one-shot (afterwards flags live only on the canvas)?**

Otherwise a half-imported state exists to reason about.

*Recommended:* Yes to all three.

<sub>Menus.dc.html M1a intro [wf-import-actions]</sub>

### Q93 · minor

**Is the near-miss band tidying-level only — case, punctuation, spacing, an obvious one-letter typo — with anything more distant treated as a new item?**

A duplicate is visible and fixable; a wrong merge silently rewrites another menu.

*Recommended:* Yes — when in doubt, new item.

*Answer:* Accepted recommended (2026-08-07): tidying-level only — case, punctuation, spacing, an obvious one-letter typo; anything more distant is a new item. When in doubt, new item.

<sub>decisions.md 33 [decisions-doc]</sub>

### Q94 · minor

**Is the near-miss row's "Different" link just a labelled way to untick the checkbox (one boolean, two affordances, flipping to "Same" when unticked)?**

Two affordances per row with no stated relationship.

*Recommended:* Yes — a readable synonym, not a third state.

*Answer:* Owner (2026-08-07): go beyond the binary — a near-miss row can open a small picker of other close library matches, in case the machine matched the wrong existing item. Same/Different remains the fast path; the picker is the correction path. Needs a small design spot in the confirm step (slice 6).

<sub>Menus.dc.html near-miss rows [wf-import-actions]</sub>

## The builder (M2 and the add-items flow M2a)

### Q95 · BLOCKING

**The '+' beside the Sections heading: does it add a new section (inline row at the rail's foot, already in typing mode, landing last on the board as a draft change) — with the bulk item drawer opening instead from an "Add many at once" link on the add-item row?**

The hi-fi's plus reads as add-a-section, the wireframe reassigns it to the bulk drawer, and no other add-a-section control exists anywhere in the bundle. Both cannot be true.

*Recommended:* Split them as described — '+' adds a section; the drawer moves to the add-item row.

*Answer:* Accepted recommended (2026-08-07): split them — '+' adds a section (inline row at the rail's foot, typing mode, lands last on the board as a draft change); the bulk drawer opens from an "Add many at once" link on the add-item row.

<sub>M2 hi-fi rail vs Menus.dc.html M2a annotation [m2-hifi, wf-additems]</sub>

### Q96 · BLOCKING

**How are sections renamed and deleted? Proposed: rename by clicking the canvas heading and typing over it; a quiet delete control with the heading; deleted sections release their items back to the library (nothing lost), all queued as draft changes; an empty board shows just the add affordance.**

No rename or delete control is drawn anywhere, and the rail is explicitly "a navigator, not a second editor".

*Recommended:* Accept the proposal.

*Answer:* Accepted recommended (2026-08-07): rename by clicking the canvas heading and typing over it; quiet delete control with the heading; deleted sections release their items back to the library; all queued as draft changes; an empty board shows just the add affordance.

<sub>M2 hi-fi rail + canvas; Menus.dc.html [m2-hifi]</sub>

### Q97 · BLOCKING

**How does someone remove an item from the board entirely (not 86)? Proposed: Delete/Backspace with the item selected plus a quiet "Remove from this board" link at the inspector's foot — queued as a draft change, item stays in the library.**

Nothing anywhere removes an item; 86 hides but keeps it, and the six-control inspector has no remove.

*Recommended:* Accept.

*Answer:* Accepted recommended (2026-08-07): Delete/Backspace with the item selected plus a quiet "Remove from this board" link at the inspector's foot — queued as a draft change, item stays in the library.

<sub>M2 hi-fi inspector; README "six controls" [m2-hifi]</sub>

### Q98 · BLOCKING

**What is "Welcome Panel" — a special built-in panel, or just an example section name? Proposed: no special panel type this build; it's an ordinary (empty) section, and a section with zero items doesn't render on the TV — the theme-generated venue-name header strip (M2b) is the branded title, always present, not editable, drawn without its misleading drag handle.**

It appears in the rail with no item count and as page 1 in Play, undefined everywhere; one reviewer proposed a built-in title panel, but the header strip already covers branding without inventing an unauthorable object.

*Recommended:* Accept — ordinary sections only; empty sections don't render; header strip is the theme-generated title.

*Answer:* OUT OF SCOPE → backlog (2026-08-07): no special panel type this build. Provisional default where the render engine touches it: ordinary sections only, a zero-item section doesn't render on the TV, and the theme-generated venue-name header strip is the branded title (always present, not editable). The built-in title-panel idea goes to the backlog.

<sub>M2 hi-fi rail; M2c "1 · Welcome"; Menus.dc.html M2b header strip [m2-hifi, code-display, wf-additems]</sub>

### Q99 · important

**Is every single edit saved into the shared draft the moment it's made — so leaving via the breadcrumb never loses anything and never prompts, and "Draft saved 10:42am" is simply the last edit's time?**

Nothing draws a leave-confirmation, and a lost just-typed edit would be worse.

*Recommended:* Yes — save-per-edit, no prompts.

<sub>M2 publish bar; README breadcrumbs [m2-hifi]</sub>

### Q100 · important

**Interim wiring while later slices cook: the Quick update | Build control absent (not greyed) until slice 6; slice 2's card-click temporarily opens the existing editor and Add-a-menu uses the existing create flow until slices 3/6 land; anything with no target is absent, never disabled?**

Slice 2 ships shelf affordances whose destinations arrive one to four slices later; the plan is silent on the gap and the slice-2 workbook will hit it.

*Recommended:* Yes — temporary wiring to existing surfaces, absence for the rest, each replaced as its slice lands.

<sub>slice-plan slices 2/3/6; M2 segmented control [m2-hifi, record-consistency]</sub>

### Q101 · important

**Viewing-as dropdown contents: the menu's target screens including offline ones (keeping last-reported shape); with none paired it reads "No screens yet" over the default canvas; until slice 4's geometry, screen name without resolution?**

The opened dropdown is never drawn and slice 3 predates real geometry.

*Recommended:* Yes to all.

<sub>M2 top bar "Viewing as Bar · 1920×1080 ▾" [m2-hifi]</sub>

### Q102 · important

**Play button states before/without screens: Play stays visible; with zero previewable screens it opens with a plain sentence ("Nothing to play against yet — pair a screen…" linking to Screens); a paired screen that hasn't reported geometry is LISTED but unselectable with "hasn't reported yet", becoming selectable when resolution arrives?**

Decision 5 says real states say what they are; decision 13 bans representative sizes; freshly-paired screens shouldn't look broken.

*Recommended:* Yes to all — blocked-with-reason, never vanished.

<sub>decisions.md 5/13; criterion 9; slice-plan slice 4 [decisions-doc, m2-hifi, m2c-hifi, readme-handoff, code-display]</sub>

### Q103 · important

**Drag pill appears on hover as well as selection (drag without click-first); cross-section moves wait for Board view in slice 5 — until then remove-and-re-add (two draft changes) is the path?**

As drawn only the selected item has a handle, and One-section view makes cross-section drags impossible anyway.

*Recommended:* Yes to both.

<sub>M2 hi-fi canvas pill; wireframe annotation [m2-hifi]</sub>

### Q104 · important

**An 86'd item is selectable and fully editable, with the availability panel flipping to a red-tinted "Off right now — 86'd 6:40pm" (switch off, body copy stating turning it on shows it immediately)?**

The inspector is only ever drawn in its green on-state.

*Recommended:* Yes.

<sub>M2 hi-fi Berry Fizz row + inspector [m2-hifi]</sub>

### Q105 · important

**When a section holds more items than fit the card, One-section view just grows and scrolls for editing — real pagination shown only in Whole board, Play and on the TV?**

Wine's 12 items would clip in the fixed-height card as drawn.

*Recommended:* Yes.

<sub>M2 hi-fi overflow:hidden card; M2c split card [m2-hifi]</sub>

### Q106 · important

**With nothing selected, the inspector keeps its place with a quiet "Select an item on the board to edit it" placeholder plus the theme footer — the canvas never resizing with selection?**

The inspector is only drawn with an item selected; ✕, empty-canvas clicks and fresh opens all deselect.

*Recommended:* Yes.

<sub>M2 hi-fi inspector ✕ [m2-hifi]</sub>

### Q107 · important

**Leave the "Feature on the board" checkbox out of slice 3 (flag stays in the schema) until you or the designer specify what a featured item looks like on the board?**

No board render anywhere shows a featured item; shipping the control would make an implementer invent board design.

*Recommended:* Yes — absent, not dead; added back the moment the treatment is specified.

<sub>M2 inspector checkbox; no featured render in bundle [m2-hifi]</sub>

### Q108 · important

**Defer item photos entirely: omit the "Add a photo" checkbox AND the 54×54 dashed canvas squares this build (schema keeps the photo field), photos arriving with the "With photos" theme work and a real upload flow?**

No upload endpoint exists, no flow is drawn, and the TV board spec never mentions photo placeholders on "the real render".

*Recommended:* Yes — omit both, keep the field.

<sub>M2 inspector + dashed squares; MenuItem.ImageUrl; no upload in Vennu.Api [m2-hifi, readme-handoff]</sub>

### Q109 · important

**The inspector's theme footer link opens a small picker over the canvas offering only the shipped looks, and changing theme queues as a draft change — never navigating to the undesigned Themes area?**

The destination is never drawn and the wireframe promises "changeable from the builder — never a theme editor buried in settings".

*Recommended:* Yes.

<sub>M2 inspector footer; Menus.dc.html themes note [m2-hifi]</sub>

### Q110 · important

**"discard draft" asks for one confirmation naming the stakes — count and authors ("Discard 3 changes? 2 of them are Alex's. This can't be undone.") — then clears the menu's whole queue with no undo?**

An inline link that can destroy several people's work with one click, no dialog drawn; the one irreversible act in the draft model.

*Recommended:* Yes.

<sub>M2/X1 publish bar "discard draft" [readme-handoff, m2-hifi, wf-scale]</sub>

### Q111 · important

**Publish bar clean state: the bar stays as the home of screen status — "Everything is on your screens · published Tue 4:12pm by Dana · go back to…", Publish and Review first absent, chips remaining; during publish the button shows a brief busy state, then chips flip to arrived/offline sentences?**

The bar is only ever drawn dirty; the after-publish chip sentences are specified but not the frame around them.

*Recommended:* Yes.

<sub>M2 publish bar; Menus.dc.html after-publish slots [m2-hifi]</sub>

### Q112 · important

**M2a add-row search covers the whole venue library (all items, 86'd included); an item already on this board still appears, labelled "already on this board · Small Plates", and picking it JUMPS to it instead of placing a second copy?**

Scope is unstated and stopping duplicates is the design's stated goal.

*Recommended:* Yes.

<sub>Menus.dc.html M2a dropdown [wf-additems]</sub>

### Q113 · important

**"Create as a new item": created with exactly the typed text as name, empty price/description, placed in the section, inspector opening with the name field focused — missing price allowed (quiet canvas flag, publish not blocked)?**

The design never shows what the new item is born with; the import flow already permits price-less items.

*Recommended:* Yes.

<sub>Menus.dc.html M2a create row [wf-additems]</sub>

### Q114 · important

**Omit the bulk drawer's "From POS" filter chip entirely this build (the item source field stays in the schema so it lights up when the POS route lands)?**

The POS import route is deferred and decision 17 shows POS surfaces only when the add-on is attached; nothing should hint at an undelivered capability.

*Recommended:* Yes — omit.

<sub>M2a chips; decisions.md 17; build-decisions known gaps [wf-additems]</sub>

### Q115 · important

**Board price format: trim trailing ".00" everywhere the shared engine renders (cards, canvas, Play, TV), keeping the em dash for a missing size price?**

The builder hi-fi renders "5.00", Play renders the same item as "5" — one engine needs one rule; Play is the later drawing and reads like a printed menu.

*Recommended:* Trim.

<sub>M2 hi-fi vs M2c hi-fi prices [m2c-hifi]</sub>

### Q116 · minor

**On a menu's very first open, the builder shows One-section view, topmost section selected, nothing selected in the inspector?**

Return visits restore "where you left off"; first opens have no left-off.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): first open shows One-section view, topmost section selected, nothing selected in the inspector; return visits restore where you left off.

<sub>M2 hi-fi; README card-click [m2-hifi]</sub>

### Q117 · minor

**Single item selection only this build — no shift-click or multi-select on the canvas (bulk operations live in the bulk-place drawer)?**

No multi-select is drawn and the inspector is single-item.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): single item selection only this build; bulk operations live in the bulk-place drawer.

<sub>M2 hi-fi selection ring [m2-hifi]</sub>

### Q118 · minor

**In-place editing is the price only — clicking a name or description selects the item and focuses the matching inspector field?**

The wireframe grants in-place editing to the price alone; unstated, everything might become canvas-editable.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): in-place editing is the price only; clicking a name or description selects the item and focuses the matching inspector field.

<sub>Menus.dc.html M2 annotation [m2-hifi]</sub>

### Q119 · minor

**Carry the current limits into the new Item table — name ≤200 and never blank (an emptied name reverts on blur), description ≤1000?**

The design specifies no limits; the DB already enforces these.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): carry the current limits — name ≤200 and never blank (an emptied name reverts on blur), description ≤1000.

<sub>012_create_menu_domain.sql [m2-hifi]</sub>

### Q120 · minor

**Add Alt+Up/Down keyboard reordering for the selected row (same draft change as a drag), since replacing every ↑/↓ button pair with drag handles otherwise removes the keyboard path entirely?**

A keyboard-only user could no longer reorder anything.

*Recommended:* Yes.

*Answer:* OUT OF SCOPE → backlog (2026-08-07): Alt+Up/Down keyboard reordering waits. Noted trade-off: with every ↑/↓ button pair replaced by drag handles, keyboard-only users cannot reorder until this lands — tracked as a backlog accessibility item.

<sub>README "Drag to reorder. Everywhere." [m2-hifi]</sub>

### Q121 · minor

**Does ⌘K find-an-item-on-this-board ship with the builder in slice 3 (the wireframe specifies it; the slice plan omits it)?**

It's the only fast path to one item on a 41-item board.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): ⌘K find-an-item-on-this-board ships with the builder in slice 3 (slice plan omission corrected).

<sub>Menus.dc.html M2a ⌘K; slice-plan slice 3 [record-consistency]</sub>

### Q122 · minor

**M2a keyboard: arrows move the highlight, Enter places, Escape closes and empties the row; with no matches Enter creates the typed text — "create new" always the last highlightable option?**

Only the ↵ hint on the top result is drawn; this row is how six items get typed in a run.

*Recommended:* Yes to all four.

*Answer:* OUT OF SCOPE → backlog (2026-08-07): the full keyboard flow spec (arrows / Enter places / Escape clears / Enter-creates-when-no-match) is not locked this build; the add-item row ships with framework-default keyboard behavior and the full flow is a backlog item.

<sub>M2a dropdown ↵ hint [wf-additems]</sub>

### Q123 · minor

**"Where it lives" vocabulary in search results: list up to two board names, summarise beyond ("on 3 boards"), append "· 86'd right now" when off, ellipsize long names?**

Only the single-board case is drawn; implementers would invent one per surface.

*Recommended:* Yes — lock the vocabulary.

*Answer:* Accepted recommended (2026-08-07): vocabulary locked — up to two board names, "on 3 boards" beyond, "· 86'd right now" when off, long names ellipsized.

<sub>M2a result rows [wf-additems]</sub>

### Q124 · minor

**After "Place 2 in Non-Alcoholic", the bulk drawer stays open with selection cleared and a brief "2 placed" note, the button retargeting as you move sections; Escape/✕ closes?**

Filling a new board is the bulk path's whole point.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): after a bulk place the drawer stays open, selection cleared, brief "2 placed" note; the button retargets as sections change; Escape/✕ closes.

<sub>M2a drawer button [wf-additems]</sub>

## Board view (M2b)

### Q125 · important

**The page strip is navigation only — clicking any chip (dashed overflow page included) jumps the canvas there, no drag-onto-chip — with all rearranging done on the canvas, since pages are a consequence, not a setting?**

The strip is drawn as state with no interactions specified.

*Recommended:* Yes.

<sub>Menus.dc.html M2b page chips [wf-additems]</sub>

### Q126 · important

**"Override it for Patio" simply switches Board view to that screen's scope — layout changes there apply to Patio only, with a visible "Overridden for Patio" marker and one "use the automatic layout again" action — no separate override editor?**

Slice 5 includes per-screen overrides but the surface is drawn nowhere.

*Recommended:* Yes — anything richer waits for a real venue to need it.

<sub>M2b sidebar link; slice-plan slice 5 [wf-additems]</sub>

### Q127 · important

**The overflow fix "Split into Whites and Reds" is really a generic "Split into two sections" cutting at the overflow point with both halves renameable — the drawn names being illustrative?**

The system can't know a wine list divides into whites and reds; guessing category names wrong beats not guessing.

*Recommended:* Yes — generic split.

<sub>M2b overflow buttons [wf-additems]</sub>

### Q128 · important

**"Starts on page 3" is a read-only fact, not a dropdown — "Right column" stays the one placement control, page position deriving from order, column and overflow choices?**

The sidebar draws it as a control, but the same screen's principle card says nobody chooses pages; a forced start page creates gaps.

*Recommended:* Read-only.

<sub>M2b "Where it sits" vs principle card [wf-additems]</sub>

### Q129 · minor

**Replace the Patio chip's "relays to 3 pages" with plain words — "online — ready · 3 pages on this screen", shown whenever a screen's reflow yields a different page count?**

The bundle's only use of "relays"; its meaning is a guess.

*Recommended:* Yes — plain words over a coined verb.

*Answer:* Accepted recommended (2026-08-07): plain words — "online — ready · 3 pages on this screen", shown whenever a screen's reflow yields a different page count. "Relays" is retired.

<sub>M2b publish bar chip [wf-additems]</sub>

### Q130 · minor

**The "room for one more section" dashed slot is both a drop target for dragged sections AND clickable to create a new empty section in that spot?**

Also gives Board view a visible add-a-section affordance on the board itself.

*Recommended:* Yes — both.

*Answer:* Accepted recommended (2026-08-07): the dashed slot is both a drop target for dragged sections and clickable to create a new empty section in that spot.

<sub>M2b dashed box [wf-additems]</sub>

## Play (M2c)

### Q131 · BLOCKING

**Does Play's problem card detect page-splits only this build (one card per affected section, stacked; absent at zero problems — no green all-clear)?**

Only one problem type is ever drawn; the inspector's text-wrap warning is a separate M2 surface.

*Recommended:* Yes — splits only.

*Answer:* Accepted recommended (2026-08-07): splits only this build — one card per affected section, stacked; absent at zero problems, no green all-clear.

<sub>M2c problem card [m2c-hifi]</sub>

### Q132 · BLOCKING

**What rule does "Readable from 18 ft" encode — is cap-height-in-inches × 10 ft acceptable, headline driven by the smallest common text (item names), larger roles supplying the "holds" comparison, written into the slice-5 workbook?**

No formula is stated anywhere and the number must be reproducible for acceptance.

*Recommended:* Accept the ×10 signage rule as described.

*Answer:* Accepted recommended (2026-08-07): cap-height-in-inches × 10 ft, headline driven by the smallest common text (item names); larger roles supply the "holds" comparison; formula written into the slice-5 acceptance workbook.

<sub>M2c "Readable from 18 ft" + provenance [m2c-hifi]</sub>

### Q133 · BLOCKING

**Drop the room-comparison sentence ("Bar's far end is about 24 ft") this build — nothing in the platform knows room dimensions — showing only the distance, the holds/won't line, and the provenance line, with an optional per-screen distance field under Screens restoring it later?**

The sentence references data with no origin and no artifact adds a way to enter it.

*Recommended:* Drop it.

*Answer:* OUT OF SCOPE → backlog (2026-08-07): the room-comparison sentence is dropped this build — show only the distance, the holds/won't line, and the provenance line. Backlog: an optional per-screen viewing-distance field under Screens restores it later.

<sub>M2c sidebar [m2c-hifi, code-display, record-consistency]</sub>

### Q134 · important

**Play is a full-window takeover (nav rail and builder chrome hidden; ✕ returns to M2 exactly as left) — treating "takes over the canvas" as flow, not geometry?**

Both drawings fill the whole 1440px viewport; the prose says canvas-only. An implementer must pick.

*Recommended:* Full-window, as both drawings show.

<sub>M2c full-bleed frame vs Menus.dc.html prose [m2c-hifi]</sub>

### Q135 · important

**Annotations (page counter, "1 OF 2", the 86 note): the guest-facing TV renders NONE; Play, the M2 canvas and M2b show them; M1 cards show none — one engine flag for editing/preview surfaces?**

The README says they're not board content but never lists which surfaces render them; the shared engine needs the answer.

*Recommended:* Yes — TV completely clean.

<sub>M2c overlays; README annotations note [m2c-hifi, code-display]</sub>

### Q136 · important

**Play paginates from guest-visible content only — the 86 note rendered inside the space the missing item freed, never pushing content — so page breaks always match the TV?**

If the note took layout space, Play's pagination would differ from the TV's, defeating its promise.

*Recommended:* Yes.

<sub>M2c 86 note placement; "real page breaks" [m2c-hifi]</sub>

### Q137 · important

**On a continuation page, the section heading repeats for guests on every surface, with the "2 OF 2" counter appearing in back-office surfaces only?**

A guest sees page 4 alone for 8 seconds; page 4 is never drawn.

*Recommended:* Yes to both.

<sub>M2c "WINE 1 OF 2" [m2c-hifi]</sub>

### Q138 · important

**Offline and stale screens stay selectable in Play — a pure simulation against last-reported geometry, with the provenance line switching to "last seen 4:12pm — offline" — only never-reported screens excluded?**

The lo-fi says offline still renders from its last report; the hi-fi never shows it selected; criterion 9 bans only unpaired screens.

*Recommended:* Yes — selectable with the honest note.

<sub>M2c chips; Menus.dc.html M2c annotation; criterion 9 [m2c-hifi, wf-scale]</sub>

### Q139 · important

**When a screen reports resolution but not physical size, the Readable-from panel stays with a plain reason ("Bar doesn't report its panel size, so we can't estimate reading distance") — never guessing a size?**

Many platforms hide panel inches; the degraded state is never drawn; decision 5 says real states say what they are.

*Recommended:* Yes.

<sub>M2c provenance; slice-plan slice 4 degrade note [m2c-hifi, readme-handoff, code-display]</sub>

### Q140 · important

**On entry, Play auto-plays from page 1 on the screen currently selected in M2's "Viewing as" control?**

The hi-fi is a mid-cycle snapshot; the start state is unspecified.

*Recommended:* Yes — matches the play–spot–fix–play-again loop.

<sub>M2c snapshot; M2 top bar [m2c-hifi]</sub>

### Q141 · important

**The draft pill disappears entirely when the queue is empty (Play is then exactly what's live) and is purely informational, never clickable?**

Only the 3-changes state is drawn.

*Recommended:* Yes.

<sub>M2c pill [m2c-hifi]</sub>

### Q142 · important

**Changes landing while Play is open: an 86 applies live (taking effect at the next page turn, re-paginating); colleagues' draft edits appear only on next Play entry?**

86 is the only thing that must be instant everywhere; a stale board would falsify Play's claim.

*Recommended:* Yes.

<sub>M2c "read-only… including your unpublished draft" [m2c-hifi]</sub>

### Q143 · important

**When Play is pressed on a menu that isn't on any screen yet, the picker offers ALL paired screens with reported geometry — previewing must not require assigning first?**

The hi-fi only shows Play for a fully-assigned menu.

*Recommended:* Yes.

<sub>M2c picker; "Late Night not on a screen" [code-display]</sub>

### Q144 · minor

**A timeline page is drawn dashed only when it contains nothing but overflow continuation — solid the moment any other content shares it?**

Dashed means "this page disappears if you fix the overflow", only true of pure continuations.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): dashed only when the page contains nothing but overflow continuation; solid the moment any other content shares it.

<sub>M2c "4 · Wine 2" dashed [m2c-hifi]</sub>

### Q145 · minor

**Page labels are auto-derived — full section names joined with " + ", continuations as "Wine 2", long labels ellipsized — no manual naming or abbreviation feature ("Non-Alc" was designer shorthand)?**

Pages are a consequence of overflow, not a setting.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): labels auto-derived — section names joined with " + ", continuations as "Wine 2", long labels ellipsized; no manual naming.

<sub>M2c timeline labels [m2c-hifi]</sub>

### Q146 · minor

**Bind Space (pause/resume), ←/→ (prev/next), Esc (close); paused state is a plain ❚❚↔▶ swap; timeline blocks clickable to jump (restarting that page's dwell)?**

Watch-mode surface with no keyboard, paused state or click behavior drawn.

*Recommended:* Yes to all.

*Answer:* OUT OF SCOPE → backlog (2026-08-07): the Play keyboard/click spec (Space pause, ←/→ pages, Esc close, clickable timeline blocks) is not locked this build; Play ships with basic default interactions and the full spec is a backlog item.

<sub>M2c transport [m2c-hifi]</sub>

### Q147 · minor

**The pill counts ALL queued changes on the menu and keeps the verbatim word "your" regardless of author ("your" reads as the venue's)?**

The draft is multi-author, so "your 3 unpublished changes" can be literally wrong.

*Recommended:* Yes.

*Answer:* Owner (2026-08-07): remove "your" and any possessive phrasing — the pill reads "Draft — includes 3 unpublished changes" (count is still all queued changes on the menu, any author). Approved deviation from the README verbatim-copy list, which carried "your".

<sub>M2c pill; M2 byline [m2c-hifi]</sub>

### Q148 · minor

**Clicking another screen chip re-paginates and restarts at page 1 (preserving play/pause), portrait boards letterboxed at real proportions?**

Pagination differs per geometry; no portrait board is drawn anywhere.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): switching chips re-paginates and restarts at page 1, preserving play/pause; portrait boards letterboxed at real proportions.

<sub>M2c Patio chip 1080×1920 [m2c-hifi]</sub>

### Q149 · minor

**Timeline at scale: it ships at every scale (X3's text line is sketch shorthand); labels ellipsize first, then blocks go number-only with the active page fully labeled; past six pages a six-block window slides with the current page, "+N" counts flanking?**

A wine book can hit 10+ pages; no overflow state is drawn and the transport bar never scrolls.

*Recommended:* Yes to all.

*Answer:* Accepted recommended (2026-08-07): labels ellipsize first, then number-only blocks with the active page fully labeled; past six pages a six-block window slides with the current page, "+N" counts flanking.

<sub>M2c timeline; X3 transport; X4 page strip [m2c-hifi, wf-scale]</sub>

### Q150 · minor

**Confirm the transport is entirely simulated — pause/prev/next never touch any real screen?**

Wiring them to devices would violate criterion 4; worth one word before slice 5.

*Recommended:* Yes.

*Answer:* Accepted recommended (2026-08-07): the transport is entirely simulated — pause/prev/next never touch a real screen.

<sub>M2c transport; Menus.dc.html "a thing you watch" [m2c-hifi]</sub>

## Quick Update (M3)

### Q151 · important

**Does Quick Update list the menu AS PUBLISHED — what's actually on screens — leaving draft-only items out until their menu publishes?**

An 86 toggle on a draft-only item changes no screen, contradicting the toggle's own promise.

*Recommended:* Yes — published content only; drafts live in the builder.

<sub>decisions.md 3/15; verbatim toggle copy [decisions-doc]</sub>

### Q152 · important

**Should M3's search also match on-air items from the venue's OTHER menus — labelled ("on Late Night") and toggleable in place — while the list stays scoped to the open menu?**

The 11pm bartender doesn't know which menu holds the item, and availability is venue-wide anyway; finding nothing would be a real failure.

*Recommended:* Yes — adds findability, no new model.

<sub>Menus.dc.html M3; M1b ⋯ menu [wf-additems]</sub>

### Q153 · important

**When an item has been off more than a day: rows switch their "since" line to amber with a day count, and the footer names the oldest ("Bone Marrow has been off for 3 days") instead of the all-clear — no banners or emails?**

Only the happy footer is drawn; decision 14 exists exactly for this case.

*Recommended:* Yes — the list itself is the reminder.

<sub>M3 "Off right now" footer; build-decisions 14 [wf-additems]</sub>

### Q154 · important

**Can a bartender still 86 an item that isn't on any board — toggle present, sub-line explaining no screen will change?**

X4 draws the unplaced row without the one control the screen exists for; availability is a fact about the venue, not the board.

*Recommended:* Yes — toggle present.

<sub>X4 "Snapper Crudo · not on any board" [wf-scale]</sub>

### Q155 · important

**In browse, an item appears under every section it's placed in, with unplaced items collected in a final "Not on any board" group; expanding a badged section shows ALL its items with toggles, off items pinned first?**

Placement grouping and expand behavior are both undefined; the badge guides, it doesn't filter.

*Recommended:* Yes to both.

<sub>X4 browse; "only surface what is off" note [wf-scale]</sub>

### Q156 · important

**Do browse sections render collapsed once there are more than six sections (expanded at six or fewer) — the file's recurring boundary?**

M3 shows a flat list at 41 items; X4 collapses at 250.

*Recommended:* Yes.

<sub>X4 "7 sections, collapsed" vs M3 flat list [wf-scale]</sub>

### Q157 · important

**Is losing bulk 86 (today's select-25 / mark-selected) acceptable — M3 ships per-row toggles only, with a "turn off this whole section" affordance as a later addition if missed?**

End-of-night bulk operations become one tap per item.

*Recommended:* Yes — ship as designed, watch for the miss.

<sub>QuickUpdateMode.tsx:46; README M3 [code-backoffice]</sub>

### Q158 · important

**Accept that this build's 11pm bartender path is desktop/laptop only — M3 merely stacks below ~900px without calling it mobile, queued as the first designed-mobile candidate?**

The designer explicitly refused to guess a phone layout; slice 6's acceptance is this path.

*Recommended:* Yes — accept and flag.

<sub>README mobile note [wf-additems]</sub>

### Q159 · minor

**Undo toast: ~10 seconds with the live age counting up, a new toggle replacing the current toast, and "undo" after expiry simply being the toggle itself?**

The toast shows "4 seconds ago" implying it lingers; nothing is ever lost since the toggle is a full undo.

*Recommended:* Yes.

<sub>M3 toast [wf-additems]</sub>

## At scale (X1–X4)

### Q160 · BLOCKING

**Is "stale" officially a third screen state — kept as today's client-derived online-but-silent-5-minutes — shown as its own amber exception, excluded from the "ready" count, but still publishable-to so it catches up?**

The design distinguishes stale from offline; the server stores only Online/Offline/Archived and the back office derives stale at 5 minutes.

*Recommended:* Yes on all — keep the 5-minute rule.

*Answer:* Accepted recommended (2026-08-07): yes on all — stale stays a distinct client-derived amber state (Online but silent 5 minutes, from last-seen; nothing new stored server-side), shown as its own exception, excluded from the "ready" count, still publishable-to so it catches up. Owner probed collapsing stale into Offline; the distinction is kept deliberately so a quiet-but-showing screen does not read as down.

<sub>X1 "stale — no reply since 6:20pm"; ScreenManagement.tsx:253; ScreenManagementService.cs [wf-scale]</sub>

### Q161 · BLOCKING

**Publish bar cutover: chips-per-screen at six targets or fewer, count-plus-exception-cards above six — and the button says "Publish N changes" in BOTH forms (the green ready chip carries the screen count; X1's "Publish to 12 screens" treated as a slip)?**

Reviewers proposed 5 or 6; six is the file's own recurring boundary. Two label formulas exist for one button.

*Recommended:* Yes — collapse above 6; one label formula everywhere.

*Answer:* Accepted recommended (2026-08-07): chips per screen at six targets or fewer; count-plus-exception-cards above six; the button reads "Publish N changes" in both forms (X1's "Publish to 12 screens" treated as a slip).

<sub>M2 chips; X1 note + button; decisions.md scale rule [decisions-doc, readme-handoff, m2-hifi, wf-scale]</sub>

### Q162 · BLOCKING

**Of X4's three one-click fixes, only "Shorten the dwell" ships as a live action this build — the split-across-screens and move-sections lines left out until their flows are designed?**

The other two have no flow drawn anywhere and aren't in the slice plan; buttons must not open unfinished journeys.

*Recommended:* Yes — dwell fix only.

*Answer:* Accepted recommended (2026-08-07): "Shorten the dwell" ships as the only live fix action this build; split-across-screens and move-sections wait for designed flows — no buttons into unfinished journeys.

<sub>X4 fix buttons; slice-plan [wf-scale]</sub>

### Q163 · BLOCKING

**Shelf scale cutover: search, filter chips, the compact 6-across grid — and the strip's collapse — all appear together at ≥7 total menus (Not-in-use included, search also matching put-away menus tagged "not in use"); at ≤6 the shelf is exactly M1, growing rows and scrolling (no "N more" collapse at small scale)?**

Three artifacts hint at three behaviors; "past six" needs a hard boundary and a stated counting rule.

*Recommended:* Yes — one cutover at 7, everything included in the count.

*Answer:* Accepted recommended (2026-08-07): one cutover at ≥7 total menus (Not-in-use included in the count; search also matches put-away menus tagged "not in use"); at ≤6 the shelf is exactly M1, growing rows and scrolling, no collapse.

<sub>X2; Menus.dc.html search note; M1 Hi-Fi grid [m1-hifi, wf-import-actions, wf-scale]</sub>

### Q164 · important

**Filter chips are single-select click-to-filter, none active on load (the sketch's sky fill is styling, not a default), click again to clear?**

Sky fill means active elsewhere, but an active "On a screen · 3" filter contradicts the six cards drawn.

*Recommended:* Yes.

<sub>X2 chips vs grid [wf-scale]</sub>

### Q165 · important

**Compact-grid rule: every on-screen menu always visible, remaining first-row slots filled by most-recently-edited others, the rest behind an inline "N more ▾" that expands in place and stays open for the session?**

Which cards earn visibility and what ▾ does are unstated.

*Recommended:* Yes.

<sub>X2 six cards + "4 more" [wf-scale]</sub>

### Q166 · important

**Once the shelf compacts, "Add a menu" becomes a plain button beside the search field (the tile remaining at ≤6 menus)?**

X2's compact grid has no add affordance anywhere.

*Recommended:* Yes.

<sub>X2 grid vs M1 tile [wf-scale]</sub>

### Q167 · important

**When exceptions outnumber one row, the publish bar grows and wraps — every exception drawn, never summarized behind a count?**

The governing rule says list length tracks what is wrong.

*Recommended:* Yes — it grows.

<sub>X1 note "however many exceptions" [wf-scale]</sub>

### Q168 · important

**The grey footer strip ships with only the "See all 12 →" link (the prose was annotation), opening a read-only panel listing all target screens with their state?**

The strip mixes designer annotation with what reads as real UI.

*Recommended:* Yes.

<sub>X1 footer strip [wf-scale]</sub>

### Q169 · important

**When several menus hold unpublished changes, the headline names each ("Summer Menu is holding 3 changes, Patio Drinks 1") — exceptions get named — with screen-count phrases still capped at the top three menus?**

The chip says two holding menus; the drawn sentence names one.

*Recommended:* Yes — the drawn sentence missed the second menu.

<sub>X2 sub-line vs "Holding changes · 2" chip [wf-scale]</sub>

### Q170 · important

**"Fix these 2" takes the user to the existing Screens area filtered to those screens — Menus points at the problem, Screens owns fixing it?**

Fixing an offline TV is hardware work, not menu work.

*Recommended:* Yes.

<sub>X2 headline button [wf-scale]</sub>

### Q171 · important

**Play's picker switches from top-bar chips to the searchable sidebar at more than six target screens, the top-bar screen name becoming a plain label (no second dropdown)?**

"Tabs stop working at about six"; the caret's role at scale is unstated.

*Recommended:* Yes.

<sub>X3; README M2c chips [wf-scale]</sub>

### Q172 · important

**Play sidebar rows: one representative row per distinct rendering (shape × page count) plus a row for every problem screen, identical extras collapsed behind an expandable count — the drawn Dining-room row being illustrative?**

As drawn the rule contradicts itself (an identical screen gets its own row).

*Recommended:* Yes — expandable so any screen is still pickable by name.

<sub>X3 rows + "7 more" [wf-scale]</sub>

### Q173 · important

**With several differing screens, the "Only Patio differs" callout lists one line per differing screen with its reason, and is absent when none differ?**

The copy is written for exactly one.

*Recommended:* Yes.

<sub>X3 differs card [wf-scale]</sub>

### Q174 · important

**The Readable-from panel survives at scale — shown for the currently selected screen below the screens list (X3 omitted it for sketch space)?**

Slice 5 needs to know it doesn't disappear past six screens.

*Recommended:* Yes — it stays.

<sub>X3 sidebar vs README M2c [wf-scale]</sub>

### Q175 · important

**The board-too-long warning fires when the full loop exceeds 60 seconds (copy always computing real loop time; page count alone never triggers it)?**

The drawing fires at 120s but states no threshold.

*Recommended:* Yes — 60 seconds.

<sub>X4 warning [wf-scale]</sub>

### Q176 · important

**Each surface ships its at-scale behavior inside its own slice — shelf search slice 2, publish-bar collapse slice 3, Play sidebar slice 5, Quick-Update collapse slice 6 — with a 20-screen/13-menu seed in the Playwright specs?**

The scale sheet insists the behavior is part of each component from day one, but no slice or gate claims it; retrofitting is a redesign.

*Recommended:* Yes.

<sub>Menus at Scale strapline; slice-plan gates [record-consistency]</sub>

### Q177 · minor

**Accept the section rail and history list as plain unbounded scrolling lists with no extra scale treatment, revisited only if real data hurts?**

The designer explicitly left them undesigned at scale; history depth is bounded by tier retention anyway.

*Recommended:* Yes.

<sub>Menus at Scale footer [wf-scale]</sub>

## Copy, tokens and small details

### Q178 · BLOCKING

**May we add a second batch of tokens preserving the exact hi-fi values the approved additions don't cover (including a selection-blue token for the new #2a78d6), treating the board palette as board-theme data rather than UI tokens?**

Decision 8 says components consume variables, never raw values — but many component-sheet values (type sizes, borders, the board palette) have no token, forcing either a broken rule or a changed look.

*Recommended:* Yes — batch-2 under the same approval; board colors live in the theme definitions.

*Answer:* Accepted recommended (2026-08-07): batch-2 token additions under the same approval, preserving the exact hi-fi values (including a selection-blue token for #2a78d6); the board palette lives in board-theme definitions, not UI tokens.

<sub>proposed-token-additions.css; README component sheet; build-decisions 8 [readme-handoff]</sub>

### Q179 · important

**Does criterion 5's banned-word rule ("unpublish/supersede/restore/archive" nowhere in the UI) apply only to Menus and surfaces this build rewrites — legacy wording elsewhere logged as copy debt for each area's own redesign?**

Screens, Promotions and the legacy menu editors use those exact words today; an app-wide scrub is far larger than this build.

*Recommended:* Yes — enforce in Menus, log the rest.

<sub>README criterion 5; ScreenManagement.tsx; DateRangePromotionAdministration.tsx [decisions-doc]</sub>

### Q180 · important

**86 screen-count rule: the number in the warning and toast counts every screen currently showing the item through ANY menu; at one screen "hides it on your screen immediately"; at zero, "Berry Fizz is off — it isn't on a screen right now"; and the toast tells the honest per-screen story when a screen is offline ("off on Bar and Patio; Lobby will catch up when it reconnects")?**

Items are shared across boards and 86 is venue-wide; "all 3 screens" overstates when Lobby is offline and breaks at one or zero.

*Recommended:* Yes to all — summarize the normal, name the exception, on the surface where truth matters most mid-shift.

<sub>M2 inspector copy; M3 toast + note; decisions.md 3/12/25 [decisions-doc, readme-handoff, m2-hifi, wf-additems]</sub>

### Q181 · important

**May all verbatim strings use natural singular and zero forms — "1 change not published", "on your screen", hide "· 0" counts — keeping the approved shapes for 2+?**

With real data the counts hit 1 and 0; "1 changes" and "on all 1 screens" are broken in the most common small-venue case.

*Recommended:* Yes — standard variants.

<sub>README verbatim-copy rule [readme-handoff]</sub>

### Q182 · important

**What does "3 changes" count — each thing CURRENTLY different from the screens (editing the same price twice counts once), i.e. exactly what Publish ships and Review lists?**

The unit is undefined: edits-since-publish vs current diff give different numbers.

*Recommended:* Current diff — latest state per field/item.

<sub>README pending bar; DraftChange model [code-api-data]</sub>

### Q183 · important

**After-publish sentences: generic forms this build ("showing your latest changes" / "still showing the menu from Tuesday 4:12pm"), typed phrasing ("the new prices") only when the whole draft is one kind of change — shapes reviewed in the slice workbook?**

The drawn sentences classify the change type, implying generated summaries with no rule for mixed drafts.

*Recommended:* Yes.

<sub>Menus.dc.html one-screen publish bar [wf-import-actions]</sub>

### Q184 · important

**The venue-name eyebrow on Menus home uses #64748b (the light-surface muted token), not the #94a3b8 the layout spec names — which the same document calls a bug on light surfaces (2.56:1)?**

The README's own M1 spec makes the exact substitution its token section forbids.

*Recommended:* Yes — the accessibility rule outranks the layout spec.

<sub>README M1 vs README tokens [readme-handoff]</sub>

### Q185 · important

**Icon set: the codebase's SkyIcon has 8 icons and the hi-fis use placeholder Unicode glyphs — extend SkyIcon with matching in-house 24px stroke icons (you eyeball them at the slice-2 workbook), rather than adopting a third-party library like lucide-react?**

"Replace with the codebase's icon set" can't be followed as written; one reviewer proposed lucide, another in-house — in-house keeps zero dependencies and one style.

*Recommended:* Extend SkyIcon in-house.

<sub>SkyIcon.tsx; README assets [readme-handoff, code-backoffice]</sub>

### Q186 · important

**Is the venue name a plain static label everywhere in this build — no caret, never clickable (switching lives under the avatar) — treating the wireframes' "Harborview Lounge ▾" as stale, per decision 29's no-leak rule?**

Wireframes draw a dropdown caret; the hi-fi and README say not clickable; single-venue scope makes a switcher here meaningless.

*Recommended:* Yes — static, no caret.

<sub>Menus.dc.html/X2 carets vs README M1; decision 29 [wf-import-actions, wf-scale, m2-hifi]</sub>

### Q187 · minor

**Amend the tracked wording of criterion 4 to "No screen content changes without a deliberate act — a publish, an accept, an availability toggle, or a confirmed Take off the screens"?**

Read literally, criterion 4 fails whenever criterion 1 passes; the checklist is asserted by tests.

*Recommended:* Yes — record the exceptions now.

<sub>README criteria 1/4; decisions.md 3/36 [decisions-doc, readme-handoff]</sub>

### Q188 · minor

**Is "within 10 seconds on an online screen" the pass line for criterion 1's "within seconds", offline screens catching up on reconnect?**

The acceptance test needs a number; reviewers proposed 5 or 10 — 10 is comfortably "seconds" and won't flake on busy networks.

*Recommended:* Yes — 10 seconds online.

<sub>README criterion 1; slice-4 workbook [decisions-doc, readme-handoff]</sub>

### Q189 · minor

**Once an 86 is older than today, the string gains the day — "86'd yesterday 6:40pm", then "86'd Mon 6:40pm" — same sentence shape?**

With no auto-reset, "86'd 6:40pm" turns ambiguous the next morning.

*Recommended:* Yes.

<sub>README verbatim; build-decisions 14 [readme-handoff]</sub>

### Q190 · minor

**Boards render prices as bare numbers exactly as typed (12, 9.5, MP) with no currency symbol, deferring any currency setting to a later venue-level decision?**

Nothing defines a symbol or format and the codebase has no currency field; the hi-fi boards show none.

*Recommended:* Yes — bare numbers.

<sub>README board rendering; M2c prices [readme-handoff, m2-hifi]</sub>

### Q191 · minor

**In the shorter pending-changes card, the 16:9 board render scales to full width and crops at the bottom (top-aligned) — never squished or letterboxed?**

A 16:9 render in a 16/7.75 box must do one of three things; each looks different.

*Recommended:* Top-aligned crop.

<sub>README pending-changes bar [readme-handoff]</sub>

### Q192 · minor

**Two offline strings coexist deliberately — time-anchored ("offline since 4:12pm") on status surfaces, reassurance form ("offline — updates when it reconnects") on publish-chip surfaces — each shipping exactly what its wireframe draws?**

No single canonical offline string exists across the bundle.

*Recommended:* Yes.

<sub>X1 vs README state vocabulary [wf-scale]</sub>

## Process and acceptance confirmations

### Q193 · important

**Is the Downloads "Vennusign back-office design (1)/design_handoff_menus" folder the approved version, to be copied verbatim into docs/design/approved/menus/ (at-scale and compare sheets included) before slice 1 starts?**

The slice plan's authority pointer targets files not in the repo yet, and the "(1)" folder name hints at more than one downloaded copy.

*Recommended:* Yes — copy it in now so the authority pointer is real before code is written.

<sub>build-decisions authority line; docs/design/approved/menus/ contents [record-consistency]</sub>

### Q194 · important

**Acceptance checklist hygiene: criterion 18 (single-venue renders zero venue affordances) asserted by a named slice-2 spec and re-checked each UI slice; criteria 11 and 14–17 stamped "deferred to a later build" so the checklist can close clean?**

18 is precisely this build's no-leak guardrail and is testable now; 11/14–17 can never flip in this build.

*Recommended:* Yes to both.

<sub>README criteria; slice-plan acceptance lines [readme-handoff, record-consistency]</sub>

### Q195 · minor

**Confirm "Put away" sits directly after Duplicate in the ⋯ menu's middle group, with "Take off the screens" still alone below the last divider?**

Your decision 16 said "sixth item" without a position; the bundle's menu is verbatim copy, so the insertion point needs your word.

*Recommended:* Yes — as the slice plan has it; the destructive action stays isolated last.

<sub>build-decisions 16; slice-plan slice 2; M1b verbatim menu [record-consistency]</sub>

## Cross-cutting (completeness-critic pass)

### Q196 · important

**When the app shows a time like "Draft saved 10:42am" or "86'd 6:40pm", is that the viewer's own device clock (as the app works today), or the venue's local time?**

Every drawn timestamp ("Draft saved 10:42am", "86'd 6:40pm", "published Tue 4:12pm", "caught up at 10:51") names a clock but never whose. The venue has a stored Timezone field, yet the back office today renders all times in the viewer's browser via toLocaleString — an owner checking from home in another zone would see different times than staff on site, and "86'd 6:40pm" is a shift-time fact.

*Recommended:* Viewer's device clock this build — it matches the rest of the app and single-venue users are almost always on site; the venue Timezone field stays untouched for a later remote-management pass.

<sub>M2 Hi-Fi - Menu builder.dc.html publish bar "Draft saved 10:42am by Alex"; src/Vennu.Core.Models/Venue.cs "public string Timezone { get; set; } = \"UTC\""</sub>

### Q197 · important

**When a draft edit fails to save (bad network, server hiccup), what does the builder do — and can it ever claim "saved" for an edit that isn't?**

Every edit saves to the server the moment it's made (merged Q99), but no artifact draws what happens when that save fails — network drop, API down. The byline would keep claiming "Draft saved" while an edit silently never reached the shared queue, and a colleague's Publish could then ship a draft missing it.

*Recommended:* The byline flips to an amber "Couldn't save your last change — retrying…" and retries automatically; Publish is unavailable until the queue is confirmed saved; the byline never shows "Draft saved" unless it's true.

<sub>M2 Hi-Fi - Menu builder.dc.html byline "Draft saved 10:42am by Alex" — no failure state drawn anywhere in the bundle</sub>

### Q198 · important

**If pressing Publish fails outright (server error, timeout), is the rule all-or-nothing — nothing changed on any screen — with the bar saying so plainly and the draft intact?**

The publish bar's busy state and the per-screen delivery chips are specified, but the publish API call itself failing (timeout, 500) is drawn nowhere. An implementer must decide whether a failed publish can leave a half-shipped state and what the bar says.

*Recommended:* Yes — publish is atomic on the server; on failure the bar returns with "Publish didn't go through — nothing changed on your screens. Try again." and the draft queue is untouched; per-screen delivery trouble after a successful publish stays the chips' job.

<sub>M2 Hi-Fi - Menu builder.dc.html "Publish 3 changes" button; merged Q111 covers only the busy state, not failure</sub>

### Q199 · important

**When someone's sign-in expires while they're editing (or flipping an 86), what happens to the change they just made — is there a sign-back-in prompt that keeps the change and sends it after they're back in?**

Edits save to the server per keystroke-commit, so an expired session mid-shift means save calls start returning 401. Today the app has no silent re-auth; the code throws a terminal error message. Nothing in the design covers a bartender's 86 or an editor's price change hitting an expired session.

*Recommended:* Yes — on a 401 during a save, show a sign-back-in prompt over the page, hold the unsent change, and send it once re-authenticated; the change is never silently dropped and the byline shows the amber unsaved state meanwhile.

<sub>src/back-office/src/api.ts 401 handling: "That venue access link is invalid or has expired." — no re-auth flow, no drawn state in the bundle</sub>

### Q200 · important

**Is the new Menus UI English-only like the rest of the back office (no translation of its copy this build), with the venue's Secondary-language setting left in place but driving nothing — and the onboarding sentence promising "language defaults" reworded so it stops promising a dropped feature?**

The build drops per-item translations and the bilingual/AI-translation features, but the venue still carries PrimaryLanguage/SecondaryLanguage fields and onboarding copy promising "language defaults." Nobody asked whether the new Menus UI itself (all the verbatim copy) is English-only, and what those venue fields mean afterwards.

*Recommended:* Yes to all three — English-only UI, language fields dormant (logged as debt for a future localization pass), onboarding copy trimmed in the same slice that retires the starter links.

<sub>src/Vennu.Core.Models/Venue.cs "PrimaryLanguage = \"en\" / SecondaryLanguage"; CustomerOnboardingApp.tsx "These details control schedules and language defaults."; build-decisions 6 drops per-item translations</sub>

### Q201 · important

**May we set generous named ceilings — around 50 menus per venue, 500 items per menu, and a paste input of about 2,000 lines — each refusing with a plain sentence ("That paste is too big — split it into two menus") rather than failing quietly, with no tier-based limits this build?**

Merged Q119 carries per-field length limits, but nothing anywhere caps counts: how many menus a venue can have, items per menu, or what happens when someone pastes a 10,000-line document into the paste box. Unbounded input is where parsers and boards fall over, and any limit message must follow decision 5's name-the-reason rule.

*Recommended:* Yes — generous hard caps with honest sentences; numbers are engineering guardrails, not plan features, and can be raised without design work.

<sub>Menus.dc.html paste route "Looks like 2 sections and 6 items"; Menus at Scale.dc.html tops out at 13 menus / 250 items — no cap named anywhere</sub>

### Q202 · important

**Should canvas items be reachable by keyboard — Tab/arrow moves focus through items in board order, focus acts as selection so the inspector follows — and should M2b section blocks take the same Alt+Up/Down reorder convention, with column moves via the keyboard-operable "Right column" control, so drag is never the only path anywhere?**

Merged Q120 restores keyboard reordering for rail rows, but two surfaces still have no keyboard path at all: selecting an item on the M2 canvas (click is the only drawn way in, and the inspector is useless without a selection) and moving M2b's section blocks (drag is the only drawn mechanism).

*Recommended:* Yes — one convention (focus = selection, Alt+arrows = reorder) across canvas, rail, and Board view; costs little in slice 3/5 and avoids an accessibility retrofit.

<sub>M2 Hi-Fi - Menu builder.dc.html (items selected only by clicking the canvas); Menus.dc.html M2b "sections as draggable blocks" — no focus order drawn for either</sub>

### Q203 · important

**On the real TV: does an 86 take the item down immediately even mid-page (that's its promise), while a full publish waits for the next page turn and then restarts the cycle at page 1 with the new version?**

When a publish or an 86 reaches a TV that is mid-way through showing a page to guests, the design never says whether the board changes under the reader's eyes or at the next page turn. "Instant 86 removal" and "don't yank content mid-read" pull opposite ways, and repagination can reshuffle every page.

*Recommended:* Yes — 86 is immediate everywhere including mid-page; publishes swap at the next page boundary (seconds away at an 8s dwell) so guests never see a layout jump mid-read.

<sub>slice-plan.md slice 4 "pages, dwell cycle, instant 86 removal"; merged Q142 settles this for Play only</sub>

### Q204 · important

**For the slice-4 workbook, is a paired browser tab acting as the TV (its window size becoming the reported geometry) acceptable proof — with one check on a real TV device before the build closes, if you have one to point at it?**

The slice-4 workbook asks the owner to "watch the TV change," but nothing says what plays the TV during acceptance. The display player runs in a browser and can be paired like a device, so a second browser tab can be the screen — but its reported geometry would be the tab's viewport, and no real TV hardware is named anywhere in the plan.

*Recommended:* Yes — browser-tab screen for the workbook (it exercises the identical player code), plus one real-device smoke check before build close; please say what device that would be.

<sub>slice-plan.md slice 4 "Acceptance workbook: publish → watch the TV change; 86 → watch it vanish"; src/display PairingPage.tsx (player runs in a browser)</sub>

### Q205 · important

**What hardware/browser do your venues' screens actually run today — and is "the engine must run wherever the current display app already runs, proven on one real device in slice 4" the right bar, rather than naming a specific old-browser floor?**

The shared render engine must run on whatever browser the venues' actual screen hardware ships — TV browsers and signage boxes often run old Chromium builds that lack modern CSS. The display app today builds for modern browsers with no explicit floor, and nobody has stated what hardware the fleet actually is.

*Recommended:* Yes — no new floor, same reach as the current player, verified on one real device during slice 4; if the fleet includes older TV browsers, name the oldest model now so the engine avoids CSS it lacks.

<sub>src/display (no browserslist/target set — Vite modern-browser default); build-decisions 13 "the render engine is shared… so a published board is literally what the TV shows"</sub>

### Q206 · minor

**Is print fully out of scope this build — no print button, no promise that browser-printing the builder or Play produces anything usable — logged as a later candidate ("print this board" from the render engine)?**

The build renders beautiful boards, and someone will eventually hit Ctrl+P on the builder expecting a printable menu. No artifact designs for print, and an accidental browser-print of the builder would produce chrome-filled pages.

*Recommended:* Yes — out of scope, one line of debt; the shared engine makes a real print feature cheap later.

<sub>Design bundle — no print affordance or print stylesheet appears in any artifact; restaurants also hand physical menus to guests</sub>

### Q207 · minor

**Is the publish/draft history the only audit trail this build — destructive-but-instant acts like discard-draft, Put away, and Take off the screens recorded as history entries too — with no separate audit log and no analytics events required?**

The draft/publish history is attributable by design, but nobody asked whether that history IS the audit record or whether you expect separate audit logging (e.g. who discarded a draft, who put a menu away) or any usage analytics from the new surfaces this build.

*Recommended:* Yes — route those three acts into the same attributable history so nothing irreversible is anonymous; a dedicated audit/telemetry system stays out of scope.

<sub>slice-plan.md slice 1 DraftChange/PublishEvent (attributable history); src/Vennu.Core.Models has only FeatureMatrixAuditEntry — no general audit log</sub>

### Q208 · minor

**Copying the bundle verbatim: is it acceptable that the authority .dc.html files need internet to view (PNGs staying the offline orientation aids), with the stray PNGs moved into the empty screenshots/ folder so the README's paths are true?**

The .dc.html authority files render through support.js, which pulls React from a CDN — so the approved design authority in the repo needs an internet connection to open, and the README's screenshot paths point at an empty folder. Merged Q193 covers copying the bundle but not these two packaging facts.

*Recommended:* Yes — accept the CDN dependency (engineers reference the files, not air-gapped), fix only the screenshot paths so the copied README isn't lying.

<sub>support.js "REACT_URL = https://unpkg.com/react@18.3.1…"; README.md "Screenshots are for orientation. The .dc.html files are the reference" — but screenshots/ is an empty folder and the PNGs sit at the bundle root</sub>

---

**Totals:** 208 questions — 41 blocking, 122 important, 45 minor.

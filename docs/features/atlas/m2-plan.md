# Atlas Milestone 2 — Vennusign adopts the generator

> **This is a record of work that shipped**, not a proposal. It is written after the fact so the
> site can render what happened; the plan it describes was executed on 2026-08-22 and merged.

**Goal:** Make Vennusign an Atlas project, so `atlas.vennusign.com` builds itself from this
repository's own records rather than being maintained by hand.

**Where it landed:** this repository. M1 built the generator in the separate `Atlas` repository and
shipped nothing here; M2 is the whole of the Vennusign side — the config, the manifests, the
workflow, the hosting and the gate.

**Built against:** Atlas `v1.0.0`, consumed as `uses: jmiedreich-ux/Atlas@v1` (decisions 39, 46 —
the version tags live in the generator's repository, and nothing here is tagged for it).

**Landed as:** PR #778 (adoption) and PR #779 (the gate), merged 2026-08-22.

**Spec:** `docs/design/approved/atlas/decisions.md`. The decisions this milestone is answerable to
are 5–8 (hosting and access), 13–21 (content model), 30–33 (build) and 40–42 (what a project
repository must provide).

---

## What shipped

| File | What it does |
|---|---|
| `atlas.config.json` | The project's identity, its repo slug, and the seven workstream directories under `docs/features/`, in the order the site presents them |
| `docs/features/<workstream>/workstream.json` | Seven manifests — what the workstream is, its stage, its position, its gate, its design links and its milestones (decision 14) |
| `.github/workflows/atlas.yml` | Build and publish. Three triggers, least privilege, no `environment:` gate |
| `.atlas/staticwebapp.config.json` | The access gate, copied into the built artifact at deploy time |
| `ROADMAP.md` and seven milestone plan files | Records the manifests reference. They were missing from `master` and the build refused to run without them |

### The workflow

Three triggers, per decision 30: push to `master`, a six-hourly schedule, and manual dispatch. The
schedule exists because issues change without any commit here at all, so build-on-merge alone would
show stale issue panels for days.

`permissions:` is `contents: read` and `issues: read` and nothing else. Without `issues` the fetch
403s and Atlas degrades to empty buckets — which would have the site claim every workstream has no
open work, worse than saying nothing.

No `environment:` gate, per decision 31: a documentation merge must never cost a deploy approval.

### Hosting

An Azure Static Web App on the **Free** tier, at the custom domain `atlas.vennusign.com`. This is a
deliberate exception to the repository's one-App-Service-per-app convention, recorded as an
exception rather than a precedent (decision 5): Static Web Apps is the only option where answering
from a phone — what became M3 — does not become a second project, because managed Functions ship in
the same deployable behind the same auth.

It stays **off** the `appsrv-basic-web` B1 plan (decision 6). That plan already carries 28 apps on
one worker, seventeen of them idle, and restarts them all together (#748). Atlas adds nothing to it.

One instance, not one per environment (decision 8). Internal records are not versioned per
environment, so `dev.`/`stage.`/`app.` would be three copies of one truth.

The deployment token is the repository secret `ATLAS_SWA_DEPLOYMENT_TOKEN`.

---

## The build refusing to run is the feature

Seven of the plan files the manifests reference were **not on `master`**. Atlas refused to build and
named each path. That refusal is the design working, not an obstacle to it: a manifest cannot claim
a milestone the repository has no record of, which is what makes decision 1 — built from source,
never maintained — structural rather than a slogan (decisions 32, 41).

The fix was to bring the records, not to soften the check.

---

## Judgement calls, recorded so they can be argued with

- **Menus** — all eleven milestones name the shared `milestone-plan.md`, which already carries a
  heading per milestone. Eleven stub files would be records invented to satisfy a convention that
  postdates the work.
- **Historical ids are preserved; only labels normalise** (decisions 17, 18). Menus keeps `3-A` and
  `6-A1` as ids, displayed as `M3.1` and `M6.1`. Twenty-one Menus files carry those ids in their
  names and are durable acceptance evidence cited in merged PRs. Nothing was renamed.
- **Menus M5 is parked at #709 with M6 complete behind it** — recorded honestly rather than
  smoothed over. M1's chart then read Menus as four milestones deep when nine were done, which is
  what M2.1 fixed.
- **Keystone** — all six milestones `gated` (later `blocked`), not `next`: nothing starts until the
  design authority leaves `docs/design/proposed/`.
- **Theme Studio, Screens, Onboarding, Platform Operations** — no milestones. They exist in Design
  and nowhere else, so an empty array is the accurate picture rather than a gap.
- **Mosaic excluded** — ROADMAP's own rule separates release codenames from feature codenames.

---

## What went wrong, and what it cost

**The site shipped public.** Every route — `ROADMAP`, the seven workstreams, and the ~370
work-package records under `docs/work-packages/` — returned 200 to an anonymous request. Atlas
decision 7 says nothing on this site is anonymous; the generator emitted no
`staticwebapp.config.json` at v1.0.0, so nothing was telling the Static Web App to require anything.
It was closed eighteen minutes later by PR #779.

Every route now requires the `reader` role, granted by invitation only; an unauthenticated request
gets a 302 to login. `/.auth/*` stays open because the login flow lives there. The Free tier's
25-invitation ceiling is accepted as sufficient (decision 7).

**CIAM was considered and rejected.** Custom OIDC providers need the Standard tier, and CIAM is the
*customer* directory — gating an internal operations surface behind it would give every customer who
ever signs up an identity that authenticates here, with only a role check keeping them out. An
invitation list is a smaller blast radius.

**The gate is copied in at deploy time rather than generated**, which was recorded as an M2 finding
against the generator and fixed in Atlas v1.1.0. The copy step still has to stay, for a reason worth
writing down rather than rediscovering: the generator's default signs in with **Microsoft** per
decision 7, while this deployment signs in with **GitHub** — the one identity that could be verified
for the owner on the night the site was gated. Deleting the step believing Atlas now handles it
would silently redirect sign-in to a provider this app is not configured for. That correction is PR
#781, and it belongs to M2.1.

The underlying mismatch is a decision to reconcile rather than a defect: decision 7 says Microsoft
and the only real deployment uses GitHub, so **decision 7 is arguably the stale half**. Still open.

---

## Deliberately excluded

- **Application code.** This milestone changed no product code, no API, no front end.
- **Per-milestone plan files for Menus.** See the judgement calls above.
- **Write-back.** Decisions 34–37, which became M3.
- **A UI of any kind.** Atlas at v1.0.0 is read-only (decision 34); everything actionable links out
  to the file or the issue.

---

## Known and unresolved at the end of this milestone

- **`docs/work-packages/` is ~370 retired-era files and every one of them renders.** The first
  published site is weighted toward material the owner may not want surfaced. Whether Atlas should
  read that directory at all is his call, recorded in #780.
- **No `workstream:*` labels exist in GitHub.** 46 of 52 open issues carry none, so issues are
  collected but not bucketed until the labels exist.
- **Decision 33 is unmet, and its subject no longer exists.** The decision says `src/atlas/**` is
  added to `scripts/ci/classify-changes.sh`'s allow-list as a no-deploy class — but the generator
  became its own repository (decision 39), so `src/atlas/**` never came to exist. What did land here
  is `atlas.config.json` and `.atlas/`, and **neither is in the allow-list**: a change to either
  trips the fail-safe and redeploys all five applications. `docs/*` and `.github/workflows/*` were
  already covered, so the manifests, the records and the workflow are fine; the two new paths at the
  repository root are not.
- **Merging this milestone triggers `deploy-dev` and `ui-regression`,** which fire on any push to
  `master`, in addition to `atlas.yml`.

---

## Verification

Built against Atlas v1.0.0 from a clean checkout of `master`: **376 pages, 108 copied files, exit
0.** The published site is the acceptance evidence — there is no workbook, because this milestone
ships no customer-facing UI.

# Keystone — decisions so far

Status: **not an approved feature.** This file records what was settled in conversation on
2026-08-20 so it is not lost. It confers no implementation authorization. The source
concept, `docs/design/progressive-customer-cutover-concept.md`, remains explicitly
unapproved: "No implementation should begin from this concept alone."

## Name and scope

**Keystone** is the codename for the feature that builds the progressive-cutover **thin
layer and discovery services**. Named for the stone placed first that everything else
bears on, and that cannot be removed while the arch stands — which is the defining
property of this layer: it cannot roll itself out progressively, so it deploys
all-customers-at-once, backward-compatible only, with immediate rollback as the sole
recovery path.

This is a *feature* codename and is separate from *release* codenames, where v1.0 is
Mosaic.

In scope: **Product Router**, **POS Webhook Receiver**, **connection membership**,
**Version Discovery Service (VDS)**, **Application Discovery Service (ADS)**.
**System Monitor** is named in the concept but is later work.

## Hosting topology — decided in shape, not in cost

Question asked: one App Service for the three thin-layer services, one plan holding thin
layer plus discovery, or a plan each?

**Decided shape:**

- **Separate App Services, not one bundled app.** The reason these components are kept
  thin is that a Webhook Receiver change must not be able to break routing. One process
  re-couples exactly what the design separates, and since immediate rollback is the only
  recovery path, a single bundle means rolling back all three to fix one.
- **One shared App Service Plan for Keystone, not a plan each.** A plan bills whether or
  not it is full, and four of these are small. A plan each buys isolation whose need
  cannot yet be measured.
- **Standard tier, not Basic.** Deployment slots are Standard-and-up, and slot swap with
  instant swap-back is the stated recovery model for these components.
- **Keystone's plan must be separate from the versioned `Vennu.Api` plan.** This is the
  boundary that matters most, and it follows from the concept doc's own constraint that
  concurrently running versions divide their plan's CPU and memory. If Keystone shared
  that pool, starting v1.6 alongside v1.5 would starve the Product Router — which sits on
  every request and whose latency is paid on every call.
- **Product Router graduates to its own plan later,** when plan-aggregate CPU or its
  added p99 becomes measurable. Not before.
- **Connection membership is probably not an App Service at all.** If Azure SignalR
  Service or a Redis backplane wins, it is managed infrastructure with no plan. The
  mechanism is still open, so no capacity is reserved for it.

Indicative shape:

```
asp-keystone-<env>   (Standard, slots)
  ├─ vds       lookup API                 request-serving
  ├─ ads       registry + health poller    continuous, low load
  ├─ wr        POS webhook receiver        bursty; holds POS secrets
  ├─ router    Product Router              on every request  <- first to graduate
  └─ sysmon    System Monitor              later; holds Azure management credentials

asp-versions-<env>   (separate)   api v1.4, v1.5, ...
connection membership -> managed service, no plan, mechanism undecided
```

**Open thread, deliberately deferred:** tier and plan cost. Standard is a tier change from
the current `rg-basic-website` arrangement and the owner has not yet accepted that cost.
This needs its own conversation before any plan is provisioned.

## First slice — superseded

**Originally recommended: VDS + ADS together.** Bootstrap order argued for it: nothing
routes until VDS answers, and VDS cannot answer usefully until ADS knows where healthy
instances are. It also ships with no traffic depending on it yet, so it is independently
mergeable and harmless if wrong.

Alternatives considered at the time: Product Router first (proves the request path early,
but needs a VDS stub and puts a new component on every request before the lookup behind it
is real); Webhook Receiver first (smallest clear boundary, but exercises none of the
assignment machinery).

**Superseded 2026-08-20.** Brainstorming found that VDS's lookup signature is undefined
until caller identification is settled, so slice 1 became the TenantContext contract and
its library, and VDS + ADS became slice 2. See *Brainstorming session — 2026-08-20* below.

## Governance gaps to close before implementation

- Keystone is a **new feature area**. It needs its own design authority under
  `docs/design/approved/keystone/` and its own question register under
  `docs/features/keystone/`. Nothing is inherited from Menus.
- The concept document is unapproved. Its `Decisions required before planning` list is
  long and includes items Keystone cannot avoid: unit of assignment, behaviour when VDS is
  unavailable on every path, the shared connection mechanism, and the trust boundary for
  forwarded webhook requests.
- Naming is settled and should be used consistently: VDS, Product Router, ADS, System
  Monitor. "Version Router" and "enforcement point" are retired.

## Method

The build is intended to run through the **superpowers** plugin (installed at user scope
on 2026-08-20): brainstorming → git worktree → written plan in 2–5 minute tasks →
subagent-driven development with two-stage review → red/green TDD → code review against
the plan → branch finishing. No feature record or design authority was pre-written here,
deliberately: brainstorming is that workflow's first stage and pre-empting it would defeat
the point of trying it.

## Brainstorming session — 2026-08-20

Run through the superpowers `brainstorming` skill, architectural path (new subsystem, new
feature area). The session did not reach a written design authority; it settled the items
below and surfaced the rest as open. Status is unchanged: recorded so it is not lost,
conferring no implementation authorization.

### The problem the session found

The Product Router must decide which version serves a request *before* the request reaches
the API. For authenticated back-office traffic the tenant identity does not exist in the
request — it is manufactured inside the API after authentication. `BackOfficeMenusController`
reads its venue from `User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)`, a
claim that exists only once the opaque `__Host-Vennusign.CustomerSession` cookie has been
resolved against the database. The Router sits in front of that and never sees the claim.

This is a VDS contract question, not only a Router question: VDS's entire public surface is
one lookup — *given something, return a version* — and that "something" is undefined until
caller identification is settled.

**It is partly solved already.** `venueFetch` in `src/back-office/src/api.ts` sets
`X-Vennusign-Venue-Id` from `localStorage`, and `CustomerBackOfficeAuthenticationHandler`
reads it; `BackOfficeAuthenticationDefaults` has carried `VenueSelectionHeaderName` and
`OrganizationIdClaim` all along. The mechanism exists — hand-rolled, in one client, on one
auth path, conditional on a venue being selected, with no contract behind it.

### TenantContext

**The carrier is named TenantContext.** A uniform, explicit statement of which tenant a
request is *about*, supplied by the caller rather than derived after authentication. Named
for the subject rather than the caller deliberately: "who am I" and "who is this about"
diverge — a support user acting on a customer's venue is both — and `SupportAccessGrants`
makes that a real case.

**Hint and authority are separate sources, always.** Every routing input the Router reads —
`venueId` in a display path, the TenantContext header, any Keystone cookie — is
caller-asserted and unverified. It selects a version and nothing else. Every authorization
decision stays inside the versioned application, derived from a durable authority: the
screen record, the customer session, the Webhook Receiver's own registration table. Where
the two disagree the authority wins, the request is not served as though the hint were true,
and the disagreement is emitted as telemetry — a mismatch means the Router mis-routed, which
makes it the built-in detector for assignment drift during a rollout.

The check is free where it matters most: `DisplayController.GetContent` already loads the
screen by id before serving anything, so comparing `screen.VenueId` to the URL costs
nothing.

**TenantContext is a cache, never the authority.** A cookie or storage entry holding a
tenant is an optimisation. If it is lost, the tenant must still be re-derivable from
something durable — the session for back office, the screen record for a display. Nothing
becomes unroutable because a browser was cleared.

**The Router forwards; it never hands out per-version hostnames.** The `__Host-` prefix on
`__Host-Vennusign.CustomerSession` forbids a `Domain` attribute, pinning the cookie to the
exact host that set it. Any design that resolves once per session and sends the browser to a
per-version hostname destroys the session at every cutover. This settles the concept's open
question between "one edge/gateway hop per request" and "a resolved-once-per-session
endpoint the bundle reuses" in favour of same-host forwarding, for browser traffic at least.

**Uniform on the wire.** The Router gets exactly one extraction rule for post-auth API
traffic: the TenantContext header. It never pattern-matches a versioned API route, because
route shapes belong to a version and the thin layer cannot be coupled to something that
changes underneath it — the reason the alternative, reading `screenId` out of
`/api/display/{screenId}/content`, was rejected. The usual objection to uniformity — that it
forces an update to devices in the field — does not apply, because nothing is live.

**A header cannot ride on a document navigation.** The initial load of any SPA is a plain
browser navigation, so TenantContext covers only the `fetch`/XHR calls a bundle makes after
it boots, never the bundle load itself. What carries tenant on a document navigation is
open.

### The pre-auth / post-auth line

**Pre-auth means no verifiable tenant exists** — not that one failed to be sent. It covers
the public site, a TV at `/pair` before it is claimed, `POST /api/screens`, the pairing-code
lifecycle and its status poll, and the back-office sign-in round trip.

**Post-auth means the tenant is derivable from a durable authority** — the customer session
for back office, the screen record for a paired display, the Webhook Receiver's registration
for a POS event.

**The line is crossed at exactly two events:** a person crosses at sign-in, a device crosses
at claim. Before those, no plumbing produces a tenant; after them, it is always
re-derivable.

**Pre-auth writes nothing.** A device acquires an identity without creating a product row;
the row appears at claim — on the post-auth side, where a tenant exists. Rejected
alternatives: routing pre-auth API calls to a designated version makes that traffic silently
dependent on one version staying alive; moving those endpoints into the thin layer would
couple Keystone to the product's `Screens` schema, which is exactly the coupling the
Webhook Receiver's own registration table exists to avoid.

Today's behaviour, for contrast: `preparePairingScreen` in `src/display/src/pairing.mjs`
calls anonymous `POST /api/screens`, which creates a screen record belonging to nobody, and
the device then polls for a claim.

**Display carries the venue in its URL** — `/display/{venueId}/{screenId}`, as a hint under
the rule above. `resolveDisplayRoute` in `src/display/src/routing.ts` currently matches
`/display/{screenId}` only.

### Scope corrections

**`dev.vennusign.com` is out of the version equation.** It serves the public marketing site
(`src/www`). It is not versioned, and the Product Router does
not route it.

**Platform Operations is a separate app.** The concept document's Environments section is
wrong where it places "the front door into the real PO assignment workflow" and the version
chooser at `dev.vennusign.com`; Platform Operations is a separate app at
`po.vennusign.com`. If a version chooser survives, it belongs at PO's own address, not on
the public site. The concept doc needs this correction, and
the dev multi-version testing problem it was solving needs somewhere else to live.

### First slice — changed

**Slice 1 is the TenantContext contract and its library, not VDS + ADS.** It lands in the
existing API and front ends with no Keystone infrastructure at all, is backward-compatible
by construction (a header nothing yet reads), is independently mergeable and harmless if
wrong, and it makes VDS's lookup signature a fact rather than a guess. **VDS + ADS becomes
slice 2**, by which point its contract is known.

The library normalizes: one resolved TenantContext out, whatever came in, so a second
extraction rule later is an adapter inside the library rather than a change to the Router.

### Carried to the next session

- What the API does on a hint/authority mismatch — redirect the client to the correct URL,
  or return a status it re-resolves on.
- What carries tenant on a document navigation: a path segment, a Keystone-owned cookie set
  on the Router's own host, or serving unattributed and letting the booted bundle
  self-correct. These are not exclusive.
- Which version serves any residual unattributed traffic, and which direction of
  expand-and-contract compatibility that leans on.
- Whether claiming a screen moves it to its venue's version immediately or at reconnect.
- Unit of assignment: organization or venue. Now informed by two facts — the wire carries a
  venue today, and `Venue.OrganizationId` is nullable by deliberate design, so that existing
  venues could be migrated rather than assigned to an invented tenant.
- Whether the TenantContext contract is defined wire-format-first. Required if the Product
  Router is not .NET, since Keystone components and the versioned API are separate
  deployables.
- Whether onboarding moves out of back office to a pre-auth surface.
- Device auto-re-pair after cleared storage — parked by the owner as its own conversation.
  Today's recovery is `POST /api/back-office/screens/pairing/replacement`, which the owner
  judged poor from a user's perspective.

### Asked and not yet answered

- Whether Keystone decides the shared connection-membership mechanism or also builds it.
  Azure SignalR Service is the only Keystone item that certainly costs money, and tier and
  plan cost remain deferred.

### Defects found, to be filed separately

- `POST /api/screens` (`ScreensController.cs:57`) is anonymous, and nothing reaps unclaimed
  screens. Repeated storage clears leave ghost records consuming a venue's
  `screen.device.pair` allowance, and screen rows can be created by anyone. Pre-existing;
  Keystone would inherit it, not cause it.

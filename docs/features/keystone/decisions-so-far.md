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
traffic. It never pattern-matches a versioned API route, because route shapes belong to a
version and the thin layer cannot be coupled to something that changes underneath it — the
reason the alternative, reading `screenId` out of `/api/display/{screenId}/content`, was
rejected. The usual objection to uniformity — that it forces an update to devices in the
field — does not apply, because nothing is live.

> **Superseded in the fourth sitting, as to mechanism only.** This sitting named the
> TenantContext *header* as that single rule. The second sitting then put the tenant in the
> URL path for document navigations, leaving two mechanisms and contradicting the very
> uniformity this decision exists to assert. The path won; the header is retired. The
> principle above — one extraction rule, never coupled to a versioned route shape — is
> unchanged.

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
the public site. Filed as **#743**, which also carries the consequence: the dev
multi-version testing problem that text was solving needs somewhere else to live.

### First slice — changed

**Slice 1 is the TenantContext contract and its library, not VDS + ADS.** It lands in the
existing API and front ends with no Keystone infrastructure at all, is backward-compatible
by construction (a header nothing yet reads), is independently mergeable and harmless if
wrong, and it makes VDS's lookup signature a fact rather than a guess. **VDS + ADS becomes
slice 2**, by which point its contract is known.

The library normalizes: one resolved TenantContext out, whatever came in, so a second
extraction rule later is an adapter inside the library rather than a change to the Router.

### Carried to the next session

*Status after the second sitting: resolved items name where they were settled.*

- **Resolved (Section 2).** What the API does on a hint/authority mismatch — 421 Misdirected
  Request for API calls, 307 to the correct URL for document navigations.
- **Resolved (Section 3).** What carries tenant on a document navigation — the path, on both
  Display and back office.
- **Resolved (Section 4).** Unit of assignment — per venue, with scheduling per organization,
  and every venue belonging to an organization.
- **Resolved in shape (Section 3).** Whether onboarding moves out of back office to a
  pre-auth surface — `/signin`, `/signup` and `/onboarding` are pre-auth root routes carrying
  no tenant.
- **Still open.** Which version serves any residual unattributed traffic, and which direction
  of expand-and-contract compatibility that leans on.
- **Still open.** Whether claiming a screen moves it to its venue's version immediately or at
  reconnect.
- **Still open.** Whether the TenantContext contract is defined wire-format-first — required
  if the Product Router is not .NET.
- **Still parked by the owner.** Device auto-re-pair after cleared storage. Today's recovery
  is `POST /api/back-office/screens/pairing/replacement`, which the owner judged poor from a
  user's perspective.

### Asked and not yet answered

- **#742** — whether Keystone decides the shared connection-membership mechanism or also
  builds it. Azure SignalR Service is the only Keystone item that certainly costs money, and
  tier and plan cost remain deferred.

### Defects found, filed

- **#741** — `POST /api/screens` (`ScreensController.cs:57`) is anonymous, and nothing reaps
  unclaimed screens. Repeated storage clears leave ghost records consuming a venue's
  `screen.device.pair` allowance, and screen rows can be created by anyone. Pre-existing;
  Keystone would inherit it, not cause it.

## Brainstorming, second sitting — 2026-08-20

Continues the architectural path. Sections 2 to 4 were presented and approved in turn. Same
status as everything above: recorded so it is not lost, conferring no implementation
authorization.

### Section 2 — resolution and correction

**Context is stamped at the crossing.** The two events that cross the pre-auth/post-auth line
are also where TenantContext is established, and both are already server-controlled responses
that need only gain a field:

- A person crosses at sign-in. `CustomerOidcEvents` already appends the session cookie and
  redirects in `TicketReceived` — the first moment the tenant is knowable.
- A device crosses at claim. The device is already polling
  `GET /api/screens/pairing/{code}/status`; the response that says "claimed" carries its venue
  and where to navigate.

**Self-correction is mandatory, not a fallback.** A cookie can be cleared, a path can be
stale, and a first visit has neither, so there is no arrangement in which the Router always
knows. The bundle boots, learns its true tenant from its first authenticated call, compares
against the version that served it, and reloads on disagreement. The concept already asks for
the ingredient: a version identifier returned on API responses, compared against what the
client booted with.

**Two distinct mismatches, and conflating them is the trap.**

| | Cause | Detected by |
|---|---|---|
| Mis-forwarded | Router resolved v1.5, request arrived at v1.4 — stale ADS entry, pool drift | Router stamps the version it resolved; the API compares against its own `VENNU_COMPONENT_VERSION`. One string compare, no lookup. |
| False premise | The hint said venue A, the authority says venue B | Where the authority is already loaded — free in `DisplayController.GetContent` |

**Both answer 421 Misdirected Request** for API calls. It is the exact HTTP semantic, and it
forces explicit re-resolution rather than inviting a client to follow a redirect blindly. A
*document* navigation gets a 307 to the correct URL instead, since no client code is running
yet to interpret a 421. Serving anyway and logging is not an option: it is precisely the
mismatch the feature exists to prevent.

**Internal tokens for everything.** Chosen over network isolation, as the standing rule for
every internal hop — Router to API, Webhook Receiver to API, and onward.

- **Asymmetric, not a shared secret.** The Router signs with a private key and each version
  verifies with the public key, so a compromised API version cannot forge Router tokens. Keys
  in Key Vault.
- **Audience-scoped and short-lived.** The token names the version it was minted for, so one
  issued for v1.4 cannot be replayed against v1.5, and it lives seconds, being per-request.
- **It records how the tenant was established, not only what it is.** The Webhook Receiver's
  assertion is stronger than the Router's — WR verified a provider signature, so its venue is
  a verified fact, while the Router resolved a caller-asserted hint. Same envelope, different
  provenance, and the receiving version needs to know which it got.

> **An internal token authenticates the hop, not the claim.** It proves the request came from
> the Router and was not tampered with in transit. It does not convert a caller-asserted
> tenant into a verified one. Authorization still comes from the authority.

Stated explicitly because "it is signed, so we can trust it" is the reasoning that would
otherwise erode the hint/authority rule later.

### Section 3 — the URL shape

**Back office has no router library.** All in-app navigation is hash-based — `#/menu`,
`#/menu/{id}`, `#/screens`, `#/menu/quick-update` — driven by a `hashchange` listener at
`App.tsx:246`, with the pathname preserved and unused for in-app routing. The hash is never
sent to the server, so the pathname is free and there is no route table to rewrite. The change
is to serve the SPA under a tenant-prefixed path and read the tenant from `location.pathname`
rather than `localStorage`. `main.tsx:14` already special-cases `/signup`, `/signin` and
`/onboarding` by pathname, which are exactly the pre-auth routes.

**Decided: back office takes the URL restructure, not a Keystone cookie.** Consistent with
Display, and it survives a cleared browser.

```
/signin  /signup  /onboarding          pre-auth · no tenant · root
/pair                                  pre-auth · device seeking an owner
/o/{orgId}/v/{venueId}#/menu           post-auth · tenant in path, app route in hash
/display/{venueId}/{screenId}          post-claim · device
```

**The pre-auth/post-auth line becomes visible in the URL** — determinable by inspection,
by a human or by the Router, with no lookup and no session resolution. Org-scoped surfaces
also gain a coherent home at `/o/{orgId}` with no venue segment, rather than sitting under an
arbitrary venue.

Platform Operations' own URL shape is not decided. PO's tenant is Vennusign itself rather than
a customer, so it may not be version-routed by tenant at all.

### Section 4 — unit of assignment

**Every venue belongs to an organization.** Single-venue operators have an organization of
one; multi-venue is simply an organization with more than one venue. This retires the org-less
venue as a supported shape and makes `/o/{orgId}/v/{venueId}` always well-formed.

**Assignment is per venue; scheduling is per organization.** Maintenance windows are
inherently venue-local — they are about service hours at a physical place, and `Venue.Timezone`
already models that per venue — so a group spanning time zones cannot share one window. But a
multi-venue manager working across two versions is a bad experience, so a wave groups an
organization's venues together: they enter a rollout as a unit and each moves at its own local
window. The organization crosses over one night rather than in one instant, and never
straddles for days.

**The Router only ever keys on venue.** *(Owner: organizations are a reporting umbrella and a
scheduler helper, not a rollout unit.)* The org segment exists in the URL for the application —
scoping data, multi-venue functions, reporting — and is not a routing input. No organization is
ever assigned a version. A session always has a current venue, so org-scoped surfaces are
served by that venue's version. An organization with no venues yet has nothing to route to,
which is onboarding, already a pre-auth root route.

Consequently **VDS's lookup is venue-keyed**, which matches both the path and the
`X-Vennusign-Venue-Id` header the wire already carries.

**Assignment is never derived from subscription shape.** Subscriptions are expected to be
flexible — venue-level, organization-level, or both at once, with merging, moving and
adjustment as circumstances dictate. That framework is explicitly not being designed now. The
only rule that matters to Keystone is that the two axes stay independent: a billing change must
never be capable of silently moving a customer to a different version.

### Product change identified, to be designed elsewhere

**Signing in should establish an organization, not a venue.** The schema already says so:
`FK_VenueMemberships_OrganizationMemberships` is a composite foreign key on
`(OrganizationId, UserId)`, so a venue membership cannot exist without an organization
membership — venue access is a refinement of organization access, enforced in the database. The
role model agrees, making organization owner/admin/member the primary grants "augmented by any
venue-specific role." Today's session disagrees with both by pinning a single `VenueIdClaim`,
which is why `loadBackOfficeSession` has to clear its stored venue and retry on a 401.

Under this model the organization is the session and the venue is navigation — which is what
`/o/{orgId}/v/{venueId}` already expresses. One constraint follows: switching venue must be a
document load rather than an in-app transition, or an org-version shell would host
venue-version content, reproducing the frontend/backend mismatch one level up. The URL
restructure supplies that document load for free.

**This is not a Keystone decision.** It touches session issuance, the venue switcher,
multi-venue features and the approved authentication design authority, so it needs its own
decision in the back-office/authentication area. It is recorded here as a strong recommendation
and a Keystone-adjacent finding, deliberately not settled as part of an infrastructure feature.

### Confirmed: data protection keys are not shared

`Program.cs:189` is a bare `AddDataProtection()` — no `PersistKeysTo*`, no
`SetApplicationName`, no `ProtectKeysWith`. On App Service that places the key ring in per-app
storage, so two versions running as separate App Services cannot read each other's protected
values. The concept lists this as something to confirm early; it is now confirmed, and the
present configuration guarantees the failure.

The session itself is unaffected: `CustomerSessionCookie` stores a raw opaque token resolved
against the database, so any version can honour it.

Four values are protected, and they split by severity:

| Protector | Purpose | Lifetime | Effect of a version move |
|---|---|---|---|
| `DataProtectionPosCredentialProtector` | POS credentials | durable at rest | **breaks permanently** |
| `DataProtectionCustomerSecretProtector` | `Vennu.CustomerAuthentication.StrongFactors.v1` | durable at rest | **breaks permanently** |
| `ProtectedPosOAuthStateService` | `Vennu.PosOAuthState.v1` | seconds | one in-flight link attempt fails; retry works |
| `CustomerPasskeyService` | `Vennu.CustomerAuthentication.PasskeyChallenges.v1` | seconds | one in-flight challenge fails; retry works |

A shared key ring with an explicit application name is therefore a prerequisite for moving any
customer between versions. It is a change to `Vennu.Api` startup rather than a Keystone
component, but it must land before the first assignment ever changes.

### Still open after the second sitting

- **Resolved in the third sitting.** Which version serves residual unattributed traffic, and
  which direction of expand-and-contract compatibility that leans on.
- Whether claiming a screen moves it to its venue's version immediately or at reconnect.
- Whether the TenantContext contract is defined wire-format-first — required if the Product
  Router is not .NET.
- Platform Operations' own routing and URL shape.
- Where the shared data-protection key ring lives, and who owns landing it.
- Whether Keystone decides the shared connection-membership mechanism or also builds it (#742).
- Device auto-re-pair after cleared storage — parked by the owner as its own conversation.

## Brainstorming, third sitting — 2026-08-20

Same status: recorded so it is not lost, conferring no implementation authorization.

### Which version serves unattributed traffic

**The default version serves it, and "default" is an explicit pointer Platform Operations
sets.** The owner's decision is that the latest version serves unattributed traffic. The
qualifier is that "latest" cannot mean "newest registered."

Registration is a fact about what exists; assignment is the decision that affects customers.
That seam is what the concept is built on. If registering v1.6 immediately handed it every
sign-in, a version with zero assigned customers would be serving customer-facing traffic — and
a broken one would lock out everybody, including customers sitting safely on an older version.
The pointer therefore advances as a deliberate PO act, normally once a first wave is healthy,
so unattributed traffic follows customers onto a version rather than leading them onto it.

What follows from it:

- **New customers start on the default version.** They have never been assigned anything, and
  onboarding is pre-auth, so they are created by whatever serves unattributed traffic and
  assigned there.
- **Compatibility direction stops mattering**, because of the first sitting's decision that
  pre-auth writes nothing. The single exception is sign-in creating a session, and
  `CustomerSessionCookie` stores a raw opaque token resolved against the database, so any
  version can honour a session the newest version issued.
- **Sign-in hands off cleanly.** The default version authenticates and redirects to
  `/o/{orgId}/v/{venueId}`; that request then routes to the customer's own version.

### The pre-auth surface — a second product change, and the same one

*Recorded, not decided. Keystone is the reason it was found, not the right owner for it.*

**Onboarding should be its own app.** The split already exists in the code. `src/back-office/src/main.tsx`
is a two-way switch at the root between two unrelated components:

```jsx
const customerEntryRoute = ["/signup", "/signin", "/onboarding"].includes(
  window.location.pathname.replace(/\/$/, ""));

{customerEntryRoute ? <CustomerOnboardingApp /> : <App />}
```

That pathname list is exactly the pre-auth set derived independently from routing needs in the
second sitting. So this is not splitting one app in two — it is unbundling two apps that are
already separate, and letting the deployment boundary match the architectural one.

The shared surface is small. `CustomerOnboardingApp` is 304 lines against `App`'s 714, and
imports only `config`, the api client, two onboarding-only components, and
`customerEntryRouting.mjs` — so a small shared package or a little duplication, not a
disentangling job. Note that `customerEntryRouting.mjs` already exports
`authenticatedCustomerDestination`, the "where do you go once authenticated" handoff, which
under the URL restructure becomes `/o/{orgId}/v/{venueId}`. The seam is already named in the
code.

**What Keystone gets from it:** back office can then assume it always has a tenant, because it
is only ever entered post-auth from a tenant-bearing URL. The whole "no tenant yet" state
leaves that app, including the `loadBackOfficeSession` clear-and-retry dance.

**This is the same change as org-as-login.** Both are consequences of taking the
pre-auth/post-auth line seriously in the *product* rather than only in the routing, so they
should be designed as one piece of work — a pre-auth surface covering `/signup`, `/signin` and
`/onboarding` — rather than as two unrelated recommendations. Both touch session issuance, the
venue switcher, multi-venue features and the approved authentication design authority, and
both belong in the back-office/authentication area rather than inside an infrastructure
feature.

### Still open after the third sitting

- **Resolved in the fourth sitting.** Whether claiming a screen moves it to its venue's
  version immediately or at reconnect.
- **Resolved in the fourth sitting.** Whether the TenantContext contract is defined
  wire-format-first.
- Platform Operations' own routing and URL shape.
- Where the shared data-protection key ring lives, and who owns landing it.
- Whether Keystone decides the shared connection-membership mechanism or also builds it (#742).
- Device auto-re-pair after cleared storage — parked by the owner as its own conversation.

## Brainstorming, fourth sitting — 2026-08-20

Same status: recorded so it is not lost, conferring no implementation authorization.

### Screen version movement

**Claim is immediate, and it is not a choice.** Claiming forces a navigation from `/pair` to
`/display/{venueId}/{screenId}`. That is a different path, therefore a document load,
therefore routed. The move falls out of the URL shape rather than needing a mechanism of its
own.

**Reassignment is at reconnect**, per the concept's existing decision: version-scoped SignalR
groups mean a screen changes version at reconnect rather than mid-session, so a cutover does
not sever an active display session.

**The client forces that reconnect.** `docs/architecture/player-delivery-reliability.md`
records that the player periodically recovers authoritative content independently of push.
Those recoveries are HTTP, so they route to the *new* version, while the SignalR connection is
still held by the *old* version's hub — and a TV may never drop it on its own. That leaves a
screen pulling content from v1.5 while listening for events on v1.4, potentially for hours.

The fix reuses Section 2's self-correction rather than adding a mechanism. API responses
already carry a version identifier that the client compares against what it booted with;
applied to the socket as well as the bundle, the rule becomes: if the version answering my
HTTP differs from the version holding my connection, drop and reconnect. One signal, two uses.

### The public contract is the URL, and the header is retired

**A contradiction between the first and second sittings, resolved in favour of the path.** The
first sitting made the TenantContext header the single extraction rule; the second put the
tenant in the path for document navigations. Two mechanisms is exactly what "uniform on the
wire" existed to prevent.

**The path already does everything the header did.** A bundle served from
`/o/{orgId}/v/{venueId}/` making a *relative* call to `api/back-office/menus` has it resolved
by the browser to `/o/{orgId}/v/{venueId}/api/back-office/menus`. The tenant rides along with
no client code at all: `venueFetch`'s `localStorage` read and conditional header are deleted
rather than formalised, and nothing can forget to send what it does not send. Display is
already path-shaped, a future service-to-service caller constructs its URLs explicitly, and
POS webhooks need neither mechanism because WR resolves merchant to venue from its own
registration table.

**The API needs no route changes.** The Router consumes the tenant prefix and forwards the
bare path, so `BackOfficeMenusController` stays at `api/back-office/menus`. The tenant travels
onward in the signed internal token, so the API never parses a tenant out of a URL at all.

### Wire-format-first — resolved, in two layers

| | Contract | Trust |
|---|---|---|
| **Public** | the URL shape — `/o/{orgId}/v/{venueId}/…` | caller-asserted, unverified — a hint |
| **Internal** | the signed token, Router to API | authenticates the hop, not the claim |

The contract is wire-format-first, but the answer is not "define a header format": the public
contract is a URL shape and the internal one is a token. A .NET library is a convenience for
producing and consuming them, never the contract itself.

This holds regardless of whether the Product Router turns out to be YARP or something else.
Concurrently running API versions are built at different times from different commits, so a
shared assembly would require every live version to carry a compatible copy — which makes the
wire format the real contract in any case. It is the same reasoning the concept already
applied to the Webhook Receiver's registration API: an API surface that can be versioned,
rather than a table shape every live version must agree on.

**The format is additive-only, permanently.** Keystone cannot roll out progressively, and an
older live version cannot be retro-updated to understand a change, so nothing may ever be
removed or reinterpreted — only added, in ways an old parser safely ignores.

### Still open after the fourth sitting

- Platform Operations' own routing and URL shape.
- Where the shared data-protection key ring lives, and who owns landing it.
- Whether Keystone decides the shared connection-membership mechanism or also builds it (#742).
- Device auto-re-pair after cleared storage — parked by the owner as its own conversation.

# Keystone — decisions on record

Status: **proposed, not approved.** Every decision below was settled with the owner across six
brainstorming sittings on 2026-08-20. Moving this bundle to `docs/design/approved/keystone/`
is the owner's act and has not happened. Until it does, this document governs nothing.

The source concept, `docs/design/progressive-customer-cutover-concept.md`, remains explicitly
unapproved. The conversational record these decisions were distilled from is
`docs/features/keystone/decisions-so-far.md`, which keeps the reasoning, the alternatives
rejected, and the code evidence behind each item.

Decisions are numbered and written as rules. Where any other document disagrees with this one,
this one wins. Open questions live in `docs/features/keystone/open-questions.md` and are never
resolved silently.

---

## Scope and naming

**1 · Keystone is the thin layer and the discovery services.** In scope: **Product Router**,
**POS Webhook Receiver**, **connection membership**, **Version Discovery Service (VDS)**, and
**Application Discovery Service (ADS)**. **System Monitor** is named in the concept and is
later work.

**2 · Keystone cannot roll itself out progressively.** It deploys all-customers-at-once,
backward-compatible only, with immediate rollback as the sole recovery path. Every constraint
that follows in this document descends from that one fact, which is also what the codename
records: the stone placed first, that everything bears on, and that cannot be removed while the
arch stands.

**3 · The names are settled.** VDS, Product Router, ADS, System Monitor. "Version Router" and
"enforcement point" are retired and must not reappear. Prefer full names in speech: "WR" and
"VDS" are easily confused when spoken.

**4 · Keystone is a feature codename, not a release codename.** Release codenames are separate;
v1.0 is Mosaic.

---

## The pre-auth / post-auth line

**5 · Pre-auth means no verifiable tenant exists.** Not that one failed to be sent — that there
is none. It covers the public site, a TV at `/pair` before it is claimed, `POST /api/screens`,
the pairing-code lifecycle and its status poll, and the sign-in round trip itself.

**6 · Post-auth means the tenant is derivable from a durable authority.** The customer session
for back office, the screen record for a paired display, the Webhook Receiver's registration for
a POS event.

**7 · The line is crossed at exactly two events.** A person crosses at sign-in; a device crosses
at claim. Before those, no plumbing produces a tenant. After them, it is always re-derivable.

**8 · Pre-auth writes nothing.** A device acquires an identity without creating a product row;
the row appears at claim, on the post-auth side, where a tenant exists. Routing pre-auth calls
to a designated version was rejected because it makes that traffic silently dependent on one
version staying alive. Moving those endpoints into the thin layer was rejected because it would
couple Keystone to the product's `Screens` schema — the coupling the Webhook Receiver's own
registration table exists to avoid.

**9 · Context is stamped at the crossing.** Both crossings are already server-controlled
responses that need only gain a field: the sign-in redirect, and the pairing-status response
that reports a claim. Neither needs a new mechanism.

---

## TenantContext

**10 · TenantContext states which tenant a request is *about*.** Named for the subject, not the
caller, and deliberately: "who am I" and "who is this about" diverge whenever a support user
acts on a customer's venue, which `SupportAccessGrants` makes a real case.

**11 · Hint and authority are separate sources, always.** Every routing input the Router reads
is caller-asserted and unverified; it selects a version and nothing else. Every authorization
decision stays inside the versioned application, derived from a durable authority. Where the two
disagree the authority wins, the request is **not** served as though the hint were true, and the
disagreement is emitted as telemetry — a mismatch means the Router mis-routed, which makes this
the built-in detector for assignment drift during a rollout.

**12 · TenantContext is a cache, never the authority.** A cookie, a storage entry or a path
segment holding a tenant is an optimisation. If it is lost the tenant must still be re-derivable
from something durable. Nothing may become unroutable because a browser was cleared.

**13 · The public contract is the URL path.** A bundle served from `/o/{orgId}/v/{venueId}/`
making a relative call has the tenant resolved into it by the browser, so the tenant rides along
with no client code and nothing can forget to send it. The TenantContext *header* considered
earlier is retired.

**14 · The Router consumes the tenant prefix and forwards the bare path.** API route templates
are therefore unchanged: `BackOfficeMenusController` stays at `api/back-office/menus`. The
tenant travels onward in the internal token, so no versioned application ever parses a tenant
out of a URL.

**15 · The Router forwards; it never hands out per-version hostnames.** The `__Host-` prefix on
`__Host-Vennusign.CustomerSession` forbids a `Domain` attribute, pinning the cookie to the exact
host that set it, so a per-version hostname would destroy the session at every cutover. This
settles the concept's open question between one hop per request and a resolved-once-per-session
endpoint, in favour of same-host forwarding.

**16 · One extraction rule, never coupled to a versioned route shape.** The Router must not
pattern-match a versioned API route — route shapes belong to a version, and the thin layer
cannot be coupled to something that changes underneath it.

**17 · Self-correction is mandatory, not a fallback.** A path can be stale, a cache can be
cleared, a first visit has neither, so there is no arrangement in which the Router always knows.
Every client boots, learns its true tenant from its first authenticated call, compares against
the version that served it, and reloads on disagreement.

**18 · The wire format is additive-only, permanently.** Keystone cannot roll out progressively
and an older live version cannot be retro-updated to understand a change, so nothing may ever be
removed or reinterpreted — only added, in ways an old parser safely ignores.

---

## The URL shape

**19 · The URL shape is fixed as follows.**

```
/signin  /signup  /onboarding          pre-auth · no tenant · root
/pair                                  pre-auth · device seeking an owner
/o/{orgId}/v/{venueId}#/menu           post-auth · tenant in path, app route in hash
/display/{venueId}/{screenId}          post-claim · device
```

**20 · The pre-auth/post-auth line is visible in the URL.** Which side of the line a request
sits on is determinable by inspection — by a person or by the Router — with no lookup and no
session resolution. This is the property that makes rule 5 operable rather than merely true.

**21 · Application navigation stays in the hash.** Back office has no router library: all
in-app navigation is hash-based and the pathname is unused for it. The hash is never sent to the
server, so the pathname is Keystone's and the hash is the application's, and the two cannot
collide.

**49 · A wrong org segment never reveals the right one.** *(Owner, 2026-08-20, overriding the
register's own recommendation at Q19.)* The Router ignores the org segment and routes on venue,
per rule 25 — but what the application does next runs through the ordinary security-role and
authentication path, and **the response must never hint that the venue might belong to another
organization.** URL correction is permitted only *after* authorization succeeds: a caller
entitled to the venue may have a stale link tidied. An unauthorized caller receives the
identical refusal they would receive for a venue that does not exist, disclosing neither its
existence nor its owner.

---

## Assignment

**22 · Every venue belongs to an organization.** Single-venue operators have an organization of
one; multi-venue is an organization with more than one venue. The org-less venue is retired as a
supported shape, which is what makes rule 19's path always well-formed.

**23 · Assignment is per venue.** Maintenance windows are about service hours at a physical
place, and `Venue.Timezone` already models that per venue, so an organization spanning time
zones cannot share one window.

**24 · Scheduling is per organization.** A wave groups an organization's venues together: they
enter a rollout as a unit and each moves at its own local window. The organization crosses over
one night rather than in one instant, and never straddles versions for days.

**25 · The Router only ever keys on venue.** Organizations are a reporting umbrella and a
scheduler helper, not a rollout unit. **No organization is ever assigned a version.** The org
segment exists in the URL for the application — scoping data, multi-venue functions, reporting —
and is not a routing input. A session always has a current venue, so org-scoped surfaces are
served by that venue's version.

**26 · Assignment is never derived from subscription shape.** Subscriptions may sit at venue
level, organization level or both, and may be merged, moved and adjusted. They are an
independent axis: a billing change must never be capable of silently moving a customer to a
different version.

**27 · The default version serves unattributed traffic, and "default" is an explicit pointer
Platform Operations sets.** It is not "newest registered". Registration is a fact about what
exists and assignment is the decision that affects customers; if registering a version handed it
every sign-in, a version with zero assigned customers could lock out everybody, including
customers safely on an older version. The pointer advances as a deliberate act, normally once a
first wave is healthy, so unattributed traffic follows customers onto a version rather than
leading them onto it.

**28 · New customers start on the default version.** They have never been assigned anything, and
onboarding is pre-auth, so they are created by whatever serves unattributed traffic and assigned
there.

---

## Screens

**29 · Claim moves a screen immediately, and it is not a choice.** Claiming forces a navigation
from `/pair` to `/display/{venueId}/{screenId}` — a different path, therefore a document load,
therefore routed. The move falls out of rule 19 rather than needing a mechanism.

**30 · Reassignment moves a screen at reconnect, and the client forces the reconnect.** Version-
scoped groups mean a screen changes version at reconnect rather than mid-session, so a cutover
does not sever an active display session. But the player periodically recovers content over HTTP,
which routes to the new version while the socket is still held by the old one. So: if the version
answering my HTTP differs from the version holding my connection, drop and reconnect. This is
rule 17's signal applied to the socket as well as the bundle — one mechanism, two uses.

---

## Trust between components

**31 · Every internal hop carries a signed token.** Router to API, Webhook Receiver to API, and
onward. Network isolation alone was rejected.

**32 · Tokens are asymmetric, audience-scoped and short-lived.** The Router signs with a private
key and each version verifies with the public key, so a compromised API version cannot forge
Router tokens. The token names the version it was minted for, so one issued for v1.4 cannot be
replayed against v1.5. Note that the verification key is shared across versions; only the
audience claim is scoped.

**33 · A token records how the tenant was established, not only what it is.** The Webhook
Receiver verified a provider signature, so its venue is a verified fact; the Router resolved a
caller-asserted hint. Same envelope, different provenance, and the receiving version must be able
to tell which it got.

**34 · A token authenticates the hop, not the claim.** It proves the request came from the Router
and was not tampered with in transit. It does **not** convert a caller-asserted tenant into a
verified one. Authorization still comes from the authority, per rule 11. This is stated
explicitly because "it is signed, so we can trust it" is the reasoning that would otherwise erode
rule 11.

**35 · A misdirected request is refused, never served.** Two causes are kept apart: the Router
resolved one version but the request arrived at another (stale ADS entry, pool drift), caught by
comparing the Router's stamped version against the receiving instance's own
`VENNU_COMPONENT_VERSION`; and a false premise, where the hint named one venue and the authority
names another, caught where the authority is already loaded. Both answer **421 Misdirected
Request** for API calls and **307** to the correct URL for document navigations. Serving anyway
and logging is not an option: it is the mismatch the feature exists to prevent.

---

## Secrets across versions

**36 · Version-scoped for transient, version-agnostic for durable.** Anything living seconds may
be bound to a version, and binding it is a security property. Anything outliving a version must
not be, and not binding it is a correctness property.

**37 · Durable secrets do not use ASP.NET Data Protection.** POS credentials and strong-auth
factors move to explicit Key Vault envelope encryption, readable by any version by construction,
and rotatable and auditable besides. Passkey challenges and POS OAuth state stay on Data
Protection: both are seconds-long, so a version move at worst fails one in-flight attempt that a
retry fixes. No shared key ring is required — the problem is removed rather than managed.

---

## Platform Operations

**38 · PO is not version-routed.** PO is an application plus its own API, deployed side by side.
One PO runs at a time. Its API is Vennusign-scoped: release state and the board, assignment
writes into VDS, operator identity and permissions, and the Vennu profile carrying maintenance
windows and cost KPIs. That data is about customers but is not product data, so no version serves
it.

**39 · Support access originates in PO and executes on the customer surface.** PO submits and
spawns the grant; the session it spawns runs through the normal, version-routed customer surface.
An operator helping a v1.4 customer therefore runs v1.4 themselves — a correctness property, not
tidiness, because a problem cannot be diagnosed from a console showing different code. The spawn
needs no new mechanism: PO sends the operator to `/o/{orgId}/v/{venueId}` and rule 25 does the
rest.

---

## Hosting

**40 · Separate App Services, not one bundled app.** A Webhook Receiver change must not be able
to break routing. One process re-couples exactly what the design separates, and since immediate
rollback is the only recovery path, a single bundle means rolling back all three to fix one.

**41 · One shared App Service Plan for Keystone, separate from the versioned plan.** The
separation from the versioned plan is the boundary that matters: concurrently running versions
divide their plan's CPU and memory, so sharing would let starting v1.6 alongside v1.5 starve the
Product Router — which sits on every request and whose latency is paid on every call. A plan each
for Keystone's own components buys isolation whose need cannot yet be measured.

**42 · Standard tier.** Deployment slots are Standard-and-up, and slot swap with instant
swap-back is the stated recovery model for components that cannot roll out progressively.

**43 · Product Router graduates to its own plan later** — when plan-aggregate CPU or its added
p99 becomes measurable, and not before.

**Deferred, deliberately:** tier and plan cost. Standard is a tier change and the owner has not
accepted that cost. Nothing is provisioned until that conversation happens.

---

## Delivery order

**44 · Slice 1 is the TenantContext contract and its library.** It lands in the existing API and
front ends with no Keystone infrastructure at all, is backward-compatible by construction, is
independently mergeable and harmless if wrong, and it makes VDS's lookup signature a fact rather
than a guess.

**45 · Slice 2 is VDS + ADS.** Bootstrap order requires it: nothing routes until VDS answers, and
VDS cannot answer usefully until ADS knows where healthy instances are. By slice 2 its contract
is known.

---

## Prerequisites owned outside Keystone

These are not Keystone's to build, and Keystone cannot ship without them.

**46 · `VENNU_COMPONENT_VERSION` must be set by deployment.** `ReleaseVersionMetadata` reads it
with a fallback of `"0.0.0-local"` and nothing sets it, so every instance reports the placeholder.
Rule 35's mis-forwarding check and the concept's assignment-aware background services both compare
against this value. Tracked as **#726**.

**47 · The deployment pipeline must be able to produce a new per-version target.** Deployment is
fully automated for the apps that exist, but names its targets directly, so it cannot stand up a
new version. This is a shape change to an automated pipeline, not the introduction of automation.

**48 · Signing in should establish an organization, not a venue**, and **onboarding should be its
own app.** Both follow from the pre-auth/post-auth line applied to the product rather than only
to routing, and both belong to the back-office/authentication area rather than to an
infrastructure feature. Recorded as strong recommendations, deliberately not settled here.

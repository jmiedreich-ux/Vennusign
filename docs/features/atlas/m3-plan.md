# Atlas Milestone 3 — Write-back

> **This is a record of work that shipped**, not a proposal. It is written after the fact so the
> site can render what happened; the work was merged and tagged on 2026-08-22.

**Goal:** Atlas can be answered from, not only read.

**Where it landed:** the `Atlas` repository, as PR jmiedreich-ux/Atlas#3, tagged **`v1.2.0`**, 479
tests on Node 22 Linux, no network in the test path and zero dependencies in the Function.
Vennusign's side of it is PR #783, which deployed the Functions and stopped the gate step discarding
their route rule.

**Spec:** `docs/design/approved/atlas/decisions.md`, decisions 34–37, which settle more than this
record restates and win where they differ.

**Numbering note.** Decision 34 says Atlas is read-only "in Milestone 1" and decision 35 assigns
writes to "Milestone 2". That was an earlier numbering: M2 became Vennusign's adoption and
write-back became M3. The decisions' intent is unchanged — this is the milestone they mean.

---

## The scope, because two of the decisions are narrower than they sound

**Decision 35 — only two things are writable: register answers and acceptance results.**

- `POST /api/answer` — records an answer to a question in a register.
- `POST /api/acceptance` — records an acceptance result.
- Nothing else.

Creating issues, approving milestones, editing manifests and triggering work belong to Platform
Operations, because *two consoles that both act is how they diverge*. A status dropdown on every
milestone was not in scope and neither was editing a manifest. Widening this is the owner's call,
not an implementer's.

**Decision 36 — writes go through a GitHub App, never `GITHUB_TOKEN`.** The stated reason is not
security, it is mechanical: a push made with the Actions token **does not trigger workflows**, so
the site would never rebuild after its own write and would sit stale, showing the answer it had just
failed to render.

**Decision 37 — a write lands as a commit to the record, and the page is rebuilt from it.** An
answer submitted on a phone becomes a commit to `open-questions.md`; the page reloaded afterwards is
rendered from that file. Atlas keeps no state of its own — no database, no cache, no queue, no
"pending" list, no copy of the answer anywhere but the repository.

---

## What shipped

**Azure Static Web Apps managed Functions**, in the same deployable and behind the same auth as the
site. That is the whole reason decision 5 chose Static Web Apps over an App Service, and the Free
tier includes them.

**Identity is the caller's, and it is separate from the ability to read.** The caller is the SWA
principal already gating the site — the `reader` role, by invitation. A write needs a role of its
own, `author`, so that being able to read is not being able to write. The principal is read from the
`x-ms-client-principal` header; no body field is ever trusted for identity.

**The committer is a GitHub App installation.** Its credentials live in the Static Web App's
application settings, never in the repository, never in a build artifact and never in `state.json`.

**A write, end to end.** Authorise the caller and validate the payload against the same closed
vocabularies `src/schema.mjs` already holds, rejecting an unknown value by name (decision 32). Read
the target record's current content and SHA through the GitHub contents API. Apply the change
minimally to the text of the record, without reformatting the surrounding file. `PUT` it back with
the SHA, which both commits and gives optimistic concurrency for free — **a stale SHA is a 409, and
the caller is told so rather than overwriting someone.** Return the commit URL; the rebuild is the
workflow's job, triggered by the push.

**It is inert until the owner configures it.** The whole path was built against a credential slot
that may be empty, and when it is empty the endpoints return a clear **503 naming the unset
settings** while the site stays fully readable. Nothing is broken while the owner has not yet
supplied the credential.

---

## What review found, and what it cost

**C1 · A demonstrated stored XSS, closed by refusing markup outright.** The write path rejected
headings, control characters, over-length text and Atlas's own markers — and not HTML.
`src/markdown.mjs` sets `html: true` deliberately (decision 11: the corpus's `<sub>` citations must
survive) and the site carries no CSP, so review posted
`<img src=x onerror=...><script>fetch(...document.cookie)</script>` through the real handler with a
valid `author` principal and got it back out of the built site verbatim.

Why it was M3's to fix rather than pre-existing: before this milestone, putting bytes into a record
required GitHub write access — people who could already do anything. M3 gives it to anyone holding
the Static Web Apps `author` role, which is a portal invitation and not GitHub access. The injected
script then runs for every reader, same-origin with `/api/answer` and `/api/acceptance`, carrying
that viewer's session — so it can drive further commits through these very endpoints and read the
whole internal corpus.

The fix refuses `<` followed by anything tag-ish, in the answer and in the acceptance note, **flatly
rather than by sanitising**: the alternative is an allow-list of safe tags and attributes that
somebody has to keep current forever, and an answer is typed into a form rather than authored as a
document. `3 < 4` still writes fine and `&lt;sub&gt;` is the way to write about a tag — the
character is not banned, markup is. The corpus's own inline HTML is untouched, because none of it is
written through here.

Verified by posting 46 payloads through the real handler, building the site and grepping the output
— including `javascript:` URLs, which need no `<` at all and are refused by markdown-it's own link
validator. A companion test pins the *reason* the rule exists, so that if `html: true` ever goes
away a test says so rather than the rule quietly outliving its purpose.

**I2 · Setext headings walked through the heading check.** It was ATX-only, so
`"Injected Heading\n================"` and `"Injected\n---"` passed and rendered as headings inside
somebody else's question. A setext underline is now refused when there is a non-blank line above it
to underline, so a thematic break and a table's delimiter row — which is what `---` usually is in an
answer — still write.

**I4 · `placeBlock` could delete a whole section.** It took the *last* closing marker in range
rather than the first after the open, so a record holding a stray `<!-- /atlas:acceptance -->`
further down lost everything between the two on the next write. A block is the span between one open
and its own close; every later marker is somebody else's text.

**Also found and closed:** an `acceptance.record` could name any in-repo path; validation trimmed a
path the handler then used untrimmed; and nothing was pinning the repository a request could reach.

Each fix was proved by breaking it again and watching the named tests go red.

---

## Vennusign's side, which had two faults of its own

Both were found after v1.2.0 and closed in PR #783.

1. **The deploy passed no `api_location`,** so the Functions were never deployed at all and
   `POST /api/answer` returned 404 rather than 401.
2. **The gate step replaced the emitted `staticwebapp.config.json` wholesale,** discarding the
   `/api/*` rule requiring the `author` role and the Functions runtime declaration along with it. It
   now merges: this project overrides only the login provider — it signs in with GitHub while the
   generator's default is Microsoft — and keeps the generator's API rule and platform block. The
   `/.auth/*` routes stay first, because a catch-all ahead of them is a login redirect loop.

---

## Deliberately excluded

- **Any third write path.** Decision 35, and it is the scope rather than a starting point.
- **A UI to post from.** There is no form on the site yet; the surface is a follow-up.
- **State outside the repository.** No cache, no queue, no pending list. Decision 37.
- **A write that can reach a repository other than the configured one.**

## Left for the owner

Creating and installing the GitHub App, supplying its credentials as application settings, and
granting himself the `author` role. Until he does, the endpoints answer 503 naming the unset
settings and the site is unaffected.

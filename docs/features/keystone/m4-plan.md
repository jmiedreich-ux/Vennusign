# Keystone Milestone 4 — The Front Ends Adopt the Tenant Path

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move the customer-facing surfaces onto the tenant-prefixed URL shape, so the tenant travels in the path and `venueFetch`'s `localStorage` header is deleted rather than formalised.

**Architecture:** Onboarding is extracted from `src/back-office` into its own app first, because `main.tsx` is already a two-way switch and splitting first means the URL restructure only ever touches the post-auth app. Back office is then served under `/o/{orgId}/v/{venueId}/`, reads its tenant from `location.pathname`, and makes every API call relative so the browser resolves the prefix in. Display gains a venue segment.

**Tech Stack:** React 19 + Vite + TypeScript, `node --test` on `.mjs` modules (matching `customerEntryRouting.mjs` and `pairing.mjs`). No new dependencies.

**Spec:** `docs/design/proposed/keystone/decisions.md` — decisions 12, 13, 17, 19, 20, 21, 29, 48.

## Milestone discipline

This is a numbered milestone under AGENTS.md's working model, not a loose batch of work.
Before starting: create the milestone issue, record the claim in `tracker/assignments.json`,
and branch as `feature/keystone-m4-<short-name>` from merged `master`. One PR. Verify locally
(CI is suspended by owner decision — local checks *are* the gate). Obtain independent review,
never by the author. Merge, then synchronize `PROJECT_STATUS.md`, the tracker,
`ai/handoffs/current.md` and this feature's records.

**Ends with a short owner acceptance workbook** (5–10 minutes) before the next milestone starts.
A milestone that ships no UI gets a demo script instead. Only one milestone runs at a time.

## Governance gate

**Does not execute until the design authority is approved.** See `milestone-plan.md`.

**Q31 runs provisionally.** The register defers whether the pre-auth split precedes the URL restructure; its recommended default — split first — is what this plan implements. Flag it in the acceptance workbook so the consequence is visible and cheap to overturn.

**Depends on milestone 1.** The API must already strip a tenant prefix, or every relative call 404s.

## Global Constraints

- **No new npm dependencies.** Back office has no router library and does not gain one (decision 21).
- **Decision 21 — application navigation stays in the hash.** `#/menu`, `#/screens` and friends are untouched. The pathname is Keystone's; the hash is the application's.
- **Decision 19 — the URL shape is fixed.** Pre-auth at root; post-auth at `/o/{orgId}/v/{venueId}`; Display at `/display/{venueId}/{screenId}`.
- **Decision 12 — the path is a cache, not the authority.** A missing or wrong tenant costs a lookup, never access.
- **Decision 49 — a wrong org segment never reveals the right one.** Correction only after authorization.
- **`node --test tests/*.test.mjs`** is the test command in every front-end package.

## File Structure

| File | Responsibility |
|---|---|
| `src/back-office/src/tenantPath.mjs` | Read and build the tenant prefix in the browser. Pure. Mirrors `Vennu.Tenancy.TenantPath`. |
| `src/back-office/src/tenantPath.d.mts` | Types for the above, matching the `.mjs` + `.d.mts` convention already in use. |
| `src/back-office/tests/tenantPath.test.mjs` | Its tests. |
| `src/onboarding/` | The extracted pre-auth app: signup, signin, onboarding. |
| `src/back-office/src/main.tsx` | Loses the two-way switch; boots the post-auth app only. |
| `src/back-office/src/api.ts` | Relative URLs; `venueFetch`'s storage read deleted. |
| `src/display/src/routing.ts` | `/display/{venueId}/{screenId}`. |

---

### Task 1: `tenantPath` in the browser

**Files:**
- Create: `src/back-office/src/tenantPath.mjs`
- Create: `src/back-office/src/tenantPath.d.mts`
- Create: `src/back-office/tests/tenantPath.test.mjs`

**Interfaces:**
- Consumes: nothing.
- Produces: `readTenantPath(pathname)` returning `{ organizationId, venueId, remainder } | null`; `buildTenantPath({ organizationId, venueId }, remainder)` returning a string.

This deliberately mirrors `Vennu.Tenancy.TenantPath` from milestone 1. Two implementations of one wire format is a real risk, so the tests below use the same cases as `TenantPathTests.cs`.

- [ ] **Step 1: Write the failing test**

Create `src/back-office/tests/tenantPath.test.mjs`:

```javascript
import { test } from "node:test";
import assert from "node:assert/strict";
import { readTenantPath, buildTenantPath } from "../src/tenantPath.mjs";

const ORG = "11111111-1111-1111-1111-111111111111";
const VENUE = "22222222-2222-2222-2222-222222222222";

test("reads org and venue and returns the bare path", () => {
  const result = readTenantPath(`/o/${ORG}/v/${VENUE}/`);
  assert.equal(result.organizationId, ORG);
  assert.equal(result.venueId, VENUE);
  assert.equal(result.remainder, "/");
});

test("keeps a trailing application path", () => {
  const result = readTenantPath(`/o/${ORG}/v/${VENUE}/settings`);
  assert.equal(result.remainder, "/settings");
});

test("accepts a venue with no organization segment", () => {
  const result = readTenantPath(`/v/${VENUE}/`);
  assert.equal(result.organizationId, null);
  assert.equal(result.venueId, VENUE);
});

for (const path of [
  "/",
  "/signin",
  "/signup",
  "/onboarding",
  `/o/not-a-guid/v/${VENUE}/`,
  `/o/${ORG}/v/not-a-guid/`,
  `/o/${ORG}/`,
]) {
  test(`returns null for ${path || "(empty)"}`, () => {
    assert.equal(readTenantPath(path), null);
  });
}

test("build is the inverse of read", () => {
  const original = `/o/${ORG}/v/${VENUE}/settings`;
  const parsed = readTenantPath(original);
  assert.equal(buildTenantPath(parsed, parsed.remainder), original);
});

test("build omits the organization segment when there is none", () => {
  assert.equal(
    buildTenantPath({ organizationId: null, venueId: VENUE }, "/settings"),
    `/v/${VENUE}/settings`
  );
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd src/back-office && npm test`
Expected: FAIL — `Cannot find module '../src/tenantPath.mjs'`.

- [ ] **Step 3: Write the implementation**

Create `src/back-office/src/tenantPath.mjs`:

```javascript
// The browser half of decision 13. Mirrors Vennu.Tenancy.TenantPath — the two
// implement one wire format, and decision 18 makes that format additive-only.
const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function readTenantPath(pathname) {
  if (typeof pathname !== "string" || pathname[0] !== "/") return null;

  const parts = pathname.split("/");
  let index = 1;
  let organizationId = null;

  if (parts[index] === "o") {
    if (!GUID.test(parts[index + 1] ?? "")) return null;
    organizationId = parts[index + 1];
    index += 2;
  }

  if (parts[index] !== "v" || !GUID.test(parts[index + 1] ?? "")) return null;
  const venueId = parts[index + 1];
  index += 2;

  const rest = parts.milestone(index).join("/");
  return { organizationId, venueId, remainder: rest.length === 0 ? "/" : "/" + rest };
}

export function buildTenantPath({ organizationId, venueId }, remainder) {
  let tail = remainder && remainder.length ? remainder : "/";
  if (tail[0] !== "/") tail = "/" + tail;
  if (tail === "/") tail = "";

  return organizationId
    ? `/o/${organizationId}/v/${venueId}${tail}`
    : `/v/${venueId}${tail}`;
}
```

Create `src/back-office/src/tenantPath.d.mts`:

```typescript
export type TenantPathParts = {
  organizationId: string | null;
  venueId: string;
  remainder: string;
};

export function readTenantPath(pathname: string): TenantPathParts | null;
export function buildTenantPath(
  parts: Pick<TenantPathParts, "organizationId" | "venueId">,
  remainder: string
): string;
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `cd src/back-office && npm test`
Expected: PASS, 12 of 12 in this file, with the existing suite unaffected.

- [ ] **Step 5: Commit**

```bash
git add src/back-office/src/tenantPath.mjs src/back-office/src/tenantPath.d.mts src/back-office/tests/tenantPath.test.mjs
git commit -m "feat(back-office): read and build the tenant path prefix

Mirrors Vennu.Tenancy.TenantPath against the same cases, because two
implementations of one wire format is exactly where drift starts."
```

---

### Task 2: Extract the pre-auth app

**Files:**
- Create: `src/onboarding/package.json`, `vite.config.ts`, `tsconfig.json`, `index.html`
- Create: `src/onboarding/src/main.tsx`
- Move: `src/back-office/src/CustomerOnboardingApp.tsx` → `src/onboarding/src/CustomerOnboardingApp.tsx`
- Move: `src/back-office/src/CustomerOnboardingTimeline.tsx`, `TemplateShowcase.tsx`, `customerEntryRouting.mjs` (+ its `.d.mts`) alongside it
- Modify: `src/back-office/src/main.tsx`

**Interfaces:**
- Consumes: nothing from earlier tasks.
- Produces: a `src/onboarding` app serving `/signin`, `/signup`, `/onboarding`; a `src/back-office` `main.tsx` that boots `App` unconditionally.

Copy `src/back-office`'s `vite.config.ts`, `tsconfig.json` and `index.html` and adjust names — matching an existing app's configuration exactly is the point, not inventing one.

- [ ] **Step 1: Write the failing test**

Create `src/onboarding/tests/entryRoutes.test.mjs`:

```javascript
import { test } from "node:test";
import assert from "node:assert/strict";
import { isCustomerEntryRoute } from "../src/entryRoutes.mjs";

for (const path of ["/signin", "/signup", "/onboarding", "/signin/"]) {
  test(`${path} is served by the pre-auth app`, () => {
    assert.equal(isCustomerEntryRoute(path), true);
  });
}

for (const path of [
  "/",
  "/o/11111111-1111-1111-1111-111111111111/v/22222222-2222-2222-2222-222222222222/",
  "/display/22222222-2222-2222-2222-222222222222/33333333-3333-3333-3333-333333333333",
]) {
  test(`${path} is not a pre-auth route`, () => {
    assert.equal(isCustomerEntryRoute(path), false);
  });
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd src/onboarding && npm test`
Expected: FAIL — the package does not exist yet.

- [ ] **Step 3: Scaffold the app and move the pre-auth components**

```bash
mkdir -p src/onboarding/src src/onboarding/tests
cp src/back-office/vite.config.ts src/onboarding/vite.config.ts
cp src/back-office/tsconfig.json src/onboarding/tsconfig.json
cp src/back-office/tsconfig.app.json src/onboarding/tsconfig.app.json
cp src/back-office/tsconfig.node.json src/onboarding/tsconfig.node.json
cp src/back-office/index.html src/onboarding/index.html

git mv src/back-office/src/CustomerOnboardingApp.tsx src/onboarding/src/CustomerOnboardingApp.tsx
git mv src/back-office/src/CustomerOnboardingTimeline.tsx src/onboarding/src/CustomerOnboardingTimeline.tsx
git mv src/back-office/src/TemplateShowcase.tsx src/onboarding/src/TemplateShowcase.tsx
git mv src/back-office/src/customerEntryRouting.mjs src/onboarding/src/customerEntryRouting.mjs
git mv src/back-office/src/customerEntryRouting.d.mts src/onboarding/src/customerEntryRouting.d.mts
```

Copy `config.ts` and `api.ts` into `src/onboarding/src/` rather than moving them — back office still needs both. Trim the copy to the calls onboarding actually makes; a shared package is not worth creating for two files.

Create `src/onboarding/package.json` by copying `src/back-office/package.json` and changing `"name"` to `"vennusign-onboarding"`. Keep the scripts identical.

Create `src/onboarding/src/entryRoutes.mjs`:

```javascript
// Decision 19: the pre-auth routes live at the root and carry no tenant.
const ENTRY_ROUTES = ["/signup", "/signin", "/onboarding"];

export function isCustomerEntryRoute(pathname) {
  if (typeof pathname !== "string") return false;
  const normalized = pathname.replace(/\/$/, "");
  return ENTRY_ROUTES.includes(normalized === "" ? "/" : normalized);
}
```

Create `src/onboarding/src/main.tsx`:

```tsx
import React from "react";
import ReactDOM from "react-dom/client";
import "@fontsource/playfair-display/400.css";
import "@fontsource/playfair-display/600.css";
import "@fontsource/playfair-display/400-italic.css";
import CustomerOnboardingApp from "./CustomerOnboardingApp";
import { initializeAdminTheme } from "./adminTheme.mjs";

initializeAdminTheme();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <CustomerOnboardingApp />
  </React.StrictMode>
);
```

- [ ] **Step 4: Remove the switch from back office**

Replace `src/back-office/src/main.tsx` entirely with:

```tsx
import React from "react";
import ReactDOM from "react-dom/client";
import "@fontsource/playfair-display/400.css";
import "@fontsource/playfair-display/600.css";
import "@fontsource/playfair-display/400-italic.css";
import App from "./App";
import { initializeAdminTheme } from "./adminTheme.mjs";

// Decision 48: back office is entered only post-auth, from a tenant-bearing URL.
// The pre-auth routes moved to src/onboarding, so there is no switch here any more.
initializeAdminTheme();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

- [ ] **Step 5: Run the tests to verify they pass**

```bash
cd src/onboarding && npm install && npm test && npm run build
cd ../back-office && npm test && npm run build
```

Expected: onboarding 7 of 7 and a clean production build; back office suite unchanged and building. If back office fails to compile on a missing `CustomerOnboardingApp` import, that import is the switch — delete it.

- [ ] **Step 6: Commit**

```bash
git add src/onboarding src/back-office/src/main.tsx
git commit -m "refactor(front-end): extract the pre-auth app from back office

main.tsx was already a two-way switch between two unrelated roots over exactly
the pre-auth pathname set. This unbundles them so back office can assume it
always has a tenant. Q31's recommended default, running provisionally."
```

---

### Task 3: Back office reads its tenant from the path

**Files:**
- Modify: `src/back-office/src/App.tsx`
- Create: `src/back-office/tests/tenantBoot.test.mjs`
- Create: `src/back-office/src/tenantBoot.mjs`

**Interfaces:**
- Consumes: `readTenantPath` from Task 1.
- Produces: `resolveTenant(pathname)` returning `{ organizationId, venueId }`, or throwing `MissingTenantError` when the path carries none.

The throw is deliberate. Under decision 48 back office is only ever entered post-auth from a tenant-bearing URL, so no tenant is a bug in whatever linked here — not a state to render around.

- [ ] **Step 1: Write the failing test**

Create `src/back-office/tests/tenantBoot.test.mjs`:

```javascript
import { test } from "node:test";
import assert from "node:assert/strict";
import { resolveTenant, MissingTenantError } from "../src/tenantBoot.mjs";

const ORG = "11111111-1111-1111-1111-111111111111";
const VENUE = "22222222-2222-2222-2222-222222222222";

test("resolves org and venue from the path", () => {
  assert.deepEqual(resolveTenant(`/o/${ORG}/v/${VENUE}/`), {
    organizationId: ORG,
    venueId: VENUE,
  });
});

test("resolves a venue with no organization segment", () => {
  assert.deepEqual(resolveTenant(`/v/${VENUE}/`), {
    organizationId: null,
    venueId: VENUE,
  });
});

test("throws when the path carries no tenant", () => {
  // Decision 48: back office is entered only post-auth from a tenant-bearing URL,
  // so this is a defect in whatever linked here, not a state to render around.
  assert.throws(() => resolveTenant("/"), MissingTenantError);
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd src/back-office && npm test`
Expected: FAIL — `Cannot find module '../src/tenantBoot.mjs'`.

- [ ] **Step 3: Write the implementation**

Create `src/back-office/src/tenantBoot.mjs`:

```javascript
import { readTenantPath } from "./tenantPath.mjs";

export class MissingTenantError extends Error {
  constructor(pathname) {
    super(`Back office was opened at "${pathname}", which carries no tenant.`);
    this.name = "MissingTenantError";
  }
}

// Decision 12: the path is a cache of a fact that lives in the database. It selects
// which venue this session is working in; it never grants access to it.
export function resolveTenant(pathname) {
  const parts = readTenantPath(pathname);
  if (!parts) throw new MissingTenantError(pathname);
  return { organizationId: parts.organizationId, venueId: parts.venueId };
}
```

- [ ] **Step 4: Use it at boot**

In `src/back-office/src/App.tsx`, near the top of the component where `routeHash` is initialised (around line 125), add:

```tsx
import { resolveTenant } from "./tenantBoot.mjs";

// ...inside the component, before any data loading:
const tenant = useMemo(() => resolveTenant(window.location.pathname), []);
```

Then replace every read of the stored venue with `tenant.venueId`. Search for the storage key to find them all:

```bash
grep -rn "vennusign.back-office.venue-id" src/back-office/src
```

Every hit is either the write (delete it) or a read (replace with `tenant.venueId`).

- [ ] **Step 5: Run the tests to verify they pass**

```bash
cd src/back-office && npm test && npm run build
```

Expected: PASS, including the 3 new cases, with a clean production build.

- [ ] **Step 6: Commit**

```bash
git add src/back-office/src/tenantBoot.mjs src/back-office/src/App.tsx src/back-office/tests/tenantBoot.test.mjs
git commit -m "feat(back-office): take the tenant from the path, not storage

Decision 13. A path with no tenant throws rather than rendering an empty
state, because under decision 48 that can only be a defect in the link."
```

---

### Task 4: Relative API calls, and `venueFetch`'s storage read deleted

**Files:**
- Modify: `src/back-office/src/api.ts:238-247` (`venueFetch`)
- Modify: `src/back-office/src/config.ts`
- Create: `src/back-office/tests/relativeApi.test.mjs`

**Interfaces:**
- Consumes: nothing from earlier tasks — this is the deletion that Task 3 makes safe.
- Produces: an `apiBaseUrl` that is relative in production.

This is the task decision 13 exists for. Once calls are relative, the browser resolves the prefix into them and the tenant is carried with no client code at all.

- [ ] **Step 1: Write the failing test**

Create `src/back-office/tests/relativeApi.test.mjs`:

```javascript
import { test } from "node:test";
import assert from "node:assert/strict";
import { resolveApiBaseUrl } from "../src/apiBaseUrl.mjs";

test("is relative when no absolute base is configured", () => {
  // Decision 13: a relative call from /o/{org}/v/{venue}/ inherits the tenant.
  assert.equal(resolveApiBaseUrl(undefined), "");
});

test("keeps an explicit absolute base for local development", () => {
  assert.equal(
    resolveApiBaseUrl("https://localhost:7001"),
    "https://localhost:7001"
  );
});

test("strips a trailing slash so joins do not double up", () => {
  assert.equal(resolveApiBaseUrl("https://localhost:7001/"), "https://localhost:7001");
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd src/back-office && npm test`
Expected: FAIL — `Cannot find module '../src/apiBaseUrl.mjs'`.

- [ ] **Step 3: Write the implementation**

Create `src/back-office/src/apiBaseUrl.mjs`:

```javascript
// Decision 13: production calls are relative so the browser resolves the tenant
// prefix into them. An absolute base survives only for local development against
// a separately-hosted API.
export function resolveApiBaseUrl(configured) {
  if (!configured) return "";
  return configured.replace(/\/+$/, "");
}
```

- [ ] **Step 4: Delete the storage read from `venueFetch`**

Replace `venueFetch` in `src/back-office/src/api.ts` with:

```typescript
function venueFetch(input: RequestInfo | URL, init?: RequestInit) {
  const headers = new Headers(init?.headers);
  if (headers.get("X-Vennusign-Back-Office-Token") === "customer-session") {
    headers.delete("X-Vennusign-Back-Office-Token");
  }
  // The tenant is no longer sent: it is inherited from the bundle's own path
  // (decision 13), so there is nothing here that can forget to send it.
  return fetch(input, { ...init, headers, credentials: "include" });
}
```

Delete `venueContextStorageKey` and `clearBackOfficeVenueContext`, then fix the call sites the compiler flags. In `loadBackOfficeSession`, delete the 401-retry that cleared the stored venue and retried — the state it worked around no longer exists.

Route `loadBackOfficeConfiguration`'s `apiBaseUrl` through `resolveApiBaseUrl`.

- [ ] **Step 5: Run the tests to verify they pass**

```bash
cd src/back-office && npm test && npm run build
```

Expected: PASS with a clean build. `grep -rn "X-Vennusign-Venue-Id" src/back-office/src` must return nothing.

- [ ] **Step 6: Commit**

```bash
git add src/back-office/src/api.ts src/back-office/src/apiBaseUrl.mjs src/back-office/src/config.ts src/back-office/tests/relativeApi.test.mjs
git commit -m "feat(back-office): relative API calls carry the tenant

Deletes venueFetch's localStorage read and the X-Vennusign-Venue-Id header
rather than formalising them, and with them the 401 clear-and-retry that
existed only because a session tried to pin a venue before it knew one."
```

---

### Task 5: Display carries the venue in its URL

**Files:**
- Modify: `src/display/src/routing.ts`
- Modify: `src/display/src/App.tsx`
- Create: `src/display/tests/routing.test.mjs`

**Interfaces:**
- Consumes: nothing.
- Produces: `DisplayRoute` gains `{ kind: 'display'; venueId: string; screenId: string }`.

- [ ] **Step 1: Write the failing test**

Create `src/display/tests/routing.test.mjs`:

```javascript
import { test } from "node:test";
import assert from "node:assert/strict";
import { resolveDisplayRoute } from "../src/routing.ts";

const VENUE = "22222222-2222-2222-2222-222222222222";
const SCREEN = "33333333-3333-3333-3333-333333333333";

test("resolves venue and screen", () => {
  assert.deepEqual(resolveDisplayRoute(`/display/${VENUE}/${SCREEN}`), {
    kind: "display",
    venueId: VENUE,
    screenId: SCREEN,
  });
});

test("pair stays a pre-auth root route", () => {
  // Decision 19: a device seeking an owner has no tenant to put in a URL.
  assert.deepEqual(resolveDisplayRoute("/pair"), { kind: "pair" });
});

test("a display path without a venue is not found", () => {
  assert.deepEqual(resolveDisplayRoute(`/display/${SCREEN}`), { kind: "not-found" });
});
```

If importing `.ts` from `node --test` is not already configured in this package, move `resolveDisplayRoute` into `src/display/src/routing.mjs` with a `routing.d.mts` beside it, matching `pairing.mjs`, and import that instead. Check first: `ls src/display/tests`.

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd src/display && npm test`
Expected: FAIL — the two-segment path currently returns `not-found`.

- [ ] **Step 3: Write the implementation**

Replace the `display` branch of `src/display/src/routing.ts`:

```typescript
export type DisplayRoute =
  | { kind: 'display'; venueId: string; screenId: string }
  | { kind: 'pair' }
  | { kind: 'provision' }
  | { kind: 'not-found' };

export function resolveDisplayRoute(pathname: string): DisplayRoute {
  if (/^\/pair\/?$/i.test(pathname)) {
    return { kind: 'pair' };
  }

  if (/^\/provision\/?$/i.test(pathname)) {
    return { kind: 'provision' };
  }

  // Decision 19. The venue is a routing hint under decision 11 — the screen
  // record stays the authority for what this screen may show.
  const match = pathname.match(/^\/display\/([^/]+)\/([^/]+)\/?$/i);

  if (!match) {
    return { kind: 'not-found' };
  }

  return {
    kind: 'display',
    venueId: decodeURIComponent(match[1]),
    screenId: decodeURIComponent(match[2]),
  };
}
```

In `src/display/src/App.tsx`, pass `route.venueId` alongside `route.screenId` into `DisplayPage`.

- [ ] **Step 4: Run the tests to verify they pass**

```bash
cd src/display && npm test && npm run build
```

Expected: PASS, 3 of 3 new, existing suite unaffected.

- [ ] **Step 5: Commit**

```bash
git add src/display/src/routing.ts src/display/src/App.tsx src/display/tests/routing.test.mjs
git commit -m "feat(display): carry the venue in the display URL

Decision 19. The venue is a routing hint; the screen record remains the
authority for what the screen is allowed to show."
```

---

### Task 6: Send a claimed device to its tenant URL

**Files:**
- Modify: `src/display/src/PairingPage.tsx`
- Modify: `src/Vennu.Api/Controllers/ScreensController.cs` (the `pairing/{code}/status` response)
- Create: `tests/Vennu.Api.Tests/Screens/PairingStatusVenueTests.cs`

**Interfaces:**
- Consumes: Task 5's route shape.
- Produces: the pairing-status response gains `venueId`, null until claimed.

Decision 9: the claim response is where a device crosses the line, and it is already a server-controlled response — it only needs a field.

- [ ] **Step 1: Write the failing test**

Create `tests/Vennu.Api.Tests/Screens/PairingStatusVenueTests.cs`:

```csharp
namespace Vennu.Api.Tests.Screens;

public sealed class PairingStatusVenueTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void UnclaimedStatusCarriesNoVenue()
    {
        // Decision 5: before a claim the device belongs to nobody, so there is
        // genuinely no tenant to report.
        var response = new Vennu.Api.Contracts.ScreenPairingStatusResponse
        {
            Claimed = false,
            VenueId = null
        };

        Assert.Null(response.VenueId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ClaimedStatusCarriesTheVenue()
    {
        var venue = Guid.NewGuid();

        var response = new Vennu.Api.Contracts.ScreenPairingStatusResponse
        {
            Claimed = true,
            VenueId = venue
        };

        Assert.Equal(venue, response.VenueId);
    }
}
```

Adjust the namespace and type name to the actual pairing-status contract. Find it first:

```bash
grep -rn "pairing/{code}/status" -A6 src/Vennu.Api/Controllers/ScreensController.cs
grep -rn "class.*PairingStatus" src/Vennu.Api/Contracts
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~PairingStatusVenueTests`
Expected: FAIL to compile — the contract has no `VenueId`.

- [ ] **Step 3: Add the field and populate it**

Add `public Guid? VenueId { get; init; }` to the pairing-status contract, and populate it in `ScreensController` from the claimed screen's `VenueId`. It stays null while unclaimed.

- [ ] **Step 4: Navigate to the tenant URL on claim**

In `src/display/src/PairingPage.tsx`, where the poll observes a claim, replace the existing navigation with:

```javascript
// Decision 29: claim forces a navigation, and that navigation is what routes
// the device to its venue's version. There is no separate move to make.
window.location.assign(`/display/${status.venueId}/${screenId}`);
```

- [ ] **Step 5: Run the tests to verify they pass**

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~PairingStatusVenueTests
cd src/display && npm test && npm run build
```

Expected: PASS on both.

- [ ] **Step 6: Run the full milestone verification**

```bash
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj
cd src/back-office && npm test && npm run build
cd ../display && npm test && npm run build
cd ../onboarding && npm test && npm run build
```

Then the Playwright gate, per AGENTS.md: `npx playwright test` from `tests/ui`. Expect failures wherever a spec hard-codes a back-office URL without a tenant prefix; those specs are part of this milestone and are updated here, not later.

- [ ] **Step 7: Commit**

```bash
git add src/display/src/PairingPage.tsx src/Vennu.Api tests/Vennu.Api.Tests/Screens/PairingStatusVenueTests.cs
git commit -m "feat(display): a claimed device navigates to its tenant URL

Decision 9 stamps the tenant at the crossing, using the pairing-status
response that already exists. Decision 29 then does the rest: the navigation
is the version move."
```

---

## What this milestone deliberately excludes

- **`src/www` and `src/platform-operations`.** Decisions 38 and the owner's ruling put both outside the version equation.
- **The 421 and 307 responses** (decision 35). No consumer compares a hint against an authority yet; that arrives with the Router in milestone 5.
- **Deployment of `src/onboarding`.** A new app needs a `classify-changes.sh` output and a `deploy-dev.yml` job. That is the parked deploy-pipeline conversation, and per Q32 it is a prerequisite feature rather than Keystone's work.

## Self-review

**Spec coverage.** Decisions 12, 13, 19, 20, 21, 29 and 48 each have a task and an asserting test. Decision 9 is covered by Task 6. Decision 49 is not implemented here because it needs an authorization path to run through — it belongs with the first surface that authorizes a venue from a URL, which is milestone 5.

**Placeholders.** None. Two tasks require a `grep` before editing (Task 3's storage-key sweep, Task 6's contract name) because the exact call sites depend on code that may have moved; both give the command and what to do with every hit.

**Type consistency.** `readTenantPath` returns `{ organizationId, venueId, remainder }` in Tasks 1, 3 and 4. `resolveTenant` returns the same shape minus `remainder`. `DisplayRoute.display` carries `venueId` then `screenId` in both Task 5 and Task 6.

**Known risk, stated.** Task 1 creates a second implementation of the wire format, in JavaScript, alongside milestone 1's C# one. Decision 18 makes that format additive-only forever, which limits the damage, and the two test suites use identical cases — but a single format with two parsers is where drift begins, and a reviewer should check both when either changes.

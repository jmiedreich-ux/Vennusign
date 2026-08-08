# Menus M1 — Demo Script

Milestone 1 is the schema spine: it has no screens of its own, so it is accepted
through this demo rather than a workbook. Everything below is walked through the
API contract against seeded data. Roughly 10 minutes.

- **Milestone:** M1 — the spine (issue #684)
- **What it proves:** acceptance criteria 1, 2, 3 and 4
- **Base URL:** the running API, `/api/back-office/menu-spine`
- **Auth:** a back-office session for the seeded venue
- **Interactive workbook:** open `m1-demo-workbook.html` in a browser to run these checks and record
  Pass / Fail / Needs Adjustment against each one. It saves as you go and exports a JSON record.
  This Markdown file and the workbook carry the same checks; the workbook is the one to actually test with.

## Before you start

Migration `058_create_menu_item_library_spine.sql` runs on startup. It carries
existing menu content into the item library and marks each venue's active menus
as published so seeded data behaves like a real venue (Q3).

---

## 1. The venue's context — timezone and ceilings

```
GET /api/back-office/menu-spine/context
```

**Expect:** the venue's own timezone (not yours) and the four ceilings read from
the allowance model.

```json
{ "timezone": "America/New_York",
  "ceilings": { "content.menu.count": 50, "content.menu.items": 500,
                "content.menu.import.lines": 2000, "publishing.history.retention": 50 },
  "menuCount": 1 }
```

**Why it matters:** every Menus surface renders times in venue-local time (Q196),
and no ceiling is a constant — a tier can change any of them (Q201).

---

## 2. Criterion 1 — an 86 reaches the screens immediately, with no publish

```
PUT /api/back-office/menu-spine/items/{itemId}/availability
{ "isAvailable": false }
```

**Expect:** the response names the item, the time, who did it, and **every screen
currently showing that item through any menu**.

Then:

```
GET /api/back-office/menu-spine/menus/{menuId}/draft
```

**Expect:** `"count": 0`. The 86 changed the world without joining the queue.

**Also check:** re-read the availability list after a minute and it is still off.
There is no auto-reset — an 86 stays off until a person turns it back on (Q14-r2).

---

## 3. Criterion 4 — a queued change does not reach a screen

```
POST /api/back-office/menu-spine/menus/{menuId}/draft
{ "targetKind": "item", "targetId": "{itemId}", "field": "price",
  "beforeValue": "12", "afterValue": "13" }
```

**Expect:** `"count": 1`.

```
GET /api/back-office/menu-spine/menus/{menuId}/history
```

**Expect:** no new publish. Nothing has reached a screen.

---

## 4. The queue is the current diff, not a keystroke log

Send the same field again with a different value:

```
POST /api/back-office/menu-spine/menus/{menuId}/draft
{ "targetKind": "item", "targetId": "{itemId}", "field": "price",
  "beforeValue": "12", "afterValue": "14" }
```

**Expect:** still `"count": 1`, and `afterValue` is now `14`. The number you see
is always exactly what Publish will ship (Q182).

---

## 5. Criterion 2 — a publish ships this menu's queue and nothing else

Queue a change on a **second** menu first, then:

```
POST /api/back-office/menu-spine/menus/{menuId}/publish
```

**Expect:**
- `version` incremented, `changeCount` matching the queue you just saw;
- one target per assigned screen, `Pending` for online screens and `Offline` for
  the rest — an offline screen is not a failure, it catches up;
- the first menu's draft is now empty;
- the **second menu's draft is untouched**.

---

## 6. Criterion 3 — the 86 survived the publish

```
GET /api/back-office/menu-spine/availability
```

**Expect:** the item from step 2 is still off. Availability is a fact about
tonight, not about the menu.

---

## 7. "Go back to" produces a draft, never a silent publish

```
POST /api/back-office/menu-spine/menus/{menuId}/go-back-to/{version}
```

**Expect:** a draft with a change in it, and **no new publish** in history. Going
back is something you then publish deliberately — never a second silent path to
the screens.

---

## 8. The destructive acts are attributable

```
DELETE /api/back-office/menu-spine/menus/{menuId}/draft
DELETE /api/back-office/menu-spine/menus/{menuId}/screens
GET    /api/back-office/menu-spine/menus/{menuId}/history
```

**Expect:** history shows `draft_discarded` and `taken_off_screens`, each with the
person who did it and a plain detail line. Nothing irreversible is anonymous.

---

## Result

| # | Check | Result |
|---|---|---|
| 1 | Context shows venue timezone and configured ceilings | |
| 2 | An 86 is instant, reaches the right screens, and never queues | |
| 3 | A queued change reaches no screen | |
| 4 | Re-editing a field keeps the count at the current diff | |
| 5 | A publish ships only its own menu's queue, with honest per-screen state | |
| 6 | The 86 survived the publish | |
| 7 | "Go back to" produces a draft, not a publish | |
| 8 | Discard and take-off are recorded with their author | |

Record **Pass**, **Fail** or **Needs Adjustment** for each row.

## Flagged for your review in this demo

1. **Provisional audit record (Q207).** Discard-draft, put-away and take-off land
   in the menu's attributable history. A dedicated audit/analytics capability is
   backlogged as #677 — confirm the history is enough for now.
2. **Provisional capability grants (Q24).** `content.menu.manage`,
   `content.menu.import` and `publishing.history.view` are auto-granted to every
   role that already edits items, so gating could be wired. Confirm before they
   harden into their own permissions.
3. **Deferred column drops.** `HappyHourPrice`, `QuantityAvailable`, `Tags` and
   `IsPopular` are absent from the item library but still exist as columns,
   because POS inventory sync and the display payload still read them. They are
   dropped by the milestone that retires their last reader (M4/M6). Confirm the
   deferral.

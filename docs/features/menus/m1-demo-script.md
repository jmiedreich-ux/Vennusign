# Menus M1 — Demo Script

Milestone 1 is the schema spine: it has no screens of its own, so it is accepted
through this demo rather than a workbook. Everything below is walked through the
API contract against seeded data. Roughly 10 minutes.

- **Milestone:** M1 — the spine (issue #684)
- **What it proves:** acceptance criteria 1, 2, 3 and 4
- **Base URL:** the running API, `/api/back-office/content`
- **Auth:** a back-office session for the seeded venue
- **Interactive workbook:** open `m1-demo-workbook.html` in a browser to run these checks and record
  Pass / Fail / Needs Adjustment against each one. It saves as you go and exports a JSON record.
  This Markdown file and the workbook carry the same checks; the workbook is the one to actually test with.
- **Runner:** `scripts/run-m1-demo.ps1` walks every check and prints what each one returned.

## Before you start

Migration `058_create_menu_item_library_spine.sql` runs on startup. It creates the
item library **empty** (Q45 — a fresh start, nothing carried from the legacy
tables); the acceptance fixture seeds a menu that is already assigned and
published so the demo has something real to walk (Q3).

**The save model.** The draft is derived, not authored. The live rows are the
menu; the screens show the last published snapshot; the draft is the difference
between them. There is no endpoint that authors a change, and no client ever
supplies a previous value — it always comes from the published snapshot.

---

## 1. The venue's context — timezone and ceilings

```
GET /api/back-office/content/context
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

## 2. Criterion 1 — an 86 commits immediately, with no publish

```
PUT /api/back-office/content/items/{itemId}/availability
{ "isAvailable": false }
```

**Expect:** the response names the item, the time, who did it, and **every screen
currently showing that item through any menu**.

Then:

```
GET /api/back-office/content/menus/{menuId}/draft
```

**Expect:** `"count": 0`. The 86 changed the world without joining the draft.

**Honest scope:** in this milestone the named screens are the notification
contract — the change is stored and pushed. The screens render the new item model
when the milestone 4 player lands.

**Also check:** re-read the availability list after a minute and it is still off.
There is no auto-reset — an 86 stays off until a person turns it back on (Q14-r2).

---

## 3. Criterion 4 — an edit changes the menu now, and reaches no screen

Change a price through the editor:

```
PUT /api/back-office/menus/{menuId}/sections/{sectionId}/items/{itemId}
{ "name": "Harbor Lemonade", "price": 10.5 }
```

Then read the derived draft:

```
GET /api/back-office/content/menus/{menuId}/draft
```

**Expect:** `"count": 1`, and the change's `beforeValue` is the price the screens
are showing — taken from the published snapshot, never from the caller.

```
GET /api/back-office/content/menus/{menuId}/history
```

**Expect:** no new publish. Nothing has reached a screen.

---

## 4. The draft is the current diff, not a keystroke log

Change the same price again, then set it back to exactly what the screens show.

**Expect:** after the second edit the count is still `1` with the latest value;
after typing the published price back in, the count is `0`. What no longer
differs is not a change (Q182).

**One item, several boards:** if the item is placed on more than one section,
editing its price is still **one** change. The count is per field per item, not
per placement.

---

## 5. Criterion 2 — a publish ships this menu and nothing else

```
POST /api/back-office/content/menus/{menuId}/publish
```

**Expect:**
- `version` incremented, `changeCount` matching the draft you just saw;
- one target per screen, `Pending` for online screens and `Offline` for the rest
  — an offline screen is not a failure, it catches up;
- this menu's draft is now empty;
- another menu's pending content is **untouched**.

**What it guarantees:** the shipped set recorded in history is the difference of
the snapshot that actually went out. If someone edits between your read and the
commit, the publish recomputes rather than recording a set that did not ship.

---

## 6. Criterion 3 — the 86 survived the publish

```
GET /api/back-office/content/availability
```

**Expect:** the item from step 2 is still off. Availability is a fact about
tonight, not about the menu.

---

## 7. "Go back to" produces a draft, never a silent publish

```
POST /api/back-office/content/menus/{menuId}/go-back-to/{version}
```

**Expect:** the menu's working state returns to how it looked then, so those
values now wait as a draft against what the screens show, and history records the
restore with its author. **No new publish.** Going back is something you then
publish deliberately — never a second silent path to the screens.

**If a screen has moved on:** when a screen that version was on now shows a
different menu, the restore is **refused** and says so. It does not restore
around the conflict and report a success that did not happen.

---

## 7b. Ask the screen, not the API

```
GET /api/back-office/content/screens/showing
```

**Expect:** every screen in the venue, and the menu and published version it is
actually showing — taken from what was published to it, never from the assignment.

This is the question the whole model is built around, and until now nothing could ask
it. That is not a small gap: the first run of this demo reported twelve checks of
twelve while a screen sat stranded on a menu the system called shelved, because every
check asked the API whether it had accepted a request and none could ask the screen.

A menu can be assigned to a screen and not yet be on it — that is what a deliberate
publish means — so this disagreeing with `GET assignments` is normal and correct.

---

## 8. Take off the screens waits for Publish (Q68)

```
DELETE /api/back-office/content/menus/{menuId}/screens
GET    /api/back-office/content/menus/{menuId}/draft
```

**Expect:** a `screens` change waiting in the draft, and history already records
`taken_off_screens` with the person who did it. Taking a menu off is permanent,
so unlike an 86 it reaches the screens on the next Publish — which records the
act again against that publish event.

**If a screen has moved on:** a screen another menu now owns is **left alone** and
**named** in the publish response. A stale take-off never blanks content someone
else deliberately put there.

---

## 9. Put away, and put back on the shelf

```
PUT /api/back-office/content/menus/{menuId}/put-away
{ "isPutAway": true }
```

**Expect:** refused while a screen is still showing the menu — it is taken off,
and that take-off published, deliberately, so nothing goes blank without someone
deciding to. Being off a screen means the published snapshot no longer names one,
not merely that the assignment has gone: until the publish carries the take-off,
the screen is still showing the menu, and shelving it there would strand that
screen with no act left able to clear it. Once the take-off has shipped, the menu
is put away, the act is recorded with its author, and the venue's active menu
count drops: a put-away menu does not count against the ceiling, which is what
makes the refusal's advice ("put one away first") true.

A screen another menu has since been given is a different matter: it is not this
menu's to release, no publish would touch it, and so it does not hold the menu on
the shelf either.

Putting one back is bounded by that same ceiling, and refuses in the same plain
words when there is no room.

A put-away menu can still be edited — only its screens are settled — and the
draft that creates can still be discarded, because going back to the published
shape of a shelved menu puts it on no screen. Going back to an older version that
*was* on a screen is refused: that would be a way onto the shelf around the
ceiling check and the record.

---

## 10. The destructive acts are attributable

```
DELETE /api/back-office/content/menus/{menuId}/draft
GET    /api/back-office/content/menus/{menuId}/history
```

**Expect:** history shows `draft_discarded`, `restored`, `taken_off_screens` and
`put_away`, each with the person who did it and a plain detail line. Nothing
irreversible is anonymous. Because the draft held the pending take-off,
discarding it puts the menu back on its screen — the working state returns to
exactly what the screens show.

---

## Result

| # | Check | Result |
|---|---|---|
| 1 | Context shows venue timezone and configured ceilings | |
| 2 | An 86 is instant, names its screens, and never joins the draft | |
| 3 | An edit shows up as a derived change and reaches no screen | |
| 4 | Editing twice is one change; typing the published value back is none | |
| 5 | A publish ships this menu's diff, with honest per-screen state | |
| 6 | The 86 survived the publish | |
| 7 | "Go back to" produces a draft, not a publish | |
| 8 | Take-off waits for Publish and is recorded with its author | |
| 9 | Put away is refused on-screen, attributable, and frees ceiling room | |
| 10 | The destructive acts are recorded with their author | |

Record **Pass**, **Fail** or **Needs Adjustment** for each row.

## Flagged for your review in this demo

1. **Provisional audit record (Q207).** Discard-draft, put-away, take-off and
   restore land in the menu's attributable history. A dedicated audit/analytics
   capability is backlogged as #677 — confirm the history is enough for now.
2. **Provisional capability grants (Q24).** `content.menu.manage`,
   `content.menu.import` and `publishing.history.view` are auto-granted to every
   role that already edits items, so gating could be wired. Confirm before they
   harden into their own permissions.
3. **Deferred column drops.** `HappyHourPrice`, `QuantityAvailable`, `Tags` and
   `IsPopular` are absent from the item library but still exist as columns,
   because POS inventory sync still reads them. They are dropped by the milestone
   that retires their last reader. Confirm the deferral.

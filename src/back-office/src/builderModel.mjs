/**
 * The builder's decisions, kept out of the component so they can be asserted
 * without a browser — the same split the shelf uses.
 *
 * Nothing here talks to the API or to React. What it decides: what is selected,
 * what the canvas shows, and every sentence the surface says about a menu. Copy
 * lives here because copy is a rule (Q181 singular/zero forms, Q147 no possessive
 * phrasing, Q123's locked "where it lives" vocabulary) and rules belong where they
 * can be tested.
 */

/** Sections in board order, defended against a malformed payload. */
export function sectionsOf(board) {
  return Array.isArray(board?.sections) ? [...board.sections].sort((a, b) => a.sortOrder - b.sortOrder) : [];
}

/** One section's items in board order. */
export function itemsOf(board, sectionId) {
  const section = sectionsOf(board).find(candidate => candidate.sectionId === sectionId);
  return Array.isArray(section?.items) ? [...section.items].sort((a, b) => a.sortOrder - b.sortOrder) : [];
}

export function sectionOf(board, sectionId) {
  return sectionsOf(board).find(section => section.sectionId === sectionId) ?? null;
}

export function findItem(board, itemId) {
  for (const section of sectionsOf(board)) {
    const item = (section.items ?? []).find(candidate => candidate.itemId === itemId);
    if (item) return { item, sectionId: section.sectionId };
  }
  return null;
}

/**
 * A menu's very first open: One-section view, topmost section selected, nothing
 * selected in the inspector (Q116). Return visits restore where you left off,
 * which is the caller's business — this is only what "no memory of this menu" means.
 */
export function firstOpenState(board) {
  return {
    view: "one-section",
    sectionId: sectionsOf(board)[0]?.sectionId ?? null,
    selectedItemId: null
  };
}

/**
 * Where the builder resumes. A remembered section that no longer exists — deleted
 * by someone else, or by a restore — falls back to the top rather than leaving the
 * canvas blank with no way to explain itself.
 */
export function resumeState(board, remembered) {
  const sections = sectionsOf(board);
  const known = sections.some(section => section.sectionId === remembered?.sectionId);
  const selectedStillThere = remembered?.selectedItemId && findItem(board, remembered.selectedItemId);

  return {
    view: remembered?.view === "whole-board" ? "whole-board" : "one-section",
    sectionId: known ? remembered.sectionId : (sections[0]?.sectionId ?? null),
    selectedItemId: selectedStillThere ? remembered.selectedItemId : null
  };
}

/**
 * What the canvas draws. One-section view is the editing view and shows exactly
 * one section; Whole board shows the menu as a guest would read it (Q105).
 */
export function canvasBoard(board, { view, sectionId }) {
  if (view !== "one-section") return board;
  const section = sectionOf(board, sectionId);
  return { ...board, sections: section ? [section] : [] };
}

/**
 * "3 changes not on your screens", with the natural singular and zero (Q181).
 *
 * A menu nobody has published is a different sentence, not a bigger number. Its
 * "changes" are its entire contents measured against nothing, so saying "12
 * changes not on your screens" describes a comparison that has no other side —
 * and it is the shelf's "Never published" state, which draws no bar at all.
 */
export function draftPhrase(count, { neverPublished = false } = {}) {
  if (neverPublished) return "Nothing on your screens yet";
  if (!count) return "Everything is on your screens";
  return count === 1 ? "1 change not on your screens" : `${count} changes not on your screens`;
}

/**
 * Whether discarding is even a thing that could happen.
 *
 * Discard puts the menu back to what its screens are showing. A menu that has
 * never published has nothing to go back TO, so the act does nothing — and a
 * control that cannot do anything is exactly what decision 5 forbids. The link is
 * absent there rather than present and inert.
 */
export function canDiscardDraft({ draftCount, publishedVersion }) {
  return Boolean(draftCount) && publishedVersion !== null && publishedVersion !== undefined;
}

/** The Publish button's label, the same in both bar forms (Q161). */
export function publishLabel(count) {
  return count === 1 ? "Publish 1 change" : `Publish ${count} changes`;
}

/**
 * A time as the venue reads it, never as the viewer's browser does (Q196). A venue
 * in Denver whose owner is in London must not be told its board published tomorrow.
 */
export function venueTime(utc, timezone) {
  if (!utc) return null;
  const when = new Date(utc);
  if (Number.isNaN(when.getTime())) return null;

  const format = zone =>
    new Intl.DateTimeFormat("en-US", {
      weekday: "short",
      hour: "numeric",
      minute: "2-digit",
      timeZone: zone
    }).format(when);

  let formatted;
  try {
    formatted = format(timezone || "UTC");
  } catch {
    // An unknown zone is the venue record's problem, not something to crash a
    // board over. UTC, and the surface still renders.
    formatted = format("UTC");
  }

  // The design writes it "Tue 4:12pm" (README, publish bar). Intl gives
  // "Tue, 4:12 PM"; the shape is the authority's, not the formatter's.
  return formatted.replace(",", "").replace(/\s([AP])M$/, (_, half) => `${half.toLowerCase()}m`);
}

/**
 * The publish bar's meta line. A menu nobody has published says so plainly rather
 * than showing an empty slot where a date should be.
 */
export function publishedLine({ lastPublishedUtc, lastPublishedBy }, timezone) {
  const when = venueTime(lastPublishedUtc, timezone);
  if (!when) return "Not published yet";
  return lastPublishedBy ? `Published ${when} by ${lastPublishedBy}` : `Published ${when}`;
}

/**
 * Q123's locked vocabulary for where an item lives: up to two board names, a count
 * beyond that. The board being edited is never named — the person is looking at it.
 */
export function boardsPhrase(boards, currentMenuId) {
  const others = (boards ?? []).filter(board => board.menuId !== currentMenuId);
  if (others.length === 0) return null;
  if (others.length === 1) return others[0].menuName;
  if (others.length === 2) return `${others[0].menuName} and ${others[1].menuName}`;
  return `${others.length} boards`;
}

/**
 * The line under the price when an item is on other boards too.
 *
 * Q5's design follow-up: one item is one shared price everywhere, and the flag on
 * that answer asked that editing it "feel easy". This is the resolution — a quiet,
 * permanent statement of fact rather than a confirmation step. A dialog on every
 * price edit is the opposite of easy, and a separate quick-price mode would be the
 * second editor decision 15 exists to refuse.
 *
 * It says "when you publish them" because that is true: the edit reaches each
 * board's draft immediately and each board's screens only on its own publish.
 */
export function sharedItemLine(boards, currentMenuId) {
  const phrase = boardsPhrase(boards, currentMenuId);
  if (!phrase) return null;
  const plural = (boards ?? []).filter(board => board.menuId !== currentMenuId).length > 1;
  return `Also on ${phrase} — ${plural ? "they" : "it"} will show this when you publish ${plural ? "them" : "it"}.`;
}

/**
 * The note drawn on an 86'd row of the canvas.
 *
 * The design authority writes it "86'd 6:40pm — hidden on all screens right now":
 * the TIME matters, because the first question about an item that is off is when
 * it went off, and the sentence is useless without it.
 */
export function unavailableNote(availability, timezone) {
  if (!availability || availability.isAvailable) return null;
  const when = venueTime(availability.changedUtc, timezone);
  return when
    ? `86'd ${when} — hidden on all screens right now`
    : "86'd — hidden on all screens right now";
}

/** A price nobody has typed yet. Quiet flag, never a refusal to publish (Q113). */
export function isMissingPrice(item) {
  return !item?.price || String(item.price).trim().length === 0;
}

/**
 * What the availability panel says about an item that is off right now (Q104).
 * The time is the venue's, and the sentence names the consequence rather than the
 * state: "off" is a setting, "hidden on all screens right now" is what a guest sees.
 */
export function availabilityLine(availability, timezone) {
  if (!availability || availability.isAvailable) return null;
  const when = venueTime(availability.changedUtc, timezone);
  const stamp = when ? ` — 86'd ${when}` : "";
  // "not part of your draft" is verbatim copy in the design authority. It is the
  // sentence that separates this control from every other one on the page.
  return `Off right now${stamp}. Hidden on every screen showing it — not part of your draft. Turning it back on shows it immediately.`;
}

/**
 * Publishing is blocked while a save has not been confirmed (Q197). The reason is
 * returned rather than a bare false, because a disabled control that cannot say why
 * is the thing decision 5 forbids.
 */
export function publishBlockedReason({ draftCount, saveState, isPutAway }) {
  if (saveState === "failed") return "Your last change hasn't saved yet. Publishing waits until it does.";
  if (saveState === "saving") return "Saving your last change…";
  if (isPutAway) return "This menu is not in use. Put it back on the shelf to publish it.";
  if (!draftCount) return null;
  return null;
}

/**
 * The publish bar's screen presentation: a chip per screen at six targets or
 * fewer, a count plus only the exceptions above that (Q161). Exceptions are drawn
 * in full however many there are — never summarised behind a count (Q167).
 */
export const screenChipCutover = 6;

export function publishTargets(screens) {
  const targets = Array.isArray(screens) ? screens : [];
  const exceptions = targets.filter(screen => screen.state !== "ready");

  return {
    mode: targets.length <= screenChipCutover ? "chips" : "count",
    total: targets.length,
    chips: targets.length <= screenChipCutover ? targets : [],
    exceptions,
    countPhrase: targets.length === 1 ? "1 screen" : `${targets.length} screens`
  };
}

/**
 * Moving one entry of a list. Returns a new array; the caller sends it to the API,
 * which refuses the whole thing if the menu moved underneath in the meantime.
 */
export function reorder(ids, from, to) {
  const next = [...(ids ?? [])];
  if (from < 0 || from >= next.length || to < 0 || to >= next.length) return next;
  const [moved] = next.splice(from, 1);
  next.splice(to, 0, moved);
  return next;
}

/**
 * ⌘K searches the board in front of you, not the library (Q121) — the library is
 * what the add row searches. Matches on name and description, because "the one with
 * the chilli in it" is how people actually look for something.
 */
export function findOnBoard(board, query) {
  const needle = (query ?? "").trim().toLowerCase();
  if (!needle) return [];

  const hits = [];
  for (const section of sectionsOf(board)) {
    for (const item of section.items ?? []) {
      const name = (item.name ?? "").toLowerCase();
      const description = (item.description ?? "").toLowerCase();
      if (name.includes(needle) || description.includes(needle)) {
        hits.push({ itemId: item.itemId, name: item.name, sectionId: section.sectionId, sectionName: section.name });
      }
    }
  }
  return hits;
}

/**
 * One queued change, said in the words of the thing it happened to.
 *
 * The API answers in the model's terms — target kind, id, field — because that is
 * what cannot drift. Turning that into "Harbor Lemonade — price" is the surface's
 * job, and it needs the board to look the names up in: an id is not a sentence.
 */
export function changeSentence(change, board) {
  const field = String(change?.field ?? "").replace(/([A-Z])/g, " $1").toLowerCase().trim();

  if (change?.targetKind === "screens") return "Which screens this menu is on";
  if (change?.targetKind === "menu") return `This menu — ${field}`;
  if (change?.targetKind === "theme") return "The look attached to this menu";

  // A placement is not an item edit: it is whether the item is ON this board, and
  // where. Saying "placed — changed" describes the model rather than the act.
  if (change?.targetKind === "placement") {
    const name = findItem(board, change?.targetId)?.item?.name ?? change?.beforeValue ?? "An item";
    if (change.field === "placed") {
      return change.afterValue === "true" ? `${name} — added to this board` : `${name} — taken off this board`;
    }
    if (change.field === "sortOrder") return `${name} — moved`;
    return `${name} — ${field}`;
  }

  if (change?.targetKind === "section") {
    const section = sectionsOf(board).find(candidate => candidate.sectionId === change.targetId);
    return section?.name ? `${section.name} — ${field}` : `A section — ${field}`;
  }

  const found = findItem(board, change?.targetId);
  if (found?.item?.name) return `${found.item.name} — ${field}`;

  // Removed items are the case where the board cannot name it: the row is gone,
  // which is exactly what the change says. The before value is the last name it had.
  return change?.beforeValue ? `${change.beforeValue} — ${field}` : `An item — ${field}`;
}

/**
 * What a queued change is doing to the value, said as the value.
 *
 * "changed" is what the code knows; "12.50 -> 14.00" is what a person came to find
 * out. Both sides are already in the change - they were being used only to pick
 * the word.
 */
export function changeValues(change) {
  const before = nonBlank(change?.beforeValue);
  const after = nonBlank(change?.afterValue);

  // A placement reads as an act, not a value: "true -> false" describes a column.
  if (change?.targetKind === "placement") return after === "true" ? "added" : "removed";

  if (before === null && after === null) return "changed";
  if (before === null) return `now ${after}`;
  if (after === null) return `was ${before}, now empty`;
  return `${before} → ${after}`;
}

function nonBlank(value) {
  const trimmed = typeof value === "string" ? value.trim() : "";
  return trimmed.length > 0 ? trimmed : null;
}

/** What deleting a section actually did, in the words the API counted (Q96). */
export function releasedPhrase(count) {
  if (!count) return "Section deleted.";
  return count === 1
    ? "Section deleted. Its item went back to your library."
    : `Section deleted. Its ${count} items went back to your library.`;
}

/**
 * The banned words, asserted against this module's own output rather than trusted.
 * Criterion 5 covers Menus and every surface rewritten here (Q179).
 */
export const bannedWords = ["unpublish", "supersede", "restore", "archive"];

/**
 * What tells two library candidates apart (A21).
 *
 * A venue library can hold the same dish twice at the same price - an older import split it - and
 * the review screen offered both as "Use the one you already have — Pad Thai $12.95", twice,
 * identically. That is not a question an operator can answer; it is one they can only guess at.
 *
 * The owner ruled against merging them silently on 2026-08-27. The screen says which menus each
 * one is on and when it was made, and the operator decides. A duplicate the venue can see is a
 * duplicate the venue can deal with.
 */

/**
 * The line under a candidate, or null when there is nothing worth saying.
 *
 * Null on a lone candidate: there is nothing to distinguish it FROM, and a line of provenance
 * under every single-answer question is noise (decision 18 - confirm only what we were unsure of).
 */
export function candidateProvenance(candidate, candidateCount) {
  if (!candidate || candidateCount <= 1) return null;

  const parts = [];
  const menus = candidate.onMenus;

  // An empty list is a fact, not a gap: a dish on no menu is exactly what tells it from the one
  // that is on two. `undefined` means nobody looked, and says nothing.
  if (Array.isArray(menus)) parts.push(menus.length === 0 ? "On no menu" : `On ${listPhrase(menus)}`);
  const made = madePhrase(candidate.itemCreatedUtc);
  if (made) parts.push(made);

  return parts.length ? parts.join(" · ") : null;
}

/** "Lunch", "Lunch and Dinner", "Lunch, Dinner and 2 more" — a list nobody has to scroll. */
export function listPhrase(names, limit = 2) {
  const shown = names.slice(0, limit);
  const rest = names.length - shown.length;
  if (rest > 0) return `${shown.join(", ")} and ${rest} more`;
  if (shown.length === 1) return shown[0];
  return `${shown.slice(0, -1).join(", ")} and ${shown[shown.length - 1]}`;
}

/** "Added 12 Aug". Absent rather than "Invalid Date" when there is no usable date. */
export function madePhrase(createdUtc) {
  if (!createdUtc) return null;
  const at = new Date(createdUtc);
  if (Number.isNaN(at.getTime())) return null;
  return `Added ${at.toLocaleDateString(undefined, { month: "short", day: "numeric" })}`;
}

/**
 * The one-line summary of what a replacement does (M6.13).
 *
 * Decision 12 — summarize the normal, name the exception. The counts are the summary; the named
 * lists beside them are the exception worth reading.
 *
 * Null when nothing changes at all. Re-importing a menu nobody has edited is a real thing to do,
 * and "0 arrive · 0 go · 0 change price" is a worse way of saying "nothing" than saying nothing.
 */
export function replaceSummary(preview) {
  if (!preview) return null;

  const parts = [];
  if (preview.arrivingCount > 0) parts.push(`${preview.arrivingCount} ${preview.arrivingCount === 1 ? "dish arrives" : "dishes arrive"}`);
  if (preview.leavingCount > 0) parts.push(`${preview.leavingCount} ${preview.leavingCount === 1 ? "goes" : "go"}`);
  if (preview.repricedCount > 0) parts.push(`${preview.repricedCount} ${preview.repricedCount === 1 ? "changes price" : "change price"}`);

  return parts.length ? parts.join(" · ") : null;
}

/** "and 4 more", or nothing when the list is whole. */
export function andMore(shown, total) {
  const rest = Number(total ?? 0) - (shown?.length ?? 0);
  return rest > 0 ? `and ${rest} more` : null;
}

/** A price as it moves. Both numbers, exactly as stored (Q115/Q190); "no price" reads as words. */
export function priceMovePhrase(move) {
  if (!move) return null;
  const from = move.from ?? "no price";
  const to = move.to ?? "no price";
  return `${from} → ${to}`;
}

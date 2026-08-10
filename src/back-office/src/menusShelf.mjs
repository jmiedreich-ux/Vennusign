/*
 * The Menus home shelf's decisions, apart from its markup.
 *
 * Everything here is a pure function of the shelf the API returned: which cards
 * show, in what order, what the headline says, and whether the shelf has grown
 * enough to change shape. Pure because these are the rules most likely to be got
 * wrong quietly - an ordering that shuffles, a count that disagrees with what is
 * drawn - and those are cheap to assert and expensive to eyeball.
 */

/**
 * The one cutover, at seven total menus (Q163).
 *
 * Put-away menus count towards it: they are still menus the person has, they
 * still appear in the strip, and search still finds them. At six or fewer the
 * shelf is exactly the M1 design - it grows and scrolls, nothing collapses.
 */
export const shelfScaleThreshold = 7;

export function isShelfAtScale(menus) {
  return (menus?.length ?? 0) >= shelfScaleThreshold;
}

/**
 * Menus in use, most recently published first, then the never-published.
 *
 * Recency is the shelf's order because the thing you changed last is the thing
 * you are most likely to be coming back to. A menu with no publish yet has no
 * recency to sort by, so it sorts by name rather than drifting.
 */
export function menusInUse(menus) {
  return (menus ?? [])
    .filter((menu) => !menu.isPutAway)
    .slice()
    .sort(byRecencyThenName);
}

/** The "Not in use" strip: put away, kept with their history (Q163). */
export function menusNotInUse(menus) {
  return (menus ?? []).filter((menu) => menu.isPutAway).slice().sort(byRecencyThenName);
}

function byRecencyThenName(left, right) {
  const leftTime = Date.parse(left?.lastPublishedUtc ?? "");
  const rightTime = Date.parse(right?.lastPublishedUtc ?? "");
  const leftKnown = Number.isFinite(leftTime);
  const rightKnown = Number.isFinite(rightTime);

  if (leftKnown && rightKnown && leftTime !== rightTime) return rightTime - leftTime;
  if (leftKnown !== rightKnown) return leftKnown ? -1 : 1;
  return String(left?.name ?? "").localeCompare(String(right?.name ?? ""));
}

/**
 * The filter chips, single-select, none active on load (Q164).
 *
 * A chip that would match nothing is not offered: an empty filter is a dead end
 * that teaches the person nothing about their own shelf.
 */
export const shelfFilters = Object.freeze([
  { key: "on-screens", label: "On screens", matches: (menu) => screensOf(menu).length > 0 },
  { key: "pending", label: "Changes waiting", matches: hasChangesWaiting },
  { key: "not-in-use", label: "Not in use", matches: (menu) => menu.isPutAway }
]);

/**
 * Changes waiting means waiting to reach a screen, so a menu that has never been
 * published has none — everything about it is waiting, which is a different fact
 * and the card states it differently.
 *
 * Shared with the card rather than written twice: a filter that counted three and
 * a shelf that drew two bars is exactly the kind of quiet disagreement nobody
 * reports, they just stop trusting the number.
 */
/**
 * A menu's screens, always as an array.
 *
 * The API sends one; this defends the boundary anyway, because every caller
 * below does arithmetic on it. A malformed payload would otherwise throw out of
 * render and take the whole shelf down rather than drawing one odd card.
 * Raised by the independent review.
 */
export function screensOf(menu) {
  return Array.isArray(menu?.screenIds) ? menu.screenIds : [];
}

export function hasChangesWaiting(menu) {
  return menu?.draftCount > 0 && menu?.publishedVersion !== null && menu?.publishedVersion !== undefined;
}

export function availableShelfFilters(menus) {
  return shelfFilters.filter((filter) => (menus ?? []).some((menu) => filter.matches(menu)));
}

/** Search covers put-away menus too, so a shelved menu can still be found (Q163). */
export function filterShelf(menus, { search = "", filter = null } = {}) {
  const term = search.trim().toLowerCase();
  const chip = shelfFilters.find((candidate) => candidate.key === filter);

  return (menus ?? []).filter((menu) => {
    if (chip && !chip.matches(menu)) return false;
    if (!term) return true;
    return String(menu.name ?? "").toLowerCase().includes(term);
  });
}

/**
 * The status headline: a sentence naming what is holding changes, capped at the
 * top three menus (Q169).
 *
 * Never a green all-clear and never a status table - decision 12. It summarises
 * the normal and names the exception, so a shelf of thirteen reads as easily as
 * a shelf of two.
 *
 * Copy uses natural singular and zero forms throughout (Q181): "1 change", not
 * "1 changes"; "your screen", not "your screens", when there is one.
 */
export function shelfHeadline(menus) {
  const inUse = menusInUse(menus);
  if (inUse.length === 0) return "Nothing on your screens yet.";

  const holding = inUse.filter(hasChangesWaiting);
  if (holding.length === 0) {
    const onScreens = inUse.filter((menu) => screensOf(menu).length > 0);
    if (onScreens.length === 0) return "Nothing is on your screens.";

    const screens = new Set(onScreens.flatMap(screensOf)).size;
    return screens === 1
      ? "Your screen is showing the latest."
      : `All ${screens} screens are showing the latest.`;
  }

  // The recorded shape, verbatim from Q169: "Summer Menu is holding 3 changes,
  // Patio Drinks 1". The first says it in full and the rest are a name and a
  // number, so a shelf of thirteen still reads as one sentence rather than a
  // list. Most changes first, because that is the one to look at.
  const byWeight = holding.slice().sort((left, right) => right.draftCount - left.draftCount);
  const named = byWeight.slice(0, 3);
  const rest = byWeight.length - named.length;

  const sentence = named
    .map((menu, index) =>
      index === 0 ? `${menu.name} is holding ${changePhrase(menu.draftCount)}` : `${menu.name} ${menu.draftCount}`
    )
    .join(", ");

  return rest > 0 ? `${sentence}, and ${rest} more ${rest === 1 ? "menu" : "menus"}.` : `${sentence}.`;
}

/** The sub-line under the headline: where those menus actually are. */
export function shelfSubLine(menus) {
  const inUse = menusInUse(menus);
  if (inUse.length === 0) return "Add a menu to get something on your screens.";

  const screens = new Set(inUse.flatMap(screensOf)).size;
  const away = menusNotInUse(menus).length;

  const parts = [];
  parts.push(screens === 1 ? "1 screen in use" : `${screens} screens in use`);
  parts.push(inUse.length === 1 ? "1 menu" : `${inUse.length} menus`);
  if (away > 0) parts.push(away === 1 ? "1 not in use" : `${away} not in use`);

  return parts.join(" · ");
}

/** "3 changes not published", with the natural singular (Q181). */
export function changePhrase(count) {
  return count === 1 ? "1 change" : `${count} changes`;
}

/**
 * A card's status line: what this menu is doing, in one phrase.
 *
 * "Never published" is its own state rather than a kind of "not on a screen":
 * a menu nobody has published has no board to show, and saying so is the honest
 * version of a blank card.
 */
export function cardStatus(menu) {
  if (menu?.isPutAway) return { tone: "idle", text: "Not in use" };
  if (menu?.publishedVersion === null || menu?.publishedVersion === undefined) {
    return { tone: "idle", text: "Never published" };
  }

  const screens = screensOf(menu).length;
  if (screens === 0) return { tone: "idle", text: "Not on a screen" };
  if (hasChangesWaiting(menu)) {
    return { tone: "pending", text: screens === 1 ? "On your screen" : `On ${screens} screens` };
  }

  return { tone: "live", text: screens === 1 ? "On your screen" : `On ${screens} screens` };
}

/**
 * How many items the board draws, for the count beside a card's name.
 *
 * Counted from the board the card is drawing, not from the menu's live rows: the
 * card shows what the screens show, so its count has to describe the same thing.
 */
export function boardCounts(board, unavailableItemIds) {
  const off = new Set(unavailableItemIds ?? []);
  const sections = (board?.sections ?? []).map((section) =>
    (section.items ?? []).filter((item) => !off.has(item.itemId))
  );
  const visible = sections.filter((items) => items.length > 0);

  return {
    sections: visible.length,
    items: visible.reduce((total, items) => total + items.length, 0)
  };
}

/**
 * Deterministic board-fit estimate. Geometry is an input, not a tier allowance;
 * callers can therefore evaluate the same page against every assigned screen.
 */
export function calculateBoardCapacity(board, geometry, theme = null) {
  const width = positive(geometry?.width, 1920);
  const height = positive(geometry?.height, 1080);
  const sections = board?.sections ?? [];
  const columns = Math.max(1, Math.floor(width / 720));
  // An attached menu theme reserves a small amount of vertical breathing room
  // for its display treatment. The estimate is deliberately conservative until
  // theme-authored measurements replace this M3-A fit model; unlike the previous
  // inert argument, changing theme can now change the exposed limit.
  const themeScale = theme && theme !== "midnight" ? 1.05 : 1;
  const usableHeight = Math.max(0, height - 220 - sections.length * 90);
  const rowsPerColumn = Math.max(0, Math.floor(usableHeight / (96 * themeScale)));
  const limit = rowsPerColumn * columns;
  const items = sections.flatMap(section => section.items ?? []);
  const dropped = items.slice(limit).map(item => item.name || "Unnamed item");
  const ratio = limit === 0 ? (items.length === 0 ? 0 : 1) : items.length / limit;
  return {
    limit,
    count: items.length,
    dropped,
    state: dropped.length > 0 ? "overflow" : ratio >= 0.8 ? "nearly-full" : "fits"
  };
}

function positive(value, fallback) {
  return typeof value === "number" && Number.isFinite(value) && value > 0 ? value : fallback;
}

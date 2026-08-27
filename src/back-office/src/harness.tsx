import { createRoot } from "react-dom/client";
import MenusHome from "./MenusHome";
import "./menus-home.css";

const shelf = ["Weekday", "Weekend"].map((name, i) => ({
  id: `m${i}`, name, isPutAway: false, publishedVersion: 3, draftCount: i === 0 ? 25 : 0,
  screens: [], screenIds: i === 0 ? ["s1"] : [], board: { sections: [] },
  lastPublishedUtc: "2026-08-26T18:00:00Z", lastPublishedBy: "Jeremy"
}));

const imports = Array.from({ length: 7 }, (_, i) => ({
  id: `1111111${i}-1111-1111-1111-111111111111`,
  itemCount: i === 6 ? 60 : 46, lineCount: 133, answersRemaining: 4,
  createdUtc: new Date(Date.now() - (i + 1) * 3600_000).toISOString(),
  updatedUtc: new Date(Date.now() - (i + 1) * 3600_000).toISOString(),
  expiresUtc: new Date(Date.now() + (24 - i) * 3600_000).toISOString()
}));

const originalFetch = window.fetch;
window.fetch = (async (input: RequestInfo | URL, init?: RequestInit) => {
  const url = String(typeof input === "string" ? input : input instanceof URL ? input.href : input.url);
  const json = (b: unknown) => new Response(JSON.stringify(b), { status: 200, headers: { "Content-Type": "application/json" } });
  if (url.includes("/menus/allowance")) return json({ used: 2, limit: 50 });
  if (url.includes("/menu-imports")) return init?.method === "DELETE" ? new Response(null, { status: 204 }) : json(imports);
  if (url.includes("/availability")) return json([]);
  if (url.includes("/menus")) return json(shelf);
  return originalFetch(input as RequestInfo, init);
}) as typeof window.fetch;

createRoot(document.getElementById("root")!).render(
  <MenusHome configuration={{ apiBaseUrl: "" } as never} accessToken="stub" venueName="My Bar"
    onOpenMenu={() => {}} onAddMenu={() => {}} starterMenuName={null}
    onFixScreens={() => {}} onQuickUpdate={() => {}} canQuickUpdate />
);

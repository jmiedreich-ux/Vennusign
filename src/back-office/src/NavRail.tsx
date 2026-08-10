import * as lucide from "lucide-react";
import LockedNavigationItem from "./LockedNavigationItem";
import {
  backOfficeRailSections,
  canOpenBackOfficeRoute,
  decisionForBackOfficeRoute,
  isBackOfficeRouteVisible
} from "./navigation.mjs";
import type { BackOfficeRoute, BackOfficeRouteDecision } from "./navigation.d.mts";
import type { UpgradeOpportunity } from "./upgradeExperience.mjs";

export type NavRailProps = {
  activePath: string;
  decisions: BackOfficeRouteDecision[];
  opportunities: readonly Readonly<UpgradeOpportunity>[];
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
  displayName: string;
  onSignOut: () => void;
  open: boolean;
  children?: React.ReactNode;
};

/**
 * The icon for a route, by name.
 *
 * Named rather than imported one by one so the route table stays the single
 * place a route is described — adding an area is a row of data, not an edit in
 * two files. A name lucide does not export falls back to a neutral glyph rather
 * than crashing the whole shell over a typo in a label.
 */
function RouteIcon({ name }: { name: string }) {
  const icons = lucide as unknown as Record<string, React.ComponentType<{ size?: number; strokeWidth?: number; "aria-hidden"?: boolean }>>;
  const Icon = icons[name] ?? lucide.Circle;
  return <Icon size={15} strokeWidth={2} aria-hidden />;
}

/**
 * The 76px icon rail, app-wide (build-decision 12).
 *
 * One flat column rather than the four collapsible groups it replaces: at this
 * width there is no room for headings, and the design's spec is a single divider
 * near the bottom. Every area lives here, so the gating is built once.
 */
export default function NavRail({
  activePath,
  decisions,
  opportunities,
  onUpgrade,
  displayName,
  onSignOut,
  open,
  children
}: NavRailProps) {
  const visibleIn = (key: string) =>
    (backOfficeRailSections.find((section) => section.key === key)?.routes ?? []).filter((route) =>
      isBackOfficeRouteVisible(route, decisions)
    );

  const work = visibleIn("work");
  const account = visibleIn("account");

  const renderItem = (route: BackOfficeRoute) => {
    const unlocked = canOpenBackOfficeRoute(route, decisions);
    const decision = decisionForBackOfficeRoute(route, decisions);
    const opportunity =
      !unlocked && decision?.resolution === "review_product_access"
        ? opportunities.find((candidate) => candidate.featureKey === route.upgradeFeature)
        : undefined;

    if (opportunity) {
      return (
        <LockedNavigationItem
          key={route.path}
          opportunity={opportunity}
          onUpgrade={onUpgrade}
          route={route.path}
        />
      );
    }

    return (
      <a
        key={route.path}
        className={`rail-item${activePath === route.path ? " active" : ""}${unlocked ? "" : " locked"}`}
        href={`#/${route.path}`}
        data-testid="nav-item"
        data-route={route.path}
        data-unlocked={unlocked}
        data-active={activePath === route.path}
        aria-disabled={!unlocked}
        aria-current={activePath === route.path ? "page" : undefined}
        /* The icon carries no words, so the accessible name comes from the full
           label rather than the abbreviated one under it. */
        aria-label={unlocked ? route.label : `${route.label} — locked`}
        title={unlocked ? route.label : decision?.message}
      >
        <RouteIcon name={route.icon} />
        <span className="rail-item-label" aria-hidden>
          {route.railLabel}
        </span>
      </a>
    );
  };

  return (
    <aside className="app-rail" id="app-sidebar" data-open={open} data-testid="nav-rail">
      <a className="rail-brand" href="#/home" aria-label="Vennusign Back Office">
        <span aria-hidden>V</span>
      </a>

      {/*
        Two landmarks, each named in full rather than built from a variable: a
        screen reader announces these, so they are copy, not data. Decision 19's
        filtering happens per section, so an area the plan does not include is
        absent — and a section left with nothing in it takes its divider with it
        rather than ruling off empty space.
      */}
      {work.length > 0 ? (
        <nav className="rail-section rail-section-work" aria-label="Back Office" data-rail-section="work">
          {work.map(renderItem)}
        </nav>
      ) : null}

      {account.length > 0 ? (
        <nav className="rail-section rail-section-account" aria-label="Account" data-rail-section="account">
          {account.map(renderItem)}
        </nav>
      ) : null}

      {children}

      <button className="rail-identity" type="button" onClick={onSignOut} title={`${displayName} — sign out`}>
        <span aria-hidden>{displayName.slice(0, 1)}</span>
        <span className="rail-item-label">Sign out</span>
      </button>
    </aside>
  );
}

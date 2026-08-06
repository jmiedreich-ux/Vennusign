import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import DateRangePromotionAdministration from "./DateRangePromotionAdministration";
import EmergencyBroadcastAdministration from "./EmergencyBroadcastAdministration";
import HappyHourAdministration from "./HappyHourAdministration";
import MealPeriodAdministration from "./MealPeriodAdministration";
import PlaylistAdministration from "./PlaylistAdministration";
import ScreenManagement from "./ScreenManagement";
import TapListAdministration from "./TapListAdministration";
import ThemeBuilder from "./ThemeBuilder";
import { loadManagedScreens, type BackOfficeCapabilityDecision, type ManagedScreen } from "./api";
import type { BackOfficeConfiguration } from "./config";
import "./operations.css";

type Props = {
  configuration: BackOfficeConfiguration;
  accessToken: string;
  venueId: string;
  capabilities: string[];
  decisions: BackOfficeCapabilityDecision[];
  area: "screens" | "themes" | "schedules" | "tap-list";
};

export default function VenueOperations({
  configuration,
  accessToken,
  venueId,
  capabilities,
  decisions,
  area
}: Props) {
  const [screens, setScreens] = useState<ManagedScreen[]>([]);
  const [screensState, setScreensState] = useState<"loading" | "ready" | "error">("loading");
  const schedulePanels = ["overview", "meal-periods", "happy-hour", "playlists", "promotions", "emergency"] as const;
  type SchedulePanel = typeof schedulePanels[number];
  const requestedPanel = new URLSearchParams(window.location.search).get("schedule");
  const [schedulePanel, setSchedulePanel] = useState<SchedulePanel>(
    schedulePanels.includes(requestedPanel as SchedulePanel) ? requestedPanel as SchedulePanel : "overview"
  );
  const tabRefs = useRef<Array<HTMLButtonElement | null>>([]);
  const allLayouts = capabilities.includes("branding.layout.manage");
  const scheduling = capabilities.includes("schedule.entry.manage");

  useEffect(() => {
    if (area !== "schedules") return;
    setScreensState("loading");
    loadManagedScreens(configuration, accessToken, venueId)
      .then(value => { setScreens(value); setScreensState("ready"); })
      .catch(() => { setScreens([]); setScreensState("error"); });
  }, [accessToken, area, configuration, venueId]);

  if (area === "screens") {
    return <div className="operations-stack">
      <ScreenManagement
        configuration={configuration}
        apiKey={accessToken}
        venueId={venueId}
        allLayoutsEnabled={allLayouts}
        pairDecision={decisions.find(decision => decision.capabilityId === "screen.device.pair")}
        targetDecision={decisions.find(decision => decision.capabilityId === "screen.content.target")}
        recoverDecision={decisions.find(decision => decision.capabilityId === "screen.delivery.recover")}
        unpairDecision={decisions.find(decision => decision.capabilityId === "screen.device.unpair")}
        videoWallEnabled={capabilities.includes("screen.wall.coordinate")}
        showUpgradePrompt={false}
      />
    </div>;
  }

  if (area === "themes") {
    return <div className="operations-stack">
      <ThemeBuilder
        configuration={configuration}
        apiKey={accessToken}
        venueId={venueId}
        advancedEnabled={allLayouts}
        showUpgradePrompt={false}
      />
    </div>;
  }

  if (area === "tap-list") {
    return <div className="operations-stack">
      <TapListAdministration
        configuration={configuration}
        apiKey={accessToken}
        venueId={venueId}
        enabled={allLayouts}
        showUpgradePrompt={false}
      />
    </div>;
  }

  const selectPanel = (panel: SchedulePanel) => {
    setSchedulePanel(panel);
    const url = new URL(window.location.href);
    url.searchParams.set("schedule", panel);
    window.history.replaceState(null, "", url);
  };
  const onTabKeyDown = (event: KeyboardEvent<HTMLButtonElement>, index: number) => {
    let next = index;
    if (event.key === "ArrowRight") next = (index + 1) % schedulePanels.length;
    else if (event.key === "ArrowLeft") next = (index - 1 + schedulePanels.length) % schedulePanels.length;
    else if (event.key === "Home") next = 0;
    else if (event.key === "End") next = schedulePanels.length - 1;
    else return;
    event.preventDefault();
    selectPanel(schedulePanels[next]);
    tabRefs.current[next]?.focus();
  };
  const labels: Record<SchedulePanel, string> = {
    overview: "Overview", "meal-periods": "Meal periods", "happy-hour": "Happy hour",
    playlists: "Playlists", promotions: "Promotions", emergency: "Emergency"
  };

  return <div className="operations-stack scheduling-workspace">
    <header className="schedule-heading"><div><p>Live-control workspace</p><h2>Schedules and overrides</h2></div><span>{screensState === "ready" ? `${screens.length} screen${screens.length === 1 ? "" : "s"}` : "Checking screens…"}</span></header>
    <nav className="schedule-tabs" role="tablist" aria-label="Schedule administration">
      {schedulePanels.map((panel, index) => <button
        key={panel} ref={node => { tabRefs.current[index] = node; }} role="tab"
        aria-selected={schedulePanel === panel} aria-controls={`schedule-panel-${panel}`}
        tabIndex={schedulePanel === panel ? 0 : -1} onClick={() => selectPanel(panel)}
        onKeyDown={event => onTabKeyDown(event, index)}>{labels[panel]}</button>)}
    </nav>
    {screensState === "error" ? <p className="state error" role="alert">Screen targets could not be loaded. Scheduling remains available, but screen-specific actions are disabled until targets refresh.</p> : null}
    {screensState === "ready" && screens.length === 0 ? <aside className="schedule-warning" role="status"><strong>No screens are available.</strong><span>Add or restore a screen before creating a playlist or sending a targeted override.</span></aside> : null}
    <section id={`schedule-panel-${schedulePanel}`} role="tabpanel" aria-label={labels[schedulePanel]}>
    {schedulePanel === "overview" ? <div className="schedule-overview">
      <article><strong>Live precedence</strong><p>Emergency broadcasts override normal content. Promotions can change layout; meal periods and happy hour change scheduled content; playlists rotate eligible slides.</p></article>
      <article><strong>Venue-local time</strong><p>Meal periods, happy hour, playlists, and promotions are resolved by the server using the venue timezone. The browser is not the scheduling authority.</p></article>
      <article><strong>Safe changes</strong><p>Choose a target before editing. Destructive and live-impact actions require confirmation and report their outcome here.</p></article>
    </div> : null}
    {schedulePanel === "meal-periods" ? <MealPeriodAdministration configuration={configuration} apiKey={accessToken} venueId={venueId} /> : null}
    {schedulePanel === "happy-hour" ? <HappyHourAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      enabled={capabilities.includes("schedule.promotion.automate")}
      showUpgradePrompt={false}
    /> : null}
    {schedulePanel === "playlists" ? <PlaylistAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      screens={screens}
      enabled={capabilities.includes("playlist_rotation")}
      showUpgradePrompt={false}
    /> : null}
    {schedulePanel === "emergency" ? <EmergencyBroadcastAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      screens={screens}
      enabled={capabilities.includes("emergency_broadcast")}
      showUpgradePrompt={false}
    /> : null}
    {schedulePanel === "promotions" ? <DateRangePromotionAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      enabled={scheduling || capabilities.includes("basic_scheduling")}
      showUpgradePrompt={false}
    /> : null}
    </section>
  </div>;
}

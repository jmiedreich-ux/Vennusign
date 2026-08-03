import { useEffect, useState } from "react";
import DateRangePromotionAdministration from "./DateRangePromotionAdministration";
import EmergencyBroadcastAdministration from "./EmergencyBroadcastAdministration";
import HappyHourAdministration from "./HappyHourAdministration";
import MealPeriodAdministration from "./MealPeriodAdministration";
import PlaylistAdministration from "./PlaylistAdministration";
import ScreenManagement from "./ScreenManagement";
import TapListAdministration from "./TapListAdministration";
import ThemeBuilder from "./ThemeBuilder";
import { loadManagedScreens, type ManagedScreen } from "./api";
import type { VenueAdminConfiguration } from "./config";
import "./operations.css";

type Props = {
  configuration: VenueAdminConfiguration;
  accessToken: string;
  venueId: string;
  capabilities: string[];
  maxScreens?: number;
  area: "screens" | "themes" | "schedules" | "tap-list";
};

export default function VenueOperations({
  configuration,
  accessToken,
  venueId,
  capabilities,
  maxScreens,
  area
}: Props) {
  const [screens, setScreens] = useState<ManagedScreen[]>([]);
  const allLayouts = capabilities.includes("all_layouts");
  const scheduling = capabilities.includes("scheduling");

  useEffect(() => {
    if (area !== "schedules") return;
    loadManagedScreens(configuration, accessToken, venueId)
      .then(setScreens)
      .catch(() => setScreens([]));
  }, [accessToken, area, configuration, venueId]);

  if (area === "screens") {
    return <div className="operations-stack">
      <ScreenManagement
        configuration={configuration}
        apiKey={accessToken}
        venueId={venueId}
        allLayoutsEnabled={allLayouts}
        maxScreens={maxScreens}
        videoWallEnabled={capabilities.includes("video_wall")}
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

  return <div className="operations-stack">
    <MealPeriodAdministration configuration={configuration} apiKey={accessToken} venueId={venueId} />
    <HappyHourAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      enabled={capabilities.includes("happy_hour")}
      showUpgradePrompt={false}
    />
    <PlaylistAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      screens={screens}
      enabled={capabilities.includes("playlist_rotation")}
      showUpgradePrompt={false}
    />
    <EmergencyBroadcastAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      screens={screens}
      enabled={capabilities.includes("emergency_broadcast")}
      showUpgradePrompt={false}
    />
    <DateRangePromotionAdministration
      configuration={configuration}
      apiKey={accessToken}
      venueId={venueId}
      enabled={scheduling || capabilities.includes("basic_scheduling")}
      showUpgradePrompt={false}
    />
  </div>;
}

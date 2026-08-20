/*
    Records reaching go-live as an achieved fact instead of re-deriving it from
    the first screen's current status.

    Before this, CustomerOnboardingService computed progress.GoLive as
    "the first screen's Status is Online right now". HeartbeatMonitor marks an
    Online screen Offline after 90 seconds of silence, and the Back Office
    customer entry routing sends anyone without progress.goLive back to
    /onboarding. A venue that powers its displays down overnight was therefore
    returned to the opening checklist every morning, because nothing recorded
    that it had ever gone live.

    GoLiveAchievedUtc is latched once, when the first screen reports Online, and
    is never cleared by a later heartbeat.

    BACKFILL: Screens.LastSeen is NULL until a heartbeat arrives and is only
    ever written by one, so a first screen with a non-NULL LastSeen has provably
    reported in at least once. Existing customers who already reached go-live
    are backfilled from it rather than being asked to onboard again. LastSeen is
    the honest floor for when that happened - the exact first-Online moment was
    never stored, so the backfill deliberately claims the last one it can prove
    instead of inventing an earlier timestamp.
*/

IF COL_LENGTH(N'dbo.CustomerOnboardingStates', N'GoLiveAchievedUtc') IS NULL
    ALTER TABLE dbo.CustomerOnboardingStates ADD GoLiveAchievedUtc DATETIME2(7) NULL;
GO

UPDATE onboarding
SET GoLiveAchievedUtc = screen.LastSeen
FROM dbo.CustomerOnboardingStates AS onboarding
INNER JOIN dbo.Screens AS screen ON screen.Id = onboarding.FirstScreenId
WHERE onboarding.GoLiveAchievedUtc IS NULL
  AND onboarding.FirstScreenId IS NOT NULL
  AND screen.LastSeen IS NOT NULL;
GO

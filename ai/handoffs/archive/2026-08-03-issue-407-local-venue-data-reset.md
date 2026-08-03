# Issue-407 Local Venue Data Reset

## Authorization

The owner explicitly added deletion of all venue data from the local Development SQL database to Issue #407 manual-test preparation. No shared or production environment was authorized.

## Target

- Server: `(localdb)\MSSQLLocalDB`
- Database: `VennuSign`
- Tool: `sqlcmd`

## Result

- Deleted venues: 3
- Deleted venue-assigned screens: 4
- Deleted venue-owned dependent menu, scheduling, screen, pairing, POS, theme, feature-usage, subscription, membership, audit, and operational records.
- Reset `CustomerOnboardingStates.VenueId` and `FirstScreenId` for affected local onboarding state.
- Preserved unassigned player records and global/customer records.

## Verification

- Venues: 0
- Venue-assigned screens: 0
- Onboarding venue/screen links: 0
- Organizations preserved: 1
- Customer users preserved: 1
- Subscription tiers preserved: 5

## Exact Next Action

Restart the API and Venue Admin app, resume onboarding at venue setup, pair the first display, and verify the newly paired screen appears with `1 of 1` quota behavior and correct `video_wall` visibility.

## Restrictions

Do not run this cleanup against shared or production databases. Do not commit or push Issue #407 before owner manual-testing approval.

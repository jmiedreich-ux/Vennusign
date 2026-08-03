# Organization Onboarding Profile

Migration 054 extends `Organizations` with nullable legal name, primary contact, contact email, contact phone, and mailing address fields. Nullable columns are the compatibility path for existing organizations; the migration invents no personal or address data. New onboarding organizations require display name, primary contact name, valid contact email, and mailing address, with legal name and phone clearly optional.

The authenticated customer session supplies the owner. The browser cannot submit an owner or organization identifier. `IdentityMembershipService` normalizes and bounds all profile values before the organization, owner membership, and audit record are created transactionally. The onboarding snapshot reads the profile only through the journey's persisted organization ID.

After authoritative pairing, both paired-offline and Online states expose Open Back Office. Back Office independently rechecks membership and the saved onboarding venue; the link never grants context or entitlement.

Focused validation covers profile normalization/persistence contracts, required UI fields, route safety, and paired transition. Azure SQL, live browser/provider/device, hosted infrastructure, credentials, containers, signing/store, cross-system, and other integration tests remain skipped.

# Database script and table audit — dev, 2026-08-20

Asked for as part of #740: check whether the migration scripts and the tables they
create are all still needed, and remove what is not.

**The short answer is that almost nothing is removable, and the reason is worth
recording so this is not re-asked.** Every table in the dev database is either
holding rows or referenced by product code. The dead weight that does exist is not
dead *schema* — it is one legacy table still wired to live code, one seeded table
for work that is backlogged, and one table that is test residue.

## How this was established

Live dev database (`dev_vennusign`), read directly, 2026-08-20. Row counts from
`sys.partitions`; the code cross-reference is a corpus of 958 `.cs/.ts/.tsx/.ps1/.mjs/.sql`
files under `src`, `tests`, `scripts`, `tools`, excluding `node_modules`, `bin`,
`obj`, `dist`, and the migration scripts themselves — so a table that appears only
in the script that creates it counts as unreferenced.

```
79 tables in the dev database
34 hold rows
45 are empty
45 of those 45 empty tables are referenced by product code
 0 empty tables are unreferenced
```

## Drift, both directions

Every table the migration scripts create exists in the database, and every table in
the database is created by a migration script — **with one exception**:

`dbo.TestRecordTrace` is created by `tests/Vennu.Data.IntegrationTests/Fixtures/DatabaseFixture.cs`,
on demand, if it is missing. No migration script creates it. It is in the dev
product database holding **81 rows from 11 test runs between 2026-08-02 and 2026-08-09**.

That is the residue of integration tests having been pointed at the dev database —
the incident already recorded in the handoff, where a Windows user-level
`VENU_TEST_AZURE_SQL_CONNECTION_STRING` sent every local run at dev.

## Do not drop TestRecordTrace yet

It looks like the obvious removal and it is the one table here that is genuinely not
product schema. But it is also the **only record of what those test runs wrote into
the dev database**, keyed by table and record:

| Table | Action | Rows | Distinct records |
|---|---|---|---|
| Venues | INSERT | 26 | 26 |
| Screens | INSERT | 30 | 30 |
| Screens | UPDATE | 9 | 9 |
| ScreenPairingCodes | INSERT | 12 | 12 |
| ScreenPairingCodes | UPDATE | 4 | 4 |

Dev currently holds 147 Venues, 128 Screens and 29 ScreenPairingCodes. So roughly a
fifth of the venues and a quarter of the screens in the dev database were created by
test runs, and this table is the map for cleaning them up. Dropping it first destroys
the map. **Clean the rows it points at, then drop it.**

This also compounds the unfiled problem in the handoff: every visit to
`dev.display.vennusign.com/pair` registers a new screen and nothing cleans them up.

## Scripts: nothing to remove

Sixteen scripts: `001_baseline.sql` plus `059`–`073`.

`001_baseline.sql` already is the consolidation — it replaced the original scripts
`002`–`058`, and `DatabaseMigrator.BaselineExistingDatabase` exists precisely so that
a database that ran the old chain records the baseline as applied rather than trying
to execute it. Every script since is still required by any database sitting at the
baseline level.

Consolidating `059`–`073` into a second baseline would buy nothing at this count and
would need that same guard written again. Not recommended.

Deleting a migration never un-applies it: DbUp decides what to run by journal name,
so a deleted script simply stops being available to a database that has not run it.

## The three tables worth a decision

**`dbo.MenuItems` — empty, superseded, and still wired.** Zero rows product-wide.
The builder writes `dbo.Items` joined through `dbo.Placements`; this is the table
that made every published menu render empty (#739). The display no longer reads it,
but `MenuRepository` still reads *and writes* it in five places, and
`PosCatalogMappingRepository` joins it. Those paths serve the POS catalog sync
(Clover, Square, Toast) and `MenuSectionManagementService`.

So retiring it is not a `DROP TABLE`. It means deciding what the POS catalog sync
targets, because today it targets a table nothing else writes. `PosConnections` and
`PosCatalogMappings` both hold zero rows, so nothing is using that path in anger yet
— which makes now the cheap moment to redirect it, but it is a feature decision, not
cleanup. **Recommend a separate change, not bundled with a deploy.**

**`dbo.LayoutTemplates` — 8 seeded rows, referenced by nothing.** The only table in
the database with rows and zero references anywhere in product code. Seeded by the
baseline, foreign key to `CapabilityDefinitions`. This looks like pre-seeding for
board layouts — Menus Milestone 5 (Board View and Play) is backlogged as #709.
Dropping it now means re-adding it then. **Recommend leaving it until #709 is
decided.**

**`dbo.AuthorityRoles` — 8 rows, referenced by nothing directly. Keep.** It is the
foreign-key parent of `AuthorityRolePermissions` (195 rows) and `ScopedRoleAssignments`.
Zero code references only means roles are resolved through permissions rather than by
reading the role table. Structurally required.

## The 45 empty-but-wired tables

These are built-ahead features: the schema and the code both exist, no customer has
used them. Removing any of them removes a feature, not dead schema.

`CapabilityAddOnAttachments` `CapabilityAllowanceUsage` `CapabilityRollouts`
`CustomerAuthenticationChallenges` `CustomerPasskeyCredentials` `CustomerRecoveryCodes`
`CustomerTotpAuthenticators` `DateRangePromotions` `EmailLoginTokens` `EmergencyBroadcasts`
`FeatureMatrixAudit` `FeatureUsages` `HaasContracts` `HappyHourSchedules` `MealPeriods`
`MenuImportAnswers` `MenuImportCandidates` `MenuImportCreatedLines` `MenuImportQuestionLines`
`MenuImportReplacementSnapshots` `MenuImportReviewQuestions` `MenuImportSessions`
`MenuImportSourceLines` `MenuItems` `OperationalEvents` `OrganizationCapabilityEntitlements`
`PlaylistSlides` `PosCatalogMappings` `PosConnections` `PosWebhookEvents`
`ProcessedStripeEvents` `RevenueDailySnapshots` `ScopedRoleAssignments`
`ScreenContentDeliveries` `ScreenReplacementAudits` `SupportAccessAuditEntries`
`SupportAccessGrants` `SystemConfigurationAudit` `SystemConfigurationRevisions`
`SystemConfigurationValues` `TapCategories` `TapItems` `VenueFeatureOverrides`
`VenueMemberships` `VenueThemes`

Two of them are worth a second look, not because the schema is wrong but because of what
the emptiness says about coverage. All eight Menus paste-import tables are empty, so
6-A1/6-A2/6-A3 have never been exercised against dev — their acceptance was local.
And `ScreenContentDeliveries` is empty while `MenuPublishTargets` holds 102 rows, which
is worth confirming is intended rather than a second projection reading a table nothing
writes, since that is exactly the shape of #739.

## What was changed

Nothing. No table was dropped and no script was deleted. Every candidate either turned
out to be load-bearing or needs a decision this audit is not entitled to make.

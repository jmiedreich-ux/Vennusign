# Retire `dbo.MenuItems` — reconcile the POS catalog sync with the real content model

**Status: Proposed — not yet approved.** Repository presence does not constitute design approval. Nothing here is scheduled or started. Parked by owner decision, 2026-08-23, until it is time to discuss it — not a bug, a decision.

Source: issue #744, filed from `docs/reports/database-schema-audit-2026-08-20.md`.

## What is true

`dbo.MenuItems` holds **zero rows** in the entire dev database. The builder writes `dbo.Items` joined to a board through `dbo.Placements` — the model the builder and display actually agree on. `dbo.MenuItems` is the table that made every published menu render empty (#739) before the display was pointed at published snapshots.

The display no longer reads it. But the legacy model is still live in code:

- `src/Vennu.Data/Repositories/MenuRepository.cs` reads **and writes** it at lines 76, 163, 173, 186, 190, 197 — including an `UPDATE dbo.MenuItems`.
- `src/Vennu.Data/Repositories/PosCatalogMappingRepository.cs:29` joins it.

Those paths serve the POS catalog sync (`CloverRealtimeSyncHandler`, `SquareRealtimeSyncHandler`, `ToastInventorySyncService`, `PosCatalogImportService`) and `MenuSectionManagementService`.

## Why this matters

The POS catalog sync currently writes item data into a table that nothing else in the product reads, and reads item availability from a table the builder never populates. If a customer connected a POS today, the imported catalogue would land somewhere the board cannot see — the same defect shape as #739, in a feature that has not been exercised yet. `PosConnections` and `PosCatalogMappings` both hold zero rows, so nothing is using this path in anger today. That makes now the cheap moment to redirect it, before real POS data is riding on it.

## The decision to make

Either:

1. **Point the POS catalog sync at `Items`/`Placements`** — the model the builder and display already agree on, or
2. **Decide the POS integration is not live** and remove both it and `dbo.MenuItems` together.

What is not viable is leaving a second content model wired to live code — that is precisely the arrangement that produced #739. Whichever way it goes, `dbo.MenuItems` should not survive it.

## Evidence

`docs/reports/database-schema-audit-2026-08-20.md` — full audit against the live dev database, 2026-08-20. Row counts, the code cross-reference method, and the two other tables that needed a decision (`LayoutTemplates`, `AuthorityRoles`).

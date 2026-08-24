-- Adds the "Available" toggle: a drafted, publish-gated flag distinct from the
-- existing immediate 86 mechanism (dbo.ItemAvailability). Defaults to listed so
-- every existing item's guest-facing state is unchanged by this migration.
ALTER TABLE dbo.Items ADD IsListed BIT NOT NULL CONSTRAINT DF_Items_IsListed DEFAULT (1);

using Vennu.Core.Models;
using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// The Menus spine against a real database. These exist because the earlier
/// milestone tests asserted that strings appeared in a SQL script, which proves
/// the script says something, not that the database does it. Everything here runs
/// the statements and reads back what actually happened.
/// </summary>
[Trait("Category", "Integration")]
public class MenuSpineIntegrationTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture = fixture;

    // ---- migration outcomes ---------------------------------------------------

    [Fact]
    public async Task Migration_CreatesTheSpineWithTenantOwnershipEnforced()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();

        foreach (var table in new[]
        {
            "Items", "Placements", "ItemAvailability", "MenuScreenAssignments",
            "MenuDraftChanges", "MenuPublishEvents", "MenuPublishTargets", "MenuHistoryEntries"
        })
        {
            var found = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
                "SELECT COUNT(*) AS Value FROM sys.tables WHERE name = @Name;",
                new { Name = table })).Single().Value;
            Assert.True(found == 1, $"Expected table dbo.{table} to exist.");
        }

        // The composite foreign keys are what make a foreign menu id impossible,
        // rather than merely discouraged.
        var composite = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
            WHERE fk.name IN ('FK_Placements_Menus','FK_Placements_Items','FK_MenuDraftChanges_Menus','FK_MenuHistoryEntries_Menus')
            GROUP BY fk.name
            HAVING COUNT(*) = 2;
            """,
            new { })).Count();
        Assert.True(composite >= 4, "Every child should reach its parent through a two-column (id, venue) key.");
    }

    [Fact]
    public async Task Migration_LeavesTheLibraryEmptyAndDoesNotCarryLegacyContent()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();

        // Q45 is a fresh start: legacy rows stay where they are.
        var legacy = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "SELECT COUNT(*) AS Value FROM dbo.MenuItems;", new { })).Single().Value;
        var carried = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value FROM dbo.Items i
            WHERE EXISTS (SELECT 1 FROM dbo.MenuItems mi WHERE mi.Id = i.Id);
            """, new { })).Single().Value;

        Assert.True(carried == 0, $"The library must not carry legacy items ({legacy} legacy rows exist, {carried} were copied).");
    }

    [Fact]
    public async Task Migration_StoresAPriceExactlyAsTyped()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);

        foreach (var typed in new[] { "12", "9.5", "9.50", "MP" })
        {
            var id = await repository.CreateItemAsync(new Item
            {
                VenueId = venueId,
                Name = fixture.UniqueValue($"price-{typed}"),
                Price = typed
            });

            var stored = await repository.GetItemAsync(venueId, id);
            // "9.5" must not become "9.50", and "MP" must survive at all.
            Assert.Equal(typed, stored!.Price);
        }
    }

    // ---- cross-tenant access ----------------------------------------------------

    [Fact]
    public async Task ForeignMenuId_CannotBeQueuedAgainstAnotherVenue()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);

        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuB = await SeedMenuAsync(dataAccess, venueB);

        // Venue A submits venue B's menu id.
        await Assert.ThrowsAnyAsync<Exception>(() => repository.UpsertDraftChangeAsync(new MenuDraftChange
        {
            VenueId = venueA,
            MenuId = menuB,
            TargetKind = DraftTargetKinds.Menu,
            Field = "theme",
            BeforeValue = "coastal",
            AfterValue = "classic-dark"
        }));

        // And nothing was written against B either way.
        var leaked = await repository.GetDraftChangesAsync(venueB, menuB);
        Assert.Empty(leaked);
    }

    [Fact]
    public async Task ForeignMenuId_CannotBePublishedByAnotherVenue()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);

        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuB = await SeedMenuAsync(dataAccess, venueB);

        await Assert.ThrowsAnyAsync<Exception>(() => repository.PublishAsync(new MenuPublishEvent
        {
            VenueId = venueA,
            MenuId = menuB,
            PublishedUtc = DateTime.UtcNow
        }));

        // B's version line is untouched: A did not consume a version number.
        Assert.Empty(await repository.GetPublishHistoryAsync(venueB, menuB, 10));
    }

    // ---- publish behaviour --------------------------------------------------------

    [Fact]
    public async Task Publish_CapturesTheQueueItRemovesAndSnapshotsTheContent()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        await repository.UpsertDraftChangeAsync(new MenuDraftChange
        {
            VenueId = venueId,
            MenuId = menuId,
            TargetKind = DraftTargetKinds.Menu,
            Field = "theme",
            BeforeValue = "coastal",
            AfterValue = "classic-dark"
        });

        var published = await repository.PublishAsync(new MenuPublishEvent
        {
            VenueId = venueId,
            MenuId = menuId,
            Author = "Reviewer",
            PublishedUtc = DateTime.UtcNow
        });

        // The count is what actually shipped, and the shipped set is retained.
        Assert.Equal(1, published.ChangeCount);
        Assert.False(string.IsNullOrWhiteSpace(published.ShippedChanges));
        Assert.Contains("classic-dark", published.ShippedChanges!, StringComparison.Ordinal);

        // The published content itself is stored, so this version can be rendered
        // and restored without the queue that produced it.
        Assert.False(string.IsNullOrWhiteSpace(published.Snapshot));
        Assert.Contains("menuId", published.Snapshot!, StringComparison.OrdinalIgnoreCase);

        Assert.Empty(await repository.GetDraftChangesAsync(venueId, menuId));
    }

    [Fact]
    public async Task Publish_UnderConcurrency_NeverDeletesAChangeItDidNotCount()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        for (var i = 0; i < 8; i++)
        {
            await repository.UpsertDraftChangeAsync(new MenuDraftChange
            {
                VenueId = venueId,
                MenuId = menuId,
                TargetKind = DraftTargetKinds.Item,
                TargetId = Guid.NewGuid(),
                Field = "price",
                BeforeValue = "1",
                AfterValue = "2"
            });
        }

        // Publish while more edits arrive. Whatever the interleaving, every change
        // that was destroyed must appear in a publish's count.
        var writer = Task.Run(async () =>
        {
            for (var i = 0; i < 8; i++)
            {
                await repository.UpsertDraftChangeAsync(new MenuDraftChange
                {
                    VenueId = venueId,
                    MenuId = menuId,
                    TargetKind = DraftTargetKinds.Item,
                    TargetId = Guid.NewGuid(),
                    Field = "price",
                    BeforeValue = "1",
                    AfterValue = "3"
                });
            }
        });

        var first = await repository.PublishAsync(new MenuPublishEvent
        { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow });
        await writer;
        var second = await repository.PublishAsync(new MenuPublishEvent
        { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow });

        var remaining = (await repository.GetDraftChangesAsync(venueId, menuId)).Count;
        Assert.Equal(16, first.ChangeCount + second.ChangeCount + remaining);
    }

    [Fact]
    public async Task Draft_RemovesAChangeTakenBackToThePublishedValue()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var itemId = Guid.NewGuid();

        await repository.UpsertDraftChangeAsync(new MenuDraftChange
        {
            VenueId = venueId, MenuId = menuId, TargetKind = DraftTargetKinds.Item,
            TargetId = itemId, Field = "price", BeforeValue = "12", AfterValue = "13"
        });
        Assert.Single(await repository.GetDraftChangesAsync(venueId, menuId));

        await repository.UpsertDraftChangeAsync(new MenuDraftChange
        {
            VenueId = venueId, MenuId = menuId, TargetKind = DraftTargetKinds.Item,
            TargetId = itemId, Field = "price", BeforeValue = "12", AfterValue = "12"
        });

        // Q182: what no longer differs is not a change.
        Assert.Empty(await repository.GetDraftChangesAsync(venueId, menuId));
    }

    [Fact]
    public async Task DiscardingADraft_WritesItsHistoryInTheSameTransaction()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        await repository.UpsertDraftChangeAsync(new MenuDraftChange
        {
            VenueId = venueId, MenuId = menuId, TargetKind = DraftTargetKinds.Menu,
            Field = "theme", BeforeValue = "coastal", AfterValue = "classic-dark"
        });

        var removed = await repository.ClearDraftAsync(venueId, menuId, "Reviewer", recordHistory: true);

        Assert.Equal(1, removed);
        var history = await repository.GetHistoryAsync(venueId, menuId, 10);
        var entry = Assert.Single(history, h => h.Kind == MenuHistoryKinds.DraftDiscarded);
        Assert.Equal("Reviewer", entry.Author);
    }

    // ---- ceilings -------------------------------------------------------------------

    [Fact]
    public async Task Ceilings_PreferTheVenueRowOverTheOrganizationRow()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);

        var organizationId = (await dataAccess.ExecuteSqlQueryAsync<GuidRow, object>(
            "SELECT OrganizationId AS Value FROM dbo.Venues WHERE Id = @VenueId;",
            new { VenueId = venueId })).Single().Value;

        // An organization ceiling of 10, deliberately raised to 100 for this venue.
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            DELETE FROM dbo.CapabilityAllowances WHERE VenueId = @VenueId AND CapabilityId = 'content.menu.count';
            INSERT dbo.CapabilityAllowances (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc, EndsUtc)
            VALUES (NEWID(), @OrganizationId, NULL, 'content.menu.count', 10, DATEADD(day,-1,SYSUTCDATETIME()), NULL),
                   (NEWID(), @OrganizationId, @VenueId, 'content.menu.count', 100, DATEADD(day,-1,SYSUTCDATETIME()), NULL);
            SELECT 1 AS Value;
            """,
            new { VenueId = venueId, OrganizationId = organizationId });

        var ceilings = await repository.GetCeilingsAsync(venueId);

        // The narrower organization row must not silently win.
        Assert.Equal(100, ceilings["content.menu.count"]);
    }

    // ---- helpers ----------------------------------------------------------------------

    private async Task<Guid> SeedVenueAsync(SqlDataAccess dataAccess)
    {
        var organizationId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.CustomerUsers (Id, Email, NormalizedEmail, DisplayName, Status, CreatedUtc, UpdatedUtc)
            VALUES (@OwnerUserId, @Name + '@example.test', UPPER(@Name + '@example.test'), @Name, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
            INSERT dbo.Organizations (Id, Name, OwnerUserId, CreatedUtc, UpdatedUtc)
            VALUES (@OrganizationId, @Name, @OwnerUserId, SYSUTCDATETIME(), SYSUTCDATETIME());
            INSERT dbo.Venues (Id, OrganizationId, Name, Type, Timezone, PrimaryLanguage, CreatedUtc, UpdatedUtc)
            VALUES (@VenueId, @OrganizationId, @Name, 'Bar', 'America/New_York', 'en', SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT 1 AS Value;
            """,
            new { OrganizationId = organizationId, VenueId = venueId, OwnerUserId = Guid.NewGuid(), Name = fixture.UniqueValue("spine") });
        return venueId;
    }

    private async Task<Guid> SeedMenuAsync(SqlDataAccess dataAccess, Guid venueId)
    {
        var menuId = Guid.NewGuid();
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.Menus (Id, VenueId, Name, IsActive, CreatedUtc, UpdatedUtc)
            VALUES (@MenuId, @VenueId, @Name, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT 1 AS Value;
            """,
            new { MenuId = menuId, VenueId = venueId, Name = fixture.UniqueValue("menu") });
        return menuId;
    }

    private sealed class CountRow
    {
        public int Value { get; set; }
    }

    private sealed class GuidRow
    {
        public Guid Value { get; set; }
    }
}

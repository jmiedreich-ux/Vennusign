using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// The derived save model against a real database. The draft is computed, never
/// stored, so what these prove is that the SQL-built snapshots are the truth the
/// model needs: they parse with the restore model, they contain the values that
/// were edited, and every write path refuses to cross a tenant or a ceiling.
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
            "MenuPublishEvents", "MenuPublishTargets", "MenuHistoryEntries"
        })
        {
            var found = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
                "SELECT COUNT(*) AS Value FROM sys.tables WHERE name = @Name;",
                new { Name = table })).Single().Value;
            Assert.True(found == 1, $"Expected table dbo.{table} to exist.");
        }

        // The draft is derived (owner decision 2026-08-09): there must be no
        // stored draft queue for it to disagree with.
        var draftTable = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "SELECT COUNT(*) AS Value FROM sys.tables WHERE name = 'MenuDraftChanges';",
            new { })).Single().Value;
        Assert.Equal(0, draftTable);

        // Review finding #5: the tenant invariant must cover the indirect
        // relationships, not only direct parents. Each of these keys makes one
        // cross-tenant (or cross-menu) reference impossible at the schema layer.
        foreach (var (name, columns) in new[]
        {
            ("FK_Placements_Menus", 2),
            ("FK_Placements_Items", 2),
            ("FK_Placements_SectionOnMenu", 2),
            ("FK_MenuPublishTargets_Event", 2),
            ("FK_MenuPublishTargets_Screens", 2),
            ("FK_MenuHistoryEntries_Menus", 2),
            ("FK_MenuHistoryEntries_PublishEvent", 3)
        })
        {
            var width = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
                """
                SELECT COUNT(*) AS Value
                FROM sys.foreign_keys fk
                JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                WHERE fk.name = @Name;
                """,
                new { Name = name })).Single().Value;
            Assert.True(width == columns, $"Expected {name} to exist with {columns} columns, found {width}.");
        }
    }

    [Fact]
    public async Task Migration_LeavesTheLibraryEmptyAndDoesNotCarryLegacyContent()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();

        // Q45 is a fresh start: legacy rows stay where they are.
        var carried = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value FROM dbo.Items i
            WHERE EXISTS (SELECT 1 FROM dbo.MenuItems mi WHERE mi.Id = i.Id);
            """, new { })).Single().Value;

        Assert.Equal(0, carried);
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

    // ---- the derived draft over real SQL ---------------------------------------

    // Review finding #3's regression: the SQL-built snapshot must parse with the
    // exact model restore and the draft depend on — nested arrays as arrays,
    // never as escaped strings.
    [Fact]
    public async Task WorkingSnapshot_ParsesWithTheRestoreModelIncludingNestedContent()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);

        var outcome = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("oysters"), Price = "MP" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        Assert.Equal(ItemPlacementOutcomes.Created, outcome.Outcome);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId });

        var parsed = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId));

        Assert.NotNull(parsed);
        Assert.Equal(menuId, parsed!.MenuId);
        var section = Assert.Single(parsed.Sections!);
        Assert.Equal(sectionId, section.SectionId);
        var item = Assert.Single(section.Items!);
        Assert.Equal("MP", item.Price);
        Assert.Equal(screenId, Assert.Single(parsed.Screens!).ScreenId);
    }

    // Review finding #9: the publish test must prove the stored snapshot carries
    // the edited value, by deserializing it — not by scanning for a substring.
    [Fact]
    public async Task Publish_StoresADeserializableSnapshotContainingTheEdit()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId });

        var created = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("lemonade"), Price = "9.5" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        var published = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, Author = "Reviewer", PublishedUtc = DateTime.UtcNow },
            changeCount: 1,
            shippedChanges: "[]");

        Assert.Equal(1, published.Version);
        Assert.Equal("[]", published.ShippedChanges);

        var snapshot = MenuSnapshot.Parse(published.Snapshot);
        Assert.NotNull(snapshot);
        var item = Assert.Single(Assert.Single(snapshot!.Sections!).Items!);
        Assert.Equal("9.5", item.Price);
        Assert.Equal(screenId, Assert.Single(snapshot.Screens!).ScreenId);

        // One delivery row per target, carrying the venue so a cross-tenant
        // target is impossible at the schema layer.
        var target = Assert.Single(await repository.GetPublishTargetsAsync(published.Id));
        Assert.Equal(screenId, target.ScreenId);
    }

    // Q182 at the database layer: the draft is the difference between the working
    // rows and the published snapshot, so an edit taken back is not a change and
    // no client-supplied before-value is involved anywhere.
    [Fact]
    public async Task Draft_IsTheDifferenceBetweenWorkingStateAndPublishedSnapshot()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId });
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("fizz"), Price = "12" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow }, 0, "[]");

        var item = (await repository.GetItemsAsync(venueId)).Single(candidate => candidate.Price == "12");
        item.Price = "13";
        Assert.True(await repository.UpdateItemAsync(item));

        var published = await repository.GetLatestPublishedSnapshotAsync(venueId, menuId);
        var change = Assert.Single(MenuSnapshot.Diff(published, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
        Assert.Equal("price", change.Field);
        Assert.Equal("12", change.BeforeValue);
        Assert.Equal("13", change.AfterValue);

        item.Price = "12";
        Assert.True(await repository.UpdateItemAsync(item));
        Assert.Empty(MenuSnapshot.Diff(published, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
    }

    // ---- publish behaviour ------------------------------------------------------

    // Q80, enforced inside the transaction: a publish that can reach nothing and
    // has nothing to release is a named refusal, never a silent version bump.
    [Fact]
    public async Task Publish_RefusesInsideTheTransactionWhenItWouldReachNothing()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        var refusal = await Assert.ThrowsAnyAsync<Exception>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow }, 0, "[]"));

        Assert.Contains("Pair a screen", refusal.Message, StringComparison.Ordinal);
        Assert.Empty(await repository.GetPublishHistoryAsync(venueId, menuId, 10));
    }

    // Q68: take-off waits as a difference in which screens the menu is on, and the
    // publish that ships it still tells the screens being released.
    [Fact]
    public async Task TakeOff_WaitsInTheDraftAndReleasedScreensAreStillTold()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId });
        var first = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow }, 0, "[]");

        Assert.Equal(1, await repository.ClearMenuAssignmentsAsync(venueId, menuId));

        // The removal is now a pending difference, not an applied one.
        var published = await repository.GetLatestPublishedSnapshotAsync(venueId, menuId);
        var change = Assert.Single(MenuSnapshot.Diff(published, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
        Assert.Equal(DraftTargetKinds.Screens, change.TargetKind);

        var second = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow }, 1, "[]");

        // The released screen is targeted by the publish that releases it, so it
        // is told to stop showing the menu rather than left on stale content.
        var target = Assert.Single(await repository.GetPublishTargetsAsync(second.Id));
        Assert.Equal(screenId, target.ScreenId);
        Assert.Empty(MenuSnapshot.Parse((await repository.GetPublishEventAsync(venueId, menuId, second.Version))!.Snapshot)!.Screens ?? []);
        Assert.NotEqual(first.Id, second.Id);
    }

    // ---- restore ---------------------------------------------------------------

    // Q67/Q43: restore puts values back onto the rows that already exist, brings
    // back removed placements and assignments, and records the act in the same
    // transaction. After restoring to a version, the draft against it is empty.
    [Fact]
    public async Task Restore_PutsTheWholeShapeBackAndRecordsTheActTogether()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId });
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("stout"), Price = "8" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        var version = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow }, 0, "[]");

        // Drift in every dimension: item value, menu value, placement, screens.
        var item = (await repository.GetItemsAsync(venueId)).Single(candidate => candidate.Price == "8");
        item.Price = "9";
        await repository.UpdateItemAsync(item);
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "UPDATE dbo.Menus SET Name = @Name WHERE Id = @MenuId; SELECT 1 AS Value;",
            new { Name = fixture.UniqueValue("renamed"), MenuId = menuId });
        var placement = (await repository.GetPlacementsAsync(venueId, menuId)).Single();
        await repository.RemovePlacementAsync(venueId, placement.Id);
        await repository.ClearMenuAssignmentsAsync(venueId, menuId);

        await repository.RestoreSnapshotAsync(
            venueId, menuId, version.Snapshot!, "Reviewer", "Went back to version 1.", DateTime.UtcNow);

        // The strongest statement available: after the restore, nothing differs
        // from the version restored to.
        Assert.Empty(MenuSnapshot.Diff(version.Snapshot, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
        var entry = Assert.Single(await repository.GetHistoryAsync(venueId, menuId, 10), h => h.Kind == MenuHistoryKinds.Restored);
        Assert.Equal("Reviewer", entry.Author);
    }

    [Fact]
    public async Task Restore_AgainstAForeignVenue_ChangesNothingAndRecordsNothing()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuB = await SeedMenuAsync(dataAccess, venueB);
        var sectionB = await SeedSectionAsync(dataAccess, venueB, menuB);
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueB, Name = fixture.UniqueValue("held"), Price = "5" },
            menuB, sectionB, itemsPerMenuLimit: 500);
        var before = await repository.GetWorkingSnapshotAsync(venueB, menuB);

        await Assert.ThrowsAnyAsync<Exception>(() => repository.RestoreSnapshotAsync(
            venueA, menuB, before!, "Intruder", "Cross-tenant restore.", DateTime.UtcNow));

        Assert.Equal(before, await repository.GetWorkingSnapshotAsync(venueB, menuB));
        Assert.Empty(await repository.GetHistoryAsync(venueB, menuB, 10));
    }

    // ---- cross-tenant access ----------------------------------------------------

    [Fact]
    public async Task ForeignMenuId_CannotBePublishedByAnotherVenue()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuB = await SeedMenuAsync(dataAccess, venueB);

        await Assert.ThrowsAnyAsync<Exception>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueA, MenuId = menuB, PublishedUtc = DateTime.UtcNow }, 0, "[]"));

        // B's version line is untouched: A did not consume a version number.
        Assert.Empty(await repository.GetPublishHistoryAsync(venueB, menuB, 10));
    }

    // Review finding #5: the item update names the venue in its WHERE clause, so
    // venue A carrying venue B's item id changes nothing.
    [Fact]
    public async Task UpdateItemAsync_CannotReachAForeignVenueItem()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var name = fixture.UniqueValue("bisque");
        var itemB = await repository.CreateItemAsync(new Item { VenueId = venueB, Name = name, Price = "14" });

        var updated = await repository.UpdateItemAsync(new Item
        {
            Id = itemB,
            VenueId = venueA,
            Name = "Hijacked",
            Price = "0"
        });

        Assert.False(updated);
        var stored = await repository.GetItemAsync(venueB, itemB);
        Assert.Equal(name, stored!.Name);
        Assert.Equal("14", stored.Price);
    }

    // Review finding #5: a placement must prove its section sits on its own menu.
    // Same venue, different menu — the schema itself refuses.
    [Fact]
    public async Task Placement_CannotReferenceASectionOfAnotherMenu()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueId);
        var menuB = await SeedMenuAsync(dataAccess, venueId);
        var sectionOnB = await SeedSectionAsync(dataAccess, venueId, menuB);
        var itemId = await repository.CreateItemAsync(new Item { VenueId = venueId, Name = fixture.UniqueValue("stray") });

        await Assert.ThrowsAnyAsync<Exception>(() => repository.CreatePlacementAsync(new Placement
        {
            VenueId = venueId,
            MenuId = menuA,
            MenuSectionId = sectionOnB,
            ItemId = itemId,
            SortOrder = 0
        }));

        Assert.Empty(await repository.GetPlacementsAsync(venueId, menuA));
    }

    // Review finding #5: a publish target carries its venue, and both the event
    // and the screen must belong to it.
    [Fact]
    public async Task PublishTarget_CannotNameAnotherVenuesScreen()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueA);
        var screenA = await SeedScreenAsync(dataAccess, venueA);
        var screenB = await SeedScreenAsync(dataAccess, venueB);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueA, ScreenId = screenA, MenuId = menuA });
        var published = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueA, MenuId = menuA, PublishedUtc = DateTime.UtcNow }, 0, "[]");

        await Assert.ThrowsAnyAsync<Exception>(() => dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.MenuPublishTargets (Id, VenueId, PublishEventId, ScreenId, State, UpdatedUtc)
            VALUES (NEWID(), @VenueId, @EventId, @ScreenId, N'Pending', SYSUTCDATETIME());
            SELECT 1 AS Value;
            """,
            new { VenueId = venueA, EventId = published.Id, ScreenId = screenB }));
    }

    // Review finding #5: history naming a publish event must name one of its own
    // menu's events — another menu's event id is refused by the schema.
    [Fact]
    public async Task HistoryEntry_CannotNameAnotherMenusPublishEvent()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueId);
        var menuB = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuA });
        var publishedOnA = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuA, PublishedUtc = DateTime.UtcNow }, 0, "[]");

        await Assert.ThrowsAnyAsync<Exception>(() => repository.RecordHistoryAsync(new MenuHistoryEntry
        {
            VenueId = venueId,
            MenuId = menuB,
            Kind = MenuHistoryKinds.Published,
            PublishEventId = publishedOnA.Id,
            OccurredUtc = DateTime.UtcNow
        }));
    }

    // ---- ceilings -------------------------------------------------------------------

    // Q201 and review finding #8: the count and the insert hold one lock, and a
    // put-away menu genuinely makes room — the refusal's own advice works.
    [Fact]
    public async Task MenuCeiling_CountsOnlyActiveMenus_SoPuttingOneAwayMakesRoom()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        var refused = await repository.CreateMenuWithinCeilingAsync(
            new Menu { VenueId = venueId, Name = fixture.UniqueValue("second") }, activeMenuLimit: 1);
        Assert.False(refused.Created);
        Assert.Equal(1, refused.ActiveMenuCount);

        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "UPDATE dbo.Menus SET IsPutAway = 1 WHERE Id = @MenuId; SELECT 1 AS Value;",
            new { MenuId = menuId });

        var admitted = await repository.CreateMenuWithinCeilingAsync(
            new Menu { VenueId = venueId, Name = fixture.UniqueValue("second") }, activeMenuLimit: 1);
        Assert.True(admitted.Created);
    }

    [Fact]
    public async Task MenuCeiling_UnderConcurrency_AdmitsExactlyTheRoomLeft()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        await SeedMenuAsync(dataAccess, venueId);

        // One seat left. Two concurrent requests race for it; the lock inside the
        // statement means exactly one wins, never both.
        var results = await Task.WhenAll(
            repository.CreateMenuWithinCeilingAsync(new Menu { VenueId = venueId, Name = fixture.UniqueValue("race-a") }, 2),
            repository.CreateMenuWithinCeilingAsync(new Menu { VenueId = venueId, Name = fixture.UniqueValue("race-b") }, 2));

        Assert.Equal(1, results.Count(result => result.Created));
        Assert.Equal(2, await repository.CountMenusAsync(venueId));
    }

    [Fact]
    public async Task ItemCeiling_RefusesAtomicallyAndTheSectionMustBeOnTheMenu()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);

        var first = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("one") }, menuId, sectionId, itemsPerMenuLimit: 1);
        Assert.Equal(ItemPlacementOutcomes.Created, first.Outcome);

        var over = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("two") }, menuId, sectionId, itemsPerMenuLimit: 1);
        Assert.Equal(ItemPlacementOutcomes.OverCeiling, over.Outcome);
        Assert.Equal(1, over.ItemCountOnMenu);
        Assert.Equal(1, await repository.CountItemsOnMenuAsync(venueId, menuId));

        var missing = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("three") }, menuId, Guid.NewGuid(), itemsPerMenuLimit: 500);
        Assert.Equal(ItemPlacementOutcomes.SectionMissing, missing.Outcome);
    }

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

    // ---- editor reads -----------------------------------------------------------

    [Fact]
    public async Task PlacedItems_CarryLiveAvailability()
    {
        if (!fixture.IsAvailable) { return; }
        var dataAccess = fixture.CreateDataAccess();
        var repository = new MenuLibraryRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("nachos"), Price = "11" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        var itemId = (await repository.GetItemsAsync(venueId)).Single().Id;

        var beforeToggle = Assert.Single(await repository.GetPlacedItemsForVenueAsync(venueId));
        Assert.True(beforeToggle.IsAvailable);

        await repository.SetAvailabilityAsync(new ItemAvailability
        {
            VenueId = venueId,
            ItemId = itemId,
            IsAvailable = false,
            ChangedUtc = DateTime.UtcNow,
            ChangedBy = "Chef"
        });

        var afterToggle = Assert.Single(await repository.GetPlacedItemsForVenueAsync(venueId));
        Assert.False(afterToggle.IsAvailable);
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

    private async Task<Guid> SeedSectionAsync(SqlDataAccess dataAccess, Guid venueId, Guid menuId)
    {
        var sectionId = Guid.NewGuid();
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.MenuSections (Id, VenueId, MenuId, Name, SortOrder, IsActive, CreatedUtc, UpdatedUtc)
            VALUES (@SectionId, @VenueId, @MenuId, @Name, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
            SELECT 1 AS Value;
            """,
            new { SectionId = sectionId, VenueId = venueId, MenuId = menuId, Name = fixture.UniqueValue("section") });
        return sectionId;
    }

    private async Task<Guid> SeedScreenAsync(SqlDataAccess dataAccess, Guid venueId)
    {
        var screenId = Guid.NewGuid();
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.Screens (Id, VenueId, ScreenKey, Name, Location, Status, Platform, AppVersion)
            VALUES (@ScreenId, @VenueId, @ScreenKey, @Name, N'North wall', N'Offline', N'web', N'm1-tests');
            SELECT 1 AS Value;
            """,
            new { ScreenId = screenId, VenueId = venueId, ScreenKey = fixture.UniqueScreenKey(), Name = fixture.UniqueValue("screen") });
        return screenId;
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

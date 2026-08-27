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
public class ContentIntegrationTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture = fixture;

    // ---- migration outcomes ---------------------------------------------------

    [Fact]
    public async Task Migration_CreatesTheContentModelWithTenantOwnershipEnforced()
    {
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

    /// <summary>
    /// Migration 061. Two rules the builder leans on, asserted against the schema
    /// itself rather than against a component that happens to honour them.
    /// </summary>
    [Fact]
    public async Task Migration_DropsTheSectionArchiveFlagAndEnforcesOnePlacementPerPage()
    {
        var dataAccess = fixture.CreateDataAccess();

        // Sections are deleted, not archived (Q96). The column is gone, so no future
        // writer of 0 can quietly take a section off a live board behind a filter.
        var archiveFlag = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value FROM sys.columns
            WHERE object_id = OBJECT_ID(N'dbo.MenuSections', N'U') AND name = N'IsActive';
            """,
            new { })).Single().Value;
        Assert.Equal(0, archiveFlag);

        foreach (var (constraint, expected) in new[]
        {
            ("UQ_Placements_MenuItem", 0),
            ("UQ_Placements_PageItem", 1),
            ("UQ_Placements_SectionItem", 0)
        })
        {
            var found = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
                "SELECT COUNT(*) AS Value FROM sys.key_constraints WHERE name = @Name;",
                new { Name = constraint })).Single().Value;
            Assert.True(found == expected, $"Expected {constraint} count {expected}, found {found}.");
        }
    }

    /// <summary>
    /// Q112's promise - picking an item already on this board jumps to it rather
    /// than placing a second copy - enforced where it is enforceable. The UI rule
    /// is the pleasant version; this is the one that survives a second editor, a
    /// retry, and a caller that never read the design.
    /// </summary>
    [Fact]
    public async Task Placing_TheSameItemTwiceOnOneBoard_IsRefusedBySchema()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var starters = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var mains = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 1);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("olives"), Price = "7" };
        await repository.CreateItemOnMenuAsync(item, menuId, starters, itemsPerMenuLimit: 500);

        // A different section of the same menu. Legal before 061, and it would have
        // rendered the same item twice to a guest.
        await Assert.ThrowsAnyAsync<Exception>(() => repository.CreatePlacementAsync(new Placement
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            MenuId = menuId,
            MenuSectionId = mains,
            ItemId = item.Id,
            SortOrder = 0
        }));

        var placements = await repository.GetPlacementsAsync(venueId, menuId);
        Assert.Single(placements.Where(placement => placement.ItemId == item.Id));
    }

    [Fact]
    public async Task Migration_LeavesTheLibraryEmptyAndDoesNotCarryLegacyContent()
    {
        var dataAccess = fixture.CreateDataAccess();

        // Q45 is a fresh start: legacy rows stay where they are.
        var carried = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value FROM dbo.Items i
            WHERE EXISTS (SELECT 1 FROM dbo.MenuItems mi WHERE mi.Id = i.Id);
            """, new { })).Single().Value;

        Assert.Equal(0, carried);
    }

    // Migration 059. The column used to be NOT NULL DEFAULT N'coastal': it forbade
    // the valid unthemed state (Q86) and defaulted to a look nobody built.
    [Fact]
    public async Task Migration_LeavesTheThemeNullableWithNoDefaultAndNoInventedValue()
    {
        var dataAccess = fixture.CreateDataAccess();

        var nullable = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value
            FROM sys.columns
            WHERE object_id = OBJECT_ID(N'dbo.Menus', N'U') AND name = 'Theme' AND is_nullable = 1;
            """, new { })).Single().Value;
        Assert.True(nullable == 1, "dbo.Menus.Theme must be nullable: a menu with no theme attached is a valid state.");

        var defaults = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT COUNT(*) AS Value
            FROM sys.default_constraints dc
            JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
            WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Menus', N'U') AND c.name = 'Theme';
            """, new { })).Single().Value;
        Assert.True(defaults == 0, "dbo.Menus.Theme must carry no default: there is no named look to default to.");

        // The fiction is gone from the rows and from every snapshot that recorded it.
        var fiction = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            SELECT
                (SELECT COUNT(*) FROM dbo.Menus WHERE Theme = N'coastal')
              + (SELECT COUNT(*) FROM dbo.MenuPublishEvents
                 WHERE ISJSON(Snapshot) = 1 AND JSON_VALUE(Snapshot, '$.theme') = N'coastal') AS Value;
            """, new { })).Single().Value;
        Assert.True(fiction == 0, "No menu or stored snapshot may still name 'coastal': it never described anything real.");
    }

    /// <summary>
    /// The reason migration 059 scrubs stored snapshots as well as rows. Comparing
    /// a published 'coastal' against a working null would report "theme changed" on
    /// every published menu in the system — a change nobody made, on the one count
    /// whose promise is that it cannot disagree with what a publish ships (Q182).
    /// </summary>
    [Fact]
    public async Task Draft_ReportsNoThemeChangeForAMenuThatNeverHadOne()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("cider"), Price = "7" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        _ = await PublishCurrentAsync(repository, venueId, menuId);

        var snapshots = await repository.GetDraftSnapshotsAsync(venueId, menuId);
        Assert.Null(MenuSnapshot.Parse(snapshots.Published)!.Theme);
        Assert.Empty(MenuSnapshot.Diff(snapshots.Published, snapshots.Working));
    }

    [Fact]
    public async Task Migration_StoresAPriceExactlyAsTyped()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);

        var outcome = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("oysters"), Price = "MP" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        Assert.Equal(ItemPlacementOutcomes.Created, outcome.Outcome);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));

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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));

        var created = await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("lemonade"), Price = "9.5" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        var published = (await PublishCurrentAsync(repository, venueId, menuId, "Reviewer")).Event;

        Assert.Equal(1, published.Version);
        // The recorded set, the recorded count and the snapshot that shipped all
        // describe the same thing: the count is the diff of the committed
        // snapshot, not of some other reading of the menu (Q182).
        var shipped = System.Text.Json.JsonSerializer.Deserialize<SnapshotChange[]>(
            published.ShippedChanges!,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        Assert.Equal(published.ChangeCount, shipped!.Length);
        Assert.Equal(MenuSnapshot.Diff(null, published.Snapshot).Count, published.ChangeCount);

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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("fizz"), Price = "12" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        await PublishCurrentAsync(repository, venueId, menuId);

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

    // Available is a drafted flag like name/description/price, not an immediate
    // one like 86: it lands in dbo.Items right away, but only reaches the draft
    // diff (and so only ships) on the next publish - this is what makes the
    // difference between the two mechanisms real at the database layer, not just
    // in application code.
    [Fact]
    public async Task GuardedItemEdit_ChangesIsListedAndParticipatesInTheDraft()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        var item = new Item { VenueId = venueId, Name = fixture.UniqueValue("cider"), Price = "8" };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, itemsPerMenuLimit: 500);
        await PublishCurrentAsync(repository, venueId, menuId);

        var published = await repository.GetLatestPublishedSnapshotAsync(venueId, menuId);
        var updated = await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, item.Price, expected: null, DateTime.UtcNow, isListed: false);
        Assert.Equal("updated", updated.Outcome);
        Assert.False(updated.IsListed);

        var live = await repository.GetItemAsync(venueId, item.Id);
        Assert.False(live!.IsListed);

        var change = Assert.Single(MenuSnapshot.Diff(published, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
        Assert.Equal("isListed", change.Field);
        Assert.Equal("true", change.BeforeValue);
        Assert.Equal("false", change.AfterValue);

        // A guard that names the wrong current value refuses, the same way a
        // stale name/description/price expectation refuses.
        var refused = await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, item.Price,
            new ItemValueExpectation(item.Name, item.Description, item.Price, IsListed: true),
            DateTime.UtcNow, isListed: true);
        Assert.Equal("item_changed", refused.Outcome);
        Assert.False(refused.IsListed);
    }

    /*
     * A19 at the database, and the defect that hid behind Q112.
     *
     * UQ_Placements_PageItem is "once per PAGE" (migration 062 replaced 061's
     * once-per-menu with it). So one dish may sit on two pages of one menu - and a
     * four-page printed menu that repeats a dish, or prices it per protein, is
     * exactly that.
     *
     * The guarded edit used to find its placement by MenuId. With two placements on
     * one menu it matched both: the guard compared the caller's expectation against
     * whichever row came back first, and the UPDATE - also keyed by menu - wrote the
     * same price to both. A price change on the lunch page silently rewrote dinner.
     *
     * The edit is now addressed by section, which is unique per item because a
     * section belongs to exactly one page.
     */
    [Fact]
    public async Task GuardedItemEdit_PricesOnePlacementWithoutTouchingTheOtherPage()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var lunchSection = await SeedSectionAsync(dataAccess, venueId, menuId);
        var dinnerPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, dinnerPage, "Dinner", DateTime.UtcNow);
        var dinnerSection = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, dinnerSection, "Dinner mains", DateTime.UtcNow, dinnerPage);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("pad-thai"), Price = "11.95" };
        await repository.CreateItemOnMenuAsync(item, menuId, lunchSection, itemsPerMenuLimit: 500);
        Assert.Equal(
            PlaceExistingOutcomes.Placed,
            (await repository.PlaceExistingItemAsync(venueId, menuId, dinnerSection, item.Id, 500, DateTime.UtcNow)).Outcome);

        // The same dish costs more at dinner. Only the dinner placement is addressed.
        var priced = await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, "13.95", expected: null, DateTime.UtcNow,
            menuId: menuId, sectionId: dinnerSection);
        Assert.Equal("updated", priced.Outcome);

        var prices = await PlacementPricesAsync(dataAccess, venueId, menuId);
        Assert.Equal("13.95", prices[dinnerSection]);
        Assert.Null(prices[lunchSection]);

        // Lunch still reads the library default; dinner reads its own price.
        var library = await repository.GetItemAsync(venueId, item.Id);
        Assert.Equal("11.95", library!.Price);

        // And the reverse: pricing lunch leaves dinner where it was.
        Assert.Equal("updated", (await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, "10.95", expected: null, DateTime.UtcNow,
            menuId: menuId, sectionId: lunchSection)).Outcome);

        prices = await PlacementPricesAsync(dataAccess, venueId, menuId);
        Assert.Equal("10.95", prices[lunchSection]);
        Assert.Equal("13.95", prices[dinnerSection]);

        // The library default is untouched by either. It is what a dish costs when
        // it is placed somewhere new, not something a menu may rewrite (A19).
        Assert.Equal("11.95", (await repository.GetItemAsync(venueId, item.Id))!.Price);
    }

    /*
     * A20 - "on all of them", the answer to the question a price change now asks.
     *
     * It writes the library default AND every placement, so the answer is true straight away
     * rather than only on the menus that happen to carry no price of their own. Writing only
     * Items.Price would have left every menu that had ever been priced unchanged - which is the
     * shape of the bug, not the fix.
     */
    [Fact]
    public async Task GuardedItemEdit_Everywhere_WritesTheLibraryAndEveryPlacement()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var lunchMenu = await SeedMenuAsync(dataAccess, venueId);
        var lunchSection = await SeedSectionAsync(dataAccess, venueId, lunchMenu);
        var dinnerMenu = await SeedMenuAsync(dataAccess, venueId);
        var dinnerSection = await SeedSectionAsync(dataAccess, venueId, dinnerMenu);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("pad-thai"), Price = "11.95" };
        await repository.CreateItemOnMenuAsync(item, lunchMenu, lunchSection, itemsPerMenuLimit: 500);
        await repository.PlaceExistingItemAsync(venueId, dinnerMenu, dinnerSection, item.Id, 500, DateTime.UtcNow);

        // Dinner is priced on its own first, so the test proves the sweep reaches a placement that
        // already carries a price of its own - not merely the ones still reading the default.
        await repository.UpdateItemValuesGuardedAsync(venueId, item.Id, item.Name, item.Description, "13.95",
            expected: null, DateTime.UtcNow, menuId: dinnerMenu, sectionId: dinnerSection);

        var everywhere = await repository.UpdateItemValuesGuardedAsync(venueId, item.Id, item.Name, item.Description, "10.00",
            expected: null, DateTime.UtcNow, menuId: lunchMenu, sectionId: lunchSection, priceEverywhere: true);

        Assert.Equal("updated", everywhere.Outcome);
        Assert.Equal("10.00", (await repository.GetItemAsync(venueId, item.Id))!.Price);
        Assert.Equal("10.00", (await PlacementPricesAsync(dataAccess, venueId, lunchMenu))[lunchSection]);
        Assert.Equal("10.00", (await PlacementPricesAsync(dataAccess, venueId, dinnerMenu))[dinnerSection]);
    }

    // And the default answer is unchanged: "here only" is what a caller that says nothing means.
    [Fact]
    public async Task GuardedItemEdit_WithoutTheEverywhereAnswer_StillTouchesOneMenu()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var lunchMenu = await SeedMenuAsync(dataAccess, venueId);
        var lunchSection = await SeedSectionAsync(dataAccess, venueId, lunchMenu);
        var dinnerMenu = await SeedMenuAsync(dataAccess, venueId);
        var dinnerSection = await SeedSectionAsync(dataAccess, venueId, dinnerMenu);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("laap"), Price = "11.95" };
        await repository.CreateItemOnMenuAsync(item, lunchMenu, lunchSection, itemsPerMenuLimit: 500);
        await repository.PlaceExistingItemAsync(venueId, dinnerMenu, dinnerSection, item.Id, 500, DateTime.UtcNow);

        await repository.UpdateItemValuesGuardedAsync(venueId, item.Id, item.Name, item.Description, "9.00",
            expected: null, DateTime.UtcNow, menuId: lunchMenu, sectionId: lunchSection);

        Assert.Equal("9.00", (await PlacementPricesAsync(dataAccess, venueId, lunchMenu))[lunchSection]);
        Assert.Null((await PlacementPricesAsync(dataAccess, venueId, dinnerMenu))[dinnerSection]);
        Assert.Equal("11.95", (await repository.GetItemAsync(venueId, item.Id))!.Price);
    }

    // A menu edit that does not say which placement it means is refused, not applied
    // to whichever row came back first. The builder always names the section; this is
    // the backstop for anything that does not.
    [Fact]
    public async Task GuardedItemEdit_RefusesAMenuEditThatCannotNameItsPlacement()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var lunchSection = await SeedSectionAsync(dataAccess, venueId, menuId);
        var dinnerPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, dinnerPage, "Dinner", DateTime.UtcNow);
        var dinnerSection = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, dinnerSection, "Dinner mains", DateTime.UtcNow, dinnerPage);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("laap"), Price = "9.00" };
        await repository.CreateItemOnMenuAsync(item, menuId, lunchSection, itemsPerMenuLimit: 500);
        await repository.PlaceExistingItemAsync(venueId, menuId, dinnerSection, item.Id, 500, DateTime.UtcNow);

        var refused = await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, "99.00", expected: null, DateTime.UtcNow, menuId: menuId);

        Assert.Equal("placement_ambiguous", refused.Outcome);

        // Nothing moved - not the placements, and not the library.
        var prices = await PlacementPricesAsync(dataAccess, venueId, menuId);
        Assert.All(prices.Values, Assert.Null);
        Assert.Equal("9.00", (await repository.GetItemAsync(venueId, item.Id))!.Price);
    }

    // One placement is still addressable without naming a section: with nothing to
    // be ambiguous about, a menu edit means the one placement on that menu.
    [Fact]
    public async Task GuardedItemEdit_WithOnePlacement_StillPricesItFromTheMenuAlone()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("som-tum"), Price = "8.50" };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, itemsPerMenuLimit: 500);

        Assert.Equal("updated", (await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, "9.50", expected: null, DateTime.UtcNow, menuId: menuId)).Outcome);

        Assert.Equal("9.50", (await PlacementPricesAsync(dataAccess, venueId, menuId))[sectionId]);
        Assert.Equal("8.50", (await repository.GetItemAsync(venueId, item.Id))!.Price);
    }

    // An edit that belongs to no menu is a library edit and still writes the library
    // default - the item panel outside the builder, and every caller that predates
    // pages. A19 demotes Items.Price; it does not retire it.
    [Fact]
    public async Task GuardedItemEdit_WithNoMenu_StillWritesTheLibraryDefault()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("sticky-rice"), Price = "3.00" };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, itemsPerMenuLimit: 500);

        Assert.Equal("updated", (await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, "4.00", expected: null, DateTime.UtcNow)).Outcome);

        Assert.Equal("4.00", (await repository.GetItemAsync(venueId, item.Id))!.Price);
        Assert.Null((await PlacementPricesAsync(dataAccess, venueId, menuId))[sectionId]);
    }

    private static async Task<Dictionary<Guid, string?>> PlacementPricesAsync(
        SqlDataAccess dataAccess, Guid venueId, Guid menuId) =>
        (await dataAccess.ExecuteSqlQueryAsync<PlacementPriceRow, object>(
            "SELECT MenuSectionId, ImportedPriceOverride AS Price FROM dbo.Placements WHERE VenueId=@VenueId AND MenuId=@MenuId;",
            new { VenueId = venueId, MenuId = menuId }))
        .ToDictionary(row => row.MenuSectionId, row => row.Price);

    private sealed class PlacementPriceRow
    {
        public Guid MenuSectionId { get; set; }
        public string? Price { get; set; }
    }

    /*
     * #908 - the count behind the shelf's limit warning.
     *
     * "Active" means NOT PUT AWAY, which is the whole reason this is a server-side count rather
     * than the number of cards the shelf drew. Putting a menu away is how a venue at its limit
     * makes room; counting put-away menus would leave no way out, and counting cards would have
     * done exactly that.
     */
    [Fact]
    public async Task CountActiveMenus_LeavesOutPutAwayMenusAndOtherVenues()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var otherVenueId = await SeedVenueAsync(dataAccess);

        var onShelf = await SeedMenuAsync(dataAccess, venueId);
        var alsoOnShelf = await SeedMenuAsync(dataAccess, venueId);
        var putAway = await SeedMenuAsync(dataAccess, venueId);
        await SeedMenuAsync(dataAccess, otherVenueId);

        Assert.Equal(3, await repository.CountActiveMenusAsync(venueId));

        await repository.SetPutAwayAsync(venueId, putAway, true, 500, "Jeremy", "Put away", DateTime.UtcNow);
        Assert.Equal(2, await repository.CountActiveMenusAsync(venueId));

        // Back on the shelf and it counts again - the way out has to actually work both ways.
        await repository.SetPutAwayAsync(venueId, putAway, false, 500, "Jeremy", "Put back", DateTime.UtcNow);
        Assert.Equal(3, await repository.CountActiveMenusAsync(venueId));

        Assert.Equal(1, await repository.CountActiveMenusAsync(otherVenueId));
        Assert.NotEqual(onShelf, alsoOnShelf);
    }

    // ---- publish behaviour ------------------------------------------------------

    // Q80, enforced inside the transaction: a publish that can reach nothing and
    // has nothing to release is a named refusal, never a silent version bump.
    [Fact]
    public async Task Publish_RefusesInsideTheTransactionWhenItWouldReachNothing()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        var refusal = await Assert.ThrowsAnyAsync<Exception>(() => PublishCurrentAsync(repository, venueId, menuId));

        Assert.Contains("Pair a screen", refusal.Message, StringComparison.Ordinal);
        Assert.Empty(await repository.GetPublishHistoryAsync(venueId, menuId, 10));
    }

    // Q68: take-off waits as a difference in which screens the menu is on, and the
    // publish that ships it still tells the screens being released.
    [Fact]
    public async Task TakeOff_WaitsInTheDraftAndReleasedScreensAreStillTold()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        var first = await PublishCurrentAsync(repository, venueId, menuId);

        Assert.Equal(1, await repository.ClearMenuAssignmentsAsync(venueId, menuId));

        // The removal is now a pending difference, not an applied one.
        var published = await repository.GetLatestPublishedSnapshotAsync(venueId, menuId);
        var change = Assert.Single(MenuSnapshot.Diff(published, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
        Assert.Equal(DraftTargetKinds.Screens, change.TargetKind);

        var second = (await PublishCurrentAsync(repository, venueId, menuId)).Event;

        // The released screen is targeted by the publish that releases it, so it
        // is told to stop showing the menu rather than left on stale content.
        var target = Assert.Single(await repository.GetPublishTargetsAsync(second.Id));
        Assert.Equal(screenId, target.ScreenId);
        Assert.Empty(MenuSnapshot.Parse((await repository.GetPublishEventAsync(venueId, menuId, second.Version))!.Snapshot)!.Screens ?? []);
        Assert.NotEqual(first.Event.Id, second.Id);
    }

    // Review finding: the shipped set was computed from one reading of the menu
    // and the snapshot from another. The statement now rebuilds the working
    // snapshot under lock and refuses if it has moved, so history can never
    // describe content that did not go out.
    [Fact]
    public async Task Publish_RefusesWhenTheMenuMovedSinceTheDiffWasComputed()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("soda"), Price = "3" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        var stale = await repository.GetWorkingSnapshotAsync(venueId, menuId);

        // Someone edits between the caller reading the menu and publishing it.
        var item = (await repository.GetItemsAsync(venueId)).Single();
        item.Price = "4";
        await repository.UpdateItemAsync(item);

        await Assert.ThrowsAsync<MenuMovedWhilePublishingException>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow },
            shippedChanges: "[]",
            expectedWorkingSnapshot: stale!,
            expectedPublishedSnapshot: null,
            expectedPublishedVersion: 0));

        // Nothing was recorded, so no version claims to have shipped that set.
        Assert.Empty(await repository.GetPublishHistoryAsync(venueId, menuId, 10));

        // Publishing what the menu actually is now succeeds.
        var current = await repository.GetWorkingSnapshotAsync(venueId, menuId);
        var published = await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow },
            "[]", current!, null, 0);
        Assert.Equal("4", Assert.Single(Assert.Single(MenuSnapshot.Parse(published.Event.Snapshot)!.Sections!).Items!).Price);
    }

    // Review #4: the guard compared the two snapshots under the database's own
    // collation, which is case- and accent-insensitive, so a rename that differed
    // only in casing read as "unchanged" and let through exactly the mismatch the
    // guard exists to prevent. The comparison is binary now.
    [Fact]
    public async Task Publish_RefusesWhenTheOnlyChangeIsLetterCasing()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = "Burger", Price = "12" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        var snapshots = await repository.GetDraftSnapshotsAsync(venueId, menuId);

        // The only difference is the case of one letter.
        var item = (await repository.GetItemsAsync(venueId)).Single(candidate => candidate.Name == "Burger");
        item.Name = "burger";
        await repository.UpdateItemAsync(item);

        await Assert.ThrowsAsync<MenuMovedWhilePublishingException>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow },
            shippedChanges: "[]",
            expectedWorkingSnapshot: snapshots.Working!,
            expectedPublishedSnapshot: snapshots.Published,
            expectedPublishedVersion: snapshots.PublishedVersion));

        Assert.Empty(await repository.GetPublishHistoryAsync(venueId, menuId, 10));
    }

    // Review #4: the guard proved the working state had not moved but never that
    // the published version it was compared against was still current, so a
    // publish by someone else in between could make this one re-ship a difference
    // that had already reached the screens.
    [Fact]
    public async Task Publish_RefusesWhenSomeoneElsePublishedSinceTheDiffWasComputed()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("cola"), Price = "3" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        // One caller reads the menu, and another publishes it before the first commits.
        var stale = await repository.GetDraftSnapshotsAsync(venueId, menuId);
        await PublishCurrentAsync(repository, venueId, menuId, "Someone else");

        // The working state is untouched, so only the version check can catch this.
        await Assert.ThrowsAsync<MenuMovedWhilePublishingException>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow },
            shippedChanges: "[]",
            expectedWorkingSnapshot: stale.Working!,
            expectedPublishedSnapshot: stale.Published,
            expectedPublishedVersion: stale.PublishedVersion));

        // Exactly one publish exists: the other person's.
        Assert.Single(await repository.GetPublishHistoryAsync(venueId, menuId, 10));
    }

    // Review #5: the published snapshot and its version were read as two separate
    // subqueries, so a publish landing between them handed back one version's
    // content labelled with another's. Both now come from the same row.
    [Fact]
    public async Task DraftRead_TakesThePublishedSnapshotAndItsVersionFromTheSameEvent()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("tea"), Price = "2" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        await PublishCurrentAsync(repository, venueId, menuId);

        var item = (await repository.GetItemsAsync(venueId)).Single();
        item.Price = "5";
        await repository.UpdateItemAsync(item);
        await PublishCurrentAsync(repository, venueId, menuId);

        var read = await repository.GetDraftSnapshotsAsync(venueId, menuId);

        // The version and the content it is labelled with have to be the same
        // event's, or a diff computed from the pair describes nothing real.
        Assert.Equal(2, read.PublishedVersion);
        var atThatVersion = await repository.GetPublishEventAsync(venueId, menuId, read.PublishedVersion);
        Assert.Equal(atThatVersion!.Snapshot, read.Published);
        Assert.Equal("5", Assert.Single(Assert.Single(MenuSnapshot.Parse(read.Published)!.Sections!).Items!).Price);
    }

    // ...and publish proves that pairing rather than trusting it: a shipped set
    // computed against a different published snapshot is refused even when the
    // version number it carries is the current one.
    [Fact]
    public async Task Publish_RefusesADiffComputedAgainstADifferentPublishedSnapshot()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("cider"), Price = "6" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        var first = await PublishCurrentAsync(repository, venueId, menuId);

        var item = (await repository.GetItemsAsync(venueId)).Single();
        item.Price = "7";
        await repository.UpdateItemAsync(item);
        await PublishCurrentAsync(repository, venueId, menuId);

        // The version is current, the working state is current, and only the base
        // the diff was taken from is stale - which is exactly what a torn read
        // produces.
        var current = await repository.GetDraftSnapshotsAsync(venueId, menuId);
        await Assert.ThrowsAsync<MenuMovedWhilePublishingException>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, PublishedUtc = DateTime.UtcNow },
            shippedChanges: "[]",
            expectedWorkingSnapshot: current.Working!,
            expectedPublishedSnapshot: first.Event.Snapshot,
            expectedPublishedVersion: current.PublishedVersion));

        Assert.Equal(2, (await repository.GetPublishHistoryAsync(venueId, menuId, 10)).Count);
    }

    // Review #5: assignment and publish refused a put-away menu, but restore puts
    // screen assignments back too - a third way onto the shelf, around the ceiling
    // check and the record, leaving a menu both put away and on a screen.
    [Fact]
    public async Task Restore_ToAVersionThatWasOnAScreen_IsRefusedWhileTheMenuIsPutAway()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        var version = await PublishCurrentAsync(repository, venueId, menuId);
        await ShelveAsync(repository, venueId, menuId);

        // The stored version has the screen in it, so restoring would re-assign it.
        Assert.NotEmpty(MenuSnapshot.Parse(version.Event.Snapshot)!.Screens!);

        await Assert.ThrowsAsync<MenuPutAwayException>(() => repository.RestoreSnapshotAsync(
            venueId, menuId, version.Event.Snapshot!, "Owner", "Went back.", DateTime.UtcNow));

        // No assignment was created, and nothing claims a restore happened.
        Assert.Empty(await repository.GetAssignmentsAsync(venueId));
        Assert.DoesNotContain(
            await repository.GetHistoryAsync(venueId, menuId, 20),
            entry => entry.Kind == MenuHistoryKinds.Restored);
    }

    // Review #6: the refusal above was written as "a put-away menu cannot be
    // restored at all", which is broader than the rule it enforces. A shelved menu
    // can still be edited, and discarding that draft goes back to the published
    // snapshot - which, because the menu had to leave its screens to be shelved,
    // has no screens in it and cannot put the menu back on one. Refusing it left
    // the draft with no way out.
    [Fact]
    public async Task Discard_OnAPutAwayMenu_GoesBackToTheScreenlessPublishedShape()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("burger"), Price = "9" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);
        var shelved = await ShelveAsync(repository, venueId, menuId);

        // Nothing this version carries can re-assign a screen.
        Assert.Empty(MenuSnapshot.Parse(shelved.Event.Snapshot)!.Screens ?? []);

        // A shelved menu is still editable; only the screens are settled.
        var item = (await repository.GetItemsAsync(venueId)).Single(candidate => candidate.Price == "9");
        item.Price = "11";
        Assert.True(await repository.UpdateItemAsync(item));
        Assert.NotEmpty(MenuSnapshot.Diff(
            shelved.Event.Snapshot,
            (await repository.GetDraftSnapshotsAsync(venueId, menuId)).Working));

        await repository.RestoreSnapshotAsync(
            venueId, menuId, shelved.Event.Snapshot!, "Owner", "Threw the draft away.", DateTime.UtcNow,
            kind: MenuHistoryKinds.DraftDiscarded);

        // The draft is gone, the menu is still off the shelf, and still on no screen.
        Assert.Empty(MenuSnapshot.Diff(
            shelved.Event.Snapshot,
            (await repository.GetDraftSnapshotsAsync(venueId, menuId)).Working));
        Assert.Empty(await repository.GetAssignmentsAsync(venueId));
        Assert.Equal(0, await repository.CountMenusAsync(venueId));
    }

    // The milestone's central claim - a screen shows the last published version, and
    // only a publish changes that - had no read behind it until now. That absence is
    // why the owner demo could report twelve of twelve while a screen sat stranded:
    // every check asked the API whether it accepted a request, and none could ask the
    // screen.
    [Fact]
    public async Task ScreensShowing_IsThePublishedTruth_NotTheAssignment()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);

        // A screen nobody has published to is showing nothing.
        var before = Assert.Single(await repository.GetScreensShowingAsync(venueId));
        Assert.Equal(screenId, before.ScreenId);
        Assert.Null(before.MenuId);

        // Assigning is intent, not delivery: the screen is still showing nothing.
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        Assert.Null(Assert.Single(await repository.GetScreensShowingAsync(venueId)).MenuId);

        // Publishing is what reaches it.
        var published = await PublishCurrentAsync(repository, venueId, menuId, "Owner");
        var showing = Assert.Single(await repository.GetScreensShowingAsync(venueId));
        Assert.Equal(menuId, showing.MenuId);
        Assert.Equal(published.Event.Version, showing.Version);
        Assert.Equal("Owner", showing.Author);

        // A take-off is intent too: until it is published the screen still shows the menu.
        await repository.TakeOffScreensAsync(venueId, menuId, "Chef", DateTime.UtcNow);
        Assert.Equal(menuId, Assert.Single(await repository.GetScreensShowingAsync(venueId)).MenuId);

        // The publish that carries the take-off is what empties it.
        await PublishCurrentAsync(repository, venueId, menuId, "Owner");
        Assert.Null(Assert.Single(await repository.GetScreensShowingAsync(venueId)).MenuId);
    }

    // A screen handed to another menu shows that menu from the moment it publishes,
    // and nothing the previous menu does afterwards changes what it is showing.
    [Fact]
    public async Task ScreensShowing_FollowsTheScreenToWhicheverMenuPublishedToItLast()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var first = await SeedMenuAsync(dataAccess, venueId);
        var second = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);

        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = first }));
        await PublishCurrentAsync(repository, venueId, first);
        Assert.Equal(first, Assert.Single(await repository.GetScreensShowingAsync(venueId)).MenuId);

        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = second }));
        await PublishCurrentAsync(repository, venueId, second);
        Assert.Equal(second, Assert.Single(await repository.GetScreensShowingAsync(venueId)).MenuId);

        // The first menu's stale take-off is refused and leaves the screen alone.
        await repository.TakeOffScreensAsync(venueId, first, "Chef", DateTime.UtcNow);
        await Assert.ThrowsAsync<ScreensTakenByAnotherMenuException>(
            () => PublishCurrentAsync(repository, venueId, first));
        Assert.Equal(second, Assert.Single(await repository.GetScreensShowingAsync(venueId)).MenuId);
    }

    // Another venue's screens are not this venue's business, published or not.
    [Fact]
    public async Task ScreensShowing_NamesOnlyThisVenuesScreens()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueA);
        var screenA = await SeedScreenAsync(dataAccess, venueA);
        _ = await SeedScreenAsync(dataAccess, venueB);

        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueA, ScreenId = screenA, MenuId = menuA }));
        await PublishCurrentAsync(repository, venueA, menuA);

        Assert.Equal(screenA, Assert.Single(await repository.GetScreensShowingAsync(venueA)).ScreenId);
        Assert.Null(Assert.Single(await repository.GetScreensShowingAsync(venueB)).MenuId);
    }

    // Review #4: a delivery target records who was *told* about a publish,
    // including the screens a take-off released. Reading membership from it meant
    // a screen-less menu could publish for ever, re-targeting screens it had
    // already let go and stepping around Q80 every time.
    [Fact]
    public async Task Publish_AfterATakeOffHasShipped_HasNothingLeftToReachAndSaysSo()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);

        await repository.TakeOffScreensAsync(venueId, menuId, "Chef", DateTime.UtcNow);
        var release = await PublishCurrentAsync(repository, venueId, menuId);
        Assert.Equal(screenId, Assert.Single(await repository.GetPublishTargetsAsync(release.Event.Id)).ScreenId);

        // The release has happened. There is nothing left to reach, so Q80 applies.
        var refusal = await Assert.ThrowsAsync<MenuNotOnAnyScreenException>(
            () => PublishCurrentAsync(repository, venueId, menuId));

        Assert.Contains("not on a screen", refusal.Message, StringComparison.Ordinal);
        Assert.Equal(2, (await repository.GetPublishHistoryAsync(venueId, menuId, 10)).Count);
        // ...and no second take-off entry claiming it happened twice.
        Assert.Single(
            await repository.GetHistoryAsync(venueId, menuId, 20),
            entry => entry.Kind == MenuHistoryKinds.TakenOffScreens && entry.PublishEventId is not null);
    }

    // Review #4: put-back was guarded, but assignment and publish were two other
    // doors onto the shelf that skipped the ceiling and the record entirely.
    [Fact]
    public async Task APutAwayMenu_CanBeNeitherGivenAScreenNorPublished()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);
        await ShelveAsync(repository, venueId, menuId);

        await Assert.ThrowsAsync<MenuPutAwayException>(async () => await repository.AssignScreenAsync(
            await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId })));
        await Assert.ThrowsAsync<MenuPutAwayException>(() => PublishCurrentAsync(repository, venueId, menuId));

        // It is still off the shelf, so it still does not count against the ceiling.
        Assert.Equal(0, await repository.CountMenusAsync(venueId));
    }

    // Review #6: put away decided "off its screens" from the working assignment
    // alone. A take-off deletes that row but reaches the screens only on the next
    // publish (Q68), so a menu could be shelved with the take-off still pending -
    // and the publish that would free the screen was then refused for being put
    // away. The screen kept showing a menu the system called shelved, with no act
    // left that could clear it.
    [Fact]
    public async Task PutAway_IsRefusedUntilTheTakeOffHasReachedTheScreens()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        var live = await PublishCurrentAsync(repository, venueId, menuId);

        // The take-off empties the working assignment, but the screen still shows
        // the published version until a publish carries it.
        Assert.Equal(1, await repository.TakeOffScreensAsync(venueId, menuId, "Owner", DateTime.UtcNow));
        Assert.Empty(await repository.GetAssignmentsAsync(venueId));
        Assert.Equal(screenId, Assert.Single(MenuSnapshot.Parse(live.Event.Snapshot)!.Screens!).ScreenId);

        var refused = await repository.SetPutAwayAsync(
            venueId, menuId, isPutAway: true, activeMenuLimit: 50, "Owner", "Put the menu away.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.StillOnScreens, refused.Outcome);

        // Nothing happened: the menu is still on the shelf, and still publishable -
        // which is the act that actually frees the screen.
        Assert.Equal(1, await repository.CountMenusAsync(venueId));
        var released = await PublishCurrentAsync(repository, venueId, menuId);
        Assert.Equal(screenId, Assert.Single(await repository.GetPublishTargetsAsync(released.Event.Id)).ScreenId);
        Assert.Empty(MenuSnapshot.Parse(released.Event.Snapshot)!.Screens ?? []);

        var shelved = await repository.SetPutAwayAsync(
            venueId, menuId, isPutAway: true, activeMenuLimit: 50, "Owner", "Put the menu away.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.Changed, shelved.Outcome);
    }

    // The same rule must not create the mirror trap: a screen another menu has
    // since been given is not this menu's to release - publish leaves it alone by
    // the owner's rule and would refuse outright, having nothing to reach - so it
    // cannot be what holds the menu on the shelf either.
    [Fact]
    public async Task PutAway_IgnoresAPublishedScreenAnotherMenuHasSinceTaken()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var otherMenuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);

        // The first menu is taken off and the screen is given to another menu
        // before the take-off is ever published.
        await repository.TakeOffScreensAsync(venueId, menuId, "Owner", DateTime.UtcNow);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = otherMenuId }));

        // There is no publish left that could release it, so nothing is pending.
        await Assert.ThrowsAsync<ScreensTakenByAnotherMenuException>(
            () => PublishCurrentAsync(repository, venueId, menuId));

        var outcome = await repository.SetPutAwayAsync(
            venueId, menuId, isPutAway: true, activeMenuLimit: 50, "Owner", "Put the menu away.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.Changed, outcome.Outcome);

        // The other menu keeps the screen throughout.
        Assert.Equal(otherMenuId, Assert.Single(await repository.GetAssignmentsAsync(venueId)).MenuId);
    }

    // The owner's rule for a stale act: never touch a screen another menu now
    // owns, and never let that be silent.
    [Fact]
    public async Task Publish_LeavesAScreenAnotherMenuNowOwnsAloneAndNamesIt()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueId);
        var menuB = await SeedMenuAsync(dataAccess, venueId);
        var kept = await SeedScreenAsync(dataAccess, venueId);
        var taken = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = kept, MenuId = menuA }));
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = taken, MenuId = menuA }));
        await PublishCurrentAsync(repository, venueId, menuA);

        // A is taken off both screens, then one of them is given to B.
        await repository.TakeOffScreensAsync(venueId, menuA, "Chef", DateTime.UtcNow);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = taken, MenuId = menuB }));

        var outcome = await PublishCurrentAsync(repository, venueId, menuA);

        // The screen B now owns is named, and no delivery row was written for it.
        Assert.Equal(taken, Assert.Single(outcome.ConflictedScreenIds));
        var target = Assert.Single(await repository.GetPublishTargetsAsync(outcome.Event.Id));
        Assert.Equal(kept, target.ScreenId);
    }

    [Fact]
    public async Task Publish_RefusesWhenEveryScreenItWasOnBelongsToAnotherMenu()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueId);
        var menuB = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuA }));
        await PublishCurrentAsync(repository, venueId, menuA);

        await repository.TakeOffScreensAsync(venueId, menuA, "Chef", DateTime.UtcNow);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuB }));

        var refusal = await Assert.ThrowsAsync<ScreensTakenByAnotherMenuException>(
            () => PublishCurrentAsync(repository, venueId, menuA));

        Assert.Contains("different menu", refusal.Message, StringComparison.Ordinal);
        // Only the first publish exists: nothing versioned against no one.
        Assert.Single(await repository.GetPublishHistoryAsync(venueId, menuA, 10));
    }

    // Q68 and Q207: the act is attributable when the person does it, and again
    // when the publish carries it to the screens.
    [Fact]
    public async Task TakeOff_IsRecordedWhenItHappensAndWhenItShips()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);

        Assert.Equal(1, await repository.TakeOffScreensAsync(venueId, menuId, "Chef", DateTime.UtcNow));

        var queued = Assert.Single(
            await repository.GetHistoryAsync(venueId, menuId, 20),
            entry => entry.Kind == MenuHistoryKinds.TakenOffScreens);
        Assert.Equal("Chef", queued.Author);
        Assert.Null(queued.PublishEventId);

        await PublishCurrentAsync(repository, venueId, menuId, author: "Publisher");

        var shipped = Assert.Single(
            await repository.GetHistoryAsync(venueId, menuId, 20),
            entry => entry.Kind == MenuHistoryKinds.TakenOffScreens && entry.PublishEventId is not null);
        Assert.Equal("Publisher", shipped.Author);
    }

    // ---- put away ---------------------------------------------------------------

    [Fact]
    public async Task PutAway_IsAttributableAndRefusedWhileTheMenuIsOnAScreen()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));

        var refused = await repository.SetPutAwayAsync(
            venueId, menuId, isPutAway: true, activeMenuLimit: 50, "Owner", "Put the menu away.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.StillOnScreens, refused.Outcome);
        Assert.Equal(1, await repository.CountMenusAsync(venueId));

        await repository.TakeOffScreensAsync(venueId, menuId, "Owner", DateTime.UtcNow);
        var put = await repository.SetPutAwayAsync(
            venueId, menuId, isPutAway: true, activeMenuLimit: 50, "Owner", "Put the menu away.", DateTime.UtcNow);

        Assert.Equal(PutAwayOutcomes.Changed, put.Outcome);
        // The refusal's own advice now works: a put-away menu makes room.
        Assert.Equal(0, await repository.CountMenusAsync(venueId));
        var entry = Assert.Single(
            await repository.GetHistoryAsync(venueId, menuId, 20),
            history => history.Kind == MenuHistoryKinds.PutAway);
        Assert.Equal("Owner", entry.Author);
    }

    [Fact]
    public async Task PutBack_IsBoundedByTheSameCeilingAsCreatingAMenu()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var first = await SeedMenuAsync(dataAccess, venueId);
        await SeedMenuAsync(dataAccess, venueId);
        await repository.SetPutAwayAsync(
            venueId, first, isPutAway: true, activeMenuLimit: 50, "Owner", "Put the menu away.", DateTime.UtcNow);

        var refused = await repository.SetPutAwayAsync(
            venueId, first, isPutAway: false, activeMenuLimit: 1, "Owner", "Put the menu back on the shelf.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.OverCeiling, refused.Outcome);

        var admitted = await repository.SetPutAwayAsync(
            venueId, first, isPutAway: false, activeMenuLimit: 2, "Owner", "Put the menu back on the shelf.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.Changed, admitted.Outcome);
        Assert.Single(
            await repository.GetHistoryAsync(venueId, first, 20),
            history => history.Kind == MenuHistoryKinds.PutBack);
    }

    // ---- the shelf ---------------------------------------------------------------

    /// <summary>
    /// The shelf reads every menu in one statement, and each card's board and its
    /// change count come from the same pair of snapshots — so a card can never draw
    /// one board while counting the difference to another.
    /// </summary>
    [Fact]
    public async Task Shelf_ReturnsEveryMenuWithItsPublishedBoardAndItsOwnDraftCount()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var publishedMenuId = await SeedMenuAsync(dataAccess, venueId);
        var neverPublishedMenuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, publishedMenuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = publishedMenuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("shelf-ale"), Price = "5" },
            publishedMenuId, sectionId, itemsPerMenuLimit: 500);

        var published = await PublishCurrentAsync(repository, venueId, publishedMenuId, "Alex");

        // One edit after the publish, so this card has exactly one waiting change.
        var item = (await repository.GetItemsAsync(venueId)).Single(candidate => candidate.Price == "5");
        item.Price = "MP";
        await repository.UpdateItemAsync(item);

        var shelf = await repository.GetShelfAsync(venueId);

        var card = Assert.Single(shelf, menu => menu.MenuId == publishedMenuId);
        Assert.Equal(published.Event.Version, card.PublishedVersion);
        Assert.Equal("Alex", card.LastPublishedBy);
        Assert.NotNull(card.LastPublishedUtc);
        var change = Assert.Single(MenuSnapshot.Diff(card.PublishedSnapshot, card.WorkingSnapshot));
        Assert.Equal("price", change.Field);
        // Exactly as typed, on the shelf as everywhere else (Q115/Q190).
        Assert.Equal("MP", change.AfterValue);

        // A menu that has never been published is a card with no board, not an error
        // and not an empty board that would render as a blank screen.
        var unpublished = Assert.Single(shelf, menu => menu.MenuId == neverPublishedMenuId);
        Assert.Null(unpublished.PublishedSnapshot);
        Assert.Null(unpublished.PublishedVersion);
        Assert.NotNull(unpublished.WorkingSnapshot);
    }

    [Fact]
    public async Task Shelf_ShowsAPutAwayMenuAsPutAway()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        await repository.SetPutAwayAsync(venueId, menuId, true, 50, "Alex", "Put away.", DateTime.UtcNow);

        var card = Assert.Single(await repository.GetShelfAsync(venueId), menu => menu.MenuId == menuId);
        Assert.True(card.IsPutAway);
    }

    [Fact]
    public async Task Shelf_NeverReachesIntoAnotherVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var mine = await SeedVenueAsync(dataAccess);
        var theirs = await SeedVenueAsync(dataAccess);
        var myMenu = await SeedMenuAsync(dataAccess, mine);
        var theirMenu = await SeedMenuAsync(dataAccess, theirs);

        var shelf = await repository.GetShelfAsync(mine);

        Assert.Contains(shelf, menu => menu.MenuId == myMenu);
        Assert.DoesNotContain(shelf, menu => menu.MenuId == theirMenu);
    }

    /// <summary>
    /// The board and the version that put it there come from one row. Read
    /// separately, a publish landing between them returns one version's board
    /// labelled with another's — the torn read this model has produced before.
    /// </summary>
    [Fact]
    public async Task PublishedBoard_CarriesTheVersionAndAuthorThatPutItOnTheScreens()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("board-ale"), Price = "4" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        var first = await PublishCurrentAsync(repository, venueId, menuId, "Alex");
        var item = (await repository.GetItemsAsync(venueId)).Single(candidate => candidate.Price == "4");
        item.Price = "4.5";
        await repository.UpdateItemAsync(item);
        var second = await PublishCurrentAsync(repository, venueId, menuId, "Sam");

        var board = await repository.GetLatestPublishedBoardAsync(venueId, menuId);

        Assert.NotNull(board);
        Assert.Equal(second.Event.Version, board!.Version);
        Assert.Equal("Sam", board.Author);
        Assert.NotEqual(first.Event.Version, board.Version);
        // The board returned is the one that version published, not the earlier one.
        Assert.Equal(second.Event.Snapshot, board.Snapshot);
    }

    [Fact]
    public async Task PublishedBoard_IsNullForAMenuThatHasNeverBeenPublished()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        Assert.Null(await repository.GetLatestPublishedBoardAsync(venueId, menuId));
    }

    // ---- history carries the version -----------------------------------------------

    /// <summary>
    /// "Go back to..." is addressed by version, so a list of what happened has to
    /// carry one. Without this the only place a client ever learns a version is the
    /// response to its own publish, and the action is unreachable from the UI.
    /// </summary>
    [Fact]
    public async Task History_CarriesTheVersionOfThePublishItNames()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("history-ale"), Price = "3" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        var published = await PublishCurrentAsync(repository, venueId, menuId);

        var entries = await repository.GetHistoryAsync(venueId, menuId, 20);

        var publishEntry = Assert.Single(entries, entry => entry.Kind == MenuHistoryKinds.Published);
        Assert.Equal(published.Event.Version, publishEntry.Version);

        // The kinds that are not a publish carry no version, rather than borrowing one.
        Assert.All(
            entries.Where(entry => entry.Kind != MenuHistoryKinds.Published),
            entry => Assert.Null(entry.Version));
    }

    // ---- duplicate -----------------------------------------------------------------

    /// <summary>
    /// Q20: the copy places the SAME library items, so a later price edit reaches
    /// both boards. Cloning the items instead would look identical on the day and
    /// diverge silently afterwards.
    /// </summary>
    [Fact]
    public async Task Duplicate_PlacesTheSameLibraryItemsOnANeverPublishedCopy()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("dupe-ale"), Price = "9" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        _ = await PublishCurrentAsync(repository, venueId, menuId);

        var copyId = Guid.NewGuid();
        var outcome = await repository.DuplicateMenuWithinCeilingAsync(
            venueId, menuId, copyId, 50, "Alex", "Duplicated.", DateTime.UtcNow);

        Assert.True(outcome.Created);

        // Same library items: one item, on two boards.
        var sourcePlacements = await repository.GetPlacementsAsync(venueId, menuId);
        var copyPlacements = await repository.GetPlacementsAsync(venueId, copyId);
        Assert.Equal(
            sourcePlacements.Select(p => p.ItemId).OrderBy(id => id),
            copyPlacements.Select(p => p.ItemId).OrderBy(id => id));

        // ...and the sections are the copy's own, not shared with the original.
        Assert.Empty(copyPlacements.Select(p => p.MenuSectionId).Intersect(sourcePlacements.Select(p => p.MenuSectionId)));

        // Never published, on no screen: delivery is always deliberate.
        Assert.Null(await repository.GetLatestPublishedBoardAsync(venueId, copyId));
        Assert.DoesNotContain(await repository.GetAssignmentsAsync(venueId), a => a.MenuId == copyId);

        // The copy's timeline says where it came from - the one thing not derivable
        // from any other column.
        var entry = Assert.Single(await repository.GetHistoryAsync(venueId, copyId, 10));
        Assert.Equal(MenuHistoryKinds.Duplicated, entry.Kind);
        Assert.Equal("Alex", entry.Author);
    }

    /// <summary>
    /// Menu names are not unique in the database, so two people duplicating the same
    /// menu at once would both read "no copy yet" and both take the name. The name is
    /// chosen inside the same lock as the insert, so the second one gets the next one.
    /// </summary>
    [Fact]
    public async Task Duplicate_GivesTheSecondCopyItsOwnName()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sourceName = (await repository.GetShelfAsync(venueId)).Single(menu => menu.MenuId == menuId).Name;

        var first = await repository.DuplicateMenuWithinCeilingAsync(
            venueId, menuId, Guid.NewGuid(), 50, "Alex", "Duplicated.", DateTime.UtcNow);
        var second = await repository.DuplicateMenuWithinCeilingAsync(
            venueId, menuId, Guid.NewGuid(), 50, "Alex", "Duplicated.", DateTime.UtcNow);

        Assert.Equal($"{sourceName} copy", first.Name);
        Assert.Equal($"{sourceName} copy 2", second.Name);
    }

    /// <summary>
    /// A duplicate is a new menu, so it is bounded by the same ceiling as creating
    /// one - otherwise Duplicate is simply the way around the limit.
    /// </summary>
    [Fact]
    public async Task Duplicate_IsRefusedAtTheMenuCeiling()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        // The venue has exactly one active menu, and the ceiling is one.
        var outcome = await repository.DuplicateMenuWithinCeilingAsync(
            venueId, menuId, Guid.NewGuid(), 1, "Alex", "Duplicated.", DateTime.UtcNow);

        Assert.False(outcome.Created);
        Assert.Equal(1, outcome.ActiveMenuCount);
        Assert.Null(outcome.Name);
    }

    /// <summary>
    /// The statement's own tenant check, the same 51001 every other menu-scoped write
    /// raises. The service refuses earlier, by not finding the menu on the caller's
    /// shelf at all; this is the backstop underneath that, so a copy can never be made
    /// from another venue's menu even if a caller reaches the repository directly.
    /// </summary>
    [Fact]
    public async Task Duplicate_RefusesAMenuFromAnotherVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var mine = await SeedVenueAsync(dataAccess);
        var theirs = await SeedVenueAsync(dataAccess);
        var theirMenu = await SeedMenuAsync(dataAccess, theirs);
        var copyId = Guid.NewGuid();

        var refusal = await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => repository.DuplicateMenuWithinCeilingAsync(
                mine, theirMenu, copyId, 50, "Alex", "Duplicated.", DateTime.UtcNow));

        Assert.Equal(51001, refusal.Number);
        Assert.DoesNotContain(await repository.GetShelfAsync(mine), menu => menu.MenuId == copyId);
        Assert.DoesNotContain(await repository.GetShelfAsync(theirs), menu => menu.MenuId == copyId);
    }

    // ---- restore ---------------------------------------------------------------

    // Q67/Q43: restore puts values back onto the rows that already exist, brings
    // back removed placements and assignments, and records the act in the same
    // transaction. After restoring to a version, the draft against it is empty.
    [Fact]
    public async Task Restore_PutsTheWholeShapeBackAndRecordsTheActTogether()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("stout"), Price = "8" },
            menuId, sectionId, itemsPerMenuLimit: 500);
        var version = await PublishCurrentAsync(repository, venueId, menuId);

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
            venueId, menuId, version.Event.Snapshot!, "Reviewer", "Went back to version 1.", DateTime.UtcNow);

        // The strongest statement available: after the restore, nothing differs
        // from the version restored to.
        Assert.Empty(MenuSnapshot.Diff(version.Event.Snapshot, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
        var entry = Assert.Single(await repository.GetHistoryAsync(venueId, menuId, 10), h => h.Kind == MenuHistoryKinds.Restored);
        Assert.Equal("Reviewer", entry.Author);
    }

    /// <summary>
    /// Going back to a version the menu had no theme on takes the theme off again.
    ///
    /// Restore used to write <c>Theme = ISNULL(t.Theme, m.Theme)</c>, which treats a
    /// null in the snapshot as "not recorded" and keeps whatever is attached now.
    /// Since an unthemed menu is a valid state (Q86), null is a recorded fact, and
    /// the guard silently made going back to it impossible: the menu would claim to
    /// be on version 1 while wearing a theme version 1 never had.
    /// </summary>
    [Fact]
    public async Task Restore_TakesTheThemeBackOffAMenuPublishedWithoutOne()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("porter"), Price = "6" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        // Version 1: no theme attached, which is the state being restored to.
        var unthemed = await PublishCurrentAsync(repository, venueId, menuId);
        Assert.Null(MenuSnapshot.Parse(unthemed.Event.Snapshot)!.Theme);

        // Someone attaches one afterwards.
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "UPDATE dbo.Menus SET Theme = @Theme WHERE Id = @MenuId; SELECT 1 AS Value;",
            new { Theme = "harbour-dark", MenuId = menuId });
        Assert.Equal(
            "harbour-dark",
            MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!.Theme);

        await repository.RestoreSnapshotAsync(
            venueId, menuId, unthemed.Event.Snapshot!, "Reviewer", "Went back to version 1.", DateTime.UtcNow);

        var afterRestore = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId));
        Assert.Null(afterRestore!.Theme);

        // And the strongest statement: nothing differs from the version restored to.
        Assert.Empty(MenuSnapshot.Diff(unthemed.Event.Snapshot, await repository.GetWorkingSnapshotAsync(venueId, menuId)));
    }

    /// <summary>
    /// The other direction, so the fix is not simply "theme is always cleared":
    /// going back to a version that HAD a theme puts that theme back.
    /// </summary>
    [Fact]
    public async Task Restore_PutsBackTheThemeTheVersionWasPublishedWith()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("saison"), Price = "8" },
            menuId, sectionId, itemsPerMenuLimit: 500);

        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "UPDATE dbo.Menus SET Theme = @Theme WHERE Id = @MenuId; SELECT 1 AS Value;",
            new { Theme = "harbour-dark", MenuId = menuId });
        var themed = await PublishCurrentAsync(repository, venueId, menuId);
        Assert.Equal("harbour-dark", MenuSnapshot.Parse(themed.Event.Snapshot)!.Theme);

        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "UPDATE dbo.Menus SET Theme = NULL WHERE Id = @MenuId; SELECT 1 AS Value;",
            new { MenuId = menuId });

        await repository.RestoreSnapshotAsync(
            venueId, menuId, themed.Event.Snapshot!, "Reviewer", "Went back.", DateTime.UtcNow);

        Assert.Equal(
            "harbour-dark",
            MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!.Theme);
    }

    // Review finding: restore only updated sections that still existed, so a
    // section added since the snapshot stayed on the board and one removed since
    // never came back. Either left the menu different from the version it claimed
    // to have gone back to, immediately after restoring.
    [Fact]
    public async Task Restore_PutsSectionsBack_WhetherAdded_Removed_OrReordered()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var first = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var second = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 1);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await repository.CreateItemOnMenuAsync(
            new Item { VenueId = venueId, Name = fixture.UniqueValue("wings"), Price = "12" },
            menuId, first, itemsPerMenuLimit: 500);
        var version = await PublishCurrentAsync(repository, venueId, menuId);

        // Every way a section can drift: one added (carrying an item of its own),
        // one deleted outright, and the order of what remains swapped.
        var added = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 7);
        var stranded = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("olives"), Price = "7" };
        await repository.CreateItemOnMenuAsync(stranded, menuId, added, itemsPerMenuLimit: 500);
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            DELETE FROM dbo.MenuSections WHERE Id = @Second;
            UPDATE dbo.MenuSections SET SortOrder = 5 WHERE Id = @First;
            SELECT 1 AS Value;
            """,
            new { First = first, Second = second });

        await repository.RestoreSnapshotAsync(
            venueId, menuId, version.Event.Snapshot!, "Reviewer", "Went back.", DateTime.UtcNow);

        // The whole shape is back: nothing differs from the version restored to.
        Assert.Empty(MenuSnapshot.Diff(version.Event.Snapshot, await repository.GetWorkingSnapshotAsync(venueId, menuId)));

        var restored = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId));
        Assert.Equal([first, second], restored!.Sections!.Select(section => section.SectionId).ToArray());

        // The section added since is DELETED, not hidden (M3: sections are deleted,
        // not archived - Q96). Hiding it left a row nothing could ever reach again.
        Assert.DoesNotContain(restored.Sections!, section => section.SectionId == added);
        var sectionRows = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "SELECT COUNT(*) AS Value FROM dbo.MenuSections WHERE Id = @Added;",
            new { Added = added })).Single().Value;
        Assert.Equal(0, sectionRows);

        // ...and its item is back in the library rather than destroyed with it. That
        // is the whole of Q96's "nothing is lost": the item was never IN the section,
        // a placement put it there, so only the placement goes.
        var placementRows = (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "SELECT COUNT(*) AS Value FROM dbo.Placements WHERE ItemId = @Item;",
            new { Item = stranded.Id })).Single().Value;
        Assert.Equal(0, placementRows);
        Assert.NotNull(await repository.GetItemAsync(venueId, stranded.Id));
    }

    // A restore that cannot put the screens back has not put the menu back, so it
    // is refused rather than reported as a success with a still-dirty draft.
    [Fact]
    public async Task Restore_RefusesWhenAScreenItWantsBelongsToAnotherMenu()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueId);
        var menuB = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuA }));
        var version = await PublishCurrentAsync(repository, venueId, menuA);

        await repository.TakeOffScreensAsync(venueId, menuA, "Chef", DateTime.UtcNow);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuB }));

        await Assert.ThrowsAsync<ScreensTakenByAnotherMenuException>(() => repository.RestoreSnapshotAsync(
            venueId, menuA, version.Event.Snapshot!, "Reviewer", "Went back.", DateTime.UtcNow));

        // B keeps its screen, and A recorded no restore that did not happen.
        var owner = (await repository.GetAssignmentsAsync(venueId)).Single(a => a.ScreenId == screenId).MenuId;
        Assert.Equal(menuB, owner);
        Assert.DoesNotContain(
            await repository.GetHistoryAsync(venueId, menuA, 20),
            entry => entry.Kind == MenuHistoryKinds.Restored);
    }

    [Fact]
    public async Task Restore_AgainstAForeignVenue_ChangesNothingAndRecordsNothing()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        Assert.DoesNotContain(
            await repository.GetHistoryAsync(venueB, menuB, 10),
            entry => entry.Kind == MenuHistoryKinds.Restored);
    }

    // ---- cross-tenant access ----------------------------------------------------

    [Fact]
    public async Task ForeignMenuId_CannotBePublishedByAnotherVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuB = await SeedMenuAsync(dataAccess, venueB);

        await Assert.ThrowsAnyAsync<Exception>(() => repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueA, MenuId = menuB, PublishedUtc = DateTime.UtcNow }, "[]", "{}", null, 0));

        // B's version line is untouched: A did not consume a version number.
        Assert.Empty(await repository.GetPublishHistoryAsync(venueB, menuB, 10));
    }

    // Review finding #5: the item update names the venue in its WHERE clause, so
    // venue A carrying venue B's item id changes nothing.
    [Fact]
    public async Task UpdateItemAsync_CannotReachAForeignVenueItem()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueA = await SeedVenueAsync(dataAccess);
        var venueB = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueA);
        var screenA = await SeedScreenAsync(dataAccess, venueA);
        var screenB = await SeedScreenAsync(dataAccess, venueB);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueA, ScreenId = screenA, MenuId = menuA }));
        var published = (await PublishCurrentAsync(repository, venueA, menuA)).Event;

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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuA = await SeedMenuAsync(dataAccess, venueId);
        var menuB = await SeedMenuAsync(dataAccess, venueId);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuA }));
        var publishedOnA = (await PublishCurrentAsync(repository, venueId, menuA)).Event;

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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);

        var refused = await repository.CreateMenuWithinCeilingAsync(
            new Menu { VenueId = venueId, Name = fixture.UniqueValue("second") }, activeMenuLimit: 1);
        Assert.False(refused.Created);
        Assert.Equal(1, refused.ActiveMenuCount);

        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            "UPDATE dbo.Menus SET IsPutAway = 1 WHERE Id = @MenuId; SELECT 1 AS Value;",
            new { MenuId = menuId });

        var newMenu = new Menu { VenueId = venueId, Name = fixture.UniqueValue("second") };
        var admitted = await repository.CreateMenuWithinCeilingAsync(newMenu, activeMenuLimit: 1);
        Assert.True(admitted.Created);
        var section = Assert.Single(await dataAccess.ExecuteSqlQueryAsync<SectionSeedRow, object>(
            "SELECT Name, PageId FROM dbo.MenuSections WHERE VenueId=@VenueId AND MenuId=@MenuId;",
            new { VenueId = venueId, MenuId = newMenu.Id }));
        Assert.Equal("Section 1", section.Name);
        Assert.NotEqual(Guid.Empty, section.PageId);
    }

    [Fact]
    public async Task MenuCeiling_UnderConcurrency_AdmitsExactlyTheRoomLeft()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
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

    [Fact]
    public async Task RestoreAllAvailability_ChangesEveryOffItemInOneVenueAndNoOtherVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var otherVenueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId);
        var first = new Item { VenueId = venueId, Name = fixture.UniqueValue("first") };
        var second = new Item { VenueId = venueId, Name = fixture.UniqueValue("second") };
        var hidden = new Item { VenueId = venueId, Name = fixture.UniqueValue("hidden") };
        await repository.CreateItemOnMenuAsync(first, menuId, sectionId, 500);
        await repository.CreateItemOnMenuAsync(second, menuId, sectionId, 500);
        await repository.CreateItemAsync(hidden);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);
        var otherMenuId = await SeedMenuAsync(dataAccess, otherVenueId);
        var otherSectionId = await SeedSectionAsync(dataAccess, otherVenueId, otherMenuId);
        var other = new Item { VenueId = otherVenueId, Name = fixture.UniqueValue("other") };
        await repository.CreateItemOnMenuAsync(other, otherMenuId, otherSectionId, 500);
        foreach (var item in new[] { first, second, hidden, other })
            await repository.SetAvailabilityAsync(new ItemAvailability { VenueId = item.VenueId, ItemId = item.Id, IsAvailable = false, ChangedBy = "Chef" });

        var changed = await repository.RestoreAllAvailabilityAsync(venueId, DateTime.UtcNow, "Owner");

        Assert.Equal(2, changed.Count);
        Assert.All(changed, state => Assert.True(state.IsAvailable));
        Assert.False((await repository.GetAvailabilityAsync(venueId)).Single(state => state.ItemId == hidden.Id).IsAvailable);
        Assert.False(Assert.Single(await repository.GetAvailabilityAsync(otherVenueId)).IsAvailable);
    }

    // ---- helpers ----------------------------------------------------------------------

    // ---- the builder's writes -------------------------------------------------

    [Fact]
    public async Task AddSection_LandsAtTheEndAndShowsUpAsADraftChange()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));
        await PublishCurrentAsync(repository, venueId, menuId);

        var added = Guid.NewGuid();
        var outcome = await repository.CreateSectionOnMenuAsync(venueId, menuId, added, "Puddings", DateTime.UtcNow);

        Assert.Equal(SectionOutcomes.Created, outcome.Outcome);
        Assert.Equal(1, outcome.SortOrder);

        // The builder writes working state and nothing else: the draft follows on
        // its own, because it is the difference from what the screens are showing.
        var snapshots = await repository.GetDraftSnapshotsAsync(venueId, menuId);
        Assert.Contains(
            MenuSnapshot.Diff(snapshots.Published, snapshots.Working),
            change => change.TargetId == added);
    }

    [Fact]
    public async Task SectionChanges_RecordOnlyTheirPageAndRefusalsRecordNothing()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var existing = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var page = Assert.Single(await repository.GetPagesAsync(venueId, menuId));
        var now = DateTime.UtcNow;
        var added = Guid.NewGuid();

        Assert.Equal(SectionOutcomes.Created, (await repository.CreateSectionOnMenuAsync(
            venueId, menuId, added, "Desserts", now, page.Id, "Jeremy")).Outcome);
        Assert.True(await repository.RenameSectionAsync(
            venueId, menuId, added, "Sweet things", now.AddSeconds(1), "Jeremy"));
        Assert.True(await repository.RenameSectionAsync(
            venueId, menuId, added, "SWEET THINGS", now.AddSeconds(2), "Jeremy"));
        Assert.Contains(MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!.Sections!,
            section => section.SectionId == added && section.Name == "SWEET THINGS");
        Assert.True(await repository.RenameSectionAsync(
            venueId, menuId, added, "SWEET THINGS", now.AddSeconds(3), "Jeremy"));
        Assert.Equal(ReorderOutcomes.Reordered, (await repository.ReorderSectionsGuardedAsync(
            venueId, menuId, [added, existing], now.AddSeconds(4), "Jeremy")).Outcome);
        await repository.CreateItemOnMenuAsync(new Item
        {
            Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("history-item"), Price = "8"
        }, menuId, added, itemsPerMenuLimit: 500);

        var historyBeforeRefusal = await repository.GetPageHistoryAsync(venueId, menuId, page.Id, 20);

        // A stale destination is refused before either the content or its history
        // can change. This is the atomicity boundary Slice 2 depends on.
        Assert.Equal(SectionOutcomes.DestinationMissing, (await repository.DeleteSectionAsync(
            venueId, menuId, added, Guid.NewGuid(), false, "Jeremy", now.AddSeconds(5))).Outcome);

        var history = await repository.GetPageHistoryAsync(venueId, menuId, page.Id, 20);
        Assert.Equal(historyBeforeRefusal.Select(entry => entry.Id), history.Select(entry => entry.Id));
        Assert.Equal(
            [MenuHistoryKinds.SectionsReordered, MenuHistoryKinds.SectionRenamed, MenuHistoryKinds.SectionRenamed, MenuHistoryKinds.ItemAdded, MenuHistoryKinds.SectionAdded],
            history.Select(entry => entry.Kind).ToArray());
        Assert.All(history, entry =>
        {
            Assert.Equal(page.Id, entry.PageId);
            Assert.Equal(page.Name, entry.PageName);
            Assert.Equal(entry.Kind == MenuHistoryKinds.ItemAdded ? null : "Jeremy", entry.Author);
            Assert.NotEqual(MenuHistoryKinds.Published, entry.Kind);
        });
        Assert.DoesNotContain(history, entry => entry.Kind == MenuHistoryKinds.SectionDeleted);

        Assert.Equal(SectionOutcomes.Deleted, (await repository.DeleteSectionAsync(
            venueId, menuId, added, null, true, "Jeremy", now.AddSeconds(6))).Outcome);
        Assert.Equal(MenuHistoryKinds.SectionDeleted,
            (await repository.GetPageHistoryAsync(venueId, menuId, page.Id, 20)).First().Kind);

        Assert.Empty(await repository.GetPageHistoryAsync(Guid.NewGuid(), menuId, page.Id, 20));
        Assert.Empty(await repository.GetPageHistoryAsync(venueId, Guid.NewGuid(), page.Id, 20));
        Assert.Empty(await repository.GetPageHistoryAsync(venueId, menuId, Guid.NewGuid(), 20));
    }

    [Fact]
    public async Task AddSection_RefusesAMenuOfAnotherVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var mine = await SeedVenueAsync(dataAccess);
        var theirs = await SeedVenueAsync(dataAccess);
        var theirMenu = await SeedMenuAsync(dataAccess, theirs);

        var outcome = await repository.CreateSectionOnMenuAsync(mine, theirMenu, Guid.NewGuid(), "Theirs", DateTime.UtcNow);

        Assert.Equal(SectionOutcomes.MenuMissing, outcome.Outcome);
    }

    /// <summary>
    /// Q96's "nothing is lost", proved on rows: the placements go, the items do not.
    /// </summary>
    [Fact]
    public async Task DeleteSection_ReleasesItsItemsBackToTheLibrary()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        var first = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("wings"), Price = "12" };
        var second = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("fries"), Price = "6" };
        await repository.CreateItemOnMenuAsync(first, menuId, sectionId, itemsPerMenuLimit: 500);
        await repository.CreateItemOnMenuAsync(second, menuId, sectionId, itemsPerMenuLimit: 500);

        var outcome = await repository.DeleteSectionAsync(venueId, menuId, sectionId, null, deletePlacements: true);

        Assert.Equal(SectionOutcomes.Deleted, outcome.Outcome);
        Assert.Equal(2, outcome.ReleasedItemCount);
        Assert.Empty(await repository.GetPlacementsAsync(venueId, menuId));
        Assert.NotNull(await repository.GetItemAsync(venueId, first.Id));
        Assert.NotNull(await repository.GetItemAsync(venueId, second.Id));
    }

    [Fact]
    public async Task DeleteSection_MovesEveryPlacementToASiblingAtomically()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var destination = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 1);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("move-me"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, menuId, source, itemsPerMenuLimit: 500);

        var outcome = await repository.DeleteSectionAsync(venueId, menuId, source, destination, deletePlacements: false);

        Assert.Equal(SectionOutcomes.Deleted, outcome.Outcome);
        Assert.Equal(1, outcome.MovedItemCount);
        Assert.Equal(0, outcome.ReleasedItemCount);
        Assert.All(await repository.GetPlacementsAsync(venueId, menuId), placement => Assert.Equal(destination, placement.MenuSectionId));
    }

    [Fact]
    public async Task DeleteSection_RefusesAStaleDestinationWithoutChangingAnything()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("stay-put"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, menuId, source, itemsPerMenuLimit: 500);

        var outcome = await repository.DeleteSectionAsync(venueId, menuId, source, Guid.NewGuid(), deletePlacements: false);

        Assert.Equal(SectionOutcomes.DestinationMissing, outcome.Outcome);
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.MenuSectionId == source && placement.ItemId == item.Id);
        Assert.Contains(MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!.Sections!, section => section.SectionId == source);
    }

    /// <summary>
    /// #797: the destination no longer has to be on the same page as the section
    /// being deleted - this is what #809 found blocked and left for a decision.
    /// </summary>
    [Fact]
    public async Task DeleteSection_CanMoveItemsToASectionOnAnotherPage()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var secondPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, secondPage, "Dinner", DateTime.UtcNow);
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var destination = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, destination, "Dinner mains", DateTime.UtcNow, secondPage);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("cross-page-move"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, menuId, source, itemsPerMenuLimit: 500);

        var outcome = await repository.DeleteSectionAsync(venueId, menuId, source, destination, deletePlacements: false);

        Assert.Equal(SectionOutcomes.Deleted, outcome.Outcome);
        Assert.Equal(1, outcome.MovedItemCount);
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.MenuSectionId == destination && placement.ItemId == item.Id);
    }

    /// <summary>
    /// #797: the conflict check has to match the widened destination - once a
    /// cross-page destination is allowed, checking only the one destination
    /// section (the old check) would let an item land twice on the same page in
    /// two different sections, which is exactly what already_on_board prevents
    /// for a plain add-item.
    /// </summary>
    [Fact]
    public async Task DeleteSection_RefusesWhenAnItemAlreadySitsElsewhereOnTheDestinationPage()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var secondPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, secondPage, "Dinner", DateTime.UtcNow);
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var destination = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, destination, "Dinner mains", DateTime.UtcNow, secondPage);
        var elsewhereOnDestinationPage = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, elsewhereOnDestinationPage, "Dinner sides", DateTime.UtcNow, secondPage);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("page-collision"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, menuId, source, itemsPerMenuLimit: 500);
        // Same item, already placed in a DIFFERENT section on the destination page -
        // not the destination section itself, which the old (pre-#797) check would
        // have missed.
        await repository.PlaceExistingItemAsync(venueId, menuId, elsewhereOnDestinationPage, item.Id, itemsPerMenuLimit: 500, now: DateTime.UtcNow);

        var outcome = await repository.DeleteSectionAsync(venueId, menuId, source, destination, deletePlacements: false);

        Assert.Equal(SectionOutcomes.DestinationConflict, outcome.Outcome);
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.MenuSectionId == source && placement.ItemId == item.Id);
    }

    /// <summary>#797: a section moves intact - same items, same order - to a different page, appended at the end.</summary>
    [Fact]
    public async Task MoveSectionToPage_RelocatesTheSectionIntactWithItsItems()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var destinationPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, destinationPage, "Dinner", DateTime.UtcNow);
        await repository.CreateSectionOnMenuAsync(venueId, menuId, Guid.NewGuid(), "Already there", DateTime.UtcNow, destinationPage);
        var section = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("travels-with-section"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, menuId, section, itemsPerMenuLimit: 500);

        var outcome = await repository.MoveSectionToPageAsync(venueId, menuId, section, destinationPage, "Jeremy");

        Assert.Equal(SectionOutcomes.Moved, outcome.Outcome);
        var snapshot = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!;
        var moved = Assert.Single(snapshot.Sections!, s => s.SectionId == section);
        Assert.Equal(destinationPage, moved.PageId);
        Assert.Equal(1, moved.SortOrder); // appended after the section already on that page (sort order 0)
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.MenuSectionId == section && placement.ItemId == item.Id);
        Assert.Equal(MenuHistoryKinds.SectionMoved,
            (await repository.GetPageHistoryAsync(venueId, menuId, destinationPage, 20)).First().Kind);
    }

    [Fact]
    public async Task MoveSectionToPage_RefusesTheSamePageWithoutChangingAnything()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var page = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var section = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        var outcome = await repository.MoveSectionToPageAsync(venueId, menuId, section, page, "Jeremy");

        Assert.Equal(SectionOutcomes.AlreadyOnPage, outcome.Outcome);
    }

    [Fact]
    public async Task MoveSectionToPage_RefusesAMissingSectionOrPage()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var page = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var section = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        Assert.Equal(SectionOutcomes.SectionMissing,
            (await repository.MoveSectionToPageAsync(venueId, menuId, Guid.NewGuid(), page, "Jeremy")).Outcome);
        Assert.Equal(SectionOutcomes.PageMissing,
            (await repository.MoveSectionToPageAsync(venueId, menuId, section, Guid.NewGuid(), "Jeremy")).Outcome);
    }

    /// <summary>
    /// The same per-page rule PlaceExistingItemSql enforces for a plain add-item
    /// (#797): moving a section must not land one of its items on a page that
    /// already has it, in a different section, refused atomically and naming what
    /// collided rather than a silent partial move.
    /// </summary>
    [Fact]
    public async Task MoveSectionToPage_RefusesWhenAnItemAlreadySitsOnTheDestinationPage()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var destinationPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, destinationPage, "Dinner", DateTime.UtcNow);
        var alreadyThere = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, alreadyThere, "Dinner mains", DateTime.UtcNow, destinationPage);
        var section = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("collides-on-move"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, menuId, section, itemsPerMenuLimit: 500);
        await repository.PlaceExistingItemAsync(venueId, menuId, alreadyThere, item.Id, itemsPerMenuLimit: 500, now: DateTime.UtcNow);

        var outcome = await repository.MoveSectionToPageAsync(venueId, menuId, section, destinationPage, "Jeremy");

        Assert.Equal(SectionOutcomes.DestinationConflict, outcome.Outcome);
        Assert.Equal(item.Id, outcome.ConflictItemId);
        Assert.Equal("Dinner mains", outcome.ConflictSectionName);
        var snapshot = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!;
        var untouched = Assert.Single(snapshot.Sections!, s => s.SectionId == section);
        Assert.NotEqual(destinationPage, untouched.PageId);
    }

    /// <summary>
    /// (MenuId, SortOrder) is unique, so a swap that writes both rows in one pass
    /// collides half-way through. The write parks every section out of the range
    /// first — this is the case that proves it.
    /// </summary>
    [Fact]
    public async Task ReorderSections_SwapsTwoWithoutCollidingOnTheUniqueSortOrder()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var first = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var second = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 1);

        var outcome = await repository.ReorderSectionsGuardedAsync(venueId, menuId, [second, first], DateTime.UtcNow);

        Assert.Equal(ReorderOutcomes.Reordered, outcome.Outcome);
        var sections = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!.Sections!;
        Assert.Equal([second, first], sections.Select(section => section.SectionId).ToArray());
    }

    /// <summary>
    /// The defect this guard exists for: the old path read the current set, checked
    /// it in C#, then wrote. A section added in that window was left out of the
    /// numbering and kept a stale sort order.
    /// </summary>
    [Fact]
    public async Task ReorderSections_RefusesAListThatNoLongerMatchesTheMenu()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var first = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var second = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 1);

        // Somebody else adds a section while this drag is in flight.
        await repository.CreateSectionOnMenuAsync(venueId, menuId, Guid.NewGuid(), "Late arrival", DateTime.UtcNow);

        var outcome = await repository.ReorderSectionsGuardedAsync(venueId, menuId, [second, first], DateTime.UtcNow);

        Assert.Equal(ReorderOutcomes.OrderStale, outcome.Outcome);

        // Refused whole: the two named sections keep the order they had, rather than
        // being half-applied around the one the caller never saw.
        var sections = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!.Sections!;
        Assert.Equal(first, sections[0].SectionId);
        Assert.Equal(second, sections[1].SectionId);
    }

    [Fact]
    public async Task ReorderItems_RefusesAListThatNoLongerMatchesTheSection()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        var first = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("a"), Price = "1" };
        var second = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("b"), Price = "2" };
        var third = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("c"), Price = "3" };
        await repository.CreateItemOnMenuAsync(first, menuId, sectionId, itemsPerMenuLimit: 500);
        await repository.CreateItemOnMenuAsync(second, menuId, sectionId, itemsPerMenuLimit: 500);
        await repository.CreateItemOnMenuAsync(third, menuId, sectionId, itemsPerMenuLimit: 500);

        // A list missing one placement is exactly the shape that used to leave the
        // omitted row at a stale sort order, colliding with a rewritten one.
        var outcome = await repository.ReorderPlacementsGuardedAsync(
            venueId, menuId, sectionId, [second.Id, first.Id], DateTime.UtcNow);

        Assert.Equal(ReorderOutcomes.OrderStale, outcome.Outcome);

        var items = MenuSnapshot.Parse(await repository.GetWorkingSnapshotAsync(venueId, menuId))!
            .Sections!.Single().Items!;
        Assert.Equal([first.Id, second.Id, third.Id], items.Select(item => item.ItemId).ToArray());
    }

    /// <summary>
    /// The guard that makes Undo safe. An inverse write carries the values it
    /// expects to find; if somebody else has edited the item since, it refuses and
    /// changes nothing rather than restoring an older value over their work.
    /// </summary>
    [Fact]
    public async Task GuardedItemEdit_RefusesWhenSomebodyElseChangedItFirst()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        var item = new Item
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = fixture.UniqueValue("chowder"),
            Description = "with oyster crackers",
            Price = "12.50"
        };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, itemsPerMenuLimit: 500);

        // Editor A raises the price. This is a plain edit — no expectation.
        var raised = await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, item.Name, item.Description, "14.00", expected: null, DateTime.UtcNow);
        Assert.Equal("updated", raised.Outcome);

        // Editor B, elsewhere, edits the same item afterwards.
        var theirs = await repository.UpdateItemValuesGuardedAsync(
            venueId, item.Id, "Editor B name", "editor B later", "99", expected: null, DateTime.UtcNow);
        Assert.Equal("updated", theirs.Outcome);

        // Editor A presses Undo. It expects to find what A wrote, and does not.
        var undone = await repository.UpdateItemValuesGuardedAsync(
            venueId,
            item.Id,
            item.Name,
            item.Description,
            "12.50",
            new ItemValueExpectation(item.Name, item.Description, "14.00"),
            DateTime.UtcNow);

        Assert.Equal("item_changed", undone.Outcome);

        // B's values are intact — all three of them, not just the guarded one.
        var live = await repository.GetItemAsync(venueId, item.Id);
        Assert.NotNull(live);
        Assert.Equal("Editor B name", live!.Name);
        Assert.Equal("editor B later", live.Description);
        Assert.Equal("99", live.Price);
    }

    /// <summary>
    /// The same guard, satisfied. NULL and empty are one absence: the API normalises
    /// an empty description to NULL going in, so an Undo echoing back what it was
    /// handed must not refuse itself over which of the two it sent.
    /// </summary>
    [Fact]
    public async Task GuardedItemEdit_AppliesWhenNothingMovedAndTreatsEmptyAsNull()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        var item = new Item
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = fixture.UniqueValue("soda"),
            Description = null,
            Price = "3.00"
        };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, itemsPerMenuLimit: 500);

        var undone = await repository.UpdateItemValuesGuardedAsync(
            venueId,
            item.Id,
            item.Name,
            null,
            "2.50",
            new ItemValueExpectation(item.Name, string.Empty, "3.00"),
            DateTime.UtcNow);

        Assert.Equal("updated", undone.Outcome);

        var live = await repository.GetItemAsync(venueId, item.Id);
        Assert.Equal("2.50", live!.Price);
    }

    /// <summary>
    /// Q112: an item already on this board is not a second copy and not an error.
    /// The caller is told which section it sits in, so the UI can jump there.
    /// </summary>
    [Fact]
    public async Task PlacingAnItemAlreadyOnTheBoard_SaysWhereItIsAndPlacesNothing()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var starters = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var mains = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 1);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("olives"), Price = "7" };
        await repository.CreateItemOnMenuAsync(item, menuId, starters, itemsPerMenuLimit: 500);

        var outcome = await repository.PlaceExistingItemAsync(
            venueId, menuId, mains, item.Id, itemsPerMenuLimit: 500, DateTime.UtcNow);

        Assert.Equal(PlaceExistingOutcomes.AlreadyOnBoard, outcome.Outcome);
        Assert.Equal(starters, outcome.ExistingSectionId);
        Assert.Single(await repository.GetPlacementsAsync(venueId, menuId));
    }

    [Fact]
    public async Task PlacingAnExistingItem_LandsAtTheEndAndCountsAgainstTheCeiling()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);

        var placed = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("first"), Price = "1" };
        await repository.CreateItemOnMenuAsync(placed, menuId, sectionId, itemsPerMenuLimit: 500);

        // A library item that is on no board yet.
        var loose = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("loose"), Price = "2" };
        await repository.CreateItemAsync(loose);

        var ok = await repository.PlaceExistingItemAsync(
            venueId, menuId, sectionId, loose.Id, itemsPerMenuLimit: 500, DateTime.UtcNow);

        Assert.Equal(PlaceExistingOutcomes.Placed, ok.Outcome);
        Assert.Equal(1, ok.SortOrder);
        Assert.Equal(2, ok.ItemCountOnMenu);

        // At the ceiling the refusal is the outcome, not an exception and not a
        // partial write.
        var third = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("third"), Price = "3" };
        await repository.CreateItemAsync(third);
        var refused = await repository.PlaceExistingItemAsync(
            venueId, menuId, sectionId, third.Id, itemsPerMenuLimit: 2, DateTime.UtcNow);

        Assert.Equal(PlaceExistingOutcomes.CeilingReached, refused.Outcome);
        Assert.Equal(2, (await repository.GetPlacementsAsync(venueId, menuId)).Count);
    }

    [Fact]
    public async Task RemovingAnItemFromABoard_LeavesItInTheLibraryAndOnOtherBoards()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var lunch = await SeedMenuAsync(dataAccess, venueId);
        var dinner = await SeedMenuAsync(dataAccess, venueId);
        var lunchSection = await SeedSectionAsync(dataAccess, venueId, lunch, sortOrder: 0);
        var dinnerSection = await SeedSectionAsync(dataAccess, venueId, dinner, sortOrder: 0);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("shared"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, lunch, lunchSection, itemsPerMenuLimit: 500);
        await repository.PlaceExistingItemAsync(venueId, dinner, dinnerSection, item.Id, 500, DateTime.UtcNow);

        var lunchPage = (await repository.GetPlacementsAsync(venueId, lunch)).Single().PageId;
        Assert.True(await repository.RemoveItemFromPageAsync(venueId, lunch, lunchPage, item.Id, DateTime.UtcNow));

        Assert.Empty(await repository.GetPlacementsAsync(venueId, lunch));
        Assert.Single(await repository.GetPlacementsAsync(venueId, dinner));
        Assert.NotNull(await repository.GetItemAsync(venueId, item.Id));
    }

    [Fact]
    public async Task MovingAnItemAcrossSections_CommitsBothOrdersAndOnePageHistoryFact()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var pageId = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, 0);
        var destination = await SeedSectionAsync(dataAccess, venueId, menuId, 1);
        var moved = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("move") };
        var stays = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("stay") };
        await repository.CreateItemOnMenuAsync(moved, menuId, source, 500);
        await repository.CreateItemOnMenuAsync(stays, menuId, destination, 500);

        var outcome = await repository.MovePlacementGuardedAsync(
            venueId, menuId, moved.Id, source, destination, [], [stays.Id, moved.Id],
            DateTime.UtcNow, author: "Jeremy");

        Assert.Equal(ReorderOutcomes.Reordered, outcome.Outcome);
        var placements = await repository.GetPlacementsAsync(venueId, menuId);
        Assert.DoesNotContain(placements, placement => placement.MenuSectionId == source);
        Assert.Equal([stays.Id, moved.Id], placements.Where(p => p.MenuSectionId == destination).OrderBy(p => p.SortOrder).Select(p => p.ItemId));
        var history = await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50);
        Assert.Single(history, entry => entry.Kind == MenuHistoryKinds.ItemMoved && entry.Detail!.Contains(moved.Name));
    }

    [Fact]
    public async Task RemovingFromOnePage_PreservesTheSameItemOnAnotherPage()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var firstPage = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var secondPage = Guid.NewGuid();
        Assert.NotNull(await repository.CreatePageAsync(venueId, menuId, secondPage, "Dinner", DateTime.UtcNow));
        var firstSection = await SeedSectionAsync(dataAccess, venueId, menuId, 0);
        var secondSection = Guid.NewGuid();
        Assert.Equal("created", (await repository.CreateSectionOnMenuAsync(venueId, menuId, secondSection, "Dinner", DateTime.UtcNow, secondPage)).Outcome);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("shared-page") };
        await repository.CreateItemOnMenuAsync(item, menuId, firstSection, 500);
        Assert.Equal(PlaceExistingOutcomes.Placed, (await repository.PlaceExistingItemAsync(venueId, menuId, secondSection, item.Id, 500, DateTime.UtcNow)).Outcome);

        Assert.True(await repository.RemoveItemFromPageAsync(venueId, menuId, firstPage, item.Id, DateTime.UtcNow, author: "Jeremy"));

        var remaining = await repository.GetPlacementsAsync(venueId, menuId);
        Assert.Single(remaining);
        Assert.Equal(secondPage, remaining.Single().PageId);
        Assert.Single(await repository.GetPageHistoryAsync(venueId, menuId, firstPage, 50), entry => entry.Kind == MenuHistoryKinds.ItemRemoved);
    }

    [Fact]
    public async Task MovingAcrossPagesOrWithAStaleOrder_ChangesNothingAndWritesNoHistory()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var firstPage = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var secondPage = Guid.NewGuid();
        await repository.CreatePageAsync(venueId, menuId, secondPage, "Dinner", DateTime.UtcNow);
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, 0);
        var sibling = await SeedSectionAsync(dataAccess, venueId, menuId, 1);
        var otherPageSection = Guid.NewGuid();
        await repository.CreateSectionOnMenuAsync(venueId, menuId, otherPageSection, "Other page", DateTime.UtcNow, secondPage);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("guarded-move") };
        await repository.CreateItemOnMenuAsync(item, menuId, source, 500);

        var crossPage = await repository.MovePlacementGuardedAsync(
            venueId, menuId, item.Id, source, otherPageSection, [], [item.Id], DateTime.UtcNow, author: "Jeremy");
        var stale = await repository.MovePlacementGuardedAsync(
            venueId, menuId, item.Id, source, sibling, [Guid.NewGuid()], [item.Id], DateTime.UtcNow, author: "Jeremy");
        var survivor = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("survivor") };
        await repository.CreateItemOnMenuAsync(survivor, menuId, source, 500);
        var historyBefore = (await repository.GetPageHistoryAsync(venueId, menuId, firstPage, 50)).Count;
        var malformed = await repository.MovePlacementGuardedAsync(
            venueId, menuId, item.Id, source, sibling, [item.Id], [item.Id], DateTime.UtcNow, author: "Jeremy");

        Assert.Equal(ReorderOutcomes.OrderStale, crossPage.Outcome);
        Assert.Equal(ReorderOutcomes.OrderStale, stale.Outcome);
        Assert.Equal(ReorderOutcomes.OrderStale, malformed.Outcome);
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.ItemId == item.Id && placement.MenuSectionId == source);
        Assert.Equal(historyBefore, (await repository.GetPageHistoryAsync(venueId, menuId, firstPage, 50)).Count);
    }

    [Fact]
    public async Task RepeatingPageRemoval_IsAnIdempotentNoOpWithOneHistoryFact()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var pageId = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, 0);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("idempotent-remove") };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, 500);

        Assert.True(await repository.RemoveItemFromPageAsync(venueId, menuId, pageId, item.Id, DateTime.UtcNow, author: "Jeremy"));
        Assert.False(await repository.RemoveItemFromPageAsync(venueId, menuId, pageId, item.Id, DateTime.UtcNow, author: "Jeremy"));

        Assert.Single(await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50), entry => entry.Kind == MenuHistoryKinds.ItemRemoved);
    }

    [Fact]
    public async Task PlacementUndoAndRedo_RefuseStaleSecondActorStateWithoutChangingPlacementOrHistory()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var pageId = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var source = await SeedSectionAsync(dataAccess, venueId, menuId, 0);
        var sibling = await SeedSectionAsync(dataAccess, venueId, menuId, 1);
        var first = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("first") };
        var removed = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("removed") };
        var last = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("last") };
        await repository.CreateItemOnMenuAsync(first, menuId, source, 500);
        await repository.CreateItemOnMenuAsync(removed, menuId, source, 500);
        await repository.CreateItemOnMenuAsync(last, menuId, source, 500);
        Assert.True(await repository.RemoveItemFromPageAsync(venueId, menuId, pageId, removed.Id, DateTime.UtcNow));

        // A second actor re-adds it elsewhere before the first actor presses Undo.
        Assert.Equal(PlaceExistingOutcomes.Placed,
            (await repository.PlaceExistingItemAsync(venueId, menuId, sibling, removed.Id, 500, DateTime.UtcNow)).Outcome);
        var beforeUndoHistory = (await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50)).Count;
        var staleUndo = await repository.TransitionPlacementGuardedAsync(
            venueId, menuId, pageId, source, removed.Id,
            [first.Id, last.Id], [first.Id, removed.Id, last.Id], 500, DateTime.UtcNow, author: "stale actor");
        Assert.Equal(ReorderOutcomes.OrderStale, staleUndo.Outcome);
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId),
            placement => placement.ItemId == removed.Id && placement.MenuSectionId == sibling);
        Assert.Equal(beforeUndoHistory, (await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50)).Count);

        // Put the item back into the exact state Undo expected, perform Undo, then
        // let another actor move it before Redo. Redo must not remove their work.
        Assert.Equal(ReorderOutcomes.Reordered, (await repository.MovePlacementGuardedAsync(
            venueId, menuId, removed.Id, sibling, source, [], [first.Id, removed.Id, last.Id],
            DateTime.UtcNow)).Outcome);
        Assert.Equal(ReorderOutcomes.Reordered, (await repository.MovePlacementGuardedAsync(
            venueId, menuId, removed.Id, source, sibling, [first.Id, last.Id], [removed.Id],
            DateTime.UtcNow)).Outcome);
        var beforeRedoHistory = (await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50)).Count;
        var staleRedo = await repository.TransitionPlacementGuardedAsync(
            venueId, menuId, pageId, source, removed.Id,
            [first.Id, removed.Id, last.Id], [first.Id, last.Id], 500, DateTime.UtcNow, author: "stale actor");
        Assert.Equal(ReorderOutcomes.OrderStale, staleRedo.Outcome);
        Assert.Contains(await repository.GetPlacementsAsync(venueId, menuId),
            placement => placement.ItemId == removed.Id && placement.MenuSectionId == sibling);
        Assert.Equal(beforeRedoHistory, (await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50)).Count);
    }

    [Fact]
    public async Task PlacementTransition_EnforcesDistinctItemCeilingWithoutWritingPlacementOrHistory()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var pageId = (await repository.GetPagesAsync(venueId, menuId)).Single().Id;
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, 0);
        var existing = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("at-limit") };
        var candidate = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("over-limit") };
        await repository.CreateItemOnMenuAsync(existing, menuId, sectionId, 500);
        await repository.CreateItemAsync(candidate);
        var historyBefore = (await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50)).Count;

        var outcome = await repository.TransitionPlacementGuardedAsync(
            venueId, menuId, pageId, sectionId, candidate.Id,
            [existing.Id], [existing.Id, candidate.Id], itemsPerMenuLimit: 1, now: DateTime.UtcNow, author: "caller");

        Assert.Equal(PlaceExistingOutcomes.CeilingReached, outcome.Outcome);
        Assert.DoesNotContain(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.ItemId == candidate.Id);
        Assert.Equal(historyBefore, (await repository.GetPageHistoryAsync(venueId, menuId, pageId, 50)).Count);
    }

    /// <summary>
    /// Wildcards typed into a search box are characters, not operators: somebody
    /// looking for "50% off" should not be shown the whole library.
    /// </summary>
    [Fact]
    public async Task SearchingTheLibrary_TreatsTypedWildcardsAsText()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);

        var literal = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("50% off pitcher"), Price = "20" };
        var other = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("house red"), Price = "8" };
        await repository.CreateItemAsync(literal);
        await repository.CreateItemAsync(other);

        var hits = await repository.SearchItemsAsync(venueId, "%", take: 50);

        Assert.DoesNotContain(hits, item => item.Id == other.Id);
        Assert.Contains(hits, item => item.Id == literal.Id);
    }

    [Fact]
    public async Task SearchingTheLibrary_FindsItemsThatAreOffRightNow()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);

        var name = fixture.UniqueValue("berry fizz");
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = name, Price = "9" };
        await repository.CreateItemAsync(item);
        await repository.SetAvailabilityAsync(new ItemAvailability
        {
            VenueId = venueId,
            ItemId = item.Id,
            IsAvailable = false,
            ChangedUtc = DateTime.UtcNow,
            ChangedBy = "Alex"
        });

        // An 86'd item is still findable and still placeable (Q112) - it is off, not
        // gone.
        Assert.Contains(await repository.SearchItemsAsync(venueId, name, take: 20), found => found.Id == item.Id);
    }

    [Fact]
    public async Task SearchingTheLibrary_CanonicalisesAmpersandsAndPunctuationInSql()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("Fish & Chips!") };
        await repository.CreateItemAsync(item);

        var query = item.Name.Replace("&", "and", StringComparison.Ordinal).Replace("!", "", StringComparison.Ordinal);
        Assert.Contains(await repository.SearchItemsAsync(venueId, query, 20), hit => hit.Id == item.Id);
    }

    /// <summary>
    /// "Also on Late Night" (Q123) reads menu names from the rows that own them, so
    /// a renamed menu cannot be described by a stale label.
    /// </summary>
    [Fact]
    public async Task ItemBoards_NameEveryBoardAnItemSitsOn()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var lunch = await SeedMenuAsync(dataAccess, venueId);
        var dinner = await SeedMenuAsync(dataAccess, venueId);
        var lunchSection = await SeedSectionAsync(dataAccess, venueId, lunch, sortOrder: 0);
        var dinnerSection = await SeedSectionAsync(dataAccess, venueId, dinner, sortOrder: 0);

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("shared"), Price = "9" };
        await repository.CreateItemOnMenuAsync(item, lunch, lunchSection, itemsPerMenuLimit: 500);
        await repository.PlaceExistingItemAsync(venueId, dinner, dinnerSection, item.Id, 500, DateTime.UtcNow);

        var boards = await repository.GetItemBoardsAsync(venueId, [item.Id]);

        Assert.Equal(2, boards.Count);
        Assert.Contains(boards, board => board.MenuId == lunch);
        Assert.Contains(boards, board => board.MenuId == dinner);
        Assert.All(boards, board => Assert.False(string.IsNullOrWhiteSpace(board.MenuName)));
    }

    /// <summary>
    /// The canvas draws the WORKING board and the publish bar describes the
    /// published one. Both come from a single read, so the two can never describe
    /// different menus - the shape that produced a torn read once already.
    /// </summary>
    [Fact]
    public async Task TheBuilderRead_ReturnsTheWorkingBoardAndThePublishThatPutTheOtherOnScreen()
    {
        var dataAccess = fixture.CreateDataAccess();
        var repository = new ContentRepository(dataAccess);
        var venueId = await SeedVenueAsync(dataAccess);
        var menuId = await SeedMenuAsync(dataAccess, venueId);
        var sectionId = await SeedSectionAsync(dataAccess, venueId, menuId, sortOrder: 0);
        var screenId = await SeedScreenAsync(dataAccess, venueId);
        await repository.AssignScreenAsync(await WithFirstPageAsync(repository, new MenuScreenAssignment { VenueId = venueId, ScreenId = screenId, MenuId = menuId }));

        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("lemonade"), Price = "9.5" };
        await repository.CreateItemOnMenuAsync(item, menuId, sectionId, itemsPerMenuLimit: 500);
        var published = await PublishCurrentAsync(repository, venueId, menuId, author: "Dana");

        // An edit after the publish: the working board moves, the published one does not.
        item.Price = "10";
        await repository.UpdateItemAsync(item);

        var snapshots = await repository.GetDraftSnapshotsAsync(venueId, menuId);

        Assert.Equal(published.Event.Version, snapshots.PublishedVersion);
        Assert.Equal("Dana", snapshots.PublishedBy);
        Assert.NotNull(snapshots.PublishedUtc);

        Assert.Equal("10", MenuSnapshot.Parse(snapshots.Working)!.Sections![0].Items![0].Price);
        Assert.Equal("9.5", MenuSnapshot.Parse(snapshots.Published)!.Sections![0].Items![0].Price);
        Assert.Single(MenuSnapshot.Diff(snapshots.Published, snapshots.Working));
    }

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
            new { OrganizationId = organizationId, VenueId = venueId, OwnerUserId = Guid.NewGuid(), Name = fixture.UniqueValue("content") });
        return venueId;
    }

    private async Task<Guid> SeedMenuAsync(SqlDataAccess dataAccess, Guid venueId)
    {
        var menuId = Guid.NewGuid();
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.Menus (Id, VenueId, Name, IsActive, CreatedUtc, UpdatedUtc)
            VALUES (@MenuId, @VenueId, @Name, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
            INSERT dbo.MenuPages (Id,VenueId,MenuId,Name,SortOrder,CreatedUtc,UpdatedUtc)
            VALUES (NEWID(),@VenueId,@MenuId,N'Page 1',0,SYSUTCDATETIME(),SYSUTCDATETIME());
            SELECT 1 AS Value;
            """,
            new { MenuId = menuId, VenueId = venueId, Name = fixture.UniqueValue("menu") });
        return menuId;
    }

    private async Task<Guid> SeedSectionAsync(SqlDataAccess dataAccess, Guid venueId, Guid menuId, int sortOrder = 0)
    {
        var sectionId = Guid.NewGuid();
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            INSERT dbo.MenuSections (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            SELECT @SectionId,@VenueId,@MenuId,p.Id,@Name,@SortOrder,SYSUTCDATETIME(),SYSUTCDATETIME()
            FROM dbo.MenuPages p WHERE p.MenuId=@MenuId AND p.VenueId=@VenueId AND p.SortOrder=0;
            SELECT 1 AS Value;
            """,
            new { SectionId = sectionId, VenueId = venueId, MenuId = menuId, Name = fixture.UniqueValue("section"), SortOrder = sortOrder });
        return sectionId;
    }

    private static async Task<MenuScreenAssignment> WithFirstPageAsync(
        ContentRepository repository,
        MenuScreenAssignment assignment)
    {
        assignment.PageId = (await repository.GetPagesAsync(assignment.VenueId, assignment.MenuId))
            .OrderBy(page => page.SortOrder)
            .ThenBy(page => page.Id)
            .First().Id;
        return assignment;
    }

    /// <summary>
    /// Publishes the menu exactly as it stands, the way the service does: the diff
    /// and the snapshot are one observation, so the expected snapshot is the one
    /// just read.
    /// </summary>
    private static async Task<PublishOutcome> PublishCurrentAsync(
        ContentRepository repository,
        Guid venueId,
        Guid menuId,
        string? author = null)
    {
        var snapshots = await repository.GetDraftSnapshotsAsync(venueId, menuId);
        var changes = MenuSnapshot.Diff(snapshots.Published, snapshots.Working);
        return await repository.PublishAsync(
            new MenuPublishEvent { VenueId = venueId, MenuId = menuId, Author = author, PublishedUtc = DateTime.UtcNow },
            System.Text.Json.JsonSerializer.Serialize(changes),
            snapshots.Working!,
            snapshots.Published,
            snapshots.PublishedVersion);
    }

    /// <summary>
    /// Puts a published menu away the only way the model allows: take it off its
    /// screens, publish that so the screens actually let go, then shelve it. Each
    /// step is asserted, so a test arranged with this cannot quietly proceed from a
    /// refusal it did not expect. Returns the take-off publish.
    /// </summary>
    private static async Task<PublishOutcome> ShelveAsync(
        ContentRepository repository,
        Guid venueId,
        Guid menuId,
        string author = "Owner")
    {
        Assert.True(await repository.TakeOffScreensAsync(venueId, menuId, author, DateTime.UtcNow) > 0);
        var takeOff = await PublishCurrentAsync(repository, venueId, menuId, author);

        var outcome = await repository.SetPutAwayAsync(
            venueId, menuId, isPutAway: true, activeMenuLimit: 50, author, "Put the menu away.", DateTime.UtcNow);
        Assert.Equal(PutAwayOutcomes.Changed, outcome.Outcome);
        return takeOff;
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

    private sealed class SectionSeedRow
    {
        public string Name { get; set; } = string.Empty;
        public Guid PageId { get; set; }
    }
}

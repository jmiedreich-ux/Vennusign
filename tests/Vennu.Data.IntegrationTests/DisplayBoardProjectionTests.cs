using Vennu.Core.Models;
using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// What a screen shows, read from a real database against content stored the way
/// the builder actually stores it.
///
/// This exists because the unit coverage for the display seeded dbo.MenuItems and
/// asserted against it, so it passed for months while the product wrote content to
/// dbo.Items and dbo.Placements instead. Every menu built in the builder reached a
/// screen empty, and a green suite said otherwise. A fake agreeing with a test says
/// nothing about the schema; this asserts against the schema.
/// </summary>
[Trait("Category", "Integration")]
public class DisplayBoardProjectionTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture = fixture;

    [Fact]
    public async Task BoardItems_ComeFromPlacementsAndItems_NotTheLegacyMenuItemsTable()
    {
        var dataAccess = fixture.CreateDataAccess();
        {
            var venueId = Guid.NewGuid();
            var menuId = Guid.NewGuid();
            var pageId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var tuna = Guid.NewGuid();
            var ham = Guid.NewGuid();

            // Exactly the shape the builder produces: a catalogue item per row, placed
            // onto a section of a page. Nothing is written to dbo.MenuItems, because the
            // builder never writes there.
            await ExecuteAsync(dataAccess, $"""
                INSERT INTO dbo.Venues (Id, Name, Timezone, Type, PrimaryLanguage)
                VALUES ('{venueId}', 'My Bar', 'America/New_York', 'bar', 'en');

                INSERT INTO dbo.Menus (Id, VenueId, Name, IsActive, CreatedUtc, UpdatedUtc)
                VALUES ('{menuId}', '{venueId}', 'My Test Menu', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                INSERT INTO dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
                VALUES ('{pageId}', '{venueId}', '{menuId}', 'Weekday Menu', 0, SYSUTCDATETIME(), SYSUTCDATETIME());

                INSERT INTO dbo.MenuSections (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
                VALUES ('{sectionId}', '{venueId}', '{menuId}', '{pageId}', 'Lunch', 0, SYSUTCDATETIME(), SYSUTCDATETIME());

                INSERT INTO dbo.Items (Id, VenueId, Name, Description, Price, Source, IsActive, CreatedUtc, UpdatedUtc)
                VALUES ('{tuna}', '{venueId}', 'Tuna Fish', 'Onion, Lettuce and Pickles', 5.00, 'manual', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
                       ('{ham}',  '{venueId}', 'Hamsandwhich', 'On Wheat Bread', 5.50, 'manual', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                INSERT INTO dbo.Placements (Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
                VALUES (NEWID(), '{venueId}', '{menuId}', '{sectionId}', '{pageId}', '{tuna}', 0, SYSUTCDATETIME(), SYSUTCDATETIME()),
                       (NEWID(), '{venueId}', '{menuId}', '{sectionId}', '{pageId}', '{ham}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                """);

            // Typed as the interface so the defaulted board readers are reachable.
            IMenuRepository repository = new MenuRepository(dataAccess);

            var sections = await repository.GetSectionsForPageAsync(venueId, pageId);
            var section = Assert.Single(sections);
            Assert.Equal("Lunch", section.Name);

            var items = await repository.GetActiveBoardItemsAsync(venueId, sectionId);

            Assert.Equal(new[] { "Tuna Fish", "Hamsandwhich" }, items.Select(item => item.Name));
            Assert.Equal(5.00m, items.First().Price);
            Assert.Equal("On Wheat Bread", items.Last().Description);

            // An item with no availability row has never been 86'd, so it is available.
            Assert.All(items, item => Assert.True(item.IsAvailable));

            // And the proof that this is not the old path: the legacy table is empty.
            Assert.Equal(0, await ScalarAsync(dataAccess, "SELECT COUNT(*) AS Value FROM dbo.MenuItems WHERE VenueId = '" + venueId + "';"));

            // 86 the tuna; the board must reflect it rather than caching availability.
            await ExecuteAsync(dataAccess, $"""
                INSERT INTO dbo.ItemAvailability (VenueId, ItemId, IsAvailable, ChangedUtc, ChangedBy)
                VALUES ('{venueId}', '{tuna}', 0, SYSUTCDATETIME(), 'test');
                """);
            var after = await repository.GetActiveBoardItemsAsync(venueId, sectionId);
            Assert.False(after.First(item => item.Name == "Tuna Fish").IsAvailable);
            Assert.True(after.First(item => item.Name == "Hamsandwhich").IsAvailable);
        }
    }

    private static Task ExecuteAsync(ISqlDataAccess dataAccess, string sql) =>
        dataAccess.ExecuteSqlQueryAsync<CountRow, object>(sql + " SELECT 0 AS Value;", new { });

    /// The caller aliases the column as Value; appending an alias here would land
    /// after a WHERE clause and produce invalid SQL.
    private static async Task<int> ScalarAsync(ISqlDataAccess dataAccess, string sql) =>
        (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(sql, new { })).Single().Value;

    [Fact]
    public async Task A_price_as_a_menu_prints_it_reaches_the_screen_as_a_number()
    {
        /*
         * The owner published an imported menu and every price on the board read $0.00.
         *
         * A paste import stores the price exactly as the menu printed it - "$7.00" - because the
         * content model keeps prices as typed (Q115/Q190, so "MP" survives). The board projection
         * ran TRY_CONVERT over that, got NULL from the currency symbol, and ISNULL made it zero.
         * Not the rare "market price, a dash" the old comment anticipated: every imported price on
         * every board.
         *
         * The second half is A19: a price belongs to the placement and the menu it is printed on.
         * The import writes that to Placements.ImportedPriceOverride, and this projection read
         * straight past it, so a per-menu price never reached a screen at all.
         */
        var dataAccess = fixture.CreateDataAccess();
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var satay = Guid.NewGuid();
        var padThai = Guid.NewGuid();
        var market = Guid.NewGuid();

        await ExecuteAsync(dataAccess, $"""
            INSERT INTO dbo.Venues (Id, Name, Timezone, Type, PrimaryLanguage)
            VALUES ('{venueId}', 'Mana-Thai', 'America/New_York', 'bar', 'en');

            INSERT INTO dbo.Menus (Id, VenueId, Name, IsActive, CreatedUtc, UpdatedUtc)
            VALUES ('{menuId}', '{venueId}', 'Dinner', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

            INSERT INTO dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            VALUES ('{pageId}', '{venueId}', '{menuId}', 'Page 1', 0, SYSUTCDATETIME(), SYSUTCDATETIME());

            INSERT INTO dbo.MenuSections (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            VALUES ('{sectionId}', '{venueId}', '{menuId}', '{pageId}', 'Appetizers', 0, SYSUTCDATETIME(), SYSUTCDATETIME());

            -- Exactly what the importer writes: the symbol the menu was printed with.
            INSERT INTO dbo.Items (Id, VenueId, Name, Description, Price, Source, IsActive, CreatedUtc, UpdatedUtc)
            VALUES ('{satay}',    '{venueId}', 'Chicken Satay', NULL, N'$7.00',  'import', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
                   ('{padThai}',  '{venueId}', 'Pad Thai',      NULL, N'$11.95', 'import', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
                   ('{market}',   '{venueId}', 'Whole Fish',    NULL, N'MP',     'import', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

            -- Pad Thai carries a per-menu price (A19). The board must prefer it over the library default.
            INSERT INTO dbo.Placements (Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc, ImportedPriceOverride)
            VALUES (NEWID(), '{venueId}', '{menuId}', '{sectionId}', '{pageId}', '{satay}',   0, SYSUTCDATETIME(), SYSUTCDATETIME(), NULL),
                   (NEWID(), '{venueId}', '{menuId}', '{sectionId}', '{pageId}', '{padThai}', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), N'$13.50'),
                   (NEWID(), '{venueId}', '{menuId}', '{sectionId}', '{pageId}', '{market}',  2, SYSUTCDATETIME(), SYSUTCDATETIME(), NULL);
            """);

        IMenuRepository repository = new MenuRepository(dataAccess);
        var items = (await repository.GetActiveBoardItemsAsync(venueId, sectionId)).ToList();

        // The symbol no longer costs the guest the price.
        Assert.Equal(7.00m, items.Single(item => item.Name == "Chicken Satay").Price);

        // And the placement's own price wins over the library default.
        Assert.Equal(13.50m, items.Single(item => item.Name == "Pad Thai").Price);

        /*
         * A genuinely non-numeric price still lands as 0. That is the pre-existing behaviour and
         * it is still wrong on a guest screen - "MP" rendered as $0.00 is a different lie, tracked
         * on its own. Asserted here so the day it changes, this test says so rather than the owner.
         */
        Assert.Equal(0m, items.Single(item => item.Name == "Whole Fish").Price);
    }

    private sealed class CountRow
    {
        public int Value { get; set; }
    
}
}

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

    private sealed class CountRow
    {
        public int Value { get; set; }
    }
}

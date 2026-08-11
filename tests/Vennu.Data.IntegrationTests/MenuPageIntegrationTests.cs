using Vennu.Core.Models;
using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;
using Microsoft.Data.SqlClient;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class MenuPageIntegrationTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Cross_menu_rotation_survives_restoring_one_menus_snapshot()
    {
        var data = fixture.CreateDataAccess();
        var repository = new ContentRepository(data);
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await data.ExecuteSqlQueryAsync<Row, object>("""
            INSERT dbo.Venues (Id,Name,Timezone,Type,PrimaryLanguage) VALUES (@VenueId,N'Rotation venue',N'UTC',N'Test',N'en');
            INSERT dbo.Screens (Id,VenueId,ScreenKey,Name,Location,Status,Platform,AppVersion)
            VALUES (@ScreenId,@VenueId,LEFT(REPLACE(CONVERT(nvarchar(36),NEWID()),'-',''),8),N'Rotation screen',N'Wall',N'Offline',N'web',N'test');
            SELECT 1 Value;
            """, new { VenueId = venueId, ScreenId = screenId });
        try
        {
            var menuA = new Menu { Id=Guid.NewGuid(),VenueId=venueId,Name="Breakfast",CreatedUtc=now,UpdatedUtc=now };
            var menuB = new Menu { Id=Guid.NewGuid(),VenueId=venueId,Name="Dinner",CreatedUtc=now,UpdatedUtc=now };
            Assert.True((await repository.CreateMenuWithinCeilingAsync(menuA,10)).Created);
            Assert.True((await repository.CreateMenuWithinCeilingAsync(menuB,10)).Created);
            var pageA=Assert.Single(await repository.GetPagesAsync(venueId,menuA.Id));
            var pageB=Assert.Single(await repository.GetPagesAsync(venueId,menuB.Id));
            await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId=venueId,ScreenId=screenId,MenuId=menuA.Id,PageId=pageA.Id,AssignedUtc=now });
            var snapshotA=Assert.IsType<string>(await repository.GetWorkingSnapshotAsync(venueId,menuA.Id));
            await repository.AssignScreenAsync(new MenuScreenAssignment { VenueId=venueId,ScreenId=screenId,MenuId=menuB.Id,PageId=pageB.Id,Rotate=true,AssignedUtc=now });
            await repository.RestoreSnapshotAsync(venueId,menuA.Id,snapshotA,"integration","restore",now);
            var rotation=(await repository.GetAssignmentsAsync(venueId)).Where(a=>a.ScreenId==screenId).ToArray();
            Assert.Equal(2,rotation.Length);
            Assert.Contains(rotation,a=>a.PageId==pageA.Id && a.PageName=="Page 1");
            Assert.Contains(rotation,a=>a.PageId==pageB.Id && a.MenuName=="Dinner");
            Assert.True(await repository.ClearPageScreenAssignmentAsync(venueId,screenId,menuA.Id,pageA.Id));
            var remaining=Assert.Single((await repository.GetAssignmentsAsync(venueId)).Where(a=>a.ScreenId==screenId));
            Assert.Equal(pageB.Id,remaining.PageId);
        }
        finally
        {
            await repository.ResetAutomationVenueAsync(venueId);
            await data.ExecuteSqlQueryAsync<Row,object>("DELETE dbo.Venues WHERE Id=@VenueId; SELECT 1 Value;",new { VenueId=venueId });
        }
    }

    [Fact]
    public async Task Page_lifecycle_preserves_tenant_last_page_and_copy_invariants()
    {
        var data = fixture.CreateDataAccess();
        var repository = new ContentRepository(data);
        var venueId = Guid.NewGuid();
        var otherVenueId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await data.ExecuteSqlQueryAsync<Row, object>("""
            INSERT dbo.Venues (Id,Name,Timezone,Type,PrimaryLanguage) VALUES
              (@VenueId,N'Pages venue',N'UTC',N'Test',N'en'),
              (@OtherVenueId,N'Other venue',N'UTC',N'Test',N'en');
            SELECT 1 Value;
            """, new { VenueId = venueId, OtherVenueId = otherVenueId });

        try
        {
            var menuId = Guid.NewGuid();
            var menu = new Menu { Id = menuId, VenueId = venueId, Name = "Dinner", CreatedUtc = now, UpdatedUtc = now };
            Assert.True((await repository.CreateMenuWithinCeilingAsync(menu, 10)).Created);
            var first = Assert.Single(await repository.GetPagesAsync(venueId, menuId));

            Assert.False(await repository.RenamePageAsync(otherVenueId, menuId, first.Id, "Stolen", now));
            Assert.Equal("last_page", (await repository.DeletePageAsync(venueId, menuId, first.Id, null)).Outcome);

            var second = Assert.IsType<MenuPage>(await repository.CreatePageAsync(venueId, menuId, Guid.NewGuid(), "Late night", now));
            var sectionId = Guid.NewGuid();
            var section = await repository.CreateSectionOnMenuAsync(venueId, menuId, sectionId, "Mains", now);
            Assert.Equal("created", section.Outcome);
            var sharedItem = new Item
            {
                Id = Guid.NewGuid(), VenueId = venueId, Name = "Shared steak", Source = ItemSources.Manual,
                IsActive = true, CreatedUtc = now, UpdatedUtc = now
            };
            Assert.Equal("created", (await repository.CreateItemOnMenuAsync(sharedItem, menuId, sectionId, 100)).Outcome);
            var mismatch = await Assert.ThrowsAsync<SqlException>(() => repository.CreatePlacementAsync(new Placement
            {
                Id = Guid.NewGuid(), VenueId = venueId, MenuId = menuId, MenuSectionId = sectionId,
                PageId = second.Id, ItemId = sharedItem.Id, SortOrder = 1, CreatedUtc = now, UpdatedUtc = now
            }));
            Assert.Equal(547, mismatch.Number);

            var otherSectionId = Guid.NewGuid();
            Assert.Equal("created", (await repository.CreateSectionOnMenuAsync(venueId, menuId, otherSectionId, "Sides", now)).Outcome);
            var concurrentItem = new Item
            {
                Id = Guid.NewGuid(), VenueId = venueId, Name = "Concurrent item", Source = ItemSources.Manual,
                IsActive = true, CreatedUtc = now, UpdatedUtc = now
            };
            Assert.Equal(concurrentItem.Id, await repository.CreateItemAsync(concurrentItem));
            async Task<Exception?> TryPlaceAsync(Guid targetSectionId)
            {
                try
                {
                    await repository.CreatePlacementAsync(new Placement
                    {
                        Id = Guid.NewGuid(), VenueId = venueId, MenuId = menuId, MenuSectionId = targetSectionId,
                        PageId = first.Id, ItemId = concurrentItem.Id, SortOrder = 20, CreatedUtc = now, UpdatedUtc = now
                    });
                    return null;
                }
                catch (Exception exception) { return exception; }
            }
            var concurrentResults = await Task.WhenAll(TryPlaceAsync(sectionId), TryPlaceAsync(otherSectionId));
            Assert.Single(concurrentResults, result => result is null);
            var collision = Assert.IsType<SqlException>(Assert.Single(concurrentResults, result => result is not null));
            Assert.Contains(collision.Number, new[] { 2601, 2627 });
            Assert.Single((await repository.GetPlacementsAsync(venueId, menuId)).Where(placement => placement.ItemId == concurrentItem.Id));

            var stale = await repository.ReorderPagesGuardedAsync(venueId, menuId, [second.Id], now);
            Assert.Equal("order_stale", stale.Outcome);
            Assert.Equal([first.Id, second.Id], (await repository.GetPagesAsync(venueId, menuId)).Select(page => page.Id));

            var reordered = await repository.ReorderPagesGuardedAsync(venueId, menuId, [second.Id, first.Id], now);
            Assert.Equal("reordered", reordered.Outcome);
            Assert.Equal([second.Id, first.Id], (await repository.GetPagesAsync(venueId, menuId)).Select(page => page.Id));

            var copy = Assert.IsType<MenuPage>(await repository.DuplicatePageAsync(venueId, menuId, first.Id, Guid.NewGuid(), now));
            Assert.Equal("Page 1 copy", copy.Name);
            Assert.Empty((await repository.GetAssignmentsAsync(venueId)).Where(assignment => assignment.PageId == copy.Id));
            var copiedPlacements = (await repository.GetPlacementsAsync(venueId, menuId)).Where(placement => placement.ItemId == sharedItem.Id).ToArray();
            Assert.Equal(2, copiedPlacements.Length);
            Assert.Equal(new[] { first.Id, copy.Id }.Order(), copiedPlacements.Select(placement => placement.PageId).Order());
            Assert.Equal("item_conflict", (await repository.DeletePageAsync(venueId, menuId, copy.Id, first.Id)).Outcome);

            var recorded = await repository.GetWorkingSnapshotAsync(venueId, menuId);
            Assert.NotNull(recorded);
            Assert.True(await repository.RenamePageAsync(venueId, menuId, second.Id, "Changed later", now));
            Assert.Equal("reordered", (await repository.ReorderPagesGuardedAsync(venueId, menuId, [copy.Id, first.Id, second.Id], now)).Outcome);
            await repository.RestoreSnapshotAsync(venueId, menuId, recorded!, "integration", "restore page shape", now);
            Assert.Equal(new[] { second.Id, first.Id, copy.Id }, (await repository.GetPagesAsync(venueId, menuId)).Select(page => page.Id));
            Assert.Equal("Late night", (await repository.GetPagesAsync(venueId, menuId)).First().Name);

            var discarded = await repository.DeletePageAsync(venueId, menuId, copy.Id, null, deleteSections: true);
            Assert.Equal("deleted", discarded.Outcome);
            Assert.DoesNotContain(await repository.GetPlacementsAsync(venueId, menuId), placement => placement.PageId == copy.Id);
            Assert.Contains(await repository.GetItemsAsync(venueId), item => item.Id == sharedItem.Id);

            var moveRequired = await repository.DeletePageAsync(venueId, menuId, first.Id, null);
            Assert.Equal("move_required", moveRequired.Outcome);
            var deleted = await repository.DeletePageAsync(venueId, menuId, first.Id, second.Id);
            Assert.Equal("deleted", deleted.Outcome);
            Assert.Equal(2, deleted.MovedSectionCount);
        }
        finally
        {
            await repository.ResetAutomationVenueAsync(venueId);
            await repository.ResetAutomationVenueAsync(otherVenueId);
            await data.ExecuteSqlQueryAsync<Row, object>("DELETE dbo.Venues WHERE Id IN (@VenueId,@OtherVenueId); SELECT 1 Value;", new { VenueId = venueId, OtherVenueId = otherVenueId });
        }
    }

    private sealed class Row { public int Value { get; set; } }
}

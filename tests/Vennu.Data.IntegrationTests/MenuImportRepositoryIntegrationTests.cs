using Vennu.Core.Models;
using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class MenuImportRepositoryIntegrationTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Create_read_and_answer_round_trip_exact_review_state()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddHours(1));

        var created = await repository.CreateAsync(aggregate);
        var answered = await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.Fallback, null,
            DateTime.UtcNow, "owner@example.com");

        Assert.Equal(MenuImportMutationOutcome.Updated, answered.Result);
        Assert.Equal(MenuImportStatuses.Resolved, answered.Aggregate!.Session.Status);
        Assert.Equal("a deliberately very long source line " + new string('x', 2500), answered.Aggregate.Lines.Single().RawText);
        Assert.Equal(MenuImportChoices.Fallback, answered.Aggregate.Questions.Single().Answer!.Choice);
        Assert.NotEqual(created.Session.Revision, answered.Aggregate.Session.Revision);
    }

    [Fact]
    public async Task Stale_revision_returns_current_state_without_overwriting_answer()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddHours(1));
        var created = await repository.CreateAsync(aggregate);
        var first = await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.Fallback, null, DateTime.UtcNow, "first");

        var stale = await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.NewItem, null, DateTime.UtcNow, "late");

        Assert.Equal(MenuImportMutationOutcome.Conflict, stale.Result);
        Assert.Equal(first.Aggregate!.Session.Revision, stale.Aggregate!.Session.Revision);
        Assert.Equal(MenuImportChoices.Fallback, stale.Aggregate.Questions.Single().Answer!.Choice);
    }

    [Fact]
    public async Task Concurrent_answers_with_the_same_revision_allow_exactly_one_writer()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddHours(1));
        var created = await repository.CreateAsync(aggregate);

        var writes = await Task.WhenAll(
            repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
                "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.Fallback, null,
                DateTime.UtcNow, "first"),
            repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
                "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.NewItem, null,
                DateTime.UtcNow, "second"));

        Assert.Single(writes, write => write.Result == MenuImportMutationOutcome.Updated);
        Assert.Single(writes, write => write.Result == MenuImportMutationOutcome.Conflict);
    }

    [Fact]
    public async Task Expired_session_is_refused_then_deleted_in_a_bounded_batch()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddMilliseconds(100));
        var created = await repository.CreateAsync(aggregate);
        // Stay well beyond any provider precision/round-trip boundary; this test
        // proves expiry semantics, not SQL Server's sub-millisecond rounding.
        var afterExpiry = aggregate.Session.ExpiresUtc.AddMinutes(1);

        Assert.Null(await repository.GetAsync(venueId, aggregate.Session.Id, afterExpiry));
        var mutation = await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.Fallback, null, afterExpiry, null);
        Assert.Equal(MenuImportMutationOutcome.Expired, mutation.Result);
        Assert.True(await repository.DeleteExpiredAsync(afterExpiry, 1000) >= 1);
        Assert.Equal(MenuImportMutationOutcome.NotFound, (await repository.PutAnswerAsync(venueId, aggregate.Session.Id,
            created.Session.Revision, "line-1-unreadable", aggregate.Questions.Single().Fingerprint,
            MenuImportChoices.Fallback, null, afterExpiry, null)).Result);
    }

    [Fact]
    public async Task Confirm_create_is_atomic_idempotent_and_unpublished()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var lines = new[]
        {
            new MenuImportSourceLine(id, venueId, 1, 0, "DINNER", "section", "DINNER", null, null, null, 1),
            new MenuImportSourceLine(id, venueId, 2, 0, "Burger  14", "item", "Burger", null, "14", null, 1)
        };
        var created = await repository.CreateAsync(new(new(id, venueId, "DINNER\nBurger  14", 1,
            MenuImportStatuses.Resolved, 2, 1, now.AddHours(1), now, now, null, []), lines, []));
        var named = await repository.SetCreateDestinationAsync(venueId, id, created.Session.Revision, " Dinner menu ", now, "owner");

        var first = await Confirm(repository, venueId, id, named.Aggregate!.Session.Revision, now, "owner");
        var retry = await Confirm(repository, venueId, id, named.Aggregate.Session.Revision, now, "owner");

        Assert.Equal(MenuImportCreateOutcome.Created, first.Result);
        Assert.Equal(MenuImportCreateOutcome.AlreadyCompleted, retry.Result);
        Assert.Equal(first.MenuId, retry.MenuId);
        Assert.Equal(first.MenuId, retry.Aggregate!.Session.CompletedMenuId);
        Assert.Equal(MenuImportCreateOutcome.AlreadyCompleted,(await repository.SetReplaceDestinationAsync(venueId,id,retry.Aggregate.Session.Revision,first.MenuId!.Value,now.AddSeconds(1),"late-owner")).Result);
        var menus = await new MenuRepository(fixture.CreateDataAccess()).GetMenusAsync(venueId);
        Assert.Single(menus, menu => menu.Id == first.MenuId && menu.PublishedVersion is null);
    }

    [Fact]
    public async Task Confirm_create_keeps_a_selected_library_items_price_scoped_to_the_new_menu()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venue = new Venue { Name = fixture.UniqueValue("price-venue"), Timezone = "UTC", Type = "Restaurant", PrimaryLanguage = "en" };
        var venueId = await new VenueRepository(dataAccess).CreateAsync(venue);
        var libraryItem = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "Burger", Price = "12", Source = ItemSources.Manual };
        await new ContentRepository(dataAccess).CreateItemAsync(libraryItem);
        var repository = new MenuImportRepository(dataAccess);
        var now = DateTime.UtcNow; var id = Guid.NewGuid(); var fingerprint = new string('c', 64);
        var line = new MenuImportSourceLine(id, venueId, 1, 0, "Burger  14", "item", "Burger", null, "14", null, 1);
        var candidate = new MenuImportCandidate(libraryItem.Id, "Burger", "12", "exact_normalized", true);
        var question = new MenuImportReviewQuestion(id, venueId, "line-1-identity", fingerprint, "identity", 0, true, 1, [1], [candidate], null);
        var started = await repository.CreateAsync(new(new(id, venueId, line.RawText, 1, MenuImportStatuses.Reviewing, 1, 1, now.AddHours(1), now, now, null, []), [line], [question]));
        var answered = await repository.PutAnswerAsync(venueId, id, started.Session.Revision, question.QuestionKey, fingerprint, MenuImportChoices.SameItem, libraryItem.Id, now, "owner");
        var named = await repository.SetCreateDestinationAsync(venueId, id, answered.Aggregate!.Session.Revision, "Imported dinner", now, "owner");

        var created = await Confirm(repository, venueId, id, named.Aggregate!.Session.Revision, now, "owner");

        Assert.Equal("12", (await new ContentRepository(dataAccess).GetItemAsync(venueId, libraryItem.Id))!.Price);
        var placed = Assert.Single(await new ContentRepository(dataAccess).GetPlacedItemsForVenueAsync(venueId));
        Assert.Equal(created.MenuId, placed.MenuId);
        Assert.Equal("14", placed.Price);
        var edited = await new ContentRepository(dataAccess).UpdateItemValuesGuardedAsync(venueId, libraryItem.Id,
            "Burger", null, "15", new ItemValueExpectation("Burger", null, "14"), now.AddSeconds(1), menuId: created.MenuId);
        Assert.Equal("updated", edited.Outcome);
        Assert.Equal("12", (await new ContentRepository(dataAccess).GetItemAsync(venueId, libraryItem.Id))!.Price);
        Assert.Equal("15", Assert.Single(await new ContentRepository(dataAccess).GetPlacedItemsForVenueAsync(venueId)).Price);
    }

    [Fact]
    public async Task Concurrent_confirmations_create_exactly_one_menu()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue(); var now = DateTime.UtcNow; var id = Guid.NewGuid();
        var line = new MenuImportSourceLine(id, venueId, 1, 0, "Burger  14", "item", "Burger", null, "14", null, 1);
        var started = await repository.CreateAsync(new(new(id, venueId, line.RawText, 1, MenuImportStatuses.Resolved, 1, 1, now.AddHours(1), now, now, null, []), [line], []));
        var named = await repository.SetCreateDestinationAsync(venueId, id, started.Session.Revision, fixture.UniqueValue("concurrent-import"), now, "owner");

        var outcomes = await Task.WhenAll(
            Confirm(repository, venueId, id, named.Aggregate!.Session.Revision, now, "one"),
            Confirm(repository, venueId, id, named.Aggregate.Session.Revision, now, "two"));

        Assert.Single(outcomes, outcome => outcome.Result == MenuImportCreateOutcome.Created);
        Assert.Single(outcomes, outcome => outcome.Result == MenuImportCreateOutcome.AlreadyCompleted);
        Assert.Single(outcomes.Select(outcome => outcome.MenuId).Distinct());
    }

    [Fact]
    public async Task Name_refusal_rolls_back_every_import_row_and_the_saved_review_can_retry()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue(); var now = DateTime.UtcNow;
        var content = new ContentRepository(fixture.CreateDataAccess());
        var existingName = fixture.UniqueValue("existing-menu");
        Assert.True((await content.CreateMenuWithinCeilingAsync(new Menu { Id=Guid.NewGuid(), VenueId=venueId, Name=existingName, CreatedUtc=now }, 50)).Created);
        var id=Guid.NewGuid(); var line=new MenuImportSourceLine(id,venueId,1, 0,"Soup  9","item","Soup",null,"9",null,1);
        var started=await repository.CreateAsync(new(new(id,venueId,line.RawText,1,MenuImportStatuses.Resolved,1,1,now.AddHours(1),now,now,null,[]),[line],[]));
        var named=await repository.SetCreateDestinationAsync(venueId,id,started.Session.Revision,existingName,now,"owner");

        var refused=await Confirm(repository,venueId,id,named.Aggregate!.Session.Revision,now,"owner");

        Assert.Equal(MenuImportCreateOutcome.NameConflict,refused.Result);
        Assert.Null(refused.Aggregate!.Session.CompletedMenuId);
        Assert.Single(await new MenuRepository(fixture.CreateDataAccess()).GetMenusAsync(venueId));
        var renamed=await repository.SetCreateDestinationAsync(venueId,id,refused.Aggregate.Session.Revision,fixture.UniqueValue("retry-menu"),now.AddSeconds(1),"owner");
        Assert.Equal(MenuImportCreateOutcome.Created,(await Confirm(repository,venueId,id,renamed.Aggregate!.Session.Revision,now.AddSeconds(1),"owner")).Result);
    }

    [Fact]
    public async Task Confirm_create_uses_the_allowance_current_inside_its_transaction()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venue = new Venue { Name = fixture.UniqueValue("allowance-venue"), Timezone = "UTC", Type = "Restaurant", PrimaryLanguage = "en" };
        var venueId = await new VenueRepository(dataAccess).CreateAsync(venue);
        var content = new ContentRepository(dataAccess);
        var now = DateTime.UtcNow;
        Assert.True((await content.CreateMenuWithinCeilingAsync(
            new Menu { Id = Guid.NewGuid(), VenueId = venueId, Name = fixture.UniqueValue("existing"), CreatedUtc = now }, 50)).Created);
        var organizationId = (await dataAccess.ExecuteSqlQueryAsync<GuidRow, object>(
            "SELECT OrganizationId AS Value FROM dbo.Venues WHERE Id=@VenueId;", new { VenueId = venueId })).Single().Value;

        var repository = new MenuImportRepository(dataAccess);
        var id = Guid.NewGuid();
        var line = new MenuImportSourceLine(id, venueId, 1, 0, "Soup  9", "item", "Soup", null, "9", null, 1);
        var started = await repository.CreateAsync(new(new(id, venueId, line.RawText, 1, MenuImportStatuses.Resolved,
            1, 1, now.AddHours(1), now, now, null, []), [line], []));
        var named = await repository.SetCreateDestinationAsync(venueId, id, started.Session.Revision,
            fixture.UniqueValue("refused-import"), now, "owner");

        // This represents an allowance reduction after the review/service read but
        // before confirmation. The enforcement boundary must read this row itself.
        await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            """
            DELETE dbo.CapabilityAllowances WHERE VenueId=@VenueId AND CapabilityId='content.menu.count';
            INSERT dbo.CapabilityAllowances(Id,OrganizationId,VenueId,CapabilityId,LimitValue,StartsUtc,EndsUtc)
            VALUES(NEWID(),@OrganizationId,@VenueId,'content.menu.count',1,DATEADD(day,-1,@Now),NULL);
            SELECT 1 AS Value;
            """, new { VenueId = venueId, OrganizationId = organizationId, Now = now });

        var refused = await Confirm(repository, venueId, id, named.Aggregate!.Session.Revision, now, "owner");

        Assert.Equal(MenuImportCreateOutcome.MenuLimit, refused.Result);
        Assert.Null(refused.Aggregate!.Session.CompletedMenuId);
        Assert.Single(await new MenuRepository(dataAccess).GetMenusAsync(venueId));
    }

    [Fact]
    public async Task Confirm_create_rechecks_current_permission_inside_its_transaction()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var line = new MenuImportSourceLine(id, venueId, 1, 0, "Soup  9", "item", "Soup", null, "9", null, 1);
        var started = await repository.CreateAsync(new(new(id, venueId, line.RawText, 1, MenuImportStatuses.Resolved,
            1, 1, now.AddHours(1), now, now, null, []), [line], []));
        var named = await repository.SetCreateDestinationAsync(venueId, id, started.Session.Revision,
            fixture.UniqueValue("permission-import"), now, "owner");

        var refused = await repository.ConfirmCreateAsync(venueId, id, named.Aggregate!.Session.Revision,
            Guid.NewGuid(), ["role-without-import-permission"], now, "owner");

        Assert.Equal(MenuImportCreateOutcome.PermissionDenied, refused.Result);
        Assert.Null(refused.Aggregate!.Session.CompletedMenuId);
        Assert.Empty(await new MenuRepository(fixture.CreateDataAccess()).GetMenusAsync(venueId));
    }

    [Fact]
    public async Task Replace_is_atomic_idempotent_preserves_menu_truth_and_can_restore_working_content()
    {
        var data=fixture.CreateDataAccess();var venue=new Venue{Name=fixture.UniqueValue("replace-venue"),Timezone="UTC",Type="Restaurant",PrimaryLanguage="en"};var venueId=await new VenueRepository(data).CreateAsync(venue);var now=DateTime.UtcNow;
        var menuId=Guid.NewGuid();var pageId=Guid.NewGuid();var sectionId=Guid.NewGuid();var oldItemId=Guid.NewGuid();
        await data.ExecuteSqlQueryAsync<CountRow,object>("""
        INSERT dbo.Menus(Id,VenueId,Name,IsActive,Theme,PublishedVersion,CreatedUtc,UpdatedUtc)VALUES(@MenuId,@VenueId,N'House menu',1,N'night',1,@Now,@Now);
        INSERT dbo.MenuPages(Id,VenueId,MenuId,Name,SortOrder,CreatedUtc,UpdatedUtc)VALUES(@PageId,@VenueId,@MenuId,N'Page 1',0,@Now,@Now);
        INSERT dbo.MenuSections(Id,VenueId,MenuId,PageId,Name,SortOrder,CreatedUtc,UpdatedUtc)VALUES(@SectionId,@VenueId,@MenuId,@PageId,N'Old section',0,@Now,@Now);
        INSERT dbo.Items(Id,VenueId,Name,Price,Source,IsActive,CreatedUtc,UpdatedUtc)VALUES(@OldItemId,@VenueId,N'Old item',N'7',N'manual',1,@Now,@Now);
        INSERT dbo.Placements(Id,VenueId,MenuId,MenuSectionId,PageId,ItemId,SortOrder,CreatedUtc,UpdatedUtc)VALUES(NEWID(),@VenueId,@MenuId,@SectionId,@PageId,@OldItemId,0,@Now,@Now);
        DECLARE @Snapshot nvarchar(max)=(SELECT @MenuId menuId,N'House menu' name,N'night' theme,8 dwellSeconds,60 loopWarningSeconds,JSON_QUERY(N'[]')screens,JSON_QUERY((SELECT @PageId pageId,N'Page 1'name,0 sortOrder FOR JSON PATH))pages,JSON_QUERY((SELECT @SectionId sectionId,@PageId pageId,N'Old section'name,0 sortOrder,JSON_QUERY((SELECT @OldItemId itemId,N'Old item'name,N'7'price,0 sortOrder FOR JSON PATH))items FOR JSON PATH))sections FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
        DECLARE @EventId uniqueidentifier=NEWID();INSERT dbo.MenuPublishEvents(Id,VenueId,MenuId,Version,ChangeCount,Snapshot,PublishedUtc,Author)VALUES(@EventId,@VenueId,@MenuId,1,0,@Snapshot,@Now,N'publisher');
        INSERT dbo.MenuHistoryEntries(Id,VenueId,MenuId,Kind,PublishEventId,Detail,Author,OccurredUtc)VALUES(NEWID(),@VenueId,@MenuId,N'published',@EventId,N'Published version 1.',N'publisher',@Now);SELECT 1 Value;
        """,new{MenuId=menuId,VenueId=venueId,PageId=pageId,SectionId=sectionId,OldItemId=oldItemId,Now=now});
        var repository=new MenuImportRepository(data);var sessionId=Guid.NewGuid();var line=new MenuImportSourceLine(sessionId,venueId,1, 0,"New item  12","item","New item",null,"12",null,1);var started=await repository.CreateAsync(new(new(sessionId,venueId,line.RawText,1,MenuImportStatuses.Resolved,1,1,now.AddHours(1),now,now,null,[]),[line],[]));
        var selected=await repository.SetReplaceDestinationAsync(venueId,sessionId,started.Session.Revision,menuId,now.AddSeconds(1),"owner");
        var content=new ContentRepository(data);Assert.True(await content.RenameSectionAsync(venueId,menuId,sectionId,"Someone else's section",now.AddSeconds(2),"other-owner"));
        var stale=await repository.ConfirmReplaceAsync(venueId,sessionId,selected.Aggregate!.Session.Revision,Guid.NewGuid(),["organization_administrator"],now.AddSeconds(3),"owner");Assert.Equal("target_conflict",stale.Result);Assert.Equal("Old item",Assert.Single(await new ContentRepository(data).GetPlacedItemsForVenueAsync(venueId)).Name);Assert.Null(stale.Aggregate!.Session.CompletedSnapshotId);
        Assert.True(await content.RenameSectionAsync(venueId,menuId,sectionId,"Old section",now.AddSeconds(3.5),"other-owner"));
        selected=await repository.SetReplaceDestinationAsync(venueId,sessionId,stale.Aggregate.Session.Revision,menuId,now.AddSeconds(4),"owner");
        var first=await repository.ConfirmReplaceAsync(venueId,sessionId,selected.Aggregate!.Session.Revision,Guid.NewGuid(),["organization_administrator"],now.AddSeconds(5),"owner");
        var retry=await repository.ConfirmReplaceAsync(venueId,sessionId,selected.Aggregate.Session.Revision,Guid.NewGuid(),["organization_administrator"],now.AddSeconds(6),"owner");
        Assert.Equal(MenuImportCreateOutcome.AlreadyCompleted,(await repository.SetReplaceDestinationAsync(venueId,sessionId,retry.Aggregate!.Session.Revision,menuId,now.AddSeconds(6.25),"late-owner")).Result);
        Assert.Equal(MenuImportCreateOutcome.Created,first.Result);Assert.Equal(MenuImportCreateOutcome.AlreadyCompleted,retry.Result);Assert.Equal(menuId,first.MenuId);var preserved=Assert.Single(await new MenuRepository(data).GetMenusAsync(venueId));Assert.Equal("night",preserved.Theme);Assert.Equal(1,preserved.PublishedVersion);
        var replacement=Assert.Single(await new ContentRepository(data).GetPlacedItemsForVenueAsync(venueId));Assert.Equal("New item",replacement.Name);
        var snapshotId=first.Aggregate!.Session.CompletedSnapshotId!.Value;Assert.True(await content.RenameSectionAsync(venueId,menuId,replacement.MenuSectionId,"Edited after import",now.AddSeconds(6.5),"other-owner"));var staleRestore=await repository.RestoreReplacementAsync(venueId,snapshotId,Guid.NewGuid(),["organization_administrator"],now.AddSeconds(7),"owner");Assert.Equal(MenuImportRestoreOutcome.Conflict,staleRestore.Result);Assert.Equal("New item",Assert.Single(await new ContentRepository(data).GetPlacedItemsForVenueAsync(venueId)).Name);Assert.True(await content.RenameSectionAsync(venueId,menuId,replacement.MenuSectionId,"Imported items",now.AddSeconds(7.25),"other-owner"));
        var restored=await repository.RestoreReplacementAsync(venueId,snapshotId,Guid.NewGuid(),["organization_administrator"],now.AddSeconds(7.5),"owner");Assert.Equal(MenuImportRestoreOutcome.Restored,restored.Result);Assert.Equal("Old item",Assert.Single(await new ContentRepository(data).GetPlacedItemsForVenueAsync(venueId)).Name);
        _=await content.UpdateItemValuesGuardedAsync(venueId,oldItemId,"Old item",null,"8",null,now.AddSeconds(7.75));Assert.Equal("8",Assert.Single(await content.GetPlacedItemsForVenueAsync(venueId)).Price);
        Assert.Equal(MenuImportRestoreOutcome.AlreadyRestored,(await repository.RestoreReplacementAsync(venueId,snapshotId,Guid.NewGuid(),["organization_administrator"],now.AddSeconds(8),"owner")).Result);
    }

    [Fact]
    public async Task Safe_match_acceptance_and_reparse_preserve_only_still_valid_answer()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venue = new Venue { Name = fixture.UniqueValue("match-venue"), Timezone = "UTC", Type = "Restaurant", PrimaryLanguage = "en" };
        var venueId = await new VenueRepository(dataAccess).CreateAsync(venue);
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "Crème-Brûlée", Price = "12" };
        await new ContentRepository(dataAccess).CreateItemAsync(item);
        var repository = new MenuImportRepository(dataAccess);
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var fingerprint = new string('b', 64);
        var line = new MenuImportSourceLine(id, venueId, 1, 0, "creme brulee  13", "item", "creme brulee", null, "13", null, 1);
        var candidate = new MenuImportCandidate(item.Id, item.Name, item.Price, "exact_normalized", true);
        var question = new MenuImportReviewQuestion(id, venueId, "line-1-identity", fingerprint, "identity", 0, true, 1, [1], [candidate], null);
        var created = await repository.CreateAsync(new(new(id, venueId, line.RawText, 1, MenuImportStatuses.Reviewing, 1, 1, now.AddHours(1), now, now, null, []), [line], [question]));

        var accepted = await repository.AcceptSafeMatchesAsync(venueId, id, created.Session.Revision, now.AddSeconds(1), "owner");
        var nextLine = line with { ParseRevision = 2 };
        var nextQuestion = question with { ParseRevision = 2 };
        var nextSession = accepted.Aggregate!.Session with { ParseRevision = 2, UpdatedUtc = now.AddSeconds(2), Revision = [] };
        var reparsed = await repository.ReplaceParseAsync(new(nextSession, [nextLine], [nextQuestion]), accepted.Aggregate.Session.Revision);

        Assert.Equal(MenuImportChoices.SameItem, reparsed.Aggregate!.Questions.Single().Answer!.Choice);
        Assert.Equal(item.Id, reparsed.Aggregate.Questions.Single().Answer!.SelectedItemId);
        Assert.Equal(2, reparsed.Aggregate.Questions.Single().Answer!.ParseRevision);
        Assert.Equal(MenuImportStatuses.Resolved, reparsed.Aggregate.Session.Status);
    }

    /*
     * #904 - listing the imports somebody started and did not finish.
     *
     * These are the three things the query can get wrong, and none of them is visible from the
     * client: counting questions from a retired parse revision, offering to "resume" an import
     * whose work is already on a menu, and letting one venue see another's.
     */
    [Fact]
    public async Task ListOpen_counts_only_unanswered_questions_at_the_current_parse_revision()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddHours(1));
        var created = await repository.CreateAsync(aggregate);

        var before = Assert.Single(await repository.ListOpenAsync(venueId, DateTime.UtcNow));
        Assert.Equal(aggregate.Session.Id, before.Id);
        Assert.Equal(1, before.AnswersRemaining);

        await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", new string('a', 64), MenuImportChoices.Fallback, null, DateTime.UtcNow, "owner@example.com");

        Assert.Equal(0, Assert.Single(await repository.ListOpenAsync(venueId, DateTime.UtcNow)).AnswersRemaining);
    }

    [Fact]
    public async Task ListOpen_leaves_out_expired_completed_and_other_venues_sessions()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();

        // Expired: past its ExpiresUtc, so there is nothing to go back to.
        await repository.CreateAsync(Aggregate(venueId, DateTime.UtcNow.AddMilliseconds(50)));

        // Completed: its work is on a menu. Offering to resume it would send the operator back to
        // a review with nothing left to do.
        var finished = CreatableAggregate(venueId, DateTime.UtcNow.AddHours(1), "Garlic Bread  6.50");
        var createdFinished = await repository.CreateAsync(finished);
        await repository.PutAnswerAsync(venueId, finished.Session.Id, createdFinished.Session.Revision,
            "line-1-unreadable", new string('b', 64), MenuImportChoices.Fallback, null, DateTime.UtcNow, "owner@example.com");
        var ready = await repository.GetAsync(venueId, finished.Session.Id, DateTime.UtcNow);
        await repository.SetCreateDestinationAsync(venueId, finished.Session.Id, ready!.Session.Revision,
            fixture.UniqueValue("finished-menu"), DateTime.UtcNow, "owner@example.com");
        ready = await repository.GetAsync(venueId, finished.Session.Id, DateTime.UtcNow);
        await Confirm(repository, venueId, finished.Session.Id, ready!.Session.Revision, DateTime.UtcNow, "owner@example.com");

        // Another venue's, which must never appear in this one's list.
        var (_, otherVenueId) = await CreateRepositoryAndVenue();
        await repository.CreateAsync(Aggregate(otherVenueId, DateTime.UtcNow.AddHours(1)));

        var open = Aggregate(venueId, DateTime.UtcNow.AddHours(1));
        await repository.CreateAsync(open);

        await Task.Delay(120);
        var listed = await repository.ListOpenAsync(venueId, DateTime.UtcNow);

        Assert.Equal([open.Session.Id], listed.Select(row => row.Id));
    }

    [Fact]
    public async Task Discard_removes_an_unfinished_import_and_everything_hanging_off_it()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddHours(1));
        await repository.CreateAsync(aggregate);

        Assert.True(await repository.DiscardAsync(venueId, aggregate.Session.Id));

        Assert.Empty(await repository.ListOpenAsync(venueId, DateTime.UtcNow));
        Assert.Null(await repository.GetAsync(venueId, aggregate.Session.Id, DateTime.UtcNow));

        // Saying no twice is not an error, but it is not a success either.
        Assert.False(await repository.DiscardAsync(venueId, aggregate.Session.Id));
    }

    [Fact]
    public async Task Discard_refuses_a_completed_session_because_its_restore_hangs_off_it()
    {
        // A completed session owns the replacement snapshot an operator restores a replaced menu
        // from. Deleting it on request would take the way back with it; the sweeper removes those
        // once they expire, which is when the restore was going to lapse anyway.
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = CreatableAggregate(venueId, DateTime.UtcNow.AddHours(1), "Olives  4.00");
        var created = await repository.CreateAsync(aggregate);
        await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", new string('b', 64), MenuImportChoices.Fallback, null, DateTime.UtcNow, "owner@example.com");
        var ready = await repository.GetAsync(venueId, aggregate.Session.Id, DateTime.UtcNow);
        await repository.SetCreateDestinationAsync(venueId, aggregate.Session.Id, ready!.Session.Revision,
            fixture.UniqueValue("kept-menu"), DateTime.UtcNow, "owner@example.com");
        ready = await repository.GetAsync(venueId, aggregate.Session.Id, DateTime.UtcNow);
        await Confirm(repository, venueId, aggregate.Session.Id, ready!.Session.Revision, DateTime.UtcNow, "owner@example.com");

        Assert.False(await repository.DiscardAsync(venueId, aggregate.Session.Id));
        Assert.NotNull(await repository.GetAsync(venueId, aggregate.Session.Id, DateTime.UtcNow));
    }

    [Fact]
    public async Task Discard_will_not_reach_another_venues_import()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var (_, otherVenueId) = await CreateRepositoryAndVenue();
        var theirs = Aggregate(otherVenueId, DateTime.UtcNow.AddHours(1));
        await repository.CreateAsync(theirs);

        Assert.False(await repository.DiscardAsync(venueId, theirs.Session.Id));
        Assert.NotNull(await repository.GetAsync(otherVenueId, theirs.Session.Id, DateTime.UtcNow));
    }

    private async Task<(MenuImportRepository Repository, Guid VenueId)> CreateRepositoryAndVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venue = new Venue { Name = fixture.UniqueValue("import-venue"), Timezone = "UTC", Type = "Restaurant", PrimaryLanguage = "en" };
        var venueId = await new VenueRepository(dataAccess).CreateAsync(venue);
        return (new MenuImportRepository(dataAccess), venueId);
    }

    /// <summary>
    /// The same shape as <see cref="Aggregate"/> with a source line a person could have typed.
    ///
    /// Aggregate's line is 2,500 characters on purpose - it exists to prove a long line survives
    /// storage. Anything that goes on to CREATE a menu from that session cannot use it: the item
    /// it would make is named after the line, and dbo.Items.Name is 200.
    /// </summary>
    private static MenuImportAggregate CreatableAggregate(Guid venueId, DateTime expiresUtc, string dish)
    {
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var fingerprint = new string('b', 64);
        var line = new MenuImportSourceLine(id, venueId, 1, 0, dish, "unresolved", null, null, null, "item_format_not_recognized", 1);
        var question = new MenuImportReviewQuestion(id, venueId, "line-1-unreadable", fingerprint, "unreadable", 0, true, 1, [1], [], null);
        return new(new MenuImportSession(id, venueId, dish, 1, MenuImportStatuses.Reviewing, 1, 0, expiresUtc, now, now, null, []), [line], [question]);
    }

    private static MenuImportAggregate Aggregate(Guid venueId, DateTime expiresUtc)
    {
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var raw = "a deliberately very long source line " + new string('x', 2500);
        var fingerprint = new string('a', 64);
        var line = new MenuImportSourceLine(id, venueId, 1, 0, raw, "unresolved", null, null, null, "item_format_not_recognized", 1);
        var question = new MenuImportReviewQuestion(id, venueId, "line-1-unreadable", fingerprint, "unreadable", 0, true, 1, [1], [], null);
        return new(new MenuImportSession(id, venueId, raw, 1, MenuImportStatuses.Reviewing, 1, 0, expiresUtc, now, now, null, []), [line], [question]);
    }

    private static Task<MenuImportCreateOutcome> Confirm(MenuImportRepository repository, Guid venueId, Guid sessionId,
        byte[] revision, DateTime now, string actor) => repository.ConfirmCreateAsync(venueId, sessionId, revision,
        Guid.NewGuid(), ["organization_administrator"], now, actor);

    private sealed class CountRow { public int Value { get; set; } }
    private sealed class GuidRow { public Guid Value { get; set; } }
}

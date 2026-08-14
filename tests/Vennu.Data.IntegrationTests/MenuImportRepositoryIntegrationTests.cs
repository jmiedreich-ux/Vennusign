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
            new MenuImportSourceLine(id, venueId, 1, "DINNER", "section", "DINNER", null, null, null, 1),
            new MenuImportSourceLine(id, venueId, 2, "Burger  14", "item", "Burger", null, "14", null, 1)
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
        var line = new MenuImportSourceLine(id, venueId, 1, "Burger  14", "item", "Burger", null, "14", null, 1);
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
        var line = new MenuImportSourceLine(id, venueId, 1, "Burger  14", "item", "Burger", null, "14", null, 1);
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
        var id=Guid.NewGuid(); var line=new MenuImportSourceLine(id,venueId,1,"Soup  9","item","Soup",null,"9",null,1);
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
        var line = new MenuImportSourceLine(id, venueId, 1, "Soup  9", "item", "Soup", null, "9", null, 1);
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
        var line = new MenuImportSourceLine(id, venueId, 1, "Soup  9", "item", "Soup", null, "9", null, 1);
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
        var line = new MenuImportSourceLine(id, venueId, 1, "creme brulee  13", "item", "creme brulee", null, "13", null, 1);
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

    private async Task<(MenuImportRepository Repository, Guid VenueId)> CreateRepositoryAndVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venue = new Venue { Name = fixture.UniqueValue("import-venue"), Timezone = "UTC", Type = "Restaurant", PrimaryLanguage = "en" };
        var venueId = await new VenueRepository(dataAccess).CreateAsync(venue);
        return (new MenuImportRepository(dataAccess), venueId);
    }

    private static MenuImportAggregate Aggregate(Guid venueId, DateTime expiresUtc)
    {
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var raw = "a deliberately very long source line " + new string('x', 2500);
        var fingerprint = new string('a', 64);
        var line = new MenuImportSourceLine(id, venueId, 1, raw, "unresolved", null, null, null, "item_format_not_recognized", 1);
        var question = new MenuImportReviewQuestion(id, venueId, "line-1-unreadable", fingerprint, "unreadable", 0, true, 1, [1], [], null);
        return new(new MenuImportSession(id, venueId, raw, 1, MenuImportStatuses.Reviewing, 1, 0, expiresUtc, now, now, null, []), [line], [question]);
    }

    private static Task<MenuImportCreateOutcome> Confirm(MenuImportRepository repository, Guid venueId, Guid sessionId,
        byte[] revision, DateTime now, string actor) => repository.ConfirmCreateAsync(venueId, sessionId, revision,
        Guid.NewGuid(), ["organization_administrator"], now, actor);

    private sealed class CountRow { public int Value { get; set; } }
    private sealed class GuidRow { public Guid Value { get; set; } }
}

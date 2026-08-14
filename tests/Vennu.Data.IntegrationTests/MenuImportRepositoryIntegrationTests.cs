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
    public async Task Expired_session_is_refused_then_deleted_in_a_bounded_batch()
    {
        var (repository, venueId) = await CreateRepositoryAndVenue();
        var aggregate = Aggregate(venueId, DateTime.UtcNow.AddMilliseconds(100));
        var created = await repository.CreateAsync(aggregate);
        var afterExpiry = aggregate.Session.ExpiresUtc.AddTicks(1);

        Assert.Null(await repository.GetAsync(venueId, aggregate.Session.Id, afterExpiry));
        var mutation = await repository.PutAnswerAsync(venueId, aggregate.Session.Id, created.Session.Revision,
            "line-1-unreadable", aggregate.Questions.Single().Fingerprint, MenuImportChoices.Fallback, null, afterExpiry, null);
        Assert.Equal(MenuImportMutationOutcome.Expired, mutation.Result);
        Assert.Equal(1, await repository.DeleteExpiredAsync(afterExpiry, 1));
        Assert.Equal(MenuImportMutationOutcome.NotFound, (await repository.PutAnswerAsync(venueId, aggregate.Session.Id,
            created.Session.Revision, "line-1-unreadable", aggregate.Questions.Single().Fingerprint,
            MenuImportChoices.Fallback, null, afterExpiry, null)).Result);
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
}

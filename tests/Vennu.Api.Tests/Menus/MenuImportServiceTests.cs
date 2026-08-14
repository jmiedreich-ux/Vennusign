using Microsoft.Extensions.Options;
using Vennu.Api.Menus;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Menus;

public sealed class MenuImportServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateTimeOffset Now = new(2026, 8, 13, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Start_retains_every_line_and_uses_configured_expiry()
    {
        var (service, repository, content) = CreateService();
        content.Ceilings[MenuCeilings.ImportSessionRetentionMinutes] = 90;

        var result = await service.StartAsync(VenueId, "DINNER\nBurger  12\n\nChef note", "owner@example.com", default);

        Assert.Equal(4, result.Lines.Count);
        Assert.Equal("", result.Lines.Single(line => line.LineNumber == 3).RawText);
        Assert.Equal(Now.UtcDateTime.AddMinutes(90), result.Session.ExpiresUtc);
        Assert.Same(result, repository.Created);
    }

    [Fact]
    public async Task Start_refuses_before_writing_when_line_ceiling_is_exceeded()
    {
        var (service, repository, content) = CreateService();
        content.Ceilings[MenuCeilings.ImportLines] = 2;

        var exception = await Assert.ThrowsAsync<MenuImportValidationException>(() =>
            service.StartAsync(VenueId, "one\ntwo\nthree", null, default));

        Assert.Contains("3 lines", exception.Message);
        Assert.Null(repository.Created);
    }

    [Fact]
    public async Task Start_refuses_before_writing_when_item_ceiling_is_exceeded()
    {
        var (service, repository, content) = CreateService();
        content.Ceilings[MenuCeilings.ItemsPerMenu] = 1;

        var exception = await Assert.ThrowsAsync<MenuImportValidationException>(() =>
            service.StartAsync(VenueId, "Burger  12\nSalad  10", null, default));

        Assert.Contains("2 items", exception.Message);
        Assert.Null(repository.Created);
    }

    [Fact]
    public async Task Promotion_reparses_and_increments_revision_without_changing_raw_paste()
    {
        var (service, repository, _) = CreateService();
        var started = await service.StartAsync(VenueId, "Lunch specials\nBurger  12", null, default);

        var outcome = await service.SetSectionOverrideAsync(VenueId, started.Session.Id, started.Session.Revision, 1, true, null, default);

        Assert.Equal(MenuImportMutationOutcome.Updated, outcome.Result);
        Assert.Equal(2, repository.Replaced!.Session.ParseRevision);
        Assert.Equal(started.Session.RawPaste, repository.Replaced.Session.RawPaste);
        Assert.Equal("section", repository.Replaced.Lines.Single(line => line.LineNumber == 1).Disposition);
    }

    [Fact]
    public async Task Undo_removes_only_manual_section_override()
    {
        var (service, repository, _) = CreateService();
        var started = await service.StartAsync(VenueId, "Lunch specials\nDINNER\nBurger  12", null, default);
        _ = await service.SetSectionOverrideAsync(VenueId, started.Session.Id, started.Session.Revision, 1, true, null, default);
        var promoted = repository.Replaced!;
        repository.Current = promoted;

        _ = await service.SetSectionOverrideAsync(VenueId, started.Session.Id, promoted.Session.Revision, 1, false, null, default);

        Assert.Equal("unresolved", repository.Replaced!.Lines.Single(line => line.LineNumber == 1).Disposition);
        Assert.Equal("section", repository.Replaced.Lines.Single(line => line.LineNumber == 2).Disposition);
    }

    [Fact]
    public async Task Get_reparses_when_a_library_candidate_dependency_changes()
    {
        var (service, repository, content) = CreateService();
        var item = new Item { Id = Guid.NewGuid(), VenueId = VenueId, Name = "Burger", Price = "12", IsActive = true };
        content.Items.Add(item);
        var started = await service.StartAsync(VenueId, "Burger  14", null, default);
        Assert.True(Assert.Single(Assert.Single(started.Questions).Candidates).IsSafe);

        item.IsActive = false;
        var refreshed = await service.GetAsync(VenueId, started.Session.Id, default);

        Assert.NotNull(refreshed);
        Assert.Equal(2, refreshed.Session.ParseRevision);
        Assert.Empty(refreshed.Questions);
        Assert.NotNull(repository.Replaced);
    }

    [Fact]
    public async Task Get_invalidates_question_identity_when_an_import_allowance_changes()
    {
        var (service, repository, content) = CreateService();
        var started = await service.StartAsync(VenueId, "Chef note", null, default);
        var originalFingerprint = Assert.Single(started.Questions).Fingerprint;

        content.Ceilings[MenuCeilings.ImportLines] = 1999;
        var refreshed = await service.GetAsync(VenueId, started.Session.Id, default);

        Assert.NotNull(refreshed);
        Assert.NotEqual(originalFingerprint, Assert.Single(refreshed.Questions).Fingerprint);
        Assert.Equal(2, refreshed.Session.ParseRevision);
        Assert.NotNull(repository.Replaced);
    }

    [Fact]
    public async Task Promotion_refuses_a_stale_open_session_after_its_allowance_drops()
    {
        var (service, _, content) = CreateService();
        var started = await service.StartAsync(VenueId, "Lunch specials\nBurger  12", null, default);
        content.Ceilings[MenuCeilings.ImportLines] = 1;

        var exception = await Assert.ThrowsAsync<MenuImportValidationException>(() =>
            service.SetSectionOverrideAsync(VenueId, started.Session.Id, started.Session.Revision, 1, true, null, default));

        Assert.Contains("no longer fits", exception.Message);
    }

    [Fact]
    public async Task Create_destination_persists_the_name_without_mutating_a_menu()
    {
        var (service, repository, _) = CreateService();
        var started = await service.StartAsync(VenueId, "DINNER\nBurger  12", "owner", default);

        var outcome = await service.SetCreateDestinationAsync(VenueId, started.Session.Id, started.Session.Revision, " Dinner ", "owner", default);

        Assert.Equal(MenuImportMutationOutcome.Updated, outcome.Result);
        Assert.Equal(" Dinner ", repository.Current!.Session.ProposedMenuName);
        Assert.Null(repository.Current.Session.CompletedMenuId);
    }

    [Fact]
    public async Task Confirm_create_resolves_current_menu_and_item_allowances()
    {
        var (service, repository, content) = CreateService();
        content.Ceilings[MenuCeilings.MenusPerVenue] = 7;
        content.Ceilings[MenuCeilings.ItemsPerMenu] = 33;
        var started = await service.StartAsync(VenueId, "DINNER\nBurger  12", "owner", default);
        var named = await service.SetCreateDestinationAsync(VenueId, started.Session.Id, started.Session.Revision, "Dinner", "owner", default);

        var outcome = await service.ConfirmCreateAsync(VenueId, started.Session.Id, named.Aggregate!.Session.Revision,
            Guid.NewGuid(), ["organization_administrator"], "owner", default);

        Assert.Equal(MenuImportCreateOutcome.Created, outcome.Result);
        Assert.True(repository.ConfirmCalled);
    }

    private static (MenuImportService Service, ImportRepositoryFake Repository, FakeContentRepository Content) CreateService()
    {
        var repository = new ImportRepositoryFake();
        var content = new FakeContentRepository();
        content.Ceilings[MenuCeilings.ImportLines] = 2000;
        var options = new StaticOptions(new MenuBuilderOptions { ImportFileSizeLimitBytes = 1_000_000, PublishRetrySilenceThreshold = TimeSpan.FromSeconds(5), HistoryRetentionDepth = 50 });
        var service = new MenuImportService(repository, content, new MenuBuilderConfigurationResolver(content, options), new MenuPasteParser(), new FixedClock());
        return (service, repository, content);
    }

    private sealed class ImportRepositoryFake : IMenuImportRepository
    {
        public MenuImportAggregate? Created { get; private set; }
        public MenuImportAggregate? Replaced { get; private set; }
        public MenuImportAggregate? Current { get; set; }
        public bool ConfirmCalled { get; private set; }
        public Task<MenuImportAggregate> CreateAsync(MenuImportAggregate aggregate, CancellationToken cancellationToken = default)
        {
            Created = Current = aggregate with { Session = aggregate.Session with { Revision = new byte[8] } }; return Task.FromResult(Current);
        }
        public Task<MenuImportAggregate?> GetAsync(Guid venueId, Guid sessionId, DateTime nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(Current);
        public Task<MenuImportMutationOutcome> ReplaceParseAsync(MenuImportAggregate aggregate, byte[] expectedRevision, CancellationToken cancellationToken = default)
        {
            Replaced = Current = aggregate with { Session = aggregate.Session with { Revision = new byte[8] } }; return Task.FromResult(new MenuImportMutationOutcome(MenuImportMutationOutcome.Updated, Current));
        }
        public Task<MenuImportMutationOutcome> PutAnswerAsync(Guid venueId, Guid sessionId, byte[] expectedRevision, string questionKey, string fingerprint, string choice, Guid? selectedItemId, DateTime answeredUtc, string? answeredBy, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<MenuImportMutationOutcome> AcceptSafeMatchesAsync(Guid venueId, Guid sessionId, byte[] expectedRevision, DateTime answeredUtc, string? answeredBy, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<MenuImportMutationOutcome> SetCreateDestinationAsync(Guid venueId, Guid sessionId, byte[] expectedRevision, string menuName, DateTime nowUtc, string? actor, CancellationToken cancellationToken = default)
        {
            Current = Current! with { Session = Current.Session with { Destination = MenuImportDestinations.Create, ProposedMenuName = menuName, Revision = new byte[8] } };
            return Task.FromResult(new MenuImportMutationOutcome(MenuImportMutationOutcome.Updated, Current));
        }
        public Task<MenuImportCreateOutcome> ConfirmCreateAsync(Guid venueId, Guid sessionId, byte[] expectedRevision, Guid actorUserId, IReadOnlyCollection<string> systemRoleKeys, DateTime nowUtc, string? actor, CancellationToken cancellationToken = default)
        {
            ConfirmCalled = true;
            var menuId = Guid.NewGuid();
            Current = Current! with { Session = Current.Session with { CompletedMenuId = menuId, CompletedUtc = nowUtc } };
            return Task.FromResult(new MenuImportCreateOutcome(MenuImportCreateOutcome.Created, Current, menuId));
        }
        public Task<int> DeleteExpiredAsync(DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class StaticOptions(MenuBuilderOptions value) : IOptionsMonitor<MenuBuilderOptions>
    {
        public MenuBuilderOptions CurrentValue => value;
        public MenuBuilderOptions Get(string? name) => value;
        public IDisposable? OnChange(Action<MenuBuilderOptions, string?> listener) => null;
    }
}

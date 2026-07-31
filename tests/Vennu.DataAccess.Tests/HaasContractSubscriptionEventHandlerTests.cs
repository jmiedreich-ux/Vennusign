using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class HaasContractSubscriptionEventHandlerTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 31, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ConfirmedCreatedEvent_ActivatesSeparateContract()
    {
        var venueId = Guid.NewGuid();
        var repository = new RepositoryFake();
        var handler = CreateHandler(repository);

        var applied = await handler.HandleAsync(new HaasContractSubscriptionEvent(
            "evt_haas_created", "customer.subscription.created", "sub_haas", venueId,
            "bar_pack", 24, "active", UtcNow.UtcDateTime));

        Assert.True(applied);
        var contract = Assert.Single(repository.Items);
        Assert.Equal(venueId, contract.VenueId);
        Assert.Equal("bar_pack", contract.BundleKey);
        Assert.Equal(159m, contract.MonthlyAmount);
        Assert.Equal(UtcNow.UtcDateTime.AddMonths(24), contract.ContractEndsUtc);
    }

    [Fact]
    public async Task ConfirmedDeletedEvent_RecordsEarlyEndWithoutCollecting()
    {
        var contract = new HaasContract
        {
            Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), BundleKey = "starter_kit", TermMonths = 18,
            MonthlyAmount = 89m, StripeSubscriptionId = "sub_haas", Status = "active",
            StartedUtc = UtcNow.UtcDateTime.AddMonths(-3), ContractEndsUtc = UtcNow.UtcDateTime.AddMonths(15)
        };
        var repository = new RepositoryFake(contract);
        var handler = CreateHandler(repository);

        await handler.HandleAsync(new HaasContractSubscriptionEvent(
            "evt_haas_deleted", "customer.subscription.deleted", "sub_haas"));

        Assert.Equal("canceled", contract.Status);
        Assert.Equal(UtcNow.UtcDateTime, contract.EndedUtc);
    }

    private static HaasContractSubscriptionEventHandler CreateHandler(RepositoryFake repository) =>
        new(new IdempotencyFake(), repository, new FixedTimeProvider(UtcNow));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class IdempotencyFake : IStripeEventIdempotencyService
    {
        public async Task<bool> ExecuteOnceAsync(string eventId, string eventType, Func<CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        {
            await handler(cancellationToken);
            return true;
        }
    }

    private sealed class RepositoryFake(params HaasContract[] contracts) : IHaasContractRepository
    {
        public List<HaasContract> Items { get; } = contracts.ToList();
        public Task<HaasContract?> GetCurrentByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(item => item.VenueId == venueId));
        public Task<HaasContract?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(item => item.StripeSubscriptionId == stripeSubscriptionId));
        public Task<bool> SaveAsync(HaasContract contract, CancellationToken cancellationToken = default)
        {
            if (!Items.Contains(contract)) Items.Add(contract);
            return Task.FromResult(true);
        }
    }
}

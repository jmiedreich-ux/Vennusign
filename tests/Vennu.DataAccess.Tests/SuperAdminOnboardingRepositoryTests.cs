using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public sealed class SuperAdminOnboardingRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task SearchAsync_ReturnsSafeProjectionAndNormalizesSearch()
    {
        string? sql = null;
        object? parameters = null;
        var expected = new SuperAdminOnboardingRecord { CustomerEmail = "owner@example.com", SubscriptionStatus = "trialing" };
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (statement, values) => { sql = statement; parameters = values; return [expected]; }
        };

        var result = await new SuperAdminOnboardingRepository(data).SearchAsync("  owner  ");

        Assert.Same(expected, Assert.Single(result));
        Assert.Contains("CustomerOnboardingStates", sql!);
        Assert.DoesNotContain("StripeCustomerId", sql!);
        Assert.DoesNotContain("StripeSubscriptionId", sql!);
        Assert.Equal("owner", parameters!.GetType().GetProperty("Search")!.GetValue(parameters));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SearchAsync_RejectsUnboundedSearch()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new SuperAdminOnboardingRepository(new FakeSqlDataAccess()).SearchAsync(new string('x', 101)));
    }
}

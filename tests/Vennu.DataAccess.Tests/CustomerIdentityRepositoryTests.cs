using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerIdentityRepositoryTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 22, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateUserAsync_NormalizesEmailAndAssignsIdentityAndTimestamps()
    {
        var data = new FakeSqlDataAccess();
        var repository = new CustomerIdentityRepository(data, new FixedTimeProvider(UtcNow));

        var result = await repository.CreateUserAsync(new CustomerUser
        {
            Email = " Customer@Example.com ",
            DisplayName = " Customer Owner "
        });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Customer@Example.com", result.Email);
        Assert.Equal("CUSTOMER@EXAMPLE.COM", result.NormalizedEmail);
        Assert.Equal("Customer Owner", result.DisplayName);
        Assert.Equal(UtcNow.UtcDateTime, result.CreatedUtc);
        Assert.Equal(UtcNow.UtcDateTime, result.UpdatedUtc);
        Assert.Same(result, Assert.Single(data.InsertedEntities));
    }

    [Fact]
    public async Task GetUserByEmailAsync_UsesNormalizedDeterministicLookup()
    {
        string? sql = null;
        object? parameters = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, capturedParameters) =>
            {
                sql = capturedSql;
                parameters = capturedParameters;
                return [];
            }
        };
        var repository = new CustomerIdentityRepository(data, new FixedTimeProvider(UtcNow));

        Assert.Null(await repository.GetUserByEmailAsync(" customer@example.com "));

        Assert.Contains("WHERE NormalizedEmail = @NormalizedEmail", sql, StringComparison.Ordinal);
        Assert.Equal("CUSTOMER@EXAMPLE.COM", Property<string>(parameters!, "NormalizedEmail"));
    }

    [Fact]
    public async Task LinkExternalIdentityAsync_TrimsSubjectAndPropagatesCancellation()
    {
        var data = new FakeSqlDataAccess();
        var repository = new CustomerIdentityRepository(data, new FixedTimeProvider(UtcNow));
        using var source = new CancellationTokenSource();

        var identity = await repository.LinkExternalIdentityAsync(new ExternalIdentity
        {
            UserId = Guid.NewGuid(), Provider = ExternalIdentityProvider.Google, ProviderSubject = " google-subject "
        }, source.Token);

        Assert.Equal("google-subject", identity.ProviderSubject);
        Assert.Equal(source.Token, data.LastCancellationToken);
    }

    [Fact]
    public async Task GetExternalIdentityAsync_IsScopedByProviderAndSubject()
    {
        string? sql = null;
        object? parameters = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, capturedParameters) =>
            {
                sql = capturedSql;
                parameters = capturedParameters;
                return [];
            }
        };
        var repository = new CustomerIdentityRepository(data, new FixedTimeProvider(UtcNow));

        Assert.Null(await repository.GetExternalIdentityAsync(ExternalIdentityProvider.Apple, "apple-subject"));

        Assert.Contains("Provider = @Provider AND ProviderSubject = @ProviderSubject", sql, StringComparison.Ordinal);
        Assert.Equal((int)ExternalIdentityProvider.Apple, Property<int>(parameters!, "Provider"));
        Assert.Equal("apple-subject", Property<string>(parameters!, "ProviderSubject"));
    }

    private static T Property<T>(object value, string name) =>
        (T)value.GetType().GetProperty(name)!.GetValue(value)!;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

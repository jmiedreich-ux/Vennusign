using Vennu.Data.Configuration;

namespace Vennu.DataAccess.Tests.Configuration;

public sealed class SystemConfigurationProviderHealthTests
{
    [Fact]
    public void SuccessAndFailureTransitionsAreSanitized()
    {
        var health = new SystemConfigurationProviderHealth();
        health.Enable();
        health.RecordSuccess();

        var successful = health.Snapshot();
        Assert.True(successful.Enabled);
        Assert.True(successful.Healthy);
        Assert.NotNull(successful.LastSuccessfulLoadUtc);

        health.RecordFailure(new InvalidOperationException("sensitive connection detail"));
        var failed = health.Snapshot();
        Assert.False(failed.Healthy);
        Assert.Equal(nameof(InvalidOperationException), failed.LastFailure);
        Assert.DoesNotContain("sensitive", failed.LastFailure, StringComparison.OrdinalIgnoreCase);

        health.RecordSuccess();
        Assert.True(health.Snapshot().Healthy);
    }
}

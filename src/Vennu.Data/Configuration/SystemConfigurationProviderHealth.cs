namespace Vennu.Data.Configuration;

public sealed class SystemConfigurationProviderHealth
{
    private readonly object gate = new();
    private DateTime? lastSuccessfulLoadUtc;
    private DateTime? lastFailureUtc;
    private string? lastFailure;

    public bool Enabled { get; private set; }

    public void Enable() => Enabled = true;

    public void RecordSuccess()
    {
        lock (gate)
        {
            lastSuccessfulLoadUtc = DateTime.UtcNow;
            lastFailure = null;
        }
    }

    public void RecordFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        lock (gate)
        {
            lastFailureUtc = DateTime.UtcNow;
            lastFailure = exception.GetType().Name;
        }
    }

    public SystemConfigurationProviderHealthSnapshot Snapshot()
    {
        lock (gate) return new(Enabled, lastSuccessfulLoadUtc, lastFailureUtc, lastFailure, Enabled && lastSuccessfulLoadUtc is not null && lastFailure is null);
    }
}

public sealed record SystemConfigurationProviderHealthSnapshot(
    bool Enabled,
    DateTime? LastSuccessfulLoadUtc,
    DateTime? LastFailureUtc,
    string? LastFailure,
    bool Healthy);

using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public interface IPosWebhookVerifier
{
    PosProvider Provider { get; }
    string SignatureHeaderName { get; }
    VerifiedPosWebhookEvent Verify(string payload, string signature);
    IReadOnlyCollection<VerifiedPosWebhookEvent> VerifyMany(string payload, string signature) =>
        [Verify(payload, signature)];
}

public interface IPosWebhookWorkSignal
{
    void Signal();
    Task WaitAsync(TimeSpan maximumDelay, CancellationToken cancellationToken);
}

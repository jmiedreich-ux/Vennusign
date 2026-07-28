namespace Vennu.Api.Contracts.Admin;

public sealed record FeatureMatrixCellChange(Guid TierId, Guid FeatureId, bool Enabled);

public sealed record FeatureMatrixUpdateRequest(IReadOnlyCollection<FeatureMatrixCellChange> Changes);

public sealed record FeatureMatrixUpdateResponse(int ChangedCount);

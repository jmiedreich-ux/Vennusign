namespace Vennu.Data.Services;

public sealed record FeatureMatrixChange(Guid TierId, Guid FeatureId, bool Enabled);

namespace Vennu.Data.Configuration;

public interface ISystemConfigurationOperationsService
{
    Task<IReadOnlyList<SystemConfigurationRevision>> GetRevisionsAsync(Guid definitionId, string environmentName, CancellationToken cancellationToken = default);
    Task RollbackAsync(SystemConfigurationRollback rollback, CancellationToken cancellationToken = default);
}

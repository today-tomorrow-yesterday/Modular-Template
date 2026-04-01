namespace ModularTemplate.Domain;

/// <summary>
/// Generic read-only repository interface.
/// Used by cache projections and as a base for full repositories.
/// </summary>
public interface IReadRepository<TEntity, in TId>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntity>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default);
}

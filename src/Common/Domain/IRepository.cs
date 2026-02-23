using Rtl.Core.Domain.Entities;

namespace Rtl.Core.Domain;

/// <summary>
/// Generic repository interface for domain entities.
/// Extends IReadRepository with write operations.
/// </summary>
public interface IRepository<TEntity, in TId> : IReadRepository<TEntity, TId>
    where TEntity : Entity
{
    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}

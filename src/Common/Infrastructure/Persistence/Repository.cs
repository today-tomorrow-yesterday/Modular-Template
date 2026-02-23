using Microsoft.EntityFrameworkCore;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Entities;

namespace Rtl.Core.Infrastructure.Persistence;

/// <summary>
/// Generic repository base class providing common CRUD operations.
/// Extends ReadRepository with write operations.
/// </summary>
public abstract class Repository<TEntity, TId, TDbContext>(TDbContext dbContext)
    : ReadRepository<TEntity, TId, TDbContext>(dbContext), IRepository<TEntity, TId>
    where TEntity : Entity
    where TDbContext : DbContext
{
    public virtual void Add(TEntity entity)
        => DbSet.Add(entity);

    public virtual void AddRange(IEnumerable<TEntity> entities)
        => DbSet.AddRange(entities);

    public virtual void Update(TEntity entity)
        => DbSet.Update(entity);

    public virtual void Remove(TEntity entity)
        => DbSet.Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
        => DbSet.RemoveRange(entities);
}

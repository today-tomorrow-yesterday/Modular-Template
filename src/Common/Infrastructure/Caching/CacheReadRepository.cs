using Microsoft.EntityFrameworkCore;
using Rtl.Core.Domain.Caching;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Rtl.Core.Infrastructure.Caching;

/// <summary>
/// Base repository for cache projection entities.
/// Provides standard read operations with a configurable ID type.
/// </summary>
/// <typeparam name="TEntity">The cache projection entity type.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <typeparam name="TDbContext">The DbContext type.</typeparam>
public abstract class CacheReadRepository<TEntity, TId, TDbContext>(TDbContext dbContext)
    : ReadRepository<TEntity, TId, TDbContext>(dbContext)
    where TEntity : class, ICacheProjection
    where TDbContext : DbContext
{
    /// <summary>
    /// Cache projections use Id as the identifier property.
    /// </summary>
    protected override Expression<Func<TEntity, TId>> IdSelector => GetIdSelector();

    /// <summary>
    /// Gets the ID selector expression. Override if your entity uses a different ID property name.
    /// </summary>
    protected virtual Expression<Func<TEntity, TId>> GetIdSelector()
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var idProperty = Expression.Property(parameter, "Id");
        return Expression.Lambda<Func<TEntity, TId>>(idProperty, parameter);
    }
}

/// <summary>
/// Base repository for cache projection entities with int IDs.
/// Convenience class for the common case.
/// </summary>
/// <typeparam name="TEntity">The cache projection entity type.</typeparam>
/// <typeparam name="TDbContext">The DbContext type.</typeparam>
public abstract class CacheReadRepository<TEntity, TDbContext>(TDbContext dbContext)
    : CacheReadRepository<TEntity, int, TDbContext>(dbContext)
    where TEntity : class, ICacheProjection
    where TDbContext : DbContext
{
}

using Microsoft.EntityFrameworkCore;
using ModularTemplate.Domain;
using System.Linq.Expressions;

namespace ModularTemplate.Infrastructure.Persistence;

/// <summary>
/// Generic read-only repository base class.
/// </summary>
public abstract class ReadRepository<TEntity, TId, TDbContext>(TDbContext dbContext)
    : IReadRepository<TEntity, TId>
    where TEntity : class
    where TDbContext : DbContext
{
    protected TDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

    /// <summary>
    /// Expression that selects the ID property from the entity.
    /// </summary>
    protected abstract Expression<Func<TEntity, TId>> IdSelector { get; }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(BuildIdPredicate(id), cancellationToken);

    public virtual async Task<IReadOnlyCollection<TEntity>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Builds a predicate expression to find an entity by its identifier.
    /// </summary>
    private Expression<Func<TEntity, bool>> BuildIdPredicate(TId id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var idProperty = ((MemberExpression)IdSelector.Body).Member;
        var propertyAccess = Expression.MakeMemberAccess(parameter, idProperty);
        var idValue = Expression.Constant(id);
        var equals = Expression.Equal(propertyAccess, idValue);
        return Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
    }
}

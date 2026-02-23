namespace Rtl.Core.Application.Persistence;

/// <summary>
/// Represents a unit of work for coordinating persistence operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Module-specific unit of work interface to prevent DI registration conflicts.
/// Each module should define a marker interface that inherits from this.
/// </summary>
public interface IUnitOfWork<TModule> : IUnitOfWork
    where TModule : class
{
}

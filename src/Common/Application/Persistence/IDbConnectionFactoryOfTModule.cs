using System.Data.Common;

namespace Rtl.Core.Application.Persistence;

/// <summary>
/// Module-specific factory for creating database connections.
/// </summary>
/// <remarks>
/// <para>
/// This generic interface enables each module to have its own database connection factory,
/// allowing modules to connect to separate databases when deployed independently.
/// </para>
/// <para>
/// The <typeparamref name="TModule"/> type parameter is a marker interface (e.g., ISampleOrdersModule)
/// that uniquely identifies the module. This enables the DI container to resolve the correct
/// connection factory for each module.
/// </para>
/// </remarks>
/// <typeparam name="TModule">The module marker interface type.</typeparam>
public interface IDbConnectionFactory<TModule>
    where TModule : class
{
    /// <summary>
    /// Opens and returns a database connection for the module.
    /// </summary>
    /// <returns>An open database connection.</returns>
    ValueTask<DbConnection> OpenConnectionAsync();
}

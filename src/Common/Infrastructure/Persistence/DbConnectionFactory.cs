using Npgsql;
using Rtl.Core.Application.Persistence;
using System.Data.Common;

namespace Rtl.Core.Infrastructure.Persistence;

/// <summary>
/// Module-specific PostgreSQL implementation of <see cref="IDbConnectionFactory{TModule}"/>.
/// </summary>
/// <remarks>
/// <para>
internal sealed class DbConnectionFactory<TModule>(NpgsqlDataSource dataSource)
    : IDbConnectionFactory<TModule>
    where TModule : class
{
    public async ValueTask<DbConnection> OpenConnectionAsync() =>
        await dataSource.OpenConnectionAsync();
}

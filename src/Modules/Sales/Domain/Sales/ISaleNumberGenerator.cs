namespace Modules.Sales.Domain.Sales;

public interface ISaleNumberGenerator
{
    Task<int> GenerateNextAsync(CancellationToken cancellationToken = default);
}

namespace Modules.Sales.Domain.Cdc;

public interface ICdcProjectCostQueries
{
    Task<IReadOnlyCollection<CdcProjectCostCategory>> GetCategoriesWithItemsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CdcProjectCostStateMatrix>> GetStateMatrixAsync(
        CancellationToken cancellationToken = default);
}

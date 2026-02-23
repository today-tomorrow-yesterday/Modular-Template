using Rtl.Core.Domain;

namespace Modules.Inventory.Domain.LandCosts;

public interface ILandCostRepository : IReadRepository<LandCost, int>
{
    Task<IReadOnlyCollection<LandCost>> GetByHomeCenterAndStockNumbersAsync(
        int homeCenterNumber,
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default);
}

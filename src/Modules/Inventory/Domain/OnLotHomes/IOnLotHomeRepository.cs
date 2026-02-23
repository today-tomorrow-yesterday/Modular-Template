using Rtl.Core.Domain;

namespace Modules.Inventory.Domain.OnLotHomes;

public interface IOnLotHomeRepository : IRepository<OnLotHome, int>
{
    Task<OnLotHome?> GetByHomeCenterAndStockNumberAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OnLotHome>> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OnLotHome>> GetByDimensionsAsync(
        decimal length,
        decimal width,
        CancellationToken cancellationToken = default);
}

using Rtl.Core.Domain;

namespace Modules.Inventory.Domain.LandParcels;

public interface ILandParcelRepository : IRepository<LandParcel, int>
{
    Task<LandParcel?> GetByHomeCenterAndStockNumberAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LandParcel>> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default);
}

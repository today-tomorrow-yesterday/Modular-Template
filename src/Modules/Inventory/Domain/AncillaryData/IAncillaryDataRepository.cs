using Rtl.Core.Domain;

namespace Modules.Inventory.Domain.AncillaryData;

public interface IAncillaryDataRepository : IReadRepository<AncillaryData, int>
{
    Task<IReadOnlyCollection<AncillaryData>> GetByHomeCenterAndStockNumbersAsync(
        int homeCenterNumber,
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default);
}

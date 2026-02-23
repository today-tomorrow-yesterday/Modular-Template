using Rtl.Core.Domain;

namespace Modules.Inventory.Domain.WheelsAndAxles;

public interface IWheelsAndAxlesTransactionRepository : IReadRepository<WheelsAndAxlesTransaction, int>
{
    Task<IReadOnlyCollection<WheelsAndAxlesTransaction>> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default);

    Task<WheelsAndAxlesTransaction?> GetLatestByStockNumbersAsync(
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default);
}

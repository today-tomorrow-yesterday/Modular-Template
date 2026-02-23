using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Inventory.Domain.WheelsAndAxles;

public sealed class WheelsAndAxlesTransaction : ICacheProjection
{
    public int Id { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public int RefTransactionId { get; set; }

    public DateTime? Date { get; set; }

    public string? Type { get; set; }

    public string? StockNumber { get; set; }

    public string? Description { get; set; }

    public int Wheels { get; set; }

    [SensitiveData] public decimal WheelValue { get; set; }

    public int BrakeAxles { get; set; }

    [SensitiveData] public decimal BrakeAxleValue { get; set; }

    public int IdlerAxles { get; set; }

    [SensitiveData] public decimal IdlerAxleValue { get; set; }

    [SensitiveData] public decimal TotalWheelsAndAxlesValue { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}

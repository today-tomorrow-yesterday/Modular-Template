using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.TradeIns;

public sealed class TradeInDetails : IVersionedDetails
{
    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public string TradeType { get; private set; } = string.Empty; // Free-form: legacy values include home types and other trade-in categories
    public int Year { get; private set; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public decimal? FloorWidth { get; private set; }
    public decimal? FloorLength { get; private set; }
    public decimal TradeAllowance { get; private set; }
    public decimal PayoffAmount { get; private set; }
    public decimal BookInAmount { get; private set; } // trade-over-allowance = TradeAllowance - BookInAmount

    private TradeInDetails() { }

    public static TradeInDetails Create(
        string tradeType,
        int year,
        string make,
        string model,
        decimal tradeAllowance,
        decimal payoffAmount,
        decimal bookInAmount,
        decimal? floorWidth = null,
        decimal? floorLength = null)
    {
        return new TradeInDetails
        {
            TradeType = tradeType,
            Year = year,
            Make = make,
            Model = model,
            TradeAllowance = Math.Round(tradeAllowance, 2),
            PayoffAmount = Math.Round(payoffAmount, 2),
            BookInAmount = Math.Round(bookInAmount, 2),
            FloorWidth = floorWidth,
            FloorLength = floorLength
        };
    }
}

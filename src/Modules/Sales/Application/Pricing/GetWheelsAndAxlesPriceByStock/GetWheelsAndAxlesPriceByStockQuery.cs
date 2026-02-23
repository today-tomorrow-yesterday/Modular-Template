using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Pricing.GetWheelsAndAxlesPriceByStock;

public sealed record GetWheelsAndAxlesPriceByStockQuery(
    Guid PublicSaleId,
    string StockNumber,
    bool IsRetaining = false) : IQuery<decimal>;

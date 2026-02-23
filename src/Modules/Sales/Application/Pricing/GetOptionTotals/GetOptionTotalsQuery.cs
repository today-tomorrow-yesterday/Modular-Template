using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Pricing.GetOptionTotals;

public sealed record GetOptionTotalsQuery(
    Guid PublicSaleId,
    int PlantNumber,
    int QuoteNumber,
    int OrderNumber,
    string EffectiveDate) : IQuery<OptionTotalsResult>;

public sealed record OptionTotalsResult(
    decimal HbgOptionTotal,
    decimal RetailOptionTotal);

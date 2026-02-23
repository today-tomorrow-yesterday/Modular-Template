using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Pricing.GetRetailPrice;

public sealed record GetRetailPriceQuery(
    Guid PublicSaleId,
    string SerialNumber,
    decimal InvoiceTotal,
    int NumberOfAxles,
    decimal HbgOptionTotal,
    decimal RetailOptionTotal,
    string ModelNumber,
    decimal BaseCost,
    string EffectiveDate) : IQuery<decimal>;

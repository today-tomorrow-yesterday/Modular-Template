using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Pricing.GetWheelsAndAxlesPrice;

public sealed record GetWheelsAndAxlesPriceQuery(
    Guid PublicSaleId,
    int NumberOfWheels,
    int NumberOfAxles) : IQuery<decimal>;

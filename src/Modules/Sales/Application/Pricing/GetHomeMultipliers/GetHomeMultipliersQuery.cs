using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Pricing.GetHomeMultipliers;

public sealed record GetHomeMultipliersQuery(
    Guid PublicSaleId,
    DateOnly? EffectiveDate = null) : IQuery<HomeMultipliersResult>;

public sealed record HomeMultipliersResult(
    DateOnly EffectiveDate,
    decimal BaseHomeMultiplier,
    decimal UpgradesMultiplier,
    decimal FreightMultiplier,
    decimal WheelsAxlesMultiplier,
    decimal DuesMultiplier);

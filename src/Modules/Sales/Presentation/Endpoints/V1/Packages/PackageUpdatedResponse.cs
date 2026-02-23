namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

public sealed record PackageUpdatedResponse(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageInsurance;

public sealed record UpdatePackageInsuranceCommand(
    Guid PackagePublicId,
    string InsuranceType,
    decimal CoverageAmount,
    bool HasFoundationOrMasonry,
    bool InParkOrSubdivision,
    bool IsLandOwnedByCustomer,
    bool IsPremiumFinanced,
    string QuoteId,
    string CompanyName,
    decimal MaxCoverage,
    decimal TotalPremium) : ICommand<UpdatePackageInsuranceResult>;

public sealed record UpdatePackageInsuranceResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

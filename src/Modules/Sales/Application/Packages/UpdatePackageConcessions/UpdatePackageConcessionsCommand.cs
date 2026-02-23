using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageConcessions;

public sealed record UpdatePackageConcessionsCommand(
    Guid PackagePublicId,
    decimal Amount) : ICommand<UpdatePackageConcessionsResult>;

public sealed record UpdatePackageConcessionsResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

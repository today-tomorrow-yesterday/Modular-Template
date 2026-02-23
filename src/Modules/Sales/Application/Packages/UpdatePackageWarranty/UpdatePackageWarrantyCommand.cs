using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageWarranty;

public sealed record UpdatePackageWarrantyCommand(
    Guid PackagePublicId,
    bool WarrantySelected,
    decimal WarrantyAmount) : ICommand<UpdatePackageWarrantyResult>;

public sealed record UpdatePackageWarrantyResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

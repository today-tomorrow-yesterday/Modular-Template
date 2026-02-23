using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageDownPayment;

public sealed record UpdatePackageDownPaymentCommand(
    Guid PackagePublicId,
    decimal Amount) : ICommand<UpdatePackageDownPaymentResult>;

public sealed record UpdatePackageDownPaymentResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

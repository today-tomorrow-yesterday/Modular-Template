using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageDownPayment;

// Flow: PUT /api/v1/packages/{packageId}/down-payment → UpdatePackageDownPaymentCommand →
//   upsert CreditLine(DownPayment) + recalculate gross profit →
//   returns updated GP/CGP/MustRecalculateTaxes
internal sealed class UpdatePackageDownPaymentCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageDownPaymentCommand, UpdatePackageDownPaymentResult>
{
    public async Task<Result<UpdatePackageDownPaymentResult>> Handle(
        UpdatePackageDownPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageDownPaymentResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Upsert down payment line
        var existing = package.Lines
            .OfType<CreditLine>()
            .SingleOrDefault(l => l.IsDownPayment);

        if (IsCreate(existing, request.Amount))
        {
           package.AddLine(CreditLine.CreateDownPayment(package.Id, request.Amount));
        }
        else if (IsUpdate(existing, request.Amount))
        {
            existing!.UpdatePricing(request.Amount, estimatedCost: 0m, retailSalePrice: 0m);
        }
        else if (IsDelete(existing, request.Amount))
        {
            package.RemoveLine(existing!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageDownPaymentResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private static bool IsCreate(CreditLine? existing, decimal amount) => existing is null && amount > 0;
    private static bool IsUpdate(CreditLine? existing, decimal amount) => existing is not null && amount > 0;
    private static bool IsDelete(CreditLine? existing, decimal amount) => existing is not null && amount == 0;
}

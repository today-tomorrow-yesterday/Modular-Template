using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.DeletePackage;

// Flow: DELETE /api/v1/packages/{publicPackageId} → DeletePackageCommand →
//   guard primary-with-siblings → hard delete → 204 NoContent
internal sealed class DeletePackageCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<DeletePackageCommand>
{
    public async Task<Result> Handle(
        DeletePackageCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package by PublicId (with tracking)
        var package = await packageRepository.GetByPublicIdAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure(PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Guard — only draft packages can be deleted
        if (package.Status != PackageStatus.Draft)
        {
            return Result.Failure(PackageErrors.OnlyDraftCanBeDeleted());
        }

        // Step 3: Guard — cannot delete the sole remaining package on a sale
        var salePackages = await GetSalePackagesAsync(package.SaleId, cancellationToken);
        if (salePackages.Count <= 1)
        {
            return Result.Failure(PackageErrors.CannotDeleteLastPackage());
        }

        // Step 4: Guard — cannot delete primary package if siblings exist
        if (package.IsPrimaryPackage)
        {
            return Result.Failure(PackageErrors.CannotDeletePrimary());
        }

        // Step 5: Remove package (hard delete)
        packageRepository.Remove(package);

        // Step 4: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<IReadOnlyCollection<Package>> GetSalePackagesAsync(
        int saleId, CancellationToken cancellationToken) =>
        await packageRepository.GetBySaleIdAsync(saleId, cancellationToken);

    private static bool HasSiblings(Package package, IReadOnlyCollection<Package> allPackages) =>
        allPackages.Count(p => p.Id != package.Id) > 0;
}

using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.SetPackageAsPrimary;

// Flow: PATCH /api/v1/packages/{publicPackageId}?action=set-as-primary → SetPackageAsPrimaryCommand →
//   promote target to Ranking 1, demote siblings → 204 NoContent. Idempotent.
internal sealed class SetPackageAsPrimaryCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<SetPackageAsPrimaryCommand>
{
    public async Task<Result> Handle(
        SetPackageAsPrimaryCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load target package by PublicId
        var package = await packageRepository.GetByPublicIdAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure(PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Short-circuit if already primary (idempotent)
        if (package.IsPrimaryPackage)
        {
            return Result.Success();
        }

        // Step 3: Load all sibling packages for the same sale (WITH tracking)
        var allPackages = await packageRepository.GetBySaleIdWithTrackingAsync(
            package.SaleId, cancellationToken);

        // Step 4: Demote all siblings and promote target
        ReassignRankings(package, allPackages);

        // Step 5: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void ReassignRankings(Package target, IReadOnlyCollection<Package> allPackages)
    {
        target.SetPrimary();

        var ranking = 2;
        foreach (var sibling in allPackages)
        {
            if (sibling.Id == target.Id)
            {
                continue;
            }

            sibling.SetNonPrimary(ranking);
            ranking++;
        }
    }
}

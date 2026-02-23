using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageName;

internal sealed class UpdatePackageNameCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageNameCommand>
{
    public async Task<Result> Handle(
        UpdatePackageNameCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package by PublicId (WITH tracking)
        var package = await packageRepository.GetByPublicIdAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Trim name before comparison and storage (legacy behavior)
        var trimmedName = request.Name.Trim();

        // Step 3: Check for duplicate name within the same sale (case-insensitive, exclude self)
        if (await HasDuplicateNameAsync(package, trimmedName, cancellationToken))
        {
            return Result.Failure(PackageErrors.DuplicateName(trimmedName));
        }

        // Step 4: Apply name update
        package.SetName(trimmedName);

        // Step 4: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<bool> HasDuplicateNameAsync(
        Package package,
        string name,
        CancellationToken cancellationToken)
    {
        var salePackages = await packageRepository.GetBySaleIdAsync(
            package.SaleId, cancellationToken);

        return salePackages.Any(p =>
            p.Id != package.Id &&
            string.Equals(p.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase));
    }
}

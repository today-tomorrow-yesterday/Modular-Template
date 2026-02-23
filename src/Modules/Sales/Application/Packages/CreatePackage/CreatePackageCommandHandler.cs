using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.CreatePackage;

// Flow: POST /api/v1/sales/{publicSaleId}/packages → CreatePackageCommand →
//   resolve sale → check duplicate name → determine primary → Package.Create → persist → return PublicId
internal sealed class CreatePackageCommandHandler(
    ISaleRepository saleRepository,
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<CreatePackageCommand, CreatePackageResult>
{
    public async Task<Result<CreatePackageResult>> Handle(
        CreatePackageCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Resolve sale by PublicId to get the internal saleId
        var sale = await saleRepository.GetByPublicIdAsync(request.SalePublicId, cancellationToken);
        if (sale is null)
            return Result.Failure<CreatePackageResult>(SaleErrors.NotFoundByPublicId(request.SalePublicId));

        // Step 2: Load existing packages and check for duplicate name
        var existingPackages = await packageRepository.GetBySaleIdAsync(sale.Id, cancellationToken);
        if (HasDuplicateName(existingPackages, request.Name))
            return Result.Failure<CreatePackageResult>(PackageErrors.DuplicateName(request.Name));

        // Step 3: Determine if this is the first package (auto-set as primary)
        var isPrimary = existingPackages.Count == 0;

        // Step 4: Create package aggregate (raises PackageReadyForFundingDomainEvent internally)
        var package = Package.Create(sale.Id, request.Name, isPrimary);

        // Step 5: Persist
        packageRepository.Add(package);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 6: Return created package identifier
        return new CreatePackageResult(package.PublicId);
    }

    private static bool HasDuplicateName(
        IReadOnlyCollection<Package> existingPackages, string name)
    {
        var trimmedName = name.Trim();
        return existingPackages.Any(p =>
            string.Equals(p.Name?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
    }
}

using Modules.Sales.Domain;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageSalesTeam;

// Flow: PUT /api/v1/packages/{packageId}/sales-team → UpdatePackageSalesTeamCommand →
//   replace SalesTeamLine (PUT semantics) → returns GP/CGP/MustRecalculateTaxes.
//   Sales team is metadata — does not affect GP/tax but returns summary for consistency.
internal sealed class UpdatePackageSalesTeamCommandHandler(
    IPackageRepository packageRepository,
    IAuthorizedUserCacheRepository authorizedUserCacheRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageSalesTeamCommand, UpdatePackageSalesTeamResult>
{
    public async Task<Result<UpdatePackageSalesTeamResult>> Handle(
        UpdatePackageSalesTeamCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package by PublicId (with sale context and line tracking)
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageSalesTeamResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Validate all AuthorizedUserId values exist in cache (active + not retired)
        if (!await ValidateAuthorizedUsersAsync(request.Members, cancellationToken))
        {
            return Result.Failure<UpdatePackageSalesTeamResult>(
                PackageErrors.InvalidAuthorizedUsers());
        }

        // Step 3: Replace existing sales team line with new members (PUT semantics)
        ReplaceSalesTeamLine(package, request.Members);

        // Step 4: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageSalesTeamResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private async Task<bool> ValidateAuthorizedUsersAsync(
        UpdatePackageSalesTeamMemberRequest[] members,
        CancellationToken cancellationToken)
    {
        var authorizedUserIds = members
            .Select(m => m.AuthorizedUserId)
            .ToList();

        return await authorizedUserCacheRepository.AllExistAsync(authorizedUserIds, cancellationToken);
    }

    private static void ReplaceSalesTeamLine(Package package, UpdatePackageSalesTeamMemberRequest[] memberRequests)
    {
        var existing = package.Lines
            .OfType<SalesTeamLine>()
            .SingleOrDefault();

        if (existing is not null)
        {
            package.RemoveLine(existing);
        }

        var members = memberRequests
            .Select(m => SalesTeamMember.Create(m.AuthorizedUserId, m.Role, m.CommissionSplitPercentage))
            .ToList();

        var details = SalesTeamDetails.Create(members);
        package.AddLine(SalesTeamLine.Create(package.Id, details));
    }
}

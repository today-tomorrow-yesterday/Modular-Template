using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.GetPackageById;

internal sealed class GetPackageByIdQueryHandler(
    IPackageRepository packageRepository,
    IFundingRequestCacheRepository fundingRequestCacheRepository,
    IAuthorizedUserCacheRepository authorizedUserCacheRepository)
    : IQueryHandler<GetPackageByIdQuery, PackageDetailResponse>
{
    public async Task<Result<PackageDetailResponse>> Handle(
        GetPackageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<PackageDetailResponse>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        var funding = await fundingRequestCacheRepository.GetByPackageIdAsync(
            package.Id, cancellationToken);

        var employeeNumberMap = await BuildEmployeeNumberMapAsync(package, cancellationToken);

        return PackageDetailMapper.MapToResponse(package, funding, employeeNumberMap);
    }

    private async Task<IReadOnlyDictionary<int, int>?> BuildEmployeeNumberMapAsync(
        Package package, CancellationToken cancellationToken)
    {
        var salesTeamLine = package.Lines.OfType<SalesTeamLine>().SingleOrDefault();
        if (salesTeamLine?.Details?.SalesTeamMembers is not { Count: > 0 } members)
            return null;

        var map = new Dictionary<int, int>();
        foreach (var member in members)
        {
            if (member.AuthorizedUserId is null) continue;
            var user = await authorizedUserCacheRepository.GetByIdAsync(
                member.AuthorizedUserId.Value, cancellationToken);
            if (user is not null)
                map[member.AuthorizedUserId.Value] = user.EmployeeNumber;
        }

        return map.Count > 0 ? map : null;
    }
}

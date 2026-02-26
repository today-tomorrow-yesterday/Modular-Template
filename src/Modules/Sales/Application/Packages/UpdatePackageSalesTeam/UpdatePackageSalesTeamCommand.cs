using Modules.Sales.Domain.Packages.SalesTeam;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageSalesTeam;

public sealed record UpdatePackageSalesTeamCommand(
    Guid PackagePublicId,
    UpdatePackageSalesTeamMemberRequest[] Members) : ICommand<UpdatePackageSalesTeamResult>;

public sealed record UpdatePackageSalesTeamMemberRequest(
    int AuthorizedUserId,
    SalesTeamRole Role,
    decimal? CommissionSplitPercentage);

public sealed record UpdatePackageSalesTeamResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Domain;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;
using System.Text.Json;
using ISeriesCommission = Rtl.Core.Application.Adapters.ISeries.Commission;

namespace Modules.Sales.Application.Commission.CalculateCommission;

// Flow: POST /api/v1/packages/{packageId}/commission → CalculateCommissionCommand →
//   Validate preconditions → Resolve EmployeeNumbers + AppId from local caches →
//   ⬇️ UpdateAllowances → ⬇️ CalculateCommission → Save CGP → SaveChangesAsync
internal sealed class CalculateCommissionCommandHandler(
    IPackageRepository packageRepository,
    IAuthorizedUserCacheRepository authorizedUserCacheRepository,
    IFundingRequestCacheRepository fundingRequestCacheRepository,
    IiSeriesAdapter iSeriesAdapter,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<CalculateCommissionCommand, CalculateCommissionResult>
{
    public async Task<Result<CalculateCommissionResult>> Handle(
        CalculateCommissionCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with full sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<CalculateCommissionResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        var sale = package.Sale;

        // Step 2: Validate preconditions
        if (sale.DeliveryAddress is null)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.NoDeliveryAddress", "Missing delivery address."));
        }

        if (string.IsNullOrWhiteSpace(sale.RetailLocation.Zip))
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.NoHomeCenterZip", "Home center ZIP required."));
        }

        var homeLine = package.Lines.OfType<HomeLine>().SingleOrDefault();
        if (homeLine?.Details is null)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.NoHomeLine", "Home line required."));
        }

        var salesTeamLine = package.Lines.OfType<SalesTeamLine>().SingleOrDefault();
        if (salesTeamLine?.Details is null || salesTeamLine.Details.SalesTeamMembers.Count == 0)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.NoSalesTeam", "Sales team required."));
        }

        var primaryMember = salesTeamLine.Details.SalesTeamMembers
            .FirstOrDefault(m => m.Role == SalesTeamRole.Primary);
        if (primaryMember is null)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.NoPrimarySalesperson", "Primary sales person not found."));
        }

        if (primaryMember.AuthorizedUserId is null)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.PrimarySalespersonNoUser", "Primary salesperson missing authorized user."));
        }

        // Error on missing AuthorizedUserId for secondary (not silent skip)
        var secondaryMember = salesTeamLine.Details.SalesTeamMembers
            .FirstOrDefault(m => m.Role == SalesTeamRole.Secondary);
        if (secondaryMember is not null && secondaryMember.AuthorizedUserId is null)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.SecondarySalespersonNoUser", "Secondary salesperson missing authorized user."));
        }

        // Step 3: Resolve EmployeeNumbers from cache.authorized_users
        var employeeRoleMap = new Dictionary<int, SalesTeamRole>();
        var splits = new List<ISeriesCommission.CommissionSplit>();
        int primaryEmployeeNumber = 0;

        foreach (var member in salesTeamLine.Details.SalesTeamMembers)
        {
            if (member.AuthorizedUserId is null) continue;

            var user = await authorizedUserCacheRepository.GetByIdAsync(
                member.AuthorizedUserId.Value, cancellationToken);

            if (user is null)
            {
                var roleName = member.Role == SalesTeamRole.Primary ? "primary" : "secondary";
                return Result.Failure<CalculateCommissionResult>(Error.Validation(
                    "Commission.SalespersonNotFound",
                    $"Could not find {roleName} salesperson info."));
            }

            if (member.Role == SalesTeamRole.Primary)
                primaryEmployeeNumber = user.EmployeeNumber;

            splits.Add(new ISeriesCommission.CommissionSplit
            {
                EmployeeNumber = user.EmployeeNumber,
                GrossPayPercentage = member.CommissionSplitPercentage ?? 0m, // Split pct → GPP (iSeries input param)
                PayPercentage = 0m, // Pay is output — iSeries returns calculated pay amount
                TotalCommissionRate = null
            });
            employeeRoleMap[user.EmployeeNumber] = member.Role;
        }

        if (splits.Count == 0)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Validation(
                "Commission.NoValidSplits", "No valid sales team members with employee numbers found."));
        }

        // Step 4: Resolve AppId from cache.funding (NOT sale.SaleNumber)
        var fundingCache = await fundingRequestCacheRepository.GetByPackageIdAsync(
            package.Id, cancellationToken);

        if (fundingCache is null)
        {
            return Result.Failure<CalculateCommissionResult>(Error.Failure(
                "Commission.NoFundingRequest", "Error retrieving Funding request for package."));
        }

        var appId = ExtractAppId(fundingCache.FundingKeys);
        var homeCenterNumber = sale.RetailLocation.RefHomeCenterNumber ?? 0;

        // Step 5: Update Allowances — must complete before commission calc
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        var isTaxExempt = taxLine?.Details?.TaxExemptionId is not null and not 0;
        var tradeIns = package.Lines.OfType<TradeInLine>().ToList();
        var projectCosts = package.Lines.OfType<ProjectCostLine>().ToList();
        var allowanceProjectCosts = projectCosts
            .Where(pc => !pc.ShouldExcludeFromPricing).ToList();

        var addOns = allowanceProjectCosts.Select(pc => new AllowanceAddOn
        {
            CategoryNumber = pc.Details?.CategoryId ?? 0,
            ItemNumber = pc.Details?.ItemId ?? 0,
            Cost = pc.EstimatedCost,
            SalePrice = pc.SalePrice
        }).ToArray();

        await iSeriesAdapter.UpdateAllowances(
            new AllowanceUpdateRequest
            {
                AppId = appId,
                CorrelationId = Guid.NewGuid(),
                HomeCenterNumber = homeCenterNumber,
                HomeSalePrice = homeLine.SalePrice,
                HomeNetInvoice = homeLine.Details.NetInvoice ?? 0m,
                NumberOfFloorSections = homeLine.Details.NumberOfFloorSections ?? 0,
                FreightCost = homeLine.Details.FreightCost ?? 0m,
                CarrierFrameDeposit = homeLine.Details.CarrierFrameDeposit ?? 0m,
                GrossCost = homeLine.Details.GrossCost ?? 0m,
                TaxIncludedOnInvoice = homeLine.Details.TaxIncludedOnInvoice ?? 0m,
                RebateOnMfgInvoice = homeLine.Details.RebateOnMfgInvoice ?? 0m,
                HomeCondition = MapHomeCondition(homeLine.Details.HomeType),
                TradeAllowance = tradeIns.Sum(t => t.Details?.TradeAllowance ?? 0m),
                BookInAmount = tradeIns.Sum(t => t.Details?.BookInAmount ?? 0m),
                TradeInType = MapTradeInTypeCode(tradeIns.FirstOrDefault()?.Details?.TradeType),
                PreviouslyTitled = taxLine?.Details?.PreviouslyTitled ?? string.Empty,
                IsTaxExempt = isTaxExempt,
                City = sale.DeliveryAddress.City ?? string.Empty,
                County = sale.DeliveryAddress.County ?? string.Empty,
                PostalCode = sale.DeliveryAddress.PostalCode ?? string.Empty,
                IsWithinCityLimits = sale.DeliveryAddress.IsWithinCityLimits,
                PointOfSaleZip = sale.RetailLocation.Zip,
                TotalAddOnCost = allowanceProjectCosts.Sum(pc => pc.EstimatedCost),
                TotalAddOnSalePrice = allowanceProjectCosts.Sum(pc => pc.SalePrice),
                AddOns = addOns
            },
            cancellationToken);

        // Step 6: ⬇️ Calculate Commission (sequential after UpdateAllowances)
        var commissionResult = await iSeriesAdapter.CalculateCommission(
            new ISeriesCommission.CommissionCalculationRequest
            {
                AppId = appId,
                Cost = homeLine.EstimatedCost + projectCosts
                    .Where(pc => !pc.ShouldExcludeFromPricing)
                    .Sum(pc => pc.EstimatedCost),
                LandPayoff = projectCosts
                    .Where(pc => !pc.ShouldExcludeFromPricing && pc.Details?.CategoryId == 3)
                    .Sum(pc => pc.EstimatedCost),
                LandImprovements = 0m,
                AdjustedCost = 0m,
                EmployeeNumber = primaryEmployeeNumber,
                HomeCondition = MapHomeCondition(homeLine.Details.HomeType),
                HomeCenterNumber = homeCenterNumber,
                Splits = splits.ToArray()
            },
            cancellationToken);

        // Step 7: Save CGP to package
        package.SetCommissionableGrossProfit(commissionResult.CommissionableGrossProfit);

        // Build split results for response (include CommissionRatePercentage)
        var splitResults = commissionResult.EmployeeSplits.Select(es =>
            new CommissionSplitResult(
                es.EmployeeNumber,
                employeeRoleMap.TryGetValue(es.EmployeeNumber, out var role) ? PackageDetailMapper.FormatRole(role) : "Unknown",
                es.GrossPayPercentage,
                es.TotalCommissionRate ?? 0m,
                es.Pay)).ToList();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CalculateCommissionResult(
            package.Id,
            commissionResult.CommissionableGrossProfit,
            commissionResult.TotalCommission,
            splitResults);
    }

    private static int ExtractAppId(JsonDocument? fundingKeys)
    {
        if (fundingKeys is null) return 0;

        foreach (var element in fundingKeys.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("Key", out var key)
                && key.GetString() == "AppId"
                && element.TryGetProperty("Value", out var value)
                && int.TryParse(value.GetString(), out var appId))
            {
                return appId;
            }
        }

        return 0;
    }

    private static HomeCondition MapHomeCondition(HomeType homeType) => homeType switch
    {
        HomeType.New => HomeCondition.New,
        HomeType.Used => HomeCondition.Used,
        HomeType.Repo => HomeCondition.Repo,
        _ => HomeCondition.New
    };

    private static char? MapTradeInTypeCode(string? tradeType) => tradeType switch
    {
        "Single Wide" => 'S',
        "Double Wide" => 'D',
        "Modular Home" => 'D',
        "Motorcycle" => 'C',
        "Boat" => 'B',
        "Motor Vehicle" => 'V',
        "Travel Trailer" => 'T',
        "5th Wheel" or "Fifth Wheel" => 'F',
        _ when tradeType is { Length: > 0 } => tradeType[0],
        _ => null
    };
}

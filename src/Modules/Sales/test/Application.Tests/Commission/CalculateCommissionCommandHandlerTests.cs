using Modules.Sales.Application.Commission.CalculateCommission;
using Modules.Sales.Domain;
using Modules.Sales.Domain.AuthorizedUsersCache;
using DomainDeliveryAddress = Modules.Sales.Domain.DeliveryAddresses.DeliveryAddress;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.Domain.Packages.TradeIns;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using System.Text.Json;
using Xunit;
using ISeriesCommission = Rtl.Core.Application.Adapters.ISeries.Commission;

namespace Modules.Sales.Application.Tests.Commission;

public sealed class CalculateCommissionCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IAuthorizedUserCacheRepository _authorizedUserCacheRepository = Substitute.For<IAuthorizedUserCacheRepository>();
    private readonly IFundingRequestCacheRepository _fundingRequestCacheRepository = Substitute.For<IFundingRequestCacheRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly CalculateCommissionCommandHandler _sut;

    public CalculateCommissionCommandHandlerTests()
    {
        _sut = new CalculateCommissionCommandHandler(
            _packageRepository,
            _authorizedUserCacheRepository,
            _fundingRequestCacheRepository,
            _iSeriesAdapter,
            _unitOfWork);
    }

    // --- Validation guard tests ---

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(new CalculateCommissionCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_delivery_address()
    {
        var package = CreatePackageWithContext(includeDeliveryAddress: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.NoDeliveryAddress", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_home_center_zip_missing()
    {
        var package = CreatePackageWithContext(homeCenterZip: "");
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.NoHomeCenterZip", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_home_line()
    {
        var package = CreatePackageWithContext(includeHomeLine: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.NoHomeLine", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_sales_team()
    {
        var package = CreatePackageWithContext(includeSalesTeam: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.NoSalesTeam", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_primary_salesperson()
    {
        var package = CreatePackageWithContext(primaryMemberRole: SalesTeamRole.Secondary);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.NoPrimarySalesperson", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_primary_salesperson_missing_authorized_user()
    {
        var package = CreatePackageWithContext(primaryAuthorizedUserId: null);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.PrimarySalespersonNoUser", result.Error.Code);
    }

    // Error on null AuthorizedUserId for secondary (not silent skip)
    [Fact]
    public async Task Returns_failure_when_secondary_salesperson_missing_authorized_user()
    {
        var package = CreatePackageWithContext(includeSecondary: true, secondaryAuthorizedUserId: null);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.SecondarySalespersonNoUser", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_authorized_user_not_found_in_cache()
    {
        var package = CreatePackageWithContext();
        SetupPackageRepo(package);
        _authorizedUserCacheRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((AuthorizedUserCache?)null);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.SalespersonNotFound", result.Error.Code);
    }

    // AppId from cache.funding, not sale.SaleNumber
    [Fact]
    public async Task Returns_failure_when_no_funding_cache_for_package()
    {
        var package = CreatePackageWithContext();
        SetupPackageRepo(package);
        SetupAuthorizedUserCache(employeeNumber: 1001);
        _fundingRequestCacheRepository.GetByPackageIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((FundingRequestCache?)null);

        var result = await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Commission.NoFundingRequest", result.Error.Code);
    }

    // --- Happy path tests ---

    // Calls UpdateAllowances before CalculateCommission
    [Fact]
    public async Task Calls_update_allowances_before_calculate_commission()
    {
        var package = CreatePackageWithContext();
        SetupFullHappyPath(package, appId: 999999);

        await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Received.InOrder(() =>
        {
            _iSeriesAdapter.UpdateAllowances(Arg.Any<AllowanceUpdateRequest>(), Arg.Any<CancellationToken>());
            _iSeriesAdapter.CalculateCommission(
                Arg.Any<ISeriesCommission.CommissionCalculationRequest>(), Arg.Any<CancellationToken>());
        });
    }

    // AppId extracted from cache.funding FundingKeys JSONB
    [Fact]
    public async Task Passes_app_id_from_funding_cache_to_iseries()
    {
        var package = CreatePackageWithContext();
        SetupFullHappyPath(package, appId: 777888);

        await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        await _iSeriesAdapter.Received(1).UpdateAllowances(
            Arg.Is<AllowanceUpdateRequest>(r => r.AppId == 777888),
            Arg.Any<CancellationToken>());

        await _iSeriesAdapter.Received(1).CalculateCommission(
            Arg.Is<ISeriesCommission.CommissionCalculationRequest>(r => r.AppId == 777888),
            Arg.Any<CancellationToken>());
    }

    // Saves CommissionableGrossProfit to package
    [Fact]
    public async Task Saves_commissionable_gross_profit_to_package()
    {
        var package = CreatePackageWithContext();
        SetupFullHappyPath(package, cgp: 18500m);

        var result = await _sut.Handle(
            new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(18500m, package.CommissionableGrossProfit);
    }

    [Fact]
    public async Task Returns_commission_result_with_cgp_and_total()
    {
        var package = CreatePackageWithContext();
        SetupFullHappyPath(package, cgp: 18500m, totalCommission: 4625m);

        var result = await _sut.Handle(
            new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(18500m, result.Value.CommissionableGrossProfit);
        Assert.Equal(4625m, result.Value.TotalCommission);
    }

    // Response includes CommissionRatePercentage per split
    [Fact]
    public async Task Returns_commission_rate_percentage_per_split()
    {
        var package = CreatePackageWithContext();
        SetupFullHappyPath(package, splitRate: 25.0m);

        var result = await _sut.Handle(
            new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.SplitDetails);
        var split = result.Value.SplitDetails.First();
        Assert.Equal(25.0m, split.CommissionRatePercentage);
        Assert.Equal("Primary Salesperson", split.Role);
    }

    [Fact]
    public async Task Calls_save_changes()
    {
        var package = CreatePackageWithContext();
        SetupFullHappyPath(package);

        await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Passes_correct_home_center_number_to_iseries()
    {
        var package = CreatePackageWithContext(homeCenterNumber: 42);
        SetupFullHappyPath(package);

        await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        await _iSeriesAdapter.Received(1).CalculateCommission(
            Arg.Is<ISeriesCommission.CommissionCalculationRequest>(r => r.HomeCenterNumber == 42),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Includes_project_cost_estimated_cost_in_cost_field()
    {
        var package = CreatePackageWithContext(includeProjectCost: true, projectCostEstimate: 2500m);
        SetupFullHappyPath(package);

        await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        // Cost = homeLine.EstimatedCost (60000) + projectCost.EstimatedCost (2500) = 62500
        await _iSeriesAdapter.Received(1).CalculateCommission(
            Arg.Is<ISeriesCommission.CommissionCalculationRequest>(r => r.Cost == 62500m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Uses_land_project_cost_for_land_payoff()
    {
        // LandPayoff = sum of EstimatedCost for ProjectCostLines with CategoryId == 3 (Land)
        var package = CreatePackageWithContext(includeLandProjectCost: true, landProjectCostEstimate: 15000m);
        SetupFullHappyPath(package);

        await _sut.Handle(new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        await _iSeriesAdapter.Received(1).CalculateCommission(
            Arg.Is<ISeriesCommission.CommissionCalculationRequest>(r => r.LandPayoff == 15000m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handles_two_member_sales_team()
    {
        var package = CreatePackageWithContext(includeSecondary: true, secondaryAuthorizedUserId: 2);
        SetupFullHappyPath(package, includeSecondaryUser: true);

        var result = await _sut.Handle(
            new CalculateCommissionCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.SplitDetails.Count);
    }

    // --- Test helpers ---

    private void SetupPackageRepo(Package package)
    {
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
    }

    private void SetupAuthorizedUserCache(int employeeNumber = 1001, int? secondaryEmployeeNumber = null)
    {
        _authorizedUserCacheRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateAuthorizedUser(1, employeeNumber));

        if (secondaryEmployeeNumber.HasValue)
        {
            _authorizedUserCacheRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
                .Returns(CreateAuthorizedUser(2, secondaryEmployeeNumber.Value));
        }
    }

    private void SetupFullHappyPath(
        Package package,
        int appId = 999999,
        decimal cgp = 18500m,
        decimal totalCommission = 4625m,
        decimal splitRate = 25.0m,
        bool includeSecondaryUser = false)
    {
        SetupPackageRepo(package);
        SetupAuthorizedUserCache(employeeNumber: 1001, secondaryEmployeeNumber: includeSecondaryUser ? 1002 : null);

        _fundingRequestCacheRepository.GetByPackageIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateFundingCache(appId));

        var splits = new List<ISeriesCommission.CommissionSplitResult>
        {
            new() { EmployeeNumber = 1001, Pay = totalCommission, GrossPayPercentage = 100m, TotalCommissionRate = splitRate }
        };

        if (includeSecondaryUser)
        {
            splits[0] = new ISeriesCommission.CommissionSplitResult
                { EmployeeNumber = 1001, Pay = totalCommission * 0.6m, GrossPayPercentage = 60m, TotalCommissionRate = splitRate };
            splits.Add(new ISeriesCommission.CommissionSplitResult
                { EmployeeNumber = 1002, Pay = totalCommission * 0.4m, GrossPayPercentage = 40m, TotalCommissionRate = splitRate });
        }

        _iSeriesAdapter.CalculateCommission(
            Arg.Any<ISeriesCommission.CommissionCalculationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ISeriesCommission.CommissionResult
            {
                CommissionableGrossProfit = cgp,
                TotalCommission = totalCommission,
                EmployeeSplits = splits.ToArray()
            });
    }

    private static Package CreatePackageWithContext(
        bool includeDeliveryAddress = true,
        bool includeHomeLine = true,
        bool includeSalesTeam = true,
        bool includeSecondary = false,
        bool includeProjectCost = false,
        bool includeTradeIn = false,
        bool includeLandProjectCost = false,
        int? primaryAuthorizedUserId = 1,
        int? secondaryAuthorizedUserId = 2,
        SalesTeamRole primaryMemberRole = SalesTeamRole.Primary,
        int homeCenterNumber = 42,
        string homeCenterZip = "43004",
        decimal projectCostEstimate = 0m,
        decimal tradeInPayoff = 0m,
        decimal landProjectCostEstimate = 0m)
    {
        var sale = Sale.Create(
            partyId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C,
            saleNumber: 12345);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocation.CreateHomeCenter(
            homeCenterNumber: homeCenterNumber, name: "Test HC", stateCode: "OH", zip: homeCenterZip, isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        if (includeDeliveryAddress)
        {
            var address = DomainDeliveryAddress.Create(
                saleId: sale.Id,
                occupancyType: "Primary",
                isWithinCityLimits: true,
                addressStyle: null, addressType: null,
                addressLine1: "123 Main St", addressLine2: null, addressLine3: null,
                city: "Columbus", county: "Franklin",
                state: "OH", country: "US", postalCode: "43004");
            address.ClearDomainEvents();
            SetProperty(sale, nameof(Sale.DeliveryAddress), address);
        }

        var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();

        if (includeHomeLine)
        {
            var homeDetails = HomeDetails.Create(
                homeType: HomeType.New,
                homeSourceType: HomeSourceType.OnLot,
                modularType: ModularType.Hud,
                stockNumber: "STK-001",
                netInvoice: 55000m,
                numberOfFloorSections: 2,
                freightCost: 3000m,
                carrierFrameDeposit: 500m,
                grossCost: 58000m,
                taxIncludedOnInvoice: 0m,
                rebateOnMfgInvoice: 0m);
            var homeLine = HomeLine.Create(
                packageId: package.Id,
                salePrice: 85000m,
                estimatedCost: 60000m,
                retailSalePrice: 90000m,
                responsibility: null,
                details: homeDetails);
            package.AddLine(homeLine);
            package.ClearDomainEvents();
        }

        if (includeSalesTeam)
        {
            var members = new List<SalesTeamMember>
            {
                SalesTeamMember.Create(primaryAuthorizedUserId, primaryMemberRole, 100m)
            };

            if (includeSecondary)
            {
                members[0] = SalesTeamMember.Create(primaryAuthorizedUserId, primaryMemberRole, 60m);
                members.Add(SalesTeamMember.Create(secondaryAuthorizedUserId, SalesTeamRole.Secondary, 40m));
            }

            var salesTeamDetails = SalesTeamDetails.Create(members);
            var salesTeamLine = SalesTeamLine.Create(packageId: package.Id, details: salesTeamDetails);
            package.AddLine(salesTeamLine);
            package.ClearDomainEvents();
        }

        if (includeProjectCost)
        {
            var pcDetails = ProjectCostDetails.Create(categoryId: 5, itemId: 1, itemDescription: "Test PC");
            var pcLine = ProjectCostLine.Create(
                packageId: package.Id,
                salePrice: projectCostEstimate,
                estimatedCost: projectCostEstimate,
                retailSalePrice: projectCostEstimate,
                responsibility: Responsibility.Seller,
                shouldExcludeFromPricing: false,
                details: pcDetails);
            package.AddLine(pcLine);
            package.ClearDomainEvents();
        }

        if (includeTradeIn)
        {
            var tradeDetails = TradeInDetails.Create(
                tradeType: "Auto", year: 2020, make: "Clayton", model: "TruMH",
                tradeAllowance: 25000m, payoffAmount: tradeInPayoff, bookInAmount: 18000m);
            var tradeLine = TradeInLine.Create(
                packageId: package.Id,
                salePrice: 25000m,
                estimatedCost: 0m,
                retailSalePrice: 25000m,
                responsibility: Responsibility.Buyer,
                details: tradeDetails,
                sortOrder: 0);
            package.AddLine(tradeLine);
            package.ClearDomainEvents();
        }

        if (includeLandProjectCost)
        {
            var landPcDetails = ProjectCostDetails.Create(categoryId: 3, itemId: 1, itemDescription: "Land Payoff");
            var landPcLine = ProjectCostLine.Create(
                packageId: package.Id,
                salePrice: landProjectCostEstimate,
                estimatedCost: landProjectCostEstimate,
                retailSalePrice: landProjectCostEstimate,
                responsibility: Responsibility.Seller,
                shouldExcludeFromPricing: false,
                details: landPcDetails);
            package.AddLine(landPcLine);
            package.ClearDomainEvents();
        }

        // Set up navigation properties via reflection (normally populated by EF Core Include)
        SetProperty(package, nameof(Package.Sale), sale);

        var packagesField = typeof(Sale).GetField("_packages", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var packages = (List<Package>)packagesField.GetValue(sale)!;
        packages.Add(package);

        return package;
    }

    private static AuthorizedUserCache CreateAuthorizedUser(int id, int employeeNumber) => new()
    {
        Id = id,
        RefUserId = id,
        FederatedId = $"fed-{id}",
        EmployeeNumber = employeeNumber,
        FirstName = "Test",
        LastName = "User",
        DisplayName = "Test User",
        IsActive = true,
        IsRetired = false,
        AuthorizedHomeCenters = [42],
        LastSyncedAtUtc = DateTime.UtcNow
    };

    private static FundingRequestCache CreateFundingCache(int appId) => new()
    {
        Id = 1,
        RefFundingRequestId = 100,
        SaleId = 0,
        PackageId = 0,
        FundingKeys = JsonDocument.Parse($$"""[{"Key":"AppId","Value":"{{appId}}"}]"""),
        LenderId = 1,
        LenderName = "Test Lender",
        RequestAmount = 85000m,
        Status = FundingRequestStatus.Approved,
        LastSyncedAtUtc = DateTime.UtcNow
    };

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!;
            prop.SetValue(obj, value);
        }
    }
}

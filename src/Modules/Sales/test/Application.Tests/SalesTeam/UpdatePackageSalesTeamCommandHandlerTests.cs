using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Modules.Sales.Domain;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.SalesTeam;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.SalesTeam;

public sealed class UpdatePackageSalesTeamCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IAuthorizedUserCacheRepository _authorizedUserCacheRepository = Substitute.For<IAuthorizedUserCacheRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageSalesTeamCommandHandler _sut;

    public UpdatePackageSalesTeamCommandHandlerTests() =>
        _sut = new UpdatePackageSalesTeamCommandHandler(
            _packageRepository, _authorizedUserCacheRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new UpdatePackageSalesTeamCommand(publicId, [new(1, SalesTeamRole.Primary, 100m)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_authorized_user_ids_invalid()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _authorizedUserCacheRepository.AllExistAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.Handle(
            new UpdatePackageSalesTeamCommand(package.PublicId, [new(999, SalesTeamRole.Primary, 100m)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.InvalidAuthorizedUsers", result.Error.Code);
    }

    [Fact]
    public async Task Creates_sales_team_line_when_none_exists()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _authorizedUserCacheRepository.AllExistAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.Handle(
            new UpdatePackageSalesTeamCommand(package.PublicId,
            [
                new(1, SalesTeamRole.Primary, 60m),
                new(2, SalesTeamRole.Secondary, 40m)
            ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var salesTeam = Assert.Single(package.Lines.OfType<SalesTeamLine>());
        Assert.NotNull(salesTeam.Details);
        Assert.Equal(2, salesTeam.Details!.SalesTeamMembers.Count);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Replaces_existing_sales_team_line()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var existingDetails = SalesTeamDetails.Create(
        [
            SalesTeamMember.Create(10, SalesTeamRole.Primary, 100m)
        ]);
        package.AddLine(SalesTeamLine.Create(package.Id, existingDetails));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _authorizedUserCacheRepository.AllExistAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.Handle(
            new UpdatePackageSalesTeamCommand(package.PublicId,
            [
                new(20, SalesTeamRole.Primary, 70m),
                new(30, SalesTeamRole.Secondary, 30m)
            ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var salesTeam = Assert.Single(package.Lines.OfType<SalesTeamLine>());
        Assert.Equal(2, salesTeam.Details!.SalesTeamMembers.Count);
        Assert.Equal(20, salesTeam.Details.SalesTeamMembers[0].AuthorizedUserId);
        Assert.Equal(30, salesTeam.Details.SalesTeamMembers[1].AuthorizedUserId);
    }

    [Fact]
    public async Task Sales_team_details_contain_correct_member_data()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _authorizedUserCacheRepository.AllExistAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.Handle(
            new UpdatePackageSalesTeamCommand(package.PublicId,
            [
                new(5, SalesTeamRole.Primary, 55.5m)
            ]),
            CancellationToken.None);

        var salesTeam = Assert.Single(package.Lines.OfType<SalesTeamLine>());
        var member = Assert.Single(salesTeam.Details!.SalesTeamMembers);
        Assert.Equal(5, member.AuthorizedUserId);
        Assert.Equal(SalesTeamRole.Primary, member.Role);
        Assert.Equal(55.5m, member.CommissionSplitPercentage);
        Assert.Equal(0m, member.CommissionAmount);
    }

    [Fact]
    public async Task Sales_team_does_not_affect_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var gpBefore = package.GrossProfit;
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _authorizedUserCacheRepository.AllExistAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.Handle(
            new UpdatePackageSalesTeamCommand(package.PublicId,
            [
                new(1, SalesTeamRole.Primary, 100m)
            ]),
            CancellationToken.None);

        Assert.Equal(gpBefore, package.GrossProfit);
    }
}

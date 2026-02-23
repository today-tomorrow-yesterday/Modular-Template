using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class GetPackageByIdQueryHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IFundingRequestCacheRepository _fundingRepository = Substitute.For<IFundingRequestCacheRepository>();
    private readonly IAuthorizedUserCacheRepository _authorizedUserRepository = Substitute.For<IAuthorizedUserCacheRepository>();
    private readonly GetPackageByIdQueryHandler _sut;

    public GetPackageByIdQueryHandlerTests() =>
        _sut = new GetPackageByIdQueryHandler(_packageRepository, _fundingRepository, _authorizedUserRepository);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new GetPackageByIdQuery(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_when_package_found()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new GetPackageByIdQuery(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Response_contains_package_public_id_and_name()
    {
        var package = Package.Create(saleId: 1, name: "Package B", isPrimary: false);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new GetPackageByIdQuery(package.PublicId), CancellationToken.None);

        Assert.Equal(package.PublicId, result.Value.Id);
        Assert.Equal("Package B", result.Value.Name);
    }

    [Fact]
    public async Task Calls_repository_with_correct_public_id()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        await _sut.Handle(new GetPackageByIdQuery(publicId), CancellationToken.None);

        await _packageRepository.Received(1)
            .GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Queries_funding_cache_when_package_found()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        await _sut.Handle(new GetPackageByIdQuery(package.PublicId), CancellationToken.None);

        await _fundingRepository.Received(1)
            .GetByPackageIdAsync(package.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_query_funding_cache_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        await _sut.Handle(new GetPackageByIdQuery(publicId), CancellationToken.None);

        await _fundingRepository.DidNotReceive()
            .GetByPackageIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Response_has_empty_funding_request_ids_when_no_funding()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _fundingRepository.GetByPackageIdAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns((FundingRequestCache?)null);

        var result = await _sut.Handle(
            new GetPackageByIdQuery(package.PublicId), CancellationToken.None);

        Assert.Empty(result.Value.FundingRequestIds);
        Assert.Null(result.Value.Funding);
    }
}

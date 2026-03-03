using Modules.Sales.Application.Sales.CreateSale;
using Modules.Sales.Domain;
using Modules.Sales.Domain.PartiesCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Sales;

public sealed class CreateSaleCommandHandlerTests
{
    private readonly IPartyCacheRepository _partyCacheRepository = Substitute.For<IPartyCacheRepository>();
    private readonly IRetailLocationRepository _retailLocationRepository = Substitute.For<IRetailLocationRepository>();
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly ISaleNumberGenerator _saleNumberGenerator = Substitute.For<ISaleNumberGenerator>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly CreateSaleCommandHandler _sut;

    private static readonly Guid TestPartyPublicId = Guid.NewGuid();
    private const int TestHomeCenterNumber = 42;
    private const int TestSaleNumber = 100001;

    public CreateSaleCommandHandlerTests()
    {
        _sut = new CreateSaleCommandHandler(
            _partyCacheRepository,
            _retailLocationRepository,
            _saleRepository,
            _saleNumberGenerator,
            _unitOfWork);

        _saleNumberGenerator.GenerateNextAsync(Arg.Any<CancellationToken>())
            .Returns(TestSaleNumber);
    }

    [Fact]
    public async Task Returns_failure_when_party_not_found()
    {
        _partyCacheRepository.GetByRefPublicIdAsync(TestPartyPublicId, Arg.Any<CancellationToken>())
            .Returns((PartyCache?)null);

        var result = await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Party.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_retail_location_not_found()
    {
        SetupPartyCache();
        _retailLocationRepository.GetByHomeCenterNumberAsync(TestHomeCenterNumber, Arg.Any<CancellationToken>())
            .Returns((RetailLocation?)null);

        var result = await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RetailLocation.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_retail_location_is_inactive()
    {
        SetupPartyCache();
        SetupRetailLocation(isActive: false);

        var result = await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RetailLocation.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task Successful_creation_returns_public_id_and_sale_number()
    {
        SetupPartyCache();
        SetupRetailLocation(isActive: true);

        var result = await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PublicId);
        Assert.Equal(TestSaleNumber, result.Value.SaleNumber);
    }

    [Fact]
    public async Task Successful_creation_calls_repository_add()
    {
        SetupPartyCache();
        SetupRetailLocation(isActive: true);

        await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        _saleRepository.Received(1).Add(Arg.Any<Sale>());
    }

    [Fact]
    public async Task Successful_creation_calls_save_changes()
    {
        SetupPartyCache();
        SetupRetailLocation(isActive: true);

        await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sale_has_correct_party_id_and_retail_location_id()
    {
        SetupPartyCache(partyId: 7);
        var retailLocation = SetupRetailLocation(isActive: true);

        await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        _saleRepository.Received(1).Add(Arg.Is<Sale>(s =>
            s.PartyId == 7 &&
            s.RetailLocationId == retailLocation.Id &&
            s.SaleType == SaleType.B2C &&
            s.SaleNumber == TestSaleNumber));
    }

    [Fact]
    public async Task Sale_has_correct_sale_type_when_specified()
    {
        SetupPartyCache();
        SetupRetailLocation(isActive: true);

        await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber, SaleType.B2B), CancellationToken.None);

        _saleRepository.Received(1).Add(Arg.Is<Sale>(s => s.SaleType == SaleType.B2B));
    }

    [Fact]
    public async Task Does_not_call_save_changes_when_party_not_found()
    {
        _partyCacheRepository.GetByRefPublicIdAsync(TestPartyPublicId, Arg.Any<CancellationToken>())
            .Returns((PartyCache?)null);

        await _sut.Handle(
            new CreateSaleCommand(TestPartyPublicId, TestHomeCenterNumber), CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- Test helpers ---

    private void SetupPartyCache(int partyId = 1)
    {
        var party = new PartyCache
        {
            Id = partyId,
            RefPublicId = TestPartyPublicId,
            DisplayName = "Test Customer"
        };

        _partyCacheRepository.GetByRefPublicIdAsync(TestPartyPublicId, Arg.Any<CancellationToken>())
            .Returns(party);
    }

    private RetailLocation SetupRetailLocation(bool isActive)
    {
        var retailLocation = RetailLocation.CreateHomeCenter(
            TestHomeCenterNumber, "Test HC", "OH", "43004", isActive);

        _retailLocationRepository.GetByHomeCenterNumberAsync(TestHomeCenterNumber, Arg.Any<CancellationToken>())
            .Returns(retailLocation);

        return retailLocation;
    }
}

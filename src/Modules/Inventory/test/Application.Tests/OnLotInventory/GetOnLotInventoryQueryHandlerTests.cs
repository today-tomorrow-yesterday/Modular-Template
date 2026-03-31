using Modules.Inventory.Application.OnLotInventory.GetOnLotInventory;
using Modules.Inventory.Domain.AncillaryData;
using Modules.Inventory.Domain.LandCosts;
using Modules.Inventory.Domain.OnLotHomes;
using NSubstitute;
using Xunit;

namespace Modules.Inventory.Application.Tests.OnLotInventory;

public sealed class GetOnLotInventoryQueryHandlerTests
{
    private readonly IOnLotHomeRepository _onLotHomeRepo = Substitute.For<IOnLotHomeRepository>();
    private readonly ILandCostRepository _landCostRepo = Substitute.For<ILandCostRepository>();
    private readonly IAncillaryDataRepository _ancillaryRepo = Substitute.For<IAncillaryDataRepository>();
    private readonly GetOnLotInventoryQueryHandler _sut;

    public GetOnLotInventoryQueryHandlerTests()
    {
        _sut = new GetOnLotInventoryQueryHandler(
            _onLotHomeRepo, _landCostRepo, _ancillaryRepo);
    }

    [Fact]
    public async Task Returns_empty_when_no_homes_found()
    {
        _onLotHomeRepo.GetByHomeCenterNumberAsync(100, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OnLotHome>());

        var result = await _sut.Handle(new GetOnLotInventoryQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Returns_joined_data_when_homes_exist()
    {
        var home = OnLotHome.Create(
            1, 100, "STK-1", "NEW", "Good", "DW", 28m, 60m,
            3, 2, 2024, "Model-A", "MakeX", "Fac-1", "SN-001",
            100_000m, 5_000m, 120_000m, 115_000m, "2024-06-01", null, DateTime.UtcNow);

        _onLotHomeRepo.GetByHomeCenterNumberAsync(100, Arg.Any<CancellationToken>())
            .Returns(new[] { home });

        var landCost = new LandCost { RefHomeCenterNumber = 100, RefStockNumber = "STK-1", AddToTotal = 10_000m, FurnitureTotal = 2_000m };
        _landCostRepo.GetByHomeCenterAndStockNumbersAsync(100, Arg.Any<IReadOnlySet<string>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { landCost });

        _ancillaryRepo.GetByHomeCenterAndStockNumbersAsync(100, Arg.Any<IReadOnlySet<string>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AncillaryData>());

        var result = await _sut.Handle(new GetOnLotInventoryQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var response = result.Value.First();
        Assert.NotNull(response.LandCosts);
        Assert.Equal(10_000m, response.LandCosts!.AddToTotal);
        Assert.Null(response.AncillaryData);
    }
}

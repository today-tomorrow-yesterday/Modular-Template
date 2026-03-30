using Modules.Inventory.Application.LandInventory.GetLandInventory;
using Modules.Inventory.Domain.LandParcels;
using NSubstitute;
using Xunit;

namespace Modules.Inventory.Application.Tests.LandInventory;

public sealed class GetLandInventoryQueryHandlerTests
{
    private readonly ILandParcelRepository _repository = Substitute.For<ILandParcelRepository>();
    private readonly GetLandInventoryQueryHandler _sut;

    public GetLandInventoryQueryHandlerTests()
    {
        _sut = new GetLandInventoryQueryHandler(_repository);
    }

    [Fact]
    public async Task Returns_empty_when_no_parcels_exist()
    {
        _repository.GetByHomeCenterNumberAsync(100, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<LandParcel>());

        var result = await _sut.Handle(new GetLandInventoryQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Filters_to_allowed_stock_types_only()
    {
        var allowed = LandParcel.Create(1, 100, "STK-1", "CR2SP", null, null, null, null,
            null, null, null, null, null, null, null, null, null, DateTime.UtcNow);
        var excluded = LandParcel.Create(2, 100, "STK-2", "UNKNOWN", null, null, null, null,
            null, null, null, null, null, null, null, null, null, DateTime.UtcNow);
        var nullType = LandParcel.Create(3, 100, "STK-3", null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, DateTime.UtcNow);

        _repository.GetByHomeCenterNumberAsync(100, Arg.Any<CancellationToken>())
            .Returns(new[] { allowed, excluded, nullType });

        var result = await _sut.Handle(new GetLandInventoryQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(allowed.PublicId, result.Value.First().PublicId);
    }
}

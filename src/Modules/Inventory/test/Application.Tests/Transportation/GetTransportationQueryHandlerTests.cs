using Modules.Inventory.Application.Transportation.GetTransportation;
using Modules.Inventory.Domain.OnLotHomes;
using Modules.Inventory.Domain.WheelsAndAxles;
using NSubstitute;
using Xunit;

namespace Modules.Inventory.Application.Tests.Transportation;

public sealed class GetTransportationQueryHandlerTests
{
    private readonly IOnLotHomeRepository _onLotHomeRepo = Substitute.For<IOnLotHomeRepository>();
    private readonly IWheelsAndAxlesTransactionRepository _waRepo = Substitute.For<IWheelsAndAxlesTransactionRepository>();
    private readonly GetTransportationQueryHandler _sut;

    public GetTransportationQueryHandlerTests()
    {
        _sut = new GetTransportationQueryHandler(_onLotHomeRepo, _waRepo);
    }

    [Fact]
    public async Task Returns_failure_when_no_matching_homes()
    {
        _onLotHomeRepo.GetByDimensionsAsync(60m, 28m, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OnLotHome>());

        var result = await _sut.Handle(new GetTransportationQuery(60m, 28m), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Returns_failure_when_no_wheels_and_axles_transaction()
    {
        var home = OnLotHome.Create(
            1, 100, "STK-1", "NEW", "Good", "DW", 28m, 60m,
            3, 2, 2024, "Model-A", "MakeX", "Fac-1", "SN-001",
            100_000m, 5_000m, 120_000m, 115_000m, "2024-06-01", null, DateTime.UtcNow);

        _onLotHomeRepo.GetByDimensionsAsync(60m, 28m, Arg.Any<CancellationToken>())
            .Returns(new[] { home });

        _waRepo.GetLatestByStockNumbersAsync(Arg.Any<IReadOnlySet<string>>(), Arg.Any<CancellationToken>())
            .Returns((WheelsAndAxlesTransaction?)null);

        var result = await _sut.Handle(new GetTransportationQuery(60m, 28m), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Returns_success_with_axle_and_wheel_counts()
    {
        var home = OnLotHome.Create(
            1, 100, "STK-1", "NEW", "Good", "DW", 28m, 60m,
            3, 2, 2024, "Model-A", "MakeX", "Fac-1", "SN-001",
            100_000m, 5_000m, 120_000m, 115_000m, "2024-06-01", null, DateTime.UtcNow);

        _onLotHomeRepo.GetByDimensionsAsync(60m, 28m, Arg.Any<CancellationToken>())
            .Returns(new[] { home });

        var transaction = new WheelsAndAxlesTransaction
        {
            Id = 1,
            BrakeAxles = 2,
            IdlerAxles = 1,
            Wheels = 6
        };
        _waRepo.GetLatestByStockNumbersAsync(Arg.Any<IReadOnlySet<string>>(), Arg.Any<CancellationToken>())
            .Returns(transaction);

        var result = await _sut.Handle(new GetTransportationQuery(60m, 28m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.NumberOfAxles);
        Assert.Equal(6, result.Value.NumberOfWheels);
    }
}

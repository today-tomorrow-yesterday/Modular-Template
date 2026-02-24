using Modules.Inventory.Application.OnLotInventory.SyncOnLotHomeFromCdc;
using Modules.Inventory.Domain;
using Modules.Inventory.Domain.OnLotHomes;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Inventory.Application.Tests.OnLotInventory;

public sealed class SyncOnLotHomeFromCdcCommandHandlerTests
{
    private readonly IOnLotHomeRepository _repository = Substitute.For<IOnLotHomeRepository>();
    private readonly IUnitOfWork<IInventoryModule> _unitOfWork = Substitute.For<IUnitOfWork<IInventoryModule>>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly SyncOnLotHomeFromCdcCommandHandler _sut;

    public SyncOnLotHomeFromCdcCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new SyncOnLotHomeFromCdcCommandHandler(_repository, _unitOfWork, _dateTimeProvider);
    }

    private static SyncOnLotHomeFromCdcCommand CreateCommand(int id = 1) =>
        new(id, 100, "STK-1", "NEW", "Good", "DW", 28m, 60m,
            3, 2, 2024, "Model-A", "MakeX", "Fac-1", "SN-001",
            100_000m, 5_000m, 120_000m, 115_000m, "2024-06-01", null);

    [Fact]
    public async Task Creates_new_home_when_not_existing()
    {
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((OnLotHome?)null);

        var result = await _sut.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Add(Arg.Any<OnLotHome>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updates_existing_home_when_found()
    {
        var existing = OnLotHome.Create(
            1, 100, "STK-1", "USED", "Fair", "SW", 16m, 50m,
            2, 1, 2020, "Old-Model", "OldMake", "Fac-2", "SN-002",
            80_000m, 2_000m, 90_000m, 85_000m, "2020-01-01", null, DateTime.UtcNow);

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.DidNotReceiveWithAnyArgs().Add(default!);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

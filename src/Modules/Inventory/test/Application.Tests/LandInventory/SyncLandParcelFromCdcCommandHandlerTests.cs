using Modules.Inventory.Application.LandInventory.SyncLandParcelFromCdc;
using Modules.Inventory.Domain;
using Modules.Inventory.Domain.LandParcels;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Inventory.Application.Tests.LandInventory;

public sealed class SyncLandParcelFromCdcCommandHandlerTests
{
    private readonly ILandParcelRepository _repository = Substitute.For<ILandParcelRepository>();
    private readonly IUnitOfWork<IInventoryModule> _unitOfWork = Substitute.For<IUnitOfWork<IInventoryModule>>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly SyncLandParcelFromCdcCommandHandler _sut;

    public SyncLandParcelFromCdcCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new SyncLandParcelFromCdcCommandHandler(_repository, _unitOfWork, _dateTimeProvider);
    }

    private static SyncLandParcelFromCdcCommand CreateCommand(int id = 1) =>
        new(id, 100, "STK-1", "CR2SP", "5", 50_000m, 10_000m, 75_000m,
            "MAP-1", "123 Main", null, "Tulsa", "OK", "74101", "Tulsa", "LN-1", "HS-1");

    [Fact]
    public async Task Creates_new_parcel_when_not_existing()
    {
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((LandParcel?)null);

        var result = await _sut.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Add(Arg.Any<LandParcel>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updates_existing_parcel_when_found()
    {
        var existing = LandParcel.Create(
            1, 100, "STK-1", "OL2SP", "3", 40_000m, 8_000m, 60_000m,
            null, null, null, null, null, null, null, null, null, DateTime.UtcNow);

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.DidNotReceiveWithAnyArgs().Add(default!);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

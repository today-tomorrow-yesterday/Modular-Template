using Modules.Inventory.Application.LandInventory.RemoveLandParcelFromCdc;
using Modules.Inventory.Domain;
using Modules.Inventory.Domain.LandParcels;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Inventory.Application.Tests.LandInventory;

public sealed class RemoveLandParcelFromCdcCommandHandlerTests
{
    private readonly ILandParcelRepository _repository = Substitute.For<ILandParcelRepository>();
    private readonly IUnitOfWork<IInventoryModule> _unitOfWork = Substitute.For<IUnitOfWork<IInventoryModule>>();
    private readonly RemoveLandParcelFromCdcCommandHandler _sut;

    public RemoveLandParcelFromCdcCommandHandlerTests()
    {
        _sut = new RemoveLandParcelFromCdcCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Returns_success_when_parcel_not_found()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns((LandParcel?)null);

        var result = await _sut.Handle(new RemoveLandParcelFromCdcCommand(99), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.DidNotReceiveWithAnyArgs().Remove(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Removes_parcel_and_saves_when_found()
    {
        var parcel = LandParcel.Create(
            1, 100, "STK-1", "CR2SP", null, null, null, null,
            null, null, null, null, null, null, null, null, null, DateTime.UtcNow);

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(parcel);

        var result = await _sut.Handle(new RemoveLandParcelFromCdcCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Remove(parcel);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

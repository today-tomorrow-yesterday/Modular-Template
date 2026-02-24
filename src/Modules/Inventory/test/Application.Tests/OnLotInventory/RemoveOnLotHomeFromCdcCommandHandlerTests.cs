using Modules.Inventory.Application.OnLotInventory.RemoveOnLotHomeFromCdc;
using Modules.Inventory.Domain;
using Modules.Inventory.Domain.OnLotHomes;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Inventory.Application.Tests.OnLotInventory;

public sealed class RemoveOnLotHomeFromCdcCommandHandlerTests
{
    private readonly IOnLotHomeRepository _repository = Substitute.For<IOnLotHomeRepository>();
    private readonly IUnitOfWork<IInventoryModule> _unitOfWork = Substitute.For<IUnitOfWork<IInventoryModule>>();
    private readonly RemoveOnLotHomeFromCdcCommandHandler _sut;

    public RemoveOnLotHomeFromCdcCommandHandlerTests()
    {
        _sut = new RemoveOnLotHomeFromCdcCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Returns_success_when_home_not_found()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns((OnLotHome?)null);

        var result = await _sut.Handle(new RemoveOnLotHomeFromCdcCommand(99), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.DidNotReceiveWithAnyArgs().Remove(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Removes_home_and_saves_when_found()
    {
        var home = OnLotHome.Create(
            1, 100, "STK-1", "NEW", "Good", "DW", 28m, 60m,
            3, 2, 2024, "Model-A", "MakeX", "Fac-1", "SN-001",
            100_000m, 5_000m, 120_000m, 115_000m, "2024-06-01", null, DateTime.UtcNow);

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(home);

        var result = await _sut.Handle(new RemoveOnLotHomeFromCdcCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Remove(home);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

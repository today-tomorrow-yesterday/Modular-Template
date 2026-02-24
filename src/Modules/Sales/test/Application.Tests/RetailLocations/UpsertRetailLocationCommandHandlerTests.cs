using Modules.Sales.Application.RetailLocations.UpsertRetailLocation;
using Modules.Sales.Domain;
using Modules.Sales.Domain.RetailLocations;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.RetailLocations;

public sealed class UpsertRetailLocationCommandHandlerTests
{
    private readonly IRetailLocationRepository _retailLocationRepository = Substitute.For<IRetailLocationRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpsertRetailLocationCommandHandler _sut;

    public UpsertRetailLocationCommandHandlerTests() =>
        _sut = new UpsertRetailLocationCommandHandler(_retailLocationRepository, _unitOfWork);

    [Fact]
    public async Task Creates_new_retail_location_when_not_found()
    {
        _retailLocationRepository.GetByHomeCenterNumberAsync(42, Arg.Any<CancellationToken>())
            .Returns((RetailLocation?)null);

        var result = await _sut.Handle(
            new UpsertRetailLocationCommand(42, "Test HC", "OH", "43004", true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _retailLocationRepository.Received(1).Add(Arg.Any<RetailLocation>());
    }

    [Fact]
    public async Task Updates_existing_retail_location_when_found()
    {
        var existing = RetailLocation.CreateHomeCenter(42, "Old Name", "OH", "43004", true);
        _retailLocationRepository.GetByHomeCenterNumberAsync(42, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Handle(
            new UpsertRetailLocationCommand(42, "New Name", "TX", "75001", false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _retailLocationRepository.DidNotReceive().Add(Arg.Any<RetailLocation>());
        Assert.Equal("New Name", existing.Name);
    }
}

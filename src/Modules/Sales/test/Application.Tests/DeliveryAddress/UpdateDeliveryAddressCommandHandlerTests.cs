using System.Reflection;
using Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.DeliveryAddresses;

public sealed class UpdateDeliveryAddressCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdateDeliveryAddressCommandHandler _sut;

    public UpdateDeliveryAddressCommandHandlerTests() =>
        _sut = new UpdateDeliveryAddressCommandHandler(_saleRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            CreateCommand(publicId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_delivery_address_not_found()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();
        // Sale has no DeliveryAddress (null by default)

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            CreateCommand(sale.PublicId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DeliveryAddress.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Successful_update_returns_success()
    {
        var sale = CreateSaleWithDeliveryAddress();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            CreateCommand(sale.PublicId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Calls_save_changes()
    {
        var sale = CreateSaleWithDeliveryAddress();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        await _sut.Handle(
            CreateCommand(sale.PublicId),
            CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UpdateDeliveryAddressCommand CreateCommand(Guid salePublicId) =>
        new(salePublicId, "Secondary Residence", false,
            "999 New St", "Dublin", "Franklin", "OH", "43017");

    private static Sale CreateSaleWithDeliveryAddress()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        var address = Domain.DeliveryAddresses.DeliveryAddress.Create(
            sale.Id, "Primary Residence", true, null, null, "123 Main", null, null, "Columbus", "Franklin", "OH", "US", "43004");
        address.ClearDomainEvents();
        SetProperty(sale, nameof(Sale.DeliveryAddress), address);

        return sale;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.SetValue(obj, value);
        }
    }
}

using Modules.Sales.Application.DeliveryAddresses.CreateDeliveryAddress;
using Modules.Sales.Domain;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.DeliveryAddress;

public sealed class CreateDeliveryAddressCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IDeliveryAddressRepository _deliveryAddressRepository = Substitute.For<IDeliveryAddressRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly CreateDeliveryAddressCommandHandler _sut;

    public CreateDeliveryAddressCommandHandlerTests() =>
        _sut = new CreateDeliveryAddressCommandHandler(_saleRepository, _deliveryAddressRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new CreateDeliveryAddressCommand(publicId, "Primary Residence", true, "123 Main", "Columbus", "Franklin", "OH", "43004"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_conflict_when_delivery_address_already_exists()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        var existingAddress = Domain.DeliveryAddresses.DeliveryAddress.Create(
            sale.Id, "Primary Residence", true, null, null, "456 Oak", null, null, "Dublin", "Franklin", "OH", "US", "43017");
        existingAddress.ClearDomainEvents();
        SetProperty(sale, nameof(Sale.DeliveryAddress), existingAddress);

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new CreateDeliveryAddressCommand(sale.PublicId, "Primary Residence", true, "123 Main", "Columbus", "Franklin", "OH", "43004"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DeliveryAddress.AlreadyExists", result.Error.Code);
    }

    [Fact]
    public async Task Successful_creation_returns_public_id()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new CreateDeliveryAddressCommand(sale.PublicId, "Primary Residence", true, "123 Main", "Columbus", "Franklin", "OH", "43004"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PublicId);
    }

    [Fact]
    public async Task Calls_delivery_address_repository_add()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        await _sut.Handle(
            new CreateDeliveryAddressCommand(sale.PublicId, "Primary Residence", true, "123 Main", "Columbus", "Franklin", "OH", "43004"),
            CancellationToken.None);

        _deliveryAddressRepository.Received(1).Add(Arg.Any<Domain.DeliveryAddresses.DeliveryAddress>());
    }

    [Fact]
    public async Task Calls_save_changes()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        await _sut.Handle(
            new CreateDeliveryAddressCommand(sale.PublicId, "Primary Residence", true, "123 Main", "Columbus", "Franklin", "OH", "43004"),
            CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Created_address_has_correct_sale_id_and_fields()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        Domain.DeliveryAddresses.DeliveryAddress? capturedAddress = null;
        _deliveryAddressRepository.When(r => r.Add(Arg.Any<Domain.DeliveryAddresses.DeliveryAddress>()))
            .Do(ci => capturedAddress = ci.Arg<Domain.DeliveryAddresses.DeliveryAddress>());

        await _sut.Handle(
            new CreateDeliveryAddressCommand(sale.PublicId, "Rental", false, "789 Elm", "Westerville", "Delaware", "OH", "43081"),
            CancellationToken.None);

        Assert.NotNull(capturedAddress);
        Assert.Equal(sale.Id, capturedAddress.SaleId);
        Assert.Equal("Rental", capturedAddress.OccupancyType);
        Assert.False(capturedAddress.IsWithinCityLimits);
        Assert.Equal("789 Elm", capturedAddress.AddressLine1);
        Assert.Equal("Westerville", capturedAddress.City);
        Assert.Equal("Delaware", capturedAddress.County);
        Assert.Equal("OH", capturedAddress.State);
        Assert.Equal("43081", capturedAddress.PostalCode);
        Assert.Null(capturedAddress.AddressStyle);
        Assert.Null(capturedAddress.AddressType);
        Assert.Null(capturedAddress.AddressLine2);
        Assert.Null(capturedAddress.AddressLine3);
        Assert.Null(capturedAddress.Country);
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

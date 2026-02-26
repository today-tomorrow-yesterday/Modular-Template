using Modules.Sales.Application.DeliveryAddresses.GetDeliveryAddress;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.DeliveryAddress;

public sealed class GetDeliveryAddressQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly GetDeliveryAddressQueryHandler _sut;

    public GetDeliveryAddressQueryHandlerTests() =>
        _sut = new GetDeliveryAddressQueryHandler(_saleRepository);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetDeliveryAddressQuery(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_delivery_address_is_null()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 100);
        sale.ClearDomainEvents();
        // DeliveryAddress is null by default

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new GetDeliveryAddressQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DeliveryAddress.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_when_delivery_address_exists()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 100);
        sale.ClearDomainEvents();

        var address = Domain.DeliveryAddresses.DeliveryAddress.Create(
            sale.Id, "Primary Residence", true, null, null,
            "123 Main St", null, null, "Columbus", "Franklin", "OH", "US", "43004");
        address.ClearDomainEvents();
        SetProperty(sale, nameof(Sale.DeliveryAddress), address);

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new GetDeliveryAddressQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St", result.Value.AddressLine1);
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(obj, value);
        }
    }
}

using Modules.SampleOrders.Application.Orders.PlaceOrder;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.ProductsCache;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.SampleOrders.Application.Tests.Orders;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IProductCacheRepository _productCacheRepository = Substitute.For<IProductCacheRepository>();
    private readonly IUnitOfWork<ISampleOrdersModule> _unitOfWork = Substitute.For<IUnitOfWork<ISampleOrdersModule>>();
    private readonly PlaceOrderCommandHandler _sut;

    public PlaceOrderCommandHandlerTests()
    {
        _sut = new PlaceOrderCommandHandler(_orderRepository, _productCacheRepository, _unitOfWork);
    }

    private static ProductCache CreateActiveProduct(int id = 1, decimal price = 29.99m)
    {
        return new ProductCache
        {
            Id = id,
            RefPublicId = Guid.NewGuid(),
            Name = "Test Product",
            Price = price,
            IsActive = true,
            LastSyncedAtUtc = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Returns_PublicId_on_success()
    {
        // Arrange
        var product = CreateActiveProduct();
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 1, Quantity: 2);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Adds_order_to_repository_on_success()
    {
        // Arrange
        var product = CreateActiveProduct();
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 1, Quantity: 3);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _orderRepository.Received(1).Add(Arg.Is<Order>(o =>
            o.CustomerId == 1 &&
            o.Lines.Count == 1));
    }

    [Fact]
    public async Task Calls_SaveChangesAsync_on_success()
    {
        // Arrange
        var product = CreateActiveProduct();
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 1, Quantity: 1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_when_product_not_found()
    {
        // Arrange
        _productCacheRepository
            .GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((ProductCache?)null);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 999, Quantity: 1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.ProductNotFound, result.Error);
    }

    [Fact]
    public async Task Returns_failure_when_product_is_inactive()
    {
        // Arrange
        var product = CreateActiveProduct();
        product.IsActive = false;
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 1, Quantity: 1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.ProductNotFound, result.Error);
    }

    [Fact]
    public async Task Returns_failure_when_customerId_is_invalid()
    {
        // Arrange
        var product = CreateActiveProduct();
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new PlaceOrderCommand(CustomerId: 0, ProductCacheId: 1, Quantity: 1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.CustomerRequired, result.Error);
    }

    [Fact]
    public async Task Does_not_add_order_when_product_not_found()
    {
        // Arrange
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((ProductCache?)null);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 1, Quantity: 1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _orderRepository.DidNotReceive().Add(Arg.Any<Order>());
    }

    [Fact]
    public async Task Does_not_call_SaveChanges_when_product_not_found()
    {
        // Arrange
        _productCacheRepository
            .GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((ProductCache?)null);

        var command = new PlaceOrderCommand(CustomerId: 1, ProductCacheId: 1, Quantity: 1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

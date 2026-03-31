using Modules.SampleSales.Application.Products.CreateProduct;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Products;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.SampleSales.Application.Tests.Products;

public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork<ISampleSalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISampleSalesModule>>();
    private readonly CreateProductCommandHandler _sut;

    public CreateProductCommandHandlerTests()
    {
        _sut = new CreateProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Returns_PublicId_on_success()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", "A widget", 29.99m, 10.00m);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Adds_product_to_repository()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", null, 29.99m, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).Add(Arg.Is<Product>(p =>
            p.Name == "Widget" &&
            p.Price.Amount == 29.99m));
    }

    [Fact]
    public async Task Calls_SaveChangesAsync_on_success()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", null, 29.99m, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_when_domain_rejects_creation()
    {
        // Arrange — empty name will fail domain validation
        var command = new CreateProductCommand("", null, 29.99m, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameEmpty, result.Error);
    }

    [Fact]
    public async Task Does_not_add_to_repository_when_domain_rejects()
    {
        // Arrange
        var command = new CreateProductCommand("", null, 29.99m, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.DidNotReceive().Add(Arg.Any<Product>());
    }

    [Fact]
    public async Task Does_not_call_SaveChanges_when_domain_rejects()
    {
        // Arrange
        var command = new CreateProductCommand("", null, 29.99m, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

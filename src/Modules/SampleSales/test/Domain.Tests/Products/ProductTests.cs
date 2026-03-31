using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Domain.Products.Events;
using Xunit;

namespace Modules.SampleSales.Domain.Tests.Products;

public sealed class ProductTests
{
    // ─── Create ───────────────────────────────────────────────────

    [Fact]
    public void Create_returns_success_with_valid_inputs()
    {
        // Arrange & Act
        var result = Product.Create("Widget", "A test widget", 29.99m, 10.00m);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Widget", result.Value.Name);
        Assert.Equal("A test widget", result.Value.Description);
        Assert.Equal(29.99m, result.Value.Price.Amount);
        Assert.Equal(10.00m, result.Value.InternalCost);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public void Create_generates_PublicId()
    {
        // Arrange & Act
        var result = Product.Create("Widget", null, 10.00m, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PublicId);
    }

    [Fact]
    public void Create_raises_ProductCreatedDomainEvent()
    {
        // Arrange & Act
        var result = Product.Create("Widget", null, 10.00m, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(
            result.Value.DomainEvents,
            e => e is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Create_trims_name_and_description()
    {
        // Arrange & Act
        var result = Product.Create("  Widget  ", "  A description  ", 10.00m, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Widget", result.Value.Name);
        Assert.Equal("A description", result.Value.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_returns_failure_when_name_is_empty(string? name)
    {
        // Arrange & Act
        var result = Product.Create(name!, null, 10.00m, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameEmpty, result.Error);
    }

    [Fact]
    public void Create_returns_failure_when_name_exceeds_max_length()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act
        var result = Product.Create(longName, null, 10.00m, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameTooLong, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_returns_failure_when_price_is_not_positive(decimal price)
    {
        // Arrange & Act
        var result = Product.Create("Widget", null, price, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.PriceInvalid, result.Error);
    }

    [Fact]
    public void Create_accepts_null_description_and_internalCost()
    {
        // Arrange & Act
        var result = Product.Create("Widget", null, 10.00m, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
        Assert.Null(result.Value.InternalCost);
    }

    [Fact]
    public void Create_sets_currency_from_parameter()
    {
        // Arrange & Act
        var result = Product.Create("Widget", null, 10.00m, null, "EUR");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("EUR", result.Value.Price.Currency);
    }

    // ─── Update ───────────────────────────────────────────────────

    [Fact]
    public void Update_changes_properties_and_raises_event()
    {
        // Arrange
        var product = Product.Create("Widget", "Old desc", 10.00m, null).Value;
        product.ClearDomainEvents();

        // Act
        var result = product.Update("Updated Widget", "New desc", 20.00m, false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Widget", product.Name);
        Assert.Equal("New desc", product.Description);
        Assert.Equal(20.00m, product.Price.Amount);
        Assert.False(product.IsActive);
        Assert.Contains(
            product.DomainEvents,
            e => e is ProductUpdatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_returns_failure_when_name_is_empty(string? name)
    {
        // Arrange
        var product = Product.Create("Widget", null, 10.00m, null).Value;

        // Act
        var result = product.Update(name!, null, 10.00m, true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameEmpty, result.Error);
    }

    [Fact]
    public void Update_returns_failure_when_name_exceeds_max_length()
    {
        // Arrange
        var product = Product.Create("Widget", null, 10.00m, null).Value;
        var longName = new string('A', 201);

        // Act
        var result = product.Update(longName, null, 10.00m, true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameTooLong, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_returns_failure_when_price_is_not_positive(decimal price)
    {
        // Arrange
        var product = Product.Create("Widget", null, 10.00m, null).Value;

        // Act
        var result = product.Update("Widget", null, price, true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.PriceInvalid, result.Error);
    }
}

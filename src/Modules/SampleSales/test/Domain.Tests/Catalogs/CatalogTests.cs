using Modules.SampleSales.Domain.Catalogs;
using Modules.SampleSales.Domain.Catalogs.Events;
using Xunit;

namespace Modules.SampleSales.Domain.Tests.Catalogs;

public sealed class CatalogTests
{
    // ─── Create ───────────────────────────────────────────────────

    [Fact]
    public void Create_returns_success_with_valid_inputs()
    {
        // Arrange & Act
        var result = Catalog.Create("Summer Collection", "Summer 2026 products");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Summer Collection", result.Value.Name);
        Assert.Equal("Summer 2026 products", result.Value.Description);
        Assert.Empty(result.Value.Products);
    }

    [Fact]
    public void Create_generates_PublicId()
    {
        // Arrange & Act
        var result = Catalog.Create("Summer Collection", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PublicId);
    }

    [Fact]
    public void Create_raises_CatalogCreatedDomainEvent()
    {
        // Arrange & Act
        var result = Catalog.Create("Summer Collection", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(
            result.Value.DomainEvents,
            e => e is CatalogCreatedDomainEvent);
    }

    [Fact]
    public void Create_trims_name_and_description()
    {
        // Arrange & Act
        var result = Catalog.Create("  Summer  ", "  Description  ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Summer", result.Value.Name);
        Assert.Equal("Description", result.Value.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_returns_failure_when_name_is_empty(string? name)
    {
        // Arrange & Act
        var result = Catalog.Create(name!, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.NameEmpty, result.Error);
    }

    [Fact]
    public void Create_returns_failure_when_name_exceeds_max_length()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act
        var result = Catalog.Create(longName, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_accepts_null_description()
    {
        // Arrange & Act
        var result = Catalog.Create("Summer Collection", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }

    // ─── Update ───────────────────────────────────────────────────

    [Fact]
    public void Update_changes_properties_and_raises_event()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", "Old desc").Value;
        catalog.ClearDomainEvents();

        // Act
        var result = catalog.Update("Winter", "New desc");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Winter", catalog.Name);
        Assert.Equal("New desc", catalog.Description);
        Assert.Contains(
            catalog.DomainEvents,
            e => e is CatalogUpdatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_returns_failure_when_name_is_empty(string? name)
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;

        // Act
        var result = catalog.Update(name!, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.NameEmpty, result.Error);
    }

    [Fact]
    public void Update_returns_failure_when_name_exceeds_max_length()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;
        var longName = new string('A', 201);

        // Act
        var result = catalog.Update(longName, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.NameTooLong, result.Error);
    }

    // ─── AddProduct ──────────────────────────────────────────────

    [Fact]
    public void AddProduct_adds_product_and_raises_event()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;
        catalog.ClearDomainEvents();
        var publicProductId = Guid.CreateVersion7();

        // Act
        var result = catalog.AddProduct(1, publicProductId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(catalog.Products);
        Assert.Equal(1, catalog.Products.First().ProductId);
        Assert.Contains(
            catalog.DomainEvents,
            e => e is CatalogProductAddedDomainEvent added && added.PublicProductId == publicProductId);
    }

    [Fact]
    public void AddProduct_with_custom_price_sets_price()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;
        var customPrice = ModularTemplate.Domain.ValueObjects.Money.Create(19.99m, "USD").Value;
        var publicProductId = Guid.CreateVersion7();

        // Act
        var result = catalog.AddProduct(1, publicProductId, customPrice);

        // Assert
        Assert.True(result.IsSuccess);
        var product = Assert.Single(catalog.Products);
        Assert.NotNull(product.CustomPrice);
        Assert.Equal(19.99m, product.CustomPrice.Amount);
    }

    [Fact]
    public void AddProduct_returns_failure_when_product_already_in_catalog()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;
        catalog.AddProduct(1, Guid.CreateVersion7());

        // Act
        var result = catalog.AddProduct(1, Guid.CreateVersion7());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.ProductAlreadyInCatalog, result.Error);
    }

    [Fact]
    public void AddProduct_allows_multiple_different_products()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;

        // Act
        catalog.AddProduct(1, Guid.CreateVersion7());
        catalog.AddProduct(2, Guid.CreateVersion7());
        catalog.AddProduct(3, Guid.CreateVersion7());

        // Assert
        Assert.Equal(3, catalog.Products.Count);
    }

    // ─── RemoveProduct ───────────────────────────────────────────

    [Fact]
    public void RemoveProduct_removes_product_and_raises_event()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;
        var publicProductId = Guid.CreateVersion7();
        catalog.AddProduct(1, publicProductId);
        catalog.ClearDomainEvents();

        // Act
        var result = catalog.RemoveProduct(1, publicProductId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(catalog.Products);
        Assert.Contains(
            catalog.DomainEvents,
            e => e is CatalogProductRemovedDomainEvent removed && removed.PublicProductId == publicProductId);
    }

    [Fact]
    public void RemoveProduct_returns_failure_when_product_not_in_catalog()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;

        // Act
        var result = catalog.RemoveProduct(99, Guid.CreateVersion7());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.ProductNotInCatalog, result.Error);
    }

    [Fact]
    public void RemoveProduct_only_removes_specified_product()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", null).Value;
        catalog.AddProduct(1, Guid.CreateVersion7());
        catalog.AddProduct(2, Guid.CreateVersion7());

        // Act
        catalog.RemoveProduct(1, Guid.CreateVersion7());

        // Assert
        Assert.Single(catalog.Products);
        Assert.Equal(2, catalog.Products.First().ProductId);
    }
}

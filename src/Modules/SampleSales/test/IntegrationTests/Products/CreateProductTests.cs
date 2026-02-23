using Modules.SampleSales.Application.Products.CreateProduct;
using Rtl.Core.Domain.Results;
using Rtl.Core.IntegrationTests.Abstractions;

namespace Modules.SampleSales.IntegrationTests.Products;

public class CreateProductTests : BaseIntegrationTest
{
    public CreateProductTests(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Should_CreateProduct_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateProductCommand(
            Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            Faker.Random.Decimal(1, 1000),
            Faker.Random.Decimal(1, 500));

        // Act
        Result<int> result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(0, result.Value);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateProductCommand("", "Description", 100, 50);

        // Act
        Result<int> result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }
}

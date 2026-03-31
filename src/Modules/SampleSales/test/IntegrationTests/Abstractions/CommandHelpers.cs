using Bogus;
using MediatR;
using Modules.SampleSales.Application.Products.CreateProduct;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.IntegrationTests.Abstractions;

internal static class CommandHelpers
{
    internal static async Task<Guid> CreateProductAsync(
        this ISender sender,
        string? name = null,
        string? description = null,
        decimal? price = null,
        decimal? internalCost = null)
    {
        var faker = new Faker();
        var command = new CreateProductCommand(
            name ?? faker.Commerce.ProductName(),
            description ?? faker.Lorem.Sentence(),
            price ?? faker.Random.Decimal(1, 1000),
            internalCost ?? faker.Random.Decimal(1, 500));

        Result<Guid> result = await sender.Send(command);
        return result.Value;
    }
}
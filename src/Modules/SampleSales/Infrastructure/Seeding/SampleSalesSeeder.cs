using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.SampleSales.Domain.Catalogs;
using Modules.SampleSales.Domain.Products;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Application.Seeding;

namespace Modules.SampleSales.Infrastructure.Seeding;

public sealed class SampleSalesSeeder : IModuleSeeder
{
    public string ModuleName => "SampleSales";
    public int Order => 2;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var productRepo = services.GetRequiredService<IProductRepository>();
        var catalogRepo = services.GetRequiredService<ICatalogRepository>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork<Domain.ISampleSalesModule>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<SampleSalesSeeder>();

        var faker = new Faker();

        // Create products
        var products = new List<Product>();
        var productNames = new[]
        {
            "Standard Widget", "Premium Widget", "Economy Widget",
            "Deluxe Gadget", "Pro Gadget", "Mini Gadget",
            "Heavy-Duty Bracket", "Light Bracket", "Adjustable Mount",
            "Precision Sensor"
        };

        foreach (var name in productNames)
        {
            var price = faker.Random.Decimal(25, 999);
            var result = Product.Create(
                name,
                faker.Lorem.Sentence(),
                price,
                faker.Random.Decimal(5, price * 0.6m));

            if (result.IsSuccess)
            {
                productRepo.Add(result.Value);
                products.Add(result.Value);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} products", products.Count);

        // Create catalogs and assign products
        var catalogDefs = new (string Name, string Desc, int[] ProductIndices)[]
        {
            ("Spring Collection", "Seasonal product lineup", [0, 1, 3, 4, 8]),
            ("Value Pack", "Budget-friendly options", [2, 5, 7]),
            ("Industrial", "Heavy-duty components", [6, 7, 8, 9])
        };

        var catalogsCreated = 0;
        foreach (var (name, desc, indices) in catalogDefs)
        {
            var result = Catalog.Create(name, desc);
            if (result.IsFailure)
            {
                continue;
            }

            var catalog = result.Value;
            catalogRepo.Add(catalog);

            // Save first to get the catalog ID, then add products
            await unitOfWork.SaveChangesAsync(ct);

            foreach (var idx in indices)
            {
                if (idx < products.Count)
                {
                    catalog.AddProduct(products[idx].Id, products[idx].PublicId);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            catalogsCreated++;
        }

        logger.LogInformation("Seeded {Count} catalogs", catalogsCreated);
    }
}

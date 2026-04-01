using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.Orders;
using DomainOrder = Modules.SampleOrders.Domain.Orders.Order;
using Modules.SampleOrders.Domain.ValueObjects;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Application.Seeding;
using ModularTemplate.Domain.ValueObjects;

namespace Modules.SampleOrders.Infrastructure.Seeding;

public sealed class SampleOrdersSeeder : IModuleSeeder
{
    public string ModuleName => "SampleOrders";
    public int Order => 1;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var customerRepo = services.GetRequiredService<ICustomerRepository>();
        var orderRepo = services.GetRequiredService<IOrderRepository>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork<Domain.ISampleOrdersModule>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<SampleOrdersSeeder>();

        var faker = new Faker();

        // Create customers
        var customers = new List<Customer>();
        for (var i = 0; i < 5; i++)
        {
            var result = Customer.Create(
                faker.Name.FirstName(),
                faker.Random.Bool(0.3f) ? faker.Name.FirstName() : null,
                faker.Name.LastName(),
                faker.Internet.Email(),
                faker.Date.PastDateOnly(30, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18))));

            if (result.IsSuccess)
            {
                var customer = result.Value;
                customer.AddAddress(
                    Address.Create(
                        faker.Address.StreetAddress(),
                        faker.Random.Bool(0.2f) ? faker.Address.SecondaryAddress() : null,
                        faker.Address.City(),
                        faker.Address.StateAbbr(),
                        faker.Address.ZipCode(),
                        "US"),
                    isPrimary: true);

                customerRepo.Add(customer);
                customers.Add(customer);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} customers", customers.Count);

        // Create orders with lines
        var ordersCreated = 0;
        foreach (var customer in customers)
        {
            var orderCount = faker.Random.Int(1, 3);
            for (var i = 0; i < orderCount; i++)
            {
                var orderResult = DomainOrder.Place(customer.Id);
                if (orderResult.IsFailure)
                {
                    continue;
                }

                var order = orderResult.Value;

                var lineCount = faker.Random.Int(1, 4);
                for (var j = 0; j < lineCount; j++)
                {
                    var price = Money.Create(faker.Random.Decimal(10, 500));
                    if (price.IsSuccess)
                    {
                        order.AddProductLine(
                            faker.Random.Int(1, 5),
                            price.Value);
                    }
                }

                order.SetShippingAddress(
                    Address.Create(
                        faker.Address.StreetAddress(),
                        null,
                        faker.Address.City(),
                        faker.Address.StateAbbr(),
                        faker.Address.ZipCode(),
                        "US"));

                orderRepo.Add(order);
                ordersCreated++;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} orders", ordersCreated);
    }
}

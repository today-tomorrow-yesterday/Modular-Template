using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.IntegrationTests.Abstractions;

namespace Modules.Sales.IntegrationTests.Abstractions;

public sealed class SalesTestFactory : IntegrationTestWebAppFactory
{
    internal static readonly Guid TestCustomerId = Guid.Parse("599d51c9-0d81-e64c-8cb1-e6070073fa6c");
    internal const int TestHomeCenterNumber = 100;
    internal const int TestAuthorizedUserId1 = 1;
    internal const int TestAuthorizedUserId2 = 2;

    static SalesTestFactory()
    {
        Environment.SetEnvironmentVariable("TEST_DB_PROVIDER", "postgresql");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Development");
        builder.UseSetting("Seeding:Enabled", "false");

        // Messaging placeholders so options validation passes
        builder.UseSetting("Messaging:EmbProducer:EventBus", "test-event-bus");
        builder.UseSetting("Messaging:SqsConsumer:SqsQueueUrl",
            "https://sqs.us-east-1.amazonaws.com/000000000000/test-queue");

        const string devKey = "MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=";
        builder.UseSetting("Encryption:Key", devKey);
        Environment.SetEnvironmentVariable("ENCRYPTION_KEY", devKey);

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IiSeriesAdapter, FakeiSeriesAdapter>();
        });
    }
}

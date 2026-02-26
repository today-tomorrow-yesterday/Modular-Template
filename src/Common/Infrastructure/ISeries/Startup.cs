using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.Adapters.ISeries;

namespace Rtl.Core.Infrastructure.ISeries;

internal static class Startup
{
    internal static IServiceCollection AddISeriesAdapter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<iSeriesOptions>()
            .Bind(configuration.GetSection(iSeriesOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<iSeriesAuthHandler>();

        services.AddHttpClient<IiSeriesAdapter, iSeriesAdapter>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<iSeriesOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddHttpMessageHandler<iSeriesAuthHandler>()
            .AddStandardResilienceHandler();

        return services;
    }
}

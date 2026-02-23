using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rtl.Core.Application.Secrets;

namespace Rtl.Core.Infrastructure.Secrets;

internal static class Startup
{
    internal static IServiceCollection AddSecretProvider(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMemoryCache();

        services.AddOptions<SecretProviderOptions>()
            .Bind(configuration.GetSection(SecretProviderOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var useAws = configuration
            .GetSection(SecretProviderOptions.SectionName)
            .GetValue<bool?>("UseAws") ?? true;

        if (useAws)
        {
            services.AddSingleton<IAmazonSecretsManager>(new AmazonSecretsManagerClient());
            services.AddSingleton<ISecretProvider, AwsSecretProvider>();
        }
        else
        {
            services.AddSingleton<ISecretProvider, DevelopmentSecretProvider>();
        }

        return services;
    }
}

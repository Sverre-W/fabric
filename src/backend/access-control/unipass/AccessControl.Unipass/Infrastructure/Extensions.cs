using System.Net.Security;
using System.Text.Json;
using AccessControl.Unipass.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AccessControl.Unipass.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddUnipassWebApi(
        this IServiceCollection collection,
        string baseAddress,
        string apiKey,
        TimeZoneInfo timeZoneInfo,
        bool disableSslValidation = false,
        bool enableLogging = false
    )
    {
        JsonSerializerOptions? jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = null,
        };

        collection.AddTransient(_ => new UnipassAuthorizationHeader(apiKey));

        var httpBuilder = collection
            .AddHttpClient("UnipassClient")
            .AddHttpMessageHandler<UnipassAuthorizationHeader>();

        if (disableSslValidation)
        {
            httpBuilder.ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                }
            );
        }

        if (enableLogging)
        {
            collection.AddTransient(sp => new JsonLoggingHandler(
                sp.GetRequiredService<ILogger<JsonLoggingHandler>>()
            ));
            httpBuilder.AddHttpMessageHandler<JsonLoggingHandler>();
        }

        httpBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(baseAddress));

        collection.AddScoped<IUnipassApi>(sp =>
        {
            var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = clientFactory.CreateClient("UnipassClient");
            return new UnipassWebApi(
                client,
                timeZoneInfo,
                jsonSerializerOptions
                    ?? new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    }
            );
        });

        return collection;
    }
}

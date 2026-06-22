using System.Text.Json;
using AccessControl.Unipass.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AccessControl.Unipass.Tests;

public class UnipassTestBase
{
    protected static ServiceProvider CreateServiceProvider(
        HttpMessageHandler handler,
        string apiKey = "some-auth-header-value",
        string baseAddress = "http://example"
    )
    {
        var services = new ServiceCollection();

        // Replace actual network handler with our mock
        services.AddSingleton(handler);

        TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

        services.AddUnipassWebApi(baseAddress, apiKey, timeZoneInfo);
        services.AddHttpClient("UnipassClient").ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }
}

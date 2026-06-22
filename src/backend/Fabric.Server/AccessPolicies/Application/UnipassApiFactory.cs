using System.Text;
using System.Text.Json;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Infrastructure;
using Fabric.Server.AccessPolicies.Domain;

namespace Fabric.Server.AccessPolicies.Application;

public sealed class UnipassApiFactory
{
    public IUnipassApi Create(UnipassSystemConfig config)
    {
        var handler = new HttpClientHandler();
        if (!config.SslValidation)
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var client = new HttpClient(new UnipassAuthorizationHeader(CreateAuthorizationHeader(config))
        {
            InnerHandler = handler
        })
        {
            BaseAddress = new Uri(config.Endpoint)
        };

        return new UnipassWebApi(
            client,
            TimeZoneInfo.Local,
            new JsonSerializerOptions { PropertyNamingPolicy = null });
    }

    private static string CreateAuthorizationHeader(UnipassSystemConfig config)
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Username}:{config.Password}"));
        return $"Basic {credentials}";
    }
}

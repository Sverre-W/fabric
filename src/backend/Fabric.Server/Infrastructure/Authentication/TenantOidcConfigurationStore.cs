using System.Collections.Concurrent;
using Fabric.Server.Tenants.Domain;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fabric.Server.Infrastructure.Authentication;

public sealed class TenantOidcConfigurationStore : ITenantOidcConfigurationStore
{
    private readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configurationManagers = new();

    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string tenantId,
        OidcSettings settings,
        CancellationToken cancellationToken)
    {
        ConfigurationManager<OpenIdConnectConfiguration> manager = GetManager(tenantId, settings);
        return await manager.GetConfigurationAsync(cancellationToken);
    }

    public void RequestRefresh(string tenantId, OidcSettings settings) => GetManager(tenantId, settings).RequestRefresh();

    private ConfigurationManager<OpenIdConnectConfiguration> GetManager(string tenantId, OidcSettings settings) =>
        _configurationManagers.GetOrAdd(GetCacheKey(tenantId, settings), _ =>
        {
            var documentRetriever = new HttpDocumentRetriever
            {
                RequireHttps = settings.RequireHttpsMetadata
            };

            return new ConfigurationManager<OpenIdConnectConfiguration>(
                settings.MetadataUrl,
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever);
        });

    private static string GetCacheKey(string tenantId, OidcSettings settings) =>
        $"{tenantId}:{settings.MetadataUrl}:{settings.RequireHttpsMetadata}";
}

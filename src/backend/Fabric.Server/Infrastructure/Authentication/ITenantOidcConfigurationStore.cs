using Fabric.Server.Tenants.Domain;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fabric.Server.Infrastructure.Authentication;

public interface ITenantOidcConfigurationStore
{
    Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string tenantId,
        OidcSettings settings,
        CancellationToken cancellationToken);

    void RequestRefresh(string tenantId, OidcSettings settings);
}

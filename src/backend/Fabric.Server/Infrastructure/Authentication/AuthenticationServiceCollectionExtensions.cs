using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Fabric.Server.Infrastructure.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddFabricAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<ITenantOidcConfigurationStore, TenantOidcConfigurationStore>();

        services.AddAuthentication(TenantBearerAuthenticationDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TenantBearerAuthenticationHandler>(
                TenantBearerAuthenticationDefaults.AuthenticationScheme,
                _ => { })
            .AddScheme<AuthenticationSchemeOptions, ReceptionKioskAuthenticationHandler>(
                ReceptionKioskAuthenticationDefaults.AuthenticationScheme,
                _ => { })
            .AddScheme<AuthenticationSchemeOptions, HardwareAgentAuthenticationHandler>(
                HardwareAgentAuthenticationDefaults.AuthenticationScheme,
                _ => { });

        var requireAuthPolicy = new AuthorizationPolicyBuilder(TenantBearerAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(requireAuthPolicy)
            .SetFallbackPolicy(requireAuthPolicy)
            .AddPolicy(ReceptionKioskAuthenticationDefaults.Policy, policy =>
            {
                policy.AuthenticationSchemes.Add(ReceptionKioskAuthenticationDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(ReceptionKioskAuthenticationDefaults.Role);
            })
            .AddPolicy(HardwareAgentAuthenticationDefaults.Policy, policy =>
            {
                policy.AuthenticationSchemes.Add(HardwareAgentAuthenticationDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(HardwareAgentAuthenticationDefaults.Role);
            });

        return services;
    }
}

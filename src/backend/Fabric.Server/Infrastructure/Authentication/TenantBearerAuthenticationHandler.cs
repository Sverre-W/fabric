using System.Security.Claims;
using System.Text.Encodings.Web;
using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Tenants.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Fabric.Server.Infrastructure.Authentication;

public sealed class TenantBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ITenantContext tenantContext,
    ITenantOidcConfigurationStore configurationStore)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string BearerPrefix = "Bearer ";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authorizationHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return AuthenticateResult.NoResult();

        if (!authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Authorization header must use the Bearer scheme.");

        string token = authorizationHeader[BearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.Fail("Bearer token is required.");

        OidcSettings oidcSettings = tenantContext.Configuration.Oidc;
        TokenValidationResult validationResult = await ValidateTokenAsync(token, oidcSettings, refreshMetadata: false);

        if (!validationResult.IsValid && validationResult.Exception is SecurityTokenSignatureKeyNotFoundException)
        {
            configurationStore.RequestRefresh(tenantContext.TenantId, oidcSettings);
            validationResult = await ValidateTokenAsync(token, oidcSettings, refreshMetadata: true);
        }

        if (!validationResult.IsValid)
            return AuthenticateResult.Fail(validationResult.Exception ?? new SecurityTokenValidationException("Bearer token is invalid."));

        if (validationResult.ClaimsIdentity is null)
            return AuthenticateResult.Fail("Bearer token did not produce an identity.");

        var principal = new ClaimsPrincipal(validationResult.ClaimsIdentity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Bearer";
        return base.HandleChallengeAsync(properties);
    }

    private async Task<TokenValidationResult> ValidateTokenAsync(
        string token,
        OidcSettings oidcSettings,
        bool refreshMetadata)
    {
        OpenIdConnectConfiguration configuration = await configurationStore.GetConfigurationAsync(
            tenantContext.TenantId,
            oidcSettings,
            Context.RequestAborted);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration.Issuer,
            ValidateAudience = true,
            ValidAudience = oidcSettings.ClientId,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };

        var tokenHandler = new JsonWebTokenHandler
        {
            MapInboundClaims = false
        };

        return await tokenHandler.ValidateTokenAsync(token, validationParameters);
    }
}

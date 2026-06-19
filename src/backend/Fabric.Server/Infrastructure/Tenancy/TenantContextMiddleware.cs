using Microsoft.Extensions.Options;

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenantContextMiddleware(
    RequestDelegate next,
    IOptions<TenancyOptions> options)
{
    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContext)
    {
        string tenantId = options.Value.Mode switch
        {
            TenancyMode.SingleTenant => TenantContext.DefaultTenantId,
            TenancyMode.MultiTenant => GetTenantFromHost(context.Request.Host.Host),
            _ => TenantContext.DefaultTenantId,
        };

        tenantContext.SetTenant(tenantId);
        await next(context);
    }

    private static string GetTenantFromHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return TenantContext.DefaultTenantId;

        int firstDot = host.IndexOf('.');
        return firstDot > 0 ? host[..firstDot] : host;
    }
}

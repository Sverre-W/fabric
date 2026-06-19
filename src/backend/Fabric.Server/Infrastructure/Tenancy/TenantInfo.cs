using Fabric.Server.Tenants.Domain;

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed record TenantInfo(string Id, TenantConfiguration Configuration);

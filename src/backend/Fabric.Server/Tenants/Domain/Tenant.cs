namespace Fabric.Server.Tenants.Domain;

public sealed class Tenant
{
    private Tenant() { }

    public string Id { get; private set; } = null!;
    public TenantConfiguration Configuration { get; private set; } = null!;

    public static Tenant Create(string id, TenantConfiguration configuration) =>
        new()
        {
            Id = id,
            Configuration = configuration
        };

    public void UpdateConfiguration(TenantConfiguration configuration) => Configuration = configuration;
}

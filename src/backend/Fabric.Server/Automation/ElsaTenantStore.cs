using Elsa.Common.Multitenancy;
using Fabric.Server.Infrastructure.Tenancy;

using ElsaTenatStore = Elsa.Tenants.ITenantStore;
using FabricTenantStore = Fabric.Server.Infrastructure.Tenancy.ITenantStore;

namespace Fabric.Server.Automation;


public class ElsaTenantStore(FabricTenantStore provider) : ElsaTenatStore
{
    public async Task<Tenant?> FindAsync(TenantFilter filter, CancellationToken cancellationToken = new CancellationToken())
    {
        TenantInfo? result = await provider.GetTenantAsync(filter.Id, cancellationToken);
        return result == null ? null : Map(result);
    }

    public Task<Tenant?> FindAsync(string id, CancellationToken cancellationToken = new CancellationToken()) => FindAsync(TenantFilter.ById(id), cancellationToken);

    public async Task<IEnumerable<Tenant>> FindManyAsync(TenantFilter filter, CancellationToken cancellationToken = new CancellationToken())
    {
        IEnumerable<Tenant> tenants = await ListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(filter.Id))
            tenants = tenants.Where(x => x.Id == filter.Id);

        return tenants;
    }

    public async Task<IEnumerable<Tenant>> ListAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        List<TenantInfo> tenants = await provider.GetAllTenantsAsync(cancellationToken);
        return tenants.ConvertAll(Map);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = new CancellationToken()) => Task.FromResult(false);

    public Task<long> DeleteAsync(TenantFilter filter, CancellationToken cancellationToken = new CancellationToken()) => Task.FromResult(0L);

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default) => Task.CompletedTask;

    private static Tenant Map(TenantInfo info) => new() { Id = info.Id, Name = info.Id };
}

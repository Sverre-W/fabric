using Fabric.Server.Tenants.Domain;
using Fabric.Server.Tenants.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Tenants.Persistence;

public sealed class TenantsDbContext : DbContext
{
    public const string Schema = "tenancy";

    public DbSet<Tenant> Tenants { get; set; } = null!;

    public TenantsDbContext(DbContextOptions<TenantsDbContext> options) : base(options)
    {
    }

    public TenantsDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantsDbContext).Assembly,
            t => t.Namespace == typeof(TenantConfigurationConfiguration).Namespace);
    }
}

using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Persistence;

public class ReceptionDbContext : TenantDbContext
{
    public const string Schema = "reception";

    public DbSet<ExpectedArrival> Arrivals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReceptionDbContext).Assembly,
            t => t.Namespace == typeof(ExpectedArrivalConfiguration).Namespace);
        ApplyTenantFilters(modelBuilder);
    }

    public ReceptionDbContext(DbContextOptions<ReceptionDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public ReceptionDbContext()
    {
    }
}

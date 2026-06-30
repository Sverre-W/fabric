using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence.Configuration;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Hardware.Persistence;

public class HardwareDbContext : TenantDbContext
{
    public const string Schema = "hardware";

    public DbSet<HardwareAgent> Agents { get; set; } = null!;
    public DbSet<HardwareDevice> Devices { get; set; } = null!;
    public DbSet<HardwareEventInboxItem> EventInbox { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new HardwareAgentConfiguration());
        modelBuilder.ApplyConfiguration(new HardwareDeviceConfiguration());
        modelBuilder.ApplyConfiguration(new HardwareEventInboxItemConfiguration());
        ApplyTenantFilters(modelBuilder);
    }

    public HardwareDbContext(DbContextOptions<HardwareDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public HardwareDbContext()
    {
    }
}

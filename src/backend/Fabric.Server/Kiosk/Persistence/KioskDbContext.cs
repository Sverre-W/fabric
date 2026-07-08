using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Persistence;

public sealed class KioskDbContext : TenantDbContext
{
    public const string Schema = "kiosk";

    public DbSet<Domain.Kiosk> Kiosks { get; set; } = null!;
    public DbSet<KioskProfile> Profiles { get; set; } = null!;
    public DbSet<KioskProfileLanguage> Languages { get; set; } = null!;
    public DbSet<KioskTranslation> Translations { get; set; } = null!;
    public DbSet<KioskThemeToken> ThemeTokens { get; set; } = null!;
    public DbSet<KioskWelcomeSettings> WelcomeSettings { get; set; } = null!;
    public DbSet<KioskHardwareBinding> HardwareBindings { get; set; } = null!;
    public DbSet<KioskAsset> Assets { get; set; } = null!;
    public DbSet<KioskDeviceAssignment> DeviceAssignments { get; set; } = null!;
    public DbSet<KioskDevice> Devices { get; set; } = null!;
    public DbSet<KioskSession> Sessions { get; set; } = null!;

    public KioskDbContext(DbContextOptions<KioskDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public KioskDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new KioskConfiguration());
        modelBuilder.ApplyConfiguration(new KioskProfileConfiguration());
        modelBuilder.ApplyConfiguration(new KioskProfileLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new KioskTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new KioskThemeTokenConfiguration());
        modelBuilder.ApplyConfiguration(new KioskWelcomeSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new KioskHardwareBindingConfiguration());
        modelBuilder.ApplyConfiguration(new KioskAssetConfiguration());
        modelBuilder.ApplyConfiguration(new KioskDeviceAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new KioskDeviceConfiguration());
        modelBuilder.ApplyConfiguration(new KioskSessionConfiguration());
        ApplyTenantFilters(modelBuilder);
    }
}

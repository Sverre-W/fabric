using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.CredentialManagement.Persistence.Configuration;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.CredentialManagement.Persistence;

public sealed class CredentialManagementDbContext : TenantDbContext
{
    public const string Schema = "credential_management";

    public DbSet<CredentialType> CredentialTypes { get; set; } = null!;
    public DbSet<CredentialTypeTarget> CredentialTypeTargets { get; set; } = null!;
    public DbSet<CredentialReservation> CredentialReservations { get; set; } = null!;
    public DbSet<Credential> Credentials { get; set; } = null!;
    public DbSet<CredentialProvisioningTransaction> CredentialProvisioningTransactions { get; set; } = null!;

    public CredentialManagementDbContext(DbContextOptions<CredentialManagementDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public CredentialManagementDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new CredentialTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CredentialTypeTargetConfiguration());
        modelBuilder.ApplyConfiguration(new CredentialReservationConfiguration());
        modelBuilder.ApplyConfiguration(new CredentialConfiguration());
        modelBuilder.ApplyConfiguration(new CredentialProvisioningTransactionConfiguration());
        ApplyTenantFilters(modelBuilder);
    }
}

using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence.Configuration;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Persistence;

public class AccessPoliciesDbContext : TenantDbContext
{
    public const string Schema = "access_policies";

    public DbSet<AccessPolicy> AccessPolicies { get; set; } = null!;
    public DbSet<PolicyRequirement> PolicyRequirements { get; set; } = null!;
    public DbSet<AccessControlSystem> AccessControlSystems { get; set; } = null!;
    public DbSet<BadgeType> BadgeTypes { get; set; } = null!;
    public DbSet<AccessLevelType> AccessLevelTypes { get; set; } = null!;
    public DbSet<IdentityMapping> IdentityMappings { get; set; } = null!;

    public AccessPoliciesDbContext(DbContextOptions<AccessPoliciesDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public AccessPoliciesDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new AccessPolicyConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyRequirementConfiguration());
        modelBuilder.ApplyConfiguration(new CredentialRequirementConfiguration());
        modelBuilder.ApplyConfiguration(new AccessRequirementConfiguration());
        modelBuilder.ApplyConfiguration(new AccessControlSystemConfiguration());
        modelBuilder.ApplyConfiguration(new BadgeTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UnipassBadgeTypeConfiguration());
        modelBuilder.ApplyConfiguration(new LenelBadgeTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AccessLevelTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UnipassAccessLevelTypeConfiguration());
        modelBuilder.ApplyConfiguration(new LenelAccessLevelTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UnipassAccessControlSystemConfiguration());
        modelBuilder.ApplyConfiguration(new LenelAccessControlSystemConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityMappingConfiguration());
        ApplyTenantFilters(modelBuilder);
    }
}

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccessPoliciesDbContext).Assembly,
            type => type.Namespace == typeof(AccessPolicyConfiguration).Namespace);
        ApplyTenantFilters(modelBuilder);
    }
}

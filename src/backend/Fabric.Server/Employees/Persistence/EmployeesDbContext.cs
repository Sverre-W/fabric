using Fabric.Server.Employees.Domain;
using Fabric.Server.Employees.Persistence.Configuration;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Employees.Persistence;

public sealed class EmployeesDbContext : TenantDbContext
{
    public const string Schema = "employees";

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; } = null!;
    public DbSet<OrganizationUnitClosure> OrganizationUnitClosures { get; set; } = null!;

    public EmployeesDbContext(DbContextOptions<EmployeesDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public EmployeesDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationUnitClosureConfiguration());
        ApplyTenantFilters(modelBuilder);
    }
}

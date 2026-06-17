using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Persistence;

public class ReceptionDbContext : DbContext
{
    public const string Schema = "reception";

    public DbSet<ExpectedArrival> Arrivals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReceptionDbContext).Assembly,
            t => t.Namespace == typeof(ExpectedArrivalConfiguration).Namespace);
    }

    public ReceptionDbContext(DbContextOptions<ReceptionDbContext> options) : base(options)
    {
    }

    public ReceptionDbContext()
    {
    }
}
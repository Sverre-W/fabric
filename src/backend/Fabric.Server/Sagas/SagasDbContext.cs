using Fabric.Server.Sagas.Persistence.Configuration;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Sagas;

public class SagasDbContext : DbContext
{
    public const string Schema = "sagas";
    public DbSet<VisitorPreOnboardingSaga> VisitorPreOnboardingSagas { get; set; } = null!;

    public SagasDbContext(DbContextOptions<SagasDbContext> options) : base(options)
    {
    }

    public SagasDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SagasDbContext).Assembly,
                t => t.Namespace == typeof(VisitorPreOnboardingSagaConfiguration).Namespace);
    }
}

using Fabric.Server.Visitors.Domain;
using Fabric.Server.Visitors.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Visitors.Persistence;

public class VisitorsDbContext : DbContext
{
    public const string Schema = "visitors";
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<VisitInvitation> Invitations { get; set; } = null!;
    public DbSet<Visitor> Visitors { get; set; } = null!;
    public DbSet<Organizer> Organizers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VisitorsDbContext).Assembly,
                t => t.Namespace == typeof(VisitConfiguration).Namespace);
    }

    public VisitorsDbContext(DbContextOptions<VisitorsDbContext> options) : base(options)
    {
    }

    public VisitorsDbContext()
    {
    }

}

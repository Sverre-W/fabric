using Fabric.Server.Locations.Domain;
using Fabric.Server.Locations.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Locations.Persistence;

public class LocationsDbContext : DbContext
{
    public const string Schema = "locations";

    public DbSet<Site> Sites { get; set; } = null!;
    public DbSet<Building> Buildings { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<LocationLookup> LocationLookups { get; set; } = null!;

    public LocationsDbContext(DbContextOptions<LocationsDbContext> options) : base(options)
    {
    }

    public LocationsDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationsDbContext).Assembly,
            type => type.Namespace == typeof(SiteConfiguration).Namespace);
    }
}

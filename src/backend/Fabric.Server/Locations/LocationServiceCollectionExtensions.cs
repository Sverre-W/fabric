using Fabric.Server.Locations.Application;
using Fabric.Server.Locations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Locations;

public static class LocationServiceCollectionExtensions
{
    public static IServiceCollection SetupLocations(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<LocationsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", LocationsDbContext.Schema));
        });

        collection.AddScoped<LocationService>();
        return collection;
    }
}

using Fabric.Server.Reception.Application;
using Fabric.Server.Reception.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception;

public static class ReceptionServiceCollectionExtensions
{
    public static IServiceCollection SetupReception(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<ReceptionDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", ReceptionDbContext.Schema));
        });

        collection.AddScoped<ReceptionService>();
        return collection;
    }
}
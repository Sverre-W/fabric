using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire;

public static class DesfireServiceCollectionExtensions
{
    public static IServiceCollection SetupDesfire(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<DesfireDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", DesfireDbContext.Schema));
        });

        collection.AddDataProtection();
        collection.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Add(DesfireJsonSerializerContext.Default));
        collection.AddOptions<DesfireEncodingOptions>()
            .Bind(configuration.GetSection("Desfire:Encoding"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        collection.AddScoped<IDesfireKeyProtector, DesfireKeyProtector>();
        collection.AddScoped<DesfireKeyGroupResolver>();
        collection.AddScoped<DesfireTransformationPlanner>();
        collection.AddScoped<DesfireDeviceLeaseStore>();
        collection.AddScoped<DesfireVariableResolver>();
        collection.AddScoped<DesfireEncodingService>();
        collection.AddHostedService<DesfireEncodingWorker>();

        return collection;
    }
}

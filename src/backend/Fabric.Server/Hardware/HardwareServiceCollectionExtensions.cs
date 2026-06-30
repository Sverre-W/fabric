using Fabric.Server.Hardware.Application;
using Fabric.Server.Hardware.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Hardware;

public static class HardwareServiceCollectionExtensions
{
    public static IServiceCollection SetupHardware(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<HardwareDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", HardwareDbContext.Schema));
        });

        collection.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Add(HardwareJsonSerializerContext.Default));

        collection.AddScoped<HardwareAgentKeyHasher>();
        collection.AddScoped<IQrScanner, QrScanner>();
        collection.AddScoped<ILabelPrinter, LabelPrinter>();
        collection.AddSingleton<HardwareCommandStore>();
        collection.AddSingleton<HardwareAgentConnectionManager>();

        return collection;
    }
}

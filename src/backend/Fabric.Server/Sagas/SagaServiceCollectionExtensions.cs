using Fabric.Server.Sagas.Kiosk;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Sagas;

public static class SagaServiceCollectionExtensions
{
    public static IServiceCollection SetupSagas(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<SagasDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"), x =>
                    x.MigrationsHistoryTable("__EFMigrationsHistory", SagasDbContext.Schema)
                );

        });

        collection.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Add(SagasJsonSerializerContext.Default));

        collection.AddSingleton<VisitorPreOnboardingSagaTrigger>();
        collection.AddScoped<VisitorPreOnboardingSagaService>();
        collection.AddHostedService<VisitorPreOnboardingWorker>();
        collection.AddSingleton<KioskSagaTrigger>();
        collection.AddScoped<KioskSagaService>();
        collection.AddHostedService<KioskWorker>();

        return collection;
    }
}

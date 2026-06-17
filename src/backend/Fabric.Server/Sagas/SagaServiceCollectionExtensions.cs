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

        collection.AddScoped<VisitorPreOnboardingSagaService>();
        collection.AddHostedService<VisitorPreOnboardingWorker>();

        return collection;
    }
}

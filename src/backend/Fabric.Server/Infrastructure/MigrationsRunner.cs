using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Hardware.Persistence;
using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Locations.Persistence;
using Fabric.Server.Reception.Persistence;
using Fabric.Server.Sagas;
using Fabric.Server.Tenants.Persistence;
using Fabric.Server.Visitors.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Infrastructure;

internal class MigrationRunner<T>(IServiceScope scope) where T : DbContext
{
    public async Task RunMigrationsAsync(CancellationToken cancellationToken)
    {
        T dbContext = scope.ServiceProvider.GetRequiredService<T>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}

public class MigrationsRunner(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        await new MigrationRunner<TenantsDbContext>(scope).RunMigrationsAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<TenantSeeder>().SeedAsync(cancellationToken);
        await new MigrationRunner<AccessPoliciesDbContext>(scope).RunMigrationsAsync(cancellationToken);
        await new MigrationRunner<VisitorsDbContext>(scope).RunMigrationsAsync(cancellationToken);
        await new MigrationRunner<SagasDbContext>(scope).RunMigrationsAsync(cancellationToken);
        await new MigrationRunner<HardwareDbContext>(scope).RunMigrationsAsync(cancellationToken);
        await new MigrationRunner<LocationsDbContext>(scope).RunMigrationsAsync(cancellationToken);
        await new MigrationRunner<ReceptionDbContext>(scope).RunMigrationsAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

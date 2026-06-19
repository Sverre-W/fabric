namespace Fabric.Server.Infrastructure.Tenancy;

public static class TenancyServiceCollectionExtensions
{
    public static IServiceCollection AddTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TenancyOptions>()
            .Bind(configuration.GetSection(TenancyOptions.SectionName))
            .Validate(options => Enum.IsDefined(options.Mode), "Tenancy mode must be SingleTenant or MultiTenant.")
            .ValidateOnStart();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextAccessor>(provider => provider.GetRequiredService<TenantContext>());

        return services;
    }
}

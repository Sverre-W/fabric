using Fabric.Server.AccessPolicies;
using Fabric.Server.AccessPolicies.Endpoints;
using Fabric.Server.Infrastructure;
using Fabric.Server.Infrastructure.Authentication;
using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Locations;
using Fabric.Server.Locations.Endpoints;
using Fabric.Server.Notifications;
using Fabric.Server.Reception;
using Fabric.Server.Reception.Endpoints;
using Fabric.Server.Sagas;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Fabric.Server.Tenants;
using Fabric.Server.Tenants.Endpoints;
using Fabric.Server.Visitors;
using Fabric.Server.Visitors.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var section = builder.Configuration.GetSection("EnableOpenApi");
bool enableOpenApi = section.Exists() && section.Get<bool>();

if (enableOpenApi)
{
    builder.Services.AddOpenApi();
}

builder.Services.AddTransient(_ => TimeProvider.System);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Clear());
builder.Services.AddTenancy(builder.Configuration);
builder.Services.AddFabricAuthentication();

builder.Services.AddCors(options =>
{
    string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

    options.AddPolicy("ApiCors", policy =>
    {
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .SetupTenants(builder.Configuration)
    .SetupAccessPolicies(builder.Configuration)
    .SetupVisitors(builder.Configuration)
    .SetupSagas(builder.Configuration)
    .SetupLocations(builder.Configuration)
    .SetupReception(builder.Configuration)
    .SetupNotifications(builder.Configuration);

builder.Services.AddHostedService<MigrationsRunner>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (enableOpenApi)
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseCors("ApiCors");
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapTenantEndpoints();
app.MapAccessPolicyEndpoints();
app.MapAccessControlSystemEndpoints();
app.MapLocationEndpoints();
app.MapReceptionEndpoints();
app.MapReceptionAccessRuleAssignmentEndpoints();
app.MapVisitorEndpoints();
app.MapOrganizerEndpoints();
app.MapVisitorPreOnboardingSagaEndpoints();

app.Run();

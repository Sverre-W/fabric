using System.Text.Json;
using System.Text.Json.Serialization;
using Fabric.Server.AccessPolicies;
using Fabric.Server.Infrastructure;
using Fabric.Server.Locations;
using Fabric.Server.Reception;
using Fabric.Server.Sagas;
using Fabric.Server.Visitors;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var section = builder.Configuration.GetSection("EnableSwagger");
bool enableSwagger = section.Exists() && section.Get<bool>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        //NOTE: This is used by swagger the actual json settings are configured using json options
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (enableSwagger)
{
    builder.Services
        .AddOpenApi()
        .AddSwaggerGen(o =>
        {
            o.DescribeAllParametersInCamelCase();
            o.UseOneOfForPolymorphism();
        });
}

builder.Services.AddTransient(_ => TimeProvider.System);

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
    .SetupAccessPolicies(builder.Configuration)
    .SetupVisitors(builder.Configuration)
    .SetupSagas(builder.Configuration)
    .SetupLocations(builder.Configuration)
    .SetupReception(builder.Configuration);

builder.Services.AddHostedService<MigrationsRunner>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (enableSwagger)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();

app.UseCors("ApiCors");

app.MapControllers();

app.Run();

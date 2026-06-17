using Fabric.Server.Infrastructure;
using Fabric.Server.Locations;
using Fabric.Server.Reception;
using Fabric.Server.Sagas;
using Fabric.Server.Visitors;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddOpenApi()
    .AddSwaggerGen()
    .AddTransient<TimeProvider>(x => TimeProvider.System);

builder.Services
    .SetupVisitors(builder.Configuration)
    .SetupSagas(builder.Configuration)
    .SetupLocations(builder.Configuration)
    .SetupReception(builder.Configuration);

builder.Services.AddHostedService<MigrationsRunner>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();

app.MapControllers();

app.Run();

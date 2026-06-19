using Fabric.Server.Notifications.Services;

namespace Fabric.Server.Notifications;

public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection SetupNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(options => options.Graph is null || options.Graph.IsConfigured(),
                "Email:Graph must include FromEmail, FromName, AzureTenantId, ApplicationId and Secret when configured.")
            .ValidateOnStart();

        services.AddScoped<EmailNotificationSender>();

        return services;
    }
}

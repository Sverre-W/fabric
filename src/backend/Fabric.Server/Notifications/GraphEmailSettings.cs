namespace Fabric.Server.Notifications;

public sealed record GraphEmailSettings
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string AzureTenantId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool SaveSentItems { get; set; }

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(FromEmail)
        && !string.IsNullOrWhiteSpace(FromName)
        && !string.IsNullOrWhiteSpace(AzureTenantId)
        && !string.IsNullOrWhiteSpace(ApplicationId)
        && !string.IsNullOrWhiteSpace(Secret);
}

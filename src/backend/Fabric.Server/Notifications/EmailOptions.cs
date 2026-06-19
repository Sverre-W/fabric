namespace Fabric.Server.Notifications;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public GraphEmailSettings? Graph { get; set; }
}

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenancyOptions
{
    public const string SectionName = "Tenancy";

    public TenancyMode Mode { get; init; } = TenancyMode.SingleTenant;
}

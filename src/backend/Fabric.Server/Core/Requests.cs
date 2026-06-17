namespace Fabric.Server.Core;

public record BaseListRequest
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 25;
}
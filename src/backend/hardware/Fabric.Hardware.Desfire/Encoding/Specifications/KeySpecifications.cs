namespace Fabric.Hardware.Desfire.Encoding.Specifications;

public record KeySpecification
{
    public string KeyGroupName { get; set; } = string.Empty;
    public string KeyGroup { get; set; } = string.Empty;
    public int KeySet { get; set; }
    public int Key { get; set; }
}

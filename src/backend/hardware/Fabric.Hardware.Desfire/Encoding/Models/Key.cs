namespace Fabric.Hardware.Desfire.Encoding.Models;

public record Key
{
    /// <summary>
    ///     The id of the key within the key set
    /// </summary>
    public int KeyId { get; set; }

    /// <summary>
    ///     The hexadecimal representation of the key
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    ///     Indicates how this key is diversified
    /// </summary>
    public bool IsKeyDiversified { get; set; }
}

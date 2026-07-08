namespace Fabric.Hardware.Desfire.Encoding.Models;

public record KeySet
{
    /// <summary>
    ///     The id of the key set
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     The order set of keys within this key set
    /// </summary>
    public Key[] Keys { get; set; } = [];
}

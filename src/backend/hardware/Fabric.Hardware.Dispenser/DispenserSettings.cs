namespace Fabric.Hardware.Dispenser;

public sealed class DispenserSettings
{
    /// <summary>
    /// The COM port used to communicate with the dispenser.
    /// </summary>
    public required string ComPort { get; init; }

    /// <summary>
    /// Timeout for waiting for command response.
    /// </summary>
    public TimeSpan ResponseTimeout { get; init; } = TimeSpan.FromSeconds(5);
}

using System.ComponentModel.DataAnnotations;

namespace Fabric.Hardware.Agent.Options;

public sealed class CollectorDeviceOptions
{
    [Required]
    public string DeviceId { get; init; } = "card-collector-01";

    [Required]
    public string ComPort { get; init; } = string.Empty;

    [Required]
    public string RfidReaderDeviceId { get; init; } = string.Empty;

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromMilliseconds(250);

    public TimeSpan RfidReadTimeout { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan RemovalTimeout { get; init; } = TimeSpan.FromSeconds(20);

    public TimeSpan AckTimeout { get; init; } = TimeSpan.FromMilliseconds(500);

    public TimeSpan MechanicalAckTimeout { get; init; } = TimeSpan.FromSeconds(3);

    public int MaxJamCount { get; init; } = 3;
}

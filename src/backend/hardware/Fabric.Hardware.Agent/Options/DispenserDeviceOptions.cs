using System.ComponentModel.DataAnnotations;

namespace Fabric.Hardware.Agent.Options;

public sealed class DispenserDeviceOptions
{
    [Required]
    public string DeviceId { get; init; } = "card-dispenser-01";

    [Required]
    public string ComPort { get; init; } = string.Empty;

    [Required]
    public string RfidReaderDeviceId { get; init; } = string.Empty;

    public TimeSpan ResponseTimeout { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan RfidReadTimeout { get; init; } = TimeSpan.FromSeconds(5);
}

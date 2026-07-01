using System.ComponentModel.DataAnnotations;

namespace Fabric.Hardware.Agent.Options;

public sealed class RfidEasReaderOptions
{
    [Required]
    public string DeviceId { get; init; } = "rfid-reader-01";

    public int ReaderId { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace Fabric.Hardware.Agent.Options;

public sealed class QrReaderDeviceOptions
{
    [Required]
    public string DeviceId { get; init; } = "qr-reader-01";

    [Required]
    public string ComPort { get; init; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Fabric.Hardware.Agent.Options;

public sealed class HardwareAgentOptions
{
    public const string SectionName = "HardwareAgent";

    [Required]
    public Uri ServerBaseUrl { get; init; } = new("https://localhost:5001");

    [Required]
    public string AgentId { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(30);

    public TimeSpan InventoryInterval { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan CommandPollInterval { get; init; } = TimeSpan.FromSeconds(2);

    public TimeSpan CommandReconcileInterval { get; init; } = TimeSpan.FromMinutes(1);

    public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromMinutes(2);

    public string ServiceName { get; init; } = "Fabric Hardware Agent";

    public QrReaderDeviceOptions[] QrReaders { get; init; } = [];

    public DispenserDeviceOptions[] Dispensers { get; init; } = [];

    public CollectorDeviceOptions[] Collectors { get; init; } = [];

    public RfidEasOptions RfidEas { get; init; } = new();
}

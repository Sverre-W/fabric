namespace Fabric.Server.Hardware.Domain;

public class HardwareDevice
{
    public Guid Id { get; private set; }
    public string AgentId { get; private set; } = default!;
    public string DeviceId { get; private set; } = default!;
    public string Kind { get; private set; } = default!;
    public string Driver { get; private set; } = default!;
    public string[] Capabilities { get; private set; } = [];
    public string State { get; private set; } = default!;
    public bool Enabled { get; private set; }
    public string DiagnosticsJson { get; private set; } = "{}";
    public DateTimeOffset LastSeenAt { get; private set; }

    public static HardwareDevice Create(
        string agentId,
        string deviceId,
        string kind,
        string driver,
        IReadOnlyList<string> capabilities,
        string state,
        string diagnosticsJson,
        DateTimeOffset lastSeenAt) => new()
    {
        Id = Guid.NewGuid(),
        AgentId = HardwareAgent.NormalizeId(agentId),
        DeviceId = NormalizeDeviceId(deviceId),
        Kind = kind.Trim(),
        Driver = driver.Trim(),
        Capabilities = [.. capabilities],
        State = state.Trim(),
        Enabled = true,
        DiagnosticsJson = diagnosticsJson,
        LastSeenAt = lastSeenAt
    };

    public void UpdateInventory(
        string kind,
        string driver,
        IReadOnlyList<string> capabilities,
        string state,
        string diagnosticsJson,
        DateTimeOffset lastSeenAt)
    {
        Kind = kind.Trim();
        Driver = driver.Trim();
        Capabilities = [.. capabilities];
        State = state.Trim();
        DiagnosticsJson = diagnosticsJson;
        LastSeenAt = lastSeenAt;
    }

    public void SetEnabled(bool enabled) => Enabled = enabled;

    public static string NormalizeDeviceId(string deviceId) => deviceId.Trim().ToLowerInvariant();
}

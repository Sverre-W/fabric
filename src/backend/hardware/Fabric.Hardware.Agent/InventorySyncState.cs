using System.Text.Json;
using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent;

public sealed class InventorySyncState(TimeSpan fullSyncInterval)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TimeSpan _fullSyncInterval = fullSyncInterval;
    private string? _lastSentFingerprint;
    private DateTimeOffset? _lastSentAt;

    public InventorySyncDecision GetDecision(IReadOnlyList<HardwareDeviceInventoryItem> devices, DateTimeOffset observedAt)
    {
        string fingerprint = CreateFingerprint(devices);
        bool inventoryChanged = _lastSentFingerprint is null
            || !string.Equals(_lastSentFingerprint, fingerprint, StringComparison.Ordinal);
        bool fullSyncDue = _lastSentAt is null || observedAt - _lastSentAt.Value >= _fullSyncInterval;

        return new InventorySyncDecision(inventoryChanged || fullSyncDue, fingerprint, inventoryChanged, fullSyncDue);
    }

    public void MarkSent(InventorySyncDecision decision, DateTimeOffset sentAt)
    {
        _lastSentFingerprint = decision.Fingerprint;
        _lastSentAt = sentAt;
    }

    private static string CreateFingerprint(IReadOnlyList<HardwareDeviceInventoryItem> devices)
    {
        InventorySnapshotItem[] normalizedDevices = [
            .. devices
                .Select(device => new InventorySnapshotItem(
                    Normalize(device.DeviceId),
                    Normalize(device.Kind),
                    Normalize(device.Driver),
                    [.. device.Capabilities.Select(Normalize).OrderBy(capability => capability, StringComparer.Ordinal)],
                    Normalize(device.State),
                    new InventorySnapshotDiagnostics(
                        NormalizeNullable(device.Diagnostics.Connection),
                        device.Diagnostics.Configured,
                        device.Diagnostics.Detected,
                        NormalizeNullable(device.Diagnostics.Platform))))
                .OrderBy(device => device.DeviceId, StringComparer.Ordinal)
        ];

        return JsonSerializer.Serialize(normalizedDevices, JsonOptions);
    }

    private static string Normalize(string value) => value.Trim();

    private static string? NormalizeNullable(string? value) => value?.Trim();

    private sealed record InventorySnapshotItem(
        string DeviceId,
        string Kind,
        string Driver,
        IReadOnlyList<string> Capabilities,
        string State,
        InventorySnapshotDiagnostics Diagnostics);

    private sealed record InventorySnapshotDiagnostics(
        string? Connection,
        bool Configured,
        bool Detected,
        string? Platform);
}

public sealed record InventorySyncDecision(
    bool ShouldSend,
    string Fingerprint,
    bool InventoryChanged,
    bool FullSyncDue);

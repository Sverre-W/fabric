using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent.Tests;

public sealed class InventorySyncStateTests
{
    [Fact]
    public void GetDecision_WhenInventoryNeverSent_ReturnsSendDecision()
    {
        var state = new InventorySyncState(TimeSpan.FromMinutes(15));

        InventorySyncDecision decision = state.GetDecision([CreateDevice("qr-1", "online")], new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero));

        decision.ShouldSend.ShouldBeTrue();
        decision.InventoryChanged.ShouldBeTrue();
        decision.FullSyncDue.ShouldBeTrue();
    }

    [Fact]
    public void GetDecision_WhenInventoryUnchangedBeforeFullSyncInterval_ReturnsSkipDecision()
    {
        var state = new InventorySyncState(TimeSpan.FromMinutes(15));
        DateTimeOffset sentAt = new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);
        InventorySyncDecision initialDecision = state.GetDecision([CreateDevice("qr-1", "online")], sentAt);
        state.MarkSent(initialDecision, sentAt);

        InventorySyncDecision nextDecision = state.GetDecision([CreateDevice("qr-1", "online")], sentAt.AddSeconds(30));

        nextDecision.ShouldSend.ShouldBeFalse();
        nextDecision.InventoryChanged.ShouldBeFalse();
        nextDecision.FullSyncDue.ShouldBeFalse();
    }

    [Fact]
    public void GetDecision_WhenInventoryChangesBeforeFullSyncInterval_ReturnsSendDecision()
    {
        var state = new InventorySyncState(TimeSpan.FromMinutes(15));
        DateTimeOffset sentAt = new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);
        InventorySyncDecision initialDecision = state.GetDecision([CreateDevice("qr-1", "online")], sentAt);
        state.MarkSent(initialDecision, sentAt);

        InventorySyncDecision nextDecision = state.GetDecision([CreateDevice("qr-1", "offline")], sentAt.AddSeconds(30));

        nextDecision.ShouldSend.ShouldBeTrue();
        nextDecision.InventoryChanged.ShouldBeTrue();
        nextDecision.FullSyncDue.ShouldBeFalse();
    }

    [Fact]
    public void GetDecision_WhenFullSyncIntervalElapsed_ReturnsSendDecision()
    {
        var state = new InventorySyncState(TimeSpan.FromMinutes(15));
        DateTimeOffset sentAt = new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);
        InventorySyncDecision initialDecision = state.GetDecision([CreateDevice("qr-1", "online")], sentAt);
        state.MarkSent(initialDecision, sentAt);

        InventorySyncDecision nextDecision = state.GetDecision([CreateDevice("qr-1", "online")], sentAt.AddMinutes(15));

        nextDecision.ShouldSend.ShouldBeTrue();
        nextDecision.InventoryChanged.ShouldBeFalse();
        nextDecision.FullSyncDue.ShouldBeTrue();
    }

    [Fact]
    public void GetDecision_WhenInventoryOnlyDiffersByOrderingAndWhitespace_ReturnsSkipDecision()
    {
        var state = new InventorySyncState(TimeSpan.FromMinutes(15));
        DateTimeOffset sentAt = new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);
        InventorySyncDecision initialDecision = state.GetDecision(
        [
            new HardwareDeviceInventoryItem(
                " qr-1 ",
                "qr-reader",
                "serial",
                ["scan", "notify"],
                "online",
                new HardwareDeviceDiagnostics(" COM3 ", Configured: true, Detected: true, Platform: " Linux ")),
            new HardwareDeviceInventoryItem(
                "qr-2",
                "qr-reader",
                "serial",
                ["scan"],
                "offline",
                new HardwareDeviceDiagnostics("COM4", Configured: true, Detected: false, Platform: "Linux"))
        ],
        sentAt);
        state.MarkSent(initialDecision, sentAt);

        InventorySyncDecision nextDecision = state.GetDecision(
        [
            new HardwareDeviceInventoryItem(
                "qr-2",
                "qr-reader",
                "serial",
                ["scan"],
                "offline",
                new HardwareDeviceDiagnostics("COM4", Configured: true, Detected: false, Platform: "Linux")),
            new HardwareDeviceInventoryItem(
                "qr-1",
                "qr-reader",
                "serial",
                ["notify", "scan"],
                "online",
                new HardwareDeviceDiagnostics("COM3", Configured: true, Detected: true, Platform: "Linux"))
        ],
        sentAt.AddSeconds(30));

        nextDecision.ShouldSend.ShouldBeFalse();
        nextDecision.InventoryChanged.ShouldBeFalse();
        nextDecision.FullSyncDue.ShouldBeFalse();
    }

    private static HardwareDeviceInventoryItem CreateDevice(string deviceId, string state) => new(
        deviceId,
        "qr-reader",
        "serial",
        ["scan"],
        state,
        new HardwareDeviceDiagnostics("COM3", Configured: true, Detected: state == "online", Platform: "Linux"));
}

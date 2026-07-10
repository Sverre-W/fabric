using Fabric.Hardware.Contracts;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Application;

public sealed class KioskDeviceResolver(KioskDbContext kioskDb, HardwareDbContext hardwareDb)
{
    public async Task<KioskDeviceResolution?> ResolveAsync(Guid kioskId, KioskDeviceType expectedType, int slotNumber, CancellationToken cancellationToken)
        => (await ResolveDetailedAsync(kioskId, expectedType, slotNumber, cancellationToken)).Resolution;

    public async Task<KioskDeviceResolutionResult> ResolveDetailedAsync(Guid kioskId, KioskDeviceType expectedType, int slotNumber, CancellationToken cancellationToken)
    {
        KioskDevice? kioskDevice = await kioskDb.Devices.AsNoTracking().SingleOrDefaultAsync(device => device.KioskId == kioskId && device.Type == expectedType && device.SlotNumber == slotNumber, cancellationToken);
        if (kioskDevice is null)
        {
            bool slotExistsForOtherType = await kioskDb.Devices.AsNoTracking().AnyAsync(device => device.KioskId == kioskId && device.SlotNumber == slotNumber, cancellationToken);
            return slotExistsForOtherType
                ? new KioskDeviceResolutionResult(null, $"Slot {slotNumber} exists, but not for device type {expectedType}.")
                : new KioskDeviceResolutionResult(null, $"No kiosk device is configured for {expectedType} slot {slotNumber}.");
        }

        if (!kioskDevice.Enabled)
            return new KioskDeviceResolutionResult(null, $"Kiosk device {expectedType} slot {slotNumber} is disabled.");

        IReadOnlyList<string> requiredCapabilities = KioskDeviceCapabilities.GetRequiredCapabilities(expectedType);
        HardwareDevice? device = await ResolveHardwareDeviceAsync(kioskDevice, cancellationToken);
        if (device is null)
            return new KioskDeviceResolutionResult(null, $"Mapped hardware device {FormatHardwareRef(kioskDevice.AgentId, kioskDevice.DeviceId)} does not exist.");

        if (!device.Enabled)
            return new KioskDeviceResolutionResult(null, $"Mapped hardware device {FormatHardwareRef(kioskDevice.AgentId, kioskDevice.DeviceId)} is disabled.");

        if (!SupportsCapabilities(device.Capabilities, requiredCapabilities))
        {
            string required = string.Join(", ", requiredCapabilities);
            string actual = string.Join(", ", device.Capabilities);
            return new KioskDeviceResolutionResult(null, $"Mapped hardware device {FormatHardwareRef(kioskDevice.AgentId, kioskDevice.DeviceId)} does not satisfy {expectedType} slot {slotNumber}. Required: {required}. Actual: {actual}.");
        }

        HardwareAgent? agent = await hardwareDb.Agents.AsNoTracking().SingleOrDefaultAsync(agent => agent.Id == kioskDevice.AgentId, cancellationToken);
        if (agent is null)
            return new KioskDeviceResolutionResult(null, $"Mapped hardware agent {kioskDevice.AgentId} does not exist.");

        if (!agent.Enabled)
            return new KioskDeviceResolutionResult(null, $"Mapped hardware agent {kioskDevice.AgentId} is disabled.");

        return new KioskDeviceResolutionResult(new KioskDeviceResolution(kioskDevice.Name, kioskDevice.Type, kioskDevice.SlotNumber, new HardwareDeviceRef(kioskDevice.AgentId, kioskDevice.DeviceId), requiredCapabilities), null);
    }

    private static bool SupportsCapabilities(IReadOnlyList<string> actualCapabilities, IReadOnlyList<string> requiredCapabilities) =>
        requiredCapabilities.All(requiredCapability => actualCapabilities.Contains(requiredCapability, StringComparer.OrdinalIgnoreCase));

    private async Task<HardwareDevice?> ResolveHardwareDeviceAsync(KioskDevice kioskDevice, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(kioskDevice.AgentId))
            return await hardwareDb.Devices.AsNoTracking().SingleOrDefaultAsync(device => device.AgentId == kioskDevice.AgentId && device.DeviceId == kioskDevice.DeviceId, cancellationToken);

        List<HardwareDevice> candidates = await hardwareDb.Devices.AsNoTracking().Where(device => device.DeviceId == kioskDevice.DeviceId).ToListAsync(cancellationToken);
        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static string FormatHardwareRef(string agentId, string deviceId) =>
        string.IsNullOrWhiteSpace(agentId) ? $"<missing-agent>/{deviceId}" : $"{agentId}/{deviceId}";
}

public sealed record KioskDeviceResolution(string Name, KioskDeviceType Type, int SlotNumber, HardwareDeviceRef Device, IReadOnlyList<string> RequiredCapabilities);

public sealed record KioskDeviceResolutionResult(KioskDeviceResolution? Resolution, string? ErrorMessage);

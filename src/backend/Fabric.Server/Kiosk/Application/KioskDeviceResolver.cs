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
    {
        KioskDevice? kioskDevice = await kioskDb.Devices.AsNoTracking().SingleOrDefaultAsync(device => device.KioskId == kioskId && device.Type == expectedType && device.SlotNumber == slotNumber, cancellationToken);
        if (kioskDevice is null || !kioskDevice.Enabled)
            return null;

        string requiredCapability = KioskDeviceCapabilities.GetRequiredCapability(expectedType);
        HardwareDevice? device = await hardwareDb.Devices.AsNoTracking().SingleOrDefaultAsync(device => device.AgentId == kioskDevice.AgentId && device.DeviceId == kioskDevice.DeviceId, cancellationToken);
        if (device is null || !device.Enabled || !device.Capabilities.Contains(requiredCapability, StringComparer.OrdinalIgnoreCase))
            return null;

        HardwareAgent? agent = await hardwareDb.Agents.AsNoTracking().SingleOrDefaultAsync(agent => agent.Id == kioskDevice.AgentId, cancellationToken);
        if (agent is null || !agent.Enabled)
            return null;

        return new KioskDeviceResolution(kioskDevice.Name, kioskDevice.Type, kioskDevice.SlotNumber, new HardwareDeviceRef(kioskDevice.AgentId, kioskDevice.DeviceId), requiredCapability);
    }
}

public sealed record KioskDeviceResolution(string Name, KioskDeviceType Type, int SlotNumber, HardwareDeviceRef Device, string RequiredCapability);

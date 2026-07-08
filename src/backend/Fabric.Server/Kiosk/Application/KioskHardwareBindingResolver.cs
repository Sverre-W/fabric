using Fabric.Hardware.Contracts;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Application;

public sealed class KioskHardwareBindingResolver(KioskDbContext kioskDb, HardwareDbContext hardwareDb)
{
    public async Task<KioskHardwareBindingResolution?> ResolveAsync(Guid kioskId, string bindingKey, CancellationToken cancellationToken)
    {
        Domain.Kiosk? kiosk = await kioskDb.Kiosks.AsNoTracking().SingleOrDefaultAsync(kiosk => kiosk.Id == kioskId, cancellationToken);
        if (kiosk is null)
            return null;

        string normalizedBindingKey = KioskHardwareBinding.NormalizeBindingKey(bindingKey);
        KioskHardwareBinding? binding = await kioskDb.HardwareBindings
            .AsNoTracking()
            .SingleOrDefaultAsync(binding => binding.ProfileId == kiosk.ProfileId && binding.BindingKey == normalizedBindingKey, cancellationToken);

        if (binding is null)
            return null;

        KioskDeviceAssignment? assignment = await kioskDb.DeviceAssignments
            .AsNoTracking()
            .Where(assignment => assignment.KioskId == kioskId && assignment.BindingKey == normalizedBindingKey && assignment.Enabled)
            .OrderBy(assignment => assignment.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
            return null;

        HardwareDevice? device = await hardwareDb.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(device => device.AgentId == assignment.AgentId && device.DeviceId == assignment.DeviceId, cancellationToken);

        if (device is null || !device.Enabled || !device.Capabilities.Contains(binding.RequiredCapability, StringComparer.OrdinalIgnoreCase))
            return null;

        HardwareAgent? agent = await hardwareDb.Agents
            .AsNoTracking()
            .SingleOrDefaultAsync(agent => agent.Id == assignment.AgentId, cancellationToken);

        if (agent is null || !agent.Enabled)
            return null;

        return new KioskHardwareBindingResolution(new HardwareDeviceRef(assignment.AgentId, assignment.DeviceId), binding.RequiredCapability);
    }
}

public sealed record KioskHardwareBindingResolution(HardwareDeviceRef Device, string RequiredCapability);

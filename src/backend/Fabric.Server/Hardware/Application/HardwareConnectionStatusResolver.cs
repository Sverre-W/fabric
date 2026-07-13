using Fabric.Server.Hardware.Contracts;
using Fabric.Server.Hardware.Domain;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Hardware.Application;

public sealed class HardwareConnectionStatusResolver(TimeProvider timeProvider, IOptions<HardwareConnectionOptions> options)
{
    private readonly HardwareConnectionOptions _options = options.Value;

    public HardwareConnectionStatus GetStatus(DateTimeOffset? lastSeenAt)
    {
        if (lastSeenAt is null)
            return HardwareConnectionStatus.Offline;

        TimeSpan age = timeProvider.GetUtcNow() - lastSeenAt.Value;
        if (age <= _options.StaleAfter)
            return HardwareConnectionStatus.Online;

        return age <= _options.OfflineAfter
            ? HardwareConnectionStatus.Stale
            : HardwareConnectionStatus.Offline;
    }

    public bool IsDeviceAvailable(HardwareDevice device) => GetDeviceAvailabilityReason(device) is null;

    public HardwareDeviceAvailabilityReason? GetDeviceAvailabilityReason(HardwareDevice device)
    {
        if (!device.Enabled)
            return HardwareDeviceAvailabilityReason.DeviceDisabled;

        return string.Equals(device.State, "online", StringComparison.OrdinalIgnoreCase)
            ? null
            : HardwareDeviceAvailabilityReason.DeviceOffline;
    }
}

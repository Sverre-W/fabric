using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Application;

public sealed class KioskSessionCleanupService(KioskDbContext db, ILogger<KioskSessionCleanupService> logger)
{
    public async Task CleanupAsync(Guid kioskId, CancellationToken cancellationToken)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.AsNoTracking().SingleOrDefaultAsync(kiosk => kiosk.Id == kioskId, cancellationToken);
        if (kiosk is null)
            return;

        KioskDevice[] cleanupDevices = await db.Devices
            .AsNoTracking()
            .Where(device => device.KioskId == kiosk.Id && device.Type == KioskDeviceType.Collector && device.Enabled && device.CleanupOnSessionEnd)
            .ToArrayAsync(cancellationToken);

        if (cleanupDevices.Length == 0)
            return;

        logger.KioskCleanupPending(kioskId, cleanupDevices.Length);
        // Hardware cleanup commands are intentionally deferred until collector capability is added to hardware contracts.
    }
}

internal static partial class KioskSessionCleanupLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Kiosk {KioskId} has {BindingCount} cleanup kiosk devices pending cleanup")]
    public static partial void KioskCleanupPending(this ILogger logger, Guid kioskId, int bindingCount);
}

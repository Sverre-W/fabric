using Fabric.Server.Desfire.Application;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Fabric.Server.Sagas.Kiosk;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Application;

public sealed class KioskSessionCancellationService(
    KioskDbContext db,
    KioskSagaService kioskSagaService,
    KioskSessionCleanupService cleanupService,
    DesfireEncodingService desfireEncodingService,
    ILogger<KioskSessionCancellationService> logger)
{
    public async Task<KioskSession?> CancelActiveSessionAsync(Guid kioskId, KioskSessionCancellationSource source, CancellationToken cancellationToken)
    {
        KioskSession? session = await db.Sessions
            .Where(candidate => candidate.KioskId == kioskId && (candidate.Status == KioskSessionStatus.Starting || candidate.Status == KioskSessionStatus.Running))
            .OrderByDescending(candidate => candidate.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return null;

        return await CancelSessionAsync(session.Id, source, cancellationToken);
    }

    public async Task<KioskSession?> CancelSessionAsync(Guid sessionId, KioskSessionCancellationSource source, CancellationToken cancellationToken)
    {
        KioskSession? session = await db.Sessions.SingleOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);
        if (session is null)
            return null;

        KioskSession cancelledSession = await kioskSagaService.CancelSessionAsync(session.Id, source, cancellationToken);
        int cancelledRunCount = await desfireEncodingService.CancelRunsForKioskSessionAsync(session.Id, cancellationToken);
        await cleanupService.CleanupAsync(cancelledSession.KioskId, cancellationToken);
        logger.KioskSessionCancelled(cancelledSession.KioskId, cancelledSession.Id, cancelledRunCount);
        return cancelledSession;
    }
}

internal static partial class KioskSessionCancellationServiceLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Cancelled kiosk session {SessionId} for kiosk {KioskId}; cancelled {CancelledRunCount} DESFire runs")]
    public static partial void KioskSessionCancelled(this ILogger logger, Guid kioskId, Guid sessionId, int cancelledRunCount);
}

using System.Text.Json;
using Elsa.Workflows;
using Fabric.Server.Kiosk.Contracts;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskInstructionWriter(KioskDbContext db, TimeProvider timeProvider)
{
    public async Task<KioskInstructionBookmark> WriteInstructionAsync(
        ActivityExecutionContext context,
        KioskSession session,
        string type,
        string mode,
        string? backgroundUrl,
        string? imageUrl,
        string? title,
        string? titleKey,
        string? message,
        string? messageKey,
        IReadOnlyDictionary<string, string>? theme,
        IReadOnlyList<KioskChoiceOption>? choices,
        IReadOnlyList<KioskFormField>? fields,
        CancellationToken cancellationToken)
    {
        string instructionId = Guid.NewGuid().ToString("N");
        var envelope = new KioskInstructionEnvelope(
            instructionId,
            session.CurrentInstructionVersion + 1,
            type,
            session.LanguageCode,
            new KioskInstructionLayout(mode, backgroundUrl, imageUrl),
            new KioskInstructionContent(title, titleKey, message, messageKey),
            theme ?? new Dictionary<string, string>(),
            choices ?? [],
            fields ?? []);

        string instructionJson = JsonSerializer.Serialize(envelope);
        session.SetInstruction(instructionId, instructionJson, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return new KioskInstructionBookmark(session.Id, instructionId);
    }
}

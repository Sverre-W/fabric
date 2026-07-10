using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Fabric.Server.Sagas;
using Fabric.Server.Sagas.Kiosk;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Application;

public sealed class KioskInstructionService(KioskDbContext db, SagasDbContext sagaDb, KioskSagaTrigger sagaTrigger, TimeProvider timeProvider)
{
    public async Task<Automation.Kiosk.KioskInstructionBookmark> ShowInstructionAsync(Guid sessionId, KioskInstructionDefinition definition, CancellationToken cancellationToken)
    {
        KioskSession session = await GetRequiredSessionAsync(sessionId, cancellationToken);
        Domain.Kiosk kiosk = await db.Kiosks.AsNoTracking().SingleAsync(kiosk => kiosk.Id == session.KioskId, cancellationToken);
        KioskProfile profile = await db.Profiles.AsNoTracking().SingleAsync(profile => profile.Id == kiosk.ProfileId, cancellationToken);
        Dictionary<string, string> translations = await LoadTranslationsAsync(profile, session.LanguageCode, cancellationToken);
        string instructionId = Guid.NewGuid().ToString("N");
        int version = session.CurrentInstructionVersion + 1;
        KioskInstruction instruction = definition.Kind switch
        {
            KioskInstructionActivityKind.Choice => new KioskChoiceInstruction(
                instructionId,
                version,
                definition.Type,
                session.LanguageCode,
                definition.Layout,
                ResolveContent(definition.Content, translations),
                [.. definition.Choices.Select(choice => new KioskChoiceOption(choice, ResolveText(choice, translations) ?? choice))]),
            KioskInstructionActivityKind.Form => new KioskFormInstruction(
                instructionId,
                version,
                definition.Type,
                session.LanguageCode,
                definition.Layout,
                ResolveContent(definition.Content, translations),
                [.. definition.Fields.Select(field => new KioskFormField(field.Name, ResolveText(field.Label, translations) ?? field.Name, ResolveText(field.Placeholder, translations), field.IsRequired, field.IsMaskRequired))]),
            _ => throw new InvalidOperationException($"Unsupported kiosk instruction kind '{definition.Kind}'.")
        };

        session.SetInstruction(instruction.InstructionId, KioskInstructionJsonSerializer.Serialize(instruction), timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return new Automation.Kiosk.KioskInstructionBookmark(session.Id, instruction.InstructionId, instruction.Kind);
    }

    public async Task SubmitInstructionAsync(Guid sessionId, string instructionId, IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken)
    {
        KioskSession session = await GetRequiredSessionAsync(sessionId, cancellationToken);
        KioskInstruction instruction = GetRequiredCurrentInstruction(session, instructionId);
        KioskSaga saga = await sagaDb.KioskSagas.SingleAsync(x => x.SessionId == session.Id, cancellationToken);
        sagaDb.KioskSagaEvents.Add(KioskSagaEvent.Create(saga.Id, KioskSagaEventType.InstructionCompleted, instruction.InstructionId, instruction.Kind, SerializeResult(instruction.CreateResult(values)), timeProvider.GetUtcNow()));
        await sagaDb.SaveChangesAsync(cancellationToken);
        sagaTrigger.Notify();
    }

    public async Task ShowMessageAsync(Guid sessionId, KioskInstructionLayout layout, KioskInstructionContent content, CancellationToken cancellationToken)
    {
        KioskSession session = await GetRequiredSessionAsync(sessionId, cancellationToken);
        Domain.Kiosk kiosk = await db.Kiosks.AsNoTracking().SingleAsync(kiosk => kiosk.Id == session.KioskId, cancellationToken);
        KioskProfile profile = await db.Profiles.AsNoTracking().SingleAsync(profile => profile.Id == kiosk.ProfileId, cancellationToken);
        Dictionary<string, string> translations = await LoadTranslationsAsync(profile, session.LanguageCode, cancellationToken);
        string instructionId = Guid.NewGuid().ToString("N");
        int version = session.CurrentInstructionVersion + 1;
        KioskInstruction instruction = new KioskMessageInstruction(
            instructionId,
            version,
            "display-message",
            session.LanguageCode,
            layout,
            ResolveContent(content, translations));

        session.SetInstruction(instruction.InstructionId, KioskInstructionJsonSerializer.Serialize(instruction), timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelInstructionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        KioskSession session = await GetRequiredSessionAsync(sessionId, cancellationToken);
        if (string.IsNullOrWhiteSpace(session.CurrentInstructionJson) || string.IsNullOrWhiteSpace(session.CurrentInstructionId))
            return;

        KioskInstruction instruction = KioskInstructionJsonSerializer.Deserialize(session.CurrentInstructionJson);
        KioskSaga saga = await sagaDb.KioskSagas.SingleAsync(x => x.SessionId == session.Id, cancellationToken);
        sagaDb.KioskSagaEvents.Add(KioskSagaEvent.Create(saga.Id, KioskSagaEventType.InstructionCancelled, instruction.InstructionId, instruction.Kind, null, timeProvider.GetUtcNow()));
        await sagaDb.SaveChangesAsync(cancellationToken);
        sagaTrigger.Notify();
    }

    public async Task ClearCurrentInstructionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        KioskSession session = await GetRequiredSessionAsync(sessionId, cancellationToken);
        if (string.IsNullOrWhiteSpace(session.CurrentInstructionId))
            return;

        session.ClearInstruction(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<KioskSession> GetRequiredSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        KioskSession? session = await db.Sessions.SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);
        return session ?? throw new InvalidOperationException($"Kiosk session '{sessionId}' not found.");
    }

    private static KioskInstructionContent ResolveContent(KioskInstructionContent content, IReadOnlyDictionary<string, string> translations) =>
        new(ResolveText(content.Title, translations), ResolveText(content.Message, translations));

    private async Task<Dictionary<string, string>> LoadTranslationsAsync(KioskProfile profile, string languageCode, CancellationToken cancellationToken)
    {
        KioskTranslation[] translationRows = await db.Translations
            .AsNoTracking()
            .Where(translation => translation.ProfileId == profile.Id && (translation.LanguageCode == languageCode || translation.LanguageCode == profile.DefaultLanguageCode))
            .OrderBy(translation => translation.LanguageCode == languageCode ? 0 : 1)
            .ToArrayAsync(cancellationToken);

        Dictionary<string, string> translations = [];
        foreach (KioskTranslation translation in translationRows)
            translations.TryAdd(translation.Key, translation.Value);

        return translations;
    }

    private static KioskInstruction GetRequiredCurrentInstruction(KioskSession session, string instructionId)
    {
        if (!string.Equals(session.CurrentInstructionId, instructionId, StringComparison.Ordinal))
            throw new InvalidOperationException("Instruction is stale.");

        if (string.IsNullOrWhiteSpace(session.CurrentInstructionJson))
            throw new InvalidOperationException($"Instruction '{instructionId}' payload not found.");

        return KioskInstructionJsonSerializer.Deserialize(session.CurrentInstructionJson);
    }

    private static string? ResolveText(string? value, IReadOnlyDictionary<string, string> translations)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string trimmedValue = value.Trim();
        return translations.TryGetValue(trimmedValue, out string? translatedValue) ? translatedValue : trimmedValue;
    }

    private static string SerializeResult(KioskInstructionResult result) => result switch
    {
        KioskChoiceInstructionResult choice => System.Text.Json.JsonSerializer.Serialize(choice, KioskJsonSerializerContext.Default.KioskChoiceInstructionResult),
        KioskFormInstructionResult form => System.Text.Json.JsonSerializer.Serialize(form, KioskJsonSerializerContext.Default.KioskFormInstructionResult),
        _ => throw new InvalidOperationException($"Unsupported kiosk instruction result type '{result.GetType().Name}'.")
    };
}

using System.Security.Claims;
using Fabric.Server.Infrastructure.Authentication;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Contracts;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Endpoints;

public static class KioskRuntimeEndpoints
{
    public static IEndpointRouteBuilder MapKioskRuntimeEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder kiosk = app.MapGroup("/api/kiosk").RequireAuthorization(KioskAuthenticationDefaults.Policy);

        kiosk.MapGet("/config", GetKioskConfig).Produces<KioskConfigResponse>().Produces(StatusCodes.Status404NotFound);
        kiosk.MapPost("/heartbeat", PostKioskHeartbeat).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
        kiosk.MapPost("/language", ChangeKioskLanguage).Produces<KioskSessionResponse>().Produces(StatusCodes.Status404NotFound);
        kiosk.MapPost("/sessions", StartKioskSession).Produces<KioskSessionResponse>().Produces(StatusCodes.Status409Conflict);
        kiosk.MapGet("/sessions/current/instruction", GetCurrentInstruction).Produces<KioskInstructionResponse>().Produces(StatusCodes.Status404NotFound);
        kiosk.MapPost("/sessions/current/instructions/{instructionId}/response", SubmitInstructionResponse).Produces<KioskSessionResponse>().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        kiosk.MapPost("/sessions/current/cancel", CancelCurrentSession).Produces<KioskSessionResponse>().Produces(StatusCodes.Status404NotFound);
        kiosk.MapGet("/assets/{assetName}", GetKioskAsset).Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetKioskConfig([FromQuery] string? languageCode, HttpContext context, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        Guid kioskId = context.User.GetKioskId();
        Domain.Kiosk? kiosk = await db.Kiosks.AsNoTracking().SingleOrDefaultAsync(kiosk => kiosk.Id == kioskId, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        KioskProfile? profile = await db.Profiles.AsNoTracking().SingleOrDefaultAsync(profile => profile.Id == kiosk.ProfileId, cancellationToken);
        if (profile is null)
            return Results.NotFound();

        KioskProfileLanguage[] languages = await db.Languages.AsNoTracking().Where(language => language.ProfileId == profile.Id).OrderBy(language => language.SortOrder).ToArrayAsync(cancellationToken);
        KioskWelcomeSettings? welcome = await db.WelcomeSettings.AsNoTracking().SingleOrDefaultAsync(settings => settings.ProfileId == profile.Id, cancellationToken);
        Dictionary<string, string> theme = await db.ThemeTokens.AsNoTracking().Where(token => token.ProfileId == profile.Id).ToDictionaryAsync(token => token.Key, token => token.Value, cancellationToken);
        ResolvedKioskWelcomeResponse? resolvedWelcome = welcome is null ? null : await ResolveWelcomeAsync(welcome, profile, languageCode, db, cancellationToken);

        return Results.Ok(new KioskConfigResponse(kiosk.ToResponse(), profile.ToResponse(), languages.Select(language => language.ToResponse()).ToArray(), welcome?.ToResponse(), resolvedWelcome, theme));
    }

    private static async Task<ResolvedKioskWelcomeResponse> ResolveWelcomeAsync(KioskWelcomeSettings welcome, KioskProfile profile, string? languageCode, KioskDbContext db, CancellationToken cancellationToken)
    {
        string resolvedLanguage = string.IsNullOrWhiteSpace(languageCode) ? profile.DefaultLanguageCode : languageCode;
        KioskTranslation[] translationRows = await db.Translations
            .AsNoTracking()
            .Where(translation => translation.ProfileId == profile.Id && (translation.LanguageCode == resolvedLanguage || translation.LanguageCode == profile.DefaultLanguageCode))
            .OrderBy(translation => translation.LanguageCode == resolvedLanguage ? 0 : 1)
            .ToArrayAsync(cancellationToken);
        Dictionary<string, string> translations = [];
        foreach (KioskTranslation translation in translationRows)
            translations.TryAdd(translation.Key, translation.Value);

        string? backgroundUrl = string.IsNullOrWhiteSpace(welcome.BackgroundAssetName) ? null : $"/api/kiosk/assets/{Uri.EscapeDataString(welcome.BackgroundAssetName)}?languageCode={Uri.EscapeDataString(resolvedLanguage)}";
        string? logoUrl = string.IsNullOrWhiteSpace(welcome.LogoAssetName) ? null : $"/api/kiosk/assets/{Uri.EscapeDataString(welcome.LogoAssetName)}?languageCode={Uri.EscapeDataString(resolvedLanguage)}";

        return new ResolvedKioskWelcomeResponse(
            ResolveText(welcome.TitleKey, translations),
            welcome.SubtitleKey is null ? null : ResolveText(welcome.SubtitleKey, translations),
            ResolveText(welcome.StartButtonKey, translations),
            backgroundUrl,
            logoUrl);
    }

    private static string ResolveText(string key, IReadOnlyDictionary<string, string> translations) => translations.TryGetValue(key, out string? value) ? value : key;

    private static async Task<IResult> PostKioskHeartbeat([FromBody] KioskHeartbeatRequest request, HttpContext context, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await GetAuthenticatedKioskAsync(context, db, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        kiosk.MarkSeen(request.ReportedAt);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> StartKioskSession([FromBody] StartKioskSessionRequest request, HttpContext context, KioskDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await GetAuthenticatedKioskAsync(context, db, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        if (kiosk.Mode != KioskMode.Active)
            return Results.Problem("Kiosk is not active.", statusCode: StatusCodes.Status409Conflict);

        KioskSession? existing = await db.Sessions
            .Where(session => session.KioskId == kiosk.Id && session.Status == KioskSessionStatus.Running)
            .OrderByDescending(session => session.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
            return Results.Ok(existing.ToResponse());

        KioskProfile profile = await db.Profiles.AsNoTracking().SingleAsync(profile => profile.Id == kiosk.ProfileId, cancellationToken);
        string languageCode = string.IsNullOrWhiteSpace(request.LanguageCode) ? profile.DefaultLanguageCode : request.LanguageCode;
        KioskSession session = KioskSession.Start(kiosk.Id, languageCode, timeProvider.GetUtcNow());
        db.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(session.ToResponse());
    }

    private static async Task<IResult> ChangeKioskLanguage([FromBody] ChangeKioskLanguageRequest request, HttpContext context, KioskDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        KioskSession? session = await GetCurrentSessionAsync(context.User.GetKioskId(), db, cancellationToken);
        if (session is null)
            return Results.NotFound();

        session.ChangeLanguage(request.LanguageCode, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(session.ToResponse());
    }

    private static async Task<IResult> GetCurrentInstruction([FromQuery] int? sinceVersion, HttpContext context, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        Guid kioskId = context.User.GetKioskId();
        KioskSession? session = await GetCurrentSessionAsync(kioskId, db, cancellationToken);
        if (session is null)
            return Results.NotFound();

        if (sinceVersion.HasValue && sinceVersion.Value >= session.CurrentInstructionVersion && session.Status == KioskSessionStatus.Running)
        {
            using var delay = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, delay.Token);
            while (!linked.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), linked.Token).ContinueWith(_ => { }, CancellationToken.None);
                await db.Entry(session).ReloadAsync(cancellationToken);
                if (session.CurrentInstructionVersion > sinceVersion.Value || session.Status != KioskSessionStatus.Running)
                    break;
            }
        }

        return Results.Ok(new KioskInstructionResponse(session.Id, session.Status, session.CurrentInstructionVersion, session.CurrentInstructionId, session.CurrentInstructionJson));
    }

    private static async Task<IResult> SubmitInstructionResponse(string instructionId, [FromBody] SubmitKioskInstructionResponseRequest request, HttpContext context, KioskDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        KioskSession? session = await GetCurrentSessionAsync(context.User.GetKioskId(), db, cancellationToken);
        if (session is null)
            return Results.NotFound();

        if (!string.Equals(session.CurrentInstructionId, instructionId, StringComparison.Ordinal))
            return Results.Problem("Instruction is stale.", statusCode: StatusCodes.Status409Conflict);

        session.SetInstruction(Guid.NewGuid().ToString("N"), "{}", timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(session.ToResponse());
    }

    private static async Task<IResult> CancelCurrentSession(HttpContext context, KioskDbContext db, KioskSessionCleanupService cleanupService, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Guid kioskId = context.User.GetKioskId();
        KioskSession? session = await GetCurrentSessionAsync(kioskId, db, cancellationToken);
        if (session is null)
            return Results.NotFound();

        session.Cancel(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        await cleanupService.CleanupAsync(kioskId, cancellationToken);
        return Results.Ok(session.ToResponse());
    }

    private static async Task<IResult> GetKioskAsset(string assetName, [FromQuery] string? languageCode, HttpContext context, KioskDbContext db, IKioskAssetStorage storage, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.AsNoTracking().SingleOrDefaultAsync(kiosk => kiosk.Id == context.User.GetKioskId(), cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        KioskProfile profile = await db.Profiles.AsNoTracking().SingleAsync(profile => profile.Id == kiosk.ProfileId, cancellationToken);
        string resolvedLanguage = string.IsNullOrWhiteSpace(languageCode) ? profile.DefaultLanguageCode : languageCode;
        KioskAsset? asset = await db.Assets.AsNoTracking()
            .Where(asset => asset.ProfileId == profile.Id && asset.Name == assetName && (asset.LanguageCode == resolvedLanguage || asset.LanguageCode == profile.DefaultLanguageCode || asset.LanguageCode == null))
            .OrderByDescending(asset => asset.LanguageCode == resolvedLanguage)
            .ThenByDescending(asset => asset.LanguageCode == profile.DefaultLanguageCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (asset is null)
            return Results.NotFound();

        Stream? stream = await storage.OpenReadAsync(asset.RelativePath, cancellationToken);
        return stream is null ? Results.NotFound() : Results.File(stream, asset.ContentType, asset.FileName);
    }

    private static async Task<Domain.Kiosk?> GetAuthenticatedKioskAsync(HttpContext context, KioskDbContext db, CancellationToken cancellationToken) =>
        await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == context.User.GetKioskId(), cancellationToken);

    private static async Task<KioskSession?> GetCurrentSessionAsync(Guid kioskId, KioskDbContext db, CancellationToken cancellationToken) =>
        await db.Sessions.Where(session => session.KioskId == kioskId && session.Status == KioskSessionStatus.Running).OrderByDescending(session => session.StartedAt).FirstOrDefaultAsync(cancellationToken);

    private static Guid GetKioskId(this ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(KioskAuthenticationDefaults.KioskIdClaim)!);
}

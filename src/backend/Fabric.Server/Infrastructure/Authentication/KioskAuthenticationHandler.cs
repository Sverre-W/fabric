using System.Security.Claims;
using System.Text.Encodings.Web;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Infrastructure.Authentication;

public sealed class KioskAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    KioskDbContext db,
    KioskKeyHasher keyHasher)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string KioskIdHeader = "fabric-kiosk-id";
    private const string KioskKeyHeader = "fabric-kiosk-key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string kioskIdValue = Request.Headers[KioskIdHeader].ToString();
        string kioskKey = Request.Headers[KioskKeyHeader].ToString();

        if (string.IsNullOrWhiteSpace(kioskIdValue) && string.IsNullOrWhiteSpace(kioskKey))
            return AuthenticateResult.NoResult();

        if (!Guid.TryParse(kioskIdValue, out Guid kioskId))
            return AuthenticateResult.Fail("Kiosk id is invalid.");

        if (string.IsNullOrWhiteSpace(kioskKey))
            return AuthenticateResult.Fail("Kiosk key is required.");

        Fabric.Server.Kiosk.Domain.Kiosk? kiosk = await db.Kiosks
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == kioskId, Context.RequestAborted);

        if (kiosk is null)
            return AuthenticateResult.Fail("Kiosk credentials are invalid.");

        if (!keyHasher.Verify(kioskKey, kiosk.ApiKeyHash, kiosk.ApiKeySalt))
            return AuthenticateResult.Fail("Kiosk credentials are invalid.");

        Claim[] claims =
        [
            new(ClaimTypes.Role, KioskAuthenticationDefaults.Role),
            new(KioskAuthenticationDefaults.KioskIdClaim, kiosk.Id.ToString()),
            new(KioskAuthenticationDefaults.KioskNameClaim, kiosk.Name),
            new(KioskAuthenticationDefaults.KioskProfileIdClaim, kiosk.ProfileId.ToString())
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

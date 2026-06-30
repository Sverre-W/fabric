using System.Security.Claims;
using System.Text.Encodings.Web;
using Fabric.Server.Reception.Application;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Infrastructure.Authentication;

public sealed class ReceptionKioskAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ReceptionDbContext db,
    ReceptionKioskKeyHasher keyHasher)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string KioskIdHeader = "reception-kiosk-id";
    private const string KioskKeyHeader = "reception-kiosk-key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string kioskIdValue = Request.Headers[KioskIdHeader].ToString();
        string kioskKey = Request.Headers[KioskKeyHeader].ToString();

        if (string.IsNullOrWhiteSpace(kioskIdValue) && string.IsNullOrWhiteSpace(kioskKey))
            return AuthenticateResult.NoResult();

        if (!Guid.TryParse(kioskIdValue, out Guid kioskId))
            return AuthenticateResult.Fail("Reception kiosk id is invalid.");

        if (string.IsNullOrWhiteSpace(kioskKey))
            return AuthenticateResult.Fail("Reception kiosk key is required.");

        ReceptionKiosk? kiosk = await db.ReceptionKiosks
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == kioskId, Context.RequestAborted);

        if (kiosk is null || !kiosk.Enabled)
            return AuthenticateResult.Fail("Reception kiosk is not enabled.");

        if (!keyHasher.Verify(kioskKey, kiosk.ApiKeyHash, kiosk.ApiKeySalt))
            return AuthenticateResult.Fail("Reception kiosk credentials are invalid.");

        Claim[] claims =
        [
            new(ClaimTypes.Role, ReceptionKioskAuthenticationDefaults.Role),
            new(ReceptionKioskAuthenticationDefaults.KioskIdClaim, kiosk.Id.ToString()),
            new(ReceptionKioskAuthenticationDefaults.KioskNameClaim, kiosk.Name),
            new(ReceptionKioskAuthenticationDefaults.KioskLocationIdClaim, kiosk.LocationId.ToString())
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

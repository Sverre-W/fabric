using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fabric.Server.Actors.Application;

internal static class ActorClaimsPrincipalExtensions
{
    private const string ObjectIdClaim = "oid";
    private const string RoleClaim = "role";
    private const string GivenNameClaim = "given_name";
    private const string FamilyNameClaim = "family_name";

    public static string? GetObjectId(this ClaimsPrincipal principal) => Normalize(principal.FindFirstValue(ObjectIdClaim));

    public static string? GetSubjectId(this ClaimsPrincipal principal) => Normalize(principal.FindFirstValue(JwtRegisteredClaimNames.Sub));

    public static string? GetEmail(this ClaimsPrincipal principal) => Normalize(principal.FindFirstValue(JwtRegisteredClaimNames.Email));

    public static string? GetDisplayName(this ClaimsPrincipal principal) => Normalize(principal.Identity?.Name);

    public static string? GetFirstName(this ClaimsPrincipal principal) => Normalize(principal.FindFirstValue(GivenNameClaim));

    public static string? GetLastName(this ClaimsPrincipal principal) => Normalize(principal.FindFirstValue(FamilyNameClaim));

    public static string[] GetActorRoles(this ClaimsPrincipal principal) => principal.FindAll(RoleClaim)
        .Select(claim => Normalize(claim.Value))
        .Where(value => value is not null)
        .Select(value => value!)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(value => value, StringComparer.Ordinal)
        .ToArray();

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

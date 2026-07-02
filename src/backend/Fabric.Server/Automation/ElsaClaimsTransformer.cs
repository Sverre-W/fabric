using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Fabric.Server.Automation;


internal class ElsaClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is ClaimsIdentity { IsAuthenticated: true } claimsIdentity)
        {
            claimsIdentity.AddClaim(new Claim("permissions", "*"));
        }
        return Task.FromResult(principal);
    }
}


using System.Security.Claims;
using System.Text.Encodings.Web;
using Fabric.Server.Hardware.Application;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Infrastructure.Authentication;

public sealed class HardwareAgentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    HardwareDbContext db,
    HardwareAgentKeyHasher keyHasher)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string AgentIdHeader = "hardware-agent-id";
    private const string AgentKeyHeader = "hardware-agent-key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string agentId = Request.Headers[AgentIdHeader].ToString();
        string agentKey = Request.Headers[AgentKeyHeader].ToString();

        if (string.IsNullOrWhiteSpace(agentId) && string.IsNullOrWhiteSpace(agentKey))
            return AuthenticateResult.NoResult();

        if (string.IsNullOrWhiteSpace(agentId))
            return AuthenticateResult.Fail("Hardware agent id is required.");

        if (string.IsNullOrWhiteSpace(agentKey))
            return AuthenticateResult.Fail("Hardware agent key is required.");

        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        HardwareAgent? agent = await db.Agents
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == normalizedAgentId, Context.RequestAborted);

        if (agent is null || !agent.Enabled)
            return AuthenticateResult.Fail("Hardware agent is not enabled.");

        if (!keyHasher.Verify(agentKey, agent.ApiKeyHash, agent.ApiKeySalt))
            return AuthenticateResult.Fail("Hardware agent credentials are invalid.");

        List<Claim> claims =
        [
            new(ClaimTypes.Role, HardwareAgentAuthenticationDefaults.Role),
            new(HardwareAgentClaims.AgentIdClaim, agent.Id),
            new(HardwareAgentClaims.AgentNameClaim, agent.Name)
        ];

        if (agent.LocationId is not null)
            claims.Add(new Claim(HardwareAgentClaims.AgentLocationIdClaim, agent.LocationId.Value.ToString()));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

using System.Security.Claims;

namespace Fabric.Server.Hardware.Application;

public static class HardwareAgentClaims
{
    public const string AgentIdClaim = "hardware-agent-id";
    public const string AgentNameClaim = "hardware-agent-name";

    public static string GetAgentId(this ClaimsPrincipal user)
    {
        string? agentId = user.FindFirstValue(AgentIdClaim);
        if (string.IsNullOrWhiteSpace(agentId))
            throw new InvalidOperationException("Hardware agent id claim is missing.");

        return agentId;
    }
}

namespace Fabric.Server.Infrastructure.Authentication;

public static class FabricRoleDefaults
{
    public const string AdminRole = "Admin";
    public const string SecurityOfficerRole = "SecurityOfficer";

    public const string AdminPolicy = "AdminOnly";
    public const string SecurityOfficerPolicy = "SecurityOfficerOnly";
    public const string AdminOrSecurityOfficerPolicy = "AdminOrSecurityOfficer";
}

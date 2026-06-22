using System.Diagnostics;

namespace AccessControl.Unipass.Infrastructure;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("AccessControl.Unipass");
}

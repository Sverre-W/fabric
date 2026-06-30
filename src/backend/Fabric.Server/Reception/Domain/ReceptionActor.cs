namespace Fabric.Server.Reception.Domain;

public sealed class ReceptionActor
{
    private ReceptionActor() { }

    public ReceptionActorType Type { get; private set; }
    public string Identifier { get; private set; } = null!;
    public string? DisplayName { get; private set; }

    public static ReceptionActor Operator(string email, string? displayName = null) =>
        new()
        {
            Type = ReceptionActorType.Operator,
            Identifier = email,
            DisplayName = displayName
        };

    public static ReceptionActor Kiosk(Guid kioskId, string kioskName) =>
        new()
        {
            Type = ReceptionActorType.Kiosk,
            Identifier = kioskId.ToString(),
            DisplayName = kioskName
        };
}

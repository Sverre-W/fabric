namespace Fabric.Server.Reception.Domain;

public sealed class ArrivalEntry
{
    private ArrivalEntry() { }

    public Guid Id { get; private set; }
    public ArrivalEntryType Type { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public ReceptionActor? Actor { get; private set; }

    internal static ArrivalEntry CheckedIn(DateTimeOffset timestamp, ReceptionActor actor) =>
        new() { Id = Guid.NewGuid(), Type = ArrivalEntryType.CheckedIn, Timestamp = timestamp, Actor = actor };

    internal static ArrivalEntry CheckedOut(DateTimeOffset timestamp, ReceptionActor actor) =>
        new() { Id = Guid.NewGuid(), Type = ArrivalEntryType.CheckedOut, Timestamp = timestamp, Actor = actor };
}

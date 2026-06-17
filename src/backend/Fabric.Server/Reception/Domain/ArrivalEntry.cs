namespace Fabric.Server.Reception.Domain;

public sealed class ArrivalEntry
{
    private ArrivalEntry() { }

    public Guid Id { get; private set; }
    public ArrivalEntryType Type { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    internal static ArrivalEntry CheckedIn(DateTimeOffset timestamp) =>
        new() { Id = Guid.NewGuid(), Type = ArrivalEntryType.CheckedIn, Timestamp = timestamp };

    internal static ArrivalEntry CheckedOut(DateTimeOffset timestamp) =>
        new() { Id = Guid.NewGuid(), Type = ArrivalEntryType.CheckedOut, Timestamp = timestamp };
}
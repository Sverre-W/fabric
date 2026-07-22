namespace Fabric.Server.AccessControl.Domain;

public sealed record SystemMetadataObject(string Id, string Name);

public abstract record SystemMetadata;

public sealed record UnipassMetadata(
    IReadOnlyList<SystemMetadataObject> Sites,
    IReadOnlyList<SystemMetadataObject> AccessRules) : SystemMetadata;

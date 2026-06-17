namespace Fabric.Server.AccessPolicies.Domain;

using System.Text.Json.Serialization;

public sealed record SystemMetadataObject(string Id, string Name);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UnipassMetadata), "unipass")]
[JsonDerivedType(typeof(LenelMetadata), "lenel")]
public abstract record SystemMetadata;

public sealed record UnipassMetadata(
    List<SystemMetadataObject> Sites,
    List<SystemMetadataObject> AccessRules) : SystemMetadata;

public sealed record LenelMetadata(
    List<SystemMetadataObject> BadgeTypes,
    List<SystemMetadataObject> AccessLevels) : SystemMetadata;

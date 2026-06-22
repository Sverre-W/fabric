using System.Text.Json.Serialization;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.Core;

namespace Fabric.Server.AccessPolicies.Contracts;

public record ListAccessControlSystemsRequest : BaseListRequest
{
    public string? Name { get; set; }
}

public record ListIdentityMappingsRequest : BaseListRequest
{
    public string? Name { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CreateUnipassAccessControlSystemRequest), "unipass")]
[JsonDerivedType(typeof(CreateLenelAccessControlSystemRequest), "lenel")]
public abstract record CreateAccessControlSystemRequest
{
    public required string Name { get; init; }
    public required string Endpoint { get; init; }
    public required bool SslValidation { get; init; }
}

public sealed record CreateUnipassAccessControlSystemRequest : CreateAccessControlSystemRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed record CreateLenelAccessControlSystemRequest : CreateAccessControlSystemRequest
{
    public required string ApiKey { get; init; }
}

public sealed record UpdateUnipassConfigRequest(
    string Endpoint,
    bool SslValidation,
    string Username,
    string? Password);

public sealed record UpdateLenelConfigRequest(
    string Endpoint,
    bool SslValidation,
    string? ApiKey);

public sealed record AddUnipassBadgeTypeRequest(
    string Name,
    int RangeStart,
    int RangeStop);

public sealed record AddLenelBadgeTypeRequest(
    string Name,
    Guid BadgeTypeId,
    LenelMetadata Metadata);

public sealed record AddUnipassAccessLevelTypeRequest(
    string Name,
    int SiteId,
    int AccessRuleId,
    UnipassMetadata Metadata);

public sealed record AddLenelAccessLevelTypeRequest(
    string Name,
    Guid AccessLevelId,
    Guid[] BadgeTypeIds,
    LenelMetadata Metadata);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UnipassAccessControlSystemResponse), "unipass")]
[JsonDerivedType(typeof(LenelAccessControlSystemResponse), "lenel")]
public abstract record AccessControlSystemResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Endpoint { get; init; }
    public required bool SslValidation { get; init; }
    public required bool HasSecret { get; init; }
}

public sealed record UnipassAccessControlSystemResponse : AccessControlSystemResponse
{
    public required string Username { get; init; }
    public required IReadOnlyList<UnipassBadgeTypeResponse> BadgeTypes { get; init; }
    public required IReadOnlyList<UnipassAccessLevelTypeResponse> AccessLevels { get; init; }
}

public sealed record LenelAccessControlSystemResponse : AccessControlSystemResponse
{
    public required IReadOnlyList<LenelBadgeTypeResponse> BadgeTypes { get; init; }
    public required IReadOnlyList<LenelAccessLevelTypeResponse> AccessLevels { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UnipassBadgeTypeResponse), "unipass")]
[JsonDerivedType(typeof(LenelBadgeTypeResponse), "lenel")]
public abstract record BadgeTypeResponse
{
    public required Guid Id { get; init; }
    public required Guid SystemId { get; init; }
    public required string Name { get; init; }
}

public sealed record UnipassBadgeTypeResponse : BadgeTypeResponse
{
    public required int RangeStart { get; init; }
    public required int RangeStop { get; init; }
}

public sealed record LenelBadgeTypeResponse : BadgeTypeResponse
{
    public required Guid BadgeTypeId { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UnipassAccessLevelTypeResponse), "unipass")]
[JsonDerivedType(typeof(LenelAccessLevelTypeResponse), "lenel")]
public abstract record AccessLevelTypeResponse
{
    public required Guid Id { get; init; }
    public required Guid SystemId { get; init; }
    public required string Name { get; init; }
}

public sealed record UnipassAccessLevelTypeResponse : AccessLevelTypeResponse
{
    public required int SiteId { get; init; }
    public required int AccessRuleId { get; init; }
}

public sealed record LenelAccessLevelTypeResponse : AccessLevelTypeResponse
{
    public required Guid AccessLevelId { get; init; }
    public required IReadOnlyList<LenelBadgeTypeResponse> BadgeTypes { get; init; }
}

public sealed record IdentityMappingResponse(
    Guid SubjectId,
    Guid SystemId,
    string FirstName,
    string LastName,
    SubjectType SubjectType,
    string ExternalId);

public static class AccessControlSystemMapper
{
    public static AccessControlSystemResponse ToResponse(this AccessControlSystem system) =>
        system switch
        {
            UnipassAccessControlSystem unipass => new UnipassAccessControlSystemResponse
            {
                Id = unipass.Id,
                Name = unipass.Name,
                Endpoint = unipass.Config.Endpoint,
                SslValidation = unipass.Config.SslValidation,
                HasSecret = !string.IsNullOrWhiteSpace(unipass.Config.Password),
                Username = unipass.Config.Username,
                BadgeTypes = unipass.BadgeTypes.Select(type => type.ToResponse()).ToList(),
                AccessLevels = unipass.AccessLevels.Select(type => type.ToResponse()).ToList()
            },
            LenelAccessControlSystem lenel => new LenelAccessControlSystemResponse
            {
                Id = lenel.Id,
                Name = lenel.Name,
                Endpoint = lenel.Config.Endpoint,
                SslValidation = lenel.Config.SslValidation,
                HasSecret = !string.IsNullOrWhiteSpace(lenel.Config.ApiKey),
                BadgeTypes = lenel.BadgeTypes.Select(type => type.ToResponse()).ToList(),
                AccessLevels = lenel.AccessLevels.Select(type => type.ToResponse()).ToList()
            },
            _ => throw new InvalidOperationException("Unknown access control system type.")
        };

    public static BadgeTypeResponse ToResponse(this BadgeType badgeType) =>
        badgeType switch
        {
            UnipassBadgeType unipass => unipass.ToResponse(),
            LenelBadgeType lenel => lenel.ToResponse(),
            _ => throw new InvalidOperationException("Unknown badge type.")
        };

    public static UnipassBadgeTypeResponse ToResponse(this UnipassBadgeType badgeType) =>
        new()
        {
            Id = badgeType.Id,
            SystemId = badgeType.SystemId,
            Name = badgeType.Name,
            RangeStart = badgeType.Range.Start,
            RangeStop = badgeType.Range.Stop
        };

    public static LenelBadgeTypeResponse ToResponse(this LenelBadgeType badgeType) =>
        new()
        {
            Id = badgeType.Id,
            SystemId = badgeType.SystemId,
            Name = badgeType.Name,
            BadgeTypeId = badgeType.BadgeTypeId
        };

    public static AccessLevelTypeResponse ToResponse(this AccessLevelType accessLevel) =>
        accessLevel switch
        {
            UnipassAccessLevelType unipass => unipass.ToResponse(),
            LenelAccessLevelType lenel => lenel.ToResponse(),
            _ => throw new InvalidOperationException("Unknown access level type.")
        };

    public static UnipassAccessLevelTypeResponse ToResponse(this UnipassAccessLevelType accessLevel) =>
        new()
        {
            Id = accessLevel.Id,
            SystemId = accessLevel.SystemId,
            Name = accessLevel.Name,
            SiteId = accessLevel.SiteId,
            AccessRuleId = accessLevel.AccessRuleId
        };

    public static LenelAccessLevelTypeResponse ToResponse(this LenelAccessLevelType accessLevel) =>
        new()
        {
            Id = accessLevel.Id,
            SystemId = accessLevel.SystemId,
            Name = accessLevel.Name,
            AccessLevelId = accessLevel.AccessLevelId,
            BadgeTypes = accessLevel.BadgeTypes.Select(type => type.ToResponse()).ToList()
        };

    public static IdentityMappingResponse ToResponse(this IdentityMapping mapping) =>
        new(
            mapping.SubjectId,
            mapping.SystemId,
            mapping.FirstName,
            mapping.LastName,
            mapping.SubjectType,
            mapping.ExternalId);
}

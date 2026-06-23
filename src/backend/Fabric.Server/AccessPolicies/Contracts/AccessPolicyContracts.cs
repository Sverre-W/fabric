using System.Text.Json.Serialization;
using Fabric.Server.AccessPolicies.Application;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.Core;

namespace Fabric.Server.AccessPolicies.Contracts;

public record ListAccessPoliciesRequest : BaseListRequest
{
    public Guid? SystemId { get; set; }
    public Guid? SubjectId { get; set; }
    public string? Name { get; set; }
    public bool? ActiveOnly { get; set; }
}

public sealed record SubjectRequest(Guid Id, string FirstName, string LastName, SubjectType SubjectType)
{
    public Subject ToDomain() => Subject.Create(Id, FirstName, LastName, SubjectType);
}

public sealed record CreateCredentialPolicyRequest(
    Guid SystemId,
    SubjectRequest Subject,
    Guid BadgeTypeId,
    int? BadgeNumber,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveUntil,
    DateTimeOffset? ProvisionFrom);

public sealed record CreateAccessPolicyRequest(
    Guid SystemId,
    SubjectRequest Subject,
    Guid AccessLevelTypeId,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveUntil,
    DateTimeOffset? ProvisionFrom);

public sealed record SubjectResponse(Guid Id, string FirstName, string LastName, SubjectType SubjectType);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CredentialRequirementResponse), "credential")]
[JsonDerivedType(typeof(AccessRequirementResponse), "access")]
public abstract record PolicyRequirementResponse;

public sealed record CredentialRequirementResponse(
    BadgeTypeResponse BadgeType,
    int? BadgeNumber) : PolicyRequirementResponse;

public sealed record AccessRequirementResponse(
    AccessLevelTypeResponse AccessLevel) : PolicyRequirementResponse;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CredentialResponse), "credential")]
[JsonDerivedType(typeof(AccessLevelResponse), "access")]
public abstract record IssuedResourceResponse
{
    public required Guid SubjectId { get; init; }
    public required Guid SystemId { get; init; }
}

public sealed record CredentialResponse : IssuedResourceResponse
{
    public required Guid BadgeTypeId { get; init; }
    public required string BadgeNumber { get; init; }
}

public sealed record AccessLevelResponse : IssuedResourceResponse
{
    public required Guid AccessLevelTypeId { get; init; }
}

public sealed record SubjectSystemAccessStateResponse(
    Guid SubjectId,
    Guid SystemId,
    IReadOnlyList<IssuedResourceResponse> IssuedResources);

public sealed record AccessPolicyChangeResponse(
    AccessPolicyResponse? Policy,
    IssuedResourceResponse? SatisfiedBy,
    SubjectSystemAccessStateResponse AccessState);

public sealed record AccessPolicyResponse(
    Guid Id,
    Guid SystemId,
    SubjectResponse Subject,
    DateTimeOffset ProvisionFrom,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveUntil,
    PolicyRequirementResponse Requirement,
    ReconciliationStatus ReconciliationStatus,
    string? ReconciliationFailureReason,
    IssuedResourceResponse? SatisfiedBy);

public static class AccessPolicyMapper
{
    public static AccessPolicyResponse ToResponse(this AccessPolicy policy) =>
        new(
            policy.Id,
            policy.SystemId,
            policy.Subject.ToResponse(),
            policy.ProvisionFrom,
            policy.EffectiveFrom,
            policy.EffectiveUntil,
            policy.Requirement.ToResponse(),
            policy.ReconciliationStatus,
            policy.ReconciliationFailureReason,
            policy.SatisfiedBy?.ToResponse());

    public static SubjectResponse ToResponse(this Subject subject) =>
        new(subject.Id, subject.FirstName, subject.LastName, subject.SubjectType);

    public static PolicyRequirementResponse ToResponse(this PolicyRequirement requirement) =>
        requirement switch
        {
            CredentialRequirement credential => new CredentialRequirementResponse(
                credential.BadgeType.ToResponse(),
                credential.BadgeNumber),
            AccessRequirement access => new AccessRequirementResponse(access.AccessLevel.ToResponse()),
            _ => throw new InvalidOperationException("Unknown policy requirement.")
        };

    public static AccessPolicyChangeResponse ToResponse(this AccessPolicyChangeResult result) =>
        new(
            result.Policy?.ToResponse(),
            result.SatisfiedBy?.ToResponse(),
            result.AccessState.ToResponse());

    public static SubjectSystemAccessStateResponse ToResponse(this SubjectSystemAccessState state) =>
        new(
            state.SubjectId,
            state.SystemId,
            state.IssuedResources.Select(resource => resource.ToResponse()).ToList());

    public static IssuedResourceResponse ToResponse(this IssuedResource resource) =>
        resource switch
        {
            Credential credential => new CredentialResponse
            {
                SubjectId = credential.SubjectId,
                SystemId = credential.SystemId,
                BadgeTypeId = credential.BadgeTypeId,
                BadgeNumber = credential.BadgeNumber
            },
            AccessLevel accessLevel => new AccessLevelResponse
            {
                SubjectId = accessLevel.SubjectId,
                SystemId = accessLevel.SystemId,
                AccessLevelTypeId = accessLevel.AccessLevelTypeId
            },
            _ => throw new InvalidOperationException("Unknown issued resource.")
        };
}

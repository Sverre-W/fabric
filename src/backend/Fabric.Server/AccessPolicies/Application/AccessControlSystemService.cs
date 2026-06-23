using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.EntityFrameworkCore;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;

namespace Fabric.Server.AccessPolicies.Application;

public class AccessControlSystemService(
    AccessPoliciesDbContext db,
    UnipassApiFactory unipassApiFactory,
    UnipassAccessPolicyReconciler unipassReconciler)
{
    public async Task<Result<AccessControlSystem, AccessControlSystemErrors>> CreateUnipassSystem(
        string name,
        UnipassSystemConfig config,
        CancellationToken cancellationToken = default)
    {
        var system = UnipassAccessControlSystem.Create(Guid.NewGuid(), name, [], [], config);
        db.AccessControlSystems.Add(system);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessControlSystem, AccessControlSystemErrors>(system);
    }

    public async Task<Result<AccessControlSystem, AccessControlSystemErrors>> CreateLenelSystem(
        string name,
        LenelSystemConfig config,
        CancellationToken cancellationToken = default)
    {
        var system = LenelAccessControlSystem.Create(Guid.NewGuid(), name, [], [], config);
        db.AccessControlSystems.Add(system);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessControlSystem, AccessControlSystemErrors>(system);
    }

    public async Task<Result<SystemMetadata, AccessControlSystemErrors>> FetchMetadata(
        Guid systemId,
        CancellationToken cancellationToken = default)
    {
        AccessControlSystem? system = await db.AccessControlSystems
            .AsNoTracking()
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        return system switch
        {
            null => Result.Failure<SystemMetadata, AccessControlSystemErrors>(AccessControlSystemErrors.SystemNotFound),
            UnipassAccessControlSystem unipass => Result.Success<SystemMetadata, AccessControlSystemErrors>(
                await FetchUnipassMetadata(unipass, cancellationToken)),
            LenelAccessControlSystem => Result.Success<SystemMetadata, AccessControlSystemErrors>(
                new LenelMetadata([], [])),
            _ => Result.Failure<SystemMetadata, AccessControlSystemErrors>(AccessControlSystemErrors.SystemProviderMismatch)
        };
    }

    private async Task<UnipassMetadata> FetchUnipassMetadata(
        UnipassAccessControlSystem system,
        CancellationToken cancellationToken)
    {
        using IUnipassApi api = unipassApiFactory.Create(system.Config);
        List<UnipassSite> sites = await api.GetSites(ct: cancellationToken);
        List<AccessRuleDto> accessRules = await api.GetAccessRules(ct: cancellationToken);

        return new UnipassMetadata(
            [.. sites.Select(site => new SystemMetadataObject(site.Id.ToString(), site.Name))],
            [.. accessRules.Select(rule => new SystemMetadataObject(rule.Id.ToString(), GetAccessRuleName(rule)))]);
    }

    private static string GetAccessRuleName(AccessRuleDto rule) =>
        new[] { rule.Name1, rule.Name2, rule.Name3 }.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? rule.Id.ToString();

    public async Task<Result<UnipassBadgeType, AccessControlSystemErrors>> AddUnipassBadgeType(
        Guid systemId,
        string name,
        int rangeStart,
        int rangeStop,
        CancellationToken cancellationToken = default)
    {
        Result<UnipassAccessControlSystem, AccessControlSystemErrors> systemResult =
            await GetUnipassSystem(systemId, cancellationToken);

        if (systemResult.IsFailure(out AccessControlSystemErrors error))
            return Result.Failure<UnipassBadgeType, AccessControlSystemErrors>(error);

        systemResult.IsSuccess(out UnipassAccessControlSystem system);
        Result<UnipassBadgeType, AccessControlSystemErrors> result = system.AddBadgeType(
            name,
            new BadgeRange(rangeStart, rangeStop));

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<LenelBadgeType, AccessControlSystemErrors>> AddLenelBadgeType(
        Guid systemId,
        string name,
        Guid badgeTypeId,
        LenelMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        Result<LenelAccessControlSystem, AccessControlSystemErrors> systemResult =
            await GetLenelSystem(systemId, cancellationToken);

        if (systemResult.IsFailure(out AccessControlSystemErrors error))
            return Result.Failure<LenelBadgeType, AccessControlSystemErrors>(error);

        systemResult.IsSuccess(out LenelAccessControlSystem system);
        Result<LenelBadgeType, AccessControlSystemErrors> result = system.AddBadgeType(name, badgeTypeId, metadata);

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<AccessControlSystemErrors>> RemoveBadgeType(
        Guid systemId,
        Guid badgeTypeId,
        CancellationToken cancellationToken = default)
    {
        if (await BadgeTypeIsReferenced(badgeTypeId, cancellationToken))
            return Result.Failure(AccessControlSystemErrors.BadgeTypeInUse);

        AccessControlSystem? system = await db.AccessControlSystems
            .Include(accessSystem => ((UnipassAccessControlSystem)accessSystem).BadgeTypes)
            .Include(accessSystem => ((LenelAccessControlSystem)accessSystem).BadgeTypes)
            .Include(accessSystem => ((LenelAccessControlSystem)accessSystem).AccessLevels)
                .ThenInclude(accessLevel => accessLevel.BadgeTypes)
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        if (system is null)
            return Result.Failure(AccessControlSystemErrors.SystemNotFound);

        Result<AccessControlSystemErrors> result = system switch
        {
            UnipassAccessControlSystem unipass => unipass.RemoveBadgeType(badgeTypeId),
            LenelAccessControlSystem lenel when LenelAccessLevelUsesBadgeType(lenel, badgeTypeId) =>
                Result.Failure(AccessControlSystemErrors.BadgeTypeInUse),
            LenelAccessControlSystem lenel => lenel.RemoveBadgeType(badgeTypeId),
            _ => Result.Failure(AccessControlSystemErrors.SystemProviderMismatch)
        };

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<UnipassAccessLevelType, AccessControlSystemErrors>> AddUnipassAccessLevel(
        Guid systemId,
        string name,
        int siteId,
        int accessRuleId,
        UnipassMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        Result<UnipassAccessControlSystem, AccessControlSystemErrors> systemResult =
            await GetUnipassSystem(systemId, cancellationToken);

        if (systemResult.IsFailure(out AccessControlSystemErrors error))
            return Result.Failure<UnipassAccessLevelType, AccessControlSystemErrors>(error);

        systemResult.IsSuccess(out UnipassAccessControlSystem system);
        Result<UnipassAccessLevelType, AccessControlSystemErrors> result = system.AddAccessLevel(
            name,
            siteId,
            accessRuleId,
            metadata);

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<LenelAccessLevelType, AccessControlSystemErrors>> AddLenelAccessLevel(
        Guid systemId,
        string name,
        Guid accessLevelId,
        IReadOnlyList<Guid> badgeTypeIds,
        LenelMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        Result<LenelAccessControlSystem, AccessControlSystemErrors> systemResult =
            await GetLenelSystem(systemId, cancellationToken);

        if (systemResult.IsFailure(out AccessControlSystemErrors error))
            return Result.Failure<LenelAccessLevelType, AccessControlSystemErrors>(error);

        List<LenelBadgeType> badgeTypes = await db.BadgeTypes
            .OfType<LenelBadgeType>()
            .Where(type => type.SystemId == systemId && badgeTypeIds.Contains(type.Id))
            .ToListAsync(cancellationToken);

        if (badgeTypes.Count != badgeTypeIds.Distinct().Count())
            return Result.Failure<LenelAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.LenelBadgeTypesNotFound);

        systemResult.IsSuccess(out LenelAccessControlSystem system);
        Result<LenelAccessLevelType, AccessControlSystemErrors> result = system.AddAccessLevel(
            name,
            accessLevelId,
            badgeTypes,
            metadata);

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<AccessControlSystemErrors>> RemoveAccessLevel(
        Guid systemId,
        Guid accessLevelTypeId,
        CancellationToken cancellationToken = default)
    {
        if (await AccessLevelTypeIsReferenced(accessLevelTypeId, cancellationToken))
            return Result.Failure(AccessControlSystemErrors.AccessLevelTypeInUse);

        AccessControlSystem? system = await db.AccessControlSystems
            .Include(accessSystem => ((UnipassAccessControlSystem)accessSystem).AccessLevels)
            .Include(accessSystem => ((LenelAccessControlSystem)accessSystem).AccessLevels)
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        if (system is null)
            return Result.Failure(AccessControlSystemErrors.SystemNotFound);

        Result<AccessControlSystemErrors> result = system switch
        {
            UnipassAccessControlSystem unipass => unipass.RemoveAccessLevel(accessLevelTypeId),
            LenelAccessControlSystem lenel => lenel.RemoveAccessLevel(accessLevelTypeId),
            _ => Result.Failure(AccessControlSystemErrors.SystemProviderMismatch)
        };

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<AccessControlSystemErrors>> DeleteIdentityMapping(
        Guid systemId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        AccessControlSystem? system = await db.AccessControlSystems
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        if (system is null)
            return Result.Failure(AccessControlSystemErrors.SystemNotFound);

        IdentityMapping? mapping = await db.IdentityMappings
            .SingleOrDefaultAsync(item => item.SystemId == systemId && item.SubjectId == subjectId, cancellationToken);

        if (mapping is null)
            return Result.Failure(AccessControlSystemErrors.IdentityMappingNotFound);

        List<AccessPolicy> policies = await db.AccessPolicies
            .Include(policy => policy.Requirement)
            .Where(policy => policy.SystemId == systemId && policy.Subject.Id == subjectId)
            .ToListAsync(cancellationToken);

        if (system is UnipassAccessControlSystem)
        {
            foreach (AccessPolicy policy in policies)
            {
                Result<string> revokeResult = await unipassReconciler.RevokePolicyResources(policy, cancellationToken);
                if (revokeResult.IsFailure(out _))
                    return Result.Failure(AccessControlSystemErrors.IdentityMappingCleanupFailed);
            }
        }

        List<IssuedProviderResource> remainingResources = await db.IssuedProviderResources
            .Where(resource => resource.SystemId == systemId && resource.SubjectId == subjectId)
            .ToListAsync(cancellationToken);
        List<UsedBadgeNumber> remainingBadgeNumbers = await db.UsedBadgeNumbers
            .Where(number => number.SystemId == systemId && number.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        db.IssuedProviderResources.RemoveRange(remainingResources);
        db.UsedBadgeNumbers.RemoveRange(remainingBadgeNumbers);
        db.AccessPolicies.RemoveRange(policies);
        db.IdentityMappings.Remove(mapping);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessControlSystemErrors>();
    }

    public async Task<Result<AccessControlSystemErrors>> UpdateUnipassConfig(
        Guid systemId,
        string endpoint,
        bool sslValidation,
        string username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        Result<UnipassAccessControlSystem, AccessControlSystemErrors> systemResult =
            await GetUnipassSystem(systemId, cancellationToken);

        if (systemResult.IsFailure(out AccessControlSystemErrors error))
            return Result.Failure(error);

        systemResult.IsSuccess(out UnipassAccessControlSystem system);
        Result<UnipassSystemConfig, AccessControlSystemErrors> config = UnipassSystemConfig.Create(
            endpoint,
            sslValidation,
            username,
            string.IsNullOrWhiteSpace(password) ? system.Config.Password : password);

        if (config.IsFailure(out AccessControlSystemErrors configError))
            return Result.Failure(configError);

        config.IsSuccess(out UnipassSystemConfig value);
        Result<AccessControlSystemErrors> result = system.UpdateConfig(value);
        if (result.IsFailure(out _))
            return result;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessControlSystemErrors>();
    }

    public async Task<Result<AccessControlSystemErrors>> UpdateLenelConfig(
        Guid systemId,
        string endpoint,
        bool sslValidation,
        string? apiKey,
        CancellationToken cancellationToken = default)
    {
        Result<LenelAccessControlSystem, AccessControlSystemErrors> systemResult =
            await GetLenelSystem(systemId, cancellationToken);

        if (systemResult.IsFailure(out AccessControlSystemErrors error))
            return Result.Failure(error);

        systemResult.IsSuccess(out LenelAccessControlSystem system);
        Result<LenelSystemConfig, AccessControlSystemErrors> config = LenelSystemConfig.Create(
            endpoint,
            sslValidation,
            string.IsNullOrWhiteSpace(apiKey) ? system.Config.ApiKey : apiKey);

        if (config.IsFailure(out AccessControlSystemErrors configError))
            return Result.Failure(configError);

        config.IsSuccess(out LenelSystemConfig value);
        Result<AccessControlSystemErrors> result = system.UpdateConfig(value);
        if (result.IsFailure(out _))
            return result;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessControlSystemErrors>();
    }

    private async Task<Result<UnipassAccessControlSystem, AccessControlSystemErrors>> GetUnipassSystem(
        Guid systemId,
        CancellationToken cancellationToken)
    {
        AccessControlSystem? system = await db.AccessControlSystems
            .Include(accessSystem => ((UnipassAccessControlSystem)accessSystem).BadgeTypes)
            .Include(accessSystem => ((UnipassAccessControlSystem)accessSystem).AccessLevels)
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        return system switch
        {
            null => Result.Failure<UnipassAccessControlSystem, AccessControlSystemErrors>(AccessControlSystemErrors.SystemNotFound),
            UnipassAccessControlSystem unipass => Result.Success<UnipassAccessControlSystem, AccessControlSystemErrors>(unipass),
            _ => Result.Failure<UnipassAccessControlSystem, AccessControlSystemErrors>(AccessControlSystemErrors.SystemProviderMismatch)
        };
    }

    private async Task<Result<LenelAccessControlSystem, AccessControlSystemErrors>> GetLenelSystem(
        Guid systemId,
        CancellationToken cancellationToken)
    {
        AccessControlSystem? system = await db.AccessControlSystems
            .Include(accessSystem => ((LenelAccessControlSystem)accessSystem).BadgeTypes)
            .Include(accessSystem => ((LenelAccessControlSystem)accessSystem).AccessLevels)
                .ThenInclude(accessLevel => accessLevel.BadgeTypes)
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        return system switch
        {
            null => Result.Failure<LenelAccessControlSystem, AccessControlSystemErrors>(AccessControlSystemErrors.SystemNotFound),
            LenelAccessControlSystem lenel => Result.Success<LenelAccessControlSystem, AccessControlSystemErrors>(lenel),
            _ => Result.Failure<LenelAccessControlSystem, AccessControlSystemErrors>(AccessControlSystemErrors.SystemProviderMismatch)
        };
    }

    private async Task<bool> BadgeTypeIsReferenced(Guid badgeTypeId, CancellationToken cancellationToken) =>
        await db.PolicyRequirements
            .OfType<CredentialRequirement>()
            .AnyAsync(requirement => requirement.BadgeType.Id == badgeTypeId, cancellationToken);

    private async Task<bool> AccessLevelTypeIsReferenced(Guid accessLevelTypeId, CancellationToken cancellationToken) =>
        await db.PolicyRequirements
            .OfType<AccessRequirement>()
            .AnyAsync(requirement => requirement.AccessLevel.Id == accessLevelTypeId, cancellationToken);

    private static bool LenelAccessLevelUsesBadgeType(LenelAccessControlSystem system, Guid badgeTypeId) =>
        system.AccessLevels.Any(accessLevel => accessLevel.BadgeTypes.Any(badgeType => badgeType.Id == badgeTypeId));

}

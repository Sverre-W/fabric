using Fabric.Server.AccessControl.Application;
using Fabric.Server.Core;
using Fabric.Server.CredentialManagement.Contracts;
using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.CredentialManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.CredentialManagement.Application;

public sealed class CredentialManagementService(
    CredentialManagementDbContext db,
    CredentialPACSAssignmentService credentialPacsAssignmentService,
    TimeProvider timeProvider)
{
    public async Task<Result<CredentialType, CredentialManagementErrors>> CreateCredentialTypeAsync(
        CreateCredentialTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        string name = request.Name.Trim();
        if (await db.CredentialTypes.AnyAsync(type => type.Name == name, cancellationToken))
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeAlreadyExists);

        Result<CredentialType, CredentialManagementErrors> create = CredentialType.Create(
            name,
            request.Technology,
            request.AllocationMode,
            request.NearLimitThreshold,
            timeProvider.GetUtcNow());

        if (create.IsFailure(out CredentialManagementErrors error))
            return Result.Failure<CredentialType, CredentialManagementErrors>(error);

        create.IsSuccess(out CredentialType credentialType);
        db.CredentialTypes.Add(credentialType);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialType, CredentialManagementErrors>(credentialType);
    }

    public async Task<Result<CredentialType, CredentialManagementErrors>> UpdateCredentialTypeAsync(
        Guid id,
        UpdateCredentialTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        CredentialType? credentialType = await db.CredentialTypes.SingleOrDefaultAsync(type => type.Id == id, cancellationToken);
        if (credentialType is null)
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        string name = request.Name.Trim();
        if (await db.CredentialTypes.AnyAsync(type => type.Id != id && type.Name == name, cancellationToken))
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeAlreadyExists);

        Result<CredentialManagementErrors> update = credentialType.Update(
            name,
            request.Technology,
            request.AllocationMode,
            request.NearLimitThreshold,
            timeProvider.GetUtcNow());

        if (update.IsFailure(out CredentialManagementErrors error))
            return Result.Failure<CredentialType, CredentialManagementErrors>(error);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialType, CredentialManagementErrors>(credentialType);
    }

    public async Task<Result<CredentialType, CredentialManagementErrors>> SetCredentialTypeStatusAsync(
        Guid id,
        CredentialTypeStatus status,
        CancellationToken cancellationToken = default)
    {
        CredentialType? credentialType = await db.CredentialTypes.SingleOrDefaultAsync(type => type.Id == id, cancellationToken);
        if (credentialType is null)
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (status == CredentialTypeStatus.Active)
            credentialType.Activate(now);
        else
            credentialType.Disable(now);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialType, CredentialManagementErrors>(credentialType);
    }

    public async Task<Result<CredentialRange, CredentialManagementErrors>> CreateCredentialRangeAsync(
        Guid credentialTypeId,
        CreateCredentialRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        CredentialType? credentialType = await db.CredentialTypes
            .Include(type => type.Ranges)
            .SingleOrDefaultAsync(type => type.Id == credentialTypeId, cancellationToken);
        if (credentialType is null)
            return Result.Failure<CredentialRange, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        if (credentialType.AllocationMode != CredentialAllocationMode.Range)
            return Result.Failure<CredentialRange, CredentialManagementErrors>(CredentialManagementErrors.CredentialRangeInvalid);

        bool overlaps = credentialType.Ranges.Any(range => !(request.RangeStop < range.RangeStart || request.RangeStart > range.RangeStop));
        if (overlaps)
            return Result.Failure<CredentialRange, CredentialManagementErrors>(CredentialManagementErrors.CredentialRangeInvalid);

        Result<CredentialRange, CredentialManagementErrors> create = CredentialRange.Create(credentialTypeId, request.RangeStart, request.RangeStop, request.IsActive);
        if (create.IsFailure(out CredentialManagementErrors error))
            return Result.Failure<CredentialRange, CredentialManagementErrors>(error);

        create.IsSuccess(out CredentialRange range);
        db.CredentialRanges.Add(range);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialRange, CredentialManagementErrors>(range);
    }

    public async Task<Result<CredentialRange, CredentialManagementErrors>> UpdateCredentialRangeAsync(
        Guid rangeId,
        UpdateCredentialRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        CredentialRange? range = await db.CredentialRanges.SingleOrDefaultAsync(item => item.Id == rangeId, cancellationToken);
        if (range is null)
            return Result.Failure<CredentialRange, CredentialManagementErrors>(CredentialManagementErrors.CredentialRangeNotFound);

        bool overlaps = await db.CredentialRanges.AnyAsync(
            item => item.Id != rangeId && item.CredentialTypeId == range.CredentialTypeId && !(request.RangeStop < item.RangeStart || request.RangeStart > item.RangeStop),
            cancellationToken);
        if (overlaps)
            return Result.Failure<CredentialRange, CredentialManagementErrors>(CredentialManagementErrors.CredentialRangeInvalid);

        Result<CredentialManagementErrors> update = range.Update(request.RangeStart, request.RangeStop, request.IsActive);
        if (update.IsFailure(out CredentialManagementErrors error))
            return Result.Failure<CredentialRange, CredentialManagementErrors>(error);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialRange, CredentialManagementErrors>(range);
    }

    public async Task<Result<Credential, CredentialManagementErrors>> IssueCredentialAsync(
        IssueCredentialRequest request,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        CredentialType? credentialType = await db.CredentialTypes
            .Include(type => type.Ranges)
            .SingleOrDefaultAsync(type => type.Id == request.CredentialTypeId, cancellationToken);
        if (credentialType is null)
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        if (credentialType.Status != CredentialTypeStatus.Active)
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeDisabled);

        string? identifier = await ResolveIdentifierAsync(credentialType, request.Identifier, cancellationToken);
        if (identifier is null)
            return Result.Failure<Credential, CredentialManagementErrors>(MapIdentifierError(credentialType, request.Identifier));

        if (await db.Credentials.AnyAsync(item => item.Identifier == identifier, cancellationToken))
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialIdentifierAlreadyExists);

        Result<Credential, CredentialManagementErrors> create = Credential.Create(
            request.CredentialTypeId,
            identifier,
            request.IdentityId,
            request.DurationKind,
            request.ValidFrom,
            request.ValidUntil,
            request.Purpose,
            request.SourceKind,
            request.SourceId,
            request.RequestedByIdentityId,
            request.ReasonText,
            now);

        if (create.IsFailure(out CredentialManagementErrors error))
            return Result.Failure<Credential, CredentialManagementErrors>(error);

        create.IsSuccess(out Credential credential);
        db.Credentials.Add(credential);
        await db.SaveChangesAsync(cancellationToken);

        await credentialPacsAssignmentService.CreateAssignmentsForCredentialAsync(
            credential.Id,
            credential.CredentialTypeId,
            credential.ValidFrom,
            credential.ValidUntil,
            cancellationToken);

        return Result.Success<Credential, CredentialManagementErrors>(credential);
    }

    private async Task<string?> ResolveIdentifierAsync(CredentialType credentialType, string? requestedIdentifier, CancellationToken cancellationToken)
    {
        switch (credentialType.AllocationMode)
        {
            case CredentialAllocationMode.Provided:
                return string.IsNullOrWhiteSpace(requestedIdentifier) ? null : requestedIdentifier.Trim();
            case CredentialAllocationMode.Range:
                if (requestedIdentifier is not null)
                {
                    CredentialRange? matchedRange = credentialType.Ranges.FirstOrDefault(range => range.IsActive && range.Contains(requestedIdentifier));
                    return matchedRange is null ? null : requestedIdentifier.Trim();
                }

                string[] usedIdentifiers = await db.Credentials
                    .Where(credential => credential.CredentialTypeId == credentialType.Id)
                    .Select(credential => credential.Identifier)
                    .ToArrayAsync(cancellationToken);
                HashSet<string> used = usedIdentifiers.ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (CredentialRange range in credentialType.Ranges.Where(range => range.IsActive).OrderBy(range => range.RangeStart))
                {
                    for (long number = range.RangeStart; number <= range.RangeStop; number++)
                    {
                        string candidate = number.ToString();
                        if (!used.Contains(candidate))
                            return candidate;
                    }
                }

                return null;
            default:
                return null;
        }
    }

    private static CredentialManagementErrors MapIdentifierError(CredentialType credentialType, string? requestedIdentifier) => credentialType.AllocationMode switch
    {
        CredentialAllocationMode.Range when requestedIdentifier is not null && !long.TryParse(requestedIdentifier, out _) => CredentialManagementErrors.CredentialIdentifierMustBeNumeric,
        CredentialAllocationMode.Range when requestedIdentifier is not null => CredentialManagementErrors.CredentialIdentifierOutsideRange,
        CredentialAllocationMode.Range => CredentialManagementErrors.CredentialIdentifierUnavailable,
        CredentialAllocationMode.Provided => CredentialManagementErrors.CredentialIdentifierRequired,
        _ => CredentialManagementErrors.CredentialIdentifierRequired,
    };
}

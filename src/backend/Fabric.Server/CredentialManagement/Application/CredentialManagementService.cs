using Fabric.Server.Core;
using Fabric.Server.CredentialManagement.Contracts;
using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.CredentialManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.CredentialManagement.Application;

public sealed class CredentialManagementService(
    CredentialManagementDbContext db,
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
            request.RangeStart,
            request.RangeStop,
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

        int currentMaxNumber = await db.Credentials
            .Where(credential => credential.CredentialTypeId == id)
            .Select(credential => (int?)credential.CredentialNumber)
            .MaxAsync(cancellationToken) ?? int.MinValue;
        int currentMinNumber = await db.Credentials
            .Where(credential => credential.CredentialTypeId == id)
            .Select(credential => (int?)credential.CredentialNumber)
            .MinAsync(cancellationToken) ?? int.MaxValue;

        if (currentMinNumber < request.RangeStart || currentMaxNumber > request.RangeStop)
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberOutsideRange);

        Result<CredentialManagementErrors> update = credentialType.Update(
            name,
            request.Technology,
            request.RangeStart,
            request.RangeStop,
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

    public async Task<Result<CredentialTypeTarget, CredentialManagementErrors>> CreateCredentialTypeTargetAsync(
        Guid credentialTypeId,
        CreateCredentialTypeTargetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await db.CredentialTypes.AnyAsync(type => type.Id == credentialTypeId, cancellationToken))
            return Result.Failure<CredentialTypeTarget, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        bool exists = await db.CredentialTypeTargets.AnyAsync(
            target => target.CredentialTypeId == credentialTypeId && target.AccessControlSystemId == request.AccessControlSystemId,
            cancellationToken);
        if (exists)
            return Result.Failure<CredentialTypeTarget, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeAlreadyExists);

        CredentialTypeTarget target = CredentialTypeTarget.Create(
            credentialTypeId,
            request.AccessControlSystemId,
            request.ProviderCredentialTypeId,
            request.ProvisioningTiming,
            timeProvider.GetUtcNow());

        db.CredentialTypeTargets.Add(target);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialTypeTarget, CredentialManagementErrors>(target);
    }

    public async Task<Result<CredentialTypeTarget, CredentialManagementErrors>> UpdateCredentialTypeTargetAsync(
        Guid targetId,
        UpdateCredentialTypeTargetRequest request,
        CancellationToken cancellationToken = default)
    {
        CredentialTypeTarget? target = await db.CredentialTypeTargets.SingleOrDefaultAsync(item => item.Id == targetId, cancellationToken);
        if (target is null)
            return Result.Failure<CredentialTypeTarget, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeTargetNotFound);

        target.Update(request.ProviderCredentialTypeId, request.ProvisioningTiming, request.IsEnabled, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialTypeTarget, CredentialManagementErrors>(target);
    }

    public async Task<Result<CredentialTypeTarget, CredentialManagementErrors>> SetCredentialTypeTargetEnabledAsync(
        Guid targetId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        CredentialTypeTarget? target = await db.CredentialTypeTargets.SingleOrDefaultAsync(item => item.Id == targetId, cancellationToken);
        if (target is null)
            return Result.Failure<CredentialTypeTarget, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeTargetNotFound);

        target.SetEnabled(enabled, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialTypeTarget, CredentialManagementErrors>(target);
    }

    public async Task<Result<CredentialReservation, CredentialManagementErrors>> ReserveCredentialAsync(
        CreateCredentialReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        CredentialType? credentialType = await db.CredentialTypes.SingleOrDefaultAsync(type => type.Id == request.CredentialTypeId, cancellationToken);
        if (credentialType is null)
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        if (credentialType.Status != CredentialTypeStatus.Active)
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeDisabled);

        int? credentialNumber = request.CredentialNumber;
        if (credentialNumber.HasValue && !credentialType.ContainsNumber(credentialNumber.Value))
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberOutsideRange);

        credentialNumber ??= await TakeNextCredentialNumberAsync(credentialType, cancellationToken);
        if (!credentialNumber.HasValue)
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberUnavailable);

        if (!await IsCredentialNumberAvailableAsync(credentialType.Id, credentialNumber.Value, cancellationToken))
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberUnavailable);

        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<CredentialReservation, CredentialManagementErrors> create = CredentialReservation.Create(
            request.CredentialTypeId,
            credentialNumber.Value,
            request.IdentityId,
            request.Purpose,
            request.SourceKind,
            request.SourceId,
            request.RequestedByIdentityId,
            request.ReasonText,
            request.ExpiresAt,
            now);

        if (create.IsFailure(out CredentialManagementErrors error))
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(error);

        create.IsSuccess(out CredentialReservation reservation);
        db.CredentialReservations.Add(reservation);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<CredentialReservation, CredentialManagementErrors>(reservation);
    }

    public async Task<Result<Credential, CredentialManagementErrors>> IssueCredentialAsync(
        IssueCredentialRequest request,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        CredentialType? credentialType = await db.CredentialTypes.SingleOrDefaultAsync(type => type.Id == request.CredentialTypeId, cancellationToken);
        if (credentialType is null)
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeNotFound);

        if (credentialType.Status != CredentialTypeStatus.Active)
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeDisabled);

        CredentialReservation? reservation = null;
        int? credentialNumber = request.CredentialNumber;
        if (request.ReservationId.HasValue)
        {
            reservation = await db.CredentialReservations.SingleOrDefaultAsync(item => item.Id == request.ReservationId.Value, cancellationToken);
            if (reservation is null)
                return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialReservationNotFound);

            if (reservation.IdentityId != request.IdentityId)
                return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialReservationIdentityMismatch);

            Result<CredentialManagementErrors> consume = reservation.Consume(now);
            if (consume.IsFailure(out CredentialManagementErrors consumeError))
                return Result.Failure<Credential, CredentialManagementErrors>(consumeError);

            credentialNumber = reservation.CredentialNumber;
        }

        if (credentialNumber.HasValue && !credentialType.ContainsNumber(credentialNumber.Value))
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberOutsideRange);

        credentialNumber ??= await TakeNextCredentialNumberAsync(credentialType, cancellationToken);
        if (!credentialNumber.HasValue)
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberUnavailable);

        if (reservation is null && !await IsCredentialNumberAvailableAsync(credentialType.Id, credentialNumber.Value, cancellationToken))
            return Result.Failure<Credential, CredentialManagementErrors>(CredentialManagementErrors.CredentialNumberUnavailable);

        Result<Credential, CredentialManagementErrors> create = Credential.Create(
            request.CredentialTypeId,
            credentialNumber.Value,
            request.IdentityId,
            request.ReservationId,
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

        CredentialTypeTarget[] targets = await db.CredentialTypeTargets
            .Where(target => target.CredentialTypeId == credentialType.Id && target.IsEnabled)
            .ToArrayAsync(cancellationToken);
        foreach (CredentialTypeTarget target in targets)
            db.CredentialProvisioningTransactions.Add(CredentialProvisioningTransaction.Create(credential, target, now));

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<Credential, CredentialManagementErrors>(credential);
    }

    private async Task<int?> TakeNextCredentialNumberAsync(CredentialType credentialType, CancellationToken cancellationToken)
    {
        int[] usedNumbers = await db.Credentials
            .Where(credential => credential.CredentialTypeId == credentialType.Id)
            .Select(credential => credential.CredentialNumber)
            .Concat(db.CredentialReservations
                .Where(reservation => reservation.CredentialTypeId == credentialType.Id && reservation.Status == CredentialReservationStatus.Active)
                .Select(reservation => reservation.CredentialNumber))
            .Distinct()
            .ToArrayAsync(cancellationToken);

        HashSet<int> used = usedNumbers.ToHashSet();
        for (int number = credentialType.RangeStart; number <= credentialType.RangeStop; number++)
        {
            if (!used.Contains(number))
                return number;
        }

        return null;
    }

    private async Task<bool> IsCredentialNumberAvailableAsync(Guid credentialTypeId, int credentialNumber, CancellationToken cancellationToken)
    {
        bool issued = await db.Credentials.AnyAsync(
            credential => credential.CredentialTypeId == credentialTypeId && credential.CredentialNumber == credentialNumber,
            cancellationToken);
        if (issued)
            return false;

        return !await db.CredentialReservations.AnyAsync(
            reservation => reservation.CredentialTypeId == credentialTypeId
                && reservation.CredentialNumber == credentialNumber
                && reservation.Status == CredentialReservationStatus.Active,
            cancellationToken);
    }
}

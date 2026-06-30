using Fabric.Server.Core;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Application;

public class ReceptionService(
    ReceptionDbContext db,
    TimeProvider timeProvider,
    ReceptionAccessPolicyService receptionAccessPolicyService)
{
    private async Task<ExpectedArrival?> GetAggregate(Guid id, CancellationToken ct) =>
        await db.Arrivals
            .Include(a => a.Entries)
            .Include(a => a.Documents)
            .SingleOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Result<ExpectedArrival, ReceptionErrors>> RegisterVisitorArrival(
        string firstName,
        string lastName,
        string? company,
        Guid visitorId,
        Guid invitationId,
        DateTimeOffset expectedArrivalTime,
        DateTimeOffset expectedOffboardTime,
        string? arrivalCode,
        Guid? locationId,
        CancellationToken ct = default)
    {
        if (expectedArrivalTime >= expectedOffboardTime)
            return Result.Failure<ExpectedArrival, ReceptionErrors>(ReceptionErrors.ExpectedArrivalMustBeBeforeExpectedOffboard);

        var arrival = ExpectedArrival.CreateVisitorArrival(
                firstName, lastName, company, visitorId, invitationId, expectedArrivalTime, expectedOffboardTime, arrivalCode, locationId);

        db.Arrivals.Add(arrival);
        await db.SaveChangesAsync(ct);
        await receptionAccessPolicyService.ApplyTrigger(arrival, ReceptionAccessPolicyTrigger.ExpectedVisitorAdded, ct);
        return Result<ExpectedArrival, ReceptionErrors>.Success(arrival);
    }

    public async Task<Result<ReceptionErrors>> UpdateArrivalCode(Guid arrivalId, string arrivalCode, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.SetArrivalCode(arrivalCode);

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<ReceptionErrors>> Reschedule(Guid arrivalId, DateTimeOffset expectedArrivalTime, DateTimeOffset expectedOffboardTime, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Reschedule(expectedArrivalTime, expectedOffboardTime);

        if (result.IsSuccess(out _))
        {
            await db.SaveChangesAsync(cancellationToken);
            await receptionAccessPolicyService.RecreateAssignedPolicies(arrival, cancellationToken);
        }

        return result;
    }

    public async Task<Result<ReceptionErrors>> Relocate(Guid arrivalId, Guid locationId, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Relocate(locationId);

        if (result.IsSuccess(out _))
        {
            await db.SaveChangesAsync(cancellationToken);
            await receptionAccessPolicyService.RecreateAssignedPolicies(arrival, cancellationToken);
        }

        return result;
    }

    public async Task<Result<ReceptionErrors>> Cancel(Guid arrivalId, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await db.Arrivals.SingleOrDefaultAsync(x => x.Id == arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

        await receptionAccessPolicyService.RetractAssignedPolicies(arrivalId, cancellationToken);
        db.Arrivals.Remove(arrival);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<ReceptionErrors>();
    }

    public async Task<Result<ExpectedArrival, ReceptionErrors>> RegisterContractorArrival(
        string firstName,
        string lastName,
        string company,
        Guid contractorId,
        Guid jobAssignmentId,
        DateTimeOffset expectedArrivalTime,
        DateTimeOffset expectedOffboardTime,
        string? arrivalCode,
        Guid locationId,
        CancellationToken ct = default)
    {
        if (expectedArrivalTime >= expectedOffboardTime)
            return Result.Failure<ExpectedArrival, ReceptionErrors>(ReceptionErrors.ExpectedArrivalMustBeBeforeExpectedOffboard);

        var arrival = ExpectedArrival.CreateContractorArrival(
                firstName, lastName, company,
            contractorId, jobAssignmentId, expectedArrivalTime, expectedOffboardTime, arrivalCode, locationId);

        db.Arrivals.Add(arrival);
        await db.SaveChangesAsync(ct);
        await receptionAccessPolicyService.ApplyTrigger(arrival, ReceptionAccessPolicyTrigger.ContractorExpectedAdded, ct);
        return Result<ExpectedArrival, ReceptionErrors>.Success(arrival);
    }

    public async Task<Result<ReceptionErrors>> Onboard(
        Guid arrivalId,
        List<CheckInDocumentRequirement> requiredDocs,
        List<CheckInDocument> providedDocs,
        string operatorEmail,
        string? operatorDisplayName = null,
        CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Onboard(timeProvider.GetUtcNow(), requiredDocs, providedDocs, operatorEmail, operatorDisplayName);
        if (result.IsSuccess(out _))
            await SaveOnboardedArrival(arrival, arrivalId, ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> OnboardFromKiosk(
        Guid arrivalId,
        List<CheckInDocumentRequirement> requiredDocs,
        List<CheckInDocument> providedDocs,
        Guid kioskId,
        string kioskName,
        CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Onboard(timeProvider.GetUtcNow(), requiredDocs, providedDocs, kioskId, kioskName);
        if (result.IsSuccess(out _))
            await SaveOnboardedArrival(arrival, arrivalId, ct);

        return result;
    }

    private async Task SaveOnboardedArrival(ExpectedArrival arrival, Guid arrivalId, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        ReceptionAccessPolicyTrigger trigger = arrival.Type == ArrivalType.Visitor
            ? ReceptionAccessPolicyTrigger.VisitorOnboarded
            : ReceptionAccessPolicyTrigger.ContractorOnboarded;
        await receptionAccessPolicyService.ApplyTrigger(arrival, trigger, ct);
    }

    public async Task<Result<ReceptionErrors>> Offboard(Guid arrivalId, string operatorEmail, string? operatorDisplayName = null, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Offboard(timeProvider.GetUtcNow(), operatorEmail, operatorDisplayName);
        if (result.IsSuccess(out _))
        {
            await db.SaveChangesAsync(ct);
            await receptionAccessPolicyService.RetractAssignedPolicies(arrivalId, ct);
        }

        return result;
    }

    public async Task<Result<ReceptionErrors>> OffboardFromKiosk(Guid arrivalId, Guid kioskId, string kioskName, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Offboard(timeProvider.GetUtcNow(), kioskId, kioskName);
        if (result.IsSuccess(out _))
        {
            await db.SaveChangesAsync(ct);
            await receptionAccessPolicyService.RetractAssignedPolicies(arrivalId, ct);
        }

        return result;
    }

    public async Task<Result<ReceptionErrors>> CheckIn(Guid arrivalId, string operatorEmail, string? operatorDisplayName = null, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.CheckIn(timeProvider.GetUtcNow(), operatorEmail, operatorDisplayName);
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> CheckInFromKiosk(Guid arrivalId, Guid kioskId, string kioskName, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.CheckIn(timeProvider.GetUtcNow(), kioskId, kioskName);
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> CheckOut(Guid arrivalId, string operatorEmail, string? operatorDisplayName = null, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.CheckOut(timeProvider.GetUtcNow(), operatorEmail, operatorDisplayName);
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> CheckOutFromKiosk(Guid arrivalId, Guid kioskId, string kioskName, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.CheckOut(timeProvider.GetUtcNow(), kioskId, kioskName);
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> ConfirmVisitor(string firstName, string lastName, string? company, Guid arrivalId, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        arrival.UpdateVisitorDetails(firstName, lastName, company);

        Result<ReceptionErrors> result = arrival.ConfirmVisitor();
        if (result.IsSuccess(out _))
        {
            await db.SaveChangesAsync(ct);
            await receptionAccessPolicyService.ApplyTrigger(arrival, ReceptionAccessPolicyTrigger.VisitorConfirmed, ct);
        }

        return result;
    }


    public async Task<Result<ReceptionErrors>> RejectConfirmationVisitor(Guid arrivalId, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.RejectConfirmationVisitor();
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }
}

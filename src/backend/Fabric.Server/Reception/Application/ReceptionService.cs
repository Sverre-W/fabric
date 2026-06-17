using Fabric.Server.Core;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Application;

public class ReceptionService(ReceptionDbContext db, TimeProvider timeProvider)
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
        Guid invitationId,
        DateTimeOffset expectedArrivalTime,
        string? arrivalCode,
        Guid? locationId,
        CancellationToken ct = default)
    {
        var arrival = ExpectedArrival.CreateVisitorArrival(
                firstName, lastName, company, invitationId, expectedArrivalTime, arrivalCode, locationId);

        db.Arrivals.Add(arrival);
        await db.SaveChangesAsync(ct);
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

    public async Task<Result<ReceptionErrors>> Reschedule(Guid arrivalId, DateTimeOffset expectedArrivalTime, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Reschedule(expectedArrivalTime);

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<ReceptionErrors>> Relocate(Guid arrivalId, Guid locationId, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Relocate(locationId);

        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<Result<ReceptionErrors>> Cancel(Guid arrivalId, CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await db.Arrivals.SingleOrDefaultAsync(x => x.Id == arrivalId, cancellationToken);

        if (arrival is null)
            return Result.Failure(ReceptionErrors.ArrivalNotFound);

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
        string? arrivalCode,
        Guid locationId,
        CancellationToken ct = default)
    {
        var arrival = ExpectedArrival.CreateContractorArrival(
                firstName, lastName, company,
            contractorId, jobAssignmentId, expectedArrivalTime, arrivalCode, locationId);

        db.Arrivals.Add(arrival);
        await db.SaveChangesAsync(ct);
        return Result<ExpectedArrival, ReceptionErrors>.Success(arrival);
    }

    public async Task<Result<ReceptionErrors>> Onboard(
        Guid arrivalId,
        List<CheckInDocumentRequirement> requiredDocs,
        List<CheckInDocument> providedDocs,
        CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Onboard(timeProvider.GetUtcNow(), requiredDocs, providedDocs);
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> Offboard(Guid arrivalId, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.Offboard(timeProvider.GetUtcNow());
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> CheckIn(Guid arrivalId, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.CheckIn(timeProvider.GetUtcNow());
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> CheckOut(Guid arrivalId, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        Result<ReceptionErrors> result = arrival.CheckOut(timeProvider.GetUtcNow());
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

        return result;
    }

    public async Task<Result<ReceptionErrors>> ConfirmVisitor(Guid visitorId, string firstName, string lastName, string? company, Guid arrivalId, CancellationToken ct = default)
    {
        ExpectedArrival? arrival = await GetAggregate(arrivalId, ct);
        if (arrival is null)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.ArrivalNotFound);

        arrival.UpdateVisitorDetails(firstName, lastName, company);

        Result<ReceptionErrors> result = arrival.ConfirmVisitor(visitorId);
        if (result.IsSuccess(out _))
            await db.SaveChangesAsync(ct);

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

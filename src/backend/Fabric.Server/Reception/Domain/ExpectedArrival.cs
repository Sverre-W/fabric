using Fabric.Server.Core;

namespace Fabric.Server.Reception.Domain;

public sealed class ExpectedArrival
{
    private ExpectedArrival() { }

    public Guid Id { get; private set; }
    public ArrivalType Type { get; private set; }
    public DateTimeOffset ExpectedArrivalTime { get; private set; }
    public string? ArrivalCode { get; private set; }
    public OnboardingStatus Status { get; private set; }
    public DateTimeOffset? OnboardedAt { get; private set; }
    public DateTimeOffset? OffboardedAt { get; private set; }
    public bool CheckedIn { get; private set; }
    public Guid? LocationId { get; private set; }

    public List<ArrivalEntry> Entries { get; private set; } = [];
    public List<CheckInDocument> Documents { get; private set; } = [];

    public bool? Confirmed { get; private set; }
    public Guid? VisitorId { get; private set; }
    public Guid? InvitationId { get; private set; }

    public Guid? ContractorId { get; private set; }
    public Guid? JobAssignmentId { get; private set; }

    public string FirstName { get; internal set; } = null!;
    public string LastName { get; internal set; } = null!;
    public string? Company { get; internal set; }

    public static ExpectedArrival CreateVisitorArrival(
        string firstName,
        string lastName,
        string? company,
        Guid visitorId,
        Guid invitationId,
        DateTimeOffset expectedArrivalTime,
        string? arrivalCode,
        Guid? locationId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = ArrivalType.Visitor,
            FirstName = firstName,
            LastName = lastName,
            Company = company,
            VisitorId = visitorId,
            InvitationId = invitationId,
            Confirmed = false,
            ExpectedArrivalTime = expectedArrivalTime,
            ArrivalCode = arrivalCode,
            Status = OnboardingStatus.NotYetOnboarded,
            LocationId = locationId,
        };

    public static ExpectedArrival CreateContractorArrival(
        string firstName,
        string lastName,
        string company,
        Guid contractorId,
        Guid jobAssignmentId,
        DateTimeOffset expectedArrivalTime,
        string? arrivalCode,
        Guid locationId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = ArrivalType.Contractor,
            FirstName = firstName,
            LastName = lastName,
            Company = company,
            ContractorId = contractorId,
            JobAssignmentId = jobAssignmentId,
            ExpectedArrivalTime = expectedArrivalTime,
            ArrivalCode = arrivalCode,
            Status = OnboardingStatus.NotYetOnboarded,
            LocationId = locationId,
        };

    public Result<ReceptionErrors> Onboard(
        DateTimeOffset timestamp,
        List<CheckInDocumentRequirement> requiredDocs,
        List<CheckInDocument> providedDocs)
    {
        Result<ReceptionErrors> validation = ValidateRequiredDocs(requiredDocs, providedDocs);
        if (validation.IsFailure(out _)) return validation;

        ReceptionErrors? guard = GuardOnboarded();
        if (guard.HasValue) return Result<ReceptionErrors>.Failure(guard.Value);

        Status = OnboardingStatus.Onboarded;
        OnboardedAt = timestamp;
        Documents = providedDocs;
        return Result<ReceptionErrors>.Success();
    }

    public Result<ReceptionErrors> Reschedule(DateTimeOffset newExpectedArrivalTime)
    {
        if (Status != OnboardingStatus.NotYetOnboarded)
            return Result.Failure(ReceptionErrors.AlreadyOnboarded);

        ExpectedArrivalTime = newExpectedArrivalTime;
        return Result.Success<ReceptionErrors>();
    }

    public Result<ReceptionErrors> Relocate(Guid locationId)
    {
        if (Status != OnboardingStatus.NotYetOnboarded)
            return Result.Failure(ReceptionErrors.AlreadyOnboarded);

        LocationId = locationId;
        return Result.Success<ReceptionErrors>();

    }

    public Result<ReceptionErrors> SetArrivalCode(string arrivalCode)
    {
        if (Status != OnboardingStatus.NotYetOnboarded)
            return Result.Failure(ReceptionErrors.AlreadyOnboarded);

        ArrivalCode = arrivalCode;
        return Result.Success<ReceptionErrors>();
    }

    public Result<ReceptionErrors> Offboard(DateTimeOffset timestamp)
    {
        Result<ReceptionErrors> checkoutResult = CheckOut(timestamp);
        if (checkoutResult.IsFailure(out _)) return checkoutResult;

        Status = OnboardingStatus.Offboarded;
        OffboardedAt = timestamp;
        return Result.Success<ReceptionErrors>();
    }

    public Result<ReceptionErrors> CheckIn(DateTimeOffset timestamp)
    {
        ReceptionErrors? guard = GuardOnboarded();
        if (guard.HasValue) return Result<ReceptionErrors>.Failure(guard.Value);

        Entries.Add(ArrivalEntry.CheckedIn(timestamp));
        CheckedIn = true;
        return Result.Success<ReceptionErrors>();
    }

    public Result<ReceptionErrors> CheckOut(DateTimeOffset timestamp)
    {
        ReceptionErrors? guard = GuardOnboarded();
        if (guard.HasValue) return Result<ReceptionErrors>.Failure(guard.Value);

        Entries.Add(ArrivalEntry.CheckedOut(timestamp));
        CheckedIn = false;
        return Result.Success<ReceptionErrors>();
    }

    public Result<ReceptionErrors> ConfirmVisitor()
    {
        if (Type != ArrivalType.Visitor)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.NotAVisitor);

        Confirmed = true;
        return Result.Success<ReceptionErrors>();
    }

    public Result<ReceptionErrors> RejectConfirmationVisitor()
    {
        if (Type != ArrivalType.Visitor)
            return Result<ReceptionErrors>.Failure(ReceptionErrors.NotAVisitor);

        Confirmed = false;
        return Result.Success<ReceptionErrors>();

    }

    private ReceptionErrors? GuardOnboarded() =>
        Status switch
        {
            OnboardingStatus.Onboarded => null,
            OnboardingStatus.NotYetOnboarded => ReceptionErrors.NotYetOnboarded,
            OnboardingStatus.Offboarded => ReceptionErrors.AlreadyOffboarded,
            _ => ReceptionErrors.InvalidStatus,
        };

    internal void UpdateVisitorDetails(string firstName, string lastName, string? company)
    {
        FirstName = firstName;
        LastName = lastName;
        Company = company;
    }

    private static Result<ReceptionErrors> ValidateRequiredDocs(
        List<CheckInDocumentRequirement> required,
        List<CheckInDocument> provided)
    {
        var missing = required
            .Where(r => r.Required && !provided.Any(d => d.Name == r.Name && d.DocumentType == r.DocumentType))
            .ToList();

        return missing.Count == 0
            ? Result<ReceptionErrors>.Success()
            : Result<ReceptionErrors>.Failure(ReceptionErrors.MissingRequiredDocuments);
    }
}

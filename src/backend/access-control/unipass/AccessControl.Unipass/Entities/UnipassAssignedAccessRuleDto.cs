namespace AccessControl.Unipass.Entities;

internal class UnipassAssignedAccessRuleDto
{
    public int Company { get; set; }

    public int Site { get; set; }

    public int Person { get; set; }

    public int RuleIndex { get; set; }

    public int RuleType { get; set; }

    public int Rule { get; set; }

    public bool? Options { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool? SndMask { get; set; }

    public string SeqId { get; set; } = "";

    public int Id
    {
        get { return RuleIndex + 1; }
        set { RuleIndex = value - 1; }
    }
}

public class UnipassAssignedAccessRule
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int RuleId { get; set; }
    public int SiteId { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public UnipassAssignedAccessRule() { }

    internal UnipassAssignedAccessRule(UnipassAssignedAccessRuleDto dto, TimeZoneInfo timeZoneInfo)
    {
        Id = dto.Id;
        PersonId = dto.Person;
        RuleId = dto.Rule;
        SiteId = dto.Site;

        if (dto.StartDate != null)
        {
            DateTime startDate = dto.StartDate.Value.Date.AddSeconds(dto.StartTime);

            var local = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);
            var offset = timeZoneInfo.GetUtcOffset(local);

            StartDate = new DateTimeOffset(local, offset);
        }

        if (dto.EndDate != null)
        {
            DateTime endDate = dto.EndDate.Value.Date.AddSeconds(dto.EndTime);

            var local = DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified);
            var offset = timeZoneInfo.GetUtcOffset(local);

            EndDate = new DateTimeOffset(local, offset);
        }
    }
}

using System.Globalization;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Enums;

namespace AccessControl.Unipass.ChangeSets;

public class AssignedAccessRuleChangeSet : IChangeSet
{
    private UnipassOperation _operation;
    private Dictionary<string, object> _properties = [];

    private AssignedAccessRuleChangeSet() { }

    public static AssignedAccessRuleChangeSet Assign(int personId, int siteId, int ruleId)
    {
        return new AssignedAccessRuleChangeSet()
        {
            _operation = UnipassOperation.Merge,
            _properties =
            {
                { nameof(UnipassAssignedAccessRuleDto.Person), personId },
                { nameof(UnipassAssignedAccessRuleDto.Site), siteId },
                { nameof(UnipassAssignedAccessRuleDto.Rule), ruleId },
            },
        };
    }

    public static AssignedAccessRuleChangeSet Revoke(int personId, int siteId, int assignedRuleId)
    {
        return new AssignedAccessRuleChangeSet()
        {
            _operation = UnipassOperation.Delete,
            _properties =
            {
                { nameof(UnipassAssignedAccessRuleDto.Person), personId },
                { nameof(UnipassAssignedAccessRuleDto.Site), siteId },
                { nameof(UnipassAssignedAccessRuleDto.Id), assignedRuleId },
            },
        };
    }

    public AssignedAccessRuleChangeSet StartTime(DateTimeOffset startTime)
    {
        _properties[nameof(UnipassAssignedAccessRuleDto.StartTime)] = startTime;
        return this;
    }

    public AssignedAccessRuleChangeSet EndTime(DateTimeOffset endTime)
    {
        _properties[nameof(UnipassAssignedAccessRuleDto.EndTime)] = endTime;
        return this;
    }

    public async Task<ChangeSetDescription> BuildChangeSet(UnipassContext context)
    {
        if (_operation == UnipassOperation.Merge)
        {
            var assignedRules = await context.Api.GetAssignedAccessRules(
                (int)_properties[nameof(UnipassAssignedAccessRuleDto.Person)],
                context.CancellationToken
            );

            var availableId = Enumerable.Range(1, 16).Except(assignedRules.Select(x => x.Id)).FirstOrDefault();

            if (availableId == 0)
            {
                throw new InvalidOperationException("The person has reached the maximum number of assigned rules");
            }

            _properties[nameof(UnipassAssignedAccessRuleDto.RuleIndex)] = availableId - 1;
            _properties[nameof(UnipassAssignedAccessRuleDto.Id)] = availableId;
        }

        ConvertTime(
            nameof(UnipassAssignedAccessRuleDto.StartTime),
            nameof(UnipassAssignedAccessRuleDto.StartDate),
            nameof(UnipassAssignedAccessRuleDto.StartTime),
            context.TimeZoneInfo
        );

        ConvertTime(
            nameof(UnipassAssignedAccessRuleDto.EndTime),
            nameof(UnipassAssignedAccessRuleDto.EndDate),
            nameof(UnipassAssignedAccessRuleDto.EndTime),
            context.TimeZoneInfo
        );

        return new ChangeSetDescription("PersonAccessRules", _operation, _properties);
    }

    private void ConvertTime(string name, string datePartName, string timePartName, TimeZoneInfo timeZoneInfo)
    {
        if (_properties.TryGetValue(name, out var timeObject) && timeObject is DateTimeOffset time)
        {
            DateTimeOffset local = TimeZoneInfo.ConvertTime(time, timeZoneInfo);
            // Unipass treats access-rule end minutes as inclusive, so write one minute earlier.
            local = local.AddMinutes(-1);

            if (local.Hour == 0 && local.Minute == 0)
                local = local.AddMinutes(-1);

            _properties[datePartName] = local.LocalDateTime;
            _properties[timePartName] = local.ToString("t", CultureInfo.InvariantCulture);
        }
    }
}

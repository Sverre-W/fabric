using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Application;

public sealed class BadgeNumberAllocationService(AccessPoliciesDbContext db)
{
    public async Task<int?> TakeNextBadgeNumber(
        Guid systemId,
        Guid badgeTypeId,
        Guid subjectId,
        BadgeRange range,
        CancellationToken cancellationToken)
    {
        List<int> used = await db.UsedBadgeNumbers
            .Where(number => number.SystemId == systemId)
            .Where(number => number.BadgeTypeId == badgeTypeId)
            .Select(number => number.BadgeNumber)
            .ToListAsync(cancellationToken);

        HashSet<int> usedSet = [.. used];
        int? badgeNumber = Enumerable.Range(range.Start, range.Stop - range.Start + 1)
            .Cast<int?>()
            .FirstOrDefault(number => number.HasValue && !usedSet.Contains(number.Value));

        if (badgeNumber is null)
            return null;

        db.UsedBadgeNumbers.Add(UsedBadgeNumber.Create(systemId, badgeTypeId, subjectId, badgeNumber.Value));
        await db.SaveChangesAsync(cancellationToken);
        return badgeNumber.Value;
    }

    public async Task<bool> TakeBadgeNumber(
        Guid systemId,
        Guid badgeTypeId,
        Guid subjectId,
        int badgeNumber,
        CancellationToken cancellationToken)
    {
        bool isUsed = await db.UsedBadgeNumbers
            .AnyAsync(number =>
                number.SystemId == systemId &&
                number.BadgeTypeId == badgeTypeId &&
                number.BadgeNumber == badgeNumber,
                cancellationToken);

        if (isUsed)
            return false;

        db.UsedBadgeNumbers.Add(UsedBadgeNumber.Create(systemId, badgeTypeId, subjectId, badgeNumber));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReleaseBadgeNumber(
        Guid systemId,
        Guid badgeTypeId,
        int badgeNumber,
        CancellationToken cancellationToken)
    {
        UsedBadgeNumber? used = await db.UsedBadgeNumbers
            .SingleOrDefaultAsync(number =>
                number.SystemId == systemId &&
                number.BadgeTypeId == badgeTypeId &&
                number.BadgeNumber == badgeNumber,
                cancellationToken);

        if (used is null)
            return;

        db.UsedBadgeNumbers.Remove(used);
        await db.SaveChangesAsync(cancellationToken);
    }
}

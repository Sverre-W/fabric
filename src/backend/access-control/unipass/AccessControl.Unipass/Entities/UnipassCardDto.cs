namespace AccessControl.Unipass.Entities;

public class UnipassCardDto
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int Model { get; set; }

    public int Layout { get; set; }

    public int SubModel { get; set; }

    public int DaysValid { get; set; }
}

public class UnipassCard
{
    public int Id { get; set; }
    public int BadgeNumber { get; set; }
    public TimeSpan? ValidityDuration { get; set; }

    public UnipassCard() { }

    internal UnipassCard(UnipassCardDto dto)
    {
        Id = dto.Id;
        BadgeNumber = dto.Number;

        if (dto.DaysValid > 0)
        {
            ValidityDuration = TimeSpan.FromDays(dto.DaysValid);
        }
    }
}

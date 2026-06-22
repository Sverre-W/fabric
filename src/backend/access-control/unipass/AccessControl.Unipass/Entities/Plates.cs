namespace AccessControl.Unipass.Entities;

public class Plates
{
    public int CardIndex { get; set; }
    public Dictionary<int, LicencePlates> LicensePlates { get; set; }

    public Plates()
    {
        CardIndex = 1;
        LicensePlates = new Dictionary<int, LicencePlates>()
        {
            {
                0,
                new LicencePlates { Id = 0 }
            },
            {
                1,
                new LicencePlates { Id = 1 }
            },
            {
                2,
                new LicencePlates { Id = 2 }
            },
            {
                3,
                new LicencePlates { Id = 3 }
            },
        };
    }
}

public class LicencePlates
{
    public required int Id { get; set; }
    public string? Values { get; set; }
}

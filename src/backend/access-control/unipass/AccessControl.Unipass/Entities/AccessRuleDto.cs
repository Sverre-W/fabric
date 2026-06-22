namespace AccessControl.Unipass.Entities;

public class AccessRuleDto
{
    public int Id { get; set; }
    public string Name1 { get; set; } = string.Empty;
    public string Name2 { get; set; } = string.Empty;
    public string Name3 { get; set; } = string.Empty;
    public int? Monday { get; set; }
    public int? Tuesday { get; set; }
    public int? Wednesday { get; set; }
    public int? Thursday { get; set; }
    public int? Friday { get; set; }
    public int? Saturday { get; set; }
    public int? Sunday { get; set; }
    public int? SpecialDayType1 { get; set; }
    public int? SpecialDayType2 { get; set; }
}

namespace AccessControl.Unipass.Entities;

public class UnipassZoneDto
{
    public int Id { get; set; }
    public string Name1 { get; set; } = string.Empty;
    public string Name2 { get; set; } = string.Empty;
    public string Name3 { get; set; } = string.Empty;
    public string AlarmEnabled { get; set; } = "N";
    public string AccessMemoryByZone { get; set; } = "N";
    public string ResetAntipassback { get; set; } = "N";
    public string? Status { get; set; } = "Y";

    public string? ScheduleCycle { get; set; }

    public string? Antipassback { get; set; }

    public string? PresenceMinimum { get; set; }

    public string? PresenceMaximum { get; set; }

    public string? Delay { get; set; }

    public string? AccessibleZones { get; set; }

    public string? AlarmProcess1 { get; set; }

    public string? AlarmProcess2 { get; set; }

    public string? AlarmProcess3 { get; set; }

    public string? AntipassbackDelay { get; set; }

    public int? ControlType { get; set; }

    public int? OfficeModeScheduleCycle { get; set; }

    public string? AlarmGroup { get; set; }
}

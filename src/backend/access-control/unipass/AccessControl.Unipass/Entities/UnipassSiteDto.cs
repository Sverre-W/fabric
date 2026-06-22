using System.Text.Json.Serialization;

namespace AccessControl.Unipass.Entities;

internal class UnipassSiteDto
{
    public int Id { get; set; }
    public bool Enabled { get; set; } = true;
    public bool AccessOnLockedReaders { get; set; } = true;
    public bool IgnoreAntiPassBack { get; set; }
    public bool OfficeModeEnabled { get; set; }
    public string Name1 { get; set; } = string.Empty;
    public string Name2 { get; set; } = string.Empty;
    public string Name3 { get; set; } = string.Empty;
    public int SaltoNCal { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MultiCycles { get; set; } = "N";
    public string? Location { get; set; }
}

public class UnipassSite
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public bool AccessOnLockedReaders { get; set; }
    public bool IgnoreAntiPassBack { get; set; }
    public bool OfficeModeEnabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }

    public UnipassSite() { }

    internal UnipassSite(UnipassSiteDto unipassSiteDto)
    {
        Id = unipassSiteDto.Id;
        Enabled = unipassSiteDto.Enabled;
        AccessOnLockedReaders = unipassSiteDto.AccessOnLockedReaders;
        IgnoreAntiPassBack = unipassSiteDto.IgnoreAntiPassBack;
        OfficeModeEnabled = unipassSiteDto.OfficeModeEnabled;
        Name = unipassSiteDto.Name1;
        Location = unipassSiteDto.Location;
    }
}

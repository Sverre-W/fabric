using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace AccessControl.Unipass.Entities;

internal class UnipassPersonDto
{
    public Dictionary<int, UnipassCardDto>? Cards { get; set; }

    public int? Id { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public UnipassPersonType AccessType { get; set; }

    public bool? Enabled { get; set; }

    public int Language { get; set; }

    public string? Sex { get; set; }

    public string? NationalRegister { get; set; }

    public CompanyDto? Company { get; set; }

    public int? MainSite { get; set; }
}

public class UnipassPerson
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
    public int Language { get; set; }

    public int? MainSite { get; set; }
    public string? NationalSecurityNumber { get; set; }
    public UnipassSex Sex { get; set; }
    public IReadOnlyList<UnipassCard> Cards { get; set; } = [];

    public UnipassPersonType PersonType { get; set; } = UnipassPersonType.Staff;

    public List<int> EnabledSites { get; set; } = [];

    public UnipassPerson() { }

    internal UnipassPerson(UnipassPersonDto personDto)
    {
        Id = personDto.Id ?? 0;
        PersonType = personDto.AccessType;
        FirstName = personDto.FirstName ?? "";
        LastName = personDto.LastName ?? "";
        IsEnabled = personDto.Enabled ?? false;
        Language = personDto.Language;
        MainSite = personDto.MainSite;
        NationalSecurityNumber = personDto.NationalRegister;
        Sex = personDto.Sex?.ToUpper() switch
        {
            "O" => UnipassSex.Other,
            "M" => UnipassSex.Male,
            "F" => UnipassSex.Female,
            _ => UnipassSex.Other,
        };
        Cards = personDto.Cards?.Values.Where(x => x.Number != 0).Select(x => new UnipassCard(x)).ToImmutableList() ?? [];

        if (personDto.Company?.Enabled == true && personDto.Company.Sites != null)
        {
            EnabledSites = personDto.Company.Sites.Values.Where(x => x.Enabled).Select(x => x.Id).ToList();
        }
    }
}

public enum UnipassSex
{
    [EnumMember(Value = "O")]
    Other,

    [EnumMember(Value = "M")]
    Male,

    [EnumMember(Value = "F")]
    Female,
}

public enum UnipassPersonType
{
    [EnumMember(Value = "pers")]
    Staff = 0,

    [EnumMember(Value = "visitor")]
    Visitor = 1,
}

using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Enums;

namespace AccessControl.Unipass.ChangeSets;

public class PersonChangeSet : IChangeSet
{
    public const int MAX_SIZE_NAME = 32;

    private Dictionary<string, object> _properties = [];
    private UnipassOperation _unipassOperation;

    private PersonChangeSet()
    {
        _unipassOperation = UnipassOperation.Merge;
    }

    public static PersonChangeSet Create(int? personId = null)
    {
        var changeSet = new PersonChangeSet() { _unipassOperation = UnipassOperation.Insert };

        if (personId.HasValue)
            changeSet._properties[nameof(UnipassPersonDto.Id)] = personId.Value;

        changeSet.FirstName("Unkown").LastName("Unkown").Enabled();
        return changeSet;
    }

    public static PersonChangeSet Update(int personId)
    {
        return new PersonChangeSet() { _unipassOperation = UnipassOperation.Update, _properties = { { nameof(UnipassPersonDto.Id), personId } } };
    }

    public static PersonChangeSet Delete(int personId)
    {
        return new PersonChangeSet() { _unipassOperation = UnipassOperation.Delete, _properties = { { nameof(UnipassPersonDto.Id), personId } } };
    }

    public PersonChangeSet FirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            firstName = "Unkown";
        }

        firstName = firstName.Length > MAX_SIZE_NAME ? firstName[..MAX_SIZE_NAME] : firstName;
        _properties[nameof(UnipassPersonDto.FirstName)] = firstName;
        return this;
    }

    public PersonChangeSet LastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            lastName = "Unkown";
        }

        lastName = lastName.Length > MAX_SIZE_NAME ? lastName[..MAX_SIZE_NAME] : lastName;

        _properties[nameof(UnipassPersonDto.LastName)] = lastName;
        return this;
    }

    public PersonChangeSet Enabled(bool enabled = true)
    {
        _properties[nameof(UnipassPersonDto.Enabled)] = enabled;
        return this;
    }

    public PersonChangeSet Sex(UnipassSex sex)
    {
        _properties[nameof(UnipassPersonDto.Sex)] = sex.GetType().GetMember(sex.ToString()).First().GetCustomAttribute<EnumMemberAttribute>()?.Value!;

        return this;
    }

    public PersonChangeSet MainSite(int siteId)
    {
        _properties[nameof(UnipassPersonDto.MainSite)] = siteId;
        return this;
    }

    public PersonChangeSet NationalSecurityNumber(string nationalSecurityNumber)
    {
        _properties[nameof(UnipassPersonDto.NationalRegister)] = nationalSecurityNumber;
        return this;
    }

    public PersonChangeSet Language(int languageId)
    {
        _properties[nameof(UnipassPersonDto.Language)] = languageId;
        return this;
    }

    public PersonChangeSet EnableSite(int siteId)
    {
        EnableSites([siteId]);
        return this;
    }

    public PersonChangeSet EnableSites(int[] siteIds)
    {
        List<SetCompanDto> company = [new(UnipassOperation.Merge, siteIds)];
        _properties["Company"] = company;
        return this;
    }

    public PersonChangeSet PersonType(UnipassPersonType accessTypes)
    {
        _properties[nameof(UnipassPersonDto.AccessType)] = accessTypes
            .GetType()
            .GetMember(accessTypes.ToString())
            .First()
            .GetCustomAttribute<EnumMemberAttribute>()
            ?.Value!;

        return this;
    }

    public Task<ChangeSetDescription> BuildChangeSet(UnipassContext _)
    {
        return Task.FromResult(new ChangeSetDescription("Persons", _unipassOperation, _properties));
    }
}

internal class SetCompanDto
{
    public int Id { get; set; } = 1;

    public bool Enabled { get; set; } = true;

    public SetCompanDto(UnipassOperation operation, int[] siteIds)
    {
        Site = [.. siteIds.Select(x => new SetSiteDto(operation, x))];
        Operation = operation;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnipassOperation Operation { get; set; }

    public List<SetSiteDto> Site { get; set; }

    public SetCompanDto()
    {
        Site = [];
    }
}

internal class SetSiteDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnipassOperation Operation { get; set; }

    public SetSiteDto(UnipassOperation operation, int siteId)
    {
        Operation = operation;
        Id = siteId;
    }

    public SetSiteDto() { }

    public bool Enabled { get; set; } = true;

    public int Id { get; set; }
}

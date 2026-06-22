namespace AccessControl.Unipass.Entities;

internal class CompanyDto
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public Dictionary<int, UnipassSiteDto> Sites { get; set; } = [];
}

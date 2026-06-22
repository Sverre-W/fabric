using System.Text.Json.Serialization;

namespace AccessControl.Unipass.Entities;

// [UnipassEntity("AccessTicking")]
public class AccessTicking
{
    public int SeqId { get; set; }
    public int SiteCode { get; set; }
    public int BadgeType { get; set; }
    public int Code { get; set; }
    public int Zone { get; set; }
    public int TwoMenRuleBadge { get; set; }
    public int TwoMenRuleSiteCode { get; set; }
    public int TwoMenRuleBadgeType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Person { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Date { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Time { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Reader { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Badge { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Keyboard { get; set; }

    /// <summary>
    /// Only used in post requests
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Operation { get; set; }
}

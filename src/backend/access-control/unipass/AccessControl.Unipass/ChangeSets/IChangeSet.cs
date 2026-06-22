using System.Text.Json.Serialization;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Enums;
using AccessControl.Unipass.Infrastructure;

namespace AccessControl.Unipass.ChangeSets;

public interface IChangeSet
{
    Task<ChangeSetDescription> BuildChangeSet(UnipassContext context);
}

public record UnipassContext(
    IUnipassApi Api,
    TimeZoneInfo TimeZoneInfo,
    CancellationToken CancellationToken
);

[JsonConverter(typeof(ChangeSetDescriptionConverter))]
public record ChangeSetDescription(
    string ResourceName,
    UnipassOperation Operation,
    Dictionary<string, object> Properties,
    Func<UnipassOperationResponse, UnipassOperationResponse>? ResponseTransformer = null
);

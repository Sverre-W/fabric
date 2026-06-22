using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AccessControl.Unipass.ChangeSets;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.Logging;

namespace AccessControl.Unipass.Infrastructure;

public sealed class UnipassWebApi(
    HttpClient client,
    TimeZoneInfo unipassTimeZone,
    JsonSerializerOptions jsonSerializerOptions
) : IUnipassApi
{
    private const string ServiceUrl = "/IDtech/IdtAPIService/api/";
    private bool _isDisposed;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        client.Dispose();
    }

    public async Task<List<UnipassSite>> GetSites(SitesFilter? sitesFilter = null, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.AddTag("Filter", sitesFilter?.BuildQueryString() ?? "No filter");
        return await GetEntitiesAsync<UnipassSiteDto, UnipassSite>("Sites", x => new UnipassSite(x), sitesFilter, ct);
    }

    public Task<List<AccessRuleDto>> GetAccessRules(AccessRuleFilter? accessRuleFilter = null, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.AddTag("Filter", accessRuleFilter?.BuildQueryString() ?? "No filter");
        return GetEntitiesAsync("RuleCalendar", accessRuleFilter, ct);
    }

    public async Task<List<UnipassAssignedAccessRule>> GetAssignedAccessRules(int personId, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.AddTag("PersonId", personId);

        return await GetEntitiesAsync<UnipassAssignedAccessRuleDto, UnipassAssignedAccessRule>(
            "PersonAccessRules",
            x => x.Rule == 0 ? null : new UnipassAssignedAccessRule(x, unipassTimeZone),
            new AssignedAccessRuleFilter().WithPersonId(personId),
            ct
        );
    }

    public async Task<UnipassPerson?> GetPerson(int personId, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.AddTag("PersonId", personId);
        var persons = await GetPersons(new PersonFilter().WithId(personId), ct);
        return persons.FirstOrDefault();
    }

    public async Task<List<UnipassPerson>> GetPersons(PersonFilter? personFilter, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity();
        activity?.AddTag("Filter", personFilter?.BuildQueryString() ?? "No filter");
        var persons = await GetEntitiesAsync<UnipassPersonDto, UnipassPerson>("Persons", x => new UnipassPerson(x), personFilter, ct);

        foreach (var person in persons)
        {
            person.Cards = person.Cards.Where(x => x.Id != 0).ToList();
        }

        return persons;
    }

    private static void ThrowOnError(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new UnipassException(response.StatusCode, response.ReasonPhrase);
        }
    }

    private async Task<List<T>> GetEntitiesAsync<T>(string endpoint, IUnipassFilter<T>? filter = null, CancellationToken cancellationToken = default)
    {
        var url = new StringBuilder();
        url.Append(ServiceUrl + endpoint);
        if (filter != null)
        {
            var queryUrl = filter.BuildQueryString();
            url.Append($"?{queryUrl}");
        }

        var response = await client.GetAsync(url.ToString(), cancellationToken);
        ThrowOnError(response);

        return await response.Content.ReadFromJsonAsync<List<T>>(jsonSerializerOptions, cancellationToken) ?? [];
    }

    private async Task<List<U>> GetEntitiesAsync<T, U>(
        string endpoint,
        Func<T, U?> dtoMapper,
        IUnipassFilter<U>? filter = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = new StringBuilder();
        url.Append(ServiceUrl + endpoint);
        if (filter != null)
        {
            var queryUrl = filter.BuildQueryString();
            url.Append($"?{queryUrl}");
        }

        var response = await client.GetAsync(url.ToString(), cancellationToken);
        ThrowOnError(response);

        var resposeContent = await response.Content.ReadFromJsonAsync<List<T>>(jsonSerializerOptions, cancellationToken) ?? [];

        return resposeContent.Select(dtoMapper).Where(x => x != null).Cast<U>().ToList();
    }

    public async Task<UnipassOperationResponse> ApplyChangeSet(IChangeSet changeSet, CancellationToken ct = default)
    {
        var operation = await changeSet.BuildChangeSet(new UnipassContext(this, unipassTimeZone, ct));
        List<ChangeSetDescription> operations = [operation];

        var response = await client.PostAsJsonAsync(ServiceUrl + operation.ResourceName, operations, jsonSerializerOptions, ct);
        ThrowOnError(response);

        var parsedResponse = await response.Content.ReadFromJsonAsync<List<UnipassOperationResponse>>(jsonSerializerOptions, ct) ?? [];

        var operationResult = parsedResponse.FirstOrDefault();

        operationResult ??= new UnipassOperationResponse() { Success = false, Message = "Did not receive a valid response " };
        operation.ResponseTransformer?.Invoke(operationResult);

        return operationResult;
    }
}

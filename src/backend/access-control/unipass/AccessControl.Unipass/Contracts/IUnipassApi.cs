using AccessControl.Unipass.ChangeSets;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Filters;

namespace AccessControl.Unipass.Contracts;

/// <summary>
/// Api for interacting with Unipass access control system
/// </summary>
public interface IUnipassApi : IDisposable
{
    /// <summary>
    /// Retrieves all sites according to the provided <paramref name="sitesFilter"/>. If filter is null, retrieves all sites.
    /// </summary>
    /// <param name="sitesFilter">The filter</param>
    /// <param name="ct">The cancellation token</param>
    Task<List<UnipassSite>> GetSites(
        SitesFilter? sitesFilter = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves all access rules according to the provided <paramref name="accessRuleFilter"/>. If
    /// filter is null, retrieves all access rules.
    /// </summary>
    /// <param name="accessRuleFilter">The filter</param>
    /// <param name="ct">The cancellation token</param>
    Task<List<AccessRuleDto>> GetAccessRules(
        AccessRuleFilter? accessRuleFilter = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves all assigned access rules for a given person.
    /// </summary>
    /// <param name="personId">The ID of the person</param>
    /// <param name="ct">The cancellation token</param>
    Task<List<UnipassAssignedAccessRule>> GetAssignedAccessRules(
        int personId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves a person by their unique identifier.
    /// </summary>
    /// <param name="personId">The unique identifier of the person.</param>
    /// <param name="ct">The cancellation token.</param>
    Task<UnipassPerson?> GetPerson(int personId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of persons based on the provided filter criteria. If criteria is null, retrieves all persons.
    /// </summary>
    /// <param name="personFilter">The filter criteria for retrieving persons.</param>
    /// <param name="ct">The cancellation token.</param>
    Task<List<UnipassPerson>> GetPersons(
        PersonFilter? personFilter,
        CancellationToken ct = default
    );

    /// <summary>
    /// Execute the changes that are given in the <see cref="IChangeSet"/>
    /// </summary>
    /// <param name="changeSet"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<UnipassOperationResponse> ApplyChangeSet(
        IChangeSet changeSet,
        CancellationToken ct = default
    );
}

namespace Fabric.Server.AccessPolicies.Domain;

public enum PolicyCreationMode
{
    FailIfSyncReconciliationFails,
    PersistPendingIfSyncReconciliationFails
}

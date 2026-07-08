using Fabric.Hardware.Desfire.Encoding.Specifications;

namespace Fabric.Hardware.Desfire.Scripting;

public sealed record KeyGroupReference(string Id, string Name);

public static class ChipDesignReferenceNormalizer
{
    public static async Task NormalizeKeyGroups(
        TemplateSpecification specification,
        Func<string, CancellationToken, Task<KeyGroupReference?>> resolve,
        CancellationToken ct = default
    )
    {
        if (specification.Picc.Key != null)
        {
            await NormalizeKeyGroup(specification.Picc.Key, resolve, ct);
        }

        foreach (ApplicationSpecification application in specification.Applications.Values)
        {
            await NormalizeKeyGroup(application, resolve, ct);
        }
    }

    private static async Task NormalizeKeyGroup(
        KeySpecification keySpecification,
        Func<string, CancellationToken, Task<KeyGroupReference?>> resolve,
        CancellationToken ct
    )
    {
        await NormalizeKeyGroupCore(
            keySpecification.KeyGroup,
            keySpecification.KeyGroupName,
            resolve,
            ct,
            (id, name) =>
            {
                keySpecification.KeyGroup = id;
                keySpecification.KeyGroupName = name;
            }
        );
    }

    private static async Task NormalizeKeyGroup(
        ApplicationSpecification application,
        Func<string, CancellationToken, Task<KeyGroupReference?>> resolve,
        CancellationToken ct
    )
    {
        await NormalizeKeyGroupCore(
            application.KeyGroup,
            application.KeyGroupName,
            resolve,
            ct,
            (id, name) =>
            {
                application.KeyGroup = id;
                application.KeyGroupName = name;
            }
        );
    }

    private static async Task NormalizeKeyGroupCore(
        string keyGroup,
        string keyGroupName,
        Func<string, CancellationToken, Task<KeyGroupReference?>> resolve,
        CancellationToken ct,
        Action<string, string> assign
    )
    {
        string selector = !string.IsNullOrWhiteSpace(keyGroup) ? keyGroup : keyGroupName;
        if (string.IsNullOrWhiteSpace(selector))
        {
            assign(keyGroup, keyGroupName);
            return;
        }

        KeyGroupReference? resolved = await resolve(selector, ct);
        if (resolved != null)
        {
            assign(resolved.Id, resolved.Name);
            return;
        }

        assign(keyGroup, string.IsNullOrWhiteSpace(keyGroupName) ? selector : keyGroupName);
    }
}

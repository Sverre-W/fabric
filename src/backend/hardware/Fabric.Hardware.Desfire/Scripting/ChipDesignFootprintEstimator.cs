using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Scripting;

public sealed record ApplicationFootprintEstimate(
    string Key,
    int FileCount,
    int FileBytes,
    int FileAllocationBytes,
    int Bytes);

public sealed record KeyGroupFootprintEstimate(
    string KeyGroup,
    int KeySetCount,
    int KeyCount,
    int Bytes);

public sealed record CardFootprintEstimate(
    int ApplicationCount,
    int ApplicationBytes,
    int FileCount,
    int FileBytes,
    int FileOverheadBytes,
    int KeyGroupCount,
    int KeySetCount,
    int KeyCount,
    int KeyBytes,
    int KnownBytes,
    int BudgetBytes,
    IReadOnlyList<ApplicationFootprintEstimate> Applications,
    IReadOnlyList<KeyGroupFootprintEstimate> KeyGroups)
{
    public int RemainingBytes => BudgetBytes - KnownBytes;
}

public static class ChipDesignFootprintEstimator
{
    private const int ApplicationOverheadBytes = 256;

    public static async Task<CardFootprintEstimate> EstimateAsync(
        TemplateSpecification design,
        IKeyGroupResolver keyGroupResolver,
        CancellationToken cancellationToken = default)
    {
        List<string> selectors = CollectRequiredKeyGroups(design);
        List<(string Selector, KeyGroupData Data)> keyGroups = [];

        foreach (string selector in selectors)
        {
            KeyGroupData? resolved = await keyGroupResolver.ResolveKeyGroup(selector, cancellationToken);
            if (resolved != null)
            {
                keyGroups.Add((selector, resolved));
            }
        }

        List<ApplicationFootprintEstimate> applicationFootprints = [];

        foreach ((string key, ApplicationSpecification application) in design.Applications)
        {
            int applicationFileCount = application.Files.Count;
            int applicationFileBytes = application.Files.Values.Sum(file => Math.Max(file.Size, 0));
            int applicationFileAllocationBytes = application.Files.Values.Sum(file => EstimateFileAllocationBytes(file.Size));
            int applicationFootprintBytes = ApplicationOverheadBytes + applicationFileAllocationBytes;

            applicationFootprints.Add(
                new ApplicationFootprintEstimate(
                    key,
                    applicationFileCount,
                    applicationFileBytes,
                    applicationFileAllocationBytes,
                    applicationFootprintBytes
                )
            );
        }

        int applicationCount = applicationFootprints.Count;
        int fileCount = applicationFootprints.Sum(application => application.FileCount);
        int fileBytes = applicationFootprints.Sum(application => application.FileBytes);
        int fileAllocationBytes = applicationFootprints.Sum(application => application.FileAllocationBytes);
        int applicationBytes = applicationFootprints.Sum(application => application.Bytes);
        int fileOverheadBytes = fileAllocationBytes - fileBytes;

        List<KeyGroupFootprintEstimate> keyGroupFootprints = [];

        foreach ((string selector, KeyGroupData data) in keyGroups)
        {
            int groupKeySetCount = data.KeySets.Length;
            int groupKeyCount = data.KeySets.Sum(set => set.Keys.Length);
            int keySize = CryptoHelper.GetKeySize(data.KeyType);
            int bytes = data.KeySets.Sum(set => set.Keys.Length * keySize);

            keyGroupFootprints.Add(new KeyGroupFootprintEstimate(selector, groupKeySetCount, groupKeyCount, bytes));
        }

        int keySetCount = keyGroupFootprints.Sum(group => group.KeySetCount);
        int keyCount = keyGroupFootprints.Sum(group => group.KeyCount);
        int keyBytes = keyGroupFootprints.Sum(group => group.Bytes);

        int knownBytes = applicationBytes + keyBytes;

        return new CardFootprintEstimate(
            applicationCount,
            applicationBytes,
            fileCount,
            fileBytes,
            fileOverheadBytes,
            keyGroups.Count,
            keySetCount,
            keyCount,
            keyBytes,
            knownBytes,
            8192,
            applicationFootprints,
            keyGroupFootprints
        );
    }

    private static List<string> CollectRequiredKeyGroups(TemplateSpecification design)
    {
        HashSet<string> keyGroups = new(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(design.Picc?.Key?.KeyGroup))
        {
            keyGroups.Add(design.Picc!.Key!.KeyGroup);
        }

        foreach (ApplicationSpecification application in design.Applications.Values)
        {
            if (!string.IsNullOrWhiteSpace(application.KeyGroup))
            {
                keyGroups.Add(application.KeyGroup);
            }
        }

        return [.. keyGroups];
    }

    private static int EstimateFileAllocationBytes(int fileSize)
    {
        if (fileSize <= 0)
        {
            return 0;
        }

        if (fileSize <= 16)
        {
            return 32;
        }

        if (fileSize <= 32)
        {
            return 64;
        }

        return ((fileSize + 31) / 32) * 32;
    }
}

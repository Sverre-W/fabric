using Fabric.Hardware.Desfire.Encoding.Models;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Operations;
using Fabric.Hardware.Desfire.Scripting.Services;

namespace Fabric.Hardware.Desfire.Scripting;

public class ChipDesignReader(IKeyGroupResolver resolver)
{
    public async Task<IExecutionPlan<ReaderExecutedPlan>> CreateExecutionPlan(
        List<(TemplateSpecification, ReaderProfile)> readerProfiles,
        CancellationToken ct = default
    )
    {
        var operations = new Dictionary<ReaderProfile, List<IDesfireOperation>>();

        foreach (var (chipDesign, readerProfile) in readerProfiles)
        {
            var applicationSpecification = chipDesign.Applications[readerProfile.ApplicationId];
            var profileOperations = await ReadProfile(applicationSpecification, readerProfile, ct);
            operations.Add(readerProfile, profileOperations);
        }

        return new ReaderExecutionPlan(operations);
    }

    public async Task<List<IDesfireOperation>> ReadProfile(
        ApplicationSpecification applicationSpecification,
        ReaderProfile profile,
        CancellationToken ct = default
    )
    {
        var appKeygroup = applicationSpecification.KeyGroup;
        var keyGroup = await resolver.ResolveKeyGroup(appKeygroup, ct);
        var fileToBeRead = applicationSpecification.Files.Values.FirstOrDefault(x => x.Id == profile.FileId);

        if (keyGroup == null)
        {
            throw new Exception($"Key group {appKeygroup} not found");
        }

        if (fileToBeRead == null)
        {
            throw new Exception($"File {profile.FileId} not found");
        }

        if (fileToBeRead.ReadKey == null)
        {
            throw new Exception($"File {profile.FileId} does not have a read key");
        }

        int fileId = int.Parse(fileToBeRead.ReadKey);

        var operations = new List<IDesfireOperation>
        {
            new SelectApplicationOperation(DesfireApplicationId.Create(profile.ApplicationId)),
            new AuthenticateOperation(keyGroup, 0, fileId),
            new GetCardUidOperation(),
            new ReadFromFileOperation(
                fileId,
                ChipDesignTransformer.FromFileMode(fileToBeRead.Mode),
                fileToBeRead.DataOffsetBytes,
                fileToBeRead.DataLengthBytes > 0 ? fileToBeRead.DataLengthBytes : fileToBeRead.Size,
                fileToBeRead.Variable
            ),
        };

        return operations;
    }
}

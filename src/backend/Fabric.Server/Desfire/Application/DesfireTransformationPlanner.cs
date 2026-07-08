using System.Text.Json;
using Fabric.Server.Desfire.Contracts;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Scripting;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireTransformationPlanner(DesfireDbContext db, DesfireKeyGroupResolver keyGroupResolver)
{
    private const string BlankKeyGroup = "_blank_";

    public async Task<TransformationPlanMetadata> GetMetadataAsync(string? fromChipDesignName, bool fromBlank, string toChipDesignName, CancellationToken ct)
    {
        TemplateSpecification current = await ResolveSourceAsync(fromChipDesignName, fromBlank, ct);
        TemplateSpecification target = await ResolveLatestDesignAsync(toChipDesignName, ct);

        List<string> requiredVariables = ChipDesignTransformer.GetRequiredVariables(current, target);
        List<string> requiredKeyGroups = ChipDesignTransformer.GetRequiredKeyGroups(current, target);
        ExecutionPlan plan = await ChipDesignTransformer.CreatePlan(keyGroupResolver, current, target, readUid: true);

        TransformationPlanOperationResponse[] operations = [.. plan.Operations.Select((operation, index) => new TransformationPlanOperationResponse(index + 1, operation.GetType().Name, operation.ToString() ?? operation.GetType().Name))];
        return new TransformationPlanMetadata(requiredVariables, requiredKeyGroups, plan.Errors.Select(error => error.Message).ToArray(), plan.Operations.Count, operations, InferVariableFormats(target));
    }

    public async Task<TemplateSpecification> ResolveLatestDesignAsync(string chipDesignName, CancellationToken ct)
    {
        ChipDesign? design = await db.ChipDesigns
            .AsNoTracking()
            .Where(candidate => candidate.Name == chipDesignName)
            .OrderByDescending(candidate => candidate.Version)
            .FirstOrDefaultAsync(ct);

        if (design is null)
            throw new InvalidOperationException($"Chip design '{chipDesignName}' does not exist.");

        return JsonSerializer.Deserialize<TemplateSpecification>(design.SpecificationJson, DesfireJson.Options)
            ?? throw new InvalidOperationException($"Chip design '{chipDesignName}' specification is invalid.");
    }

    private async Task<TemplateSpecification> ResolveSourceAsync(string? fromChipDesignName, bool fromBlank, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(fromChipDesignName))
            return await ResolveLatestDesignAsync(fromChipDesignName, ct);

        if (!fromBlank)
            throw new InvalidOperationException("Transformation source must be a chip design or blank chip type.");

        return CreateBlankSpecification();
    }

    private static TemplateSpecification CreateBlankSpecification() => new()
    {
        Picc = new PiccSpecification
        {
            Key = new KeySpecification
            {
                KeyGroup = BlankKeyGroup,
                KeyGroupName = BlankKeyGroup,
                KeySet = 0,
                Key = 0
            }
        },
        Applications = []
    };

    private static IReadOnlyDictionary<string, VariableFormatRequest> InferVariableFormats(TemplateSpecification target)
    {
        Dictionary<string, VariableFormatRequest> formats = new(StringComparer.OrdinalIgnoreCase);
        foreach (FileSpecification file in target.Applications.Values.SelectMany(application => application.Files.Values))
        {
            if (string.IsNullOrWhiteSpace(file.Variable) || formats.ContainsKey(file.Variable))
                continue;

            formats[file.Variable] = InferFormat(file.Encoding);
        }

        return formats;
    }

    private static VariableFormatRequest InferFormat(string? encoding)
    {
        if (string.Equals(encoding, "text", StringComparison.OrdinalIgnoreCase))
            return new VariableFormatRequest(DesfireVariableFormatKind.Text);

        if (string.Equals(encoding, "hex", StringComparison.OrdinalIgnoreCase))
            return new VariableFormatRequest(DesfireVariableFormatKind.Hex);

        string[] parts = (encoding ?? string.Empty).Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 3 && string.Equals(parts[0], "uint", StringComparison.OrdinalIgnoreCase) && int.TryParse(parts[1], out int length))
            return new VariableFormatRequest(DesfireVariableFormatKind.UInt, length);

        return new VariableFormatRequest(DesfireVariableFormatKind.Hex);
    }
}

public sealed record TransformationPlanMetadata(IReadOnlyList<string> RequiredVariables, IReadOnlyList<string> RequiredKeyGroups, IReadOnlyList<string> Errors, int OperationCount, IReadOnlyList<TransformationPlanOperationResponse> Operations, IReadOnlyDictionary<string, VariableFormatRequest> VariableFormats);

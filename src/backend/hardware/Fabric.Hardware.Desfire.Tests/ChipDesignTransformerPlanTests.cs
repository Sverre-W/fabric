using Fabric.Hardware.Desfire.Encoding.Models;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Scripting;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using System.Collections.Generic;
using System.Linq;

namespace Fabric.Hardware.Desfire.Tests;

public class ChipDesignTransformerPlanTests
{
    [Fact]
    public async Task CreatePlan_UsesNewPiccKeyForApplicationCreationAfterPiccKeyChange()
    {
        TestKeyGroupResolver resolver = new(
            ("picc", CreateKeyGroup(KeyType.Aes, "11111111111111111111111111111111")),
            ("app", CreateKeyGroup(KeyType.Aes, "22222222222222222222222222222222"))
        );

        TemplateSpecification current = new()
        {
            Picc = new PiccSpecification
            {
                Key = new KeySpecification
                {
                    KeyGroup = "_blank_",
                    KeyGroupName = "_blank_",
                    KeySet = 0,
                    Key = 0,
                },
            },
            Applications = [],
        };

        TemplateSpecification next = new()
        {
            Picc = new PiccSpecification
            {
                Key = new KeySpecification
                {
                    KeyGroup = "picc",
                    KeyGroupName = "picc",
                    KeySet = 0,
                    Key = 0,
                },
            },
            Applications = new Dictionary<string, ApplicationSpecification>
            {
                ["ABCABC"] = new ApplicationSpecification
                {
                    Aid = "ABCABC",
                    KeyGroup = "app",
                    KeyGroupName = "app",
                    KeySettings = new ApplicationKeySettingsSpecification
                    {
                        ChangeKey = "0",
                        AllowCreateDelete = false,
                        Changeable = true,
                        MasterKeyChangeable = true,
                        FreeDirectoryListing = true,
                    },
                    Files = [],
                },
            },
        };

        ExecutionPlan plan = await ChipDesignTransformer.CreatePlan(resolver, current, next, readUid: true);
        List<string> operations = plan.Operations.Select(operation => operation.ToString()!).ToList();

        int piccChangeIndex = operations.IndexOf("Change PICC Key [Aes, 16 bytes]");
        int appCreateIndex = operations.IndexOf("Create application ABCABC");

        piccChangeIndex.ShouldBeGreaterThanOrEqualTo(0);
        appCreateIndex.ShouldBeGreaterThan(piccChangeIndex);
        operations[appCreateIndex - 1].ShouldBe("Authenticate AES with key 0 (16 bytes)");
        operations.Skip(piccChangeIndex + 1).ShouldNotContain(operation => operation.StartsWith("Probe default PICC key", System.StringComparison.Ordinal));
    }

    private static KeyGroupData CreateKeyGroup(KeyType keyType, string keyValue)
    {
        return new KeyGroupData
        {
            KeyType = keyType,
            KeySets =
            [
                new KeySet
                {
                    Id = 0,
                    Keys =
                    [
                        new Key
                        {
                            KeyId = 0,
                            Value = keyValue,
                            IsKeyDiversified = false,
                        },
                    ],
                },
            ],
        };
    }

    private sealed class TestKeyGroupResolver(params (string Name, KeyGroupData Group)[] groups) : IKeyGroupResolver
    {
        private readonly Dictionary<string, KeyGroupData> _groups = groups.ToDictionary(group => group.Name, group => group.Group, System.StringComparer.OrdinalIgnoreCase);

        public Task<KeyGroupData?> ResolveKeyGroup(string keyGroupName, System.Threading.CancellationToken ct = default)
        {
            return Task.FromResult(_groups.TryGetValue(keyGroupName, out KeyGroupData? keyGroup) ? keyGroup : null);
        }
    }
}

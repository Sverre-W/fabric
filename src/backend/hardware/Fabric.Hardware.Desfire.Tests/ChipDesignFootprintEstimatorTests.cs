
//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class ChipDesignFootprintEstimatorTests
// {
//     [Fact]
//     public async Task EstimateAsync_SumsFilesAndKeys()
//     {
//         TemplateSpecification design = new()
//         {
//             Picc = new PiccSpecification(),
//             Applications = new Dictionary<string, ApplicationSpecification>
//             {
//                 ["A10041"] = new ApplicationSpecification
//                 {
//                     Aid = "A10041",
//                     KeyGroup = "app-key-group",
//                     KeyGroupName = "App Key Group",
//                     Files = new Dictionary<string, FileSpecification>
//                     {
//                         ["F01"] = new FileSpecification { Id = 0, Size = 32 },
//                         ["F02"] = new FileSpecification { Id = 1, Size = 64 },
//                     },
//                 },
//             },
//         };
//
//         FakeKeyGroupResolver resolver = new();
//         resolver.Register(
//             "app-key-group",
//             new KeyGroupData
//             {
//                 KeyType = Fabric.Hardware.Desfire.Protocol.KeyType.Aes,
//                 KeySets =
//                 [
//                     new KeySet
//                     {
//                         Id = 0,
//                         Keys = [new Key { KeyId = 0, Value = "00000000000000000000000000000000" }],
//                     },
//                     new KeySet
//                     {
//                         Id = 1,
//                         Keys = [new Key { KeyId = 0, Value = "00000000000000000000000000000000" }],
//                     },
//                 ],
//             }
//         );
//
//         CardFootprintEstimate estimate = await ChipDesignFootprintEstimator.EstimateAsync(design, resolver);
//
//         estimate.ApplicationCount.ShouldBe(1);
//         estimate.ApplicationBytes.ShouldBe(384);
//         estimate.FileCount.ShouldBe(2);
//         estimate.FileBytes.ShouldBe(96);
//         estimate.FileOverheadBytes.ShouldBe(32);
//         estimate.KeyGroupCount.ShouldBe(1);
//         estimate.KeySetCount.ShouldBe(2);
//         estimate.KeyCount.ShouldBe(2);
//         estimate.KeyBytes.ShouldBe(32);
//         estimate.KnownBytes.ShouldBe(416);
//         estimate.BudgetBytes.ShouldBe(8192);
//         estimate.Applications.ShouldHaveSingleItem();
//         estimate.Applications[0].Key.ShouldBe("A10041");
//         estimate.Applications[0].Bytes.ShouldBe(384);
//         estimate.KeyGroups.ShouldHaveSingleItem();
//         estimate.KeyGroups[0].KeyGroup.ShouldBe("app-key-group");
//         estimate.KeyGroups[0].Bytes.ShouldBe(32);
//     }
//
//     private sealed class FakeKeyGroupResolver : IKeyGroupResolver
//     {
//         private readonly Dictionary<string, KeyGroupData> _groups = new(StringComparer.OrdinalIgnoreCase);
//
//         public void Register(string selector, KeyGroupData keyGroup)
//         {
//             _groups[selector] = keyGroup;
//         }
//
//         public Task<KeyGroupData?> ResolveKeyGroup(string keyGroupName, CancellationToken ct = default)
//         {
//             return Task.FromResult(_groups.TryGetValue(keyGroupName, out KeyGroupData? keyGroup) ? keyGroup : null);
//         }
//     }
// }
//

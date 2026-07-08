// using Shouldly;
// using Fabric.Hardware.Desfire.Encoder.Shared;
//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class EncodeRuntimeInputResolverTests
// {
//     [Fact]
//     public void MergeVariables_UsesExpectedPrecedence()
//     {
//         Dictionary<string, string> defaults = new()
//         {
//             ["CompanyName"] = "IBM",
//             ["LastName"] = "{{LastName}}",
//         };
//
//         Dictionary<string, string> payload = new()
//         {
//             ["CompanyName"] = "Microsoft",
//         };
//
//         Dictionary<string, string> cli = new()
//         {
//             ["LastName"] = "Humperdink",
//         };
//
//         Dictionary<string, string> merged = EncodeRuntimeInputResolver.MergeVariables(defaults, payload, cli);
//
//         merged["CompanyName"].ShouldBe("Microsoft");
//         merged["LastName"].ShouldBe("Humperdink");
//     }
//
//     [Fact]
//     public void GetMissingRequiredVariables_FlagsPlaceholdersAndMissingValues()
//     {
//         Dictionary<string, string> variables = new()
//         {
//             ["CompanyName"] = "IBM",
//             ["LastName"] = "{{LastName}}",
//         };
//
//         List<string> missing = EncodeRuntimeInputResolver.GetMissingRequiredVariables(
//             ["CompanyName", "LastName", "BadgeNumber"],
//             variables
//         );
//
//         missing.ShouldBe(["LastName", "BadgeNumber"]);
//     }
//
//     [Fact]
//     public void GetMissingRequiredVariables_AllowsEmptyValues()
//     {
//         Dictionary<string, string> variables = new()
//         {
//             ["empty"] = string.Empty,
//         };
//
//         List<string> missing = EncodeRuntimeInputResolver.GetMissingRequiredVariables(
//             ["empty"],
//             variables
//         );
//
//         missing.ShouldBeEmpty();
//     }
//
//     [Fact]
//     public void Merge_OverlayRuntimeKeygroupsAndVariables()
//     {
//         EncodeOperation operation = new()
//         {
//             Variables = new Dictionary<string, string>
//             {
//                 ["CompanyName"] = "IBM",
//             },
//         };
//
//         EncodeRuntimePayload payload = new()
//         {
//             Keygroups = new Dictionary<string, EncodeOperationKeyGroup>
//             {
//                 ["default-des-key"] = new EncodeOperationKeyGroup
//                 {
//                     Name = "default-des-key",
//                     KeyType = Fabric.Hardware.Desfire.Encoding.Models.KeyType.Des,
//                     KeySets = [new Fabric.Hardware.Desfire.Encoding.Models.KeySet
//                     {
//                         Id = 0,
//                         Keys = [new Fabric.Hardware.Desfire.Encoding.Models.Key
//                         {
//                             KeyId = 0,
//                             Value = "0000000000000000",
//                             IsKeyDiversified = false,
//                         }],
//                     }],
//                 },
//             },
//             Variables = new Dictionary<string, string>
//             {
//                 ["CompanyName"] = "Microsoft",
//                 ["LastName"] = "Humperdink",
//             },
//         };
//
//         EncodeOperation merged = EncodeRuntimeInputResolver.Merge(operation, payload);
//
//         merged.Keygroups.ShouldNotBeNull();
//         merged.Keygroups!["default-des-key"].Name.ShouldBe("default-des-key");
//         merged.Variables["CompanyName"].ShouldBe("Microsoft");
//         merged.Variables["LastName"].ShouldBe("Humperdink");
//     }
// }

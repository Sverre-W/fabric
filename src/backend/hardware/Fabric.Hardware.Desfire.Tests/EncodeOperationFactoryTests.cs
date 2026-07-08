// using Shouldly;
// using Fabric.Hardware.Desfire.Encoding.Models;
// using Fabric.Hardware.Desfire.Encoding.Specifications;
// using Fabric.Hardware.Desfire.Encoder.Shared;
//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class EncodeOperationFactoryTests
// {
//     [Fact]
//     public void Create_uses_placeholders_for_missing_values()
//     {
//         ChipDesign from = new()
//         {
//             Name = "From",
//             Specification = new TemplateSpecification { Picc = new PiccSpecification(), Applications = new Dictionary<string, ApplicationSpecification>() },
//         };
//         ChipDesign to = new()
//         {
//             Name = "To",
//             Specification = new TemplateSpecification
//             {
//                 Picc = new PiccSpecification(),
//                 Applications = new Dictionary<string, ApplicationSpecification>
//                 {
//                     ["app"] = new ApplicationSpecification
//                     {
//                         Files = new Dictionary<string, FileSpecification>
//                         {
//                             ["file"] = new FileSpecification { Variable = "NumeroDeCart", Encoding = "uint:7:be" },
//                         },
//                     },
//                 },
//             },
//         };
//
//         EncodeOperation operation = EncodeOperationFactory.Create(from, to);
//
//         operation.FromDesign.ShouldBe(from);
//         operation.ToDesign.ShouldBe(to);
//         operation.Keygroups.ShouldBeNull();
//         operation.Variables["NumeroDeCart"].ShouldBe("{{NumeroDeCart}}");
//     }
//
//     [Fact]
//     public void Create_includes_provided_variables_and_keygroups()
//     {
//         ChipDesign from = new()
//         {
//             Name = "From",
//             Specification = new TemplateSpecification { Picc = new PiccSpecification(), Applications = new Dictionary<string, ApplicationSpecification>() },
//         };
//         ChipDesign to = new()
//         {
//             Name = "To",
//             Specification = new TemplateSpecification
//             {
//                 Picc = new PiccSpecification(),
//                 Applications = new Dictionary<string, ApplicationSpecification>
//                 {
//                     ["app"] = new ApplicationSpecification
//                 },
//             },
//         };
//
//         Dictionary<string, EncodeOperationKeyGroup> keygroups = new()
//         {
//             ["defaultdes"] = new EncodeOperationKeyGroup
//             {
//                 Name = "defaultdes",
//                 KeyType = KeyType.Aes,
//                 KeySets =
//                 [
//                     new KeySet
//                     {
//                         Id = 0,
//                         Keys = [new Key { KeyId = 0, Value = "00112233445566778899AABBCCDDEEFF", IsKeyDiversified = false }],
//                     },
//                 ],
//             },
//         };
//
//         EncodeOperation operation = EncodeOperationFactory.Create(from, to, keygroups, new Dictionary<string, string> { ["NumeroDeCart"] = "47551" });
//
//         operation.Keygroups.ShouldNotBeNull();
//         operation.Keygroups!["defaultdes"].Name.ShouldBe("defaultdes");
//         operation.Variables["NumeroDeCart"].ShouldBe("47551");
//
//         string json = EncodeOperationFactory.Serialize(operation);
//         json.ShouldContain("\"fromDesign\"");
//         json.ShouldContain("\"toDesign\"");
//         json.ShouldContain("\"defaultdes\"");
//     }
// }

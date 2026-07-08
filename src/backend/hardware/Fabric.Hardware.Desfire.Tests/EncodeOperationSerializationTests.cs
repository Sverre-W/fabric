// using System.Text.Json;
// using Shouldly;
// using Fabric.Hardware.Desfire.Encoder.Shared;
//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class EncodeOperationSerializationTests
// {
//     [Fact]
//     public void SerializeOperation_OmitsNullKeygroups()
//     {
//         EncodeOperation operation = new()
//         {
//             Variables = new Dictionary<string, string>
//             {
//                 ["badge"] = "hex:0102",
//             },
//         };
//
//         string json = JsonSerializer.Serialize(operation, new JsonSerializerOptions
//         {
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//         });
//
//         using JsonDocument document = JsonDocument.Parse(json);
//         document.RootElement.TryGetProperty("keygroups", out _).ShouldBeFalse();
//         document.RootElement.GetProperty("variables").GetProperty("badge").GetString().ShouldBe("hex:0102");
//     }
// }

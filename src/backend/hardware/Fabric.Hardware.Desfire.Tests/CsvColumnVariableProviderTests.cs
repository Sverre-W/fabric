// using Fabric.Hardware.Desfire.Encoding.Server.Models;
//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class CsvColumnVariableProviderTests
// {
//     [Fact]
//     public async Task GetNextVariable_should_return_utf8_bytes_from_the_current_row()
//     {
//         CsvColumnVariableProvider provider = new()
//         {
//             CsvColumn = "FirstName",
//             Name = "First name",
//         };
//
//         byte[] value = await provider.GetNextVariable(new Dictionary<string, string> { ["FirstName"] = "Ada" });
//
//         Assert.Equal("Ada", System.Text.Encoding.UTF8.GetString(value));
//     }
//
//     [Fact]
//     public async Task GetNextVariable_should_fail_when_the_column_is_missing()
//     {
//         CsvColumnVariableProvider provider = new()
//         {
//             CsvColumn = "FirstName",
//             Name = "First name",
//         };
//
//         await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetNextVariable(new Dictionary<string, string> { ["LastName"] = "Lovelace" }));
//     }
// }

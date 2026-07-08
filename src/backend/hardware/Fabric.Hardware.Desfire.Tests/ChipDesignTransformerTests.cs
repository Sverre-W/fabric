

// namespace Fabric.Hardware.Desfire.Tests;
//
// public class ChipDesignTransformerTests
// {
//     [Fact]
//     public async Task ChangePiccKey_reauthenticates_before_changing_when_previous_auth_used_same_key_number()
//     {
//         var resolver = new TestKeyGroupResolver(
//             ("current-group", CreateKeyGroup(KeyType.Aes, "00000000000000000000000000000000")),
//             ("next-group", CreateKeyGroup(KeyType.Aes, "11111111111111111111111111111111"))
//         );
//
//         TemplateSpecification current = new()
//         {
//             Picc = new PiccSpecification
//             {
//                 Key = new KeySpecification
//                 {
//                     KeyGroup = "current-group",
//                     KeyGroupName = "current-group",
//                     KeySet = 0,
//                     Key = 0,
//                 },
//             },
//             Applications = [],
//         };
//
//         TemplateSpecification next = new()
//         {
//             Picc = new PiccSpecification
//             {
//                 Key = new KeySpecification
//                 {
//                     KeyGroup = "next-group",
//                     KeyGroupName = "next-group",
//                     KeySet = 0,
//                     Key = 0,
//                 },
//             },
//             Applications = [],
//         };
//
//         ExecutionPlan plan = await ChipDesignTransformer.CreatePlan(resolver, current, next, readUid: true);
//         List<string> ops = plan.Operations.Select(op => op.ToString()!).ToList();
//
//         ops.ShouldContain(op => op == "Get card UID");
//         ops.ShouldContain(op => op == "Change PICC Key [Aes, 16 bytes]");
//
//         int changeIndex = ops.FindIndex(op => op.StartsWith("Change PICC Key", StringComparison.Ordinal));
//         changeIndex.ShouldBeGreaterThanOrEqualTo(0);
//
//         ops[changeIndex - 1].ShouldStartWith("Authenticate");
//     }
//
//     [Fact]
//     public async Task BuildApplicationDescription_includes_iso_df_name_when_present()
//     {
//         ApplicationSpecification application = new()
//         {
//             Aid = "F11C2C",
//             IsoDfName = "EVA-App",
//             KeyGroup = "group",
//             KeyGroupName = "group",
//             SecureMessing = new SecureMessingConfiguration(),
//             Use2BytesFileIdentifier = false,
//             Files = [],
//         };
//
//         KeyGroupData keyGroup = CreateKeyGroup(KeyType.Aes, "00000000000000000000000000000000");
//         ApplicationKeySettings keySettings = new()
//         {
//             ChangeKey = ChangeKey.AnyApplicationKey(),
//             KeySettingsReadOnly = false,
//             AllowCreateAndDeleteWithoutMasterKey = true,
//             MasterKeyReadOnly = false,
//             FreeDirectoryListing = true,
//         };
//         ApplicationSettings applicationSettings = new()
//         {
//             KeyType = KeyType.Aes,
//             ApplicationKeys = 5,
//             ExtendedApplicationSettings = false,
//             Use2ByteFileIdentifiers = false,
//         };
//
//         MethodInfo buildMethod = typeof(ChipDesignTransformer).GetMethod(
//             "BuildApplicationDescription",
//             BindingFlags.NonPublic | BindingFlags.Static
//         )!;
//
//         ApplicationDescription description = (ApplicationDescription)buildMethod.Invoke(
//             null,
//             [application, keyGroup, keySettings, applicationSettings]
//         )!;
//
//         MethodInfo buildHeaderMethod = typeof(ApplicationDescription).GetMethod(
//             "Build",
//             BindingFlags.NonPublic | BindingFlags.Instance
//         )!;
//
//         byte[] header = (byte[])buildHeaderMethod.Invoke(description, [])!;
//         Convert.ToHexString(header).ShouldContain(Convert.ToHexString(System.Text.Encoding.ASCII.GetBytes("EVA-App")));
//     }
//
//     [Fact]
//     public void ParseKey_supports_reserved_desfire_values_and_legacy_aliases()
//     {
//         MethodInfo parseMethod = typeof(ChipDesignTransformer).GetMethod(
//             "ParseKey",
//             BindingFlags.NonPublic | BindingFlags.Static
//         )!;
//
//         ChangeKey anyKey = (ChangeKey)parseMethod.Invoke(null, ["any"] )!;
//         ChangeKey eKey = (ChangeKey)parseMethod.Invoke(null, ["E"] )!;
//         ChangeKey noneKey = (ChangeKey)parseMethod.Invoke(null, ["none"] )!;
//         ChangeKey fKey = (ChangeKey)parseMethod.Invoke(null, ["F"] )!;
//
//         anyKey.ShouldBe(ChangeKey.AnyApplicationKey());
//         eKey.ShouldBe(ChangeKey.AnyApplicationKey());
//         noneKey.ShouldBe(ChangeKey.ReadOnly());
//         fKey.ShouldBe(ChangeKey.ReadOnly());
//     }
//
//     [Fact]
//     public void GetWritableKey_prefers_readwrite_key_when_write_key_is_locked()
//     {
//         MethodInfo method = typeof(ChipDesignTransformer).GetMethod(
//             "GetWritableKey",
//             BindingFlags.NonPublic | BindingFlags.Static
//         )!;
//
//         DesfireFileAccessRights rights = new()
//         {
//             WriteKey = ChangeKey.ReadOnly(),
//             ReadWriteKey = ChangeKey.SpecificKey(2),
//         };
//
//         ChangeKey writableKey = (ChangeKey)method.Invoke(null, [rights])!;
//
//         writableKey.ShouldBe(ChangeKey.SpecificKey(2));
//     }
//
//     private static KeyGroupData CreateKeyGroup(KeyType keyType, string keyValue)
//     {
//         return new KeyGroupData
//         {
//             KeyType = keyType,
//             KeySets =
//             [
//                 new KeySet
//                 {
//                     Id = 0,
//                     Keys =
//                     [
//                         new Key
//                         {
//                             KeyId = 0,
//                             Value = keyValue,
//                             IsKeyDiversified = false,
//                         },
//                     ],
//                 },
//             ],
//         };
//     }
//
//     private sealed class TestKeyGroupResolver(params (string Name, KeyGroupData Group)[] groups) : IKeyGroupResolver
//     {
//         private readonly Dictionary<string, KeyGroupData> _groups = groups.ToDictionary(group => group.Name, group => group.Group, StringComparer.OrdinalIgnoreCase);
//
//         public Task<KeyGroupData?> ResolveKeyGroup(string keyGroupName, CancellationToken ct = default)
//         {
//             return Task.FromResult(_groups.TryGetValue(keyGroupName, out KeyGroupData? keyGroup) ? keyGroup : null);
//         }
//     }
// }

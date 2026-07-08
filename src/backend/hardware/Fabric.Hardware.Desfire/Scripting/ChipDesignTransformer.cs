using Fabric.Hardware.Desfire.Encoding.Models;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Encoding.Utilities;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Operations;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Utils;
using FileMode = Fabric.Hardware.Desfire.Encoding.Specifications.FileMode;
using KeyType = Fabric.Hardware.Desfire.Protocol.KeyType;

namespace Fabric.Hardware.Desfire.Scripting;

public class ChipDesignTransformer
{
    private const string BlankAutoDetectPlaceholderKeyGroup = "_blank_";
    private readonly List<ScriptError> _errors = [];
    private readonly Dictionary<string, KeyGroupData> _keyGroups = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _requiredVariables = [];
    private readonly int currentKeySet = 0;
    private int? currentAuthenticatedKey;
    private string? currentSelectedApp = "000000";
    public bool ReadUid { get; set; } = false;

    private ChipDesignTransformer() { }

    public static async Task<ExecutionPlan> CreatePlan(
        IKeyGroupResolver resolver,
        TemplateSpecification current,
        TemplateSpecification toBe,
        bool readUid = false
    )
    {
        ChipDesignTransformer calculator = new() { ReadUid = readUid };
        var (operations, errors) = await calculator.CalculateTransform(resolver, current, toBe);

        return new ExecutionPlan(operations, calculator._requiredVariables, errors);
    }

    public static List<string> GetRequiredKeyGroups(TemplateSpecification current, TemplateSpecification toBe)
    {
        List<string> keyGroups = [];

        if (current.Picc.Key?.KeyGroup != null && current.Picc.Key.KeyGroup != BlankAutoDetectPlaceholderKeyGroup)
        {
            keyGroups.Add(current.Picc.Key.KeyGroup);
        }

        if (toBe.Picc.Key?.KeyGroup != null && toBe.Picc.Key.KeyGroup != BlankAutoDetectPlaceholderKeyGroup)
        {
            keyGroups.Add(toBe.Picc.Key.KeyGroup);
        }

        foreach (ApplicationSpecification application in current.Applications.Values)
        {
            keyGroups.Add(application.KeyGroup);
        }

        foreach (ApplicationSpecification application in toBe.Applications.Values)
        {
            keyGroups.Add(application.KeyGroup);
        }

        return [.. keyGroups.Distinct()];
    }

    public static List<string> GetRequiredVariables(TemplateSpecification current, TemplateSpecification toBe)
    {
        HashSet<string> variables = [];

        foreach (ApplicationSpecification application in toBe.Applications.Values)
        {
            foreach (FileSpecification file in application.Files.Values)
            {
                if (!variables.Contains(file.Variable))
                {
                    variables.Add(file.Variable);
                }
            }
        }

        foreach (ApplicationSpecification application in current.Applications.Values)
        {
            foreach (FileSpecification file in application.Files.Values)
            {
                if (variables.Contains(file.Variable))
                {
                    variables.Remove(file.Variable);
                }
            }
        }

        return [.. variables];
    }

    public async Task<(List<IDesfireOperation>, List<ScriptError>)> CalculateTransform(
        IKeyGroupResolver keyGroupResolver,
        TemplateSpecification current,
        TemplateSpecification toBe
    )
    {
        if (toBe.Picc.Key == null && current.Picc.Key != null && current.Picc.Key.KeyGroup != BlankAutoDetectPlaceholderKeyGroup)
        {
            toBe.Picc.Key = current.Picc.Key;
        }

        List<List<IDesfireOperation>> operations = [];

        await ResolveKeyGroups(keyGroupResolver, current, toBe);
        operations.Add([.. ReadUidFromCard(current)]);
        operations.Add([.. GetReadVariableOperations(current, toBe)]);
        operations.Add([.. CalculatePiccChanges(current.Picc, toBe.Picc)]);
        operations.Add([.. CalculateApplicationChanges(current, toBe)]);
        operations.Add([.. AdaptConfigurationSettings(current, toBe)]);

        return ([.. operations.SelectMany(x => x)], _errors);
    }

    private async Task ResolveKeyGroups(IKeyGroupResolver keyGroupResolver, TemplateSpecification current, TemplateSpecification toBe)
    {
        List<string> keygroups = GetRequiredKeyGroups(current, toBe);
        foreach (string keygroup in keygroups)
        {
            if (keygroup == BlankAutoDetectPlaceholderKeyGroup)
                continue;

            KeyGroupData? resolvedGroup = await keyGroupResolver.ResolveKeyGroup(keygroup);

            if (resolvedGroup == null)
            {
                AddError($"{keygroup} key group does not exists");
            }
            else
            {
                RegisterKeyGroupAliases(current, toBe, keygroup, resolvedGroup);
            }
        }
    }

    private void RegisterKeyGroupAliases(TemplateSpecification current, TemplateSpecification toBe, string selector, KeyGroupData resolvedGroup)
    {
        _keyGroups[selector] = resolvedGroup;

        RegisterKeyGroupAliases(current.Picc.Key, selector, resolvedGroup);
        RegisterKeyGroupAliases(toBe.Picc.Key, selector, resolvedGroup);

        foreach (ApplicationSpecification app in current.Applications.Values)
        {
            RegisterKeyGroupAliases(app, selector, resolvedGroup);
        }

        foreach (ApplicationSpecification app in toBe.Applications.Values)
        {
            RegisterKeyGroupAliases(app, selector, resolvedGroup);
        }
    }

    private void RegisterKeyGroupAliases(KeySpecification? keySpecification, string selector, KeyGroupData resolvedGroup)
    {
        if (keySpecification == null)
        {
            return;
        }

        if (!string.Equals(keySpecification.KeyGroup, selector, StringComparison.OrdinalIgnoreCase) && !string.Equals(keySpecification.KeyGroupName, selector, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(keySpecification.KeyGroup))
        {
            _keyGroups[keySpecification.KeyGroup] = resolvedGroup;
        }

        if (!string.IsNullOrWhiteSpace(keySpecification.KeyGroupName))
        {
            _keyGroups[keySpecification.KeyGroupName] = resolvedGroup;
        }
    }

    private void RegisterKeyGroupAliases(ApplicationSpecification application, string selector, KeyGroupData resolvedGroup)
    {
        if (!string.Equals(application.KeyGroup, selector, StringComparison.OrdinalIgnoreCase) && !string.Equals(application.KeyGroupName, selector, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(application.KeyGroup))
        {
            _keyGroups[application.KeyGroup] = resolvedGroup;
        }

        if (!string.IsNullOrWhiteSpace(application.KeyGroupName))
        {
            _keyGroups[application.KeyGroupName] = resolvedGroup;
        }
    }

    private KeyGroupData GetKeyGroupData(string keyGroup, string keyGroupName = "")
    {
        if (_keyGroups.TryGetValue(keyGroup, out KeyGroupData? resolved))
        {
            return resolved;
        }

        if (!string.IsNullOrWhiteSpace(keyGroupName) && _keyGroups.TryGetValue(keyGroupName, out resolved))
        {
            return resolved;
        }

        throw new KeyNotFoundException(keyGroup);
    }

    private IEnumerable<IDesfireOperation> ReadUidFromCard(TemplateSpecification current)
    {
        bool shouldReadUid = ReadUid || _keyGroups.Values.Any(x => x.KeyDiversificationStrategy != null);

        if (!shouldReadUid)
            yield break;

        if (current.Picc.Key?.KeyGroup == BlankAutoDetectPlaceholderKeyGroup)
        {
            yield return new AuthenticateDefaultProbeOperation(current.Picc.Key.Key);
            yield return new GetCardUidOperation();
            yield break;
        }

        KeyGroupData? nonDiversifiedKey = null;
        string? keyGroup = null;

        foreach (KeyValuePair<string, KeyGroupData> kvp in _keyGroups)
        {
            if (kvp.Value.KeySets[currentKeySet].Keys.Any(x => !x.IsKeyDiversified))
            {
                nonDiversifiedKey = kvp.Value;
                keyGroup = kvp.Key;
                break;
            }
        }

        if (nonDiversifiedKey == null || keyGroup == null)
        {
            AddError("At least one non diversified key is required when Random ID is enabled");
            yield break;
        }

        string? aid = null;

        bool canAuthWithPicc = current.Picc.Key?.KeyGroup == keyGroup;

        if (canAuthWithPicc)
        {
            aid = "000000";
        }
        else
        {
            ApplicationSpecification? application = current.Applications.Values.FirstOrDefault(x => x.KeyGroup == keyGroup);
            aid = application?.Aid;
        }

        if (aid == null)
        {
            AddError("Cannot find application to use for non diversified authentication");
            yield break;
        }

        foreach (
            IDesfireOperation op in Authenticate(aid, keyGroup, nonDiversifiedKey.KeySets[currentKeySet].Keys.First(x => !x.IsKeyDiversified).KeyId)
        )
        {
            yield return op;
        }

        yield return new GetCardUidOperation();
    }

    private IEnumerable<IDesfireOperation> AdaptConfigurationSettings(TemplateSpecification current, TemplateSpecification toBe)
    {
        bool piccSettingsNeedChange = current.Picc.Config.PiccSettings != toBe.Picc.Config.PiccSettings;

        bool secureMessagingNeedChange = current.Picc.Config.SecureMessaging != toBe.Picc.Config.SecureMessaging;

        if (piccSettingsNeedChange || secureMessagingNeedChange)
        {
            if (toBe.Picc.Key == null)
            {
                AddError("PICC key required to change PICC settings");
                yield break;
            }

            foreach (IDesfireOperation op in Authenticate("000000", toBe.Picc.Key.KeyGroup, toBe.Picc.Key.Key))
            {
                yield return op;
            }
        }

        if (secureMessagingNeedChange)
        {
            yield return new SecureMessagingChangeOperation(toBe.Picc.Config.SecureMessaging);
        }

        if (piccSettingsNeedChange)
        {
            if (current.Picc.Config.PiccSettings.RandomIdEnabled != toBe.Picc.Config.PiccSettings.RandomIdEnabled)
            {
                if (!toBe.Picc.Config.PiccSettings.RandomIdEnabled)
                {
                    AddError("Cannot disable random ID.");
                }
            }
            yield return new ChangePiccSettingsOperation(toBe.Picc.Config.PiccSettings);
        }
    }

    private IEnumerable<IDesfireOperation> GetReadVariableOperations(TemplateSpecification current, TemplateSpecification toBe)
    {
        // This list represents the variables that are required to read from the card. (appId, fileId, variable)
        List<string> requiredReadingVariables = [];

        foreach (ApplicationSpecification application in toBe.Applications.Values)
        {
            foreach (FileSpecification file in application.Files.Values)
            {
                if (_requiredVariables.Contains(file.Variable))
                {
                    continue; // Skip if already required
                }
                _requiredVariables.Add(file.Variable);

                //We would require the variable to be read from the card under the following conditions:
                // 1. The file is not already in the card
                // 2. The file needs to created or re-created
                // 3. The application is not already in the card

                bool requiresReading = false;

                ApplicationSpecification? currentSpec = current.Applications.Values.FirstOrDefault(x => x.Aid == application.Aid);

                requiresReading = requiresReading || currentSpec == null;

                if (currentSpec != null)
                    requiresReading = requiresReading || ApplicationRequiresRecreation(currentSpec, application);

                if (currentSpec != null)
                {
                    FileSpecification? currentFile = currentSpec.Files.Values.FirstOrDefault(x => x.Id == file.Id);

                    requiresReading = requiresReading || currentFile == null;

                    if (currentFile != null)
                        requiresReading = requiresReading || FileRequiresRecreation(currentFile, file);
                }

                if (requiresReading)
                {
                    if (!requiredReadingVariables.Contains(file.Variable))
                    {
                        requiredReadingVariables.Add(file.Variable);
                    }
                }
            }
        }

        foreach (ApplicationSpecification application in current.Applications.Values)
        {
            foreach (FileSpecification file in application.Files.Values)
            {
                if (_requiredVariables.Contains(file.Variable))
                {
                    _requiredVariables.Remove(file.Variable);
                }

                ChangeKey readKey = ParseKey(file.ReadKey);

                if (!requiredReadingVariables.Contains(file.Variable))
                {
                    continue;
                }

                requiredReadingVariables.Remove(file.Variable);

                if (readKey.AllowNoKeys)
                {
                    yield return new ReadFromFileOperation(
                        file.Id,
                        FromFileMode(file.Mode),
                        file.DataOffsetBytes,
                        GetFileDataLength(file),
                        file.Variable
                    );
                    continue;
                }

                foreach (IDesfireOperation op in Authenticate(application.Aid, application.KeyGroup, readKey.KeyId))
                {
                    yield return op;
                }

                ReadFromFileOperation readOperation = new(
                    file.Id,
                    FromFileMode(file.Mode),
                    file.DataOffsetBytes,
                    GetFileDataLength(file),
                    file.Variable
                );
                yield return readOperation;
            }
        }
    }

    private IEnumerable<IDesfireOperation> CalculatePiccChanges(PiccSpecification current, PiccSpecification toBe)
    {
        if (toBe.Key == null)
        {
            yield break;
        }

        if (toBe.Key != current.Key)
        {
            if (current.Key == null)
            {
                AddError("PICC key required to change PICC key");
                yield break;
            }

            // Force a fresh PICC authentication. The cached key number alone is not enough here,
            // because prior operations may have authenticated the same key number in a different scope.
            currentAuthenticatedKey = null;

            foreach (IDesfireOperation op in Authenticate("000000", current.Key))
            {
                yield return op;
            }

            KeyGroupData keyGroup = GetKeyGroupData(toBe.Key.KeyGroup, toBe.Key.KeyGroupName);

            yield return new ChangePiccKeyOperation(keyGroup, currentKeySet, toBe.Key.Key, 0x00);
            currentAuthenticatedKey = null;

            if (current.KeySettings != toBe.KeySettings)
            {
                foreach (IDesfireOperation op in Authenticate("000000", toBe.Key.KeyGroup, toBe.Key.Key))
                {
                    yield return op;
                }

                yield return new ChangePiccKeySettingsOperation(
                    new PiccKeySettings
                    {
                        AllowCreateAndDeleteWithoutMasterKey = toBe.KeySettings.AllowCreateDelete,
                        AllowDamKeys = toBe.KeySettings.AllowDamKeys,
                        MasterKeyReadOnly = !toBe.KeySettings.MasterKeyChangeable,
                        KeySettingsReadOnly = !toBe.KeySettings.Changeable,
                        FreeDirectoryListing = toBe.KeySettings.FreeDirectoryListing,
                    }
                );
            }
        }
        else
        {
            if (current.KeySettings != toBe.KeySettings)
            {
                if (current.Key == null)
                {
                    AddError("PICC key required to change PICC key settings");
                    yield break;
                }

                foreach (IDesfireOperation op in Authenticate("000000", current.Key.KeyGroup, current.Key.Key))
                {
                    yield return op;
                }

                yield return new ChangePiccKeySettingsOperation(
                    new PiccKeySettings
                    {
                        AllowCreateAndDeleteWithoutMasterKey = toBe.KeySettings.AllowCreateDelete,
                        AllowDamKeys = toBe.KeySettings.AllowDamKeys,
                        MasterKeyReadOnly = !toBe.KeySettings.MasterKeyChangeable,
                        KeySettingsReadOnly = !toBe.KeySettings.Changeable,
                        FreeDirectoryListing = toBe.KeySettings.FreeDirectoryListing,
                    }
                );
            }
        }
    }

    private bool ApplicationRequiresRecreation(ApplicationSpecification currentApp, ApplicationSpecification toBeApp)
    {
        KeyGroupData oldKeyGroup = GetKeyGroupData(currentApp.KeyGroup, currentApp.KeyGroupName);
        KeyGroupData newKeyGroup = GetKeyGroupData(toBeApp.KeyGroup, toBeApp.KeyGroupName);
        ChangeKey changeKey = ParseKey(currentApp.KeySettings.ChangeKey);

        bool needToRecreateApp = !currentApp.KeySettings.Changeable && currentApp.KeySettings != toBeApp.KeySettings;

        needToRecreateApp =
            needToRecreateApp
            || (
                toBeApp.KeyGroup != currentApp.KeyGroup
                && (
                    !currentApp.KeySettings.Changeable
                    || oldKeyGroup.KeyType != newKeyGroup.KeyType
                    || oldKeyGroup.KeySets.Length != newKeyGroup.KeySets.Length
                    || oldKeyGroup.KeySets.First().Keys.Length != newKeyGroup.KeySets.First().Keys.Length
                    || changeKey.AllowNoKeys
                )
            );

        return needToRecreateApp;
    }

    private bool FileRequiresRecreation(FileSpecification currentFile, FileSpecification toBeFile)
    {
        return currentFile.Size != toBeFile.Size || currentFile.Mode != toBeFile.Mode;
    }

    private IEnumerable<IDesfireOperation> CalculateApplicationChanges(TemplateSpecification current, TemplateSpecification toBe)
    {
        List<ApplicationSpecification> toBeApps = toBe.Applications.Values.ToList();
        List<ApplicationSpecification> currentApps = current.Applications.Values.ToList();

        List<ApplicationSpecification> addedApplication = toBeApps.Where(x => !currentApps.Any(y => y.Aid == x.Aid)).ToList();
        List<ApplicationSpecification> removedApplication = currentApps.Where(x => !toBeApps.Any(y => y.Aid == x.Aid)).ToList();
        List<ApplicationSpecification> updatedApplication = toBeApps.Where(x => currentApps.Any(y => y.Aid == x.Aid)).ToList();

        foreach (ApplicationSpecification app in updatedApplication)
        {
            ApplicationSpecification currentApp = currentApps.First(x => x.Aid == app.Aid);
            ApplicationSpecification toBeApp = app;

            KeyGroupData oldKeyGroup = GetKeyGroupData(currentApp.KeyGroup, currentApp.KeyGroupName);
            KeyGroupData newKeyGroup = GetKeyGroupData(toBeApp.KeyGroup, toBeApp.KeyGroupName);

            ChangeKey changeKey = ParseKey(currentApp.KeySettings.ChangeKey);

            if (ApplicationRequiresRecreation(currentApp, toBeApp))
            {
                addedApplication.Add(toBeApp);
                removedApplication.Add(currentApp);
                break;
            }

            if (currentApp.KeySettings != toBeApp.KeySettings)
            {
                foreach (IDesfireOperation op in Authenticate(currentApp.Aid, currentApp.KeyGroup, changeKey.KeyId))
                {
                    yield return op;
                }

                yield return new ChangeApplicationKeySettingsOperation(
                    new ApplicationKeySettings
                    {
                        ChangeKey = ParseKey(toBeApp.KeySettings.ChangeKey),
                        KeySettingsReadOnly = !toBeApp.KeySettings.Changeable,
                        FreeDirectoryListing = toBeApp.KeySettings.FreeDirectoryListing,
                        MasterKeyReadOnly = !toBeApp.KeySettings.MasterKeyChangeable,
                        AllowCreateAndDeleteWithoutMasterKey = toBeApp.KeySettings.AllowCreateDelete,
                    }
                );

                changeKey = ParseKey(toBeApp.KeySettings.ChangeKey);
            }

            if (toBeApp.KeyGroup != currentApp.KeyGroup)
            {
                foreach (IDesfireOperation op in Authenticate(currentApp.Aid, currentApp.KeyGroup, changeKey.KeyId))
                {
                    yield return op;
                }

                for (int i = 0; i < oldKeyGroup.KeySets[currentKeySet].Keys.Length; i++)
                {
                    if (i == changeKey.KeyId)
                    {
                        continue;
                    }

                    yield return new ChangeKeyOperation(oldKeyGroup, newKeyGroup, currentKeySet, i, 0x01);
                }

                yield return new ChangeKeyOperation(oldKeyGroup, newKeyGroup, currentKeySet, changeKey.KeyId, 0x01);

                currentAuthenticatedKey = null;
            }

            List<FileSpecification> addFiles = toBeApp
                .Files.Where(x => !currentApp.Files.Any(y => y.Value.Id == x.Value.Id))
                .Select(x => x.Value)
                .ToList();
            List<FileSpecification> removedFiles = currentApp
                .Files.Where(x => !toBeApp.Files.Any(y => y.Value.Id == x.Value.Id))
                .Select(x => x.Value)
                .ToList();

            IEnumerable<FileSpecification> updatedFiles = toBeApp
                .Files.Where(x => currentApp.Files.Any(y => y.Value.Id == x.Value.Id))
                .ToList()
                .Select(x => x.Value);

            foreach (FileSpecification newFile in updatedFiles)
            {
                FileSpecification oldFile = currentApp.Files.First(x => x.Value.Id == newFile.Id).Value;
                bool needsWrite = oldFile.Variable != newFile.Variable || oldFile.Encoding != newFile.Encoding;
                bool rightsChanged =
                    oldFile.Mode != newFile.Mode
                    || oldFile.ChangeKey != newFile.ChangeKey
                    || oldFile.ReadKey != newFile.ReadKey
                    || oldFile.WriteKey != newFile.WriteKey
                    || oldFile.ReadWriteKey != newFile.ReadWriteKey;
                DesfireFileAccessRights finalAccessRights = BuildFileAccessRights(newFile);
                
                if (FileRequiresRecreation(oldFile, newFile))
                {
                    removedFiles.Add(oldFile);
                    addFiles.Add(newFile);
                }
                else if (needsWrite)
                {
                    DesfireFileOptions fileOptions = new()
                    {
                        CommunicationMode = FromFileMode(newFile.Mode),
                        AdditionalAccessRights = false,
                        SecureDynamicMessaging = false,
                    };

                    changeKey = ParseKey(newFile.ChangeKey);

                    foreach (IDesfireOperation op in Authenticate(currentApp.Aid, toBeApp.KeyGroup, changeKey.KeyId))
                    {
                        yield return op;
                    }

                    ChangeKey writeKey = GetWritableKey(finalAccessRights);
                    foreach (IDesfireOperation op in Authenticate(currentApp.Aid, toBeApp.KeyGroup, writeKey.KeyId))
                    {
                        yield return op;
                    }

                    yield return new WriteToFileOperation(
                        newFile.Id,
                        FromFileMode(newFile.Mode),
                        newFile.Variable,
                        newFile.Encoding,
                        newFile.DataOffsetBytes,
                        GetFileDataLength(newFile)
                    );
                }
            }

            if (!toBeApp.KeySettings.AllowCreateDelete && removedFiles.Count != 0)
            {
                foreach (IDesfireOperation op in Authenticate(currentApp.Aid, toBeApp.KeyGroup, 0))
                {
                    yield return op;
                }
            }

            foreach (FileSpecification file in removedFiles)
            {
                yield return new DeleteFileOperation(file.Id);
            }

            foreach (FileSpecification file in addFiles)
            {
                foreach (IDesfireOperation op in CreateFile(toBeApp, file))
                {
                    yield return op;
                }
            }
        }

        //IF PICC key is required to delete apps
        if (removedApplication.Count != 0 && !current.Picc.AllowCreateDelete)
        {
            string? key = toBe.Picc.Key?.KeyGroup ?? current.Picc.Key?.KeyGroup;

            if (key == null)
            {
                AddError("PICC key required to delete application");
                yield break;
            }

            foreach (IDesfireOperation op in Authenticate("000000", key, 0))
            {
                yield return op;
            }
        }

        foreach (ApplicationSpecification app in removedApplication)
        {
            yield return new DeleteApplicationOperation(DesfireApplicationId.Create(app.Aid));
        }

        foreach (ApplicationSpecification app in addedApplication)
        {
            PiccSpecification piccForCreateAuth = IsBlankPicc(current.Picc) ? current.Picc : toBe.Picc;

            foreach (IDesfireOperation op in CreateApplication(piccForCreateAuth, app))
            {
                yield return op;
            }
        }
    }

    private static bool IsBlankPicc(PiccSpecification picc) => picc.Key?.KeyGroup == BlankAutoDetectPlaceholderKeyGroup;

    private IEnumerable<IDesfireOperation> CreateApplication(PiccSpecification picc, ApplicationSpecification app)
    {
        if (!picc.AllowCreateDelete)
        {
            if (picc.Key == null)
            {
                AddError("PICC key required to create application");
                yield break;
            }

            foreach (IDesfireOperation op in Authenticate("000000", picc.Key.KeyGroup, 0))
            {
                yield return op;
            }
        }

        KeyGroupData appKeyGroup = GetKeyGroupData(app.KeyGroup, app.KeyGroupName);

        ApplicationKeySettings keySettings = new()
        {
            ChangeKey = ParseKey(app.KeySettings.ChangeKey),
            MasterKeyReadOnly = false,
            KeySettingsReadOnly = false,
            AllowCreateAndDeleteWithoutMasterKey = app.KeySettings.AllowCreateDelete,
            FreeDirectoryListing = app.KeySettings.FreeDirectoryListing,
        };

        ApplicationSettings applicationSettings = new()
        {
            KeyType = appKeyGroup.KeyType,
            ApplicationKeys = (ushort)appKeyGroup.KeySets[currentKeySet].Keys.Length,
            ExtendedApplicationSettings = false,
            Use2ByteFileIdentifiers = app.Use2BytesFileIdentifier,
        };

        yield return new CreateApplicationOperation(
            BuildApplicationDescription(app, appKeyGroup, keySettings, applicationSettings)
        );

        SelectApplicationOperation? selectAppOperation = SelectApplication(app.Aid);

        if (selectAppOperation != null)
        {
            yield return selectAppOperation;
        }

        foreach (IDesfireOperation op in AuthenticateDefault(app.Aid, 0, appKeyGroup.KeyType))
        {
            yield return op;
        }

        for (int i = 0; i < appKeyGroup.KeySets[currentKeySet].Keys.Length; i++)
        {
            if (currentAuthenticatedKey == i)
            {
                //Skip the change key, to avoid losing current session
                continue;
            }

            yield return new ChangeKeyOperation(DefaultKeyGroup(appKeyGroup.KeyType), appKeyGroup, currentKeySet, i, 0x00);
        }

        yield return new ChangeKeyOperation(DefaultKeyGroup(appKeyGroup.KeyType), appKeyGroup, currentKeySet, currentAuthenticatedKey ?? 0, 0x00);

        //Current key is changed, so authentication is lost
        currentAuthenticatedKey = null;

        foreach (FileSpecification file in app.Files.Values)
        {
            foreach (IDesfireOperation operation in CreateFile(app, file))
            {
                yield return operation;
            }
        }

        if (!app.KeySettings.MasterKeyChangeable || !app.KeySettings.Changeable)
        {
            //Ensure this is a sesperate instance of the key settings since this is fully run before
            //exection time
            var updatedKeySettings = new ApplicationKeySettings
            {
                ChangeKey = ParseKey(app.KeySettings.ChangeKey),
                MasterKeyReadOnly = !app.KeySettings.MasterKeyChangeable,
                KeySettingsReadOnly = !app.KeySettings.Changeable,
                AllowCreateAndDeleteWithoutMasterKey = app.KeySettings.AllowCreateDelete,
                FreeDirectoryListing = app.KeySettings.FreeDirectoryListing,
            };

            foreach (IDesfireOperation op in Authenticate(app.Aid, app.KeyGroup, 0))
            {
                yield return op;
            }

            yield return new ChangeApplicationKeySettingsOperation(updatedKeySettings);
        }
    }

    private IEnumerable<IDesfireOperation> CreateFile(ApplicationSpecification application, FileSpecification file)
    {
        if (!application.KeySettings.AllowCreateDelete)
        {
            foreach (IDesfireOperation op in Authenticate(application.Aid, application.KeyGroup, 0))
            {
                yield return op;
            }
        }

        DesfireFileOptions fileOptions = new()
        {
            CommunicationMode = FromFileMode(file.Mode),
            AdditionalAccessRights = false,
            SecureDynamicMessaging = false,
        };

        DesfireFileAccessRights finalAccessRights = BuildFileAccessRights(file);

        StandardDesfireFile desfireFile = DesfireFile.CreateStandardFile(file.Id, fileOptions, finalAccessRights, file.Size, 0);

        yield return new CreateFileOperation(desfireFile);

        ChangeKey writeKey = GetWritableKey(finalAccessRights);
        foreach (IDesfireOperation op in Authenticate(application.Aid, application.KeyGroup, writeKey.KeyId))
        {
            yield return op;
        }

        yield return new WriteToFileOperation(
            file.Id,
            fileOptions.CommunicationMode,
            file.Variable,
            file.Encoding,
            file.DataOffsetBytes,
            GetFileDataLength(file)
        );
    }

    private static ApplicationDescription BuildApplicationDescription(
        ApplicationSpecification app,
        KeyGroupData appKeyGroup,
        ApplicationKeySettings keySettings,
        ApplicationSettings applicationSettings)
    {
        ApplicationDescription description = ApplicationDescription
            .NewApplication(DesfireApplicationId.Create(app.Aid))
            .KeySettings(keySettings)
            .Settings(applicationSettings);

        if (IsoDfNameUtilities.TryGetBytes(app.IsoDfName, 1, 16, out byte[] isoDfName))
        {
            description = description.IsoDfName(isoDfName);
        }

        if (appKeyGroup.KeySets.Length > 1)
        {
            description = description
                .ExtendedSettings(new ApplicationExtendedSettings { AdditionalKeySets = true })
                .ActiveKeySetVersion(0)
                .KeySets((ushort)appKeyGroup.KeySets.Length);

            if (appKeyGroup.KeyType == KeyType.Aes)
            {
                description = description.Only16ByteKeys();
            }

            description = description.KeySetKeySettings(keySettings);
        }

        return description;
    }

    public static KeyGroupData DefaultKeyGroup(KeyType keyType)
    {
        int keyLength = CryptoHelper.GetKeySize(keyType);

        string keyData = Convert.ToHexString(new byte[keyLength]);

        Key[] keys =
        [
            .. Enumerable.Repeat(
                new Key
                {
                    Value = keyData,
                    IsKeyDiversified = false,
                    KeyId = 0,
                },
                16
            ),
        ];

        return new KeyGroupData
        {
            KeyType = keyType,
            KeyDiversificationStrategy = null,
            KeySets = [new KeySet { Id = 0, Keys = keys }],
        };
    }

    private static ChangeKey ParseKey(string key)
    {
        // sentinel: keep legacy aliases for now so older specs still parse cleanly.
        if (!int.TryParse(key, out int changeKeyIndex))
        {
            string normalizedKey = key.Trim().ToLowerInvariant();

            if (normalizedKey is "any" or "e")
            {
                return ChangeKey.AnyApplicationKey();
            }

            if (normalizedKey is "none" or "f")
            {
                return ChangeKey.ReadOnly();
            }
        }

        return ChangeKey.SpecificKey(changeKeyIndex);
    }

    private static DesfireFileAccessRights BuildFileAccessRights(FileSpecification file)
    {
        return new DesfireFileAccessRights
        {
            ChangeKey = ParseKey(file.ChangeKey),
            ReadKey = ParseKey(file.ReadKey),
            WriteKey = ParseKey(file.WriteKey),
            ReadWriteKey = ParseKey(file.ReadWriteKey),
        };
    }

    private static ChangeKey GetWritableKey(DesfireFileAccessRights accessRights)
    {
        if (!accessRights.WriteKey.AllowNoKeys)
        {
            return accessRights.WriteKey;
        }

        if (!accessRights.ReadWriteKey.AllowNoKeys)
        {
            return accessRights.ReadWriteKey;
        }

        return accessRights.WriteKey;
    }

    private static int GetFileDataLength(FileSpecification file)
    {
        return file.DataLengthBytes > 0 ? file.DataLengthBytes : file.Size;
    }

    public static CommunicationMode FromFileMode(FileMode mode)
    {
        return mode switch
        {
            FileMode.Mac => CommunicationMode.Cmac,
            FileMode.Encrypted => CommunicationMode.Enciphered,
            _ => CommunicationMode.Plain,
        };
    }

    private void AddError(string message)
    {
        _errors.Add(new ScriptError { Message = message });
    }

    private SelectApplicationOperation? SelectApplication(string aid)
    {
        if (currentSelectedApp != aid)
        {
            DesfireApplicationId applicationId = DesfireApplicationId.Create(aid);
            SelectApplicationOperation operation = new(applicationId);
            currentAuthenticatedKey = null;
            currentSelectedApp = aid;
            return operation;
        }

        return null;
    }

    private IEnumerable<IDesfireOperation> AuthenticateDefault(string aid, int key, KeyType keyType)
    {
        SelectApplicationOperation? selectOperation = SelectApplication(aid);
        if (selectOperation != null)
        {
            yield return selectOperation;
        }

        if (currentAuthenticatedKey == null || currentAuthenticatedKey.Value != key)
            yield return new AuthenticateDefaultOperation(DefaultKeyGroup(keyType), 0);

        currentAuthenticatedKey = key;
    }

    private IEnumerable<IDesfireOperation> Authenticate(string aid, string keyGroup, int keyId)
    {
        return Authenticate(
            aid,
            new KeySpecification
            {
                Key = keyId,
                KeyGroup = keyGroup,
                KeySet = currentKeySet,
            }
        );
    }

    private IEnumerable<IDesfireOperation> Authenticate(string aid, KeySpecification keySpecification)
    {
        SelectApplicationOperation? selectOperation = SelectApplication(aid);
        if (selectOperation != null)
        {
            yield return selectOperation;
        }

        if (currentAuthenticatedKey == null || currentAuthenticatedKey.Value != keySpecification.Key)
        {
            if (keySpecification.KeyGroup == BlankAutoDetectPlaceholderKeyGroup)
            {
                yield return new AuthenticateDefaultProbeOperation(keySpecification.Key);
                currentAuthenticatedKey = keySpecification.Key;
                yield break;
            }

            KeyGroupData keyGroup = GetKeyGroupData(keySpecification.KeyGroup, keySpecification.KeyGroupName);
            yield return new AuthenticateOperation(keyGroup, keySpecification.KeySet, keySpecification.Key);

            currentAuthenticatedKey = keySpecification.Key;
        }
    }

    public static byte[] CalculateKey(ExecutionState plan, KeyGroupData keyGroup, int keySet, int keyId)
    {
        Key keyData = keyGroup.KeySets[keySet].Keys[keyId];
        if (keyGroup.KeyDiversificationStrategy == null || !keyData.IsKeyDiversified)
        {
            return Convert.FromHexString(keyData.Value);
        }

        byte[] diversificationInput = new byte[31];
        int offset = 0;

        foreach (DiversificationInput input in keyGroup.KeyDiversificationStrategy.Inputs)
        {
            byte[] inputData = input.Option switch
            {
                DiversificationInputOptions.Uid => Convert.FromHexString(plan.CardUid),
                DiversificationInputOptions.Uid4Bytes => Convert.FromHexString(plan.CardUid),
                DiversificationInputOptions.ApplicationId => Convert.FromHexString(plan.SelectedApplication),
                DiversificationInputOptions.ApplicationIdReversed => Convert.FromHexString(plan.SelectedApplication).Reverse().ToArray(),
                DiversificationInputOptions.KeyNo => [(byte)keyId],
                DiversificationInputOptions.FixedHexValue => Convert.FromHexString(input.Data!),
                _ => throw new ArgumentOutOfRangeException(),
            };
            Array.Copy(inputData, 0, diversificationInput, offset, inputData.Length);
            offset += inputData.Length;
        }

        byte[] diversifiedKeyData = (keyGroup.KeyType, keyGroup.KeyDiversificationStrategy.Algorithm) switch
        {
            (KeyType.Aes, KeyDiversificationAlgorithm.NxpAn10922) => CryptoHelper.DiversifyAesKey(
                Convert.FromHexString(keyData.Value),
                diversificationInput
            ),

            _ => throw new NotSupportedException(
                $"{keyGroup.KeyType} with {keyGroup.KeyDiversificationStrategy.Algorithm} algorithm is not supported."
            ),
        };

        return diversifiedKeyData;
    }
}

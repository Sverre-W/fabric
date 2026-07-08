namespace Fabric.Hardware.Desfire.Encoding.Specifications;

public class TemplateSpecification
{
    public PiccSpecification Picc { get; set; } = default!;
    public Dictionary<string, ApplicationSpecification> Applications { get; set; } = [];
}

public class PiccSpecification
{
    public KeySpecification? Key { get; set; }
    public bool AllowCreateDelete { get; set; }
    public PiccKeySettingsSpecification KeySettings { get; set; } = new();
    public PiccConfigurationSpecification Config { get; set; } = new();
}

public record KeySettingsSpecification
{
    public bool Changeable { get; set; } = true;
    public bool MasterKeyChangeable { get; set; } = true;
    public bool FreeDirectoryListing { get; set; } = true;
    public bool AllowCreateDelete { get; set; } = true;
}

public record ApplicationKeySettingsSpecification : KeySettingsSpecification
{
    public string ChangeKey { get; set; } = "0";
}

public record PiccKeySettingsSpecification : KeySettingsSpecification
{
    public bool AllowDamKeys { get; set; } = true;
}

public class ApplicationSpecification
{
    public string Aid { get; set; } = string.Empty;
    public string IsoDfName { get; set; } = string.Empty;
    public string KeyGroupName { get; set; } = string.Empty;
    public string KeyGroup { get; set; } = default!;
    public ApplicationKeySettingsSpecification KeySettings { get; set; } = new();
    public SecureMessingConfiguration SecureMessing { get; set; } = new();
    public bool Use2BytesFileIdentifier { get; set; }
    public Dictionary<string, FileSpecification> Files { get; set; } = [];
}

public enum FileMode
{
    Plain,
    Mac,
    Encrypted,
}

public class FileSpecification
{
    public int Id { get; set; }
    public FileMode Mode { get; set; }
    public string Variable { get; set; } = default!;
    public int Size { get; set; } = 0;
    /// <summary>
    ///     Zero-based byte offset of the payload inside the file.
    /// </summary>
    public int DataOffsetBytes { get; set; } = 0;
    /// <summary>
    ///     Optional logical payload length. If zero, the file size is used.
    /// </summary>
    public int DataLengthBytes { get; set; } = 0;
    /// <summary>
    ///     How to encode the bound variable before writing it to the file. Supported values: <c>text</c>, <c>hex</c>, <c>uint:7:be</c>, <c>uint:7:le</c>.
    /// </summary>
    public string Encoding { get; set; } = default!;
    public string ReadKey { get; set; } = "0";
    public string WriteKey { get; set; } = "0";
    public string ReadWriteKey { get; set; } = "0";
    public string ChangeKey { get; set; } = "0";
}

public record PiccConfigurationSpecification
{
    public PiccSettings PiccSettings { get; set; } = new();
    public SecureMessingConfiguration SecureMessaging { get; set; } = new();
}

public record SecureMessingConfiguration
{
    public bool DisableD40 { get; set; }
    public bool DisableEv1 { get; set; }
    public bool DisableEv2Chaining { get; set; }
}

public record PiccSettings
{
    public bool EnableLegacyRandomId { get; set; }
    public bool IsoVirtualCardMandatory { get; set; }
    public bool ProximityCheckMandatory { get; set; }
    public bool RandomIdEnabled { get; set; }
    public bool DisableCardFormat { get; set; }
}

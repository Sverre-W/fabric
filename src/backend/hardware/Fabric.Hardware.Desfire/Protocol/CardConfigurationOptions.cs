namespace Fabric.Hardware.Desfire.Protocol;

//See page 54 & 195 - DS487033

/// <summary>
///     Different configuration options
/// </summary>
public enum CardConfigurationOptions
{
    Picc = 0x00,
    DefaultKeysUpdate = 0x01,
    AtsUpdate = 0x02,
    SakUpdate = 0x03,
    SecureMessagingConfiguration = 0x04,
    CapabilityData = 0x05,
    VcInstallationIdentifier = 0x06,
    AtqaUpdate = 0x0c,
}

using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Protocol;

public class CardConfiguration
{
    private CardConfiguration(CardConfigurationOptions option, byte[] data)
    {
        Option = option;
        Data = data;
    }

    public CardConfigurationOptions Option { get; private init; }
    public byte[] Data { get; private init; }

    /// <summary>
    ///     Sets the secure configuration settings for the current selected <see cref="DesfireApplicationId" />
    /// </summary>
    /// <param name="ev2ChainingDisabled"></param>
    /// <param name="ev1Disabled"></param>
    /// <param name="d40Disabled"></param>
    /// <returns></returns>
    public static CardConfiguration SetSecureMessaging(bool ev2ChainingDisabled, bool ev1Disabled, bool d40Disabled)
    {
        bool[] bits = new bool[16];

        bits[10] = ev2ChainingDisabled;
        bits[9] = ev1Disabled;
        bits[8] = d40Disabled;

        return new CardConfiguration(CardConfigurationOptions.SecureMessagingConfiguration, BitUtilities.BitArrayToByteArray(bits));
    }

    /// <summary>
    ///     Set's the configuration of the PICC
    /// </summary>
    /// <param name="disableFormat">Disable Cmd.Format at PICC level only, once disabled cannot be enabled again</param>
    /// <param name="enableRandomId">Enable random ID, Once enabled, Random ID cannot be disabled anymore </param>
    /// <param name="mandatoryProximityCheck">Indicates a mandatory proximity check after implicit or explicit virtual card selection, Once activated, it cannot be deactivated</param>
    /// <param name="mandatoryVcAuth">Indicates the mandatory use of Cmd.ISOSelect and Cmd.ISOExternal Authenticate for Virtual Card Selection, cannot be deactivated again</param>
    /// <param name="useLegacyRandomId">Enable legacy random ID compatible with DESFire EV1</param>
    /// <returns></returns>
    public static CardConfiguration SetPiccConfig(
        bool disableFormat,
        bool enableRandomId,
        bool mandatoryProximityCheck,
        bool mandatoryVcAuth,
        bool useLegacyRandomId
    )
    {
        bool[] bits = new bool[8];

        bits[0] = disableFormat;
        bits[1] = enableRandomId;
        bits[2] = mandatoryProximityCheck;
        bits[3] = mandatoryVcAuth;
        bits[5] = useLegacyRandomId;

        return new CardConfiguration(CardConfigurationOptions.Picc, BitUtilities.BitArrayToByteArray(bits));
    }

    /// <summary>
    ///     Set configuration for delegated applications (Must select a delegated application first)
    /// </summary>
    /// <param name="disableFormat">Disables Cmd.Format for the selected application</param>
    /// <returns></returns>
    public static CardConfiguration SetDelegatedConfig(bool disableFormat)
    {
        bool[] bits = new bool[8];

        bits[0] = disableFormat;

        return new CardConfiguration(CardConfigurationOptions.Picc, BitUtilities.BitArrayToByteArray(bits));
    }

    /// <summary>
    ///     Updates the default application key value for new applications
    /// </summary>
    /// <param name="key">If key is less than 24 bytes, padding will be added in case of a 3KDES application</param>
    /// <param name="version"></param>
    /// <returns></returns>
    public static CardConfiguration SetDefaultApplicationKey(byte[] key, byte version)
    {
        if (key.Length > 24)
        {
            throw new ArgumentException("Default application key cannot exceed 24 bytes", nameof(key));
        }

        byte[] data = new byte[25];
        Array.Copy(key, data, key.Length);
        data[24] = version;

        return new CardConfiguration(CardConfigurationOptions.DefaultKeysUpdate, data);
    }
}

using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Protocol.Authentication;
using Fabric.Hardware.Desfire.Session;
using Fabric.Hardware.Desfire.Utils;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Desfire.Services;

public record NoData
{
    public static NoData Instance { get; } = new();
}

/// <summary>
///     A wrapper for the <see cref="ICardReader" /> that implements the DESFire commands
/// </summary>
public class DesfireReader : IDisposable
{
    public ILogger Logger { get; }

    private readonly IRfidEncoder _cardEncoder;
    private readonly bool _disposeCardReader;

    /// <summary>
    ///     Initialize a new Desfire Reader
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="encoder">The card reader</param>
    /// <param name="disposeCardReader">Default true, on dispose the underlying card reader is also disposed</param>
    public DesfireReader(ILogger logger, IRfidEncoder encoder, bool disposeCardReader = true)
    {
        Logger = logger;
        Session = new PlainDesfireSession(Logger, encoder);
        SelectedApplication = DesfireApplicationId.PICC;
        _cardEncoder = encoder;
        _disposeCardReader = disposeCardReader;
    }

    /// <summary>
    ///     The current authenticated session or <see cref="PlainDesfireSession" /> if unauthenticated
    /// </summary>
    public IDesfireSession Session { get; set; }

    public DesfireApplicationId SelectedApplication { get; private set; } =
        DesfireApplicationId.PICC;

    public void Dispose()
    {
        if (_disposeCardReader)
        {
            _cardEncoder.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Ensure the session transitions to <see cref="PlainDesfireSession" /> after an error occured
    /// </summary>
    /// <param name="frame"></param>
    private void UpdateSessionOnError(DesfireResponseFrame frame)
    {
        if (frame.StatusCode.IsSuccess() || frame.StatusCode == DesfireStatusCode.AdditionalFrame)
        {
            return;
        }

        //If an error occurs we need to switch back to an unauthenticated state
        Session = new PlainDesfireSession(Logger, _cardEncoder);
    }

    private static void WriteUInt24LittleEndian(Span<byte> destination, int value)
    {
        destination[0] = (byte)value;
        destination[1] = (byte)(value >> 8);
        destination[2] = (byte)(value >> 16);
    }

    /// <summary>
    ///     Authenticate via AES
    /// </summary>
    /// <param name="keyId">The id of key to authenticate with</param>
    /// <param name="keyData">The actual key</param>
    /// <param name="ct"></param>
    /// <param name="randomA"></param>
    /// <returns>True if authentication is successful</returns>
    public async Task<IDesfireResponse> AuthenticateEv2(
        DesfireKeyId keyId,
        byte[] keyData,
        byte[]? randomA = null,
        CancellationToken ct = default
    )
    {
        //Make sure we're unauthenticated in the first place
        Session = new PlainDesfireSession(Logger, _cardEncoder);

        DesfireResponseFrame authResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.AuthenticateEv2First,
                Header = [(byte)keyId, 0x00],
            },
            ct
        );

        if (authResponse.StatusCode != DesfireStatusCode.AdditionalFrame)
        {
            return DesfireResponse.Create(
                authResponse.StatusCode.IsSuccess()
                    ? DesfireStatusCode.AuthenticationError
                    : authResponse.StatusCode
            );
        }

        AuthenticationHelperEv2SecureMessaging authenticationHelper =
            AuthenticationHelper.Ev2SecureMessaging(KeyType.Aes, keyData, randomA);
        byte[] challengeResponse = authenticationHelper.Challenge(authResponse.Data);

        DesfireResponseFrame authChallengeResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.NextFrame,
                Data = challengeResponse,
            },
            ct
        );

        if (authChallengeResponse.StatusCode != DesfireStatusCode.Success)
        {
            return DesfireResponse.Create(authChallengeResponse.StatusCode);
        }

        authenticationHelper.ChallengeResume(authChallengeResponse.Data);

        Session = new Ev2SecureMessaging(
            Logger,
            _cardEncoder,
            keyId,
            authenticationHelper.TransactionIdentifier,
            authenticationHelper.SessionKey,
            authenticationHelper.SessionKeyMacing
        );
        return DesfireResponse.Create(authChallengeResponse.StatusCode);
    }

    /// <summary>
    ///     Authenticate via AES
    /// </summary>
    /// <param name="keyId">The id of key to authenticate with</param>
    /// <param name="keyData">The actual key</param>
    /// <param name="ct"></param>
    /// <param name="randomA"></param>
    /// <returns>True if authentication is successful</returns>
    public async Task<IDesfireResponse> AuthenticateAes(
        DesfireKeyId keyId,
        byte[] keyData,
        byte[]? randomA = null,
        CancellationToken ct = default
    )
    {
        //Make sure we're unauthenticated in the first place
        Session = new PlainDesfireSession(Logger, _cardEncoder);

        DesfireResponseFrame authResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.AuthenticateAes,
                Header = [(byte)keyId],
            },
            ct
        );

        if (authResponse.StatusCode != DesfireStatusCode.AdditionalFrame)
        {
            return DesfireResponse.Create(authResponse.StatusCode);
        }

        AuthenticationHelper authenticationHelper = AuthenticationHelper.Ev1SecureMessaging(
            KeyType.Aes,
            keyData,
            randomA
        );

        byte[] challengeResponse = authenticationHelper.Challenge(authResponse.Data);

        DesfireResponseFrame authChallengeResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.NextFrame,
                Data = challengeResponse,
            },
            ct
        );

        if (authChallengeResponse.StatusCode != DesfireStatusCode.Success)
        {
            return DesfireResponse.Create(
                authChallengeResponse.StatusCode.IsSuccess()
                    ? DesfireStatusCode.AuthenticationError
                    : authChallengeResponse.StatusCode
            );
        }

        authenticationHelper.ChallengeResume(authChallengeResponse.Data);

        Session = new Ev1SecureMessaging(
            Logger,
            _cardEncoder,
            keyId,
            authenticationHelper.SessionKey,
            KeyType.Aes
        );
        return DesfireResponse.Create(authChallengeResponse.StatusCode);
    }

    /// <summary>
    ///     Authenticate via 2TDES, 3TDES
    /// </summary>
    /// <param name="keyId">The id of key to authenticate with</param>
    /// <param name="keyData">The actual key</param>
    /// <param name="keyType">The key type</param>
    /// <param name="ct"></param>
    /// <param name="randomA"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If an AES key is used to authenticate</exception>
    public async Task<IDesfireResponse> AuthenticateIso(
        DesfireKeyId keyId,
        byte[] keyData,
        KeyType keyType,
        byte[]? randomA = null,
        CancellationToken ct = default
    )
    {
        //Make sure we're unauthenticated in the first place
        Session = new PlainDesfireSession(Logger, _cardEncoder);

        if (keyType == KeyType.Aes)
        {
            throw new ArgumentException(
                $"{keyType} is not supported for this authentication",
                nameof(keyType)
            );
        }

        DesfireResponseFrame authResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.AuthenticateIso,
                Header = [(byte)keyId],
            },
            ct
        );

        if (authResponse.StatusCode != DesfireStatusCode.AdditionalFrame)
        {
            return DesfireResponse.Create(
                authResponse.StatusCode.IsSuccess()
                    ? DesfireStatusCode.AuthenticationError
                    : authResponse.StatusCode
            );
        }

        AuthenticationHelper authenticationHelper = AuthenticationHelper.Ev1SecureMessaging(
            keyType,
            keyData,
            randomA
        );

        byte[] challengeResponse = authenticationHelper.Challenge(authResponse.Data);

        DesfireResponseFrame authChallengeResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.NextFrame,
                Data = challengeResponse,
            },
            ct
        );

        if (authChallengeResponse.StatusCode != DesfireStatusCode.Success)
        {
            return DesfireResponse.Create(authChallengeResponse.StatusCode);
        }

        authenticationHelper.ChallengeResume(authChallengeResponse.Data);
        Session = new Ev1SecureMessaging(
            Logger,
            _cardEncoder,
            keyId,
            authenticationHelper.SessionKey,
            keyType
        );
        return DesfireResponse.Create(authChallengeResponse.StatusCode);
    }

    public async Task<IDesfireResponse> Authenticate(
        DesfireKeyId keyId,
        byte[] keyData,
        KeyType keyType,
        byte[]? randomA = null,
        CancellationToken ct = default
    )
    {
        //Make sure we're unauthenticated in the first place
        Session = new PlainDesfireSession(Logger, _cardEncoder);

        if (keyType == KeyType.Aes)
        {
            throw new ArgumentException(
                $"{keyType} is not supported for this authentication",
                nameof(keyType)
            );
        }

        DesfireResponseFrame authResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.Authenticate,
                Header = [(byte)keyId],
            },
            ct
        );

        if (authResponse.StatusCode != DesfireStatusCode.AdditionalFrame)
        {
            return DesfireResponse.Create(authResponse.StatusCode);
        }

        AuthenticationHelper authenticationHelper = AuthenticationHelper.D40SecureMessaging(
            keyType,
            keyData,
            randomA
        );

        byte[] challengeResponse = authenticationHelper.Challenge(authResponse.Data);

        DesfireResponseFrame authChallengeResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.NextFrame,
                Data = challengeResponse,
            },
            ct
        );

        if (authChallengeResponse.StatusCode != DesfireStatusCode.Success)
        {
            return DesfireResponse.Create(
                authResponse.StatusCode.IsSuccess()
                    ? DesfireStatusCode.AuthenticationError
                    : authResponse.StatusCode
            );
        }

        authenticationHelper.ChallengeResume(authChallengeResponse.Data);
        Session = new D40SecureMessaging(
            Logger,
            _cardEncoder,
            keyType,
            keyId,
            authenticationHelper.SessionKey
        );
        return DesfireResponse.Create(authChallengeResponse.StatusCode);
    }

    /// <summary>
    ///     Returns the Desfire Version and card SAN number
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDesfireResponse<DesfireVersion>> GetVersion(CancellationToken ct = default)
    {
        DesfireResponseFrame hardwareResponse = await Session.SendCommand(
            new DesfireCommandFrame { Command = DesfireCommand.GetVersion },
            ct
        );

        if (hardwareResponse.StatusCode != DesfireStatusCode.AdditionalFrame)
        {
            DesfireVersion version = new()
            {
                CardId = "",
                Software = null,
                Hardware = new VersionInfo(hardwareResponse.Data),
            };

            return DesfireResponse.Create(hardwareResponse.StatusCode, version);
        }

        DesfireResponseFrame softwareResponse = await Session.SendCommand(
            new DesfireCommandFrame { Command = DesfireCommand.NextFrame },
            ct
        );

        if (softwareResponse.StatusCode != DesfireStatusCode.AdditionalFrame)
        {
            DesfireVersion version = new()
            {
                CardId = "",
                Software = new VersionInfo(softwareResponse.Data),
                Hardware = new VersionInfo(hardwareResponse.Data),
            };

            return DesfireResponse.Create(softwareResponse.StatusCode, version);
        }

        DesfireResponseFrame cardDataResponse = await Session.SendCommand(
            new DesfireCommandFrame { Command = DesfireCommand.NextFrame },
            ct
        );

        DesfireVersion fullVersion = new()
        {
            CardId = cardDataResponse.Data.Length >= 7 ? Convert.ToHexString(cardDataResponse.Data.AsSpan(0, 7)) : string.Empty,
            Software = new VersionInfo(softwareResponse.Data),
            Hardware = new VersionInfo(hardwareResponse.Data),
        };

        return DesfireResponse.Create(cardDataResponse.StatusCode, fullVersion);
    }

    /// <summary>
    ///     Returns the Card UID, requires authentication.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDesfireResponse<string>> GetCardUid(CancellationToken ct = default)
    {
        DesfireResponseFrame cardUidResponse = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.GetCardUid,
                ResponseCommunicationMode = CommunicationMode.Enciphered,
                ExpectedLength = 7,
            },
            ct
        );

        UpdateSessionOnError(cardUidResponse);

        if (!cardUidResponse.StatusCode.IsSuccess())
        {
            return DesfireResponse.Create<string>(cardUidResponse.StatusCode, null);
        }

        bool hasUidFormat = cardUidResponse.Data.Length > 1 && cardUidResponse.Data[0] == 0x00;

        int length = hasUidFormat ? cardUidResponse.Data[1] : 7;

        int start = hasUidFormat ? 2 : 0;

        if (cardUidResponse.Data.Length < start + length)
        {
            return DesfireResponse.Create<string>(DesfireStatusCode.LengthError, null);
        }

        return cardUidResponse.AsResponse(data =>
            Convert.ToHexString(cardUidResponse.Data.AsSpan(start, length))
        );
    }

    /// <summary>
    ///     Returns the remaining free memory on the card, if supported.
    /// </summary>
    public async Task<IDesfireResponse<byte[]>> GetFreeMemory(CancellationToken ct = default)
    {
        DesfireResponseFrame response = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.GetFreeMemory,
                ResponseCommunicationMode = CommunicationMode.Plain,
                ExpectedLength = 3,
            },
            ct
        );

        UpdateSessionOnError(response);

        return response.AsResponse(data =>
        {
            byte[] memory = new byte[3];
            data.AsSpan(0, Math.Min(data.Length, memory.Length)).CopyTo(memory);
            return memory;
        });
    }

    /// <summary>
    ///     Select the given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="ct"></param>
    /// <exception cref="Exception"></exception>
    public async Task<IDesfireResponse> SelectApplication(
        DesfireApplicationId applicationId,
        CancellationToken ct = default
    )
    {
        // When selecting an application the session always expires
        Session = new PlainDesfireSession(Logger, _cardEncoder);

        DesfireResponseFrame response = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.SelectApplication,
                Header = applicationId.AsBytes(),
            },
            ct
        );

        if (response.StatusCode.IsSuccess())
        {
            SelectedApplication = applicationId;
        }

        return response.AsNoData();
    }

    /// <summary>
    ///     Returns the key settings of the current selected application. By default, the PICC is selected
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <seealso cref="SelectApplication" />
    // public async Task<IDesfireResponse<KeySettings>> GetKeySettings(CancellationToken ct = default)
    // {
    //     DesfireResponseFrame cmd = await _session.SendCommand(new DesfireCommandFrame { Command = DesfireCommand.GetKeySettings }, ct);
    //
    //     UpdateSessionOnError(cmd);
    //
    //     return cmd.AsResponse(data => new KeySettings
    //     {
    //         ApplicationKeySettings = (ApplicationKeySettings)data[0],
    //         ApplicationSettings = (ApplicationSettings)data[1]
    //     });
    // }

    /// <summary>
    ///     List all applications
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDesfireResponse<ICollection<DesfireApplicationId>>> ListApplications(
        CancellationToken ct = default
    )
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.GetApplicationIds,
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = CommunicationMode.Plain,
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsResponse(
            data =>
            {
                DesfireApplicationId[] applications = new DesfireApplicationId[data.Length / DesfireApplicationId.ApplicationIdLength];
                for (int i = 0; i < applications.Length; i++)
                {
                    byte[] applicationId = data
                        .AsSpan(i * DesfireApplicationId.ApplicationIdLength, DesfireApplicationId.ApplicationIdLength)
                        .ToArray();
                    applications[i] = new DesfireApplicationId(applicationId);
                }

                return applications;
            },
            []
        );
    }

    /// <summary>
    ///     Creates a new application
    /// </summary>
    /// <param name="description">The description of the application to create </param>
    /// <param name="ct"></param>
    /// <returns>True if successful</returns>
    public async Task<IDesfireResponse> CreateApplication(
        ApplicationDescription description,
        CancellationToken ct = default
    )
    {
        byte[] header = description.Build();

        DesfireResponseFrame rsp = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.CreateApplication,
                Header = header,
                CommunicationMode = CommunicationMode.Plain,
            },
            ct
        );

        UpdateSessionOnError(rsp);

        return rsp.AsNoData();
    }

    public async Task<IDesfireResponse> DeleteApplication(
        DesfireApplicationId applicationId,
        CancellationToken ct = default
    )
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.DeleteApplication,
                Header = applicationId.AsBytes(),
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsNoData();
    }

    public async Task<IDesfireResponse> ChangePiccKey(
        KeyType keyType,
        byte[] newKey,
        byte keyVersion,
        CancellationToken ct = default
    )
    {
        if (Session.KeyId != 0x00)
        {
            throw new InvalidOperationException("Requires to be authenticated with the PICC master key");
        }

        if (SelectedApplication != DesfireApplicationId.PICC)
        {
            throw new InvalidOperationException("The PICC must be selected to execute this operation");
        }

        byte keyNo = keyType switch
        {
            KeyType.Aes => 0x80,
            KeyType.TDes => 0x00,
            KeyType.Tdes3K => 0x40,
            KeyType.Tdes2K => 0x00,
            _ => throw new ArgumentOutOfRangeException(nameof(keyType), keyType, null),
        };

        int commandLength = keyType == KeyType.Aes ? newKey.Length + 1 : newKey.Length;

        byte[] commandData = new byte[commandLength];
        Array.Copy(newKey, commandData, newKey.Length);

        if (keyType == KeyType.Aes)
        {
            commandData[newKey.Length] = keyVersion;
        }

        //See Page 77 & 204

        DesfireCommandFrame command = new()
        {
            Command = DesfireCommand.ChangeKey,
            Header = [keyNo],
            Data = commandData,
            CommunicationMode = CommunicationMode.Enciphered,
            ResponseCommunicationMode = CommunicationMode.Plain,
        };

        DesfireResponseFrame result = await Session.SendCommand(command, ct);
        UpdateSessionOnError(result);
        Session = new PlainDesfireSession(Logger, _cardEncoder); //We must reauthenticate with the new key!

        return result.AsNoData();
    }

    public async Task<IDesfireResponse> ChangeKey(
        KeyType keyType,
        DesfireKeyId keyId,
        byte[] oldKey,
        byte[] newKey,
        byte? keyVersion = null,
        CancellationToken ct = default
    )
    {
        byte keyNo = (byte)(keyId & 0x3F);

        bool currentKeyWillChange = keyId == Session.KeyId;

        byte[] commandData = [];

        bool crcCalculated = false;

        if (keyType == KeyType.Aes)
        {
            if (!keyVersion.HasValue)
            {
                throw new ArgumentException("AES key requires a key version", nameof(keyVersion));
            }

            if (currentKeyWillChange)
            {
                commandData = new byte[newKey.Length + 1];
                Array.Copy(newKey, commandData, newKey.Length);
                commandData[newKey.Length] = keyVersion.Value;
                Logger.LogDebug("Changing the current AES authenticated key!");
            }
            else
            {
                crcCalculated = true;
                byte[] xorKey = BitUtilities.XorByteArray(newKey, oldKey);

                byte[] crcData = new byte[xorKey.Length + 3];
                crcData[0] = (byte)DesfireCommand.ChangeKey;
                crcData[1] = keyNo;
                Array.Copy(xorKey, 0, crcData, 2, xorKey.Length);
                crcData[^1] = keyVersion.Value;

                byte[] crc32 = CryptoHelper.CalculateCrc32(crcData);
                byte[] crc32Nk = CryptoHelper.CalculateCrc32(newKey);

                commandData = new byte[xorKey.Length + 9];
                Array.Copy(xorKey, commandData, xorKey.Length);
                commandData[xorKey.Length] = keyVersion.Value;
                Array.Copy(crc32, 0, commandData, xorKey.Length + 1, crc32.Length);
                Array.Copy(crc32Nk, 0, commandData, xorKey.Length + 5, crc32Nk.Length);

                Logger.LogDebug("Changing an AES key different from the authenticated key");
            }
        }
        else
        {
            if (currentKeyWillChange)
            {
                commandData = new byte[newKey.Length];
                Array.Copy(newKey, commandData, newKey.Length);
            }
            else
            {
                crcCalculated = true;
                byte[] xorKey = BitUtilities.XorByteArray(newKey, oldKey);

                byte[] crcData = new byte[xorKey.Length + 2];
                crcData[0] = (byte)DesfireCommand.ChangeKey;
                crcData[1] = keyNo;
                Array.Copy(xorKey, 0, crcData, 2, xorKey.Length);

                byte[] crc16 = CryptoHelper.CalculateCrc16(crcData);
                byte[] crc16Nk = CryptoHelper.CalculateCrc16(newKey);

                commandData = new byte[xorKey.Length + crc16.Length + crc16Nk.Length];
                Array.Copy(xorKey, commandData, xorKey.Length);
                Array.Copy(crc16, 0, commandData, xorKey.Length, crc16.Length);
                Array.Copy(crc16Nk, 0, commandData, xorKey.Length + crc16.Length, crc16Nk.Length);
            }
        }

        //See Page 77 & 204

        DesfireCommandFrame command = new()
        {
            Command = DesfireCommand.ChangeKey,
            Header = [keyNo],
            Data = commandData,
            CommunicationMode = CommunicationMode.Enciphered,
            ResponseCommunicationMode = CommunicationMode.Plain,
            ApplyCrc = !crcCalculated,
        };

        DesfireResponseFrame result = await Session.SendCommand(command, ct);

        UpdateSessionOnError(result);

        if (currentKeyWillChange)
        {
            Session = new PlainDesfireSession(Logger, _cardEncoder); //We must reauthenticate with the new key!
        }

        return result.AsNoData();
    }

    /// <summary>
    ///     Changes the configuration of the card or application
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDesfireResponse> ChangeConfiguration(
        CardConfiguration configuration,
        CancellationToken ct = default
    )
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.SetConfiguration,
                Header = [(byte)configuration.Option],
                Data = configuration.Data,
                CommunicationMode = CommunicationMode.Enciphered,
                ResponseCommunicationMode = CommunicationMode.Plain,
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsNoData();
    }

    /// <summary>
    ///     Change the key settings of the current applications
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="ct"></param>
    /// <remarks>DS487033 Page 218</remarks>
    /// <returns></returns>
    public async Task<IDesfireResponse> ChangeKeySettings(
        ApplicationKeySettings settings,
        CancellationToken ct = default
    )
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.ChangeKeySettings,
                Header = [],
                Data = [(byte)settings],
                CommunicationMode = CommunicationMode.Enciphered,
                ResponseCommunicationMode = CommunicationMode.Plain,
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsNoData();
    }

    /// <summary>
    ///     Change the key settings of the current applications
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="ct"></param>
    /// <remarks>DS487033 Page 218</remarks>
    /// <returns></returns>
    public async Task<IDesfireResponse> ChangeKeySettings(
        PiccKeySettings settings,
        CancellationToken ct = default
    )
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.ChangeKeySettings,
                Header = [],
                Data = [(byte)settings],
                CommunicationMode = CommunicationMode.Enciphered,
                ResponseCommunicationMode = CommunicationMode.Plain,
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsNoData();
    }

    public async Task<IDesfireResponse> DeleteFile(int fileId, CancellationToken ct = default)
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.DeleteFile,
                Header = [(byte)fileId],
                Data = [],
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = CommunicationMode.Plain,
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsNoData();
    }

    /// <summary>
    ///     Creates the given file under the currently selected applciation
    /// </summary>
    /// <param name="file"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IDesfireResponse> CreateFile(DesfireFile file, CancellationToken ct = default)
    {
        DesfireCommand command = file switch
        {
            StandardDesfireFile _ => DesfireCommand.CreateStandardFile,
            _ => throw new NotImplementedException(),
        };

        byte[] header = new byte[7];
        header[0] = (byte)file.FileNumber;
        header[1] = (byte)file.FileOptions;

        byte[] accessRights = file.AccessRights.GetBytes();
        Array.Copy(accessRights, 0, header, 2, accessRights.Length);

        WriteUInt24LittleEndian(header.AsSpan(4, 3), file.FileSize);

        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = command,
                Header = header,
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = CommunicationMode.Cmac,
                Data = [],
            },
            ct
        );

        UpdateSessionOnError(result);

        return result.AsNoData();
    }

    public async Task<IDesfireResponse> ChangeFileSettings(
        int fileId,
        DesfireFileOptions options,
        DesfireFileAccessRights accessRights,
        CancellationToken ct = default
    )
    {
        var command = DesfireCommand.ChangeFileSettings;
        var header = new byte[] { (byte)fileId };

        var data = new byte[3];
        var accessRightsBytes = accessRights.GetBytes();
        data[0] = (byte)options;

        Array.Copy(accessRightsBytes, 0, data, 1, accessRightsBytes.Length);

        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame()
            {
                Command = command,
                Header = header,
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = CommunicationMode.Plain,
                Data = data,
            },
            ct
        );

        UpdateSessionOnError(result);
        return result.AsNoData();
    }

    /// <summary>
    ///     Lists all file ids of the current applciation
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDesfireResponse<ICollection<int>>> ListFiles(CancellationToken ct = default)
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.GetFileIds,
                Header = [],
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = CommunicationMode.Cmac,
                Data = [],
            },
            ct
        );

        return result.AsResponse(x => x.Select(y => (int)y).ToList());
    }

    public Task<IDesfireResponse> WriteData(
        int fileId,
        byte[] data,
        CommunicationMode communicationMode,
        CancellationToken ct = default
    )
    {
        return WriteData(fileId, data, 0, data.Length, communicationMode, ct);
    }

    public async Task<IDesfireResponse> WriteData(
        int fileId,
        byte[] data,
        int offset,
        int length,
        CommunicationMode communicationMode,
        CancellationToken ct = default
    )
    {
        int maxChunkSize = communicationMode == CommunicationMode.Enciphered ? 128 : 236;
        int bytesWritten = 0;
        DesfireResponseFrame? lastResult = null;

        while (bytesWritten < length)
        {
            int chunkLength = Math.Min(maxChunkSize, length - bytesWritten);
            byte[] header = new byte[7];
            header[0] = BitUtilities.ByteFromInt(fileId, 4);

            WriteUInt24LittleEndian(header.AsSpan(1, 3), offset + bytesWritten);
            WriteUInt24LittleEndian(header.AsSpan(4, 3), chunkLength);

            byte[] chunkData = data.AsSpan(bytesWritten, chunkLength).ToArray();

            lastResult = await Session.SendCommand(
                new DesfireCommandFrame
                {
                    Command = DesfireCommand.WriteData,
                    Header = header,
                    CommunicationMode = communicationMode,
                    ResponseCommunicationMode = CommunicationMode.Plain,
                    Data = chunkData,
                },
                ct
            );

            UpdateSessionOnError(lastResult);
            bytesWritten += chunkLength;
        }

        return lastResult?.AsNoData() ?? DesfireResponse.Create(DesfireStatusCode.Success);
    }

    public async Task<IDesfireResponse<byte[]>> ReadData(
        int fileId,
        int offset,
        int length,
        CommunicationMode communicationMode,
        CancellationToken ct = default
    )
    {
        byte[] header = new byte[7];
        header[0] = BitUtilities.ByteFromInt(fileId, 4);

        WriteUInt24LittleEndian(header.AsSpan(1, 3), offset);
        WriteUInt24LittleEndian(header.AsSpan(4, 3), length);

        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.ReadData,
                Header = header,
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = communicationMode,
                Data = [],
                ExpectedLength = length,
            },
            ct
        );

        UpdateSessionOnError(result);
        return result.AsResponse(x => x);
    }

    /// <summary>
    ///     At PICC level, all applications and files are deleted. At application level (only for delegated applications), all files are deleted. The deleted memory is released and can be reuse
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDesfireResponse> Format(CancellationToken ct = default)
    {
        DesfireResponseFrame result = await Session.SendCommand(
            new DesfireCommandFrame
            {
                Command = DesfireCommand.Format,
                Header = [],
                CommunicationMode = CommunicationMode.Plain,
                ResponseCommunicationMode = CommunicationMode.Cmac,
                Data = [],
            },
            ct
        );

        UpdateSessionOnError(result);
        return result.AsNoData();
    }
}

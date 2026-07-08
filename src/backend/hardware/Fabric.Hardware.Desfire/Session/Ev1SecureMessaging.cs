using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Utils;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Fabric.Hardware.Desfire.Session;

/*
 * More information about DESFire EV1 Secure Messaging can be found in the DS487033 Page 38 - 44
 */

/// <summary>
///     An authenticated session using EV1 Secure Mode
///     <remarks>DS487033 Page 36</remarks>
/// </summary>
public class Ev1SecureMessaging : DesfireSession
{
    private readonly ILogger _logger;
    private byte[] _lastIv;

    public Ev1SecureMessaging(ILogger logger, IRfidEncoder cardEncoder, byte keyId, byte[] sessionKey, KeyType keyType, byte[]? iv = null)
        : base(logger, cardEncoder)
    {
        _logger = logger;
        KeyId = keyId;
        SessionKey = sessionKey;
        KeyType = keyType;
        _lastIv = iv ?? new byte[CryptoHelper.GetBlockSize(keyType)];
    }

    public byte[] SessionKey { get; }
    public override KeyType KeyType { get; }

    protected override byte[] PreProcessEncrypt(DesfireCommandFrame command)
    {
        byte[] apdu = command.CalculateApdu();

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Encrypting command {Command} with IV: {IV}", Convert.ToHexString(apdu), Convert.ToHexString(_lastIv));
        }

        byte[] encrypted = command.EncryptEv1(KeyType, _lastIv, SessionKey);
        command.Data = encrypted;
        _lastIv = command.Data[^_lastIv.Length..];

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Encrypted command: {Command} IV: {IV}", Convert.ToHexString(encrypted), Convert.ToHexString(_lastIv));
        }

        return command.CalculateApdu();
    }

    protected override byte[] PreProcessCmaced(DesfireCommandFrame command)
    {
        byte[] apdu = command.CalculateApdu();

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Calculating CMAC for {Message}, IV: {IV}", Convert.ToHexString(apdu), Convert.ToHexString(_lastIv));
        }
        byte[] cmac = CryptoHelper.Cmac(KeyType, _lastIv, SessionKey, apdu);
        _lastIv = cmac;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("CMAC is {CMAC}", Convert.ToHexString(cmac));
        }

        byte[] fullCommand = new byte[apdu.Length + 8];
        Array.Copy(apdu, fullCommand, apdu.Length);
        Array.Copy(cmac, 0, fullCommand, apdu.Length, 8);
        return fullCommand;
    }

    protected override byte[] PreProcessPlain(DesfireCommandFrame command)
    {
        byte[] apdu = command.CalculateApdu();

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Calculating CMAC for {Message}, IV: {IV}", Convert.ToHexString(apdu), Convert.ToHexString(_lastIv));
        }
        _lastIv = CryptoHelper.Cmac(KeyType, _lastIv, SessionKey, apdu);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("CMAC is {CMAC}", Convert.ToHexString(_lastIv.AsSpan(0, 8)));
        }
        return apdu;
    }

    protected override byte[] PostProcess(CommunicationMode mode, byte[] response, int? length)
    {
        if (mode is CommunicationMode.Plain or CommunicationMode.Cmac)
        {
            //Console.WriteLine($"Verifying CMAC {Convert.ToHexString(response[^8..])} with LAST IV: {Convert.ToHexString(_lastIv)}");
            byte[] cmac = VerifyCmac(response);
            _lastIv = cmac;

            //We remove the last 8 bytes from the response which is the CMAC we received
            return response[..^8];
        }

        if (length == null)
        {
            throw new NotSupportedException("Length required for decryption");
        }

        /*
         * We receive a response with the following bytes
         *  byte[0]: status code (not part of the encrypted message)
         *  byte[1..] is the encrypted part, consisting out of a response, crc32 and padding
         */
        byte[] plainText = CryptoHelper.Decrypt(KeyType, _lastIv, SessionKey, response[1..]);
        //AESHelper.Decrypt(_lastIv, SessionKey, response[1..]);

        //The actual data response
        byte[] data = plainText[..length.Value];

        //The crc of the response
        byte[] crc32 = plainText[length.Value..(length.Value + 4)];

        //The value over which the response CRC was calculated
        byte[] crcInput = new byte[length.Value + 1];
        Array.Copy(data, crcInput, data.Length);
        crcInput[data.Length] = response[0];

        //Lets calculate the CRC from our side to see if the match
        byte[] ourCrc = CryptoHelper.CalculateCrc32(crcInput);

        if (!CryptographicOperations.FixedTimeEquals(ourCrc, crc32))
        {
            throw new InvalidOperationException("Invalid CRC32 on response");
        }

        //Update the last iv to the last block of the encrypted message
        _lastIv = response[^_lastIv.Length..];

        //Return the response code and decrypted data
        byte[] decryptedResponse = new byte[data.Length + 1];
        decryptedResponse[0] = response[0];
        Array.Copy(data, 0, decryptedResponse, 1, data.Length);
        return decryptedResponse;
    }

    private byte[] VerifyCmac(byte[] response)
    {
        //Data length is equals to the response size minus 8 bytes for cmac and minus 1 byte for the status
        int dataLength = response.Length - 9;

        //We should CMAC  the data and status of the response data
        byte[] dataForCmac = new byte[dataLength + 1];
        byte[] receivedCmac = response[^8..];

        //We copy the response data over (skipping the first bytes of the response which is the status code)
        Array.Copy(response, 1, dataForCmac, 0, dataLength);
        //We need to calculate the CMAC over the response data appended with the status code, which should be last in the array
        dataForCmac[dataLength] = response[0];

        byte[] cmacCalculated = CryptoHelper.Cmac(KeyType, _lastIv, SessionKey, dataForCmac);

        //Verify the CMAC we received matches the CMAC we calculated
        //In case the CMAC is 16 bytes only the 8 leftmost bytes are sent in the response, so only compare those
        if (!CryptographicOperations.FixedTimeEquals(cmacCalculated.AsSpan(0, 8), receivedCmac))
        {
            _logger.LogTrace(
                "CMAC for {Message} with IV: {IV}. Expected: {Expected}, Actual: {Actual}",
                Convert.ToHexString(dataForCmac),
                Convert.ToHexString(_lastIv),
                Convert.ToHexString(cmacCalculated.AsSpan(0, 8)),
                Convert.ToHexString(receivedCmac)
            );

            string message =
                $"Cmac does not match, Expected: {Convert.ToHexString(cmacCalculated.AsSpan(0, 8))} Actual: {Convert.ToHexString(receivedCmac)}";

            throw new InvalidOperationException(message);
        }

        //Return the un-truncated CMAC to be used for the IV of the next operation
        return cmacCalculated;
    }
}

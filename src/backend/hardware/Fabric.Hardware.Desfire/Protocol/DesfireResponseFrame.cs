using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Protocol;

public record DesfireResponseFrame
{
    public DesfireStatusCode StatusCode { get; init; }
    public required byte[] Data { get; set; }
    public byte[] Cmac { get; init; } = [];
}

public record DesfireCommandFrame
{
    /// <summary>
    ///     The command to be executed
    /// </summary>
    public DesfireCommand Command { get; init; }

    /// <summary>
    ///     Header data of the command
    /// </summary>
    public byte[] Header { get; init; } = [];

    /// <summary>
    ///     The actual command data
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    ///     Set this property if the response is <see cref="CommunicationMode.Enciphered" />
    /// </summary>
    public int? ExpectedLength { get; set; }

    /// <summary>
    ///     Indicates if the CRC values should be calculated
    /// </summary>
    public bool ApplyCrc { get; set; } = true;

    /// <summary>
    ///     The communication method for this command
    /// </summary>
    public CommunicationMode CommunicationMode { get; init; } = CommunicationMode.Plain;

    public CommunicationMode ResponseCommunicationMode { get; init; } = CommunicationMode.Plain;

    public byte[] CalculateApdu()
    {
        byte[] apdu = new byte[Header.Length + Data.Length + 1];
        apdu[0] = (byte)Command;
        Header.CopyTo(apdu.AsSpan(1));
        Data.CopyTo(apdu.AsSpan(Header.Length + 1));
        return apdu;
    }

    public byte[] EncryptEv1(KeyType keyType, byte[] iv, byte[] key)
    {
        byte[] crcBytes = ApplyCrc ? CryptoHelper.CalculateCrc32(CalculateApdu()) : [];

        //Ensure that we pad the data
        int blockSize = CryptoHelper.GetBlockSize(keyType);
        byte[] paddedData = BuildPaddedData(Data, crcBytes, blockSize);

        //Replace the plaintext data with the cipher data.
        return CryptoHelper.Encrypt(keyType, iv, key, paddedData);
    }

    public byte[] EncryptD40(KeyType keyType, byte[] iv, byte[] key)
    {
        byte[] crcBytes = ApplyCrc ? CryptoHelper.CalculateCrc16(Data) : [];

        //Ensure that we pad the data
        int blockSize = CryptoHelper.GetBlockSize(keyType);
        byte[] paddedData = BuildPaddedData(Data, crcBytes, blockSize);

        //We should always decrypt according to the specification
        return CryptoHelper.Decrypt(keyType, iv, key, paddedData);
    }

    private static byte[] BuildPaddedData(byte[] data, byte[] crcBytes, int blockSize)
    {
        int dataLength = data.Length + crcBytes.Length;
        int paddingRequired = blockSize - dataLength % blockSize;
        paddingRequired = paddingRequired == blockSize ? 0 : paddingRequired;

        byte[] paddedData = new byte[dataLength + paddingRequired];
        data.CopyTo(paddedData.AsSpan());
        crcBytes.CopyTo(paddedData.AsSpan(data.Length));

        return paddedData;
    }
}

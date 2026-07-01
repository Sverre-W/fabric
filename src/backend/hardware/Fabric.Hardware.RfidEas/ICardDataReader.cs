namespace Fabric.Hardware.RfidEas;

public interface ICardDataReader
{
    string ReadData(byte[] data);
}

/// <summary>
///     Hexadecimal format
/// </summary>
public sealed class HexadecimalCardReader : ICardDataReader
{
    public string ReadData(byte[] data)
    {
        string cardData = BitConverter.ToString(data)
            .Replace("-", "");

        return Convert.ToUInt64(cardData, 16).ToString();
    }
}

/// <summary>
///     BCD Format
/// </summary>
public sealed class BcdCardReader : ICardDataReader
{
    public string ReadData(byte[] data)
    {
        int length = Math.Min(data.Length, 4);
        byte[] cardData = data[..length].Reverse().ToArray();
        return BitConverter.ToString(cardData)
            .Replace("-", "");
    }
}

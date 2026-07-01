namespace Fabric.Hardware.RfidEas;

public interface ICardDataTransformer
{
    /// <summary>
    ///     Apply a transformation on the given data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    byte[] Transform(byte[] data);
}

public sealed class NoTransformation : ICardDataTransformer
{
    /// <summary>
    ///     Applies no transformation and returns the given data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public byte[] Transform(byte[] data) => data;
}

public sealed class InvertBytesTransformation : ICardDataTransformer
{
    public byte[] Transform(byte[] data) => data.Reverse().ToArray();
}

public sealed class InvertBitsTransformation : ICardDataTransformer
{
    public byte[] Transform(byte[] data) => data.Select(InvertBits).ToArray();

    private static byte InvertBits(byte value)
    {
        byte result = 0;
        for (int index = 0; index < 8; index++)
            result = (byte)((result << 1) | ((value >> index) & 1));

        return result;
    }
}

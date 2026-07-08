using Fabric.Hardware.Desfire.Protocol;

namespace Fabric.Hardware.Desfire.Utils;

public static class DesfireUtilities
{
    public static byte[] RotateByte(byte[] data)
    {
        byte[] rotated = new byte[data.Length];
        Array.Copy(data, 1, rotated, 0, data.Length - 1);
        rotated[data.Length - 1] = data[0];
        return rotated;
    }

    public static bool IsValid(this IEnumerable<DesfireResponseFrame> frames)
    {
        return frames.All(x => x.StatusCode is DesfireStatusCode.AdditionalFrame or DesfireStatusCode.Success);
    }

    public static bool IsEqual(byte[] array1, byte[] array2)
    {
        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }
}

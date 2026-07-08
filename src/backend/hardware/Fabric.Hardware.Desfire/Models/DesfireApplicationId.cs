using System.Globalization;

namespace Fabric.Hardware.Desfire.Models;

/// <summary>
///     Represents the identifier of a DESFire application
/// </summary>
public record DesfireApplicationId
{
    public const int ApplicationIdLength = 3;

    public DesfireApplicationId(byte[] data)
    {
        if (data.Length != ApplicationIdLength)
        {
            throw new ArgumentException("Invalid length of AID");
        }

        Id = [.. data];
    }

    private byte[] Id { get; }

    /// <summary>
    ///     The application ID that represents the PICC
    /// </summary>
    /// <returns></returns>
    public static DesfireApplicationId PICC { get; } = new(new byte[ApplicationIdLength]);

    public virtual bool Equals(DesfireApplicationId? other)
    {
        return other is not null && other.Id.AsSpan().SequenceEqual(Id);
    }

    public override string ToString()
    {
        /*return Convert.ToHexString([.. Id]).PadLeft(ApplicationIdLength * 2, '0');*/
        Span<byte> bytes = stackalloc byte[ApplicationIdLength];
        Id.AsSpan().CopyTo(bytes);
        bytes.Reverse();
        return Convert.ToHexString(bytes).PadLeft(ApplicationIdLength * 2, '0');
    }

    public byte[] AsBytes()
    {
        byte[] bytes = new byte[ApplicationIdLength];
        Array.Copy(Id, bytes, 3);
        return bytes;
    }

    /// <summary>
    ///     Create an application id from a hex string
    /// </summary>
    /// <param name="applicationIdHex"></param>
    /// <returns></returns>
    public static DesfireApplicationId Create(string applicationIdHex)
    {
        if (applicationIdHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            // Remove "0x" prefix if present
            applicationIdHex = applicationIdHex[2..];
        }

        int hexLength = ApplicationIdLength * 2;

        applicationIdHex = applicationIdHex.PadLeft(hexLength, '0');

        if (IsValidHex(applicationIdHex) && applicationIdHex.Length <= hexLength)
        {
            byte[] bytes = Convert.FromHexString(applicationIdHex);
            /*return new DesfireApplicationId(bytes);*/
            Array.Reverse(bytes);
            return new DesfireApplicationId(bytes);
        }

        throw new ArgumentException("Invalid Application ID");
    }

    /// <summary>
    ///     Create an application ID
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    public static DesfireApplicationId Create(byte[] applicationId)
    {
        return new DesfireApplicationId(applicationId);
    }

    private static bool IsValidHex(string input)
    {
        return int.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        foreach (byte value in Id)
        {
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }

    // public override int GetHashCode()
    // {
    //     return Id.GetHashCode();
    // }
    //
    // public override bool Equals(object? obj)
    // {
    //     if (obj is DesfireApplicationId other)
    //     {
    //         return Id.SequenceEqual(other.Id);
    //     }
    //
    //     return false;
    // }
}

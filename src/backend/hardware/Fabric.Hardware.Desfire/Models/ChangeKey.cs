namespace Fabric.Hardware.Desfire.Models;

/// <summary>
///     The key to manage the <see cref="ApplicationKeySettings" />
/// </summary>
public record ChangeKey
{
    internal ChangeKey(byte value)
    {
        Key = value;
    }

    internal byte Key { get; set; }

    public int KeyId => AllowAnyKey ? 0 : Key;

    public bool AllowAnyKey => Key == 0xE;
    public bool AllowNoKeys => Key == 0xF;

    /// <summary>
    ///     Designate a specific key to change the key settings
    /// </summary>
    /// <param name="keyId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ChangeKey SpecificKey(int keyId)
    {
        if (keyId < 0 || keyId > 13)
        {
            throw new ArgumentException("Key must be between 0 and 13");
        }

        return new ChangeKey((byte)keyId);
    }

    /// <summary>
    ///     Designate a specific key to change the key settings
    /// </summary>
    /// <param name="keyId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ChangeKey SpecificKey(byte keyId)
    {
        if (keyId > 0xD)
        {
            throw new ArgumentException("Key must be between 0x0 and 0xD");
        }

        return new ChangeKey(keyId);
    }

    /// <summary>
    ///     Allow any key in the application to change the key settings
    /// </summary>
    /// <returns></returns>
    public static ChangeKey AnyApplicationKey()
    {
        return new ChangeKey(0xE);
    }

    /// <summary>
    ///     The application key settings cannot be changed
    /// </summary>
    /// <returns></returns>
    public static ChangeKey ReadOnly()
    {
        return new ChangeKey(0xF);
    }

    public static explicit operator byte(ChangeKey d)
    {
        return d.Key;
    }

    public static explicit operator ChangeKey(byte b)
    {
        return new ChangeKey(b);
    }

    public override string ToString()
    {
        return Key switch
        {
            0xE => "E",
            0xF => "F",
            _ => Key.ToString(),
        };
    }
}

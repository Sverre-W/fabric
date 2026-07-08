using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Fabric.Hardware.Desfire.Scripting.Utilities;

public static class VariableEncodingUtilities
{
    public static byte[] EncodeForFile(byte[] value, string? encoding)
    {
        if (string.IsNullOrWhiteSpace(encoding))
        {
            return value;
        }

        EncodingSpec spec = Parse(encoding);
        return spec.Kind switch
        {
            EncodingKind.Text => value,
            EncodingKind.Hex => EncodeHex(value),
            EncodingKind.UnsignedInteger => EncodeUnsignedInteger(value, spec.WidthInBytes, spec.LittleEndian),
            _ => value,
        };
    }

    private static byte[] EncodeHex(byte[] value)
    {
        string text = System.Text.Encoding.UTF8.GetString(value).Trim();
        return text.Length == 0 ? [] : Convert.FromHexString(text);
    }

    private static byte[] EncodeUnsignedInteger(byte[] value, int widthInBytes, bool littleEndian)
    {
        if (widthInBytes <= 0)
        {
            throw new InvalidDataException("Unsigned integer encoding requires a positive byte width.");
        }

        ulong numericValue = ParseUnsignedInteger(value);
        byte[] encoded = new byte[widthInBytes];

        for (int i = 0; i < widthInBytes; i++)
        {
            encoded[littleEndian ? i : (widthInBytes - 1 - i)] = (byte)(numericValue & 0xFF);
            numericValue >>= 8;
        }

        if (numericValue != 0)
        {
            throw new InvalidDataException($"Value does not fit in {widthInBytes} bytes.");
        }

        return encoded;
    }

    private static ulong ParseUnsignedInteger(byte[] value)
    {
        if (value.Length == 0)
        {
            return 0;
        }

        string text = System.Text.Encoding.UTF8.GetString(value).Trim();
        if (text.Length > 0 && text.All(char.IsDigit))
        {
            if (!ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out ulong numericValue))
            {
                throw new InvalidDataException($"Value '{text}' is not a valid unsigned integer.");
            }

            return numericValue;
        }

        if (value.Length > sizeof(ulong))
        {
            throw new InvalidDataException("Binary value is too large to encode as an unsigned integer.");
        }

        ulong binaryValue = 0;
        foreach (byte b in value)
        {
            binaryValue = (binaryValue << 8) | b;
        }

        return binaryValue;
    }

    private static EncodingSpec Parse(string encoding)
    {
        string[] tokens = encoding
            .Trim()
            .Split([':', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
        {
            return new EncodingSpec(EncodingKind.Text, 0, false);
        }

        string kind = tokens[0].ToLowerInvariant();
        return kind switch
        {
            "text" or "utf8" or "utf-8" or "ascii" or "raw" => new EncodingSpec(EncodingKind.Text, 0, false),
            "hex" => new EncodingSpec(EncodingKind.Hex, 0, false),
            "uint" or "int" or "number" => ParseUnsignedIntegerEncoding(tokens),
            _ => throw new InvalidDataException($"Unsupported encoding '{encoding}'."),
        };
    }

    private static EncodingSpec ParseUnsignedIntegerEncoding(string[] tokens)
    {
        int widthInBytes = 0;
        bool littleEndian = false;

        foreach (string token in tokens.Skip(1))
        {
            if (int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out int width))
            {
                widthInBytes = width;
                continue;
            }

            if (token.Equals("le", StringComparison.OrdinalIgnoreCase) || token.Equals("little", StringComparison.OrdinalIgnoreCase) || token.Equals("little-endian", StringComparison.OrdinalIgnoreCase))
            {
                littleEndian = true;
                continue;
            }

            if (token.Equals("be", StringComparison.OrdinalIgnoreCase) || token.Equals("big", StringComparison.OrdinalIgnoreCase) || token.Equals("big-endian", StringComparison.OrdinalIgnoreCase))
            {
                littleEndian = false;
                continue;
            }

            throw new InvalidDataException($"Unsupported unsigned integer encoding token '{token}'.");
        }

        return new EncodingSpec(EncodingKind.UnsignedInteger, widthInBytes, littleEndian);
    }

    private readonly record struct EncodingSpec(EncodingKind Kind, int WidthInBytes, bool LittleEndian);

    private enum EncodingKind
    {
        Text,
        Hex,
        UnsignedInteger,
    }
}

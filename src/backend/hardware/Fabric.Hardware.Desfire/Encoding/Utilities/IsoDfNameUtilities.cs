namespace Fabric.Hardware.Desfire.Encoding.Utilities;

public static class IsoDfNameUtilities
{
    public static bool TryGetBytes(string? value, int minBytes, int maxBytes, out byte[] bytes)
    {
        bytes = [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalized = value.Trim();
        if (!normalized.All(c => c is >= ' ' and <= '~'))
        {
            return false;
        }

        bytes = System.Text.Encoding.ASCII.GetBytes(normalized);
        return bytes.Length >= minBytes && bytes.Length <= maxBytes;
    }
}

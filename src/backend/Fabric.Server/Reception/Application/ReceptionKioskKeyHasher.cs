using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Fabric.Server.Reception.Application;

public sealed class ReceptionKioskKeyHasher
{
    private const int SaltSize = 32;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public ReceptionKioskKey CreateKey()
    {
        string key = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(KeySize));
        string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
        string hash = Hash(key, salt);

        return new ReceptionKioskKey(key, hash, salt);
    }

    public bool Verify(string key, string hash, string salt)
    {
        byte[] expectedHash = Convert.FromBase64String(hash);
        byte[] actualHash = Convert.FromBase64String(Hash(key, salt));

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static string Hash(string key, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(key, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);

        return Convert.ToBase64String(hash);
    }
}

public sealed record ReceptionKioskKey(string Key, string Hash, string Salt);

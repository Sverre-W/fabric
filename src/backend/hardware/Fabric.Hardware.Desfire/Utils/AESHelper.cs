using System.Security.Cryptography;

namespace Fabric.Hardware.Desfire.Utils;

public class AESHelper
{
    public static byte[] Encrypt(byte[] iv, byte[] key, byte[] payload)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;
        aesAlg.Padding = PaddingMode.Zeros;

        using ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using MemoryStream msEncrypt = new();
        using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);

        csEncrypt.Write(payload, 0, payload.Length);
        csEncrypt.FlushFinalBlock();
        return msEncrypt.ToArray();
    }

    public static byte[] Decrypt(byte[] myKey, byte[] myMsg)
    {
        return Decrypt(new byte[8], myKey, myMsg);
    }

    public static byte[] Decrypt(byte[] iv, byte[] key, byte[] payload)
    {
        return Decrypt(iv, key, payload, 0, payload.Length);
    }

    public static byte[] Decrypt(byte[] iv, byte[] key, byte[] payload, int offset, int length)
    {
        using Aes aesAlg = Aes.Create();

        aesAlg.Padding = PaddingMode.None;
        aesAlg.Key = key;
        aesAlg.IV = iv;

        using ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(payload);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using MemoryStream decryptedStream = new();
        csDecrypt.CopyTo(decryptedStream);
        return decryptedStream.ToArray();
    }
}

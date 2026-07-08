using Fabric.Hardware.Desfire.Protocol;
using Nito.HashAlgorithms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Fabric.Hardware.Desfire.Utils;

public static class CryptoHelper
{
    private const byte Rb64 = 0x1B;
    private const byte Rb128 = 0x87;

    private static readonly CRC32 Crc32 = new(
        new CRC32.Definition
        {
            TruncatedPolynomial = 0x04C11DB7,
            Initializer = 0xFFFFFFFF,
            FinalXorValue = 0x00000000,
            ReverseDataBytes = true,
            ReverseResultBeforeFinalXor = true,
        }
    );

    public static int GetBlockSize(KeyType keyType)
    {
        return keyType switch
        {
            KeyType.Aes => 16,
            _ => 8,
        };
    }

    public static int GetKeySize(KeyType keyType)
    {
        return keyType switch
        {
            KeyType.Aes => 16,
            KeyType.TDes => 8,
            KeyType.Tdes2K => 16,
            KeyType.Tdes3K => 24,
            KeyType.None => throw new NotSupportedException(),
            _ => throw new NotSupportedException(),
        };
    }

    public static byte[] Cmac(KeyType keyType, byte[] iv, byte[] key, byte[] data)
    {
        int blockSize = GetBlockSize(keyType);
        byte rb = blockSize == 8 ? Rb64 : Rb128;

        byte[] zeroVector = new byte[blockSize];

        byte[] nistL = Encrypt(keyType, zeroVector, key, zeroVector);

        byte[] nistK1 = GetSubK1(nistL, blockSize, rb);
        byte[] nistK2 = GetSubK2(nistK1, blockSize, rb);

        return CalculateCmac(key, nistK1, nistK2, data, iv, blockSize, keyType);
    }

    public static byte[] DiversifyAesKey(byte[] aesKey, byte[] diversificationData)
    {
        byte[] m = new byte[diversificationData.Length + 1];
        m[0] = 0x01;
        Array.Copy(diversificationData, 0, m, 1, diversificationData.Length);
        byte[] iv = new byte[16];

        if (m.Length >= 16)
        {
            return Cmac(KeyType.Aes, iv, aesKey, m);
        }

        throw new NotSupportedException("Investigate");
    }

    private static int Crc16A(byte[] a, int offset, int length)
    {
        int crc = 0x6363;
        for (int i = offset; i < offset + length; i++)
        {
            crc = Crc16AddByte(crc, a[i]);
        }

        return crc;
    }

    private static int Crc16AddByte(int crc, byte b)
    {
        int bb = (b ^ crc) & 0xFF;
        bb = (bb ^ (bb << 4)) & 0xFF;
        return (crc >> 8) ^ (bb << 8) ^ (bb << 3) ^ (bb >> 4);
    }

    /// <summary>
    ///     Calculates the CRC16 according to CRC A of ISO/IEC 14443-3
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] CalculateCrc16(byte[] source)
    {
        int crc = Crc16A(source, 0, source.Length);
        byte[] ret = new byte[2];
        ret[1] = (byte)((crc >> 8) & 0xFF);
        ret[0] = (byte)(crc & 0xFF);
        return ret;
    }

    public static byte[] CalculateCrc32(byte[] source)
    {
        return Crc32.ComputeHash(source);
    }

    /// <summary>
    ///     MF3ICD40, which only supports DES/3DES, has two cryptographic
    ///     modes of operation (CBC): send mode and receive mode. In send mode,
    ///     data is first XORed with the IV and then decrypted.
    /// </summary>
    /// <param name="keyType"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte[] EncryptDesfireNative(KeyType keyType, byte[] key, byte[] data)
    {
        string algorithm = GetAlgorithm(keyType);
        int blockLength = GetBlockSize(keyType);

        byte[] iv = new byte[blockLength];
        byte[] lastCipherBlock = new byte[blockLength];
        byte[] cipherText = new byte[data.Length];

        for (int offset = 0; offset < data.Length; offset += blockLength)
        {
            int chunkLength = Math.Min(blockLength, data.Length - offset);
            byte[] chunk = new byte[blockLength];
            Array.Copy(data, offset, chunk, 0, chunkLength);

            lastCipherBlock = BitUtilities.XorByteArray(chunk, lastCipherBlock);
            byte[] cipher = Process(algorithm, false, iv, key, lastCipherBlock);

            Array.Copy(cipher, 0, cipherText, offset, chunkLength);
        }

        return cipherText;
    }

    /// <summary>
    ///     MF3ICD40, which only supports DES/3DES, has two cryptographic
    ///     modes of operation (CBC): send mode and receive mode. In send mode,
    ///     data is first XORed with the IV and then decrypted.
    /// </summary>
    /// <param name="keyType"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte[] DecryptDesfireNative(KeyType keyType, byte[] key, byte[] data)
    {
        string algorithm = keyType switch
        {
            KeyType.Tdes2K or KeyType.Tdes3K => "DESede/CBC/NoPadding",
            KeyType.TDes => "DES/CBC/NoPadding",
            _ => throw new ArgumentOutOfRangeException(nameof(keyType), keyType, null),
        };

        int blockLength = GetBlockSize(keyType);

        byte[] iv = new byte[blockLength];

        byte[] cipherText = new byte[data.Length];
        int offset = blockLength;

        byte[] firstBlock = new byte[blockLength];
        Array.Copy(data, firstBlock, blockLength);
        byte[] lastCipherBlock = Process(algorithm, false, iv, key, firstBlock);
        Array.Copy(lastCipherBlock, 0, cipherText, 0, blockLength);

        for (int dataOffset = blockLength; dataOffset < data.Length; dataOffset += blockLength)
        {
            byte[] chunk = new byte[blockLength];
            Array.Copy(data, dataOffset, chunk, 0, blockLength);
            lastCipherBlock = Process(algorithm, false, iv, key, chunk);
            byte[] decrypted = BitUtilities.XorByteArray(lastCipherBlock, chunk);

            Array.Copy(decrypted, 0, cipherText, offset, blockLength);
            offset += blockLength;
        }

        return cipherText;
    }

    private static string GetAlgorithm(KeyType keyType)
    {
        string algorithm = keyType switch
        {
            KeyType.Aes => "AES/CBC/NoPadding",
            KeyType.Tdes2K => "DESede/CBC/NoPadding",
            KeyType.Tdes3K => "DESede/CBC/NoPadding",
            KeyType.TDes => "DES/CBC/NoPadding",
            _ => throw new ArgumentOutOfRangeException(nameof(keyType), keyType, null),
        };

        return algorithm;
    }

    public static byte[] Encrypt(KeyType keyType, byte[] iv, byte[] key, byte[] data)
    {
        string algorithm = GetAlgorithm(keyType);
        return Process(algorithm, true, iv, key, data);
    }

    public static byte[] Decrypt(KeyType keyType, byte[] iv, byte[] key, byte[] data)
    {
        string algorithm = GetAlgorithm(keyType);
        return Process(algorithm, false, iv, key, data);
    }

    private static byte[] Process(string algorithm, bool encrypt, byte[] iv, byte[] key, byte[] data)
    {
        IBufferedCipher cipher = CipherUtilities.GetCipher(algorithm);
        cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(key), iv));
        return cipher.DoFinal(data);
    }

    #region CMAC_HELPERS

    private static byte[] CalculateCmac(byte[] k, byte[] k1, byte[] k2, byte[] block, byte[] eIv, int size, KeyType type)
    {
        int paddedLength = block.Length == 0 || block.Length % size != 0
            ? block.Length - (block.Length % size) + size
            : block.Length;

        byte[] newBlock = new byte[paddedLength];
        block.CopyTo(newBlock.AsSpan());

        if (block.Length == 0 || block.Length % size != 0)
        {
            newBlock[block.Length] = 0x80;
        }

        if (block.Length != 0 && block.Length % size == 0)
        {
            // complete block: K1
            for (int i = newBlock.Length - size; i < newBlock.Length; i++)
            {
                newBlock[i] ^= k1[i - newBlock.Length + size];
            }
        }
        else
        {
            // incomplete block: K2
            for (int i = newBlock.Length - size; i < newBlock.Length; i++)
            {
                newBlock[i] ^= k2[i - newBlock.Length + size];
            }
        }

        byte[] encryptedData = Encrypt(type, eIv, k, newBlock);

        //Console.WriteLine($"CMAC full: {Convert.ToHexString(encryptedData)}");

        byte[] cmac = new byte[size];
        Array.Copy(encryptedData, encryptedData.Length - size, cmac, 0, size);

        return cmac;
    }

    private static byte[] GetSubK2(byte[] k1, int size, byte poly)
    {
        byte[] rb = new byte[size];
        rb[size - 1] = poly;
        byte[] k2 = ShiftLeft(k1);

        if ((k1[0] & 0x80) != 0)
        {
            for (int i = 0; i < size; i++)
            {
                k2[i] = (byte)(k2[i] ^ rb[i]);
            }
        }

        return k2;
    }

    private static byte[] GetSubK1(byte[] l, int size, byte poly)
    {
        byte[] rb = new byte[size];
        rb[size - 1] = poly;
        byte[] k1 = ShiftLeft(l);

        if ((l[0] & 0x80) != 0)
        {
            for (int i = 0; i < size; i++)
            {
                k1[i] = (byte)(k1[i] ^ rb[i]);
            }
        }

        return k1;
    }

    private static byte[] ShiftLeft(byte[] data)
    {
        byte[] result = new byte[data.Length];
        int carry = 0;

        for (int i = data.Length - 1; i >= 0; i--)
        {
            int value = data[i];
            result[i] = (byte)((value << 1) | carry);
            carry = (value & 0x80) == 0 ? 0 : 1;
        }

        return result;
    }

    public static bool IsSymmetricKey(byte[] key)
    {
        for (int i = 0; i < key.Length / 2; i++)
        {
            if (key[i] != key[key.Length - i - 1])
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}

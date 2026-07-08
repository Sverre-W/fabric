using System.Security.Authentication;
using System.Security.Cryptography;
using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Protocol.Authentication;

public abstract class AuthenticationHelper
{
    protected AuthenticationHelper(KeyType keyType, byte[] key)
    {
        Iv = new byte[CryptoHelper.GetBlockSize(keyType)];
        Key = key;
        KeyType = keyType;
        SessionKey = [];
    }

    protected byte[] RndA { get; set; } = [];
    protected byte[] RndB { get; set; } = [];
    protected byte[] Key { get; set; }
    protected byte[] Iv { get; set; }
    protected KeyType KeyType { get; set; }
    public byte[] SessionKey { get; set; }
    public byte[] InitializationVector => Iv;

    /// <summary>
    ///     Decrypts the Random B value and returns the challenge response
    /// </summary>
    /// <param name="encryptedRndB"></param>
    /// <returns></returns>
    public abstract byte[] Challenge(byte[] encryptedRndB);

    /// <summary>
    ///     Decrypt the Rotated RndA and verifies it matches the generated RndA
    /// </summary>
    /// <param name="encryptedResponse"></param>
    /// <returns>The session</returns>
    public abstract void ChallengeResume(byte[] encryptedResponse);

    protected void GenerateSessionKey()
    {
        byte[] sessionKey = new byte[24];

        switch (KeyType)
        {
            case KeyType.Aes:
                Array.Copy(RndA, 0, sessionKey, 0, 4);
                Array.Copy(RndB, 0, sessionKey, 4, 4);
                Array.Copy(RndA, 12, sessionKey, 8, 4);
                Array.Copy(RndB, 12, sessionKey, 12, 4);
                SessionKey = sessionKey[..16];
                break;

            case KeyType.TDes:
                Array.Copy(RndA, 0, sessionKey, 0, 4);
                Array.Copy(RndB, 0, sessionKey, 4, 4);
                SessionKey = sessionKey[..8];
                break;

            case KeyType.Tdes2K:

                Array.Copy(RndA, 0, sessionKey, 0, 4);
                Array.Copy(RndB, 0, sessionKey, 4, 4);
                if (CryptoHelper.IsSymmetricKey(Key))
                {
                    Array.Copy(RndA, 0, sessionKey, 8, 4);
                    Array.Copy(RndB, 0, sessionKey, 12, 4);
                }
                else
                {
                    Array.Copy(RndA, 4, sessionKey, 8, 4);
                    Array.Copy(RndB, 4, sessionKey, 12, 4);
                }

                SessionKey = sessionKey[..16];
                break;

            case KeyType.Tdes3K:
                Array.Copy(RndA, 0, sessionKey, 0, 4);
                Array.Copy(RndB, 0, sessionKey, 4, 4);
                Array.Copy(RndA, 6, sessionKey, 8, 4);
                Array.Copy(RndB, 6, sessionKey, 12, 4);
                Array.Copy(RndA, 12, sessionKey, 16, 4);
                Array.Copy(RndB, 12, sessionKey, 20, 4);
                SessionKey = sessionKey;
                break;
            case KeyType.None:
            default:
                throw new ArgumentException("Invalid key", nameof(KeyType));
        }
    }

    public static AuthenticationHelperD40 D40SecureMessaging(KeyType keyType, byte[] key, byte[]? rndA = null)
    {
        return new AuthenticationHelperD40(keyType, key, rndA);
    }

    public static AuthenticationHelperEv1SecureMessaging Ev1SecureMessaging(KeyType keyType, byte[] key, byte[]? rndA = null)
    {
        return new AuthenticationHelperEv1SecureMessaging(keyType, key, rndA);
    }

    public static AuthenticationHelperEv2SecureMessaging Ev2SecureMessaging(KeyType keyType, byte[] key, byte[]? rndA = null)
    {
        return new AuthenticationHelperEv2SecureMessaging(keyType, key, rndA);
    }
}

public class AuthenticationHelperD40 : AuthenticationHelper
{
    public AuthenticationHelperD40(KeyType keyType, byte[] key, byte[]? rndA = null)
        : base(keyType, key)
    {
        RndA = rndA ?? RandomNumberGenerator.GetBytes(8);
    }

    public override byte[] Challenge(byte[] encryptedRndB)
    {
        RndB = CryptoHelper.Decrypt(KeyType, Iv, Key, encryptedRndB);
        byte[] rndBRotated = DesfireUtilities.RotateByte(RndB);

        byte[] challenge = new byte[RndA.Length + rndBRotated.Length];
        Array.Copy(RndA, 0, challenge, 0, RndA.Length);
        Array.Copy(rndBRotated, 0, challenge, RndA.Length, rndBRotated.Length);

        byte[] response = CryptoHelper.Encrypt(KeyType, Iv, Key, challenge);

        return response;
    }

    public override void ChallengeResume(byte[] encryptedResponse)
    {
        byte[] rotatedA = CryptoHelper.Decrypt(KeyType, Iv, Key, encryptedResponse);
        byte[] rotatedAOriginal = DesfireUtilities.RotateByte(RndA);

        if (!DesfireUtilities.IsEqual(rotatedA, rotatedAOriginal))
        {
            throw new AuthenticationException();
        }

        GenerateSessionKey();
    }
}

public class AuthenticationHelperEv2SecureMessaging : AuthenticationHelper
{
    public AuthenticationHelperEv2SecureMessaging(KeyType keyType, byte[] key, byte[]? rndA = null)
        : base(keyType, key)
    {
        RndA = rndA ?? RandomNumberGenerator.GetBytes(CryptoHelper.GetBlockSize(keyType));
        TransactionIdentifier = [];
        SessionKeyMacing = [];
    }

    public byte[] TransactionIdentifier { get; private set; }

    public byte[] SessionKeyMacing { get; private set; }

    public override byte[] Challenge(byte[] encryptedRndB)
    {
        RndB = CryptoHelper.Decrypt(KeyType, Iv, Key, encryptedRndB);

        byte[] rndBRotated = DesfireUtilities.RotateByte(RndB);

        byte[] challengeData = new byte[RndA.Length + rndBRotated.Length];
        Array.Copy(RndA, 0, challengeData, 0, RndA.Length);
        Array.Copy(rndBRotated, 0, challengeData, RndA.Length, rndBRotated.Length);

        byte[] challenge = CryptoHelper.Encrypt(KeyType, Iv, Key, challengeData);
        return challenge;
    }

    public override void ChallengeResume(byte[] encryptedResponse)
    {
        byte[] plainTextResponse = CryptoHelper.Decrypt(KeyType, Iv, Key, encryptedResponse);

        byte[] rotatedAOriginal = DesfireUtilities.RotateByte(RndA);
        byte[] rotatedA;

        if (plainTextResponse.Length == RndB.Length)
        {
            rotatedA = plainTextResponse;
        }
        else
        {
            TransactionIdentifier = plainTextResponse[..4];
            rotatedA = plainTextResponse[4..(4 + RndA.Length)];
        }

        if (!DesfireUtilities.IsEqual(rotatedA, rotatedAOriginal))
        {
            throw new AuthenticationException("RndA does not match");
        }

        SetSessionKeys2();
    }

    private void SetSessionKeys()
    {
        byte[] sessionKeyInput = new byte[32];

        //Prefix session key for encrypting
        byte[] fixedPrefixVEncrypt = [0xA5, 0x5A, 0x00, 0x01, 0x00, 0x80];
        byte[] fixedPrefixMacing = [0x5A, 0xA5, 0x00, 0x01, 0x00, 0x80];

        //Session key for encrypting

        Array.Copy(fixedPrefixVEncrypt, 0, sessionKeyInput, 26, fixedPrefixVEncrypt.Length);
        Array.Copy(RndA[14..16], 0, sessionKeyInput, 24, RndA[14..16].Length);

        byte[] xorAandB = BitUtilities.XorByteArray(RndA[8..14], RndA[10..16]);

        Array.Copy(xorAandB, 0, sessionKeyInput, 18, xorAandB.Length);
        Array.Copy(RndB[..10], 0, sessionKeyInput, 8, 10);
        Array.Copy(RndA[..8], 0, sessionKeyInput, 0, 8);

        SessionKey = CryptoHelper.Cmac(KeyType, new byte[Iv.Length], Key, sessionKeyInput);
        Array.Copy(fixedPrefixMacing, 0, sessionKeyInput, 26, fixedPrefixMacing.Length);
        SessionKeyMacing = CryptoHelper.Cmac(KeyType, new byte[Iv.Length], Key, sessionKeyInput);
    }

    private void SetSessionKeys2()
    {
        byte[] sessionKeyInput = new byte[32];

        //Prefix session key for encrypting
        byte[] fixedPrefixVEncrypt = [0xA5, 0x5A, 0x00, 0x01, 0x00, 0x80];
        byte[] fixedPrefixMacing = [0x5A, 0xA5, 0x00, 0x01, 0x00, 0x80];

        //Session key for encrypting

        Array.Copy(fixedPrefixVEncrypt, 0, sessionKeyInput, 0, fixedPrefixVEncrypt.Length);
        Array.Copy(RndA[..2], 0, sessionKeyInput, 6, 2);

        byte[] xorAandB = BitUtilities.XorByteArray(RndA[2..9], RndA[..7]);

        Array.Copy(xorAandB, 0, sessionKeyInput, 8, xorAandB.Length);
        Array.Copy(RndB[6..], 0, sessionKeyInput, 14, 10);
        Array.Copy(RndA[8..], 0, sessionKeyInput, 24, 8);

        SessionKey = CryptoHelper.Cmac(KeyType, new byte[Iv.Length], Key, sessionKeyInput);
        Array.Copy(fixedPrefixMacing, 0, sessionKeyInput, 0, fixedPrefixMacing.Length);
        SessionKeyMacing = CryptoHelper.Cmac(KeyType, new byte[Iv.Length], Key, sessionKeyInput);
    }
}

public class AuthenticationHelperEv1SecureMessaging : AuthenticationHelper
{
    public AuthenticationHelperEv1SecureMessaging(KeyType keyType, byte[] key, byte[]? rndA = null)
        : base(keyType, key)
    {
        RndA = rndA ?? RandomNumberGenerator.GetBytes(CryptoHelper.GetBlockSize(keyType));
    }

    public override byte[] Challenge(byte[] encryptedRndB)
    {
        RndB = CryptoHelper.Decrypt(KeyType, Iv, Key, encryptedRndB);

        Iv = encryptedRndB[^Iv.Length..];

        byte[] rndBRotated = DesfireUtilities.RotateByte(RndB);

        byte[] challengeData = new byte[RndA.Length + rndBRotated.Length];
        Array.Copy(RndA, 0, challengeData, 0, RndA.Length);
        Array.Copy(rndBRotated, 0, challengeData, RndA.Length, rndBRotated.Length);

        byte[] challenge = CryptoHelper.Encrypt(KeyType, Iv, Key, challengeData);
        Iv = challenge[^Iv.Length..];

        return challenge;
    }

    public override void ChallengeResume(byte[] encryptedResponse)
    {
        byte[] rotatedA = CryptoHelper.Decrypt(KeyType, Iv, Key, encryptedResponse);
        byte[] rotatedAOriginal = DesfireUtilities.RotateByte(RndA);

        if (!DesfireUtilities.IsEqual(rotatedA, rotatedAOriginal))
        {
            throw new AuthenticationException();
        }

        GenerateSessionKey();
    }
}

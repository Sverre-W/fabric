namespace Fabric.Hardware.Desfire.Protocol;

/// <summary>
///     The type of key used for encryption
/// </summary>
public enum KeyType
{
    /// <summary>
    ///     No key type
    /// </summary>
    None,

    /// <summary>
    ///     Triple DES Encryption (K1 = k2 = k3)
    /// </summary>
    TDes,

    /// <summary>
    ///     Triple DES Encryption with 2 Keys (K1 = K3)
    /// </summary>
    Tdes2K,

    /// <summary>
    ///     Triple DES Encryption (k1 != k2) (k1 != k3) (k2 != k3)
    /// </summary>
    Tdes3K,

    /// <summary>
    ///     AES Encryption
    /// </summary>
    Aes,
}

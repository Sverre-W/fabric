using Microsoft.AspNetCore.DataProtection;

namespace Fabric.Server.Desfire.Application;

public interface IDesfireKeyProtector
{
    string Protect(string plaintextHex);
    string Unprotect(string protectedValue);
}

public sealed class DesfireKeyProtector(IDataProtectionProvider dataProtectionProvider) : IDesfireKeyProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Fabric.Server.Desfire.KeyMaterial.v1");

    public string Protect(string plaintextHex) => _protector.Protect(plaintextHex.Trim());

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}

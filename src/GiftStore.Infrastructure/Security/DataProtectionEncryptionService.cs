namespace GiftStore.Infrastructure.Security;

using Microsoft.AspNetCore.DataProtection;
using GiftStore.Application.Interfaces;

public class DataProtectionEncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public DataProtectionEncryptionService(IDataProtectionProvider provider)
    {
        // Isolated cryptographic purpose boundary for digital gift-card codes
        _protector = provider.CreateProtector("GiftStore.GiftCardCodes.v1");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        return _protector.Unprotect(cipherText);
    }
}

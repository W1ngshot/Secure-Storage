using System.Security.Cryptography;
using SecureStorage.Domain.Security;

namespace SecureStorage.Infrastructure.Security;

public class AesEncryptionService : IEncryptionService
{
    public byte[] Encrypt(byte[] plainData, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);

        return aes.IV.Concat(cipher).ToArray();
    }

    public byte[] Decrypt(byte[] cipherData, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = cipherData.Take(16).ToArray();
        var actualCipher = cipherData.Skip(16).ToArray();

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(actualCipher, 0, actualCipher.Length);
    }
}
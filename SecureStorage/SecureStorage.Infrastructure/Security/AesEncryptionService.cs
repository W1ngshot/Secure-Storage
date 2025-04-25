using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

    public string Encrypt<T>(T plainText, byte[] key)
    {
        var encryptedBytes = Encrypt(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(plainText))
            , key);
        return Convert.ToBase64String(encryptedBytes);
    }

    public T Decrypt<T>(string encryptedBase64, byte[] key)
    {
        var decryptedBytes = Decrypt(Convert.FromBase64String(encryptedBase64), key);
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(decryptedBytes))!;
    }

    public string? TryDecrypt(byte[] cipherData, byte[] key)
    {
        try
        {
            return Encoding.UTF8.GetString(Decrypt(cipherData, key));
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    public T? TryDecrypt<T>(string cipherData, byte[] key)
    {
        try
        {
            return Decrypt<T>(cipherData, key);
        }
        catch (CryptographicException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
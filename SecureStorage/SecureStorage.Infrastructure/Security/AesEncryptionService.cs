using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SecureStorage.Core.Interfaces.Security;

namespace SecureStorage.Infrastructure.Security;

public class AesEncryptionService : IEncryptionService
{
    public byte[] Encrypt(byte[] plainData, byte[] key)
    {
        using var gcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plainData.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        gcm.Encrypt(nonce, plainData, ciphertext, tag);

        return nonce.Concat(ciphertext).Concat(tag).ToArray();
    }

    public byte[] Decrypt(byte[] cipherData, byte[] key)
    {
        var nonce = cipherData.Take(AesGcm.NonceByteSizes.MaxSize).ToArray();
        var ciphertext = cipherData
            .Skip(AesGcm.NonceByteSizes.MaxSize)
            .Take(cipherData.Length - AesGcm.NonceByteSizes.MaxSize - AesGcm.TagByteSizes.MaxSize).ToArray();
        var tag = cipherData.Skip(cipherData.Length - AesGcm.TagByteSizes.MaxSize).ToArray();

        using var gcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var plaintextBytes = new byte[ciphertext.Length];

        gcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);

        return plaintextBytes;
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
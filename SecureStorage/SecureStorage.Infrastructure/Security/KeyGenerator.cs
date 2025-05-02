using System.Security.Cryptography;
using System.Text;
using SecureStorage.Domain.Security;

namespace SecureStorage.Infrastructure.Security;

public class KeyGenerator : IKeyGenerator
{
    public string GenerateKeyFromUserId(byte[] masterKey, string userId)
    {
        using var hmac = new HMACSHA256(masterKey);
        return ToBase64(
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(userId)));
    }

    public string GenerateRandomKey(int keySize = 32)
    {
        var bytes = new byte[keySize];
        RandomNumberGenerator.Fill(bytes);
        return ToBase64(bytes);
    }

    public byte[] GenerateRandomByteKey(int keySize = 32)
    {
        var bytes = new byte[keySize];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    private static string ToBase64(byte[] key) => Convert.ToBase64String(key);
}